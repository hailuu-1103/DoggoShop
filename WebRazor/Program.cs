using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Text.Json.Serialization;
using WebRazor;
using WebRazor.Materials;

var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddRazorPages();
builder.Services.AddSession(otp => otp.IdleTimeout = TimeSpan.FromMinutes(30));
builder.Services.AddDbContext<PRN221DBContext>();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt => {
        opt.AccessDeniedPath = "/Account/Login";
        opt.LoginPath = "/Account/Login";
    });

var app = builder.Build();
app.UseStaticFiles();
app.UseSession();
app.MapRazorPages();
app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Strict
});
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<HubServer>("/Hub");

app.Run();