using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Services
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<RendicionService>();
builder.Services.AddScoped<IMfaService, MfaService>();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Add authorization
builder.Services.AddAuthorization();

// SignalR y servicios relacionados
builder.Services.AddSignalR();
builder.Services.AddScoped<RendicionesPrimar.Services.RendicionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHsts();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirect root to login if not authenticated
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" && !(context.User.Identity?.IsAuthenticated ?? false))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    await next();
});

// AGREGAR MAPEO DE CONTROLADORES CON ATRIBUTOS
app.MapControllers();

// RUTA POR DEFECTO
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear el hub de notificaciones SignalR
app.MapHub<NotificacionesHub>("/notificacionesHub");

app.Run();