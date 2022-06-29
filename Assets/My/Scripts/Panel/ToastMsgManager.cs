using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ToastMsgManager : MonoBehaviour
{
    public Button yesButton, noButton, okButton;
    public GameObject failText;
    public GameObject downText;
    public GameObject connectText;


    FileDownloader fileDownloader;
    Font changefont;
    string dataSetName;
    private bool isWifiCheck;

    float test = 0;
    #region ENABLE_DISABLE
    private void Awake()
    {
        isWifiCheck = false;
        fileDownloader = Manager.FileDownloader;
    }

    private void OnEnable()
    {
        yesButton.onClick.AddListener(() => ClickBtn(yesButton));
        noButton.onClick.AddListener(() => ClickBtn(noButton));
        okButton.onClick.AddListener(() => ClickBtn(okButton));
    }

    private void OnDisable()
    {
        yesButton.onClick.RemoveListener(() => ClickBtn(yesButton));
        noButton.onClick.RemoveListener(() => ClickBtn(noButton));
        okButton.onClick.RemoveListener(() => ClickBtn(okButton));
    }
    #endregion

    private void ClickBtn(Button bb)
    {
        switch (bb.name)
        {
            case "btn_yes":
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ToastMessage("connectFail", dataSetName, false);
                }
                else if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    if(!isWifiCheck)
                    {
                        downText.GetComponent<Text>().text = LocalizationManager.GetTermTranslation("UI_wifi");
                        connectText.GetComponent<Text>().text = LocalizationManager.GetTermTranslation("UI_datasize");
                        isWifiCheck = true;
                    }
                    else
                    {
                        fileDownloader.OnClickYes(dataSetName);
                        gameObject.SetActive(false);
                    }
                }

                else
                {
                    fileDownloader.OnClickYes(dataSetName);
                    gameObject.SetActive(false);
                }


                //fileDownloader.OnClickYes(dataSetName);

                break;
            case "btn_ok":
            case "btn_no":
                gameObject.SetActive(false);
                break;
        }
    }

    public void ToastMessage(string localKey, string dataSetName, bool on)
    {
        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        string bookNumber = "";

        if (!dataSetName.Equals(string.Empty))
        {
            bookNumber = dataSetName.Substring(dataSetName.Length - 1);
            this.dataSetName = dataSetName;
        }

        PanelButtonSetting(on);

        Text[] goText = GetComponentsInChildren<Text>();
        foreach (Text txt in goText)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;

            if (txt.name.Equals("downloadFile"))
            {
                if (localKey.Equals("downWait"))
                {
                    txt.text = LocalizationManager.GetTermTranslation("UI_" + localKey);
                }
                else
                {
                    if (bookNumber.Equals("l"))
                        txt.text = LocalizationManager.GetTermTranslation("UI_downAllStart");
                    else
                        txt.text = LocalizationManager.GetTermTranslation("UI_" + localKey).Replace("*", bookNumber);
                }

            }
        }
    }

    public void PopUpMsg(string msg, bool btn)
    {
        changefont = Resources.Load<Font>("fonts/baloo-regular");

        PanelButtonSetting(btn);
        okButton.GetComponentInChildren<Text>().text = "OK";

        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = msg;
        transform.GetChild(0).GetChild(0).GetComponent<Text>().font = changefont;
    }

    public void PanelButtonSetting(bool on)
    {
        isWifiCheck = false;
        yesButton.gameObject.SetActive(on);
        noButton.gameObject.SetActive(on);
        okButton.gameObject.SetActive(!on);
        failText.SetActive(on);

        if (on)
        {
            yesButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_yes");
            yesButton.GetComponentInChildren<Text>().font = changefont;
            noButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_no");
            noButton.GetComponentInChildren<Text>().font = changefont;
        }
        else
        {
            okButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_ok");
            okButton.GetComponentInChildren<Text>().font = changefont;
        }
    }
}
