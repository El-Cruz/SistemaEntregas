using System.Net.Http.Json;
using Entregas.Shared;

namespace Entregas.MAUI.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> GuardarEntregaAsync(EntregaModel entrega)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiCliente");

                // Enviamos el objeto entrega a la ruta "api/entregas"
                var response = await client.PostAsJsonAsync("api/entregas", entrega);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear el error si lo necesitas
                Console.WriteLine($"Error al conectar con la API: {ex.Message}");
                return false;
            }
        }
    }
}
