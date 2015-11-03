using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using demo.sinch.com.Models;
using Sinch.ServerSdk;
using Sinch.ServerSdk.ApiFilters;
using Sinch.ServerSdk.Calling;
using Sinch.WebApiClient;

namespace demo.sinch.com.Controllers {
    public class ConferenceController : Controller {
        private readonly string appKey;
        private readonly string appSecret;

        public ConferenceController() {
            appKey = ConfigurationManager.AppSettings["applicationKey"];
            appSecret = ConfigurationManager.AppSettings["applicationSecret"];
        }

        public ConferenceController(string applicationKey, string applicationSecret) {
            appKey = applicationKey;
            appSecret = applicationSecret;
        }

        [Route("~/Conference")]
        public ActionResult JoinConference(string id) {
            ViewBag.applicationKey = appKey;
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.id = id;
            }
            return View();
        }

        [Authorize]
        [Route("~/Conference/Create")]
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Create() {
            var model = new CreateConferenceModel();
            model.Conference = new Conference();
            model.Conference.ConferenceEndDate = DateTime.Today.AddDays(5);
            model.Conference.OwnerId = User.Identity.Name;
            var code = "";
            using (var db = new ConferenceContext())
            {
                var rng = new Random();
                var value = rng.Next(100, 9999); //1
                code = value.ToString("0000");
                while (
                    db.Conferences.Any(
                        m => m.PinCode == code && (m.ConferenceEndDate <= DateTime.Today || m.ValidForever)))
                {
                    value = rng.Next(100, 9999); //1
                    code = value.ToString("0000");
                }
            }
            model.Conference.PinCode = code;
            return View(model);
        }

        [Authorize]
        [Route("~/Conference/Create")]
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> Create(CreateConferenceModel model) {
            using (var db = new ConferenceContext())
            {
                // lets add a new guid to the model to ensure that all conferences are uniq
                model.Conference.ConferenceId = Guid.NewGuid();
                var utcdate = model.Conference.ConferenceEndDate.ToUniversalTime();
                model.Conference.ConferenceEndDate = utcdate.Date;
                model.Conference.OwnerId = User.Identity.Name;
                db.Conferences.Add(model.Conference);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("MyConferences");
        }

        [Authorize]
        [Route("~/Conference/My")]
        public ActionResult MyConferences() {
            using (var db = new ConferenceContext())
            {
                var conferences = db.Conferences.Where(m => m.OwnerId == User.Identity.Name).ToList();
                return View(conferences);
            }
        }

        [Authorize]
        [Route("~/Conference/{id}")]
        public async Task<ViewResult> Details(Guid id) {
            var model = new ConferenceDetailsViewModel();
            using (var db = new ConferenceContext())
            {
                var conference =
                    db.Conferences.FirstOrDefault(m => m.OwnerId == User.Identity.Name && m.ConferenceId == id);
                model.Conference = conference;
                try
                {
                    var conf = await Getconference(conference.ConferenceId.ToString()).Get();
                    // store the participants in the result model

                    if (conf != null)
                    {
                        model.Participants = conf.Participants;
                    }
                    else
                    {
                        model.Participants = new IParticipant[0];
                    }
                }
                catch (Exception)
                {}

                return View(model);
            }
        }

        [Route("~/Conference/delete/{id}")]
        public async Task<RedirectToRouteResult> Delete(Guid id) {
            using (var db = new ConferenceContext())
            {
                var conference =
                    db.Conferences.FirstOrDefault(m => m.OwnerId == User.Identity.Name && m.ConferenceId == id);
                db.Conferences.Remove(conference);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("MyConferences");
        }

        [Route("~/Conference/Callout")]
        public async Task<JsonResult> CallOut(string number, string conferenceId) {
            try
            {
                var factory = new WebApiClientFactory().CreateClient<ICalloutApiEndpoints>("https://api.sinch.com",
                    new ApplicationSigningFilter(appKey, Convert.FromBase64String(appSecret)), new RestReplyFilter());
                number = number.StartsWith("+") ? number.Trim() : "+" + number.Trim();
                await factory.AddParticipant(new CalloutRequest
                {
                    method = "conferenceCallout",
                    conferenceCallout = new ConferenceCallout
                    {
                        cli = "+17864088194",
                        destination = new Destination {endpoint = number, type = "number"},
                        domain = "pstn",
                        conferenceId = conferenceId,
                        enableDice = true
                    }
                });
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private IConference Getconference(string conferenceId) {
            // 1. Create an api factory
            var sinch = SinchFactory.CreateApiFactory(appKey, appSecret);
            // 2. Get a ConferenceApi client
            var conferenceClient = sinch.CreateConferenceApi();
            //fetch the conference 
            try
            {
                return conferenceClient.Conference(conferenceId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Route("~/GetTicket")]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetTicket(string id) {
            var loginObject = new LoginObject(appKey, appSecret);
            loginObject.userTicket = loginObject.Signature(id);

            return Json(loginObject, JsonRequestBehavior.AllowGet);
        }
    }
}