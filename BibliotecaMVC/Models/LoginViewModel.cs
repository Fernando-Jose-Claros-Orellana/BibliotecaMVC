using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El usuario o correo electrónico es obligatorio.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
