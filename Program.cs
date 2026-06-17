using Microsoft.EntityFrameworkCore;
using LOGIN.Data;
//using MudBlazor.Services; // Agrega este using arriba

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// REGISTRO DE SERVICIOS (Antes de builder.Build)
// ==========================================

// Agregar controladores MVC (vistas) y API controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // ← NUEVO para la API

// Base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Caché y Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Blazor y MudBlazor (¡Movido aquí arriba!)

// builder.Services.AddMudServices(); // ← Registra MudBlazor AQUÍ, antes del Build.

// ==========================================
// CONSTRUCCIÓN DE LA APP
// ==========================================
var app = builder.Build();

// ==========================================
// CONFIGURACIÓN DEL PIPELINE (Middleware)
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Las sesiones deben ir después de Routing pero antes de la Autorización
app.UseSession();
app.UseAuthorization();

// ==========================================
// MAPEO DE RUTAS Y ENDPOINTS
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllers(); // ← NUEVO (para endpoints /api/...)

app.Run();