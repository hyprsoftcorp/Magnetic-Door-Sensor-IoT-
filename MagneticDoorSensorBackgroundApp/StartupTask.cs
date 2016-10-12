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
        #region Enumerations

        private enum LogLevel
        {
            ErrorsOnly,
            Verbose
        }

        private enum LogEntryType
        {
            Info,
            Warning,
            Error
        }

        #endregion

        #region Fields

        private const string DefaultLogFilename = "activity.log";

        private bool _isCloudServicesInitRequired = true;
        private bool _isCloudServicesInitializing;
        private BackgroundTaskDeferral _deferral;
        private GpioPin _sensorPin;
        private IHubProxy _hubProxy;
        private DeviceClient _iotHubClient;
#if DEBUG
        private LogLevel _logLevel = LogLevel.Verbose;
#else
        private LogLevel _logLevel = LogLevel.ErrorsOnly;
#endif

        #endregion

        #region Methods

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += async (sender, reason) =>
            {
                await LogAsync(LogEntryType.Warning, $"TaskCanceled - Reason: {reason}.");
                _sensorPin.ValueChanged -= SensorPinValueChanged;
                _iotHubClient?.Dispose();
                _deferral?.Complete();
            };

            _sensorPin = GpioController.GetDefault().OpenPin(18);
            _sensorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            _sensorPin.ValueChanged += SensorPinValueChanged;
        }

        private async void SensorPinValueChanged(GpioPin pin, GpioPinValueChangedEventArgs args)
        {
            try
            {
                await LogAsync(LogEntryType.Info, nameof(SensorPinValueChanged));

                // If we are already initializing our cloud services ignore this sensor pin value change.
                if (_isCloudServicesInitializing)
                    return;

                // Do we need to attempt to reconnect to our cloud services because of a prior failure?
                if (_isCloudServicesInitRequired)
                {
                    await InitCloudServicesAsync();
                    _isCloudServicesInitRequired = false;
                }   // cloud service needs initialization?

                var data = new SensorData
                {
                    State = _sensorPin.Read() == GpioPinValue.High ? SensorState.Open : SensorState.Closed
                };
                await _hubProxy.Invoke("DataChanged", data);
                await _iotHubClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))));
            }
            catch (Exception ex)
            {
                await LogAsync(LogEntryType.Error, nameof(SensorPinValueChanged) + ": " + ex.ToString());
                _isCloudServicesInitRequired = true;
            }
        }

        private async Task InitCloudServicesAsync()
        {
            try
            {
                if (_isCloudServicesInitializing)
                {
                    await LogAsync(LogEntryType.Warning, "Cloud services initialization in progress.");
                    return;
                }

                _isCloudServicesInitializing = true;

                await LogAsync(LogEntryType.Info, nameof(InitCloudServicesAsync));

                _iotHubClient?.Dispose();
                _iotHubClient = DeviceClient.CreateFromConnectionString(Constants.DeviceConnectionString);

                var hubConnection = new HubConnection(MagneticDoorSensorShared.Constants.SignalREndPoint);
                _hubProxy = hubConnection.CreateHubProxy("SensorHub");
                await hubConnection.Start();
            }
            catch (Exception ex)
            {
                await LogAsync(LogEntryType.Error, nameof(InitCloudServicesAsync) + ": " + ex.ToString());
            }
            finally
            {
                _isCloudServicesInitializing = false;
            }
        }

        private async Task LogAsync(LogEntryType type, string message)
        {
            if ((_logLevel == LogLevel.ErrorsOnly && type == LogEntryType.Error) || _logLevel == LogLevel.Verbose)
            {
                var logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(DefaultLogFilename, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(logFile, $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {type} {message} {Environment.NewLine}");
            }
        }

        #endregion
    }
}
