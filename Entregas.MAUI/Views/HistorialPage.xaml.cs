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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Evitamos fugas desuscribiendo MessagingCenter
            _vm?.Unsubscribe();
        }
    }
}