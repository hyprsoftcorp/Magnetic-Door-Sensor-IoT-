using Microsoft.AspNet.SignalR.Client;
using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using MagneticDoorSensorShared;
using Windows.Storage;
using System.Threading.Tasks;

namespace MagneticDoorSensorBackgroundApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const string DefaultDateTimeFormat = "G";
        private const string DefaultLogFilename = "error.log";

        private BackgroundTaskDeferral _deferral;
        private GpioPin _sensorPin;
        private IHubProxy _hubProxy;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (sender, reason) => _deferral?.Complete();

            try
            {
                var controller = GpioController.GetDefault();
                _sensorPin = controller.OpenPin(18);
                _sensorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                var hubConnection = new HubConnection(Constants.SignalREndPoint);
                _hubProxy = hubConnection.CreateHubProxy("SensorHub");
                await hubConnection.Start();

                _sensorPin.ValueChanged += async (s, a) =>
                {
                    try
                    {
                        await _hubProxy.Invoke("DataChanged", new SensorData
                        {
                            State = _sensorPin.Read() == GpioPinValue.High ? SensorState.Open : SensorState.Closed
                        });
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
