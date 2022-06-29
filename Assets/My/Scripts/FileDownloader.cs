using I2.Loc;
using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileDownloader : MonoBehaviour
{
    public Text assetLoaderText, downSpeedText;
    public GameObject bookDownloadGO;
    public Image mainProgress;

    AnimalDataSetLoader aDSL;
    CanvasManager canvasManager;
    CheckCode checkCode;

    private string savedDataSet;
    private int bookNumber;
    [SerializeField]
    private List<FileList> fileList = new List<FileList>();

    private WWW ping;
    private WWW downPing;
    private GameObject blocker;
    private bool downloadInProgress;
    private float pingWait = 10f;
    private float pingWaitEllap;
    private float downPingWait = 9f;
    private float downPingWaitEllap;
    private float downServerWait = 12f;
    private float downServerWaitEllap;

    private long totalSize;
    private long lastSize;
    private float timerSpeed = 0.75f;
    private float timerSpeedEllap = 0;
    private double speed = 0;
    //private string url = "https://tm-book-2019-2-19.oss-cn-beijing.aliyuncs.com/assets/";
    private string pingUrl = "http://bookplusapp.co.kr/fileStorage/tm_book_2019_2_19/";
    private string url = "http://bookplusapp.co.kr/";
    private string[] audioFolder = { "kor", "eng", "chn", "esp", "jpn", "deu", "fra", "are", "ita", "pol", "hin", "heb", "rus", "tha", "word" };

    public int coroutineNumber;
    private List<IEnumerator> coroutines;

    private void Awake()
    {
        aDSL = GetComponent<AnimalDataSetLoader>();
        canvasManager = FindObjectOfType<CanvasManager>();
        checkCode = FindObjectOfType<CheckCode>();
    }


    private void Update()
    {
        if (ping != null)
        {
            if (ping.isDone && string.IsNullOrEmpty(ping.error))
            {
                StartCoroutine(Download(savedDataSet));

                Debug.Log("Connected successfully");
                ping = null;
            }
            else if (pingWaitEllap < pingWait)
            {
                pingWaitEllap += Time.deltaTime;
            }
            else
            {
                Debug.Log("Connection check failed - No ping from server");
                Destroy(blocker);
                ActiveWindow("connectFail");
                savedDataSet = string.Empty;
                ping = null;
            }
        }

        if (downloadInProgress)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                downServerWaitEllap = 0;
            }
            else if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (downServerWaitEllap < downServerWait)
                {
                    downServerWaitEllap += Time.deltaTime;
                }
                else
                {
                    downServerWaitEllap = 0;
                    ForceQuitDownload(true);
                    Debug.Log("Download force quitted - Internet connection lost");
                }
            }
            else
            {
                if (downPing != null)
                {
                    if (downPing.isDone && string.IsNullOrEmpty(downPing.error))
                    {
                        downPing = new WWW(pingUrl);
                        //downPing = new WWW(pingurl);
                    }
                    else if (downPingWaitEllap < downPingWait)
                    {
                        downPingWaitEllap += Time.deltaTime;
                    }
                    else
                    {
                        ForceQuitDownload(true);
                        Debug.Log("Download force quitted - Server connection lost");
                    }
                }
            }
        }
    }

    //파일 체크 → 있으면 프리펩셋팅 / 없으면 다운로드후 프리펩셋팅
    public void CheckFile(string bookName)
    {
        bool checkBook = true;

        if (bookName.Contains("tm_full"))
        {
            for (int i = 0; i < 5; i++)
            {
                string videoPath = string.Format("{0}/videos/tagme3d_new_book{1}_video", Application.persistentDataPath, (i + 1).ToString());
                string audioPath = string.Format("{0}/audios/tagme3d_new_book{1}_audio", Application.persistentDataPath, (i + 1).ToString());
                if (!File.Exists(videoPath) || File.Exists(audioPath))
                {
                    checkBook = false;
                    break;
                }
            }

            bool assetExist = true;
            //여기 4->5
            for (int i = 0; i < 5; i++)
            {
                bool assetCheck = File.Exists(string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, i));
                assetExist = assetExist && assetCheck;
            }

            if (checkBook && assetExist)
            {
                if (!aDSL.check)
                {
                    canvasManager.bookPanel.SetActive(true);
                    canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();

                    //CreateBlocker();
                    //StartCoroutine(TargetDataSetting("TagMe3D_New_Full"));
                }
            }
            else
            {
                canvasManager.bookPanel.SetActive(true);
                canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();
                //OnClickYes("TagMe3D_New_Full");
            }
        }
        else
        {
            int bookNum = Convert.ToInt32(bookName.Substring(bookName.Length - 1));
            savedDataSet = string.Format("TagMe3D_New_Book{0}", bookNum);

            string videoPath = string.Format("{0}/video/tagme3d_new_book{1}_video", Application.persistentDataPath, bookNum.ToString());
            string audioPath = string.Format("{0}/audio/tagme3d_new_book{1}_audio", Application.persistentDataPath, bookNum.ToString());

            if (!File.Exists(videoPath) || !File.Exists(audioPath)) checkBook = false;

            bool assetExist = File.Exists(string.Format("{0}/assets/tagme3d_new_book{1}", Application.persistentDataPath, bookNum));

            if (checkBook && assetExist)
            {
                canvasManager.bookPanel.SetActive(true);
                canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();

                CreateBlocker();
                StartCoroutine(TargetDataSetting(string.Format("TagMe3D_New_Book{0}", bookNum)));
            }
            else
            {
                canvasManager.bookPanel.SetActive(true);
                canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();
                OnClickYes(string.Format("TagMe3D_New_Book{0}", bookNum));
            }
        }
    }


    public void OnClickYes(string dataSetName)
    {
        FindObjectOfType<BookPanelManager>().btns_tm[0].gameObject.SetActive(false);
        if (ping == null)
        {
            Font localFont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

            assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downCheckPing");
            assetLoaderText.font = localFont;

            pingWaitEllap = 0;

            ping = new WWW(url);
            //ping = new WWW(pingurl);
            savedDataSet = dataSetName;
            mainProgress.fillAmount = 0;

            int intOut = 0;
            if (int.TryParse(savedDataSet.Substring(savedDataSet.Length - 1), out intOut))
            {
                bookNumber = intOut;
            }
            else
            {
                bookNumber = 0;
            }

            CreateBlocker();
        }
    }

    private void ActiveWindow(string st)
    {
        canvasManager.toastMsgPanel.SetActive(false);
        canvasManager.toastMsgPanel.SetActive(true);
        canvasManager.toastMsgPanel.GetComponent<ToastMsgManager>().ToastMessage(st, string.Empty, false);
    }

    private void CreateBlocker()
    {
        if (blocker != null)
            Destroy(blocker);
        blocker = new GameObject("blocker", typeof(Image), typeof(Button));
        blocker.transform.parent = canvasManager.bookPanel.transform;
        RectTransform rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition3D = new Vector3(0, 0, 0);
        rect.localScale = new Vector3(1, 1, 1);
        Image image = blocker.GetComponent<Image>();
        image.color = Color.clear;
        Button button = blocker.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ActiveWindow("downWait"));
    }

    private void ForceQuitDownload(bool window)
    {
        StopAllCoroutines();
        assetLoaderText.text = string.Empty;

        if (blocker != null)
        {
            Destroy(blocker);
        }


        if (downPing != null)
        {
            downPing = null;
        }

        if (window)
            ActiveWindow("connectFail");
        else
            ActiveWindow("downFileError");

        mainProgress.fillAmount = 0;
        downSpeedText.transform.parent.gameObject.SetActive(false);
        savedDataSet = string.Empty;

        downloadInProgress = false;
    }

    public IEnumerator Download(string dataSetName)
    {
        if (Directory.Exists(Application.persistentDataPath + "/asset"))
        {
            Directory.Delete(Application.persistentDataPath + "/asset", true);
        }
        if (Directory.Exists(Application.persistentDataPath + "/audio"))
        {
            Directory.Delete(Application.persistentDataPath + "/audio", true);
        }
        if (Directory.Exists(Application.persistentDataPath + "/video"))
        {
            Directory.Delete(Application.persistentDataPath + "/video", true);
        }
        downloadInProgress = true;
        fileList.Clear();

        string progressString;
        savedDataSet = dataSetName;

        downPing = new WWW(pingUrl + dataSetName + "s.zip");
        //downPing = new WWW(pingurl);

        bookDownloadGO.transform.GetChild(6).gameObject.SetActive(false);

        downSpeedText.transform.parent.gameObject.SetActive(true);
        downSpeedText.gameObject.SetActive(false);


        //____________ Check Server Files
        assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downFileCheck");

        totalSize = 0;

        bool checkInfo = false;

        StartCoroutine(Download_Information(dataSetName.ToLower(), output =>
        {
            checkInfo = true;
        }));

        while (!checkInfo)
        {
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < fileList.Count; i++)
        {
            totalSize += fileList[i].size;
        }


        //____________ Download Start
        progressString = LocalizationManager.GetTermTranslation("UI_downloading");
        assetLoaderText.text = string.Format("{0}...", progressString);

        downSpeedText.gameObject.SetActive(true);

        bool checkDown = false;

        StartCoroutine(Download_Data(dataSetName.ToLower(), output =>
        {
            checkDown = true;
        }));

        while (!checkDown)
        {
            long currentSize = 0;

            for (int i = 0; i < fileList.Count; i++)
            {
                currentSize += fileList[i].size;
            }

            if (timerSpeedEllap < timerSpeed)
            {
                timerSpeedEllap += Time.deltaTime;
                speed += (lastSize - currentSize);
            }
            else
            {
                float speedMultiplier = 1 / timerSpeed;
                speed *= speedMultiplier;

                timerSpeedEllap = 0;
                if (speed <= 0)
                {
                    downSpeedText.text = "Unpredictable";
                }
                else if (speed < 1024)
                {
                    downSpeedText.text = string.Format("{0}/Bps", speed.ToString("####"));
                }
                else if (speed < 1048576)
                {
                    downSpeedText.text = string.Format("{0}/Kbps", (speed / 1024).ToString("####.#"));
                }
                else
                {
                    downSpeedText.text = string.Format("{0}/Mbps", (speed / 1048576).ToString("####.#"));
                }
                speed = 0;
            }

            lastSize = currentSize;

            mainProgress.fillAmount = (float)(totalSize - currentSize) / totalSize;
            assetLoaderText.text = string.Format("{0}...   {1:00.0}%", progressString, ((float)(totalSize - currentSize) / totalSize) * 100);

            yield return new WaitForEndOfFrame();
        }
        downSpeedText.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();


        //____________ Download File Extraction
        bool checkExtract = false;

        StartCoroutine(Download_Extract(dataSetName.ToLower(), output =>
        {
            checkExtract = true;
        }));

        while (!checkExtract)
        {
            yield return new WaitForEndOfFrame();
        }


        downSpeedText.transform.parent.gameObject.SetActive(false);
        assetLoaderText.text = string.Empty;

        Debug.Log("Total downloaded file Size : " + totalSize);

        if (!totalSize.Equals(0))
        {
            StartCoroutine(TargetDataSetting(dataSetName));
        }
        else
        {
            ForceQuitDownload(false);
        }

        savedDataSet = string.Empty;
        downloadInProgress = false;

        yield return null;
    }

    private IEnumerator TargetDataSetting(string dataSetName)
    {
        PrefabShelter prefabShelter = FindObjectOfType<PrefabShelter>();

        if (dataSetName.Equals("TagMe3D_New_Full"))
        {
            aDSL.tagmeDataSets = new List<string>();

            //여기 4->5
            for (int j = 0; j < 5; j++)
            {
                int bookNum = j + 1;
                if (bookNum.Equals(5))
                {
                    FindObjectOfType<BookPanelManager>().scrollPanel.transform.localPosition = new Vector3(0, 865, 0);
                }
                assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downSetPrefab").Replace("*", bookNum.ToString());
                Image prefabProgress = bookDownloadGO.transform.GetChild(7).GetChild(0).GetChild(0).GetChild(bookNum).GetChild(0).GetComponent<Image>();
                //prefabProgress.gameObject.SetActive(true);
                prefabProgress.fillAmount = 1;

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

                    if (!aDSL.tagmeTargets[targetNum].Equals(string.Empty))
                    {
                        AssetBundleRequest req = bundles.LoadAssetAsync<GameObject>(aDSL.tagmeTargets[targetNum]);
                        prefabShelter.tmModel[targetNum] = new TMModel((GameObject)req.asset, false);
                    }
                    else
                        prefabShelter.tmModel[targetNum] = new TMModel(null, false);

                    prefabProgress.fillAmount = 1 - ((i + 1f) / 100f);
                    yield return prefabProgress.fillAmount;
                }
                bundles.Unload(false);

                aDSL.tagmeDataSets.Add(string.Format("TagMe3D_New_Book{0}", j + 1));
                aDSL.fileExist = true;
            }

            Destroy(blocker);
            Resources.UnloadUnusedAssets();
            yield return new WaitForEndOfFrame();

            canvasManager.PanelManager(true);
            assetLoaderText.text = string.Empty;
        }
        else
        {
            int bookNum = Convert.ToInt32(dataSetName.Substring(dataSetName.Length - 1, 1));

            assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downSetPrefab").Replace("*", bookNum.ToString());
            int imgNum = bookNum < 4 ? 1 : 2;
            int bookNumber = bookNum < 4 ? bookNum-1 : bookNum - 4;
            Image prefabProgress = bookDownloadGO.transform.GetChild(7).GetChild(0).GetChild(0).GetChild(imgNum).GetChild(bookNumber).GetChild(0).GetComponent<Image>();
            //prefabProgress.gameObject.SetActive(true);
            prefabProgress.fillAmount = 1;

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

                if (targetNum >= aDSL.tagmeTargets.Count) break;

                if (!aDSL.tagmeTargets[targetNum].Equals(string.Empty))
                {
                    AssetBundleRequest req = bundles.LoadAssetAsync<GameObject>(aDSL.tagmeTargets[targetNum]);
                    prefabShelter.tmModel[targetNum] = new TMModel((GameObject)req.asset, false);
                }
                else
                    prefabShelter.tmModel[targetNum] = new TMModel(null, false);

                prefabProgress.fillAmount = 1 - ((i + 1f) / 100f);
                yield return prefabProgress.fillAmount;
            }
            bundles.Unload(false);

            bool checkSet = false;
            for (int i = 0; i < aDSL.tagmeDataSets.Count; i++)
            {
                if (aDSL.tagmeDataSets.Equals(dataSetName))
                    checkSet = true;
            }

            if (!checkSet)
                aDSL.tagmeDataSets.Add(dataSetName);

            Destroy(blocker);
            Resources.UnloadUnusedAssets();
            yield return new WaitForEndOfFrame();

            canvasManager.PanelManager(true);
            assetLoaderText.text = string.Empty;
        }
    }


    #region DOWNLOAD_DATA
    private IEnumerator Download_Information(string name, Action<bool> output)
    {
        string path = string.Format("{0}{1}s.zip", pingUrl, name);

        UnityWebRequest req = UnityWebRequest.Get(path);
        req.method = "HEAD";
        req.Send();

        while (!req.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        while (downServerWaitEllap != 0)
        {
            req.Abort();

            req = UnityWebRequest.Get(req.url);
            req.Send();

            while (!req.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        long fileSize = 0;
        if (long.TryParse(req.GetResponseHeader("Content-Length"), out fileSize))
        {
            Debug.Log("download file size : " + fileSize);
            fileList.Add(new FileList(name, fileSize));
        }

        output(true);

        yield return null;
    }

    private IEnumerator Download_Data(string name, Action<bool> output)
    {
        string path = string.Format("{0}{1}s.zip", pingUrl, name);
        int index = fileList.FindIndex(find => find.name == name);
        long originalSize = fileList[index].size;

        UnityWebRequest req = new UnityWebRequest(path);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.Send();

        while (!req.isDone)
        {
            fileList[index].size = originalSize - (long)(originalSize * req.downloadProgress);

            yield return new WaitForEndOfFrame();
        }

        while (downServerWaitEllap != 0)
        {
            req.Abort();

            fileList[index].size = originalSize;

            req = UnityWebRequest.Get(req.url);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.Send();

            while (!req.isDone)
            {
                fileList[index].size = originalSize - (long)(originalSize * req.downloadProgress);

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        fileList[index].size = 0;

        string downloadPath = string.Format("{0}/{1}s.zip", Application.persistentDataPath, name);
        File.WriteAllBytes(downloadPath, req.downloadHandler.data);

        output(true);

        yield return null;
    }

    #endregion     //DOWNLOAD_DATA


    private IEnumerator Download_Extract(string name, Action<bool> output)
    {
        string zipFile = string.Format("{0}/{1}s.zip", Application.persistentDataPath, name);
        string location = Application.persistentDataPath;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        Directory.CreateDirectory(location);

        using (ZipFile zip = ZipFile.Read(zipFile))
        {
            zip.ExtractAll(location, ExtractExistingFileAction.OverwriteSilently);
        }

#elif UNITY_ANDROID
		using (AndroidJavaClass zipper = new AndroidJavaClass ("com.tsw.zipper")) {
			zipper.CallStatic ("unzip", zipFile, location);
		}
#elif UNITY_IPHONE
		unzip (zipFile, location);
#endif

        output(true);
        File.Delete(zipFile);

        yield return null;
    }


    public IEnumerator StartCoroutine_Queue(IEnumerator coroutine)
    {
        while (coroutines.Count > coroutineNumber)
        {
            for (int i = 0; i < coroutines.Count; i++)
            {
                if (coroutines[i] == null)
                {
                    coroutines.Remove(coroutines[i]);
                }

                if (!coroutines[i].MoveNext())
                {
                    coroutines.Remove(coroutines[i]);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        coroutines.Add(coroutine);
        StartCoroutine(coroutine);

        yield return null;
    }

    //private void ClearLog()
    //{
    //    Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
    //    Type type = assembly.GetType("UnityEditorInternal.LogEntries");
    //    MethodInfo method = type.GetMethod("Clear");
    //    method.Invoke(new object(), null);
    //}
}

[Serializable]
public class FileList
{
    public string name;
    public long size;

    public FileList(string _name, long _size)
    {
        name = _name;
        size = _size;
    }
}

[Serializable]
public class Exceptions
{
    public string name;
    public string type;
    public int book;

    public Exceptions(string _name, string _type, int _book)
    {
        name = _name;
        type = _type;
        book = _book;
    }
}