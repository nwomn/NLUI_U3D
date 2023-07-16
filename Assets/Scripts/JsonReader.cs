using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEditor.Search;
using UnityEngine;

public class JsonReader : MonoBehaviour
{
    void Start()
    {
        JArray messages = new() { new JObject { { "role", "user" }, { "content", "This is a test!" } }, };
        JObject json = new()
        {
            { "model", "gpt-3.5-turbo-0613" },
            { "messages", messages},
        };
        Debug.Log(json);
        messages.Add(new JObject { { "role", "system" }, { "content", "It indeed is a test. " } });
        Debug.Log(json);
    }
}
