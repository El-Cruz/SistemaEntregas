using System.Collections.ObjectModel;
using System.Windows.Input; // Requerido para ICommand
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Requerido para mapas
using Microsoft.Maui.Devices.Sensors; // Requerido para ubicación
using Entregas.MAUI.Models;
using CommunityToolkit.Maui.Views;
using Entregas.MAUI.Views;
using Microsoft.Maui.Controls; // Shell
using Microsoft.Maui.Dispatching;
using Entregas.MAUI.Services;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Entregas.MAUI.Utilities;

namespace Entregas.MAUI.ViewModels
{
    public class DashboardViewModel
    {
        public ObservableCollection<EntregaModel> Entregas { get; set; }

        public ICommand NavegarAMapaCommand { get; }
        public ICommand? AbrirDetalleCommand { get; private set; }

        public DashboardViewModel()
        {
            Entregas = new ObservableCollection<EntregaModel>();

            NavegarAMapaCommand = new Command<EntregaModel>(async (entrega) => await IniciarNavegacionSatelital(entrega));
            AbrirDetalleCommand = new Command<EntregaModel>(MostrarPopupDetalle);

            // Subscribe using CommunityToolkit WeakReferenceMessenger
            try
            {
                WeakReferenceMessenger.Default.Register<DashboardViewModel, EntregaCreadaMessage>(this, (r, msg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var entrega = msg.Value;
                        if (entrega != null && entrega.Estado != 2)
                            Entregas.Insert(0, entrega);
                    });
                });

                WeakReferenceMessenger.Default.Register<DashboardViewModel, EntregaConcretadaMessage>(this, (r, msg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var entrega = msg.Value;
                        if (entrega == null) return;

                        var existente = Entregas.FirstOrDefault(e => e.Id == entrega.Id);
                        if (existente != null)
                        {
                            // actualizar propiedades para que la UI reaccione
                            existente.Estado = entrega.Estado;
                            existente.Receptor = entrega.Receptor;
                            existente.FirmaBase64 = entrega.FirmaBase64;
                            existente.Observaciones = entrega.Observaciones;
                            existente.Repartidor = entrega.Repartidor;

                            // eliminar si ya fue concretada
                            if (entrega.Estado == 2)
                                Entregas.Remove(existente);
                        }
                        else
                        {
                            if (entrega.Estado != 2)
                                Entregas.Insert(0, entrega);
                        }
                    });
                });
            }
            catch
            {
                // No bloquear si Messenger no está disponible
            }
        }

        public async Task RefrescarDashboard()
        {
            var listaDb = await DatabaseService.ObtenerEntregasAsync();
            Entregas.Clear();
            foreach (var item in listaDb.OrderByDescending(e => e.FechaEntrega))
            {
                // cargar solo las no completadas en el dashboard activo (Estado != 2)
                if (item.Estado != 2)
                    Entregas.Add(item);
            }
        }

        private async Task IniciarNavegacionSatelital(EntregaModel entrega)
        {
            if (entrega == null) return;

            if (entrega.Latitud == 0 && entrega.Longitud == 0)
            {
                await Shell.Current.DisplayAlert("Ubicación Inválida", "Esta entrega no tiene coordenadas registradas. Asegúrate de buscar el local con el botón 'Buscar' al crear el pedido.", "OK");
                return;
            }

            Location ubicacionDestino = new Location(entrega.Latitud, entrega.Longitud);

            MapLaunchOptions opciones = new MapLaunchOptions
            {
                Name = entrega.Destinatario,
                NavigationMode = NavigationMode.Driving
            };

            try
            {
                await Map.Default.OpenAsync(ubicacionDestino, opciones);
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo abrir la aplicación de mapas.", "OK");
            }
        }

        private void MostrarPopupDetalle(EntregaModel entrega)
        {
            if (entrega == null) return;

            try
            {
                var popup = new DetallePedidoPopup(entrega);
                Shell.Current.CurrentPage.ShowPopup(popup);
            }
            catch (Exception ex)
            {
                Shell.Current.DisplayAlert("Error de Popup", $"Motivo: {ex.Message}", "OK");
            }
        }

        // Unregister messenger handlers to avoid leaks
        public void Unsubscribe()
        {
            try
            {
                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
            catch { }
        }
    }
}
