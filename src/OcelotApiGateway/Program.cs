using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OcelotApiGateway.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure CORS for the gateway:
const string corsPolicy = "cors-app-policy";
builder.Services.AddCors(c => c.AddPolicy(corsPolicy, corsPolicyBuilder =>
{
    corsPolicyBuilder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddOcelot(builder.Configuration);

// Add custom claim authorizer to project:
builder.Services.DecorateClaimAuthoriser();

// Add custom JWT authentication to our project:
string issuer = builder.Configuration.GetValue<string>("Keycloak:Issuer");
string publicKey = builder.Configuration.GetValue<string>("Keycloak:PublicKey");
builder.Services.ConfigureJWT(builder.Environment.IsDevelopment(), jwtKey: publicKey, issuer: issuer);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kinoroom-Keycloak", Version = "v1"});
    // Add security definition:
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT authorization header",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Id = JwtBearerDefaults.AuthenticationScheme, //The name of the previously defined security scheme.
                    Type = ReferenceType.SecurityScheme
                }
            },new List<string>()
        }
    });
});

// Add Ocelot to the project with a config file.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseOcelot().Wait();
app.Run();