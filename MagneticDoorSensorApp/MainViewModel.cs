using Microsoft.AspNet.SignalR.Client;
using System;
using System.ComponentModel;
using MagneticDoorSensorShared;
using Windows.UI.Core;
using System.Runtime.CompilerServices;
using FontAwesome.UWP;
using Windows.UI.Xaml;
using Windows.ApplicationModel;

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
            hubConnection.Error += ex => RunOnUiThread(() =>
            {
                LastError = $"Notification Hub Error: {ex.Message}";
                BusyVisibility = Visibility.Collapsed;
            });

            _hubProxy = hubConnection.CreateHubProxy("SensorHub");
            _hubProxy.On<SensorData>("dataChanged", (data) => RunOnUiThread(() =>
            {
                State = data.State;
                LastUpdated = data.LastUpdated.ToLocalTime();
                switch (data.State)
                {
                    case SensorState.Closed:
                        Icon = FontAwesomeIcon.Lock;
                        break;
                    case SensorState.Open:
                        Icon = FontAwesomeIcon.Unlock;
                        break;
                    default:
                        Icon = FontAwesomeIcon.QuestionCircle;
                        break;
                }
                BusyVisibility = Visibility.Collapsed;
            }));

            hubConnection.Start();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private SensorState _state = SensorState.Unknown;
        public SensorState State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        private FontAwesomeIcon _icon = FontAwesomeIcon.QuestionCircle;
        public FontAwesomeIcon Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
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

        private Visibility _busyVisibility = Visibility.Visible;
        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set
            {
                _busyVisibility = value;
                OnPropertyChanged();
            }
        }

        public string AppVersion
        {
            get { return $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}"; }
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
