using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Entregas.MAUI.Models;
using Entregas.MAUI.Services;
using Entregas.MAUI.Utilities;
using Microsoft.Maui.Controls;

namespace Entregas.MAUI.ViewModels
{
    public class CrearEntregaViewModel : INotifyPropertyChanged
    {
        private readonly GooglePlacesService _googlePlacesService;

        public EntregaModel NuevaEntrega { get; set; }
        public ObservableCollection<string> ProductosDisponibles { get; set; }

        // Productos añadidos a la orden (vista)
        public ObservableCollection<ProductoEntrega> ProductosEnOrden { get; set; }

        // Campos de la fila de entrada
        private string? _selectedProducto;
        public string? SelectedProducto
        {
            get => _selectedProducto;
            set { _selectedProducto = value; OnPropertyChanged(); }
        }

        private string _cantidadTexto = "1";
        public string CantidadTexto
        {
            get => _cantidadTexto;
            set { _cantidadTexto = value; OnPropertyChanged(); }
        }

        private bool _agregarChecked;
        public bool AgregarChecked
        {
            get => _agregarChecked;
            set
            {
                if (_agregarChecked == value) return;
                _agregarChecked = value;
                OnPropertyChanged();
                if (value)
                {
                    // Cuando el usuario marca el check, intentamos agregar y desmarcamos
                    AddProducto();
                    // Aseguramos que el check vuelva a false para permitir agregar de nuevo
                    _agregarChecked = false;
                    OnPropertyChanged(nameof(AgregarChecked));
                }
            }
        }

        // Simulación de base de datos local de clientes (Nombre -> Dirección)
        private Dictionary<string, string> ClientesFrecuentes = new();

        public ICommand BuscarCoordenadasCommand { get; }
        public ICommand GuardarEntregaCommand { get; }
        public ICommand AutocompletarClienteCommand { get; }

        public ICommand AddProductoCommand { get; }
        public ICommand RemoveProductoCommand { get; }

        public CrearEntregaViewModel()
        {
            _googlePlacesService = new GooglePlacesService();
            NuevaEntrega = new EntregaModel
            {
                CodigoEntrega = GenerarCodigoEntrega(),
                FechaEntrega = DateTime.Now
            };

            ProductosDisponibles = new ObservableCollection<string>
            {
                "Chicken & Zucchini", "Beef & Beets", "Pork & Yucca",
                "Lamb & Sweet Potatoe", "Fish & Squash", "Quail & Malanga",
                "Pork & Brown Rice", "Fresh Balance", "Renal Forte",
                "Inmune Shield", "Glyco Balance", "Pork S/A + Malanga",
                "Bark Bites Probioticas"
            };

            ProductosEnOrden = new ObservableCollection<ProductoEntrega>();

            // Clientes de prueba para el autocompletado
            ClientesFrecuentes.Add("Veterinaria Patitas", "Av. República y Eloy Alfaro");

            BuscarCoordenadasCommand = new Command<string>(async (direccion) => await BuscarUbicacion(direccion));
            GuardarEntregaCommand = new Command(async () => await GuardarEntrega());
            AutocompletarClienteCommand = new Command<string>(VerificarClienteFrecuente);

            AddProductoCommand = new Command(AddProducto);
            RemoveProductoCommand = new Command<ProductoEntrega>(RemoveProducto);
        }

        private string GenerarCodigoEntrega()
        {
            Random rnd = new Random();
            return $"ENT-{rnd.Next(1000, 9999)}";
        }

        private void VerificarClienteFrecuente(string nombreLocal)
        {
            if (!string.IsNullOrWhiteSpace(nombreLocal) && ClientesFrecuentes.ContainsKey(nombreLocal))
            {
                NuevaEntrega.Direccion = ClientesFrecuentes[nombreLocal];
                OnPropertyChanged(nameof(NuevaEntrega));
            }
        }

        private async System.Threading.Tasks.Task BuscarUbicacion(string direccion)
        {
            var coordenadas = await _googlePlacesService.BuscarCoordenadasAsync(direccion);
            if (coordenadas.Latitud != 0 && coordenadas.Longitud != 0)
            {
                NuevaEntrega.Latitud = coordenadas.Latitud;
                NuevaEntrega.Longitud = coordenadas.Longitud;
                await Shell.Current.DisplayAlert("Éxito", "Ubicación encontrada en Google Maps.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Aviso", "No se encontraron coordenadas exactas.", "OK");
            }
        }

        private async System.Threading.Tasks.Task GuardarEntrega()
        {
            // 1. Validar que la entrega tenga coordenadas buscadas en el mapa
            if (NuevaEntrega.Latitud == 0 && NuevaEntrega.Longitud == 0)
            {
                await Shell.Current.DisplayAlert("Atención", "Por favor, busca la ubicación en el mapa antes de guardar.", "OK");
                return;
            }

            // 2. Validar que se haya añadido al menos un producto a la lista
            if (!ProductosEnOrden.Any())
            {
                await Shell.Current.DisplayAlert("Atención", "Debe agregar al menos un producto a la orden.", "OK");
                return;
            }

            // 3. Asegurar que la lista de productos del modelo esté emparejada con la interfaz
            NuevaEntrega.Productos = ProductosEnOrden.ToList();

            // 4. GUARDAR EN LA BASE DE DATOS FÍSICA (SQLite)
            await DatabaseService.GuardarEntregaAsync(NuevaEntrega);

            // Notify other parts of the app that a new entrega was created
            try
            {
                WeakReferenceMessenger.Default.Send(new EntregaCreadaMessage(NuevaEntrega));
            }
            catch { }

            await Shell.Current.DisplayAlert("Éxito", $"Entrega {NuevaEntrega.CodigoEntrega} guardada correctamente.", "OK");

            // 5. Resetear la vista a los valores por defecto para un nuevo pedido
            ProductosEnOrden.Clear();

            NuevaEntrega = new EntregaModel
            {
                CodigoEntrega = GenerarCodigoEntrega(),
                FechaEntrega = DateTime.Now
            };

            SelectedProducto = null;
            CantidadTexto = "1";

            // Notificar cambios a la UI
            OnPropertyChanged(nameof(ProductosEnOrden));
            OnPropertyChanged(nameof(NuevaEntrega));
            OnPropertyChanged(nameof(SelectedProducto));
            OnPropertyChanged(nameof(CantidadTexto));

            // 6. Redirigir al usuario al Dashboard
            await Shell.Current.GoToAsync("//DashboardPage");
        }

        private void AddProducto()
        {
            if (string.IsNullOrWhiteSpace(SelectedProducto))
            {
                Shell.Current.DisplayAlert("Aviso", "Seleccione un producto.", "OK");
                return;
            }

            if (!int.TryParse(CantidadTexto, out int cantidad) || cantidad <= 0)
            {
                Shell.Current.DisplayAlert("Aviso", "Ingrese una cantidad válida (> 0).", "OK");
                return;
            }

            var producto = new ProductoEntrega
            {
                Nombre = SelectedProducto!,
                Cantidad = cantidad
            };

            ProductosEnOrden.Add(producto);
            NuevaEntrega.Productos.Add(producto);

            // Limpiar inputs para la siguiente fila
            SelectedProducto = null;
            CantidadTexto = "1";
        }

        private void RemoveProducto(ProductoEntrega? producto)
        {
            if (producto == null) return;
            ProductosEnOrden.Remove(producto);
            NuevaEntrega.Productos.Remove(producto);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}