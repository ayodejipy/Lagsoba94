using Lagsoba94.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Lagsoba94.Models.ViewModels
{
    public class NewsVM
    {
        public NewsVM() { }

        public NewsVM(NewsDTO row)
        {
            NewsId = row.NewsId;
            Slug = row.Slug;
            Headline = row.Headline;
            Heading = row.Heading;
            NewsBody = row.NewsBody;
            NewsCaster = row.NewsCaster;
            AuthorizedBy = row.AuthorizedBy;
            UploadDate = row.UploadDate;
            TypeId = row.TypeId;
            Authorized = row.Authorized;
        }

        public int NewsId { get; set; }
        public string Slug { get; set; }
        [Required(ErrorMessage = "Please provide a News Headline")]
        [StringLength(255, ErrorMessage = "Minimum of 5 and maximum of 255 characters allowed.", MinimumLength = 5)]
        public string Headline { get; set; }
        [Required(ErrorMessage = "Please provide a News Heading")]
        [StringLength(500, ErrorMessage = "Minimum of 5 and maximum of 500 characters allowed.", MinimumLength = 5)]
        public string Heading { get; set; }
        [Required]
        [AllowHtml]
        [Display(Name = "News Body")]
        public string NewsBody { get; set; }
        [Display(Name = "News Caster")]
        public int NewsCaster { get; set; }
        [Display(Name = "Uthorized By")]
        public int AuthorizedBy { get; set; }
        [Display(Name = "Upload Date")]
        public DateTime UploadDate { get; set; }
        [Required(ErrorMessage = "Please select the news type from the dropdown")]
        public int TypeId { get; set; }
        public bool? Authorized { get; set; }
        [Display(Name = "News Type")]
        public string NewsType { get; set; }

        public string ImageSrc { get; set; }

        public SelectList NewsTypeList { get; set; }
    }

    public class SingleNewsVM
    {
        public NewsVM News { get; set; }

        public IEnumerable<RecentNews> RecentNews { get; set; }
    }

    public class RecentNews
    {
        public RecentNews() { }

        public int NewsId { get; set; }
        public string Slug { get; set; }
        public string Headline { get; set; }
        public string ImageSrc { get; set; }
        public DateTime Date { get; set; }
    }

    public class NewsType {
        public NewsType() { }

        public NewsType(NewsTypeDTO row) {
            TypeId = row.TypeId;
            Description = row.Description;
        }

        public int TypeId { get; set; }
        public string Description { get; set; }
    }

    public class CommentVM
    {
        public CommentVM() { }
        
        public CommentVM(CommentDTO row)
        {
            CommentId = row.CommentId;
            NewsId = row.NewsId;
            UserId = row.UserId;
            Comment = row.Comment;
            Date = row.Date;
        }

        public int CommentId { get; set; }
        public int NewsId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}