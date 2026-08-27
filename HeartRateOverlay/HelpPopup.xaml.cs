using HeartRateOverlay.ViewModels;
using CommunityToolkit.Maui.Views;

namespace HeartRateOverlay;

public partial class HelpPopup : Popup
{
	public HelpPopup(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private void OnCloseClicked(object sender, EventArgs e)
    {
		CloseAsync();
    }
}