namespace Entregas.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("ComprobantePage", typeof(Views.ComprobantePage));
            Routing.RegisterRoute("ResumenEntregaPage", typeof(Views.ResumenEntregaPage)); // NUEVA RUTA
        }
    }
}
