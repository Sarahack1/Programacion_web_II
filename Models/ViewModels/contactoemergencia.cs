using System.ComponentModel.DataAnnotations;

namespace Practica02MVC.Models;

public class ContactoEmergencia
{
    public long IdContacto { get; set; }

    [Required(ErrorMessage = "Debe seleccionar a la usuaria dueña del contacto.")]
    [Display(Name = "Usuaria Propietaria")]
    public long IdUsuarioPropietario { get; set; }

    [Required(ErrorMessage = "El nombre del contacto es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre completo del contacto")]
    public string NombreContacto { get; set; } = "";

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Número de teléfono")]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "El parentesco es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Parentesco (Ej. Madre, Hermana, Amigo)")]
    public string Parentesco { get; set; } = "";

    // Propiedad auxiliar: No existe en la tabla contactos_emergencia.
    // Se llenará con un JOIN a la tabla usuarios/perfiles para mostrar un nombre legible.
    public string? NombrePropietario { get; set; }
}