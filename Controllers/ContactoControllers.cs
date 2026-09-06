using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Practica02MVC.Data;
using Practica02MVC.Models;
using System.Security.Claims;

namespace Practica02MVC.Controllers;

[Authorize]
public class ContactosController : Controller
{
    private readonly ContactoEmergenciaRepository _repository;

    public ContactosController(ContactoEmergenciaRepository repository)
    {
        _repository = repository;
    }

    // READ: todos los usuarios autenticados (Listar / Detalles).
// READ: Listar filtrando por usuario o rol
public async Task<IActionResult> Index()
{
    // Extraemos el ID del usuario que inició sesión desde las "cookies" (Claims) de .NET
    string? idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    _ = long.TryParse(idClaim, out long usuarioId);

    // Verificamos si tiene el poder absoluto
    bool esAdmin = User.IsInRole("administradora");

    // Mandamos a pedir la lista con sus reglas estrictas
    var contactos = await _repository.GetAllAsync(usuarioId, esAdmin);
    return View(contactos);
}

    public async Task<IActionResult> Details(long id)
    {
        var contacto = await _repository.GetByIdAsync(id);
        if (contacto is null) return NotFound();
        return View(contacto);
    }

    // CREATE: administradora.
    [Authorize(Roles = "administradora, usuaria_final")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarCatalogosAsync();
        return View(new ContactoEmergencia());
    }

    [Authorize(Roles = "administradora, usuaria_final")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactoEmergencia contacto)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(contacto);
            return View(contacto);
        }

        try
        {
            long id = await _repository.CreateAsync(contacto);
            TempData["Mensaje"] = "Contacto registrado correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (MySqlException)
        {
            ModelState.AddModelError("", "Ocurrió un error al guardar en la base de datos.");
            await CargarCatalogosAsync(contacto);
            return View(contacto);
        }
    }

    // UPDATE: administradora y usuaria_final.
    [Authorize(Roles = "administradora")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var contacto = await _repository.GetByIdAsync(id);
        if (contacto is null) return NotFound();
        await CargarCatalogosAsync(contacto);
        return View(contacto);
    }

    [Authorize(Roles = "administradora")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ContactoEmergencia contacto)
    {
        if (id != contacto.IdContacto) return BadRequest();

        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(contacto);
            return View(contacto);
        }

        try
        {
            bool actualizado = await _repository.UpdateAsync(contacto);
            if (!actualizado) return NotFound();
            TempData["Mensaje"] = "Contacto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (MySqlException)
        {
            ModelState.AddModelError("", "Ocurrió un error al actualizar.");
            await CargarCatalogosAsync(contacto);
            return View(contacto);
        }
    }

    // DELETE: solamente administradora.
    [Authorize(Roles = "administradora")]
    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var contacto = await _repository.GetByIdAsync(id);
        if (contacto is null) return NotFound();
        return View(contacto);
    }

    [Authorize(Roles = "administradora")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        bool eliminado = await _repository.DeleteAsync(id);
        if (!eliminado) return NotFound();
        
        TempData["Mensaje"] = "Contacto dado de baja correctamente (Borrado lógico).";
        return RedirectToAction(nameof(Index));
    }

    // Método auxiliar para llenar el <select> en las vistas de Create y Edit
    private async Task CargarCatalogosAsync(ContactoEmergencia? contacto = null)
    {
        ViewBag.Usuarias = new SelectList(
            await _repository.GetUsuariasAsync(),
            "Id", "Nombre", contacto?.IdUsuarioPropietario);
    }
}