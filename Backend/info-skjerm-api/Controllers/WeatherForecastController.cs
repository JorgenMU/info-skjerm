using System.Net;
using info_skjerm_api.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace info_skjerm_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IHttpClientFactory httpClientFactory) : ControllerBase
    {
        // Henter værvarsel for i nå og de neste 6 timene
        [HttpGet("Today")]
        public async Task<IActionResult> GetWeatherForecastTodayAsync()
       {
            const string apiUrl = "https://api.met.no/weatherapi/locationforecast/2.0/complete?lat=63.21&lon=10.22";

            using var client = httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");

            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode) return Problem("The Yr Api returned an unsuccessful status code");
                
            var content = await response.Content.ReadAsStreamAsync();
            var jsonElements = await JsonSerializer.DeserializeAsync<WeatherForecast>(content, JsonSerializerOptions.Default);
            var timeSeries = jsonElements.properties.timeSeries;
                
            List<TodayWeatherForecast> todayWeatherForecasts = [];

            for (var i = 0; i < 7; i++)
            {
                todayWeatherForecasts.Add(new TodayWeatherForecast
                {
                    airTemperature = timeSeries[i].data.instant.details.airTemperature,
                    symbol_code = timeSeries[i].data.next1Hours.summary.symbolCode,
                    time = timeSeries[i].time
                });
            }
            return Ok(todayWeatherForecasts);
       }

        //Henter værvarsel klokken 12 GMT for de neste dagene i en uke
        [HttpGet("NextDays")]
        public async Task<IActionResult> GetWeatherForecastNextWeekAsync()
        {
            const string apiUrl = "https://api.met.no/weatherapi/locationforecast/2.0/complete?lat=63.21&lon=10.22";

            using var client = httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
            
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode) return Problem("The Yr Api returned an unsuccessful status code");
            
            var content = await response.Content.ReadAsStreamAsync();
            var jsonElements = await JsonSerializer.DeserializeAsync<WeatherForecast>(content, JsonSerializerOptions.Default);
            var timeSeries = jsonElements?.properties.timeSeries;
                
            var currentDate = int.Parse(jsonElements?.properties.timeSeries[0].time.Substring(startIndex:8, length:2) ?? throw new InvalidOperationException());
                
            const int noon = 12;
            var dayIndexer = 1;
                
            List<NextDaysWeatherForecast> forecastNextWeekList = [];
                
            //looper igjennom timseries og legger til de riktige dagene og tidspuktet i listen
            foreach (TimeSeries timeSeriesElement in timeSeries)
            {
                if (dayIndexer == 8)
                {
                    break;
                }
                
                //koverterer og deler opp en streng til nummer man kan behandle
                var nextHours = int.Parse(timeSeriesElement.time.Substring(startIndex: 11, length: 2));
                var nextDate = int.Parse(timeSeriesElement.time.Substring(startIndex: 8, length: 2));
                
                if (nextDate != currentDate + dayIndexer || nextHours != noon) continue;
                
                forecastNextWeekList.Add(new NextDaysWeatherForecast
                {
                    airTemperature = timeSeriesElement.data.instant.details.airTemperature,
                    symbol_code = timeSeriesElement.data.next6Hours.summary.symbolCode,
                    time = timeSeriesElement.time
                });
                dayIndexer++;
            }
            return Ok(forecastNextWeekList);
        }
    }
}