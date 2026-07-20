using CommunityToolkit.Maui.Views;
using Entregas.Shared;

namespace Entregas.MAUI.Views;

public partial class DetallePedidoPopup : Popup
{
    private EntregaModel _entregaSeleccionada;

    public DetallePedidoPopup(EntregaModel entrega)
    {
        InitializeComponent();
        _entregaSeleccionada = entrega;

        // Enlazamos los datos del pedido a la interfaz visual del Popup
        BindingContext = _entregaSeleccionada;
    }

    private async void OnCheckClicked(object sender, EventArgs e)
    {
        Close();

        // Empaquetamos los datos del pedido para enviarlos a la siguiente pantalla
        var parametros = new Dictionary<string, object>
        {
            { "Entrega", _entregaSeleccionada }
        };

        await Shell.Current.GoToAsync("ComprobantePage", parametros);
    }

    private void OnCerrarClicked(object sender, EventArgs e)
    {
        Close();
    }
}