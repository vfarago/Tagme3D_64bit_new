using UnityEngine;
using Vuforia;

//public class TrackableEventHandler : MonoBehaviour, ITrackableEventHandler
public class TrackableEventHandler : DefaultObserverEventHandler
{
    //protected TrackableBehaviour mTrackableBehaviour;
    protected ObserverBehaviour mTrackableBehaviour;
    //protected TrackableBehaviour.Status currentStatus;

    protected ARManager arManager;
    protected CanvasManager canvasManager;
    protected PrefabLoader prefabLoader;
    protected PrefabShelter prefabShelter;

    protected virtual void Awake()
    {
        arManager = FindObjectOfType<ARManager>();
        canvasManager = Manager.CanvasManager;
        prefabLoader = Manager.PrefabLoader;
        prefabShelter = Manager.PrefabShelter;

        //mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        mTrackableBehaviour = GetComponent<ObserverBehaviour>();
        if (mTrackableBehaviour)
        //{
        //    mTrackableBehaviour.RegisterTrackableEventHandler(this);
        //}
        //currentStatus = TrackableBehaviour.Status.DETECTED;
        OnTrackingLost();
    }


    //바꿔야합니다0627
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

    protected virtual void OnTrackingFound()
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


    protected virtual void OnTrackingLost()
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