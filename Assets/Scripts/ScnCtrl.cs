using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;

public class ScnCtrl : MonoBehaviour
{
    public GameObject cubePrefab; // 可以在Inspector视图中拖拽一个立方体预制体到这个字段
    public GameObject spherePrefab; // 可以在Inspector视图中拖拽一个球体预制体到这个字段
                                  // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void BttnRunNLUI()
    {
        GameObject.Find("NLUI").GetComponent<NLUI>().Run();
    }
    public void InputUserQueryChanged()
    {
        GameObject.Find("NLUI").GetComponent<NLUI>().user_query = GameObject.Find("InputUserQuery").GetComponent<InputField>().text;
    }
    public void InputDurationChanged()
    {
        GameObject.Find("SpeechToText").GetComponent<SpeechToText>().duration = int.Parse(GameObject.Find("InputDuration").GetComponent<InputField>().text);
    }
    public void BttnCube()
    {
        for (int i = 0; i < 10; i++)
        {
            // 在位置(0, 0, 0)处创建一个立方体
            _ = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity);
        }
    }
    public void BttnSphere()
    {
        // 在位置(0, 0, 0)处创建一个球体
        _ = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
    }
    public void BttnDuXiaoYu()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoYu");
        }
    }

    public void BttnDuXiaoMei()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoMei");
        }
    }

    public void BttnDuYaYa()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuYaYa");
        }
    }

    public void BttnDuXiaoYao()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoYao");
        }
    }

    public void BttnDuXiaoLu()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoLu");
        }
    }

    public void BttnDuBoWen()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuBoWen");
        }
    }

    public void BttnDuXiaoTong()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoTong");
        }
    }

    public void BttnDuXiaoMeng()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoMeng");
        }
    }

    public void BttnDuMiDuo()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuMiDuo");
        }
    }

    public void BttnDuXiaoJiao()
    {
        if (GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().Text != "")
        {
            GameObject.Find("TextToSpeech").GetComponent<TextToSpeech>().StartPlay("DuXiaoJiao");
        }
    }
    public void BttnStop()
    {
        GameObject.Find("Audio Source").GetComponent<AudioSource>().Stop();
    }
}
