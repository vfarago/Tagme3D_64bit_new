using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class HowToUseManager : MonoBehaviour
{
    public GameObject linkButton;

    Text[] howToUse;

    private void Awake()
    {
        howToUse = GetComponentsInChildren<Text>();
        Button[] link = linkButton.GetComponentsInChildren<Button>();
        foreach (Button btn in link)
        {
            btn.onClick.AddListener(() => LinkUrl(btn));
        }
    }

    void OnEnable()
    {
        Font changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        foreach (Text txt in howToUse)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;
        }
    }

    void OnDisable()
    {
        Button[] link = linkButton.GetComponentsInChildren<Button>();
        foreach (Button btn in link)
        {
            btn.onClick.RemoveListener(() => LinkUrl(btn));
        }
    }

    private void LinkUrl(Button btn)
    {
        switch (btn.name)
        {
            case "link_BuyBook":
            case "link_SignUp":
                Application.OpenURL("http://bookplusapp.com");

                break;
                

                break;
            case "link_Youtube":
                Application.OpenURL(LocalizationManager.GetTermTranslation("UI_youtube_book"));

                break;
            case "link_Youku":
                Application.OpenURL("https://v.youku.com/v_show/id_XMTU5OTEzNzQwNA==.html");

                break;
        }
    }

}
