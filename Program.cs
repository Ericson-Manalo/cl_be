
using cl_be.Models;
using cl_be.Models.Auth;
using cl_be.Models.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace cl_be
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Ignorare serializzatore e loop infinito di annidamenti
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            //autorizzazione cors 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontEnd", builder =>
                {
                    builder
                        //.WithOrigins("http://127.0.0.1:5500") 
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // Servizio per validare il Jwt generato
            JwtSettings jwtSettings = new JwtSettings();
            jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.SecretKey!);
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        ClockSkew = TimeSpan.FromSeconds(3),
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            // In Program.cs
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AdventureWorksLT2019"),
                    sqlOptions => sqlOptions.UseCompatibilityLevel(110) // <--- AGGIUNGI QUESTA RIGA
                ));


            builder.Services.AddAuthorization(opt =>
            {
                opt.AddPolicy("AdminPolicy", policy =>
                {
                    policy.RequireRole("Admin");
                });

                opt.AddPolicy("UserPolicy", policy =>
                {
                    policy.RequireRole("Admin", "User");
                });
            });

            ////Servizio per connettersi al db AdventureWorksLt2019
            //builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
            //{
            //    options.UseSqlServer(
            //        builder.Configuration.GetConnectionString("AdventureWorksLT2019")

            //        ?? throw new InvalidOperationException("Connessione non avvenuta"))
            //    ;
            //});


            //Servizio per connettersi al db ClcredsDb

            builder.Services.AddDbContext<ClcredsDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CLCredsDb")
                    ?? throw new InvalidOperationException("Connessione non avvenuta"));
            });

            //Servizio per connettersi al DB ReviewMDB MONGODB
            // Ottieni la sezione dal configuration
            var mongoSection = builder.Configuration.GetSection("ReviewsDB");

            // Controllo compatto, lancia eccezione se la sezione manca
            var mongoConfig = mongoSection.Get<ReviewMDBConfig>()
                ?? throw new InvalidOperationException("La sezione ReviewsDB è mancante o non valida in appsettings.json");

            // Controlla che tutti i valori fondamentali siano presenti
            if (string.IsNullOrEmpty(mongoConfig.ConnectionString) ||
                string.IsNullOrEmpty(mongoConfig.DatabaseName) ||
                string.IsNullOrEmpty(mongoConfig.ReviewsCollectionName))
            {
                throw new InvalidOperationException("La configurazione di MongoDB è incompleta. Controlla ReviewsDB in appsettings.json");
            }

            // Registra la configurazione e il servizio
            builder.Services.Configure<ReviewMDBConfig>(mongoSection);
            builder.Services.AddSingleton<ReviewService>();

            var app = builder.Build(); 

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();



            // Inserisci qui il middleware CORS
            app.UseCors("AllowFrontEnd");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
