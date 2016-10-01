using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using MagneticDoorSensorShared;
using System.Web.Hosting;
using System.IO;
using Newtonsoft.Json;

namespace MagneticDoorSensorWebApi
{
    public class SensorHub : Hub
    {
        private static object _lockObject = new object();

        private string Path { get { return HostingEnvironment.MapPath("~/App_Data/sensordata.json"); } }

        public void DataChanged(SensorData data)
        {
            lock (_lockObject)
                File.WriteAllText(Path, JsonConvert.SerializeObject(data));
            Clients.All.dataChanged(data);
        }

        public override Task OnConnected()
        {
            lock (_lockObject)
            {
                if (!File.Exists(Path))
                    File.WriteAllText(Path, JsonConvert.SerializeObject(new SensorData()));
                Clients.Caller.dataChanged(JsonConvert.DeserializeObject<SensorData>(File.ReadAllText(Path)));
            }
            return base.OnConnected();
        }
    }
}