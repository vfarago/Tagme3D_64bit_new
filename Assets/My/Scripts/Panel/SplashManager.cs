using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashManager : MonoBehaviour
{
    public Image progressBar;
    public Text txt_version;

    IEnumerator Start()
    {
        yield return null;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        txt_version.text = string.Format("v.{0}", Application.version);

        AsyncOperation op = SceneManager.LoadSceneAsync("TMPeriscope");
        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if (op.progress >= 0.9f)
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer) * 0.5f;

                if (progressBar.fillAmount.Equals(0.5f))
                    op.allowSceneActivation = true;
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer) * 0.5f;
                if (progressBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
        }
    }
}
