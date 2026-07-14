namespace Entregas.API.Models
{
    public class Entrega
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Observaciones { get; set; } = string.Empty;

        // 0: Pendiente, 1: En Camino, 2: Entregado
        public int Estado { get; set; } = 0;

        // Coordenadas para la redirección a Google Maps
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        // Guardaremos la firma digitalizada
        public string FirmaBase64 { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
