using Ecomm_project_1.DataAccess;
using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using Stripe;
using System.Security.Claims;
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("conSTR") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.LogoutPath = $"/Identity/Account/Logout";
});
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "2174004093432938";
    option.AppSecret = "e500016c5f2c995b6a183d70c44f88f9";
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "371976608900-d218sif2e4e1hf6c3h1gg8vj61d9q38k.apps.googleusercontent.com";
    options.ClientSecret = "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";
});
builder.Services.AddAuthentication().AddLinkedIn(options =>
{
    options.ClientId = "86rkutftb5b04j";
    options.ClientSecret = "WPL_AP1.saDVgPSJE5GSemJi.Pe74EA==";
});
builder.Services.AddAuthentication().AddGitHub(options =>
{
    options.ClientId = "M1Q0VHp4SkVncjFNQ2tjT2ZmQ2M6MTpjaQ";
    options.ClientSecret = "3PCiYcOpvqvhBzkK5UDENzPBxnMLS4NQlYHtJGeIZsvM3uJ2Sy";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("RazorpaySettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddTwilioClient(builder.Configuration.GetSection("Twilio"));
builder.Services.AddScoped<ITwilioService, TwilioService>();
builder.Services.AddTransient<ISmsSender, SmsSender>();



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

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings")["SecretKey"];

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
