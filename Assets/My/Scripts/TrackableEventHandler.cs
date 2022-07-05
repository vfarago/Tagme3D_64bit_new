using UnityEngine;
using Vuforia;

//public class TrackableEventHandler : MonoBehaviour, ITrackableEventHandler
public class TrackableEventHandler : DefaultObserverEventHandler
{
    //protected TrackableBehaviour mTrackableBehaviour;
    //protected TrackableBehaviour.Status currentStatus;

    protected ARManager arManager;
    protected CanvasManager canvasManager;
    protected PrefabLoader prefabLoader;
    protected PrefabShelter prefabShelter;

    //  Elon
    //protected ObserverBehaviour mObserverBehaviour;
    protected Status currentStatus;
    protected StatusInfo statusInfo;
    protected virtual void Awake()
    {
        arManager = FindObjectOfType<ARManager>();
        canvasManager = Manager.CanvasManager;
        prefabLoader = Manager.PrefabLoader;
        prefabShelter = Manager.PrefabShelter;

        //mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        //if (mObserverBehaviour)
        //{
        //    mTrackableBehaviour.RegisterTrackableEventHandler(this);
        //}
        //currentStatus = TrackableBehaviour.Status.DETECTED;
        currentStatus = mObserverBehaviour.TargetStatus.Status;
        statusInfo = mObserverBehaviour.TargetStatus.StatusInfo;
        OnTrackingLost();
    }


    //바꿔야합니다0627
    //public virtual void OnTrackableStateChanged(Status previousStatus,Status newStatus)
    //{
    //    if(Status.EXTENDED_TRACKED == newStatus || Status.TRACKED == newStatus)
    //    {
    //        currentStatus = newStatus;
    //        OnTrackingFound();
    //    }
    //    else
    //    {
    //        currentStatus = newStatus;
    //        OnTrackingLost();
    //    }          
    //}

    protected override void HandleTargetStatusChanged(Status previousStatus, Status newStatus)
    {
        Debug.Log("점심시간");

        if (Status.EXTENDED_TRACKED == newStatus || Status.TRACKED == newStatus)
        {
            currentStatus = newStatus;
            OnTrackingFound();
        }
        else
        {
            currentStatus = newStatus;
            OnTrackingLost();
        }
    }




    //public virtual void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    //{
    //    if (newStatus == TrackableBehaviour.Status.DETECTED ||
    //        newStatus == TrackableBehaviour.Status.TRACKED ||
    //        newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
    //    {
    //        currentStatus = newStatus;
    //        OnTrackingFound();
    //    }
    //    else
    //    {
    //        currentStatus = newStatus;
    //        OnTrackingLost();
    //    }
    //}

    protected override void OnTrackingFound()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < rendererComponents.Length; i++)
        {
            rendererComponents[i].enabled = true;
        }

        for (int i = 0; i < colliderComponents.Length; i++)
        {
            colliderComponents[i].enabled = true;
        }
    }


    protected override void OnTrackingLost()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < rendererComponents.Length; i++)
        {
            rendererComponents[i].enabled = false;
        }

        for (int i = 0; i < colliderComponents.Length; i++)
        {
            colliderComponents[i].enabled = false;
        }
    }
}