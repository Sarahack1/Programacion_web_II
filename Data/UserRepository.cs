using MySqlConnector;
using Practica02FincaMVC.Models;

namespace Practica02FincaMVC.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SigilaConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena SigilaConnection.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
      
        const string sql = @"
            SELECT u.id_usuario, u.email, u.password, u.estado, u.must_change_password,
                   r.nombre_rol, p.nombre_completo
            FROM usuarios u
            INNER JOIN roles r ON u.id_rol = r.id_rol
            LEFT JOIN perfiles p ON u.id_usuario = p.id_usuario
            WHERE u.email = @email
            LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            IdUsuario = reader.GetInt64("id_usuario"),
            Email = reader.GetString("email"),
            Password = reader.GetString("password"),
            Estado = reader.GetBoolean("estado"),
            MustChangePassword = reader.GetBoolean("must_change_password"),
            NombreRol = reader.GetString("nombre_rol"),
            
            NombreCompleto = reader.IsDBNull(reader.GetOrdinal("nombre_completo")) 
                             ? "Usuario Sin Perfil" 
                             : reader.GetString("nombre_completo")
        };
    }
}