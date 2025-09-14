using System.Net.Http.Headers;
using AgileActors.Application.Services;
using AgileActors.Application.Services.Providers;
using AgileActors.Core.External;
using AgileActors.Core.Stats;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IApiStatsStore, InMemoryApiStatsStore>();
builder.Services.AddScoped<IAggregationService, AggregationService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "a148e95a6672882b6ad61cddaeb6e859";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AgileActorsApi";



builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddHttpClient<NewsProvider>(c =>
{
    c.BaseAddress = new Uri("https://newsapi.org/");
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "news"));

builder.Services.AddHttpClient<WeatherProvider>(c =>
{
    c.BaseAddress = new Uri("https://api.openweathermap.org/");
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "weather"));

builder.Services.AddHttpClient<SpotifyProvider>(c =>
{
    c.BaseAddress = new Uri("https://api.spotify.com/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "spotify"));

// Register providers as IExternalProvider
builder.Services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<NewsProvider>());
builder.Services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<WeatherProvider>());
builder.Services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<SpotifyProvider>());


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddOpenApi();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

var app = builder.Build();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Agile Actors API");
        options.WithTheme(ScalarTheme.BluePlanet);
        options.WithSidebar(true);
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = "Bearer"
        };

    });
}

//app.UseSwagger();
//app.UseSwaggerUI();

app.MapControllers();

app.Run();

//Just for testing with Scalar 
internal sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer

{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            document.Components ??= new OpenApiComponents();

            var securitySchemeId = "Bearer";

            document.Components.SecuritySchemes.Add(securitySchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            });

            // Add "Bearer" scheme as a requirement for the API as a whole
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = securitySchemeId, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
            });
        }
    }
}
