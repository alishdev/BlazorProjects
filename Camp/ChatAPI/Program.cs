namespace ChatAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Define a specific CORS policy
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    // IMPORTANT: For production, replace "*" with your actual WordPress domain
                    // For example: .WithOrigins("https://www.your-wordpress-site.com")
                    policy.WithOrigins("*") 
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
        
        // Add services to the container.
        builder.Services.AddControllers();

        // Add Swagger/OpenAPI for testing
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        // Enable the CORS policy
        app.UseCors(MyAllowSpecificOrigins);
        
        app.UseAuthorization();
        
        app.MapControllers();

        /*var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        {
                            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            TemperatureC = Random.Shared.Next(-20, 55),
                            Summary = summaries[Random.Shared.Next(summaries.Length)]
                        })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");*/
        app.Run();
    }
}