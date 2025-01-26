using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherController> _logger;
    private const string ApiKey = "1b1d54a5fb6e222c6528d168cf7a2ff5"; //apikey

    public WeatherController(HttpClient httpClient, ILogger<WeatherController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        if (string.IsNullOrEmpty(city))
        {
            _logger.LogWarning("City name is missing in the request.");
            return BadRequest("City name is required.");
        }

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={ApiKey}&units=metric";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API response: {content}");

                var weatherData = JObject.Parse(content);

                
                if (weatherData["cod"].ToString() != "200")
                {
                    _logger.LogWarning($"City not found: {city}");
                    return NotFound($"City '{city}' not found.");
                }

                
                var weather = new
                {
                    City = weatherData["name"]?.ToString(),
                    Temperature = weatherData["main"]?["temp"]?.ToString(),
                    Humidity = weatherData["main"]?["humidity"]?.ToString(),
                    WeatherDescription = weatherData["weather"]?[0]?["description"]?.ToString()
                };

                _logger.LogInformation($"Weather data retrieved for city: {city}");
                return Ok(weather);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error retrieving weather data: {errorContent}");
                return StatusCode((int)response.StatusCode, $"Error retrieving weather data: {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP request failed: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving weather data. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}