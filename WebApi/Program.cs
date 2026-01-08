using System.Globalization;
using System.Text;
using BusinessObject;
using BusinessObject.Seeds;
using DataAccess.Repositories;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Loggers;
using WebApi.Middlewares;
using WebApi.Services;
using WebApi.Services.Implementations;
using WebApi.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Create Authorize box on Swagger
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme,
        },
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } }
    );
});

builder.Services.AddControllers().AddDataAnnotationsLocalization().AddViewLocalization();

builder.Services.AddDbContext<MyBlogContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging()
);

builder.Services.Configure<BaseSettings>(builder.Configuration.GetSection("BaseSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("BaseSettings:EmailSettings")
);
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("BaseSettings:JwtSettings")
);
builder.Services.Configure<UploadSettings>(
    builder.Configuration.GetSection("BaseSettings:UploadSettings")
);

var jwtSettings = builder.Configuration.GetSection("BaseSettings:JwtSettings");
var frontendUrl = builder.Configuration["BaseSettings:FrontendUrl"]!;

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            ),
            //ClockSkew = TimeSpan.Zero, // remove delay of token when expire
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Priority read token from Cookie
                if (context.Request.Cookies.ContainsKey("accessToken"))
                {
                    context.Token = context.Request.Cookies["accessToken"];
                }
                // Fallback: read from Authorization header (for testing with Postman, Swagger,...)
                else if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring(7);
                    }
                }

                return Task.CompletedTask;
            },
            // 401 - Unauthorized
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var response = new ApiResponse(401, "Unauthorized");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(response);
            },

            // 403  - Forbidden
            OnForbidden = async context =>
            {
                var response = new ApiResponse(403, "Forbidden");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(response);
            },
        };
    });
builder.Services.AddLocalization(options => options.ResourcesPath = "");

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("vi"),
    new CultureInfo("ja"),
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Hosts
builder.Services.AddHostedService<AccountCleanupHelper>();

// Loggers
builder.Services.AddScoped<ApiLogger>();
builder.Services.AddScoped<TimerLogger>();
builder.Services.AddSingleton<MyBlogLogger>();

// Helpers
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<EmailHelper>();

// builder.Services.AddScoped<CloudinaryHelper>();
builder.Services.AddScoped<UploadHelper>();

// Repositories
// builder.Services.AddScoped<IAccountRepository, AccountRepository>();
// builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// builder.Services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
// builder.Services.AddScoped<IPictureRepository, PictureRepository>();
// builder.Services.AddScoped<IPostRepository, PostRepository>();
// builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUploadService, UploadService>();

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(
//         "AllowAll",
//         builder =>
//         {
//             builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
//         }
//     );
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "_myAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

var app = builder.Build();

// Middlewares
// app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<ApiLoggingMiddleware>();

app.UseRequestLocalization(
    app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value
);

// Comment this line to prevent automatic database migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyBlogContext>();
    Seeder.Seed(db);
}

app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    }
);
app.UseRouting();

app.UseCors("_myAllowSpecificOrigins");

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
// {
app.UseSwagger();
app.UseSwaggerUI();

// }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://0.0.0.0:8080");

app.Run();
