using Newtonsoft.Json.Linq;
using UnityEngine;

public class Functions : MonoBehaviour
{
    // public GameObject cubePrefab; // 可以在Inspector视图中拖拽一个立方体预制体到这个字段
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
    public JObject create_cube(string number)
    {
        int num = int.Parse(number);
        Debug.Log(num);
        GameObject prefab = GameObject.Find("Cube");
        prefab.tag = "Temp";
        for (int i = 0; i < num; i++)
        {
            // 在位置(0, 0, 0)处创建一个立方体
            _ = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
        return new JObject { { "result", "立方体已经创建" } };
    }
    public JObject create_sphere(string number)
    {
        int num = int.Parse(number);
        Debug.Log(num);
        GameObject prefab = GameObject.Find("Sphere");
        prefab.tag = "Temp";
        for (int i = 0; i < num; i++)
        {
            // 在位置(0, 0, 0)处创建一个球体
            _ = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
        return new JObject { { "result", "球体已经创建" } };
    }
    public JObject clear_objects()
    {
        GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("Temp");

        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }
        return new JObject { { "result", "所有对象已被清除" } };
    }
}
