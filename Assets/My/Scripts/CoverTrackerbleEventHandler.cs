using UnityEngine;

public class CoverTrackerbleEventHandler : TrackableEventHandler
{
    public string targetName;

    private bool isEndAR;

    protected override void Awake()
    {
        base.Awake();
        //바꿔야합니다0627
        //targetName = mTrackableBehaviour.TrackableName.ToLower();
    }

    protected override void OnTrackingFound()
    {
        isEndAR = prefabLoader.isEndAR;

        if (!isEndAR && !canvasManager.isCoverTarget)
        {
            Debug.Log("        found " + targetName);
            canvasManager.isCoverTarget = true;
            prefabLoader.isTargetoff = true;
            canvasManager.OnTargetOffObject(true);
        }
    }
}
