using ExcelDataReader;
using Lagsoba94.Areas.Vote.Models.Data;
using Lagsoba94.Areas.Vote.Models.ViewModel;
using Lagsoba94.Filters;
using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lagsoba94.Areas.Vote.Controllers
{
    [VoteUserAuthorize(Roles = "Electoral Admin, Electoral Supervisor")]
    public class VoterController : Controller
    {
        private DbContext db = new DbContext();
        private new VoteUserManager User = new VoteUserManager();

        private ResultVM Result { get; set; }

        // GET: Voter
        public async Task<ActionResult> Index()
        {
            IEnumerable<VoterVM> model = User.GetAllVoters();
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(VoterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Result = await User.CreateUserAsync(model);

            if (!Result.Success)
            {
                ModelState.AddModelError("duplicate", Result.ErrorMessage);
                return View(model);
            }

            TempData["SM"] = "Voter added successfully";
            return RedirectToAction("Create");
        }

        [HttpPost]
        public ActionResult UploadVoters(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            // get current time stamp
            DateTime dateNow = DateTime.Now;

            string[] fileExt = importFile.FileName.Split('.');

            //Extract Image File Name.
            string fileName = Convert.ToString(dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " " + dateNow.Hour + "-" + dateNow.Minute + "-" + dateNow.Second + "."+ fileExt.Last());

            //Set the Image File Path.
            string filePath = "~/Uploads/" + fileName;

            string serverpath = Server.MapPath("~/Uploads");
            string fullserverpath = serverpath + "\\" + fileName;

            //Save the Image File in Folder.
            importFile.SaveAs(Server.MapPath(filePath));

            string extension = Path.GetExtension(filePath).ToLower();

            string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

            if (validFileTypes.Contains(extension))
            {
                List<VoterVM> fileData = new List<VoterVM>();

                if (extension == ".csv")
                {
                    try
                    {
                        fileData = GetDataFromCSVFile(importFile.InputStream);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { Status = 0, Message = ex.Message });
                    }
                }
                else
                {
                    List<VoterVM> fromExcel = ReadExcel(fullserverpath);
                    fileData = fromExcel.ToList();
                }

                // add users
                Result = User.CreateMultipleUsers(fileData);

                if (Result.Success)
                {
                    string message;
                    if (Result.ErrorMessages != null && Result.ErrorMessages.Count() > 0)
                    {
                        message = "Voters added with some errors;";
                        foreach (var error in Result.ErrorMessages)
                        {
                            message += "\n" + error;
                        }
                    }
                    else
                    {
                        message = "Voters Added Successfully";
                    }

                    return Json(new { Status = 1, Message = message });
                }
                else
                {
                    string message = "Unable to add voters.";
                    foreach (var error in Result.ErrorMessages)
                    {
                        message += "\n" + error;
                    }

                    return Json(new { Status = 1, Message = message });
                }
            }
            else
            {
                return Json(new { Status = -1, Message = "Invalid file type selected." });
            }
        }

        public ActionResult DownloadTemplate()
        {
            string filePath = "~/Uploads/VoterTemplate.xlsx";
            string fullName = Server.MapPath(filePath);

            FileStream fs = System.IO.File.OpenRead(fullName);
            byte[] data = new byte[fs.Length];

            return File(fullName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VoterTemplate.xlsx");
        }

        public ActionResult Edit(string voterId)
        {
            VoteUserDTO user = db.VoteUsers.Where(x => x.VoterId == voterId).FirstOrDefault();
            if (user == null)
                return RedirectToAction("Index");

            VoterVM model = new VoterVM(user);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(VoterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.VoteUsers.Where(x => x.VoterId == model.VoterId).FirstOrDefault();
            if (user == null)
                return RedirectToAction("Index");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            db.SaveChanges();

            TempData["SM"] = "Changes Saved";

            return RedirectToAction("Edit", new { voterId = model.VoterId });
        }

        public ActionResult Delete(string voterId)
        {
            //Result = User.RemoveUser(voterId);

            if (!Result.Success)
            {
                TempData["ERR"] = Result.ErrorMessage;
                return RedirectToAction("Index");
            }

            TempData["SM"] = "Voter deleted successfully.";
            return RedirectToAction("Index");
        }

        #region Helpers
        private List<VoterVM> GetDataFromCSVFile(Stream stream)
        {
            var voterList = new List<VoterVM>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names  
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        var dataTable = dataSet.Tables[0];
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {
                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            voterList.Add(new VoterVM()
                            {
                                FirstName = objDataRow["First Name"].ToString(),
                                LastName = objDataRow["Last Name"].ToString(),
                                Email = objDataRow["Email"].ToString(),
                                Phone = objDataRow["Phone"].ToString(),
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return voterList;
        }

        private OleDbConnection OpenConnection(string path)
        {
            OleDbConnection oledbConn = null;
            try
            {
                if (Path.GetExtension(path) == ".xls")
                    oledbConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + path +
                                                            "; Extended Properties= \"Excel 8.0;HDR=Yes;IMEX=2\"");
                else if (Path.GetExtension(path) == ".xlsx")
                    oledbConn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" +
                                        path + "; Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';");

                oledbConn.Open();
            }
            catch (Exception)
            {
                //Error
            }
            return oledbConn;
        }

        private List<VoterVM> ExtractVoterExcel(OleDbConnection oledbConn)
        {
            OleDbCommand cmd = new OleDbCommand();
            OleDbDataAdapter oleda = new OleDbDataAdapter();
            DataSet dsEmployeeInfo = new DataSet();

            cmd.Connection = oledbConn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM [Voters$]"; //Excel Sheet Name ( Voters )
            oleda = new OleDbDataAdapter(cmd);
            oleda.Fill(dsEmployeeInfo, "Employee");

            var result = dsEmployeeInfo.Tables[0].AsEnumerable().Select(s => new VoterVM
            {
                FirstName = Convert.ToString(s["First Name"] != DBNull.Value ? s["First Name"] : ""),
                LastName = Convert.ToString(s["Last Name"] != DBNull.Value ? s["Last Name"] : ""),
                Email = Convert.ToString(s["Email"] != DBNull.Value ? s["Email"] : ""),
                Phone = Convert.ToString(s["Phone"] != DBNull.Value ? s["Phone"] : "")
            }).ToList();

            return result;
        }

        public List<VoterVM> ReadExcel(string path)
        {
            List<VoterVM> result = new List<VoterVM>();
            try
            {
                OleDbConnection oledbConn = OpenConnection(path);
                if (oledbConn.State == ConnectionState.Open)
                {
                    result = ExtractVoterExcel(oledbConn);
                    oledbConn.Close();
                }
            }
            catch (Exception ex)
            {
                // Error
            }
            return result;
        }

        public string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (appUrl != "/")
                appUrl = "/" + appUrl;

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }
        #endregion
    }
}