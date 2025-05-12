/*
This file is for all endpoints for bus routes
For other endpoints, create new controllers
The route for these endpoints are {baseurl}/BusTimes/{endpoint}

Authored by @Marcus-Aastum
*/

using System.Globalization;
using System.Text;
using System.Text.Json;
using info_skjerm_api.Model;
using info_skjerm_api.Model.Bus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace info_skjerm_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusTimesController(IHttpClientFactory httpClientFactory) : ControllerBase
    {

        //This endpoint returns bus departures, with an optional property "num" which defines how many buses to be returned (default 20)
        [HttpGet("departures")]
        [ProducesResponseType(typeof(BusStop), 200)]
        public async Task<IActionResult> GetDeparture(int num = 20)
        {
            //Defines a request object with url and graphql-query
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.entur.io/journey-planner/v3/graphql");
            var query = """{"query": "{ stopPlace( id: \"NSR:StopPlace:44029\" ) { id name estimatedCalls( numberOfDepartures: """ + num.ToString() + """ ) { realtime aimedArrivalTime expectedArrivalTime destinationDisplay { frontText } quay { id } serviceJourney { journeyPattern { line { id name transportMode } } } } }}"}""";
            
            //Sends the request and converts response into a "Businfo" object
            var httpClient = httpClientFactory.CreateClient();
            
            request.Headers.Add("ET-Client-Name", "tillervgs-infoskjerm");
            request.Content = new StringContent(query, Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStreamAsync();
            var jsonResponse = await JsonSerializer.DeserializeAsync<Businfo>(responseContent);

            List<BusRoute> northBound = [];
            List<BusRoute> southBound = [];
            List<BusRoute> all = [];

            //Iterates through every bus departure
            foreach (var estimatedCalls in jsonResponse.data.stopPlace.estimatedCalls)
            {
                //Assigns the departure to a "BusRoute"-object, and adds it to a list of all bus departures
                var busRoute = new BusRoute
                {
                    destination = estimatedCalls.destinationDisplay.frontText,
                    time = estimatedCalls.expectedArrivalTime,
                    isRealTime = estimatedCalls.realtime,
                    busLine = int.Parse(estimatedCalls.serviceJourney.journeyPattern.line.id.Split(":")[2].Split("_")[1])
                };
                all.Add(busRoute);

                switch (estimatedCalls.quay.id)
                {
                    //Puts the object in the northbound or southbound list based on what "quay" it leaves from
                    case "NSR:Quay:75606":
                        northBound.Add(busRoute);
                        break;
                    case "NSR:Quay:75607":
                        southBound.Add(busRoute);
                        break;
                }
            }
            //Creates a "BusStop"-object and returns this
            var busStop = new BusStop
            {
                northBound = northBound,
                southBound = southBound,
                all = all
            };

            return Ok(busStop);
        }
        
        
        //This file is only for endpoints relating to bus routes. 
    }
}