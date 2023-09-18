using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Web;
using System.IO;
using System;
using System.Threading.Tasks;

public class TextToSpeech : MonoBehaviour
{
    public AudioSource audioSource;

    public string Text; // 文本内容
    public string fileName; // 保存文件
    public int Spd; // 语速，取值0-15，默认为5中语速
    public string Per; // 度逍遥（精品）=5003，度小鹿=5118，度博文=106，度小童=110，度小萌=111，度米朵=103，度小娇=5

    public void BttnTtSClick()
    {
        StartCoroutine(TtSRequest(Text, Application.dataPath + "/Audios/" + fileName + ".wav", Per, Spd));
    }
    IEnumerator TtSRequest(string text,string savePath, string per="5003", int spd=5)
    {
        string tex = HttpUtility.UrlEncode(text);
        // Debug.Log(tex);
        string url = "https://tsn.baidu.com/text2audio";
        byte[] requestData = Encoding.UTF8.GetBytes(
            "tex=" + tex +
            "&tok=24.81ae3f57f7e6d25aa83732651de948a4.2592000.1697454261.282335-35964679" + // Access Token
            "&cuid=Womendeai080921" + // 用户唯一标识，用来计算UV值。建议填写能区分用户的机器 MAC 地址或 IMEI 码，长度为60字符以内
            "&ctp=1" + // 客户端类型选择，web端填写固定值1
            "&lan=zh" + // 固定值zh。语言选择,目前只有中英文混合模式，填写固定值zh
            "&vol=9" + // 音量，基础音库取值0-9，精品音库取值0-15，默认为5中音量（取值为0时为音量最小值，并非为无声）
            "&per=" + per +
            "&spd=" + spd +
            "&aue=6"); // 3为mp3格式(默认)； 4为pcm-16k；5为pcm-8k；6为wav（内容同pcm-16k）;
                            // 其他的控制参数详见https://ai.baidu.com/ai-doc/SPEECH/mlbxh7xie

        using (UnityWebRequest www = new(url, "POST"))
        {
            // 设置上传处理程序和下载处理程序（如果需要额外上传byte[]格式的数据到服务器以及服务器响应是一些文件类型的数据）
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            www.uploadHandler = new UploadHandlerRaw(requestData);

            // 设置请求头部，如果有需要的话
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.error);
            }
            else
            {
                string contentType = www.GetResponseHeader("Content-Type");
                Debug.Log("Content-Type: " + contentType);
                if (contentType == "audio/wav")
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    // 在这里处理音频数据（audioClip）
                    Debug.Log("音频下载成功！");
                    SaveAudioToFile(savePath, audioClip);
                }
                else if (contentType == "application/json")
                {
                    Debug.Log("重试... ");
                }
            }
        }
    }
    public void StartPlay(string name)
    {
        StartCoroutine(LoadAudio(GameObject.Find("Audio Source").GetComponent<AudioSource>(), Application.dataPath + "/Audios/" + name + ".wav"));
    }
    IEnumerator LoadAudio(AudioSource audioSourse, string wavFilePath)
    {
        using (WWW www = new WWW("file://" + wavFilePath))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // 成功加载.wav文件，创建AudioClip变量
                AudioClip audioClip = www.GetAudioClip();
                if (audioClip != null)
                {
                    // 在这里可以对AudioClip进行操作，比如播放或保存等
                    audioSourse.clip = audioClip;
                    audioSourse.Play();
                }
            }
            else
            {
                // 加载失败，输出错误信息
                Debug.LogError("Failed to load audio: " + www.error);
            }
        }
    }


    // 在接收到音频数据后调用此方法
    public void SaveAudioToFile(string path, AudioClip audioClip)
    {
        string filePath = path; // 音频文件保存路径，可根据需要更改文件名和扩展名

        // 将AudioClip保存为WAV文件
        SavWav.Save(filePath, audioClip);

        Debug.Log("音频文件已保存：" + filePath);
    }
}

public static class SavWav
{
    public static bool Save(string filePath, AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("AudioClip为空，无法保存为WAV文件。");
            return false;
        }

        // 获取音频数据
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        // 将音频数据转换为字节数组
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = new byte[2];
            byteArr = System.BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        // 创建WAV文件并写入数据
        using (FileStream fileStream = CreateEmpty(filePath))
        {
            if (fileStream == null)
            {
                Debug.LogWarning("无法创建WAV文件。");
                return false;
            }

            WriteHeader(fileStream, audioClip);
            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        return true;
    }

    static FileStream CreateEmpty(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) // 写入空的44字节头部
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void WriteHeader(FileStream fileStream, AudioClip audioClip)
    {
        int hz = audioClip.frequency;
        int channels = audioClip.channels;
        int samples = audioClip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        byte[] chunkSize = System.BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        byte[] subChunk1 = System.BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        ushort one = 1;

        byte[] audioFormat = System.BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        byte[] numChannels = System.BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        byte[] sampleRate = System.BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        byte[] byteRate = System.BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels
        fileStream.Write(byteRate, 0, 4);

        ushort blockAlign = (ushort)(channels * 2);
        fileStream.Write(System.BitConverter.GetBytes(blockAlign), 0, 2);

        ushort bitsPerSample = 16;
        byte[] bits = System.BitConverter.GetBytes(bitsPerSample);
        fileStream.Write(bits, 0, 2);

        byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);

        byte[] subChunk2 = System.BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
}
