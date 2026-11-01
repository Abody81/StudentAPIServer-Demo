using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyFirstRestAPI_Project.Authorization;
using Scalar.AspNetCore;
using System.Collections.Generic;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"]!;
var jwtIssuer = jwt["Issuer"]!;
var jwtAudience = jwt["Audience"]!;

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,

            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your JWT token}"
        };

        var components = document.Components ??= new OpenApiComponents();
        components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = securityScheme
        };

        var reference = new OpenApiSecuritySchemeReference("Bearer", document, null);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [reference] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("StudentOwnerOrAdmin", policy =>
        policy.Requirements.Add(new StudentOwnerOrAdminRequirement()));
    });

builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnerOrAdminHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
