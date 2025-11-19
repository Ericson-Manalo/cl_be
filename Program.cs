
using cl_be.Models;
using cl_be.Models.Services;
using Microsoft.EntityFrameworkCore;
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


            //Servizio per connettersi al db AdventureWorksLt2019
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AdventureWorksLT2019")
                    ?? throw new InvalidOperationException("Connessione non avvenuta"));
            });


            //Servizio per connettersi al db ClcredsDb

            builder.Services.AddDbContext<ClcredsDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CLCredsDb")
                    ?? throw new InvalidOperationException("Connessione non avvenuta"));
            });


            //Servizio per connettersi al DB ReviewMDB MONGODB
            builder.Services.Configure<ReviewMDBConfig>(
                builder.Configuration.GetSection("ReviewMDB"));

            builder.Services.AddSingleton<ReviewService>();

            var app = builder.Build(); 

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
