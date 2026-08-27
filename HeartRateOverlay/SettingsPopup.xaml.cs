using CommunityToolkit.Maui.Views;
using HeartRateOverlay.ViewModels;

namespace HeartRateOverlay;

public partial class SettingsPopup : Popup
{
	public SettingsPopup(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseAsync();
    }
}