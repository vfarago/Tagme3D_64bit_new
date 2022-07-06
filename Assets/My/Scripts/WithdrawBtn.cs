using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WithdrawBtn : MonoBehaviour
{
    [SerializeField] private GameObject parentPanel;
    private void Update()
    {
        if (parentPanel.activeSelf)
        {
            if (Manager.CheckCode.storedType.Contains("admin") || Manager.CheckCode.storedType.Contains("group"))
            {
                GetComponent<Image>().color = new Color(0, 0, 0, 0);
                GetComponent<Button>().interactable = false;
            }
            else
            {
                GetComponent<Image>().color = new Color(1, 1, 1, 1);
                GetComponent<Button>().interactable = true;
                Font font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
                GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_WithdrawTxt");
                GetComponentInChildren<Text>().font = font;
            }
        }
    }
}