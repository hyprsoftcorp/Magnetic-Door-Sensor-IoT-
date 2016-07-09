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

            _hubProxy = hubConnection.CreateHubProxy("SensorHub");

            _hubProxy.On<SensorData>("dataChanged", async (data) => await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

        #endregion

        #region Methods

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
