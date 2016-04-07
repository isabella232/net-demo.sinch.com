using System.Web.Mvc;

namespace demo.sinch.com.Controllers {
    public class HomeController : Controller {
        private readonly string appKey;
        private readonly string appSecret;

        public ActionResult Index() {
            //using (var db = new ConferenceContext())
            //{
            //    var model = db.Conferences.ToList();
            //    return View(model);
            //}
            return View();
        }
        [Route("autotrader")]
        public ActionResult AutoTrader() {
            return View();
        }
    }
}