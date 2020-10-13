using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using Lagsoba94.Models.ViewModel;

namespace Lagsoba94.Helpers
{
    public static class MailClient
    {
        private static readonly SmtpClient Client;

        static MailClient()
        {
            Client = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["SmtpServer"],
                Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            Client.UseDefaultCredentials = false;
            Client.Credentials = new NetworkCredential(
                ConfigurationManager.AppSettings["SmtpUser"],
                ConfigurationManager.AppSettings["SmtpPass"]);
        }

        private static bool SendMessage(string from, string to, string subject, string body)
        {
            MailMessage mm = null;
            bool isSent = false;
            try
            {
                // Create our message
                mm = new MailMessage(from, to, subject, body);
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mm.IsBodyHtml = true;
                mm.BodyEncoding = UTF8Encoding.UTF8;

                // Send it
                Client.Send(mm);
                isSent = true;
            }
            // Catch any errors, these should be logged and
            catch (Exception ex)
            {
                // If you wish to log email errors,
                // add it here...
                var exMsg = ex.Message;
            }
            finally
            {
                mm.Dispose();
            }
            return isSent;
        }

        public static bool SendEmail(EmailMessageVM model)
        {
            return SendMessage(ConfigurationManager.AppSettings["adminEmail"], model.ToAddress, model.Subject, model.Message);
        }
    }
}