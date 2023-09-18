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
    private static JObject jsonRequest; // ͳһ��JSON����ģ�����
    private static string api_key;
    private System.Random random = new System.Random();

    void Start()
    {
        NLUI_Initialized();
    }
    // �����ʼ��
    void NLUI_Initialized()
    {
        api_key = apiKey[random.Next(apiKey.Length)];
        Memory = new()
        {
            new JObject
            {
                { "role", "system" },
                { "content", "����һ����Ȼ����UI�������������û�����Ȼ�������󣬷���Ӧ�õ��ú����ĺ������Ͷ�Ӧ�������������Ӧ��������Ӧ������Ȼ���Իظ��û�����Ļظ�Ӧ����������������" +
                "1. ע��ظ���಻�ܳ���100�֡�" +
                "2. ���ڲ�������һ��������������Ӧ�����û�������ȷ���û�������" }
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
        contentText.text = "Answering Question...";
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
            Debug.Log(messageJson["function_call"]);

            // JSON Responce Example: 
            //{
            //    "role": "assistant",
            //    "content": null,
            //    "function_call": {
            //        "name": "get_current_weather",
            //        "arguments": "{\n  \"location\": \"�Ͼ�\",\n  \"unit\": \"celsius\"\n}"
            //    }
            //}
            if (messageJson["function_call"] != null)
            {
                string functionName = (string)messageJson["function_call"]["name"]; // Ҫ���õĺ�����
                string argumentsJson = (string)messageJson["function_call"]["arguments"];
                JObject argumentsObject = JObject.Parse(argumentsJson);
                string[] parameters = argumentsObject.Values().Select(v => v.ToString()).ToArray(); // Ҫ����Ĳ���
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

                // ����Lambda���ʽ���ò�ͬ��ֵ
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
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text = (string)messageJson["content"];
            string per = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Per;
            Debug.Log(per);
            Action<string, string> setTextToSpeechValues = (fileName, per) =>
            {
                TextToSpeech tts = GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>();
                tts.fileName = fileName;
                tts.Per = per;
                tts.BttnTtSClick();
                Task.Delay(TimeSpan.FromSeconds(1)).Wait(); // ��ʱ1��
            };

            // ����Lambda���ʽ���ò�ͬ��ֵ
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
    /// ����������
    /// </summary>
    /// <param name="query">Ҫ���͵�JSON��ʽ����</param>
    /// <param name="OnResponse">���յ���������Ӧ��Ҫ���õĴ�����</param>
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