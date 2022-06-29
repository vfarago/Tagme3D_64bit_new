//using I2.Loc;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TouchScript.Gestures;
//using System.IO;
//public class MRPanel : MonoBehaviour
//{
//    public Button[] buttons;
//    public Sprite[] onSpr;
//    public Sprite[] offSpr;
//    public GameObject titleZone;
//    public ScrollRect[] rects;
//    public bool isDone;
//    public Camera arCam, mainCam;
//    private Button[] tempBtn;
//    private bool[] isReady;
//    private int lastNum;
//    //70 40 -180 24

//    // Start is called before the first frame update
//    void Awake()
//    {
       
//    }

//    public void SetButtons()
//    {
//        isDone = false;
//        buttons = GetComponentsInChildren<Button>();
//        rects = GetComponentsInChildren<ScrollRect>();
//        tempBtn = new Button[5];
//        isReady = new bool[5];

//        for (int i = 0; i < buttons.Length; i++)
//        {
//            int num = i;
//            if (num < 5)
//            {
//                buttons[num].onClick.AddListener(() =>
//                {
//                    OnClick();
//                    buttons[num].GetComponent<Image>().sprite = onSpr[num];
//                    rects[num].gameObject.SetActive(true);
//                });
//            }
//        }

//        for (int i = 0; i < 5; i++)
//        {
//            Button btn = tempBtn[i] = rects[i].GetComponentInChildren<Button>();
//            Text text = btn.GetComponentInChildren<Text>();

//            GameObject[] go = Resources.LoadAll<GameObject>(string.Format("objects/Book{0}", i + 1));
//            btn.name = text.text = go[0].gameObject.name;
//            btn.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
//            btn.gameObject.transform.GetChild(1).gameObject.SetActive(false);
//            btn.onClick.AddListener(() =>
//            {
//                //Manager.isMR = true;
//                //mainCam.enabled = true;
//                //arCam.enabled = false;
//                //Manager.CanvasManager.PanelManager(true);
//                //Manager.PrefabLoader.ChangePrefab(btn.gameObject.name, true);
//                //Manager.CanvasManager.OnPhonicsPanel(true);
//                //gameObject.SetActive(false);

//                Manager.isMR = true;
//                Manager.CanvasManager.PanelManager(true);
//                Manager.CanvasManager.arPanel.transform.GetChild(0).gameObject.SetActive(false);
//                DataSetOnOff[] onoffshelter = Manager.AnimalDataSetLoader.gameObject.transform.GetComponentsInChildren<DataSetOnOff>();

//                for (int k = 0; k < onoffshelter.Length; k++)
//                {
//                    GameObject objs = onoffshelter[k].gameObject;
//                    for (int l = 0; l < objs.transform.childCount; l++)
//                    {
//                        if (btn.name.Contains(objs.transform.GetChild(l).name.ToLower()))
//                        {
//                            DynamicTrackableEventHandler handler = objs.transform.GetChild(l).GetComponent<DynamicTrackableEventHandler>();
//                            handler.OnMRFound(btn.name, true);
//                            break;
//                        }
//                    }
//                }

//                gameObject.SetActive(false);

//            });
//            for (int j = 2; j < go.Length; j += 2)
//            {
//                GameObject obj = Instantiate(btn.gameObject, btn.transform.parent);
//                Button btnIns = obj.GetComponent<Button>();
//                Text textIns = btnIns.GetComponentInChildren<Text>();
//                btnIns.name = textIns.text = go[j].name;
//                btnIns.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
//                btnIns.gameObject.transform.GetChild(1).gameObject.SetActive(false);
//                btnIns.onClick.AddListener(() =>
//                {
//                    Manager.isMR = true;
//                    Manager.CanvasManager.PanelManager(true);
//                    Manager.CanvasManager.arPanel.transform.GetChild(0).gameObject.SetActive(false);
//                    DataSetOnOff[] onoffshelter = Manager.AnimalDataSetLoader.gameObject.transform.GetComponentsInChildren<DataSetOnOff>();

//                    for (int k = 0; k < onoffshelter.Length; k++)
//                    {
//                        GameObject objs = onoffshelter[k].gameObject;
//                        for (int l = 0; l < objs.transform.childCount; l++)
//                        {
//                            if (btnIns.name.Contains(objs.transform.GetChild(l).name.ToLower()))
//                            {
//                                DynamicTrackableEventHandler handler = objs.transform.GetChild(l).GetComponent<DynamicTrackableEventHandler>();
//                                handler.OnMRFound(btnIns.name, true);
//                                break;
//                            }
//                        }
//                    }

//                    gameObject.SetActive(false);
//                });
//            }
//            SettingButton(i);
//            if (i > 0)
//            {
//                rects[i].gameObject.SetActive(false);
//            }
//        }
//        isDone = true;
//    }

//    private void OnEnable()
//    {
//        rects[lastNum].gameObject.SetActive(true);
//        for (int i = 0; i < titleZone.transform.childCount; i++)
//        {
//            GameObject obj = titleZone.transform.GetChild(i).gameObject;
//            if (obj.activeSelf) obj.SetActive(false);
//            if (obj.name.Equals(LocalizationManager.CurrentLanguage.ToLower()))
//                obj.SetActive(true);
//        }

//        for (int i = 0; i < 5; i++)
//        {
//            int nonExistStopNum = i.Equals(1) ? 12 : 11;
//            GameObject scrCont = gameObject.transform.GetChild(5 + i).GetChild(0).GetChild(0).gameObject;
//            string fileLoc = string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, i + 1);
//            bool isExist = File.Exists(fileLoc);

//            if (!Manager.CheckCode.isLogined)
//            {
//                for (int j = 0; j < scrCont.transform.childCount; j++)
//                {
//                    scrCont.transform.GetChild(j).gameObject.SetActive(false);
//                }
//                scrCont.transform.GetChild(0).gameObject.SetActive(true);
//                scrCont.transform.GetChild(1).gameObject.SetActive(true);
//            }
//            else
//            {
//                SettingButton(i);


//                if (Manager.CheckCode.isScaned[i])
//                {


//                    for (int j = 0; j < 100; j++)
//                    {
//                        if (j.Equals(nonExistStopNum))
//                        {
//                            if (!isExist) break;
//                            if (!Manager.CheckCode.isScaned[i]) break;
//                        }

//                        if (scrCont.transform.GetChild(j).gameObject==null)
//                            Debug.Log("여기다");

//                        scrCont.transform.GetChild(j).gameObject.SetActive(true);
//                    }
//                }
//            }
//        }
//    }
//    private void OnDisable()
//    {
//        for (int i = 0; i < rects.Length; i++)
//        {
//            if (rects[i].gameObject.activeSelf) lastNum = i;
//            rects[i].gameObject.SetActive(false);
//        }
//    }
//    void SettingButton(int bookNumber)
//    {
//        string fileLoc = string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, bookNumber + 1);
//        if (File.Exists(fileLoc) && !isReady[bookNumber])
//        {
//            for (int j = 0; j < 100; j++)
//            {
//                int isScan = bookNumber;
//                if (Manager.PrefabShelter.tmModel[(bookNumber * 100) + (j)] == null) continue;
//                if (Manager.PrefabShelter.tmModel[(bookNumber * 100) + (j)].model == null) continue;
//                GameObject obj = Instantiate(tempBtn[bookNumber].gameObject, tempBtn[bookNumber].transform.parent);
//                Button btnIns = obj.GetComponent<Button>();
//                Text textIns = btnIns.GetComponentInChildren<Text>();
//                btnIns.name = textIns.text = Manager.PrefabShelter.tmModel[(bookNumber * 100) + (j)].model.name.ToLower();
//                btnIns.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
//                btnIns.gameObject.transform.GetChild(1).gameObject.SetActive(false);
//                btnIns.gameObject.transform.GetChild(2).gameObject.SetActive(false);
//                btnIns.gameObject.transform.GetChild(1).gameObject.name = "isLock";
//                btnIns.onClick.AddListener(() =>
//                {
//                    if (Manager.CheckCode.isScaned[isScan])
//                    {
//                        Manager.isMR = true;
//                        Manager.CanvasManager.PanelManager(true);
//                        Manager.CanvasManager.arPanel.transform.GetChild(0).gameObject.SetActive(false);
//                        DataSetOnOff[] onoffshelter = Manager.AnimalDataSetLoader.gameObject.transform.GetComponentsInChildren<DataSetOnOff>();

//                        for (int k = 0; k < onoffshelter.Length; k++)
//                        {
//                            GameObject objs = onoffshelter[k].gameObject;
//                            for (int l = 0; l < objs.transform.childCount; l++)
//                            {
//                                if (btnIns.name.Contains(objs.transform.GetChild(l).name.ToLower()))
//                                {
//                                    DynamicTrackableEventHandler handler = objs.transform.GetChild(l).GetComponent<DynamicTrackableEventHandler>();
//                                    handler.OnMRFound(btnIns.name, false);
//                                    break;
//                                }
//                            }
//                        }

//                        gameObject.SetActive(false);
//                    }
//                });
//            }
//            isReady[bookNumber] = true;
//        }
//    }
//    void OnClick()
//    {
//        for (int i = 0; i < 5; i++)
//        {
//            int num = i;
//            if (buttons[num].GetComponent<Image>().sprite.name.Contains("active"))
//            {
//                rects[num].gameObject.SetActive(false);
//                buttons[num].GetComponent<Image>().sprite = offSpr[num];
//                break;
//            }
//        }
//    }
//    void TargetClick()
//    {
//        Debug.Log("hee");
//    }
//}
