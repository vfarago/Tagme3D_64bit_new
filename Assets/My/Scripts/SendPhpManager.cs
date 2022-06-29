using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SendPhpManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public IEnumerator SendCode(string userID, string productID, string languageID, string activeID, string wordName)
    {
        WWWForm form = new WWWForm();

        form.AddField("userid", userID);
        form.AddField("prdtid", productID);
        form.AddField("langid", languageID);
        form.AddField("activeid", activeID);
        form.AddField("word", wordName);

        //여기 주소 적을것
        UnityWebRequest www = UnityWebRequest.Post("", form);
        yield return www.SendWebRequest();

        string response = www.downloadHandler.text;
        Debug.Log(response);

        yield return null;
    }
}
