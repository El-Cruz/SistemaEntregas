using System.Collections.ObjectModel;
using System.Windows.Input; // Requerido para ICommand
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Requerido para mapas
using Microsoft.Maui.Devices.Sensors; // Requerido para ubicación
using Entregas.MAUI.Models;
using CommunityToolkit.Maui.Views;
using Entregas.MAUI.Views;
using Microsoft.Maui.Controls; // MessagingCenter
using Microsoft.Maui.Dispatching;
using Entregas.MAUI.Services;
using System.Linq;

namespace Entregas.MAUI.ViewModels
{
    public class DashboardViewModel
    {
        public ObservableCollection<EntregaModel> Entregas { get; set; }

        // 1. Declaramos el Comando
        public ICommand NavegarAMapaCommand { get; }
        public ICommand? AbrirDetalleCommand { get; private set; }

        public DashboardViewModel()
        {
            Entregas = new ObservableCollection<EntregaModel>();
            CargarDatosDePrueba();

            // 2. Inicializamos los Comandos y le decimos qué método ejecutar
            NavegarAMapaCommand = new Command<EntregaModel>(async (entrega) => await IniciarNavegacionSatelital(entrega));
            AbrirDetalleCommand = new Command<EntregaModel>(MostrarPopupDetalle);

            // Subscribirse a mensajes de creación de entrega para actualizar el dashboard automáticamente
            try
            {
                MessagingCenter.Subscribe<CrearEntregaViewModel, EntregaModel>(this, "EntregaCreada", (sender, entrega) =>
                {
                    // Aseguramos ejecución en hilo UI
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Insertamos arriba para visualizar primero las recientes
                        Entregas.Insert(0, entrega);
                    });
                });

                // Subscribirse a eventos de concreción/actualización de entrega
                MessagingCenter.Subscribe<object, EntregaModel>(this, "EntregaConcretada", (sender, entrega) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (entrega == null) return;

                        // Buscar por Id la entrega en la colección actual
                        var existente = Entregas.FirstOrDefault(e => e.Id == entrega.Id);
                        if (existente != null)
                        {
                            // Actualizar propiedades relevantes para que la UI reaccione
                            existente.Estado = entrega.Estado;
                            existente.Receptor = entrega.Receptor;
                            existente.FirmaBase64 = entrega.FirmaBase64;
                            existente.Observaciones = entrega.Observaciones;
                            existente.Repartidor = entrega.Repartidor;

                            // Si la entrega ya fue concretada (estado 2), removerla del dashboard activo
                            if (entrega.Estado == 2)
                            {
                                Entregas.Remove(existente);
                            }
                        }
                        else
                        {
                            // Si por algún motivo la entrega no existía y no está concretada, añadirla
                            if (entrega.Estado != 2)
                                Entregas.Insert(0, entrega);
                        }
                    });
                });
            }
            catch
            {
                // No bloquear si MessagingCenter no está disponible en algún entorno
            }
        }

        public async Task RefrescarDashboard()
        {
            var listaDb = await DatabaseService.ObtenerEntregasAsync();
            Entregas.Clear();
            foreach (var item in listaDb.OrderByDescending(e => e.FechaEntrega)) // Ordenadas por la más reciente
            {
                Entregas.Add(item);
            }
        }

        // 3. El método de la Parte A, ahora adaptado para recibir el modelo de la entrega
        private async Task IniciarNavegacionSatelital(EntregaModel entrega)
        {
            if (entrega == null) return;

            // Si la latitud y longitud son 0, la API de Google no capturó la ubicación al crearla
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

        private void CargarDatosDePrueba()
        {
            Entregas.Clear();

            Entregas.Add(new EntregaModel
            {
                Id = 1,
                CodigoEntrega = "ENT-1042",
                Destinatario = "Veterinaria Patitas",
                Direccion = "Av. República y Eloy Alfaro",
                Estado = 0,
                Latitud = -0.180653,
                Longitud = -78.467838,
                Productos = new List<ProductoEntrega>
        {
            new ProductoEntrega { Nombre = "Chicken & Zucchini", Cantidad = 12 },
            new ProductoEntrega { Nombre = "Beef & Beets", Cantidad = 3 }
        }
            });

            Entregas.Add(new EntregaModel
            {
                Id = 2,
                CodigoEntrega = "ENT-1043",
                Destinatario = "Doggy Care Spa",
                Direccion = "Av. de los Shyris y Naciones Unidas",
                Estado = 1,
                Latitud = -0.1756,
                Longitud = -78.4799,
                Productos = new List<ProductoEntrega>
        {
            new ProductoEntrega { Nombre = "Renal Forte", Cantidad = 8 }
        }
            });
        }

        // Optional: permitir que la página llame a esto al salir para evitar memory leaks
        public void Unsubscribe()
        {
            try
            {
                MessagingCenter.Unsubscribe<CrearEntregaViewModel, EntregaModel>(this, "EntregaCreada");
                MessagingCenter.Unsubscribe<object, EntregaModel>(this, "EntregaConcretada");
            }
            catch { }
        }
    }
}
