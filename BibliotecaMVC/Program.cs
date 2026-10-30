using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDB")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<BibliotecaContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<BibliotecaMVC.Servicios.ILibroService, BibliotecaMVC.Servicios.LibroService>();
builder.Services.AddScoped<BibliotecaMVC.Servicios.IAutorService, BibliotecaMVC.Servicios.AutorServiceAlternativo>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = ["admin", "bibliotecario", "usuario"];

    foreach (string roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el rol '{roleName}': {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
            }
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var usuarios = await userManager.Users.ToListAsync();

    foreach (IdentityUser usuario in usuarios)
    {
        var rolesUsuario = await userManager.GetRolesAsync(usuario);

        if (rolesUsuario.Count == 0)
        {
            IdentityResult assignmentResult = await userManager.AddToRoleAsync(usuario, "usuario");

            if (!assignmentResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo asignar el rol básico al usuario: {string.Join(", ", assignmentResult.Errors.Select(error => error.Description))}");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
