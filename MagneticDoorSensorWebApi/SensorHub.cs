using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using MagneticDoorSensorShared;

namespace MagneticDoorSensorWebApi
{
    public class SensorHub : Hub
    {
        // We only have a single door sensor so this static property will be shared by all clients.
        public static SensorData Data { get; private set; } = new SensorData();

        public void DataChanged(SensorData data)
        {
            Data = data;
            Clients.All.dataChanged(data);
        }

        public override Task OnConnected()
        {
            Clients.Caller.dataChanged(Data);
            return base.OnConnected();
        }
    }
}