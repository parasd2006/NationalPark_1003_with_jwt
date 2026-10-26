using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Repository;
using NationalParkWebApp_1003.Repository.IRepository;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IunitOfWorkRepository, UnitofworkReposiory>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddHttpClient();
builder.Services.Configure<NationalParkWebApp_1003.Models.StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<NationalParkWebApp_1003.Utility.NotificationSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();