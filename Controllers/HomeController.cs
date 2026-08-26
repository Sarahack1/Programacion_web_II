using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Practica02FincaMVC.Controllers;

// ¡El candado principal! Obliga a que todos tengan sesión iniciada.
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Nivel 1: Solo la administradora del sistema Sigila puede ver esto
    [Authorize(Roles = "administradora")]
    public IActionResult Administracion()
    {
        return View();
    }

    // Nivel 2: Adaptación del método "Salud" de la práctica a "Alertas" para Sigila
    [Authorize(Roles = "administradora, usuaria_final")]
    public IActionResult Alertas()
    {
        return View();
    }

    // Nivel 3: Adaptación del método "Operaciones" a "RedApoyo" 
    [Authorize(Roles = "administradora, contacto_emergencia")]
    public IActionResult RedApoyo()
    {
        return View();
    }
}