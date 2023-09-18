using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine.UI;
using OpenAI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class NLUI : MonoBehaviour
{
    [SerializeField] private Text contentText;
    readonly string[] apiKey = {
        "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",
        "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"
    };
    public static JArray Memory;
    public string user_query;

    private static UnityWebRequest request;
    private static JObject jsonRequest; // 统一的JSON请求模板对象
    private static string api_key;
    private System.Random random = new System.Random();

    void Start()
    {
        NLUI_Initialized();
    }
    // 本体初始化
    void NLUI_Initialized()
    {
        api_key = apiKey[random.Next(apiKey.Length)];
        Memory = new()
        {
            new JObject
            {
                { "role", "system" },
                { "content", "你是一个自然语言UI，接下来接收用户的自然语言请求，返回应该调用函数的函数名和对应参数，最后结合相应函数的响应，用自然语言回复用户。你的回复应当满足以下条件：" +
                "1. 注意回复最多不能超过100字。" +
                "2. 对于不符合任一函数描述的请求，应当向用户反问以确认用户的需求。" }
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
        contentText.text = "Answering Question...";
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
            Debug.Log(messageJson["function_call"]);

            // JSON Responce Example: 
            //{
            //    "role": "assistant",
            //    "content": null,
            //    "function_call": {
            //        "name": "get_current_weather",
            //        "arguments": "{\n  \"location\": \"南京\",\n  \"unit\": \"celsius\"\n}"
            //    }
            //}
            if (messageJson["function_call"] != null)
            {
                string functionName = (string)messageJson["function_call"]["name"]; // 要调用的函数名
                string argumentsJson = (string)messageJson["function_call"]["arguments"];
                JObject argumentsObject = JObject.Parse(argumentsJson);
                string[] parameters = argumentsObject.Values().Select(v => v.ToString()).ToArray(); // 要传入的参数
                JObject result = (JObject)CallFunction(functionName, parameters);
                Debug.Log(result);
                api_key = apiKey[random.Next(apiKey.Length)];
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
                GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text = content;
                string per = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Per;
                Debug.Log(per);
                Action<string, string> setTextToSpeechValues = (fileName, per) =>
                {
                    TextToSpeech tts = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>();
                    tts.fileName = fileName;
                    tts.Per = per;
                    tts.BttnTtSClick();
                };

                // 调用Lambda表达式设置不同的值
                setTextToSpeechValues("DuXiaoYu", "1");
                setTextToSpeechValues("DuXiaoMei", "0");
                setTextToSpeechValues("DuYaYa", "4");
                setTextToSpeechValues("DuXiaoYao", "5003");
                setTextToSpeechValues("DuXiaoLu", "5118");
                setTextToSpeechValues("DuBoWen", "106");
                setTextToSpeechValues("DuXiaoTong", "110");
                setTextToSpeechValues("DuXiaoMeng", "111");
                setTextToSpeechValues("DuMiDuo", "103");
                setTextToSpeechValues("DuXiaoJiao", "5");
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
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text = (string)messageJson["content"];
            string per = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Per;
            Debug.Log(per);
            Action<string, string> setTextToSpeechValues = (fileName, per) =>
            {
                TextToSpeech tts = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>();
                tts.fileName = fileName;
                tts.Per = per;
                tts.BttnTtSClick();
                Task.Delay(TimeSpan.FromSeconds(1)).Wait(); // 延时1秒
            };

            // 调用Lambda表达式设置不同的值
            setTextToSpeechValues("DuXiaoYu", "1");
            setTextToSpeechValues("DuXiaoMei", "0");
            setTextToSpeechValues("DuYaYa", "4");
            setTextToSpeechValues("DuXiaoYao", "5003");
            setTextToSpeechValues("DuXiaoLu", "5118");
            setTextToSpeechValues("DuBoWen", "106");
            setTextToSpeechValues("DuXiaoTong", "110");
            setTextToSpeechValues("DuXiaoMeng", "111");
            setTextToSpeechValues("DuMiDuo", "103");
            setTextToSpeechValues("DuXiaoJiao", "5");
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
        request = UnityWebRequest.Post("https://api.openai.com/v1/chat/completions", sendData);
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