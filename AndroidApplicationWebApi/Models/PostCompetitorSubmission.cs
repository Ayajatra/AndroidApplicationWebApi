using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndroidApplicationWebApi.Models
{
    public class PostCompetitorSubmission
    {
        public int CompetitionId { get; set; }
        public int CompetitorId { get; set; }
        public string UploadFileExtension { get; set; }
        public string UploadFile { get; set; }
    }
}