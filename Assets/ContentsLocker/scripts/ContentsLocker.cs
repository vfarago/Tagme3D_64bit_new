using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ContentsLocker : MonoBehaviour
{
    string title = "sp";
    //차단 통과는 의존적이지 않게 작성되어야 한다. 기존의 창이 사용될경우 이 로커의 가로막기 오브젝트는 생성되지 않는다.
    public Action<bool> fncBlock;
    //그룹아이디일때만 작동해야 한다. 큐알로그인에 성공한후 작동한다.
    public static bool isActivate;//인스턴스는 부르자마자 나타나므로 인스턴스가 나타나지 않는 상태에서 사용유무를 검사하게 만든 변수 사용하지 않을때를 구분해주기위함. 리소스미사용시 생성금지 먼저 엑티베이트를 해야 사용할 수 있다. 큐알 로그인을 시도할때 처음 작동하게해야한다.
    [SerializeField] string uniqueCode;
    string qrCode;
    public string setQRCode { set { qrCode = value; } }
    [SerializeField] GameObject lockerPrefab;
    GameObject lockerOBJInstance;
    Button btnTrylogin;
    Text txtInfo;
    public string msgNetworkDisconnected = "네트워크가 연결되어있지 않습니다.\nwifi와 데이터 네트워크를 확인해주십시오.";
    public string msgExceededUser = "사용자 이용제한 초과로 인해 로그아웃 되었습니다.\n다른 사용자의 연결을 끊고 대신 로그인할까요?";
    public bool isChecked = false;//php인증으로 통과된 상태
    private static Lazy<ContentsLocker> _instance;
    public static ContentsLocker Instance
    {
        get
        {
            isActivate = true;//조건 만족하든 만족하지 않든 인스턴스를 생성하므로 바로 트루
            if (_instance == null || _instance.Value == null)
            {
                _instance = new Lazy<ContentsLocker>(() =>
                {
                    ContentsLocker instance = FindObjectOfType<ContentsLocker>();

                    if (instance == null)
                    {
                        GameObject obj = new GameObject("ContentsLocker");
                        instance = obj.AddComponent<ContentsLocker>();

                        DontDestroyOnLoad(obj);
                    }
                    return instance;
                }
            );


            }
            return _instance.Value;
        }
    }

    Coroutine curCor;
    Coroutine netCheckCor;

    public void StartNetcheck(Action reservation, NetworkReachability status)
    {
        if (netCheckCor == null)
            netCheckCor = StartCoroutine(CorNetCheck(reservation, status));
    }
    public void StopNetcheck()
    {
        netCheckCor = null;
        StopCoroutine("CorNetCheck");
    }
    IEnumerator CorNetCheck(Action reservation, NetworkReachability status)
    {
        while (true)
        {
            if (status != Application.internetReachability)
            {
                reservation();
                StopNetcheck();
            }
            yield return new WaitForSeconds(1);
        }
    }


    public void UnloadContentsLocker()
    {
        isActivate = false;
        StopAllCoroutines();
        curCor = null;
        if (lockerOBJInstance)
        {
            Destroy(lockerOBJInstance);
        }
        Destroy(this.gameObject);
    }

    enum NetworkMode
    {
        Checkmode,
        QRLogin
    }

    void FindButtons()
    {
        btnTrylogin = lockerOBJInstance.GetComponentInChildren<Button>();
        btnTrylogin.onClick.AddListener(() => TryLogin());
        txtInfo = lockerOBJInstance.GetComponentInChildren<Text>();
    }
    void TryLogin()
    {
        //로그인을 시도한다.
        //시도하는동안 버튼은 비활성화한다.
        btnTrylogin.interactable = false;
        //실패하면 다시한번 해보라는 안내를 발생시킨다.
        //가능하면 네트워크 오류라면 네트워크 오류라고 알려주는 친절함
        //유니티 네트워크 오류를 받아온다.
        StartLogin(qrCode, uniqueCode, (x) =>
        {
            btnTrylogin.interactable = true;
            fncBlock(false);
        }, (returnedErrorCode) =>
        {
            if (returnedErrorCode == -1)
            {
                //네트워크 오류창 표시
                txtInfo.text = msgNetworkDisconnected;
            }
            btnTrylogin.interactable = true;
        });
    }
    public void TryLogin(Action<string> success, Action<int> failed)
    {
        StartLogin(uniqueCode, qrCode, success, failed);
    }
    void Awake()
    {
        string tmstr = SystemInfo.deviceUniqueIdentifier;

        if (tmstr == string.Empty)
        {
            string key = "uniqueCode";
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, Guid.NewGuid().ToString());

                //없으면 새로 생성하고 저장한다.
            }
            uniqueCode = PlayerPrefs.GetString(key);
        }
        else
        {
            uniqueCode = tmstr;
        }

        //uniqueCode = Guid.NewGuid().ToString();
    }
    public void SecurityCheckStart()
    {
        if (qrCode == string.Empty || qrCode == null)
        {
            Debug.LogError("올바른 접근이 아닙니다! 존재하는 큐알코드가 없음");
            fncBlock(true);//올바른접근이아닙니다 텍스트를 표시한다.
        }
        else
        {
            StartCheck(uniqueCode, qrCode, () => { fncBlock(false); }, (x) => { fncBlock(true); });
        }
    }
    public void SecurityCheckStart(Action<bool> action)
    {
        if (qrCode == string.Empty || qrCode == null)
        {
            Debug.LogError("올바른 접근이 아닙니다! 존재하는 큐알코드가 없음");
            fncBlock(true);//올바른접근이아닙니다 텍스트를 표시한다.
        }
        else
        {
            StartCheck(uniqueCode, qrCode, () => { action(true); }, (x) => { action(false); });
        }
    }
    public void QRLoginStart(string qrCode, Action<string> success, Action<int> failed)
    {
        this.qrCode = qrCode;
        StartLogin(uniqueCode, qrCode, success, failed);
    }
    public void QRLoginStart(Action<string> success, Action<int> failed)
    {
        StartLogin(uniqueCode, qrCode, success, failed);
    }
    //private void OnEnable()
    //{
    //    if (fncBlock == null) fncBlock = (boolean) => { Lock(boolean); };
    //    if (lockerPrefab == null)
    //    {
    //        lockerPrefab = Resources.Load<GameObject>("prefab_contentLocker");
    //        if (lockerPrefab == null)
    //        {
    //            Debug.LogError("지정된 프리팹 없음!");
    //        }
    //    }
    //    if (lockerOBJInstance == null)
    //    {
    //        lockerOBJInstance = GameObject.Instantiate(lockerPrefab);
    //        DontDestroyOnLoad(lockerOBJInstance);
    //        lockerOBJInstance.SetActive(false);
    //    }
    //    FindButtons();
    //}
    public void StartLoginCheck(Action successAction, Action<int> faildAction)//로그아웃 상태를 확인한다.
    {
        StartCheck(uniqueCode, qrCode, successAction, faildAction);
    }
    void StartCheck(string uniqueCode, string qrCode, Action success, Action<int> fail)
    {
        if (curCor == null)
            curCor = StartCoroutine(Cor_Network(NetworkMode.Checkmode, (rtnInt, rtnstr) =>
            {
                switch (rtnInt)
                {
                    case 0000:
                        isChecked = true;
                        success();
                        break;

                    case 0001:
                        isChecked = false;
                        Debug.Log($"uniqueCode:{uniqueCode}//qrCode: {qrCode}//Error: {rtnstr}");
                        fail(rtnInt);
                        print(rtnstr);
                        break;
                    case 0003:
                        fail(rtnInt);
                        isChecked = false;
                        print(rtnstr);
                        break;
                    default:
                        print(rtnstr);
                        isChecked = false;
                        break;
                }
            }, uniqueCode, qrCode));
    }
    void StartLogin(string uniqueCode, string qrCode, Action<string> success, Action<int> fail)
    {
        if (curCor == null)
            curCor = StartCoroutine(Cor_Network(NetworkMode.QRLogin, (rtnInt, rtnstr) =>
            {
                print(rtnstr);
                switch (rtnInt)
                {
                    case 0000:
                        isChecked = true;
                        success(rtnstr);
                        break;

                    case 0002:
                        isChecked = true;
                        success(rtnstr);
                        print("aleady logined");
                        break;
                    case 0003:
                        UnloadContentsLocker();
                        fail(rtnInt);
                        print(rtnstr);
                        break;
                    case 0004:
                        print("QR에 허가되어있는 타이틀이 아님.");
                        fail(rtnInt);
                        break;
                    case -1:
                        //인터넷 에러
                        fail(rtnInt);
                        break;
                    case -2:
                        //로직 에러
                        fail(rtnInt);
                        break;
                    default:
                        UnloadContentsLocker();
                        fail(rtnInt);
                        print(rtnstr);
                        break;
                }
                //lockerOBJ.SetActive(boolean); 

            }, uniqueCode, qrCode));
    }
    void Lock(bool boolean)
    {
        //큐알 로그인에서 로그 아웃되며 알림창을 표시한다. 재 로그인 가능하게 하는 버튼을 세우고 모든 터치를 막는다.
        //모든 터치를 막는것은 이벤트시스템을 끄거나? 표시한 캔버스로 막거나 한다.
        //캔버스로 틀어막는것도 나쁘지않은것 같다.
        lockerOBJInstance.SetActive(boolean);
        //거기에 더해 맨 앞으로 나올 수 있도록 마지막줄에 들어가도록 한다.=>프리팹단계에서 캔버스 소트오더를 맨앞으로 변경.
        isChecked = !boolean;
    }
    void Login()
    {
    }
    private IEnumerator Cor_Network(NetworkMode mode, System.Action<int, string> output, string uniqueCode, string qrCode)
    {
        int returnInt = 0;
        string returnString = string.Empty;

        WWWForm form = new WWWForm();
        print(uniqueCode);
        form.AddField("uniqueCode", uniqueCode);
        form.AddField("QRCode", qrCode);
        form.AddField("title", title);
        UnityWebRequest req = null;
        switch (mode)
        {
            case NetworkMode.Checkmode:
                req = UnityWebRequest.Post("http://bookplusapp.com/unity_api/group_id_checker.php", form);
                yield return req.SendWebRequest();

                break;

            case NetworkMode.QRLogin:
                req = UnityWebRequest.Post("http://bookplusapp.com/unity_api/group_id_login.php", form);
                yield return req.SendWebRequest();


                break;
        }

        if (req.error != null)
        {
            returnString = req.error;
            if (req.isNetworkError) returnInt = -1;
            if (req.isHttpError) returnInt = -2;
        }
        else
        {
            returnString = req.downloadHandler.text;
        }
        if (!returnString.Contains("<"))
        {
            print(req.downloadHandler.text);
        }
        else
        {
            returnInt = int.Parse(returnString.Split('<')[1].Split('>')[0]);
        }
        output(returnInt, returnString);
        curCor = null;
    }
    void registQR()
    {
        //디비에 꽂아넣는건 불편하니깐 큐알체킹해서 자동으로 추가하게 하는 구문-
    }
    string GetResult(string inputText)
    {



        return null;
    }


}
