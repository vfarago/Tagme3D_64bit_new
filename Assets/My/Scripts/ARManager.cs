using UnityEngine;
using Vuforia;
using System;
using System.Collections;

public class ARManager : MonoBehaviour
{
    private static ARManager instance;

    public enum State { IDLE, ARState, StudyViewState };
    public enum HintState { ZERO, SINGLE, MULTI };

    public State state;
    public Camera arCam;
    public Camera StudyViewCamera;

    public bool isFrontCamera;
    public HintState hintState = HintState.ZERO;

    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;

    public static ARManager Instance
    {
        get
        {
            if (instance == null) //Finds the instance if it doesn't exist
                instance = GameObject.FindObjectOfType<ARManager>();

            return instance;
        }
    }


    void Start()
    {
        //ARCamera.enabled = true;
        StudyViewCamera.enabled = false;
        state = State.IDLE;
        isFrontCamera = false;
        //TurnOnAR(false, false);
        setHintZero();
        VuforiaApplication.Instance.OnVuforiaStopped += () => { checkStop = true; };
    }


    // ARRenderHint [start]
    public void setHintMulti()
    {
        //first stop ObjectTracker
        //bool needsObjectTrackerRestart = stopRunningObjectTracker();

        //바꿔야합니다0627
        //VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 10);

        //  Elon 220630
        VuforiaBehaviour.Instance.SetMaximumSimultaneousTrackedImages(10);
        //vuforiaBehaviour.SetMaximumSimultaneousTrackedImages(10);


        hintState = HintState.MULTI;

        //finally restart ObjectTracker
        //if (needsObjectTrackerRestart)
        //    restartRunningObjectTracker();
    }


    public void setHintSingle()
    {
        //first stop ObjectTracker
        //bool needsObjectTrackerRestart = stopRunningObjectTracker();

        hintState = HintState.SINGLE;
        //바꿔야합니다0627
        //VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 1);

        //  Elon 220630
        VuforiaBehaviour.Instance.SetMaximumSimultaneousTrackedImages(1);
        //vuforiaBehaviour.SetMaximumSimultaneousTrackedImages(1);

        //if (needsObjectTrackerRestart)
        //    restartRunningObjectTracker();
    }


    public void setHintZero()
    {
        //first stop ObjectTracker
        //bool needsObjectTrackerRestart = stopRunningObjectTracker();
        //stopRunningObjectTracker();

        hintState = HintState.ZERO;
        //바꿔야합니다0627
        //VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 0); //it doesn't seem to work

        //  Elon 220630
        //VuforiaBehaviour.Instance.SetMaximumSimultaneousTrackedImages(0);

        ////finally restart ObjectTracker
        //if (needsObjectTrackerRestart)
        //    restartRunningObjectTracker();
    }



    private bool stopRunningObjectTracker()
    {
        //bool needsObjectTrackerRestart = false;

        //VuforiaBehaviour.Instance.enabled = false;
        ////바꿔야합니다0627
        //ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        //if (objectTracker != null)
        //{
        //    if (objectTracker.IsActive)
        //    {
        //        objectTracker.Stop();
        //        needsObjectTrackerRestart = true;
        //    }
        //}
        //return needsObjectTrackerRestart;
        return false;
    }



    private bool restartRunningObjectTracker()
    {
        bool hasObjectTrackerRestarted = false;

        //바꿔야합니다0627
        //ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        //if (objectTracker != null)
        //{
        //    if (!objectTracker.IsActive)
        //    {
        //        hasObjectTrackerRestarted = objectTracker.Start();
        //    }
        //}
        return hasObjectTrackerRestarted;
    }

    // ARRenderHint [end]



    public void ChangeCamera(string destCamera)
    {
        if (destCamera.Equals("ARCamera"))
        {
            TurnOnAR(true, true);
            StudyViewCamera.enabled = false;
            arCam.enabled = true;
            state = State.ARState;
        }
        else
        { //MainCamera
            TurnOnAR(false, false);
            arCam.enabled = false;
            StudyViewCamera.enabled = true;
            state = State.StudyViewState;
        }

    }

    public void UseVuforiaCam(Action action)
    {
        VuforiaBehaviour.Instance.enabled = true;
    }
    public void UseWebCam(Action done)
    {
        Debug.LogError("check");
        StartCoroutine(Cor_WaitToWebcamChange(done));
    }
    IEnumerator Cor_WaitToWebcamChange(Action done)
    {
        VuforiaBehaviour.Instance.enabled = false;
        while (checkStop)
        {
            yield return new WaitForEndOfFrame();
        }
        done();
        checkStop = false;
    }


    bool checkStop = false;

    public void UseWebCam(QRCodeReaderDemo demo)
    {
        VuforiaBehaviour.Instance.enabled = false;
    }
    public void changeCameraStateToIdle()
    {
        state = State.IDLE;
        setHintZero();
    }


    //안씁니다0627
    //https://developer.vuforia.com/forum/faq/unity-how-select-camera-and-mirroring
    //public void UseFrontCamera(bool ready)
    //{

    //    if (ready) //Front Camera = true
    //    {
    //        isFrontCamera = true;

    //        // turn off the curent camera : the back camera
    //        Vuforia.CameraDevice.Instance.Stop();
    //        Vuforia.CameraDevice.Instance.Deinit();

    //        // turn on the front camera
    //        Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_FRONT);
    //        Vuforia.CameraDevice.Instance.Start();

    //        //turn on the mirroring
    //        Vuforia.VuforiaRenderer.VideoBGCfgData config = Vuforia.VuforiaRenderer.Instance.GetVideoBackgroundConfig();
    //        config.reflection = Vuforia.VuforiaRenderer.VideoBackgroundReflection.ON;
    //        Vuforia.VuforiaRenderer.Instance.SetVideoBackgroundConfig(config);

    //        //Debug.Log("UseFrontCamera");
    //        return;
    //    }
    //    else //Back Camera = false
    //    {
    //        isFrontCamera = false;

    //        // turn off the curent camera : the front camera
    //        Vuforia.CameraDevice.Instance.Stop();
    //        Vuforia.CameraDevice.Instance.Deinit();

    //        // turn on the back camera
    //        Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_BACK);
    //        Vuforia.CameraDevice.Instance.Start();

    //        //turn off the mirroring
    //        Vuforia.VuforiaRenderer.VideoBGCfgData config = Vuforia.VuforiaRenderer.Instance.GetVideoBackgroundConfig();
    //        config.reflection = Vuforia.VuforiaRenderer.VideoBackgroundReflection.OFF;
    //        Vuforia.VuforiaRenderer.Instance.SetVideoBackgroundConfig(config);

    //        //Debug.Log("UseBackCamera");
    //        return;
    //    }
    //}

    public void TurnOnAR(bool use, bool isStop)
    {
        //바꿔야합니다0627
        //if (use)
        //{
        //    ActivateDataSet(isStop);
        //    // turn on the front camera
        //    if (isFrontCamera)
        //        Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_FRONT);
        //    else
        //        Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_BACK);
        //    Vuforia.CameraDevice.Instance.Start();
        //}
        //else
        //{
        //    ActivateDataSet(false);
        //    // turn off the curent camera 
        //    Vuforia.CameraDevice.Instance.Stop();
        //    Vuforia.CameraDevice.Instance.Deinit();
        //}
    }


    public void ActivateDataSet(bool activate)
    {
        //바꿔야합니다0627
        //Vuforia.ObjectTracker objectTracker = Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>();

        //if (activate)
        //{
        //    objectTracker.Start();
        //}
        //else
        //{
        //    objectTracker.Stop();
        //}
        //print("               Tracking  " + objectTracker.IsActive);
    }

}
