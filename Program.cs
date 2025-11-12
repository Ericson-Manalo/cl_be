
using cl_be.Models;
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

            //cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontEnd", builder =>
                {
                    builder
                        //.WithOrigins("http://127.0.0.1:5500") 
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        ;
                });
            });


            //Servizio per connettersi al db
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AdventureWorksLT2019")
                    ?? throw new InvalidOperationException("Connessione non avvenuta"));
            });

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
