using I2.Loc;
using System;
using UnityEngine;
using UnityEngine.UI;

public class QRChoice : MonoBehaviour
{
    public Text txt;

    AccountManager accountManager;
    Button[] btns;
    string[] bookIndex = new string[4];

    private void OnEnable()
    {
        accountManager = FindObjectOfType<AccountManager>();

        txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
        txt.font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

        btns = GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            Button btn = btns[i];
            btn.interactable = true;
            if (!btn.name.Contains("QRChoice"))
                btn.transform.GetComponentInChildren<Text>().color = btn.GetComponent<Image>().color;

            btn.onClick.AddListener(() => OnClick(btn));
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < btns.Length; i++)
            btns[i].onClick.RemoveAllListeners();
    }

    public void ResponseInfo(string[] list)
    {
        bookIndex = list;
        for (int i = 0; i < bookIndex.Length; i++)
        {
            Button btn = btns[i + 1];
            if (bookIndex[i].Equals(string.Empty))
            {
                btn.interactable = false;
                btn.transform.GetComponentInChildren<Text>().color = btn.colors.disabledColor;
            }
        }
    }



    private void OnClick(Button btn)
    {
        if (!btn.name.Contains("QRChoice"))
        {
            btn.interactable = false;
            accountManager.QRButtonController(null, bookIndex[Convert.ToInt32(btn.name)]);
        }
        Destroy(this.gameObject);
    }


}