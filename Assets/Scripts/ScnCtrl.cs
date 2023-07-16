using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScnCtrl : MonoBehaviour
{
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
}
