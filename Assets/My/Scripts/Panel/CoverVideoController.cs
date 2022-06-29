using I2.Loc;
using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CoverVideoController : MonoBehaviour
{
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern void goToStudyViewController(string obj_name);
#endif
    CanvasManager canvasManager;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    public MeshRenderer screen;
    private Text errorMsg;

    private void OnEnable()
    {
        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged += onPanStateChanged;
        }
        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged += onScaleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged -= onPanStateChanged;
        }
        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged -= onScaleStateChanged;
        }
        StopAllCoroutines();
        videoPlayer.Stop();
        audioSource.Stop();
    }


    void Start()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        errorMsg = canvasManager.arPanel.transform.GetChild(2).GetComponent<Text>();

        errorMsg.text = string.Empty;
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(PlayVideo());
    }


    //__________Play Video, Audio and Recording
    IEnumerator PlayVideo()
    {
        videoPlayer.playOnAwake = false;
        audioSource.playOnAwake = false;

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = Resources.Load<VideoClip>("prefabs/tm_intro_video");

        errorMsg.text = LocalizationManager.GetTermTranslation("UI_coverScanText");
        errorMsg.font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

        videoPlayer.errorReceived += VideoPlayer_errorReceived;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
        audioSource.Pause();

        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = screen;

        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Play();
        audioSource.Play();

        errorMsg.text = string.Empty;
    }

    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        errorMsg.text = "Video connection failed.";
        errorMsg.font = Resources.Load<Font>("fonts/baloo-regular");
    }


    private void onPanStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var gesture = (PanGesture)sender;

                //2nd attempt
                if (gesture.WorldDeltaPosition != Vector3.zero)
                {
                    if (Math.Abs(gesture.WorldDeltaPosition.x) > Math.Abs(gesture.WorldDeltaPosition.z))
                    {//horizontal
                        transform.Rotate(0, 0, -gesture.WorldDeltaPosition.x * 5f, Space.World);
                    }
                    else
                    {
                        transform.Rotate(gesture.WorldDeltaPosition.z * 5f, 0, 0, Space.World);
                    }
                }

                break;
        }
    }

    private void onScaleStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        float objectScale = 1;
        float MinScale = 0.5f;
        float MaxScale = 2.5f;

        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:

                var gesture = (ScaleGesture)sender;

                float localDeltaScale = gesture.LocalDeltaScale;

                //scaling
                float currentScale = transform.localScale.x;
                if (localDeltaScale >= 1f)
                    currentScale *= (1 + (objectScale * 0.05f));
                else
                    currentScale *= (1 - (objectScale * 0.05f));

                currentScale = Mathf.Clamp(currentScale, MinScale, MaxScale);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);



                break;
        }
    }
}
