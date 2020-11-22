using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndroidApplicationWebApi.Models
{
    public class PostCompetition
    {
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string UploadFileExtension { get; set; }
        public string UploadFile { get; set; }
    }
}