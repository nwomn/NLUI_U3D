using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine.UI;

public class NLUI : MonoBehaviour
{
    [SerializeField] private Text contentText;
    // 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
    readonly string apiKey = "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";
    public static JArray Memory { get; set; }
    public string user_query;

    private static UnityWebRequest request;
    private static JObject jsonRequest; // 统一的JSON请求模板对象
    private static string api_key;

    void Start()
    {
        NLUI_Initialized();
    }
    // 本体初始化
    void NLUI_Initialized()
    {
        api_key = apiKey;
        Memory = new()
        {
            new JObject
            {
                { "role", "system" },
                { "content", "你是一个自然语言UI，接下来接收用户的自然语言请求，返回应该调用函数的函数名和对应参数，最后结合相应函数的响应，用自然语言回复用户。无论用户说什么，你都只能判断用户输入符合哪一种函数描述，然后根据函数返回响应。" }
            }
        };
    }
    // 逻辑链启动
    public void Run()
    {
        JObject jsonQuery = new JObject
        {
            { "role", "user" },
            { "content", user_query }
        };
        Memory.Add(jsonQuery);

        string filePath = Path.Combine(Application.dataPath, "Func_Desc\\Func_Desc.json");
        string jsonString = File.ReadAllText(filePath);
        JArray functionArray = JArray.Parse(jsonString);

        jsonRequest = new JObject
        {
            { "model", "gpt-3.5-turbo-0613" },
            { "messages", Memory},
            { "functions", functionArray }
        };
        ChatCompletion(jsonRequest, FirstFunctionResponseCallBack);
    }
    // 第一次请求的回调函数：根据用户自然语言请求获取要调用函数名和传入的参数
    private void FirstFunctionResponseCallBack(AsyncOperation asyncOperation)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Request failed: " + request.error);
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            JObject responseObject = JObject.Parse(responseText);
            string message = responseObject["choices"][0]["message"].ToString();
            JObject messageJson = JObject.Parse(message);
            Memory.Add(messageJson); // 添加第一次响应到信息列表
            Debug.Log(Memory);
            // Debug.Log((string)messageJson["content"] != "null");

            // JSON Responce Example: 
            //{
            //    "role": "assistant",
            //    "content": null,
            //    "function_call": {
            //        "name": "get_current_weather",
            //        "arguments": "{\n  \"location\": \"南京\",\n  \"unit\": \"celsius\"\n}"
            //    }
            //}
            if ((string)messageJson["content"] == null)
            {
                string functionName = (string)messageJson["function_call"]["name"]; // 要调用的函数名
                string argumentsJson = (string)messageJson["function_call"]["arguments"];
                JObject argumentsObject = JObject.Parse(argumentsJson);
                string[] parameters = argumentsObject.Values().Select(v => v.ToString()).ToArray(); // 要传入的参数
                JObject result = (JObject)CallFunction(functionName, parameters);
                Debug.Log(result);
                Memory.Add(new JObject
                {
                    { "role", "function" },
                    { "name", functionName },
                    { "content", result.ToString() }
                });
                ChatCompletion(jsonRequest, SecondFunctionResponseCallBack);
            } else
            {
                string content = (string)messageJson["content"];
                Debug.Log(content);
                contentText.text = content;
            }
        }
    }
    // 第二次请求的回调函数：根据之前的信息以及函数返回结果获取函数返回结果的自然语言描述
    private void SecondFunctionResponseCallBack(AsyncOperation asyncOperation)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Request failed: " + request.error);
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            JObject responseObject = JObject.Parse(responseText);
            string message = responseObject["choices"][0]["message"].ToString();
            JObject messageJson = JObject.Parse(message);
            Memory.Add(messageJson); // 添加第一次响应到信息列表
            Debug.Log(Memory);
            contentText.text = (string)messageJson["content"];
        }
    }
    /// <summary>
    /// 请求函数本体
    /// </summary>
    /// <param name="query">要发送的JSON格式请求</param>
    /// <param name="OnResponse">接收到服务器响应后要调用的处理函数</param>
    private static void ChatCompletion(JObject query, Action<AsyncOperation> OnResponse)
    {
        Debug.Log(query);
        var sendData = JsonConvert.SerializeObject(query);

        request = new();
        request = UnityWebRequest.PostWwwForm("https://api.openai.com/v1/chat/completions", sendData);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + api_key);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(sendData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        var handle = request.SendWebRequest();
        handle.completed += OnResponse;
    }
    /// <summary>
    /// 动态执行函数的函数
    /// </summary>
    /// <param name="functionName">要执行的函数名</param>
    /// <param name="parameters">要执行函数的函数参数（限定为string类型）</param>
    /// <returns></returns>
    private static object CallFunction(string functionName, string[] parameters)
    {
        // 获取函数的MethodInfo对象
        var methodInfo = typeof(Functions).GetMethod(functionName);

        // 创建一个实例（如果是静态方法则为null）
        Functions instance = new();

        // 将参数数组转换为object数组
        object[] convertedParams = Array.ConvertAll(parameters, p => (object)p);

        // 调用函数并获取返回值
        object result = methodInfo.Invoke(instance, convertedParams);

        return result;
    }
}