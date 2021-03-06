using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Xml;
using Vuforia;
using System.Security.AccessControl;

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

    public string serverUrl = "http://bookplusapp.co.kr/fileStorage/tm_book_20_3_12/";

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

    //  Elon
    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;
    private string databaseUrl = "";
    private string databaseFileName = "";
    private string dataAsJSON;

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
        //LoadDataSet();

        VuforiaApplication.Instance.OnVuforiaStarted += LoadDataSet;
        ARManager.Instance.trackable = false;
    }

    void OnDestroy()
    {
        //VuforiaARController.Instance.UnregisterVuforiaStartedCallback(LoadDataSet);
    }

    void LoadDataSet()
    {
        //ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        Debug.Log("LoadDataSet");

        //#if UNITY_EDITOR
        dataSetName = string.Format("TagMe3D_New_Book{0}", dataSetNumber);
        databaseUrl = string.Format(Application.persistentDataPath + "/{0}{1}", dataSetName, ".xml");
        //databaseUrl = string.Format("http://bookplusapp.co.kr/fileStorage/tm_book_20_3_12" + "/{0}{1}", dataSetName, ".xml")
        //#elif UNITY_ANDROID
        //        dataSetName = string.Format("TagMe3D_New_Book{0}", dataSetNumber);
        //        databaseFileName = string.Format("TagMe3D_New_Book{0}{1}", dataSetNumber, ".xml");

        //        string path = "jar:file://" + Application.dataPath + "!/assets/Vuforia/"+ databaseFileName;
        //        WWW www = new WWW(path);
        //        while (!www.isDone) { }
        //        databaseUrl = string.Format("{0}/{1}", Application.persistentDataPath, databaseFileName);
        //        File.WriteAllBytes(databaseUrl, www.bytes);

        //        StreamReader wr = new StreamReader(databaseUrl);
        //        string line;
        //        while ((line = wr.ReadLine()) != null)
        //        {
        //            //your code
        //            Debug.Log(line);
        //        }
        //#endif
        //IEnumerable<ObserverBehaviour> observers = vuforiaBehaviour.ObserverFactory.CreateBehavioursFromDatabase("Assets/StreamingAssets/Vuforia/VuforiaMigration.xml");

        DownloadFileAll(() => { StartCoroutine(CheckFile(dataSetName));});
        //DataSet dataSet = objectTracker.CreateDataSet();
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
    }
    Coroutine cur_Cor;
    List<Coroutine> progress = new List<Coroutine>();
    void DownloadFileAll(Action done)
    {
        StartCoroutine(Cor_DownloadFileAll("TagMe3D_New_Book{0}.xml", () => { StartCoroutine(Cor_DownloadFileAll("TagMe3D_New_Book{0}.dat", () => { done(); })); }));

    }
    IEnumerator Cor_DownloadFileAll(string format, Action done)
    {

        for (int i = 1; i < 6; i++)
        {
            int dataSetNumber = i;
            string dataSetName = string.Format(format, dataSetNumber);
            string databaselocal = string.Format(Application.persistentDataPath + "/{0}", dataSetName);

            if (!File.Exists(databaselocal))
            {
                cur_Cor = StartCoroutine(DownloadFile(format));
                progress.Add(cur_Cor);
            }
            while (cur_Cor != null)
            {
                yield return new WaitForEndOfFrame();
            }
            dataSetNumber++;
            if (progress.Contains(cur_Cor))
            {
                progress.Remove(cur_Cor);
            }
        }
        done();
    }
    private IEnumerator DownloadFile(string format)
    {
        string dataSetName = string.Format(format, dataSetNumber);
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + "/" + dataSetName);
        //www.downloadHandler = new DownloadHandlerFile(Application.persistentDataPath+ "TagMe3D_New_Book1.xml");
        //print(serverUrl + "TagMe3D_New_Book1.xml");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        byte[] results = www.downloadHandler.data;

        File.WriteAllBytes(Application.persistentDataPath + "/" + dataSetName, results);
        cur_Cor = null;
        //yield return StartCoroutine(CheckFile(dataSetName));
    }

    //private void SetDirectorySecurity(string linePath)
    //{
    //    DirectorySecurity dSecurity = Directory.GetAccessControl(linePath);
    //    dSecurity.AddAccessRule(new FileSystemAccessRule("Users",
    //                                                                FileSystemRights.FullControl,
    //                                                                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
    //                                                                PropagationFlags.None,
    //                                                                AccessControlType.Allow));
    //    Directory.SetAccessControl(linePath, dSecurity);
    //}
    //바꿔야합니다0627
    IEnumerator CheckFile(string _dataSetName)
    {

        //SetDirectorySecurity(Application.persistentDataPath);
        //  Elon 220629
        GameObject temp = transform.Find(_dataSetName).gameObject;
        IEnumerable<ObserverBehaviour> observers = vuforiaBehaviour.ObserverFactory.CreateBehavioursFromDatabase(databaseUrl);

        foreach (ObserverBehaviour ob in observers)
        {
            if (ob.name.Equals("New Game Object"))
            {

            }

            ob.gameObject.name = ob.TargetName;

            if (ob.TargetName.Contains("cover"))
            {
                ob.gameObject.transform.parent = transform;
                ob.gameObject.AddComponent<CoverTrackerbleEventHandler>();
            }
            else
            {
                bool checkfree = false;
                ob.gameObject.transform.parent = temp.transform;
                DynamicTrackableEventHandler dteh = ob.gameObject.AddComponent<DynamicTrackableEventHandler>();

                for (int i = 0; i < freePage.Length; i++)
                {
                    if (freePage[i].Equals(ob.TargetName))
                    {
                        checkfree = true;
                        break;
                    }
                }
                dteh.isFreeModel = checkfree;

                if (checkfree)
                    tagmeTargets.Add(string.Empty);
                else
                    tagmeTargets.Add(ob.TargetName.ToLower());
            }
        }
        //IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();

        //foreach (TrackableBehaviour tb in tbs)
        //{
        //    if (tb.name.Equals("New Game Object"))
        //    {
        //        tb.gameObject.name = tb.TrackableName;

        //        if (tb.TrackableName.Contains("cover"))
        //        {
        //            tb.gameObject.transform.parent = transform;
        //            tb.gameObject.AddComponent<CoverTrackerbleEventHandler>();
        //        }
        //        else
        //        {
        //            bool checkfree = false;

        //            tb.gameObject.transform.parent = temp.transform;
        //            DynamicTrackableEventHandler dteh = tb.gameObject.AddComponent<DynamicTrackableEventHandler>();

        //            for (int i = 0; i < freePage.Length; i++)
        //            {
        //                if (freePage[i].Equals(tb.TrackableName))
        //                {
        //                    checkfree = true;
        //                    break;
        //                }
        //            }
        //            dteh.isFreeModel = checkfree;

        //            if (checkfree)
        //                tagmeTargets.Add(string.Empty);
        //            else
        //                tagmeTargets.Add(tb.TrackableName.ToLower());
        //        }
        //    }
        //}
        yield return new WaitForEndOfFrame();

        bool check = true;
        bool assetCheck = true;
        bool[] checks = new bool[100];

        for (int i = 0; i < checks.Length; i++)
        {
            checks[i] = true;
        }

        assetCheck = File.Exists(string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, dataSetNumber));

        Debug.Log(string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, dataSetNumber));

        for (int i = 0; i < 100; i++)
        {
            int index = i + ((dataSetNumber - 1) * 100);


            if (!tagmeTargets[index].Equals(string.Empty))
            {
                bool videoPath = File.Exists(string.Format("{0}/videos/tagme3d_new_book{1}_video", Application.persistentDataPath, dataSetNumber));
                bool audioPath = File.Exists(string.Format("{0}/audios/tagme3d_new_book{1}_audio", Application.persistentDataPath, dataSetNumber));
                if (!videoPath || !audioPath)
                {
                    checks[i] = false;
                    break;
                }
            }

            progressBar.fillAmount = 0.5f + (((index + 1) / 500f) * 0.5f);
            yield return progressBar.fillAmount;
        }


        for (int i = 0; i < checks.Length; i++)
        {
            if (!checks[i])
            {
                check = false;
                break;
            }
        }
        yield return check = assetCheck && check;

        if (check)
        {
            tagmeDataSets.Add(dataSetName);
        }

        if (dataSetNumber < 5)
        {
            dataSetNumber++;

            //  Elon 220629
            VuforiaApplication.Instance.OnVuforiaStarted += LoadDataSet;
            //VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
        }
        else
        {
            StartCoroutine(TargetPrefabSetting());
        }

        yield return null;
    }

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

        Manager.MRPanel.gameObject.SetActive(true);
        Manager.MRPanel.SetButtons();
        while (!Manager.MRPanel.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        Manager.MRPanel.gameObject.SetActive(false);
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