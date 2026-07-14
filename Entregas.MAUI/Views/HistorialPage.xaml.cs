using Microsoft.Maui.Controls;
using Entregas.MAUI.ViewModels;

namespace Entregas.MAUI.Views
{
    public partial class HistorialPage : ContentPage
    {
        private HistorialViewModel _vm => BindingContext as HistorialViewModel;

        public HistorialPage()
        {
            InitializeComponent();
        }

        // NUEVO: Se ejecuta cada vez que entras a la pestaña de Historial
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm != null)
            {
                await _vm.CargarHistorialAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // _vm?.Unsubscribe(); // Comentado para que el historial siga escuchando en segundo plano
        }
    }
}