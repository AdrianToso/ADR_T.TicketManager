using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ADR_T.TicketManager.Infrastructure.Identity;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ADR_T.TicketManager.WebAPI.Extensions;

public static class ServiceExtensions
{
    // Extracción de la configuración de JSON y Servicios base
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Configurar serialización de Enums como strings
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();

        return services;
    }

    // Extracción de la configuración de JWT, Identity y Policies
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración de autenticación JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!))
            };
        });

        // Configurar políticas basadas en roles Identity
        services.AddAuthorization(options =>
        {
            Policies.ConfigurarPolicies(options);
        });

        // Configuración de Identity (mantener aquí o en Infrastructure, pero NO en ambos)
        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        });

        return services;
    }

    // Extracción de la configuración de CORS
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigin = configuration["CorsSettings:AngularClientOrigin"] ?? "http://localhost:4200";

        // PARA SOPORTAR DOCKER
        var additionalOrigins = configuration.GetSection("CorsSettings:AdditionalOrigins").Get<string[]>();
        var allOrigins = new List<string> { allowedOrigin };

        if (additionalOrigins != null)
        {
            allOrigins.AddRange(additionalOrigins);
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            options.AddPolicy("AllowAngular", builder =>
            {
                builder.WithOrigins(allOrigins.ToArray())
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
        return services;
    }

    // Extracción de la configuración de Swagger
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
            c.SchemaFilter<EnumSchemaFilter>();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ticket Manager API", Version = "v1" });

            // Define el esquema de seguridad JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
        return services;
    }
}