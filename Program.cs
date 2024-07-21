using System.Text;
using BtcMiner.Entity;
using BtcMiner.Helpers;
using BtcMiner.Middleware;
using BtcMiner.Models;
using BtcMiner.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("BtcMinerDb") ?? "Data Source = BtcMiner.db";
builder.Services.AddSqlite<MinerDb>(builder.Configuration.GetConnectionString("BtcMinerDb"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Btc Miner Api",
            Description = "Telegram Web App to claim BTC",
            Version = "v1"
        }
    );
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["AppSettings:Secret"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key), // Replace with your security key
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Btc Miner WebApp API V1");
    });

    app.MapGet(
        "/users",
        (MinerDb db) =>
        {
            return db.Users.ToList();
        }
    );
    app.MapPost(
        "/users",
        (AuthRequest model, MinerDb db) =>
        {
            var u = new User
            {
                TelegramId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                LanguageCode = model.LanguageCode,
                AllowWritePm = model.AllowWritePm,
                IsPremium = model.IsPremium,
                ProfilePicUrl = model.ProfilePicUrl
            };
            db.Users.Add(u);
            db.SaveChanges();
            return db.Users.ToList();
        }
    );
}
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapPost(
    "/login",
    ([FromBody] AuthRequest model, IAuthenticationService authenticationService) =>
    {
        var response = authenticationService.Authenticate(model);

        if (response == null)
        {
            // register
            return Results.Unauthorized();
        }

        return Results.Ok(response);
    }
);

app.MapGet(
        "/tasks",
        (MinerDb db, HttpContext context, IAuthenticationService authenticationService) =>
        {
            var user = context.Items["User"] as User;
            return authenticationService.GetTasks(user!);
        }
    )
    .RequireAuthorization();

app.MapGet(
        "/refferals",
        (MinerDb db, HttpContext context, IAuthenticationService authenticationService) =>
        {
            var user = context.Items["User"] as User;
            return authenticationService.GetReferals(user!);
        }
    )
    .RequireAuthorization();

app.MapGet(
        "/claim",
        (MinerDb db, HttpContext context, IAuthenticationService authenticationService) =>
        {
            var user = context.Items["User"] as User;
            return authenticationService.Claim(user!);
        }
    )
    .RequireAuthorization();

app.MapGet(
        "/user/me",
        (HttpContext context, IAuthenticationService authenticationService) =>
            authenticationService.GetMe(context.Items["User"] as User)
    )
    .RequireAuthorization();

app.MapGet(
        "/time",
        (IAuthenticationService authenticationService) => authenticationService.GetServerTime()
    )
    .RequireAuthorization();

app.MapGet(
        "/time/remain",
        (IAuthenticationService authenticationService, HttpContext context) =>
            authenticationService.GetRemainClaimTime(context.Items["User"] as User)
    )
    .RequireAuthorization();

app.Run();
