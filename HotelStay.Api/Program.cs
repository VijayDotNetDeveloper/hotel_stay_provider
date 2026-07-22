using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:5042");

        builder.Services.AddSingleton<DataService>();
        builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                    .AllowAnyHeader()
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

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
