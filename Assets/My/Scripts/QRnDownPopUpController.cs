using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class QRnDownPopUpController : MonoBehaviour
{
    CanvasManager canvasManager;
    AccountManager accountManager;
    CheckCode checkCode;
    ARManager arManager;

    public bool isQR;
    private Button yes, no;
    private Toggle toastToggle;
    private Text title, main;

    private void Awake()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        accountManager = FindObjectOfType<AccountManager>();
        checkCode = FindObjectOfType<CheckCode>();
        arManager = ARManager.Instance;

        toastToggle = GetComponentInChildren<Toggle>();
        toastToggle.onValueChanged.AddListener(delegate
        {
            QrToastValueChanged(toastToggle.isOn);
        });
    }

    private void OnEnable()
    {
        toastToggle.isOn = canvasManager.isNotToastAgain;

        Button[] btns = GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            Button btn = btns[i];
            btn.onClick.AddListener(() => OnClick(btn));

            if (btn.name.Equals("btn_yes"))
                yes = btn;
            else if (btn.name.Equals("btn_cancel"))
                no = btn;
        }

        Text[] txts = GetComponentsInChildren<Text>();
        for (int i = 0; i < txts.Length; i++)
        {
            Text txt = txts[i];

            if (!txt.name.Contains("noLo_"))
            {
                txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            }
            txt.font = canvasManager.localizeFont;

            if (txt.name.Contains("title"))
                title = txt;
            else if (txt.name.Contains("main"))
                main = txt;
        }

        if (isQR)
        {
            yes.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_QRScan");
            title.text = LocalizationManager.GetTermTranslation("UI_serialinfoTitle");
            main.text = LocalizationManager.GetTermTranslation("UI_serialinfoTextFull");
        }
        else
        {
            yes.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_downloadTitle");
            title.text = LocalizationManager.GetTermTranslation("UI_downInfoTitle");
            main.text = LocalizationManager.GetTermTranslation("UI_downInfoText");
        }
    }

    //Toast 그만띄워!
    private void QrToastValueChanged(bool on)
    {
        canvasManager.isNotToastAgain = on;
        checkCode.NotToastAgain(true);
    }

    private void OnClick(Button btn)
    {
        switch (btn.name)
        {
            case "btn_yes":
                canvasManager.PanelManager(false);

                if (isQR)
                {
                    accountManager.SerialCheck();
                }
                else
                {
                    canvasManager.bookPanel.SetActive(true);
                    canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();
                }


                break;
            case "btn_cancel":
                gameObject.SetActive(false);
                arManager.ActivateDataSet(true);

                break;
        }
    }
}
