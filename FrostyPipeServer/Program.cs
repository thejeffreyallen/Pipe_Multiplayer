
using FrostyPipeServer.ServerFiles;
using System.Text;
using Microsoft.AspNetCore.Authentication.Certificate;

Console.OutputEncoding = Encoding.Unicode;
Console.WriteLine($"PIPE ONLINE SERVER V{Servermanager.VERSIONNUMBER}");
Console.WriteLine("Powered by Riptide Networking\n");
Servermanager.Startup();




Console.WriteLine($"Booting with maxplayers: {Servermanager.config.Maxplayers}, port: {Servermanager.config.Port}, tick rate max: {Servermanager.config.TickrateMax} ,min: {Servermanager.config.TickrateMin}");

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(
        CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
     app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();






