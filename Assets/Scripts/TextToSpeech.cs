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

    public string Text; // �ı�����
    public string fileName; // �����ļ�
    public int Spd; // ���٣�ȡֵ0-15��Ĭ��Ϊ5������
    public string Per; // ����ң����Ʒ��=5003����С¹=5118���Ȳ���=106����Сͯ=110����С��=111�����׶�=103����С��=5

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
            "&cuid=Womendeai080921" + // �û�Ψһ��ʶ����������UVֵ��������д�������û��Ļ��� MAC ��ַ�� IMEI �룬����Ϊ60�ַ�����
            "&ctp=1" + // �ͻ�������ѡ��web����д�̶�ֵ1
            "&lan=zh" + // �̶�ֵzh������ѡ��,Ŀǰֻ����Ӣ�Ļ��ģʽ����д�̶�ֵzh
            "&vol=9" + // ��������������ȡֵ0-9����Ʒ����ȡֵ0-15��Ĭ��Ϊ5��������ȡֵΪ0ʱΪ������Сֵ������Ϊ������
            "&per=" + per +
            "&spd=" + spd +
            "&aue=6"); // 3Ϊmp3��ʽ(Ĭ��)�� 4Ϊpcm-16k��5Ϊpcm-8k��6Ϊwav������ͬpcm-16k��;
                            // �����Ŀ��Ʋ������https://ai.baidu.com/ai-doc/SPEECH/mlbxh7xie

        using (UnityWebRequest www = new(url, "POST"))
        {
            // �����ϴ������������ش�����������Ҫ�����ϴ�byte[]��ʽ�����ݵ��������Լ���������Ӧ��һЩ�ļ����͵����ݣ�
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            www.uploadHandler = new UploadHandlerRaw(requestData);

            // ��������ͷ�����������Ҫ�Ļ�
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
                    // �����ﴦ����Ƶ���ݣ�audioClip��
                    Debug.Log("��Ƶ���سɹ���");
                    SaveAudioToFile(savePath, audioClip);
                }
                else if (contentType == "application/json")
                {
                    Debug.Log("����... ");
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
                // �ɹ�����.wav�ļ�������AudioClip����
                AudioClip audioClip = www.GetAudioClip();
                if (audioClip != null)
                {
                    // ��������Զ�AudioClip���в��������粥�Ż򱣴��
                    audioSourse.clip = audioClip;
                    audioSourse.Play();
                }
            }
            else
            {
                // ����ʧ�ܣ����������Ϣ
                Debug.LogError("Failed to load audio: " + www.error);
            }
        }
    }


    // �ڽ��յ���Ƶ���ݺ���ô˷���
    public void SaveAudioToFile(string path, AudioClip audioClip)
    {
        string filePath = path; // ��Ƶ�ļ�����·�����ɸ�����Ҫ�����ļ�������չ��

        // ��AudioClip����ΪWAV�ļ�
        SavWav.Save(filePath, audioClip);

        Debug.Log("��Ƶ�ļ��ѱ��棺" + filePath);
    }
}

public static class SavWav
{
    public static bool Save(string filePath, AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("AudioClipΪ�գ��޷�����ΪWAV�ļ���");
            return false;
        }

        // ��ȡ��Ƶ����
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        // ����Ƶ����ת��Ϊ�ֽ�����
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

        // ����WAV�ļ���д������
        using (FileStream fileStream = CreateEmpty(filePath))
        {
            if (fileStream == null)
            {
                Debug.LogWarning("�޷�����WAV�ļ���");
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

        for (int i = 0; i < 44; i++) // д��յ�44�ֽ�ͷ��
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
