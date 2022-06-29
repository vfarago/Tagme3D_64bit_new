using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CheckCode : MonoBehaviour
{
    public Image isLogin;
    public Sprite logOn, logOff;
    public List<string> objName;
    public bool[] isScaned;
    public bool isLogined;

    CanvasManager canvasManager;
    AccountManager accountManager;
    private string title = "tm";
    private static string dataFolder;

    public bool reLogined;
    public bool loginConfirm, storedAdmin;
    public List<string> codeConfirms;
    public string storedID, storedPW, storedName, storedMail, storedDate, storedType;
    public List<SerialField> storedSerial;

    public Sprite adminImage, memberAImage, memberBImage, memberCImage, normalImage;

    [HideInInspector]
    public string myCode;
    [HideInInspector]
    public string myIdentifier;
    [HideInInspector]
    public string myModel;

    public void Awake()
    {
        isScaned = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            isScaned[i] = false;
        }
        canvasManager = FindObjectOfType<CanvasManager>();
        accountManager = FindObjectOfType<AccountManager>();

        dataFolder = string.Format("{0}/Data", Application.persistentDataPath);
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        string drawFolder = string.Format("{0}/drawImage", Application.persistentDataPath);
        if (!Directory.Exists(drawFolder))
            Directory.CreateDirectory(drawFolder);

        objName = new List<string>();
        string objPath = string.Format("{0}/TrackingObj", dataFolder);
        if (File.Exists(objPath))
        {
            string readFile = File.ReadAllText(objPath);
            string[] obj = readFile.Split('\n');
            for (int i = 0; i < obj.Length - 1; i++)
            {
                objName.Add(obj[i]);
            }
        }

        // ★★★★★ 아래 print는 절대 지우면 안돼! ★★★★★
        print("   언어초기화!  " + LocalizationManager.CurrentLanguage);
        string langPath = string.Format("{0}/UiLanguage", dataFolder);
        if (File.Exists(langPath))
        {
            string readFile = File.ReadAllText(langPath);
            I2.Loc.LocalizationManager.CurrentLanguage = readFile.Split(',')[0];
            canvasManager.ui_CurrentLang = readFile.Split(',')[0];
            canvasManager.isStartLangPanel = Convert.ToBoolean(readFile.Split(',')[1]);
            accountManager.isQRInfoPanel = Convert.ToBoolean(readFile.Split(',')[2]);
            //print(string.Format("       초기값:   {0}   ,   {1}", LocalizationManager.CurrentLanguage, canvasManager.isStartLangPanel));
        }
        else
        {
            if (Application.systemLanguage.Equals(SystemLanguage.Korean))
            {
                I2.Loc.LocalizationManager.CurrentLanguage = "kor";
                canvasManager.ui_CurrentLang = "kor";
            }
            else if (Application.systemLanguage.Equals(SystemLanguage.Chinese))
            {
                I2.Loc.LocalizationManager.CurrentLanguage = "chn";
                canvasManager.ui_CurrentLang = "chn";
            }
            else
            {
                I2.Loc.LocalizationManager.CurrentLanguage = "eng";
                canvasManager.ui_CurrentLang = "eng";
            }
            canvasManager.isStartLangPanel = false;
            accountManager.isQRInfoPanel = false;

            SaveCurrentUiLanguage();
        }
        canvasManager.StartLanguage();
        StartCoroutine(ReLogin());

        NotToastAgain(false);
        NotCautionMsg(false, false);
        StartCoroutine(Manager.AnimalDataSetLoader.FreeModelCheck());
    }

    public IEnumerator ReLogin()
    {
        reLogined = false;

        string path = string.Format("{0}/Used", dataFolder);
        if (File.Exists(path))
        {
            isLogined = true;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //File.Delete(path);
                reLogined = true;
                CheckRegister();
            }

            else
            {
                string reID = string.Empty;
                string rePW = string.Empty;

                StreamReader read = new StreamReader(path);

                string readAll = read.ReadToEnd();
                read.Close();
                string[] readSplit = readAll.Split('\n');

                for (int i = 0; i < readSplit.Length; i++)
                {
                    if (readSplit[i].Contains("login:"))
                    {
                        string[] infos = readSplit[i + 1].Split(',');
                        reID = infos[0];
                        rePW = infos[1];
                        break;
                    }
                }

                StartCoroutine(StartLogin(reID, rePW, title, returned =>
                {
                    reLogined = true;
                    Debug.Log("Re-Logined");

                }));

                while (!reLogined)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        else
        {
            isLogined = false;
            codeConfirms = new List<string>();
            reLogined = true;
        }

        yield return null;
    }

    public void CheckRegister()
    {
        loginConfirm = false;
        codeConfirms = new List<string>();

        storedSerial = new List<SerialField>();

        string path = string.Format("{0}/Used", dataFolder);

        if (File.Exists(path))
        {
            StreamReader read = new StreamReader(path);

            string readAll = read.ReadToEnd();
            read.Close();
            string[] readSplit = readAll.Split('\n');

            for (int i = 0; i < readSplit.Length; i++)
            {
                if (readSplit[i].Contains("login:"))
                {
                    string[] infos = readSplit[i + 1].Split(',');
                    storedID = infos[0];
                    storedPW = infos[1];
                    storedName = infos[2];
                    storedMail = infos[3];
                    storedDate = infos[4];
                    storedType = infos[5];
                    loginConfirm = true;
                    isLogin.sprite = logOn;
                    continue;
                }

                if (readSplit[i].Contains("using:"))
                {
                    bool checkNext = false;
                    int checkIndex = 0;

                    for (int j = i; j < readSplit.Length; j++)
                    {
                        if (j != i)
                        {
                            if (readSplit[j].Contains("using:"))
                            {
                                checkNext = true;
                                checkIndex = j;
                                break;
                            }
                        }
                    }

                    int checkLength = 0;

                    if (checkNext)
                    {
                        checkLength = checkIndex - (i + 1);
                    }
                    else
                    {
                        checkLength = readSplit.Length - (i + 1);
                    }

                    bool isUsing = false;
                    string code = string.Empty;
                    string book = string.Empty;
                    string[] deviceA = new string[0];
                    string[] deviceB = new string[0];

                    string[] splitUsing = readSplit[i].Replace("using:", string.Empty).Split(',');
                    code = splitUsing[0];
                    book = Regex.Replace(splitUsing[1], "[^0-9^a-z^_]", string.Empty);

                    switch (checkLength)
                    {
                        case 0:
                            storedSerial.Add(new SerialField(code, book));
                            break;

                        case 1:
                            deviceA = readSplit[i + 1].Split(',');

                            if (deviceA[1] == SystemInfo.deviceUniqueIdentifier)
                            {
                                myCode = code;
                                myIdentifier = SystemInfo.deviceUniqueIdentifier;
                                myModel = SystemInfo.deviceModel;

                                isUsing = true;
                                if (book.Equals("tm_full") || book.Contains("member"))
                                {
                                    Debug.Log("Full Checked");

                                    if (codeConfirms.FindIndex(find => find == "tm1") < 0)
                                    {
                                        codeConfirms.Add("tm1");
                                        isScaned[0] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm2") < 0)
                                    {
                                        codeConfirms.Add("tm2");
                                        isScaned[1] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm3") < 0)
                                    {
                                        codeConfirms.Add("tm3");
                                        isScaned[2] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm4") < 0)
                                    {
                                        codeConfirms.Add("tm4");
                                        isScaned[3] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm5") < 0)
                                    {
                                        codeConfirms.Add("tm5");
                                        isScaned[4] = true;
                                    }
                                }
                                else if (codeConfirms.FindIndex(find => find == book) < 0)
                                {
                                    if (!book.Contains("armat") || !book.Contains("_cn"))
                                    {
                                        codeConfirms.Add(book);
                                        int bookCode = int.Parse(book.Split('m')[1]) - 1;
                                        isScaned[bookCode] = true;
                                    }
                                }
                            }

                            if (deviceA[0] == "A")
                            {
                                storedSerial.Add(new SerialField(isUsing, code, book, deviceA, true));
                            }
                            else
                            {
                                storedSerial.Add(new SerialField(isUsing, code, book, deviceA, false));
                            }
                            break;

                        case 2:
                            deviceA = readSplit[i + 1].Split(',');
                            deviceB = readSplit[i + 2].Split(',');

                            if (deviceA[1] == SystemInfo.deviceUniqueIdentifier || deviceB[1] == SystemInfo.deviceUniqueIdentifier)
                            {
                                myCode = code;
                                myIdentifier = SystemInfo.deviceUniqueIdentifier;
                                myModel = SystemInfo.deviceModel;

                                isUsing = true;
                                if (book.Equals("tm_full") || book.Contains("member"))
                                {
                                    Debug.Log("Full Checked");

                                    if (codeConfirms.FindIndex(find => find == "tm1") < 0)
                                    {
                                        codeConfirms.Add("tm1");
                                        isScaned[0] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm2") < 0)
                                    {
                                        codeConfirms.Add("tm2");
                                        isScaned[1] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm3") < 0)
                                    {
                                        codeConfirms.Add("tm3");
                                        isScaned[2] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm4") < 0)
                                    {
                                        codeConfirms.Add("tm4");
                                        isScaned[3] = true;
                                    }
                                    if (codeConfirms.FindIndex(find => find == "tm5") < 0)
                                    {
                                        codeConfirms.Add("tm5");
                                        isScaned[4] = true;
                                    }
                                }
                                else if (codeConfirms.FindIndex(find => find == book) < 0)
                                {
                                    if (!(book.Contains("armat") || book.Contains("_cn")))
                                        codeConfirms.Add(book);
                                }
                            }

                            storedSerial.Add(new SerialField(isUsing, code, book, deviceA, deviceB));
                            break;
                    }
                    continue;
                }

                if (readSplit[i].Contains("ADMIN_ACCOUNT"))
                {
                    for (int j = 0; j < 5; j++)
                    {
                        isScaned[j] = true;
                    }
                    loginConfirm = true;
                    codeConfirms.Add("tm1");
                    codeConfirms.Add("tm2");
                    codeConfirms.Add("tm3");
                    codeConfirms.Add("tm4");
                    codeConfirms.Add("tm5");
                    isLogin.sprite = logOn;
                    break;
                }
            }
        }
        else
        {
            loginConfirm = false;
            codeConfirms.Clear();
            isLogin.sprite = logOff;
        }
    }

    public void WriteAccount(Text name, Text id, Text mail, Text date, Image type, SerialGroup group2)
    {
        group2.ResetAll();

        name.text = storedName;
        name.GetComponent<TextPositionController>().SetTextPosition();
        id.text = storedID;
        id.GetComponent<TextPositionController>().SetTextPosition();
        mail.text = storedMail;
        mail.GetComponent<TextPositionController>().SetTextPosition();
        date.text = storedDate;
        date.GetComponent<TextPositionController>().SetTextPosition();

        switch (storedType)
        {
            case "admin":
                type.sprite = adminImage;
                break;

            case "memberA":
                type.sprite = memberAImage;
                break;

            case "memberB":
                type.sprite = memberBImage;
                break;

            case "memberC":
                type.sprite = memberCImage;
                break;

            case "normal":
                type.sprite = normalImage;
                break;
        }

        bool someCreated = false;

        for (int i = 0; i < storedSerial.Count; i++)
        {
            if (storedSerial[i].book.Equals("tm1") || storedSerial[i].book.Equals("tm2") || storedSerial[i].book.Equals("tm3") || storedSerial[i].book.Equals("tm4")
                || storedSerial[i].book.Equals("tm_full") || storedSerial[i].book.Contains("member"))
            {
                string code = storedSerial[i].code;
                string book = storedSerial[i].book;
                bool existA = false;
                string deviceAIden = string.Empty;
                string deviceA = string.Empty;
                string deviceADate = string.Empty;

                if (storedSerial[i].deviceAExist)
                {
                    existA = true;
                    deviceAIden = storedSerial[i].deviceA[1];
                    deviceA = storedSerial[i].deviceA[2];
                    deviceADate = storedSerial[i].deviceA[3];
                }

                bool existB = false;
                string deviceBIden = string.Empty;
                string deviceB = string.Empty;
                string deviceBDate = string.Empty;

                if (storedSerial[i].deviceBExist)
                {
                    existB = true;
                    deviceBIden = storedSerial[i].deviceB[1];
                    deviceB = storedSerial[i].deviceB[2];
                    deviceBDate = storedSerial[i].deviceB[3];
                }

                if (!existA && existB)
                {
                    existA = existB;
                    deviceAIden = deviceBIden;
                    deviceA = deviceB;
                    deviceADate = deviceBDate;

                    existB = false;
                    deviceBIden = string.Empty;
                    deviceB = string.Empty;
                    deviceBDate = string.Empty;
                }

                if (storedSerial[i].isUsing)
                {
                    if (deviceAIden == SystemInfo.deviceUniqueIdentifier)
                    {
                        group2.Create(book, code, existA, deviceAIden, deviceA, deviceADate, existB, deviceBIden, deviceB, deviceBDate, true, true);
                        someCreated = true;
                        continue;
                    }
                    if (deviceBIden == SystemInfo.deviceUniqueIdentifier)
                    {
                        group2.Create(book, code, existA, deviceAIden, deviceA, deviceADate, existB, deviceBIden, deviceB, deviceBDate, true, false);
                        someCreated = true;
                        continue;
                    }
                }
                else
                {
                    group2.Create(book, code, existA, deviceAIden, deviceA, deviceADate, existB, deviceBIden, deviceB, deviceBDate, false, false);
                    someCreated = true;
                }
            }
        }

        group2.SizeScaler(someCreated);
    }

    //=====================로그인
    public IEnumerator StartLogin(string id, string pw, string title, System.Action<string> returned)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("user_pwd", pw);
        form.AddField("title", title);
        form.AddField("device_code", SystemInfo.deviceUniqueIdentifier);

        //List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        //form.Add(new MultipartFormDataSection("user_id", id));
        //form.Add(new MultipartFormDataSection("user_pwd", pw));
        //form.Add(new MultipartFormDataSection("title", title));
        //form.Add(new MultipartFormDataSection("device_code", SystemInfo.deviceUniqueIdentifier));

        UnityWebRequest www = UnityWebRequest.Post("http://bookplusapp.com/unity_api/login.php", form);
        yield return www.SendWebRequest();

        string response = www.downloadHandler.text;

        if (www.isNetworkError || www.isHttpError)
        {
            returned("error");
        }
        else
        {
            switch (response)
            {
                case "Wrong Password":
                    //Alert
                    break;
                case "Wrong UserID":
                    //Alert
                    break;
                case "Email has not been verified":
                    break;

                default:
                    if (response.Contains("Login Successfully "))
                    {
                        isLogined = true;
                        List<string> wwwList = response.Split('\n').ToList();

                        bool isAdmin = false;
                        string name = string.Empty;
                        string mail = string.Empty;
                        string date = string.Empty;
                        string type = string.Empty;
                        List<SerialField> serials = new List<SerialField>();

                        for (int i = 0; i < wwwList.Count; i++)
                        {
                            if (wwwList[i].Contains("Admin account verified"))
                            {
                                isAdmin = true;
                                storedAdmin = true;
                                break;
                            }

                            if (wwwList[i].Contains("User Name: "))
                            {
                                name = wwwList[i].Replace("User Name: ", string.Empty);
                                continue;
                            }

                            if (wwwList[i].Contains("User email: "))
                            {
                                mail = wwwList[i].Replace("User email: ", string.Empty);
                                continue;
                            }

                            if (wwwList[i].Contains("Register Date: "))
                            {
                                string removeUnder = wwwList[i].Replace("Register Date: ", string.Empty);
                                removeUnder = removeUnder.Remove(removeUnder.IndexOf(' '));
                                date = removeUnder;
                                continue;
                            }

                            if (wwwList[i].Contains("MemberType: "))
                            {
                                type = wwwList[i].Replace("MemberType: ", string.Empty);
                                continue;
                            }

                            if (wwwList[i].Contains("Connected Serial Code: "))
                            {
                                int count = int.Parse(wwwList[i].Substring(0, 1));
                                string code = wwwList[i].Replace(count.ToString() + "Connected Serial Code: ", string.Empty);
                                string bookType = wwwList[i + 1].Replace(count.ToString() + "Title: ", string.Empty);

                                if (wwwList.Count > i + 4 && wwwList[i + 4].Contains(count.ToString() + "Registered DeviceB: "))
                                {
                                    string[] listA = wwwList[i + 2].Split(':');
                                    string[] listB = wwwList[i + 4].Split(':');

                                    string listAID = listA[1].Remove(0, 1).Replace(" DeviceType", string.Empty);
                                    string listAName = listA[2].Remove(0, 1).Replace(" RegisterDate", string.Empty);
                                    string listADate = wwwList[i + 3].Replace(count.ToString() + "RegisterDate DeviceA:  ", string.Empty).Split(' ')[0];
                                    string listBID = listB[1].Remove(0, 1).Replace(" DeviceType", string.Empty);
                                    string listBName = listB[2].Remove(0, 1).Replace(" RegisterDate", string.Empty);
                                    string listBDate = wwwList[i + 5].Replace(count.ToString() + "RegisterDate DeviceB:  ", string.Empty).Split(' ')[0];

                                    string[] deviceA = new string[4]
                                    {
                                    "A",
                                    listAID,
                                    listAName,
                                    listADate
                                    };
                                    string[] deviceB = new string[4]
                                    {
                                    "B",
                                    listBID,
                                    listBName,
                                    listBDate
                                    };

                                    bool isUsing = false;

                                    if (deviceA[1] == SystemInfo.deviceUniqueIdentifier || deviceB[1] == SystemInfo.deviceUniqueIdentifier)
                                    {
                                        isUsing = true;
                                    }

                                    serials.Add(new SerialField(isUsing, code, bookType, deviceA, deviceB));
                                }
                                else if (wwwList.Count > i + 3 && wwwList[i + 2].Contains(count.ToString() + "Registered DeviceA: "))
                                {
                                    string[] listA = wwwList[i + 2].Split(':');

                                    string listAID = listA[1].Remove(0, 1).Replace(" DeviceType", string.Empty);
                                    string listAName = listA[2].Remove(0, 1).Replace(" RegisterDate", string.Empty);
                                    string listADate = wwwList[i + 3].Replace(count.ToString() + "RegisterDate DeviceA:  ", string.Empty).Split(' ')[0];

                                    string[] deviceA = new string[4]
                                    {
                                    "A",
                                    listAID,
                                    listAName,
                                    listADate
                                    };

                                    bool isUsing = false;

                                    if (deviceA[1] == SystemInfo.deviceUniqueIdentifier)
                                    {
                                        isUsing = true;
                                    }

                                    serials.Add(new SerialField(isUsing, code, bookType, deviceA, true));
                                }
                                else if (wwwList.Count > i + 3 && wwwList[i + 2].Contains(count.ToString() + "Registered DeviceB: "))
                                {
                                    string[] listB = wwwList[i + 2].Split(':');

                                    string listBID = listB[1].Remove(0, 1).Replace(" DeviceType", string.Empty);
                                    string listBName = listB[2].Remove(0, 1).Replace(" RegisterDate", string.Empty);
                                    string listBDate = wwwList[i + 3].Replace(count.ToString() + "RegisterDate DeviceB:  ", string.Empty).Split(' ')[0];

                                    string[] deviceB = new string[4]
                                    {
                                    "B",
                                    listBID,
                                    listBName,
                                    listBDate
                                    };

                                    bool isUsing = false;

                                    if (deviceB[1] == SystemInfo.deviceUniqueIdentifier)
                                    {
                                        isUsing = true;
                                    }

                                    serials.Add(new SerialField(isUsing, code, bookType, deviceB, false));
                                }
                                else
                                {
                                    serials.Add(new SerialField(code, bookType));
                                }
                            }
                        }
                        SaveLoginFile(id, pw, name, mail, date, type, isAdmin, serials);
                    }
                    break;
            }
        }
        Debug.Log(response);

        CheckRegister();
        returned(response);
    }

    //=====================회원가입
    public IEnumerator StartSignUp(string id, string pw, string name, string email, System.Action<string> returned)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", title);
        form.AddField("username", id);
        form.AddField("password", pw);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("submitted", "submit");

        UnityWebRequest www = UnityWebRequest.Post("http://bookplusapp.com/unity_api/register.php", form);
        yield return www.SendWebRequest();

        string response = www.downloadHandler.text;

        if (www.isNetworkError || www.isHttpError)
        {
            returned("error");
        }
        else
        {
            string returnMsg = "";

            if (response.Contains("Invalid ID format"))
            {
                returnMsg = "Invalid_ID_format";
            }
            else if (response.Contains("a valid email value"))
            {
                returnMsg = "Invalid_email_format";
            }
            else if (response.Contains("This ID is already used"))
            {
                returnMsg = "ID_used";
            }
            else if (response.Contains("This email is already registered"))
            {
                returnMsg = "email_used";
            }
            else if (response.Contains("Successfully Registered User"))
            {
                returnMsg = "Success_Regist";
            }
            CheckRegister();
            returned(returnMsg);
        }
        //Debug.Log(response);
    }


    //멤버등급 Normal회원 - QR코드 스캔등록
    public IEnumerator StartAuthorization(string id, string pw, string code, string type, System.Action<string> returned)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("serial_code", code);
        form.AddField("member_type", type);
        form.AddField("device_code", SystemInfo.deviceUniqueIdentifier);
        form.AddField("device_name", SystemInfo.deviceModel);

        UnityWebRequest www = UnityWebRequest.Post("http://bookplusapp.com/unity_api/device_activate.php", form);
        yield return www.SendWebRequest();

        string response = www.downloadHandler.text;

        if (www.isNetworkError || www.isHttpError)
        {
            returned("error");
        }
        else
        {
            string splitted = response.Split('\n')[0];

            switch (splitted)
            {
                case "Successfully registered":
                case "The Device is already registered":
                    Text names = accountManager.names;
                    Text ids = accountManager.id;
                    Text mails = accountManager.email;
                    Text dates = accountManager.date;
                    Image types = accountManager.type;
                    SerialGroup group2 = accountManager.group2;

                    StartCoroutine(StartLogin(id, pw, title, temp =>
                    {
                        WriteAccount(names, ids, mails, dates, types, group2);
                        CheckRegister();
                        returned(response);
                    }));

                    break;

                default:
                    returned(response);
                    break;
            }
        }
        //Debug.Log(response);
        //Debug.Log(www.error);
    }

    //멤버십 가입회원 - 기기등록
    public IEnumerator AuthorizationMembership(System.Action<string> returned)
    {
        for (int i = 0; i < storedSerial.Count; i++)
        {
            //Debug.Log(storedSerial[i].code);

            StartCoroutine(StartAuthorization(storedID, storedPW, storedSerial[i].code, storedType, returning =>
            {
                returned(returning);
            }));
            break;
        }

        yield return null;
    }


    public IEnumerator UnregisterDevice(string code, string identi, string model, Action<string> returned)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", storedID);
        form.AddField("serial_code", code);
        form.AddField("device_code", identi);
        form.AddField("device_name", model);
        form.AddField("title", title);

        UnityWebRequest www = UnityWebRequest.Post("http://bookplusapp.com/unity_api/device_deactivate.php", form);
        yield return www.SendWebRequest();

        //Debug.Log(www.text);

        if (www.error != null)
        {
            returned("error");
        }
        else
        {
            StartCoroutine(StartLogin(storedID, storedPW, title, returning =>
            {
                returned(string.Empty);
            }));
        }
    }

    //로컬에 파일 세이브
    #region LOCAL_FILE_SAVE

    public void RunSaveViaData()
    {
        SaveLoginFile(storedID, storedPW, storedName, storedMail, storedDate, storedType, storedAdmin, storedSerial);
    }

    private static void SaveLoginFile(string id, string pw, string name, string mail, string date, string type, bool isAdmin, List<SerialField> serials)
    {
        //Debug.Log("                                        Save login Run");

        string path = string.Format("{0}/Used", dataFolder);

        FileStream file = File.Create(path);
        StreamWriter write = new StreamWriter(file);

        write.Write(string.Format("login:\n{0},{1},{2},{3},{4},{5}", id, pw, name, mail, date, type));

        if (isAdmin)
        {
            write.Write(string.Format("\nADMIN_ACCOUNT"));
        }
        else
        {
            for (int i = 0; i < serials.Count; i++)
            {
                write.Write(string.Format("\nusing:{0},{1}", serials[i].code, serials[i].book));

                if (serials[i].deviceAExist)
                {
                    write.Write(string.Format("\n{0},{1},{2},{3}", serials[i].deviceA[0], serials[i].deviceA[1], serials[i].deviceA[2], serials[i].deviceA[3]));
                }

                if (serials[i].deviceBExist)
                {
                    write.Write(string.Format("\n{0},{1},{2},{3}", serials[i].deviceB[0], serials[i].deviceB[1], serials[i].deviceB[2], serials[i].deviceB[3]));
                }
            }
        }

        write.Close();
        file.Close();
    }

    #endregion


    public void Logout()
    {
        isLogined = false;
        for (int i = 0; i < 5; i++)
        {
            isScaned[i] = false;
        }
        string path = string.Format("{0}/Used", dataFolder);
        if (File.Exists(path))
            File.Delete(path);

        string qrPath = string.Format("{0}/qrToast", dataFolder);
        if (File.Exists(qrPath))
            File.Delete(qrPath);

        SaveCurrentUiLanguage();
        canvasManager.isNotToastAgain = false;
        CheckRegister();
    }


    //스캔한 타겟 이름 로컬파일로 기억~!!
    public void SaveOnTrackingObject()
    {
        string path = string.Format("{0}/TrackingObj", dataFolder);
        StreamWriter file = File.CreateText(path);
        for (int i = 0; i < objName.Count; i++)
        {
            file.Write(objName[i] + "\n");
        }
        file.Close();
    }

    //현재 ui언어 로컬파일로 기억!!
    public void SaveCurrentUiLanguage()
    {
        string langPath = string.Format("{0}/UiLanguage", dataFolder);
        StreamWriter langFile = File.CreateText(langPath);
        langFile.Write(string.Format("{0},{1},{2}", canvasManager.ui_CurrentLang, canvasManager.isStartLangPanel, accountManager.isQRInfoPanel));
        langFile.Close();
    }

    //qr토스트 그만보기로컬저장용
    public void NotToastAgain(bool isSave)
    {
        string qrPath = string.Format("{0}/qrToast", dataFolder);

        if (isSave)
        {
            StreamWriter qrToastFile = File.CreateText(qrPath);
            qrToastFile.Write(canvasManager.isNotToastAgain);
            qrToastFile.Close();
        }
        else
        {
            if (File.Exists(qrPath))
                canvasManager.isNotToastAgain = Convert.ToBoolean(File.ReadAllText(qrPath));
            else
                canvasManager.isNotToastAgain = false;
        }
    }

    //caution 그만보기로컬저장용
    public void NotCautionMsg(bool on, bool value)
    {
        string path = string.Format("{0}/caution", dataFolder);

        if (on) //Save
        {
            StreamWriter cautionFile = File.CreateText(path);
            cautionFile.Write(value);
            cautionFile.Close();
        }
        else //Read
        {
            if (File.Exists(path))
                canvasManager.isNotCautionAgain = Convert.ToBoolean(File.ReadAllText(path));
            else
                canvasManager.isNotCautionAgain = false;
        }
    }
}

#region SERIALFIELD

[System.Serializable]
public class SerialField
{
    public bool isUsing;
    public string code;
    public string book;
    public bool deviceAExist;
    public string[] deviceA;
    public bool deviceBExist;
    public string[] deviceB;

    public SerialField(string _code, string _book)
    {
        isUsing = false;
        code = _code;
        book = _book;
        deviceAExist = false;
        deviceA = new string[4];
        deviceBExist = false;
        deviceB = new string[4];
    }

    public SerialField(bool _isUsing, string _code, string _book, string[] _device, bool isDeviceA)
    {
        isUsing = _isUsing;
        code = _code;
        book = _book;
        if (isDeviceA)
        {
            deviceAExist = true;
            deviceA = _device;
            deviceBExist = false;
            deviceB = new string[4];
        }
        else
        {
            deviceBExist = true;
            deviceB = _device;
            deviceAExist = false;
            deviceA = new string[4];
        }
    }

    public SerialField(bool _isUsing, string _code, string _book, string[] _deviceA, string[] _deviceB)
    {
        isUsing = _isUsing;
        code = _code;
        book = _book;
        deviceAExist = true;
        deviceA = _deviceA;
        deviceBExist = true;
        deviceB = _deviceB;
    }
}
#endregion     //SERIALFIELD