using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica02FincaMVC.Data;
using Practica02FincaMVC.Models.ViewModels;

namespace Practica02FincaMVC.Controllers;

public class CuentaController : Controller
{
    private readonly UserRepository _userRepository;

    public CuentaController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        // Si la usuaria ya tiene sesión, la mandamos directo al panel
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByEmailAsync(model.Email);

        // Adaptación Sigila: Validamos 'Estado' en lugar del 'Active' genérico de la práctica
        if (user is null || !user.Estado)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        bool passwordOk;
        try
        {
            // El núcleo de seguridad: Verificamos el hash con BCrypt
            passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        }
        catch
        {
            passwordOk = false;
        }

        if (!passwordOk)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        // Adaptación Sigila: Armamos los "Claims" con tus variables normalizadas
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
            new(ClaimTypes.Name, user.NombreCompleto),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.NombreRol.ToLowerInvariant())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // Ruta requerida para el script de inactividad
    [Authorize]
    [HttpGet]
    public IActionResult Ping()
    {
        return NoContent();
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> LogoutInactividad()
    {
        // Destruimos la cookie de sesión de forma segura
        await HttpContext.SignOutAsync(
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Preparamos el mensaje del servidor
        TempData["AmongUs"] = "El servidor cerró la sala por inactividad";
        
        return RedirectToAction("Login");
    }
}