using CommunityToolkit.Maui.Extensions;
using HeartRateOverlay.ViewModels;

namespace HeartRateOverlay
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            var popup = new SettingsPopup((MainViewModel)BindingContext);
            this.ShowPopup(popup);
        }

        private void OnHelpClicked(object sender, EventArgs e)
        {
            var popup = new HelpPopup((MainViewModel)BindingContext);
            this.ShowPopup(popup);
        }
    }
}
