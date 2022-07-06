using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WithdrawBtn : MonoBehaviour
{
    private void OnEnable()
    {
        if(Manager.CheckCode.storedType.Contains("admin")|| Manager.CheckCode.storedType.Contains("group"))
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            Font font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
            GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_WithdrawTxt");
            GetComponentInChildren<Text>().font = font;
        }
    }
    private void OnDisable()
    {
        
    }
}
