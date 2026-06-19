using Microsoft.EntityFrameworkCore;
using LOGIN.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// REGISTRO DE SERVICIOS (Antes de builder.Build)
// ==========================================

// Agregar controladores MVC (vistas) y API controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// 🔥 CAMBIO IMPORTANTE: Base de datos con PostgreSQL/Supabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Configuración para mejorar la estabilidad en la nube
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);

        // Configurar para usar el pooler de conexiones de Supabase
     //   npgsqlOptions.MaxPoolSize(100);
      //  npgsqlOptions.MinPoolSize(1);
    });
});

// Caché y Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// CONSTRUCCIÓN DE LA APP
// ==========================================
var app = builder.Build();
// Fuerza a .NET y Npgsql a aceptar comportamientos heredados de DateTime (Local como UTC)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ==========================================
// CONFIGURACIÓN DEL PIPELINE (Middleware)
// ==========================================

// 🔥 NUEVO: Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Migraciones aplicadas correctamente a Supabase/PostgreSQL");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
        Console.WriteLine($"Detalle: {ex.InnerException?.Message}");
        // La aplicación continuará ejecutándose, pero las migraciones fallaron
    }
}

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

app.MapControllers(); // Para endpoints /api/...

app.Run();