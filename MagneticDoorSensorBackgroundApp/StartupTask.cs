using Microsoft.AspNet.SignalR.Client;
using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using MagneticDoorSensorShared;

namespace MagneticDoorSensorBackgroundApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private GpioPin _sensorPin;
        private IHubProxy _hubProxy;

        public void Run(IBackgroundTaskInstance taskInstance)
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
                hubConnection.Start();

                _sensorPin.ValueChanged += (s, a) =>
                {
                    try
                    {
                        _hubProxy.Invoke("DataChanged", new SensorData
                        {
                            State = _sensorPin.Read() == GpioPinValue.High ? SensorState.Open : SensorState.Closed
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
