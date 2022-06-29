using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PermissionController : MonoBehaviour
{
    public Button btn_yes;
    public Text perTxt;

    private int index;
    private readonly string[] texts =
        { "Please allow following permission to use the app 'Tagme3D'\n- Camera(required)\n- Storage(required)\n- Microphone(required)",
        "태그미3D 앱을 이용하기 위해서는 다음의 접근 권한을 요청합니다.\n- 카메라(필수)\n- 저장공간(필수)\n- 마이크(필수)" };
    private readonly string[] btnTexts = { "Permission Reqeust", "권한 요청" };
    private readonly string[] permissionList = { Permission.Camera, Permission.Microphone, Permission.ExternalStorageWrite };

    private void Awake()
    {
        if (Application.systemLanguage.Equals(SystemLanguage.Korean))
            index = 1;
        else
            index = 0;

        perTxt.text = texts[index];
        btn_yes.GetComponentInChildren<Text>().text = btnTexts[index];

        bool allConfirm = true;
        for (int i = 0; i < 3; i++)
        {
            if (!Check(permissionList[i]))
            {
                allConfirm = false;
                break;
            }
        }

        if (allConfirm)
            SceneManager.LoadScene("Splash");
    }

    public void OnClick()
    {
        StartCoroutine(StartPemission());
    }

    IEnumerator StartPemission()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!Permission.HasUserAuthorizedPermission(permissionList[0]))
        {
            Permission.RequestUserPermission(permissionList[0]);
        }
        yield return new WaitUntil(() => Check(permissionList[0]));

        if (!Permission.HasUserAuthorizedPermission(permissionList[1]))
        {
            Permission.RequestUserPermission(permissionList[1]);
        }
        yield return new WaitUntil(() => Check(permissionList[1]));

        if (!Permission.HasUserAuthorizedPermission(permissionList[2]))
        {
            Permission.RequestUserPermission(permissionList[2]);
        }
        yield return new WaitUntil(() => Check(permissionList[2]));

        SceneManager.LoadScene("SPlash");
#endif
        //아래두줄 임시조치입니다 0627
        //SceneManager.LoadScene("SPlash");
        //return null;
    }

    private bool Check(string what)
    {
        if (Permission.HasUserAuthorizedPermission(what))
            return true;
        else
        {
            StopCoroutine(StartPemission());
            return false;
        }
    }
}
