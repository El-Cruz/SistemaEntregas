namespace Entregas.MAUI.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cada vez que entras a esta pantalla, recarga la base de datos
        if (BindingContext is ViewModels.DashboardViewModel vm)
        {
            await vm.RefrescarDashboard();
        }
    }
}