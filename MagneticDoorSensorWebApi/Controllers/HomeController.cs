using System.Web.Mvc;

namespace MagneticDoorSensorWebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home";

            return View();
        }
    }
}
