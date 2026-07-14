using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Entregas.MAUI.Models;
using Entregas.MAUI.Services; // Agregado para acceder a la BD
using System.Linq;
using System.Threading.Tasks;

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

            // Mantenemos las suscripciones en vivo por si la app está abierta
            try
            {
                MessagingCenter.Subscribe<CrearEntregaViewModel, EntregaModel>(this, "EntregaCreada", async (sender, entrega) =>
                {
                    await CargarHistorialAsync();
                });

                MessagingCenter.Subscribe<object, EntregaModel>(this, "EntregaConcretada", async (sender, entrega) =>
                {
                    await CargarHistorialAsync();
                });
            }
            catch { }
        }

        // NUEVO: Método que consulta la BD y actualiza la lista
        public async Task CargarHistorialAsync()
        {
            try
            {
                var entregasDb = await DatabaseService.ObtenerEntregasAsync();

                // Filtramos solo las completadas (Estado == 2) y ordenamos por fecha (más recientes primero)
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
            catch { /* Ignorar si falla la carga inicial */ }
        }

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