namespace Entregas.MAUI.Views;
using Entregas.MAUI.Models;

[QueryProperty(nameof(EntregaFinal), "EntregaFirmada")]
public partial class ResumenEntregaPage : ContentPage
{
    private EntregaModel _entregaFinal;
    public EntregaModel EntregaFinal
    {
        get => _entregaFinal;
        set
        {
            _entregaFinal = value;
            BindingContext = _entregaFinal;
            CargarFirmaVisual(); // Convertimos el texto Base64 de nuevo a Imagen
        }
    }

    public ResumenEntregaPage()
    {
        InitializeComponent();

        // Ocultamos el botón nativo de "Atrás" para forzar que el flujo termine con el botón inferior
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
    }

    private void CargarFirmaVisual()
    {
        if (!string.IsNullOrEmpty(EntregaFinal?.FirmaBase64))
        {
            byte[] imageBytes = Convert.FromBase64String(EntregaFinal.FirmaBase64);
            FirmaImagen.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        // Regresa al usuario a la pantalla principal limpiando el historial de navegación
        await Shell.Current.GoToAsync("//DashboardPage");
    }
}