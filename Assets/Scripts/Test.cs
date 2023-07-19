using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
