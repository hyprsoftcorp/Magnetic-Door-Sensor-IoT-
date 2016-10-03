using Microsoft.AspNet.SignalR.Client;
using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using MagneticDoorSensorShared;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json;

namespace MagneticDoorSensorBackgroundApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const string DefaultDateTimeFormat = "G";
        private const string DefaultLogFilename = "error.log";

        private BackgroundTaskDeferral _deferral;
        private GpioPin _sensorPin;
        private IHubProxy _hubProxy;
        private DeviceClient _deviceClient;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (sender, reason) => _deferral?.Complete();

            try
            {
                if (_deviceClient == null)
                    _deviceClient = DeviceClient.CreateFromConnectionString(Constants.DeviceConnectionString);

                var controller = GpioController.GetDefault();
                _sensorPin = controller.OpenPin(18);
                _sensorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                var hubConnection = new HubConnection(MagneticDoorSensorShared.Constants.SignalREndPoint);
                _hubProxy = hubConnection.CreateHubProxy("SensorHub");
                await hubConnection.Start();

                _sensorPin.ValueChanged += async (s, a) =>
                {
                    try
                    {
                        var data = new SensorData
                        {
                            State = _sensorPin.Read() == GpioPinValue.High ? SensorState.Open : SensorState.Closed
                        };
                        await _hubProxy.Invoke("DataChanged", data);
                        await _deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))));
                    }
                    catch (Exception ex)
                    {
                        await LogAsync(ex.ToString());
                    }
                };
            }
            catch (Exception ex)
            {
                await LogAsync(ex.ToString());
            }
        }

        private async Task LogAsync(string message)
        {
            var logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(DefaultLogFilename, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(logFile, $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message} {Environment.NewLine}");
        }
    }
}
