using Tahsilat.NET.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Secret key ve webhook secret'ı appsettings.local.json'a yazın; dosya git'e girmez.
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ITahsilatClient singleton olarak kaydedilir; controller'lar constructor'dan alır.
builder.Services.AddTahsilat(options =>
{
    options.ApiKey = builder.Configuration["Tahsilat:ApiKey"]
        ?? throw new InvalidOperationException("Tahsilat:ApiKey ayarı eksik. appsettings.local.json dosyasına sk_test_... anahtarınızı yazın.");
    options.TimeoutSeconds = 80;     // varsayılan
    options.MaxNetworkRetries = 3;   // varsayılan
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
