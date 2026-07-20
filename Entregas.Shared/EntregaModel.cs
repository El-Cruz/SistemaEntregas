using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using SQLite;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

< !DOCTYPE html >
< html lang = "es" >
< head >
    < meta charset = "UTF-8" >
    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
    < title > Panel de Control -Sistema de Entregas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
</head>
<body class= "bg-light" >
    < !--Header-- >
    < nav class= "navbar navbar-expand-lg navbar-dark bg-dark mb-4" >
        < div class= "container" >
            < a class= "navbar-brand fw-bold" href = "#" >
                < i class= "bi bi-truck me-2" ></ i > Sistema de Entregas
            </a>
        </div>
    </nav>

    <div class= "container mb-5" >
        < div class= "d-flex justify-content-between align-items-center mb-4" >
            < h2 >< i class= "bi bi-box-seam me-2" ></ i > Gestión de Entregas</h2>
            <button class= "btn btn-outline-secondary btn-sm" onclick = "cargarEntregas()" >
                < i class= "bi bi-arrow-clockwise" ></ i > Recargar
            </ button >
        </ div >


        < !--Formulario para crear entregas -->
        <div class= "card shadow-sm mb-4" >
            < div class= "card-header bg-primary text-white font-weight-bold" >
                < i class= "bi bi-plus-circle me-1" ></ i > Registrar Nueva Entrega
            </div>
            <div class= "card-body" >
                < form id = "formEntrega" onsubmit = "crearEntrega(event)" >
                    < div class= "row g-3" >
                        < div class= "col-md-3" >
                            < label class= "form-label" > Código de Entrega</label>
                            <input type = "text" id= "codigoEntrega" class= "form-control" placeholder = "Ej: ENT-101" required >
                        </ div >
                        < div class= "col-md-4" >
                            < label class= "form-label" > Destinatario </ label >
                            < input type = "text" id = "destinatario" class= "form-control" placeholder = "Nombre del cliente" required >
                        </ div >
                        < div class= "col-md-5" >
                            < label class= "form-label" > Dirección </ label >
                            < input type = "text" id = "direccion" class= "form-control" placeholder = "Dirección de destino" required >
                        </ div >
                        < div class= "col-md-4" >
                            < label class= "form-label" > Repartidor </ label >
                            < input type = "text" id = "repartidor" class= "form-control" placeholder = "Asignar repartidor" >
                        </ div >
                        < div class= "col-md-3" >
                            < label class= "form-label" > Estado Inicial </ label >
                            < select id = "estado" class= "form-select" >
                                < option value = "0" > Pendiente </ option >
                                < option value = "1" > En Ruta </ option >
                                < option value = "2" > Entregado </ option >
                            </ select >
                        </ div >
                        < div class= "col-md-5" >
                            < label class= "form-label" > Observaciones </ label >
                            < input type = "text" id = "observaciones" class= "form-control" placeholder = "Notas o detalles adicionales" >
                        </ div >
                        < div class= "col-12 text-end" >
                            < button type = "submit" class= "btn btn-success px-4" >
                                < i class= "bi bi-save me-1" ></ i > Guardar Registro
                            </ button >
                        </ div >
                    </ div >
                </ form >
            </ div >
        </ div >

        < !--Tabla para listar y eliminar registros -->
        <div class= "card shadow-sm" >
            < div class= "card-header bg-secondary text-white font-weight-bold" >
                < i class= "bi bi-list-task me-1" ></ i > Registros en Base de Datos
            </div>
            <div class= "card-body p-0" >
                < div class= "table-responsive" >
                    < table class= "table table-hover align-middle mb-0" >
                        < thead class= "table-dark" >
                            < tr >
                                < th > ID </ th >
                                < th > Código </ th >
                                < th > Fecha </ th >
                                < th > Destinatario </ th >
                                < th > Dirección </ th >
                                < th > Repartidor </ th >
                                < th > Estado </ th >
                                < th class= "text-center" > Acciones </ th >
                            </ tr >
                        </ thead >
                        < tbody id = "tablaEntregas" >
                            < !--Filas dinámicas-- >
                        </ tbody >
                    </ table >
                </ div >
            </ div >
        </ div >
    </ div >

    < script >
        const apiUrl = '/api/entregas';

// Helper para mostrar badge visual según Estado
function obtenerBadgeEstado(estado)
{
    switch (parseInt(estado))
    {
        case 1:
            return '<span class="badge bg-warning text-dark"><i class="bi bi-truck me-1"></i>En Ruta</span>';
        case 2:
            return '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Entregado</span>';
        default:
            return '<span class="badge bg-secondary"><i class="bi bi-clock me-1"></i>Pendiente</span>';
    }
}

// Formateador de fecha
function formatearFecha(fechaIso)
{
    if (!fechaIso) return '-';
    const f = new Date(fechaIso);
    return f.toLocaleDateString() + ' ' + f.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Obtener entregas (GET)
async function cargarEntregas()
{
    try
    {
        const response = await fetch(apiUrl);
        if (!response.ok) throw new Error('Error al obtener datos');

        const entregas = await response.json();
        const tbody = document.getElementById('tablaEntregas');
        tbody.innerHTML = '';

        if (entregas.length === 0)
        {
            tbody.innerHTML = `
                        < tr >
                            < td colspan = "8" class= "text-center py-4 text-muted" >
                                < i class= "bi bi-inbox fs-3 d-block mb-2" ></ i > No hay entregas registradas actualmente.
                            </td>
                        </tr>`;
return;
                }

                entregas.forEach(e => {
                tbody.innerHTML += `
                        < tr >
                            < td >< strong >#${e.id}</strong></td>
                            < td >< span class= "badge bg-light text-dark border" >${ e.codigoEntrega || 'S/N'}</ span ></ td >
                            < td >< small >${ formatearFecha(e.fechaEntrega)}</ small ></ td >
                            < td >${ e.destinatario || '-'}</ td >
                            < td >${ e.direccion || '-'}</ td >
                            < td >${ e.repartidor || '-'}</ td >
                            < td >${ obtenerBadgeEstado(e.estado)}</ td >
                            < td class= "text-center" >
                                < button class= "btn btn-outline-danger btn-sm" onclick = "eliminarEntrega(${e.id})" title = "Eliminar" >
                                    < i class= "bi bi-trash" ></ i >
                                </ button >
                            </ td >
                        </ tr >
                    `;
                });
            } catch (err) {
    console.error(err);
    document.getElementById('tablaEntregas').innerHTML = `
                    < tr >< td colspan = "8" class= "text-center text-danger py-3" > Error al conectar con la API.</td></tr>`;
            }
        }

        // Crear nueva entrega (POST)
        async function crearEntrega(event)
{
            event.preventDefault();

    const nuevaEntrega = {
                codigoEntrega: document.getElementById('codigoEntrega').value,
                destinatario: document.getElementById('destinatario').value,
                direccion: document.getElementById('direccion').value,
                repartidor: document.getElementById('repartidor').value,
                estado: parseInt(document.getElementById('estado').value),
                observaciones: document.getElementById('observaciones').value,
                fechaEntrega: new Date().toISOString()
            };

try
{
    const res = await fetch(apiUrl, {
    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevaEntrega)
                });

    if (res.ok)
    {
        document.getElementById('formEntrega').reset();
        cargarEntregas();
    }
    else
    {
        alert('Error al guardar la entrega.');
    }
}
catch (err)
{
    console.error(err);
    alert('Ocurrió un error de red.');
}
        }

        // Eliminar entrega (DELETE)
        async function eliminarEntrega(id)
{
    if (confirm(`¿Seguro que deseas eliminar la entrega #${id}?`)) {
                try
        {
            const res = await fetch(`${ apiUrl}/${ id}`, { method: 'DELETE' });
            if (res.ok)
            {
                cargarEntregas();
            }
            else
            {
                alert('No se pudo eliminar el registro.');
            }
        }
        catch (err)
        {
            console.error(err);
            alert('Error al intentar conectar.');
        }
}
        }

        // Carga inicial
        cargarEntregas();
    </ script >
</ body >
</ html >