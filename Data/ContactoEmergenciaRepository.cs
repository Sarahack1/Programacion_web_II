using MySqlConnector;
using Practica02MVC.Models;

namespace Practica02MVC.Data;

public class ContactoEmergenciaRepository
{
    private readonly string _connectionString;

    public ContactoEmergenciaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SigilaConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena SigilaConnection.");
    }

    public async Task<List<ContactoEmergencia>> GetAllAsync(long usuarioId, bool esAdmin)
    {
        // Construimos la consulta base (solo contactos activos por el borrado lógico)
        string sql = @"
            SELECT c.id_contacto, c.id_usuario_propietario, p.nombre_completo AS nombre_propietario,
                c.nombre_contacto, c.telefono, c.parentesco
            FROM contactos_emergencia c
            INNER JOIN perfiles p ON p.id_usuario = c.id_usuario_propietario
            WHERE c.estado_contacto = 1 ";

        // Si NO es administradora, le concatenamos un candado extra a la consulta
        if (!esAdmin)
        {
            sql += " AND c.id_usuario_propietario = @usuarioId ";
        }

        sql += " ORDER BY c.id_contacto DESC;";

        var lista = new List<ContactoEmergencia>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);

        // Si agregamos el candado, le pasamos el valor del parámetro por seguridad
        if (!esAdmin)
        {
            command.Parameters.AddWithValue("@usuarioId", usuarioId);
        }

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(MapContacto(reader));
        }
        return lista;
    }

    public async Task<ContactoEmergencia?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT c.id_contacto, c.id_usuario_propietario, p.nombre_completo AS nombre_propietario,
                   c.nombre_contacto, c.telefono, c.parentesco
            FROM contactos_emergencia c
            INNER JOIN perfiles p ON p.id_usuario = c.id_usuario_propietario
            WHERE c.id_contacto = @id
            LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapContacto(reader) : null;
    }
    public async Task<int> CreateAsync(ContactoEmergencia contacto)
    {
        const string sql = @"
            INSERT INTO contactos_emergencia 
            (id_usuario_propietario, nombre_contacto, telefono, parentesco) 
            VALUES 
            (@id_usuario, @nombre, @telefono, @parentesco);";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, contacto);
        
        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }

    public async Task<bool> UpdateAsync(ContactoEmergencia contacto)
    {
        const string sql = @"
            UPDATE contactos_emergencia 
            SET id_usuario_propietario = @id_usuario,
                nombre_contacto = @nombre,
                telefono = @telefono,
                parentesco = @parentesco
            WHERE id_contacto = @id;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, contacto);
        command.Parameters.AddWithValue("@id", contacto.IdContacto);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // Borrado Lógico: Solo actualizamos el estado a 0 (Inactivo)
        const string sql = "UPDATE contactos_emergencia SET estado_contacto = 0 WHERE id_contacto = @id;";
        
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Método para llenar la lista desplegable de usuarias
    public async Task<List<CatalogoItem>> GetUsuariasAsync()
    {
        var lista = new List<CatalogoItem>();
        const string sql = "SELECT id_usuario AS id, nombre_completo AS nombre FROM perfiles ORDER BY nombre_completo;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new CatalogoItem
            {
                Id = reader.GetInt64("id"),
                Nombre = reader.GetString("nombre")
            });
        }
        return lista;
    }

    private static void AddParameters(MySqlCommand command, ContactoEmergencia contacto)
    {
        // Los parámetros evitan la inyección SQL protegiendo a Sigila
        command.Parameters.AddWithValue("@id_usuario", contacto.IdUsuarioPropietario);
        command.Parameters.AddWithValue("@nombre", contacto.NombreContacto.Trim());
        command.Parameters.AddWithValue("@telefono", contacto.Telefono.Trim());
        command.Parameters.AddWithValue("@parentesco", contacto.Parentesco.Trim());
    }

    private static ContactoEmergencia MapContacto(MySqlDataReader reader)
    {
        return new ContactoEmergencia
        {
            IdContacto = reader.GetInt64("id_contacto"),
            IdUsuarioPropietario = reader.GetInt64("id_usuario_propietario"),
            NombrePropietario = reader.GetString("nombre_propietario"),
            NombreContacto = reader.GetString("nombre_contacto"),
            Telefono = reader.GetString("telefono"),
            Parentesco = reader.GetString("parentesco")
        };
    }
}