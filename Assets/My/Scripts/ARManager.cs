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

    Action hookVuOn;
    Action hookVuOff;
    [SerializeField] VuforiaBehaviour[] vb;

    public bool isMR = false;

    public static ARManager Instance
    {
        get
        {
            VuforiaApplication.Instance.OnVuforiaInitialized += (x) => { VuforiaSubscribeSetting(); };
            if (instance == null) //Finds the instance if it doesn't exist
                instance = GameObject.FindObjectOfType<ARManager>();

            return instance;
        }
    }
    private void Awake()
    {
        //vb = FindObjectsOfType<VuforiaBehaviour>();
        //foreach(var item in vb)
        //{
        //    item.
        //}
        hookVuOn += () =>  checkStop = false;
        hookVuOff += () => checkStop = true;  
    }

    void Start()
    {
        vb = FindObjectsOfType<VuforiaBehaviour>(true);
        //ARCamera.enabled = true;
        StudyViewCamera.enabled = false;
        state = State.IDLE;
        isFrontCamera = false;
        //TurnOnAR(false, false);
        setHintZero();

    }
    static void VuforiaSubscribeSetting()//static 작성 사유. 이친구가 생겼다가 없어졌다가 하는지 온스탑 온스타트가 제대로 걸리지가 않아서 작성하였음 brendan220706
    {
        VuforiaApplication.Instance.OnVuforiaStopped += () => { checkStop = true;/* Debug.LogError(checkStop); */};
        VuforiaApplication.Instance.OnVuforiaStarted += () => { checkStop = false;/* Debug.LogError(checkStop);*/ };
        if (VuforiaApplication.Instance.GetVuforiaBehaviour() != null)
        {
            checkStop = !VuforiaApplication.Instance.GetVuforiaBehaviour().enabled;
        }
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
    Coroutine cur_Cor;
    public void UseVuforiaCam(Action done)
    {
        if (cur_Cor != null) return;
        cur_Cor=StartCoroutine(Cor_WaitToVucamChange(done));
    }
    public void UseWebCam(Action done)
    {
        if (cur_Cor != null) return;
        cur_Cor =StartCoroutine(Cor_WaitToWebcamChange(done));
    }
    IEnumerator Cor_WaitToWebcamChange(Action done)
    {
        if (!VuforiaBehaviour.Instance.enabled)//이미 꺼져있다면 패스
        {
            done();
            checkStop = true;
            cur_Cor = null;
            yield break;
        }
        VuforiaApplication.Instance.OnVuforiaStopped += hookVuOff;
        VuforiaApplication.Instance.GetVuforiaBehaviour().enabled = false;
        while (VuforiaApplication.Instance.GetVuforiaBehaviour().enabled)
        {
            print("roknroll");
            yield return new WaitForSeconds(1);
        }
        VuforiaApplication.Instance.OnVuforiaStopped -= hookVuOff;
        done();
        checkStop = true;
        cur_Cor = null;
    }
    IEnumerator Cor_WaitToVucamChange(Action done)
    {
        if(ContentsLocker.isActivate)
            ContentsLocker.Instance.isChecked = false;//스캔들어갈때 첫번째도 그룹인원 체크하기 위함.
        if (VuforiaBehaviour.Instance.enabled)//이미 켜져있다면 패스
        {
            done();
            checkStop = false;
            cur_Cor = null;
            yield break;
        }
        VuforiaApplication.Instance.OnVuforiaStarted += hookVuOn;
        VuforiaApplication.Instance.GetVuforiaBehaviour().enabled = true;
        while (!VuforiaApplication.Instance.GetVuforiaBehaviour().enabled)
        {
            print("roknroll");
            yield return new WaitForSeconds(1);
        }
        VuforiaApplication.Instance.OnVuforiaStarted -= hookVuOn;
        done();
        checkStop = false;
        cur_Cor = null;
    }

    static bool checkStop = false;

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
