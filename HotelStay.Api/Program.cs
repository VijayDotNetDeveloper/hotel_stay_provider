using System.Text.Json.Serialization;
using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var serverUrl = builder.Configuration.GetValue<string>("Api:Urls");
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            builder.WebHost.UseUrls(serverUrl);
        }

        builder.Services.AddSingleton<DataService>();
        builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        builder.Services.AddSingleton<IHotelSearchService, HotelSearchService>();
        builder.Services.AddSingleton<IReservationService, ReservationService>();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();
        app.UseCors("FrontendPolicy");

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
