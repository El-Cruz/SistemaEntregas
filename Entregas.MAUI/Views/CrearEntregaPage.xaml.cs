using AndroidX.Lifecycle;
using Entregas.MAUI.ViewModels;

namespace Entregas.MAUI.Views;

public partial class CrearEntregaPage : ContentPage
{
    public CrearEntregaPage(CrearEntregaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Verifica si los campos ya tienen datos al cargar la página para dejar las etiquetas flotando
        this.Loaded += (s, e) =>
        {
            if (!string.IsNullOrEmpty(DestinatarioEntry.Text)) FloatLabel(DestinatarioEntry, DestinatarioLabel, DestinatarioBorder, true, true);
            if (!string.IsNullOrEmpty(DireccionEntry.Text)) FloatLabel(DireccionEntry, DireccionLabel, DireccionBorder, true, true);
            if (!string.IsNullOrEmpty(CantidadEntry.Text)) FloatLabel(CantidadEntry, CantidadLabel, CantidadBorder, true, true);
        };
    }

    // Eventos XAML redirigidos a nuestro animador universal
    private void Destinatario_Focused(object sender, FocusEventArgs e) => FloatLabel(DestinatarioEntry, DestinatarioLabel, DestinatarioBorder, true);
    private void Destinatario_Unfocused(object sender, FocusEventArgs e) => FloatLabel(DestinatarioEntry, DestinatarioLabel, DestinatarioBorder, false);

    private void Direccion_Focused(object sender, FocusEventArgs e) => FloatLabel(DireccionEntry, DireccionLabel, DireccionBorder, true);
    private void Direccion_Unfocused(object sender, FocusEventArgs e) => FloatLabel(DireccionEntry, DireccionLabel, DireccionBorder, false);

    private void Cantidad_Focused(object sender, FocusEventArgs e) => FloatLabel(CantidadEntry, CantidadLabel, CantidadBorder, true);
    private void Cantidad_Unfocused(object sender, FocusEventArgs e) => FloatLabel(CantidadEntry, CantidadLabel, CantidadBorder, false);

    // Método universal que replica tu CSS
    private async void FloatLabel(Entry entry, Label label, Border border, bool isFocused, bool isInitialLoad = false)
    {
        uint duration = isInitialLoad ? 0u : 150u; // 150ms exactos como en tu CSS

        if (isFocused)
        {
            // .input:focus (Color azul de Google)
            border.Stroke = Color.FromArgb("#1a73e8");
            label.TextColor = Color.FromArgb("#1a73e8");

            // transform: translateY(-50%) scale(0.8);
            await Task.WhenAll(
                label.TranslateTo(0, -28, duration, Easing.CubicOut),
                label.ScaleTo(0.8, duration, Easing.CubicOut)
            );
        }
        else
        {
            // Solo regresamos el estilo si NO hay un texto válido (input:valid en CSS)
            if (string.IsNullOrEmpty(entry.Text))
            {
                border.Stroke = Color.FromArgb("#9e9e9e");
                label.TextColor = Color.FromArgb("#e8e8e8");

                // Regresa a su tamaño y posición original
                await Task.WhenAll(
                    label.TranslateTo(0, 0, duration, Easing.CubicOut),
                    label.ScaleTo(1, duration, Easing.CubicOut)
                );
            }
            else
            {
                // Si hay texto, se queda flotando pero el borde regresa a gris
                border.Stroke = Color.FromArgb("#9e9e9e");
                label.TextColor = Color.FromArgb("#e8e8e8");
            }
        }
    }

    // Mantén tu evento existente aquí abajo...
    private void OnDestinatarioTextChanged(object sender, TextChangedEventArgs e) { /* Tu código previo */ }
}