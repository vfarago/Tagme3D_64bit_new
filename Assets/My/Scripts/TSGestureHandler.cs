using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;
using Vuforia;
//using System.Runtime.InteropServices;

public class TSGestureHandler : MonoBehaviour
{
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern void goToStudyViewController(string obj_name);
#endif

    CanvasManager canvasManager;
    PrefabLoader prefabLoader;
    float onTargetScale;
    float objectScale;
    float maxScale;
    float minScale;

    //바꿔야합니다0627
    //public TrackableBehaviour mTrackableBehaviour;
    public string targetName;
    public bool isFreeModel;
    public int counter = 0;

    void Start()
    {
        prefabLoader = Manager.PrefabLoader;
        canvasManager = Manager.CanvasManager;
        onTargetScale = transform.localScale.x;
        
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
    }

    #region ENABLE_and_DISABLE
    private void OnEnable()
    {
        if (GetComponent<TapGesture>() != null)
        {
            GetComponent<TapGesture>().Tapped += TappedHandler;
        }

        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged += OnScaleStateChanged;
        }

        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged += OnPanStateChanged;
        }

    }

    private void OnDisable()
    {
        if (GetComponent<TapGesture>() != null)
        {
            GetComponent<TapGesture>().Tapped -= TappedHandler;
        }
        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged -= OnScaleStateChanged;
        }
        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged -= OnPanStateChanged;
        }
    }
    #endregion


    private void TappedHandler(object sender, EventArgs e)
    {

#if UNITY_IPHONE && !UNITY_EDITOR
			goToStudyViewController (obj_name);
#endif
        counter++;

        if (counter == 1)
            StartCoroutine(DoubleTapEvent());
    }
    // 터치→ 오브젝트 타겟 분리, 더블탭→ 파닉스전환
    IEnumerator DoubleTapEvent()
    {
        yield return new WaitForSeconds(0.5f);
        if (counter > 1)
        {
            yield return new WaitForSeconds(0.02f);

            //camera changer
            ARManager.Instance.ChangeCamera("MainCamera");
            ARManager.Instance.setHintZero();

            prefabLoader.ChangePrefab(targetName, isFreeModel);

            canvasManager.OnPhonicsPanel(true);
        }
        else
        {
            prefabLoader.TargetOffMoving(gameObject);
            canvasManager.OnTargetOffObject(true);
            //바꿔야합니다0627
            //mTrackableBehaviour.OnTrackerUpdate(TrackableBehaviour.Status.NOT_FOUND);
        }

        yield return new WaitForSeconds(0.05f);
        prefabLoader.ModelFalse();

        counter = 0;
    }
    public IEnumerator MRDoubleTapEvent()
    {
        yield return new WaitForSeconds(0.5f);
        {
            prefabLoader.TargetOffMoving(gameObject);
            canvasManager.OnTargetOffObject(true);
            //바꿔야합니다0627
            //mTrackableBehaviour.OnTrackerUpdate(TrackableBehaviour.Status.NOT_FOUND);
        }

        yield return new WaitForSeconds(0.05f);
        prefabLoader.ModelFalse();

        counter = 0;
    }

    private void OnPanStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        //if (!Enabled)
        //{
        //    return;
        //}

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
                        transform.Rotate(0, 0, -gesture.WorldDeltaPosition.x * 1.3f, Space.World);
                    }
                    else
                    {
                        transform.Rotate(gesture.WorldDeltaPosition.z * 1.3f, 0, 0, Space.World);
                    }
                }

                break;
        }
    }



    private void OnScaleStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        //if (!Enabled)
        //{
        //    return;
        //}
        float scaleSpeed;

        if (prefabLoader.isTargetoff)
        {
            objectScale = 1;
            scaleSpeed = 0.05f;
            minScale = 50f;
            maxScale = 250f;
        }
        else
        {
            objectScale = onTargetScale;
            scaleSpeed = 1.5f;
            minScale = 0.05f;
            maxScale = 2.0f;
        }

        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:

                var gesture = (ScaleGesture)sender;

                float localDeltaScale = gesture.LocalDeltaScale;
                //float objectScale = transform.localScale.x;

                //scaling
                float currentScale = transform.localScale.x;
                if (localDeltaScale >= 1f)
                    currentScale *= (1 + (objectScale * scaleSpeed));
                else
                    currentScale *= (1 - (objectScale * scaleSpeed));

                currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);



                break;
        }
    }

}
