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

// Add custom JWT authentication to our project:
builder.Services.ConfigureJWT(builder.Environment.IsDevelopment(), "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnvpN9dE+EerI1GtvdQnbpmtdAws3/TjvFy89EdhpoTi/dRBsCwnSkyPhcJ/q67wLRnlR5EItTKKjWlviO90gH5/7/TLUkFRAND+yJp0xBYePoJOCvRf9HjD55x25cpZqdXnr7L6ZsCDUfZAH2CxEjYB6OpwXwMMEHyCxtDwhpEEjPmAk2cYHSPyhWtxSTvo84PeCTdCQMKg3RxXqifeQ0+hnvb3bmS52sr6gy7I5POtJsVI8JHWNjB/cycv5S2pzFM+jfLjf4cOw3uFUSShjSkBwL1Y6hz/ps8GsK+e56bVkolqbeUA/2+Oyi6G+7pRVn1tgz/rYtmWocyRk5rbjNQIDAQAB");
builder.Services.AddControllers();
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
