using UnityEngine;

public class LogoSize : MonoBehaviour
{
    public RectTransform logo, mid;

    float logoPosY;
    float midPosY;

    void Awake()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;

        if (0 < windowAspect && windowAspect < 0.49f)
        { // 412x846 갤럭시S9
            logoPosY = -450f;
            midPosY = 100f;
        }
        else if (0.49f <= windowAspect && windowAspect < 0.57f)
        { // 9:16  = 0.5625
            logoPosY = -450f;
            midPosY = 50f;
        }
        else if (0.57f <= windowAspect && windowAspect < 0.65f)
        { // 10:16 = 0.625
            logoPosY = -420f;
            midPosY = 0f;
        }
        else
        {  // 3:4 = 0.75
            logoPosY = -370f;
            midPosY = -50f;
        }
    }

    private void Start()
    {
        logo.anchoredPosition = new Vector2(logo.anchoredPosition.x, logoPosY);
        mid.anchoredPosition = new Vector2(logo.anchoredPosition.x, midPosY);
    }
}
