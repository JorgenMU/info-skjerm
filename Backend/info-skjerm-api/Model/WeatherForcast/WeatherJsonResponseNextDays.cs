namespace info_skjerm_api.Model;

public class WeatherJsonResponseNextDays
{
    public List<NextDaysWeatherForecast> nextDaysForcastList { get; set; } = new();
}

public class NextDaysWeatherForecast
{
    public float airTemperature { get; set; }

    public string symbol_code { get; set; }

    public string time { get; set; }
}