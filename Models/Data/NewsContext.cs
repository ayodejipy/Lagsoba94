using Lagsoba94.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Lagsoba94.Models.Data
{
    public static class NewsContext
    {
        public static IEnumerable<NewsVM> GetAllNews()
        {
            using (DbContext db = new DbContext())
            {
                var result = db.News.ToArray().OrderByDescending(x => x.UploadDate).Select(x => new NewsVM(x)).Where(x => x.Authorized == true).ToList();
                foreach (var news in result)
                {
                    if (news.NewsBody.Contains("<img"))
                    {
                        int index = news.NewsBody.IndexOf("<img");
                        int startIndex = news.NewsBody.IndexOf("src=", index);
                        int endIndex = news.NewsBody.IndexOf("\"", startIndex + 6);
                        news.ImageSrc = news.NewsBody.Substring(startIndex + 6, (endIndex - startIndex) - 6);
                    }
                }
                return result;
            }
        }

        public static IEnumerable<NewsVM> GetNews(string slug)
        {
            using (DbContext db = new DbContext())
            {
                var result = db.News.Select(x => new NewsVM(x)).Where(x => x.Slug == slug && x.Authorized == true).ToList();
                return result;
            }
        }

        public static IEnumerable<NewsType> GetAllNewsType()
        {
            using (DbContext db = new DbContext())
            {
                var result = db.NewsType.ToArray().Select(x => new NewsType(x)).ToList();
                return result;
            }
        }
    }
}