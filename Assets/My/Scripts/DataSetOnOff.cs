using UnityEngine;
using Vuforia;

public class DataSetOnOff : MonoBehaviour
{
    AnimalDataSetLoader adsl;

    private void Awake()
    {
        adsl = GetComponentInParent<AnimalDataSetLoader>();
    }

    public void TrackingActiveOn(bool on)
    {
        if (GetComponentInChildren<ImageTargetBehaviour>() != null)
        {
            ImageTargetBehaviour[] itb = GetComponentsInChildren<ImageTargetBehaviour>();
            for (int i = 0; i < itb.Length; i++)
            {
                DynamicTrackableEventHandler dteh = itb[i].GetComponent<DynamicTrackableEventHandler>();

                if (!dteh.isFreeModel)
                    itb[i].enabled = on;
            }
        }
    }
}
