using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Xml;
using Vuforia;

public class AnimalDataSetLoader : MonoBehaviour
{
    public UnityEngine.UI.Image progressBar;
    public Text progressText;
    public GameObject arCam;

    public string dataSetName = "";  // Assets/StreamingAssets/QCAR/DataSetName
    public bool fileExist = false; //file Exist Check

    private int dataSetNumber = 1;       // 책넘버
    public List<string> tagmeDataSets; //로컬파일로 다운받은 데이터셋이름(All 소문자)
    public List<string> tagmeTargets; //전체 타겟이름(All 소문자)
    public bool check = true;

    private string[] freePage = { "Pig", "Cat", "Dog", "Hamster", "Ant", "Duck", "Giraffe", "Lion", "Cow", "Raccoon", "Elephant",
            "Ladybug", "Goose", "Fly", "Fox", "Koala", "Bird", "Beaver", "Eagle", "Butterfly", "Alligator", "Flamingo", "Worm",
            "Helicopter", "Helmet", "Bicycle", "Ambulance", "Bus", "Car", "Hot_Air_Balloon", "Trailer", "Truck_Trans", "Fire_Truck", "Traffic_Cone",
            "Balloons", "Truck_Toy", "Cards", "Doll", "Drumstick", "Dice", "Guitar", "Drum", "Easel", "Swing_Set", "Blocks",
            "Avocado", "Jam", "Juice", "Kale", "Noodle", "Nuts", "Rice", "Vinegar", "Yam", "Yogurt", "Zucchini"};

    string[] audioFolder = { "kor", "eng", "chn", "esp", "jpn", "deu", "fra", "are", "ita", "pol", "hin", "heb", "rus", "tha", "word" };
    private string url = "http://bookplusapp.co.kr/fileStorage/tm_book_2019_2_19/";
    CanvasManager canvasManager;
    PrefabShelter prefabShelter;

    //바꿔야합니다0627
    void Awake()
    {
        //VuforiaAbstractConfiguration.Instance.Vuforia.DelayedInitialization = false;
        arCam.GetComponent<VuforiaBehaviour>().enabled = true;
        //VuforiaRuntime.Instance.InitVuforia();

        canvasManager = Manager.CanvasManager;
        prefabShelter = Manager.PrefabShelter;

        tagmeTargets = new List<string>();
        tagmeDataSets = new List<string>();

        dataSetName = string.Format("TagMe3D_New_Book{0}", dataSetNumber);

        //Vuforia ver.6.2

        //VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
        LoadDataSet();
    }

    void OnDestroy()
    {
        //VuforiaARController.Instance.UnregisterVuforiaStartedCallback(LoadDataSet);
    }

    void LoadDataSet()
    {
        ////ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        //dataSetName = string.Format("TagMe3D_New_Book{0}", dataSetNumber);
        ////DataSet dataSet = objectTracker.CreateDataSet();
        //var dataSet = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget();
        //if (dataSet.Load(dataSetName))
        //{
        //    if (!objectTracker.ActivateDataSet(dataSet))
        //    {
        //        // Note: ImageTracker cannot have more than 1000 total targets activated
        //        Debug.Log("<color=yellow>Failed to Activate DataSet: " + (dataSetName) + "</color>");
        //    }

        //    if (!objectTracker.Start())
        //    {
        //        Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
        //    }

        //    StartCoroutine(CheckFile(dataSetName, objectTracker));
        //    objectTracker.Stop();
        //}
        //else
        //{
        //    Debug.LogError("<color=yellow>Failed to load dataset: '" + (dataSetName) + "'</color>");
        //}

        string streamingAssetsPath = Application.streamingAssetsPath;
        for (int i = 0; i < 5; i++)
        {
            string dataSetName = "/Vuforia/TagMe3D_New_Book" + (i + 1).ToString() + ".xml";
            string finalPath = streamingAssetsPath + dataSetName;
            if (!File.Exists(finalPath)) continue;
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(finalPath);

                string targetName = "";
                var imageTarget = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(dataSetName, targetName);
            }
        }
    }

    //바꿔야합니다0627
    //IEnumerator CheckFile(string dataSetName, ObjectTracker objectTracker)
    //{
    //    GameObject temp = transform.Find(dataSetName).gameObject;
    //    IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();

    //    foreach (TrackableBehaviour tb in tbs)
    //    {
    //        if (tb.name.Equals("New Game Object"))
    //        {
    //            tb.gameObject.name = tb.TrackableName;

    //            if (tb.TrackableName.Contains("cover"))
    //            {
    //                tb.gameObject.transform.parent = transform;
    //                tb.gameObject.AddComponent<CoverTrackerbleEventHandler>();
    //            }
    //            else
    //            {
    //                bool checkfree = false;

    //                tb.gameObject.transform.parent = temp.transform;
    //                DynamicTrackableEventHandler dteh = tb.gameObject.AddComponent<DynamicTrackableEventHandler>();

    //                for (int i = 0; i < freePage.Length; i++)
    //                {
    //                    if (freePage[i].Equals(tb.TrackableName))
    //                    {
    //                        checkfree = true;
    //                        break;
    //                    }
    //                }
    //                dteh.isFreeModel = checkfree;

    //                if (checkfree)
    //                    tagmeTargets.Add(string.Empty);
    //                else
    //                    tagmeTargets.Add(tb.TrackableName.ToLower());
    //            }
    //        }
    //    }
    //    yield return new WaitForEndOfFrame();

    //    bool check = true;
    //    bool assetCheck = true;
    //    bool[] checks = new bool[100];

    //    for (int i = 0; i < checks.Length; i++)
    //    {
    //        checks[i] = true;
    //    }

    //    assetCheck = File.Exists(string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, dataSetNumber));


    //    for (int i = 0; i < 100; i++)
    //    {
    //        int index = i + ((dataSetNumber - 1) * 100);


    //        if (!tagmeTargets[index].Equals(string.Empty))
    //        {
    //            bool videoPath = File.Exists(string.Format("{0}/videos/tagme3d_new_book{1}_video", Application.persistentDataPath, dataSetNumber));
    //            bool audioPath = File.Exists(string.Format("{0}/audios/tagme3d_new_book{1}_audio", Application.persistentDataPath, dataSetNumber));
    //            if (!videoPath || !audioPath)
    //            {
    //                checks[i] = false;
    //                break;
    //            }
    //            //if(!File.Exists(audioPath) || File.Exists(videoPath))
    //            //{
    //            //    checks[i] = false;
    //            //    break;
    //            //}
    //            //for (int j = 0; j < audioFolder.Length; j++)
    //            //{
    //            //    string audioPath = string.Format("{0}/audio/{1}/{2}.mp3", Application.persistentDataPath, audioFolder[j], tagmeTargets[index]);

    //            //    if (!File.Exists(videoPath) || !File.Exists(audioPath))
    //            //    {
    //            //        checks[i] = false;
    //            //        break;
    //            //    }
    //            //}
    //        }

    //        progressBar.fillAmount = 0.5f + (((index + 1) / 500f) * 0.5f);
    //        yield return progressBar.fillAmount;
    //    }


    //    for (int i = 0; i < checks.Length; i++)
    //    {
    //        if (!checks[i])
    //        {
    //            check = false;
    //            break;
    //        }
    //    }
    //    yield return check = assetCheck && check;

    //    if (check)
    //    {
    //        tagmeDataSets.Add(dataSetName);
    //    }

    //    if (dataSetNumber < 5)
    //    {
    //        dataSetNumber++;


    //        VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
    //    }
    //    else
    //    {
    //        StartCoroutine(TargetPrefabSetting());
    //    }

    //    yield return null;
    //}

    //InitializeTarget() 5권 완료 후 AssetBundle 컴포넌트에 셋팅 → 로딩끝
    private IEnumerator TargetPrefabSetting()
    {
        progressBar.fillAmount = 0;
        this.check = true;

        if (tagmeDataSets.Count.Equals(0))
            prefabShelter.nothingModel = true;

        for (int j = 0; j < tagmeDataSets.Count; j++)
        {
            int bookNum = Convert.ToInt32(tagmeDataSets[j].Substring(tagmeDataSets[j].Length - 1, 1));

#if UNITY_EDITOR
            string path = string.Format("file:///{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, bookNum);
#elif UNITY_ANDROID
            string path = string.Format("file://{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, bookNum);
#endif

            UnityWebRequest webr = UnityWebRequestAssetBundle.GetAssetBundle(path);
            webr.Send();

            while (!webr.isDone)
                yield return new WaitForEndOfFrame();

            AssetBundle bundles = DownloadHandlerAssetBundle.GetContent(webr);


            for (int i = 0; i < 100; i++)
            {
                int targetNum = ((bookNum - 1) * 100) + i;


                if (!tagmeTargets[targetNum].Equals(string.Empty))
                {
                    AssetBundleRequest req = bundles.LoadAssetAsync<GameObject>(tagmeTargets[targetNum]);
                    prefabShelter.tmModel[targetNum] = new TMModel((GameObject)req.asset, false);
                    //Renderer[] renderer = prefabShelter.tmModel[targetNum].model.GetComponentsInChildren<Renderer>(true);
                    ////foreach(Renderer item in renderer)
                    ////{
                    ////    if(item.materials != null)
                    ////    {
                    ////        foreach(Material mat in item.materials)
                    ////        {
                    ////            Shader sha = mat.shader;
                    ////            sha = Shader.Find(sha.name);
                    ////        }
                    ////    }
                    ////}
                    //for(int k = 0; k < renderer.Length; k++)
                    //{
                    //    Shader sha = renderer[k].material.shader;
                    //    sha = Shader.Find(sha.name);
                    //}
                }
                else
                    prefabShelter.tmModel[targetNum] = new TMModel(null, false);



                progressText.text = string.Format("Preparing Tagme3D Friends   {0}/{1}", (j * 100) + (i + 1f), tagmeDataSets.Count * 100f);
                progressBar.fillAmount = ((j * 100) + (i + 1f)) / (tagmeDataSets.Count * 100f);
                yield return progressBar.fillAmount;
            }
            bundles.Unload(false);

            Resources.UnloadUnusedAssets();
        }

        yield return new WaitForEndOfFrame();

        //Manager.MRPanel.gameObject.SetActive(true);
        //Manager.MRPanel.SetButtons();
        //while (!Manager.MRPanel.isDone)
        //{
        //    yield return new WaitForEndOfFrame();
        //}
        //Manager.MRPanel.gameObject.SetActive(false);
        canvasManager.OnLoadingDone();

        yield return null;
    }

    public IEnumerator FreeModelCheck()
    {
        bool isCheck = true;
        string fileName = "";
        string path = "";
        for (int i = 0; i < 2; i++)
        {
            switch (i)
            {
                case 0:
                    fileName = "tagme3d_new_free_audio";
                    path = string.Format("{0}/audios/{1}", Application.persistentDataPath, fileName);
                    break;
                case 1:
                    fileName = "tagme3d_new_free_video";
                    path = string.Format("{0}/videos/{1}", Application.persistentDataPath, fileName);
                    break;
            }
            if (File.Exists(path))
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    isCheck = true;
                    break;
                }
                else
                {
                    FileInfo inf = new FileInfo(path);
                    long fileSize = inf.Length;

                    UnityWebRequest reqs = UnityWebRequest.Head(url + fileName);
                    reqs.SendWebRequest();
                    while (!reqs.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    long checkSize = long.Parse(reqs.GetResponseHeader("Content-Length"));
                    Debug.Log(checkSize);
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
        if (!isCheck) StartCoroutine(Manager.FileDownloader.Download("tagme3d_new_free"));
        yield return null;
    }

}