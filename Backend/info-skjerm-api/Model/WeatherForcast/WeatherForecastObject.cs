namespace info_skjerm_api.Model

//Objekt oppsett basert p� yr apiet
{
    public record WeatherForecast
    {
        public required Properties properties { get; init;  }
    }

    public abstract record Properties
    {
        public required List<TimeSeries> timeSeries { get; set; }
    }

    public abstract record TimeSeries
    {
        public required string time { get; set; }
        public required Data data { get; set; }
    }

    public abstract record Data
    {
        public required Instant instant { get; set; }

        public required Next1Hours next1Hours { get; set; }
        
        public required Next6Hours next6Hours { get; set; }
    }

    public abstract record Instant
    {
        public required Details details { get; set; }
    }

    public abstract record Details
    {
        public required float airTemperature { get; set; }
    }

    public abstract record Next1Hours
    {
       public required Summary summary { get; set; }
    }

    public abstract record Next6Hours
    {
        public required Summary summary { get; set; }
    }

    public abstract record Summary
    {
        public required string symbolCode { get; set; }
    }
}