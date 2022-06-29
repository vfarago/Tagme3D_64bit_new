using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
public class BookPanelManager : MonoBehaviour
{
    public GameObject requireTxt;
    public Button[] btns_tm;
    public GameObject scrollPanel;

    CheckCode checkCode;
    AnimalDataSetLoader adsl;
    CanvasManager canvasManager;

    private string url = "http://bookplusapp.co.kr/fileStorage/tm_book_2019_2_19/";

    string path;
    private void Awake()
    {
        path = Application.persistentDataPath;

        checkCode = FindObjectOfType<CheckCode>();
        adsl = FindObjectOfType<AnimalDataSetLoader>();
        canvasManager = FindObjectOfType<CanvasManager>();

        btns_tm = GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        requireTxt.SetActive(true);

        if (!btns_tm[0].gameObject.activeSelf) btns_tm[0].gameObject.SetActive(true);

        for (int i = 0; i < btns_tm.Length; i++)
        {
            Button btn = btns_tm[i];
            btn.onClick.AddListener(() => Onclick(btn));

            if (btn.name.Equals("TagMe3D_New_Full"))
            {
                if (adsl.tagmeDataSets.Count > 3)
                    requireTxt.SetActive(false);

                if (adsl.tagmeDataSets.Count > 2)
                    btn.gameObject.SetActive(false);
                else
                    btn.gameObject.SetActive(true);
            }
            if (btn.name.Contains("Book"))
            {
            string assetName = string.Format("tagme3d_new_book{0}", btn.name.Split('k')[1]);
                StartCoroutine(CheckBundle(assetName,
                    check1 =>
                    {
                        if (check1)
                        {
                            btn.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        else
                        {
                            btn.transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    ));
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < btns_tm.Length; i++)
        {
            btns_tm[i].onClick.RemoveAllListeners();
        }
    }

    private void Onclick(Button btn)
    {
        switch (btn.name)
        {
            case "BGButton":
                gameObject.GetComponent<PanelMovingController>().PanelOff();

                break;
            case "TagMe3D_New_Full":
                canvasManager.toastMsgPanel.SetActive(true);
                canvasManager.toastMsgPanel.GetComponent<ToastMsgManager>().ToastMessage("downStart", btn.name, true);

                break;
            default:
                if (btn.transform.GetChild(0).gameObject.activeSelf)
                {
                    canvasManager.toastMsgPanel.SetActive(true);
                    canvasManager.toastMsgPanel.GetComponent<ToastMsgManager>().ToastMessage("downStart", btn.name, true);
                }
                else
                {
                    canvasManager.toastMsgPanel.SetActive(true);
                    canvasManager.toastMsgPanel.GetComponent<ToastMsgManager>().ToastMessage("downExist", btn.name, false);
                }

                break;
        }
    }

    IEnumerator CheckBundle(string assetName, System.Action<bool> exists)
    {
        bool isCheck = false;
        for (int i = 0; i < 3; i++)
        {
            string format = "";
            string fileName = "";
            switch (i)
            {
                case 0:
                    format = string.Format("{0}/assets/{1}", path, assetName);
                    fileName = assetName;
                    break;

                case 1:
                    format = string.Format("{0}/audios/{1}_audio", path, assetName);
                    fileName = assetName+"_audio";
                    break;

                case 2:
                    format = string.Format("{0}/videos/{1}_video", path, assetName);
                    fileName = assetName + "_video";
                    break;

            }
            if (File.Exists(format))
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    isCheck = true;
                    break;
                }
                else
                {
                    FileInfo inf = new FileInfo(format);
                    long fileSize = inf.Length;

                    UnityWebRequest reqs = UnityWebRequest.Head(url + fileName);
                    reqs.SendWebRequest();

                    while (!reqs.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    long checkSize = long.Parse(reqs.GetResponseHeader("Content-Length"));

                    if (checkSize == 0)
                    {
                        isCheck = true;
                    }
                    else if (fileSize == checkSize)
                    {
                        isCheck = true;
                    }
                    else
                    {
                        //File.Delete(path + assetName);
                        isCheck = false;
                        break;
                    }
                }
            }
            else
            {
                isCheck = false;
                break;
            }
        }
        if (isCheck) exists(true);
        else exists(false);

        yield return null;
    }
}
