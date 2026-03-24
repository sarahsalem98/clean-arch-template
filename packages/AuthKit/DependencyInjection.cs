using System.Text;
using AuthKit.Options;
using AuthKit.Providers;
using CleanArchTemplate.Application.Common.Behaviours;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Infrastructure.Services;
using CleanArchTemplate.WebAPI.Authorization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthKit;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all AuthKit services: JWT, password hashing, current user, social providers,
    /// and authorization handlers. Call AddAuthKitEntities() on your ModelBuilder separately.
    /// </summary>
    public static IServiceCollection AddAuthKit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthKitOptions>? configure = null)
    {
        var options = new AuthKitOptions();
        configure?.Invoke(options);

        // MediatR — registers all command handlers bundled in this package
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        });

        // FluentValidation — registers all validators bundled in this package
        services.AddValidatorsFromAssembly(assembly);

        // Core auth services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Authorization handlers
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationHandler, PortalHandler>();

        // HttpClient (used by social providers)
        services.AddHttpClient("AuthKit.Facebook");

        // Social providers — registered only when credentials are provided
        if (options.IsGoogleEnabled)
            services.AddScoped<ISocialAuthProvider>(
                _ => new GoogleAuthProvider(new GoogleOptions { ClientId = options.GoogleClientId }));

        if (options.IsAppleEnabled)
            services.AddScoped<ISocialAuthProvider>(
                _ => new AppleAuthProvider(options.Apple));

        if (options.IsFacebookEnabled)
            services.AddScoped<ISocialAuthProvider>(sp =>
                new FacebookAuthProvider(options.Facebook, sp.GetRequiredService<IHttpClientFactory>()));

        // JWT authentication
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("AuthKit: 'Jwt:Secret' is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
