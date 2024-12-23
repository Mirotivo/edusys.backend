using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using MediatR;
using System.Reflection;
using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Configurations
var key = builder.Configuration.GetValue<string>("Jwt:Key");

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddSingleton<Dictionary<string, string>>(new Dictionary<string, string>());
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<skillseekDbContext>(options =>
// {
//     // Ensure the database is created
//     options.UseSqlServer(connectionString);
//     options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
//     options.EnableSensitiveDataLogging();
// });
string connString = builder.Configuration.GetConnectionString("Sqlite") ?? "";
builder.Services.AddDbContext<skillseekDbContext>(option => option.UseSqlite(connString));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddTransients(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<PaymentGatewayFactory>();
builder.Services.AddScoped<PayPalPaymentGateway>();
builder.Services.AddScoped<StripePaymentGateway>();
builder.Services.AddScoped<INotificationChannel, SignalRNotificationChannel>();
builder.Services.AddScoped<INotificationChannel, SmsNotificationChannel>();
builder.Services.AddScoped<INotificationChannel, EmailNotificationChannel>();
builder.Services.AddSingleton<IEmailService, SendGridEmailService>();
builder.Services.AddSingleton<ISmsService, TwilioSmsService>();

builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                OnTokenValidated = context =>
                {
                    // Token validation succeeded
                    Console.WriteLine("Token validated: " + context.Principal.Identity.Name);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notification"))
                    {
                        context.Token = accessToken;
                    }

                    // Invoked when a WebSocket or Long Polling request is received.
                    Console.WriteLine("Message received: " + context.Token);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // Authentication challenge event
                    Console.WriteLine("Authentication challenge: " + context.Error);
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    // Authorization failure event
                    Console.WriteLine("Authorization failed: " + context);
                    return Task.CompletedTask;
                }
            };

        });

// Configurations
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Payments:Stripe"));
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("Payments:PayPal"));
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("Notifications:SendGrid"));
builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection("Notifications:Twilio"));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("ExternalServices:Google"));
builder.Services.Configure<FacebookOptions>(builder.Configuration.GetSection("ExternalServices:Facebook"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new
            {
                Message = "Validation failed",
                Errors = errors
            };

            return new BadRequestObjectResult(response);
        });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<skillseekDbContext>();
    DbInitializer.Initialize(dbContext);
}

// Add Prometheus metrics
app.UseMetricServer(); // Exposes metrics at /metrics endpoint
app.UseHttpMetrics();  // Measures HTTP request duration and counts

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        Console.WriteLine($"Serving file: {ctx.File.PhysicalPath}");
    }
});
app.UseCookiePolicy();

app.UseRouting(); // Add this line to configure routing

app.UseCors(options =>
{
    options.WithOrigins(
        "http://localhost:4200",
        "https://localhost:8000",
        "https://localhost:9000/",
        "https://92.205.162.126:8000",
        "http://110.144.148.168",
        "http://110.144.148.168:80",
        "http://110.144.148.168:8000"
    );
    // options.AllowAnyOrigin();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
    options.AllowCredentials();
});

app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();
// app.UseAuthentication(); // Add authentication middleware

// app.MapControllers();
// app.MapGet("/", () => "Hello World!");
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    app.MapHub<NotificationHub>("/notification");
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Welcome to the dev page!");
    });
});


using (var scope = app.Services.CreateScope())
{
    var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
    try
    {
        paymentService.ProcessPastBookedLessons().GetAwaiter().GetResult();
        Console.WriteLine("Successfully processed past booked lessons.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing lessons: {ex.Message}");
    }
}
app.Run();
