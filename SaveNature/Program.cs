using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaveNature.Data;
using SaveNature.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // Регистрация HttpClient
builder.Services.AddScoped<RecommendationMatcherService>();


// Настройка Identity
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Контроллеры и статические файлы
app.UseDefaultFiles();
app.UseStaticFiles();

//Маршрутизация и middleware
app.UseHttpsRedirection();
app.UseRouting();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowLocalhost", builder =>
//    {
//        builder.WithOrigins("https://localhost:7143")
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//    });
//});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// В pipeline
app.UseCors("AllowLocalhost");
app.UseAuthorization();
app.UseAuthentication();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");
app.MapRazorPages();

app.Run();



// t=20240716T155500&s=308.00&fn=7281440701274569&i=3503&fp=1264772410&n=1