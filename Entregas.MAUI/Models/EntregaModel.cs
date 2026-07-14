using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using System.Text.Json;

namespace Entregas.MAUI.Models
{
    public class ProductoEntrega
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    // Agregamos INotifyPropertyChanged para que la interfaz se actualice en tiempo real
    public class EntregaModel : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CodigoEntrega { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; } = DateTime.Now;
        public string Destinatario { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        [Ignore]
        public List<ProductoEntrega> Productos { get; set; } = new();
        public string ProductosJson
        {
            get => JsonSerializer.Serialize(Productos);
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    Productos = JsonSerializer.Deserialize<List<ProductoEntrega>>(value) ?? new();
            }
        }

        // Convertimos el Estado en una propiedad reactiva
        private int _estado;
        public int Estado
        {
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    OnPropertyChanged(); // ¡Notifica a la vista que el color debe cambiar!
                }
            }
        }

        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Repartidor { get; set; } = string.Empty;
        public string Receptor { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string FirmaBase64 { get; set; } = string.Empty;

        // Motor interno para notificar cambios a la interfaz
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
