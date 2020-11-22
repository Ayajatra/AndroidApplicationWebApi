using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Http.Headers;
using AndroidApplicationWebApi.Models;
using System.Data.Entity;
using System.Web.Hosting;

namespace AndroidApplicationWebApi.Controllers
{
    [RoutePrefix("api")]
    public class ValuesController : ApiController
    {
        private string AppData { get; } = HostingEnvironment.MapPath("~/App_Data");

        [Route("user")]
        public IHttpActionResult PostUser(PostUser postUser)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var user = db.Users
                    .Include(x => x.Subject)
                    .Include(x => x.UserType)
                    .FirstOrDefault(x => x.Name == postUser.Name && x.Password == postUser.Password);

                if (user == null) return NotFound();

                return Ok(new
                { 
                    user.Id,
                    user.Name,
                    user.Password,
                    user.UserTypeId,
                    UserTypeName = user.UserType.Name,
                    user.SubjectId
                });
            }
        }

        [Route("competition")]
        public IHttpActionResult GetCompetitions(int userId)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var user = db.Users.Find(userId);
                if (user == null) return NotFound();

                var competitions = db.Competitions
                    .Where(x => user.SubjectId == null || x.SubjectId == user.SubjectId)
                    .Include(x => x.Subject)
                    .Select(x => new
                    { 
                        x.Id,
                        x.Name,
                        x.SubjectId,
                        SubjectName = x.Subject.Name,
                        x.DateTimeStart,
                        x.DateTimeEnd,
                        x.UploadFileName
                    })
                    .ToArray();

                return Ok(competitions);
            }
        }

        [Route("competition")]
        public IHttpActionResult PostCompetition(PostCompetition postCompetition)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var competition = new Competition
                {
                    Name = postCompetition.Name,
                    DateTimeStart = postCompetition.DateTimeStart,
                    DateTimeEnd = postCompetition.DateTimeEnd,
                    UploadFileName = $"{Guid.NewGuid()}.{postCompetition.UploadFileExtension}",
                    SubjectId = postCompetition.SubjectId
                };

                var bytes = Convert.FromBase64String(postCompetition.UploadFile);
                File.WriteAllBytes(Path.Combine(AppData, competition.UploadFileName), bytes);

                db.Competitions.Add(competition);
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("competition")]
        public IHttpActionResult PutCompetition(int id, PostCompetition postCompetition)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var competition = db.Competitions.Find(id);
                competition.Name = postCompetition.Name;
                competition.DateTimeStart = postCompetition.DateTimeStart;
                competition.DateTimeEnd = postCompetition.DateTimeEnd;
                competition.SubjectId = postCompetition.SubjectId;

                File.Delete(Path.Combine(AppData, competition.UploadFileName));
                competition.UploadFileName = $"{Guid.NewGuid()}.{postCompetition.UploadFileExtension}";
                var bytes = Convert.FromBase64String(postCompetition.UploadFile);
                File.WriteAllBytes(Path.Combine(AppData, competition.UploadFileName), bytes);

                db.Competitions.Attach(competition);
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("competition")]
        public IHttpActionResult DeleteCompetition(int id)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                db.Database.ExecuteSqlCommand($"delete from Competition where Id = {id}");
                return Ok();
            }
        }

        [Route("competition/download")]
        public IHttpActionResult GetCompetitionDownload(int competitionId)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var competition = db.Competitions.Find(competitionId);
                if (competition == null) return NotFound();

                var bytes = File.ReadAllBytes(Path.Combine(AppData, competition.UploadFileName));
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = competition.UploadFileName
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return ResponseMessage(response);
            }
        }

        [Route("competitorsubmission")]
        public IHttpActionResult GetCompetitorSubmissions(int competitionId)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var submissions = db.CompetitorSubmissions
                    .Where(x => x.CompetitionId == competitionId)
                    .Include(x => x.User)
                    .Select(x => new
                    {
                        x.Id,
                        x.CompetitionId,
                        x.CompetitorId,
                        CompetitorName = x.User.Name,
                        x.SubmitDateTime,
                        x.UploadFileName
                    })
                    .ToArray();

                return Ok(submissions);
            }
        }

        [Route("competitorsubmission")]
        public IHttpActionResult PostCompetitorSubmission(PostCompetitorSubmission postSubmission)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var submission = new CompetitorSubmission
                {
                    CompetitionId = postSubmission.CompetitionId,
                    CompetitorId = postSubmission.CompetitorId,
                    SubmitDateTime = DateTime.Now,
                    UploadFileName = $"{Guid.NewGuid()}.{postSubmission.UploadFileExtension}"
                };

                var bytes = Convert.FromBase64String(postSubmission.UploadFile);
                File.WriteAllBytes(Path.Combine(AppData, submission.UploadFileName), bytes);

                db.CompetitorSubmissions.Add(submission);
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("competitorsubmission/download")]
        public IHttpActionResult GetCompetitorSubmissionDownload(int competitorSubmissionId)
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var submission = db.CompetitorSubmissions.Find(competitorSubmissionId);
                if (submission == null) return NotFound();

                var bytes = File.ReadAllBytes(Path.Combine(AppData, submission.UploadFileName));
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = submission.UploadFileName
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return ResponseMessage(response);
            }
        }

        [Route("subject")]
        public IHttpActionResult GetSubjects()
        {
            using (var db = new AndroidCaseCompetitionEntities())
            {
                var subjects = db.Subjects.Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToArray();

                return Ok(subjects);
            }
        }
    }
}
