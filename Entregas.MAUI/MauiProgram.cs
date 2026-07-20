using CommunityToolkit.Maui;
using Entregas.MAUI.Services;
using Entregas.MAUI.ViewModels;
using Entregas.MAUI.Views;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Entregas.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // 2. ASEGÚRATE DE TENER ESTA LÍNEA AQUÍ
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient("ApiCliente", client =>
            {
                // En Android, 'localhost' es 10.0.2.2
                client.BaseAddress = new Uri("https://sistemaentregas.onrender.com");
            });
            // Registro de tu servicio de API
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<CrearEntregaViewModel>();
            builder.Services.AddTransient<CrearEntregaPage>();
            builder.Services.AddTransient<CrearEntregaViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
