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
    private static JObject jsonRequest; // ͳһ��JSON����ģ�����
    private static string api_key;

    void Start()
    {
        NLUI_Initialized();
    }
    // �����ʼ��
    void NLUI_Initialized()
    {
        api_key = apiKey;
        Memory = new()
        {
            new JObject
            {
                { "role", "system" },
                { "content", "����һ����Ȼ����UI�������������û�����Ȼ�������󣬷���Ӧ�õ��ú����ĺ������Ͷ�Ӧ�������������Ӧ��������Ӧ������Ȼ���Իظ��û��������û�˵ʲô���㶼ֻ���ж��û����������һ�ֺ���������Ȼ����ݺ���������Ӧ��" }
            }
        };
    }
    // �߼�������
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
    // ��һ������Ļص������������û���Ȼ���������ȡҪ���ú������ʹ���Ĳ���
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
            Memory.Add(messageJson); // ��ӵ�һ����Ӧ����Ϣ�б�
            Debug.Log(Memory);
            // Debug.Log((string)messageJson["content"] != "null");

            // JSON Responce Example: 
            //{
            //    "role": "assistant",
            //    "content": null,
            //    "function_call": {
            //        "name": "get_current_weather",
            //        "arguments": "{\n  \"location\": \"�Ͼ�\",\n  \"unit\": \"celsius\"\n}"
            //    }
            //}
            if ((string)messageJson["content"] == null)
            {
                string functionName = (string)messageJson["function_call"]["name"]; // Ҫ���õĺ�����
                string argumentsJson = (string)messageJson["function_call"]["arguments"];
                JObject argumentsObject = JObject.Parse(argumentsJson);
                string[] parameters = argumentsObject.Values().Select(v => v.ToString()).ToArray(); // Ҫ����Ĳ���
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
    // �ڶ�������Ļص�����������֮ǰ����Ϣ�Լ��������ؽ����ȡ�������ؽ������Ȼ��������
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
            Memory.Add(messageJson); // ��ӵ�һ����Ӧ����Ϣ�б�
            Debug.Log(Memory);
            contentText.text = (string)messageJson["content"];
        }
    }
    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="query">Ҫ���͵�JSON��ʽ����</param>
    /// <param name="OnResponse">���յ���������Ӧ��Ҫ���õĴ�����</param>
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
    /// ��ִ̬�к����ĺ���
    /// </summary>
    /// <param name="functionName">Ҫִ�еĺ�����</param>
    /// <param name="parameters">Ҫִ�к����ĺ����������޶�Ϊstring���ͣ�</param>
    /// <returns></returns>
    private static object CallFunction(string functionName, string[] parameters)
    {
        // ��ȡ������MethodInfo����
        var methodInfo = typeof(Functions).GetMethod(functionName);

        // ����һ��ʵ��������Ǿ�̬������Ϊnull��
        Functions instance = new();

        // ����������ת��Ϊobject����
        object[] convertedParams = Array.ConvertAll(parameters, p => (object)p);

        // ���ú�������ȡ����ֵ
        object result = methodInfo.Invoke(instance, convertedParams);

        return result;
    }
}