using Windows.UI.Xaml.Controls;

namespace MagneticDoorSensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        MainViewModel _viewModel;
        public MainViewModel ViewModel { get { return _viewModel ?? (_viewModel = new MainViewModel()); } }
    }
}
