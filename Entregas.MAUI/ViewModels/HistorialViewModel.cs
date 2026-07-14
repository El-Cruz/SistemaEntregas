using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Entregas.MAUI.Models;
using System.Linq;

namespace Entregas.MAUI.ViewModels
{
    public class HistorialViewModel
    {
        public ObservableCollection<EntregaModel> Entregas { get; } = new ObservableCollection<EntregaModel>();

        public ICommand ViewComprobanteCommand { get; }
        public ICommand RefreshCommand { get; }

        public HistorialViewModel()
        {
            // Command to open comprobante (ComprobantePage expects query param "Entrega")
            ViewComprobanteCommand = new Command<EntregaModel>(async (entrega) =>
            {
                if (entrega == null) return;
                try
                {
                    var parametros = new Dictionary<string, object> { { "Entrega", entrega } };
                    await Shell.Current.GoToAsync("ComprobantePage", parametros);
                }
                catch
                {
                    // Silencioso en caso de error de navegación
                }
            });

            // Simple refresh placeholder (could call a DB service if available)
            RefreshCommand = new Command(() => { /* Implement refresh from DB if present */ });

            // Subscribe to nuevas entregas para mantener el historial actualizado en tiempo real
            try
            {
                MessagingCenter.Subscribe<CrearEntregaViewModel, EntregaModel>(this, "EntregaCreada", (sender, entrega) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Entregas.Insert(0, entrega);
                    });
                });

                // Escuchar entregas concretadas para agregarlas al historial
                MessagingCenter.Subscribe<object, EntregaModel>(this, "EntregaConcretada", (sender, entrega) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (entrega == null) return;

                        // Evitar duplicados: buscar por Id
                        var existe = Entregas.FirstOrDefault(e => e.Id == entrega.Id);
                        if (existe == null)
                        {
                            // Solo agregar si está concretada (estado 2)
                            if (entrega.Estado == 2)
                                Entregas.Insert(0, entrega);
                        }
                        else
                        {
                            // actualizar datos del historial si ya existe
                            existe.Estado = entrega.Estado;
                            existe.FirmaBase64 = entrega.FirmaBase64;
                            existe.Receptor = entrega.Receptor;
                            existe.Observaciones = entrega.Observaciones;
                            existe.Repartidor = entrega.Repartidor;
                        }
                    });
                });
            }
            catch
            {
                // MessagingCenter puede fallar en algunos entornos; no detener la app
            }
        }

        // Llamar desde la página al cerrar para evitar memory leaks
        public void Unsubscribe()
        {
            try
            {
                MessagingCenter.Unsubscribe<CrearEntregaViewModel, EntregaModel>(this, "EntregaCreada");
                MessagingCenter.Unsubscribe<object, EntregaModel>(this, "EntregaConcretada");
            }
            catch
            {
                // ignorar
            }
        }
    }
}