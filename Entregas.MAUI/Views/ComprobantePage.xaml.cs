namespace Entregas.MAUI.Views;
using Entregas.MAUI.Models;
using Entregas.MAUI.Services;

// Esto le dice a MAUI que reciba el parámetro "Entrega" que enviamos desde el Popup
[QueryProperty(nameof(EntregaActual), "Entrega")]
public partial class ComprobantePage : ContentPage
{
    private EntregaModel _entregaActual;
    public EntregaModel EntregaActual
    {
        get => _entregaActual;
        set { _entregaActual = value; OnPropertyChanged(); }
    }

    public ComprobantePage()
    {
        InitializeComponent();
    }

    private async void OnConfirmarEntregaClicked(object sender, EventArgs e)
    {
        // 1. Extraer los nombres de las cajas de texto
        EntregaActual.Repartidor = RepartidorEntry.Text ?? "No especificado";
        EntregaActual.Receptor = ReceptorEntry.Text ?? "No especificado";
        EntregaActual.Observaciones = ObservacionesEntry.Text ?? "";
        EntregaActual.Observaciones = string.IsNullOrWhiteSpace(ObservacionesEntry.Text) ? "Sin observaciones" : ObservacionesEntry.Text;

        // ¡NUEVO! Actualizamos el estado a 2 (Entregado). 
        // Gracias al paso anterior, esto pintará la tarjeta de verde en el Dashboard instantáneamente.
        EntregaActual.Estado = 2;
        await DatabaseService.GuardarEntregaAsync(EntregaActual);

        // 2. Extraer la firma
        var flujoImagen = await PadFirmaDigital.GetImageStream(300, 150);
        if (flujoImagen != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await flujoImagen.CopyToAsync(memoryStream);
                EntregaActual.FirmaBase64 = Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        await DatabaseService.GuardarEntregaAsync(EntregaActual);

        // 3. Enviar todos los datos recolectados al diseño final del comprobante
        var parametros = new Dictionary<string, object>
        {
            { "EntregaFirmada", EntregaActual }
        };
        await Shell.Current.GoToAsync("ResumenEntregaPage", parametros);
    }

    private void OnLimpiarFirmaClicked(object sender, EventArgs e)
    {
        PadFirmaDigital.Lines.Clear();
    }

    // Example snippet to call when an entrega se concretó (adaptar al flujo real de la página)
    public async Task MarcarComoConcretadaAsync(EntregaModel entrega, string receptor, string firmaBase64, string observaciones = "")
    {
        if (entrega == null) return;

        entrega.Estado = 2; // 2 = completado
        entrega.Receptor = receptor;
        entrega.FirmaBase64 = firmaBase64;
        entrega.Observaciones = observaciones;

        // Persistir cambios (usar el método de tu DB; se usa GuardarEntregaAsync si actualiza por Id)
        await DatabaseService.GuardarEntregaAsync(entrega);

        // Notificar a otras vistas / viewmodels que la entrega fue concretada
        MessagingCenter.Send(this, "EntregaConcretada", entrega);

        // Opcional: navegar fuera o mostrar confirmación
        await Shell.Current.DisplayAlert("Entrega", "La entrega fue registrada como completada.", "OK");
    }
}