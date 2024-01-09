using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using Newtonsoft.Json;
using BingMapsRESTToolkit;
using System.Security.Cryptography;

namespace DiscordBotCSharp
{
    public class DiscordBot
    {

        public string msgZip = string.Empty;
        private readonly DiscordSocketClient _client;
        static Task Main(string[] args)
        {
            return new DiscordBot().MainAsync(); /*.GetAwaiter().GetResult(); */

        }

        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private DiscordBot()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);

            _client.Ready += ReadyAsync;
            //_client.MessageReceived += MessageReceivedAsync;
            //_client.InteractionCreated += InteractionCreatedAsync;

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,

                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _client.Log += LogAsync;
            //_services = ConfigureServices();
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;

            if (msg == null) return;

            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

            int pos = 0;

            if (msg.HasCharPrefix('!', ref pos))
            {
                var context = new SocketCommandContext(_client, msg);

                var result = await _commands.ExecuteAsync(context, pos, _services);
                string[] msgBingZip = msg.Content.Split(' ');
                Console.WriteLine($" poppycock {msgBingZip[1]}");
                var msgZip = msgBingZip.ToString();
                //string msgZipString = msgBingZip[1];
                msgZip = msgBingZip[1];

                Console.WriteLine("pre msg");
                await context.Channel.SendMessageAsync("TEST TEST TESSST");

                //Console.WriteLine($" this is msg var {msgZip}");
                Console.WriteLine("TEST!!!!");

                //var bingKey = $"ArbfN_bF_xSvHbSSuKusfDNW1lK76l_y6yU_Zjbln7pYFx95UFKT8rmT6mrZw0Jq";
                var client = new HttpClient();
                //int postalCodeBing = Convert.ToInt32(Console.ReadLine());
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://dev.virtualearth.net/REST/v1/Locations?&postalCode={msgZip}&key=ArbfN_bF_xSvHbSSuKusfDNW1lK76l_y6yU_Zjbln7pYFx95UFKT8rmT6mrZw0Jq");
                var response = await client.SendAsync(request);
                //response.EnsureSuccessStatusCode();
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                //Console.WriteLine(msgZip);
                var content = response.Content.ReadAsStringAsync();
                var objBing = JsonConvert.DeserializeObject<Rootobject>(content.Result);
                var latLong = new List<string>();
                foreach (var item in objBing.resourceSets)
                {
                    foreach (var item2 in item.resources)
                    {
                        Console.WriteLine("TEST 1111");
                        Console.WriteLine(item.resources);

                        foreach (var item3 in item2.geocodePoints)
                        {
                            foreach (var coordinate in item3.coordinates)
                            {
                                Console.WriteLine("TEST 2222");
                                Console.WriteLine("coordinate TEST");
                                Console.WriteLine(coordinate);
                                latLong.Add(coordinate.ToString());
                                Console.WriteLine(latLong);
                                Console.WriteLine(string.Join(", ", latLong));
                                //latLong.ForEach(x => Console.WriteLine(x));
                                //Console.WriteLine(latLong[0]);
                            }
                        }
                    }
                }

                var clientPw = new HttpClient();
                var requestPw = new HttpRequestMessage(HttpMethod.Get, $"https://api.pirateweather.net/forecast/M6mbdMFwOBGYSoBF/{latLong[0]},{latLong[1]}");
                request.Headers.Add("apikey", "M6mbdMFwOBGYSoBF");
                var responsePw = await clientPw.SendAsync(requestPw);
                responsePw.EnsureSuccessStatusCode();
                //Console.WriteLine(await responsePw.Content.ReadAsStringAsync());

                var contentPw = responsePw.Content.ReadAsStringAsync();
                var objPw = JsonConvert.DeserializeObject<PirateWeather>(contentPw.Result);
                Console.WriteLine(objPw.currently.temperature);
                var curTemp = objPw.currently.temperature;
                var weatherSum = objPw.currently.summary;
                //var icon =
                var realFeel = objPw.currently.apparentTemperature;
                var precipProb = objPw.currently.precipProbability;
                var hum = objPw.currently.humidity;
                var windSpeed = objPw.currently.windSpeed;
                var units = objPw.flags.units;

                Console.WriteLine("TEST3333");
                await context.Channel.SendMessageAsync($"Summary: {weatherSum}\nCurrent Temperature: {curTemp}\n" +
                    $"Real Feel: {realFeel}\nPrecipitation Probability: {precipProb}\nHumidity: {hum}\n" +
                    $"Wind Speed: {windSpeed}\nUnits: {units}");
            }

            
        }

        public async Task MainAsync()
        {
            var botToken = "MTE3NDc5ODgzNDU5ODE2NjU1OQ.GVb_yf.v832NSKLRfauUDHRBCpJXf4OOY_e07CBM-BkBQ";
            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

    }
}

public class Rootobject
{
    public string authenticationResultCode { get; set; }
    public string brandLogoUri { get; set; }
    public string copyright { get; set; }
    public Resourceset[] resourceSets { get; set; }
    public int statusCode { get; set; }
    public string statusDescription { get; set; }
    public string traceId { get; set; }
}

public class Resourceset
{
    public int estimatedTotal { get; set; }
    public Resource[] resources { get; set; }
}

public class Resource
{
    public string __type { get; set; }
    public float[] bbox { get; set; }
    public string name { get; set; }
    public Point point { get; set; }
    public Address address { get; set; }
    public string confidence { get; set; }
    public string entityType { get; set; }
    public Geocodepoint[] geocodePoints { get; set; }
    public string[] matchCodes { get; set; }
}

public class Point
{
    public string type { get; set; }
    public float[] coordinates { get; set; }
}

public class Address
{
    public string adminDistrict { get; set; }
    public string adminDistrict2 { get; set; }
    public string countryRegion { get; set; }
    public string formattedAddress { get; set; }
    public string locality { get; set; }
    public string postalCode { get; set; }
}

public class Geocodepoint
{
    public string type { get; set; }
    public float[] coordinates { get; set; }
    public string calculationMethod { get; set; }
    public string[] usageTypes { get; set; }
}



public class PirateWeather
{
    public float latitude { get; set; }
    public float longitude { get; set; }
    public string timezone { get; set; }
    public float offset { get; set; }
    public int elevation { get; set; }
    public Currently currently { get; set; }
    public Minutely minutely { get; set; }
    public Hourly hourly { get; set; }
    public Daily daily { get; set; }
    public object[] alerts { get; set; }
    public Flags flags { get; set; }
}

public class Currently
{
    public int time { get; set; }
    public string summary { get; set; }
    public string icon { get; set; }
    public int nearestStormDistance { get; set; }
    public int nearestStormBearing { get; set; }
    public float precipIntensity { get; set; }
    public float precipProbability { get; set; }
    public float precipIntensityError { get; set; }
    public string precipType { get; set; }
    public float temperature { get; set; }
    public float apparentTemperature { get; set; }
    public float dewPoint { get; set; }
    public float humidity { get; set; }
    public float pressure { get; set; }
    public float windSpeed { get; set; }
    public float windGust { get; set; }
    public int windBearing { get; set; }
    public float cloudCover { get; set; }
    public float uvIndex { get; set; }
    public float visibility { get; set; }
    public float ozone { get; set; }
}

public class Minutely
{
    public string summary { get; set; }
    public string icon { get; set; }
    public Datum[] data { get; set; }
}

public class Datum
{
    public int time { get; set; }
    public float precipIntensity { get; set; }
    public float precipProbability { get; set; }
    public float precipIntensityError { get; set; }
    public string precipType { get; set; }
}

public class Hourly
{
    public string summary { get; set; }
    public string icon { get; set; }
    public Datum1[] data { get; set; }
}

public class Datum1
{
    public int time { get; set; }
    public string icon { get; set; }
    public string summary { get; set; }
    public float precipIntensity { get; set; }
    public float precipProbability { get; set; }
    public float precipIntensityError { get; set; }
    public float precipAccumulation { get; set; }
    public string precipType { get; set; }
    public float temperature { get; set; }
    public float apparentTemperature { get; set; }
    public float dewPoint { get; set; }
    public float humidity { get; set; }
    public float pressure { get; set; }
    public float windSpeed { get; set; }
    public float windGust { get; set; }
    public int windBearing { get; set; }
    public float cloudCover { get; set; }
    public float uvIndex { get; set; }
    public float visibility { get; set; }
    public float ozone { get; set; }
}

public class Daily
{
    public string summary { get; set; }
    public string icon { get; set; }
    public Datum2[] data { get; set; }
}

public class Datum2
{
    public int time { get; set; }
    public string icon { get; set; }
    public string summary { get; set; }
    public int sunriseTime { get; set; }
    public int sunsetTime { get; set; }
    public float moonPhase { get; set; }
    public float precipIntensity { get; set; }
    public float precipIntensityMax { get; set; }
    public int precipIntensityMaxTime { get; set; }
    public float precipProbability { get; set; }
    public float precipAccumulation { get; set; }
    public string precipType { get; set; }
    public float temperatureHigh { get; set; }
    public int temperatureHighTime { get; set; }
    public float temperatureLow { get; set; }
    public int temperatureLowTime { get; set; }
    public float apparentTemperatureHigh { get; set; }
    public int apparentTemperatureHighTime { get; set; }
    public float apparentTemperatureLow { get; set; }
    public int apparentTemperatureLowTime { get; set; }
    public float dewPoint { get; set; }
    public float humidity { get; set; }
    public float pressure { get; set; }
    public float windSpeed { get; set; }
    public float windGust { get; set; }
    public int windGustTime { get; set; }
    public int windBearing { get; set; }
    public float cloudCover { get; set; }
    public float uvIndex { get; set; }
    public int uvIndexTime { get; set; }
    public float visibility { get; set; }
    public float temperatureMin { get; set; }
    public int temperatureMinTime { get; set; }
    public float temperatureMax { get; set; }
    public int temperatureMaxTime { get; set; }
    public float apparentTemperatureMin { get; set; }
    public int apparentTemperatureMinTime { get; set; }
    public float apparentTemperatureMax { get; set; }
    public int apparentTemperatureMaxTime { get; set; }
}

public class Flags
{
    public string[] sources { get; set; }
    public Sourcetimes sourceTimes { get; set; }
    public int neareststation { get; set; }
    public string units { get; set; }
    public string version { get; set; }
}

public class Sourcetimes
{
    public string hrrr_018 { get; set; }
    public string hrrr_subh { get; set; }
    public string hrrr_1848 { get; set; }
    public string gfs { get; set; }
    public string gefs { get; set; }
}


