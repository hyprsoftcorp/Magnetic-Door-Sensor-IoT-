using Microsoft.AspNet.SignalR.Client;
using System;
using System.ComponentModel;
using MagneticDoorSensorShared;
using Windows.UI.Core;
using System.Runtime.CompilerServices;

namespace MagneticDoorSensorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private IHubProxy _hubProxy;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            var hubConnection = new HubConnection(Constants.SignalREndPoint);
            hubConnection.Error += ex => RunOnUiThread(() => LastError = ex.Message);

            _hubProxy = hubConnection.CreateHubProxy("SensorHub");

            _hubProxy.On<SensorData>("dataChanged", (data) => RunOnUiThread(() =>
            {
                State = data.State;
                LastUpdated = data.LastUpdated.ToLocalTime();
            }));

            hubConnection.Start();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private SensorState _state;
        public SensorState State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        private string _lastError;
        public string LastError
        {
            get { return _lastError; }
            set
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private async void RunOnUiThread(Action action)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
