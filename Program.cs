using Npgsql.EntityFrameworkCore.PostgreSQL;
using GestorAtividades.Data;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UsuarioService>();


builder.Services.AddControllersWithViews();

// Banco de dados MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Autenticação por Cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Auth/Login";
        config.AccessDeniedPath = "/Auth/Denied";
        config.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ATENÇÃO: Autenticação antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
