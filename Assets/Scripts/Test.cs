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

    // Update is called once per frame
    void Update()
    {
        
    }
}
