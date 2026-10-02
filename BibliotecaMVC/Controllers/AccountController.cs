using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string nombreUsuario = model.UsernameOrEmail.Trim();

        if (nombreUsuario.Contains('@'))
        {
            IdentityUser? usuario = await _userManager.FindByEmailAsync(nombreUsuario);

            if (usuario != null)
            {
                nombreUsuario = usuario.UserName ?? nombreUsuario;
            }
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            nombreUsuario,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Las credenciales no son correctas.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IdentityUser usuario = new()
        {
            UserName = model.Username.Trim(),
            Email = model.Email.Trim()
        };

        IdentityResult resultado = await _userManager.CreateAsync(usuario, model.Password);

        if (resultado.Succeeded)
        {
            return RedirectToAction(nameof(Login));
        }

        foreach (IdentityError error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
