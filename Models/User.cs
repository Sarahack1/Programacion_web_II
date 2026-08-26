namespace Practica02FincaMVC.Models;

public class User
{
    public long IdUsuario { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public int IdRol { get; set; }
    public bool Estado { get; set; }
    public bool MustChangePassword { get; set; }
    public string NombreRol { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
}