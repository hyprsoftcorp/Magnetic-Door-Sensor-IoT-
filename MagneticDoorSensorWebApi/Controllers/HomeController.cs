using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace MagneticDoorSensorWebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home";

            return View();
        }

        public ActionResult Events()
        {
            var storageAccount = CloudStorageAccount.Parse(Constants.TableStorageConnectionString);
            var client = storageAccount.CreateCloudTableClient();
            var table = client.GetTableReference("MagDoorSensorHistory");
            var query = new TableQuery().Take(10);
            query.SelectColumns = new string[] { "LastUpdated", "State" };
            var results = table.ExecuteQuery(query).OrderByDescending(sensorData => sensorData.Timestamp).Select(data => new { LastUpdated = data["LastUpdated"].DateTime, State = data["State"].Int64Value });
            return Content(JsonConvert.SerializeObject(results));
        }
    }
}
