using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System;

namespace Entregas.MAUI.Services
{
    public class GooglePlacesService
    {
        // Reemplázalo por tu clave real
        private readonly string _apiKey = "AIzaSyDvsm4truQu9HXvxpEb1ZfhODhVAskbUnA";
        private readonly HttpClient _httpClient;

        public GooglePlacesService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(double Latitud, double Longitud)> BuscarCoordenadasAsync(string textoBusqueda)
        {
            if (string.IsNullOrWhiteSpace(textoBusqueda))
                return (0, 0);

            string url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(textoBusqueda)}&key={_apiKey}";

            try
            {
                var respuesta = await _httpClient.GetFromJsonAsync<GooglePlacesResponse>(url);

                if (respuesta != null && respuesta.Results.Count > 0)
                {
                    var ubicacion = respuesta.Results[0].Geometry.Location;
                    return (ubicacion.Lat, ubicacion.Lng);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar Google Places: {ex.Message}");
            }

            return (0, 0);
        }

        public async Task<List<string>> AutocompleteAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return new List<string>();

            // Puedes ajustar &types=establishment o &types=geocode según lo que necesites
            string url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Uri.EscapeDataString(input)}&key={_apiKey}&types=establishment";

            try
            {
                var respuesta = await _httpClient.GetFromJsonAsync<AutocompleteResponse>(url);
                if (respuesta != null && respuesta.Predictions != null)
                {
                    return respuesta.Predictions.Select(p => p.Description).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Autocomplete: {ex.Message}");
            }

            return new List<string>();
        }
    }

    // Clases auxiliares para mapear la respuesta JSON de Google (Text Search)
    public class GooglePlacesResponse
    {
        public List<PlaceResult> Results { get; set; } = new();
    }

    public class PlaceResult
    {
        public GeometryData Geometry { get; set; } = new();
    }

    public class GeometryData
    {
        public LocationData Location { get; set; } = new();
    }

    public class LocationData
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    // Clases para autocomplete
    public class AutocompleteResponse
    {
        public List<Prediction> Predictions { get; set; } = new();
    }

    public class Prediction
    {
        public string Description { get; set; } = string.Empty;
    }
}
