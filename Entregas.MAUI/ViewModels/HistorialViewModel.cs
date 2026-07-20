using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Entregas.Shared;
using Entregas.MAUI.Services; // Agregado para acceder a la BD
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Entregas.MAUI.Utilities;

namespace Entregas.MAUI.ViewModels
{
    public class HistorialViewModel
    {
        public ObservableCollection<EntregaModel> Entregas { get; } = new ObservableCollection<EntregaModel>();

        public ICommand ViewComprobanteCommand { get; }
        public ICommand RefreshCommand { get; }

        public HistorialViewModel()
        {
            ViewComprobanteCommand = new Command<EntregaModel>(async (entrega) =>
            {
                if (entrega == null) return;
                try
                {
                    var parametros = new Dictionary<string, object> { { "EntregaFirmada", entrega } };
                    await Shell.Current.GoToAsync("ResumenEntregaPage", parametros);
                }
                catch { /* Silencioso en caso de error */ }
            });

            RefreshCommand = new Command(async () => await CargarHistorialAsync());

            try
            {
                WeakReferenceMessenger.Default.Register<HistorialViewModel, EntregaCreadaMessage>(this, async (r, msg) =>
                {
                    await CargarHistorialAsync();
                });

                WeakReferenceMessenger.Default.Register<HistorialViewModel, EntregaConcretadaMessage>(this, async (r, msg) =>
                {
                    await CargarHistorialAsync();
                });
            }
            catch { }
        }

        public async Task CargarHistorialAsync()
        {
            try
            {
                var entregasDb = await DatabaseService.ObtenerEntregasAsync();

                var completadas = entregasDb.Where(e => e.Estado == 2)
                                            .OrderByDescending(e => e.FechaEntrega)
                                            .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Entregas.Clear();
                    foreach (var entrega in completadas)
                    {
                        Entregas.Add(entrega);
                    }
                });
            }
            catch { }
        }

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