using Newtonsoft.Json.Linq;
using UnityEngine;

public class Functions : MonoBehaviour
{
    public JObject get_current_weather(string location, string unit)
    {
        JObject weather_info = new JObject { 
            { "location", location }, 
            { "temperature", "72" },
            { "unit", unit },
            { "forecast", new JArray{ "sunny", "windy" } }
        };
        return weather_info;
    }
    public JObject get_bus_info(string location)
    {
        JObject bus_info = new JObject {
            { "location", location },
            { "310", "32" },
            { "139", "10" },
            { "50", "15" },
            { "unit", "minite" }
        };
        return bus_info;
    }
    public JObject other_situations()
    {
        // For handling the situation that the user query is not fitting in any functions
        return new JObject();
    }
}
