using SQLite;
using Entregas.Shared;

namespace Entregas.MAUI.Services
{
    public static class DatabaseService
    {
        private static SQLiteAsyncConnection? _database;

        private static async Task Init()
        {
            if (_database != null) return;

            // Define la ruta física donde vivirá la base de datos en tu Android/iOS
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "MinueEntregas.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Crea la tabla si es la primera vez que se abre la app
            await _database.CreateTableAsync<EntregaModel>();
        }

        public static async Task<List<EntregaModel>> ObtenerEntregasAsync()
        {
            await Init();
            return await _database.Table<EntregaModel>().ToListAsync();
        }

        public static async Task GuardarEntregaAsync(EntregaModel entrega)
        {
            await Init();

            // Si el Id es mayor a 0, significa que ya existe, entonces la actualizamos. Si no, la creamos.
            if (entrega.Id > 0)
                await _database.UpdateAsync(entrega);
            else
                await _database.InsertAsync(entrega);
        }
    }
}
