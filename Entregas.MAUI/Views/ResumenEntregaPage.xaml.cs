using System;
using System.IO;
using Entregas.MAUI.Models;
using Entregas.MAUI.Services;
using Microsoft.Maui.Controls;

namespace Entregas.MAUI.Views;
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
        if (EntregaFinal == null)
        {
            await Shell.Current.GoToAsync("//DashboardPage");
            return;
        }

        try
        {
            // Marcar como concretada y persistir cambio
            EntregaFinal.Estado = 2; // 2 = completado
            await DatabaseService.GuardarEntregaAsync(EntregaFinal);

            // Notificar a viewmodels (Dashboard, Historial) para que actualicen listas en tiempo real
            MessagingCenter.Send(this, "EntregaConcretada", EntregaFinal);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo registrar la entrega: {ex.Message}", "OK");
            // Si falla la persistencia, no navegamos para que el usuario reintente
            return;
        }

        // Regresa al usuario a la pantalla principal limpiando el historial de navegación
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    // New: back arrow handler — always go back one step in the navigation stack
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            // Fallback: if the previous route is not available, go to root dashboard
            await Shell.Current.GoToAsync("//DashboardPage");
        }
    }
}