using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using OnlineTesting.API.Localization;
using OnlineTesting.API.Middleware;
using OnlineTesting.API.Services;
using OnlineTesting.Application;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Infrastructure;
using OnlineTesting.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();
builder.Services.AddScoped<ILanguageContext, LanguageContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Roles.Policies.OwnerAccess, p =>
        p.RequireRole(Roles.Owner));
    options.AddPolicy(Roles.Policies.PlatformAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin));
    options.AddPolicy(Roles.Policies.OrgAdminAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.OrgAdmin));
    options.AddPolicy(Roles.Policies.CrmAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.OrgAdmin, Roles.Staff));
    options.AddPolicy(Roles.Policies.GroupsAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.OrgAdmin, Roles.Staff, Roles.Teacher));

    // Defense-in-depth: any endpoint without an explicit [Authorize]/[AllowAnonymous]
    // requires authentication by default, instead of silently becoming public.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login/Register: 10 attempts per minute per IP
    options.AddPolicy("auth-strict", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    // Refresh/Telegram/Google: 20 attempts per minute per IP
    options.AddPolicy("auth-normal", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
});

builder.Services.AddResponseCaching();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите JWT access token (без префикса 'Bearer ')."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.UseMiddleware<LanguageMiddleware>();
app.MapControllers();

app.Run();