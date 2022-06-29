/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/
using I2.Loc;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;

public class DynamicTrackableEventHandler : TrackableEventHandler
{
    public bool isFreeModel = false;
    public string targetName;
    public bool isModelLoading = false;

    private bool isEndAR;
    GameObject m3dModel = null;

    protected override void Awake()
    {
        base.Awake();
        //바꿔야합니다0627
        //targetName = mTrackableBehaviour.TrackableName.ToLower();
    }

    #region PRIVATE_METHODS

    protected override void OnTrackingFound()
    {
        if (isFreeModel)
        {
            isEndAR = prefabLoader.isEndAR;

            canvasManager.OnTrackingFound(false);

            if (!isModelLoading && !isEndAR)
            {
                //Debug.Log("        found " + targetName);
                StartCoroutine(loadModelAsync());
                isModelLoading = true;
            }
        }
        else
        {
            bool isExist = false;
            bool isConfirm = false;
            for (int i = 0; i < prefabShelter.tmModel.Length; i++)
            {
                if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                {
                    if (prefabShelter.tmModel[i].model.name.Equals(targetName))
                    {
                        isExist = true;
                        isConfirm = prefabShelter.tmModel[i].isConfirm;
                        ReturnDB.UserName = AccountManager.RETURNNAME;
                        ReturnDB.Code = 0;
                        ReturnDB.TargetCode = i;
                        ReturnDB db = new ReturnDB();
                        db.SendDB();
                        break;
                    }
                }
            }

            if (isConfirm)
            {
                isEndAR = prefabLoader.isEndAR;

                canvasManager.OnTrackingFound(false);

                if (!isModelLoading && !isEndAR)
                {
                    //Debug.Log("        found " + targetName);

                    StartCoroutine(loadModelAsync());
                    isModelLoading = true;
                }
            }
            else
            {
                canvasManager.OnInfoSerial(isExist);
            }
        }
    }


    protected override void OnTrackingLost()
    {
        if (isModelLoading && !prefabLoader.isTargetoff)
        {
            //Debug.Log("        lost " + targetName);

            Destroy(m3dModel);
            m3dModel = null;

            isModelLoading = false;
        }
    }


    private IEnumerator loadModelAsync()
    {
        if (prefabLoader.isTargetoff)
            yield return prefabLoader.DestroyObj();

        if (m3dModel == null && !isEndAR)
        {
            if (isFreeModel)
            {
                string lang = LocalizationManager.CurrentLanguage;
                LocalizationManager.CurrentLanguage = "book";
                string bookNum = LocalizationManager.GetTermTranslation(targetName);
                //GameObject go = Resources.Load<GameObject>(string.Format("objects/{0}", targetName));
                GameObject go = Resources.Load<GameObject>(string.Format("objects/Book{0}/{1}", bookNum, targetName));
                LocalizationManager.CurrentLanguage = lang;
                m3dModel = Instantiate(go, transform, false);

            }
            else
            {
                for (int i = 0; i < prefabShelter.tmModel.Length; i++)
                {
                    if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                    {
                        if (prefabShelter.tmModel[i].model.name.Equals(targetName))
                        {
                            m3dModel = Instantiate(prefabShelter.tmModel[i].model, transform, false);
                            RendererSet(m3dModel);
                            break;
                        }
                    }
                }
            }

            if (m3dModel != null)
            {
                m3dModel.tag = "augmentation";

                m3dModel.transform.Rotate(0, 270, -90, Space.Self); // side  10.18.2017

                StartCoroutine(RepositionAugmentation(0.3f));

                yield return m3dModel;

                if (m3dModel != null)
                {
                    //gestures  [start]
                    TapGesture tagGesture = m3dModel.AddComponent<TapGesture>();
                    tagGesture.NumberOfTapsRequired = 1;
                    tagGesture.TimeLimit = 1;

                    ScaleGesture scaleGesture = m3dModel.AddComponent<ScaleGesture>();
                    PanGesture panGesture = m3dModel.AddComponent<PanGesture>();

                    scaleGesture.AddFriendlyGesture(panGesture);
                    panGesture.AddFriendlyGesture(scaleGesture);

                    TSGestureHandler gestureHandler = m3dModel.AddComponent<TSGestureHandler>();
                    //임시조치입니다0627
                    //gestureHandler.mTrackableBehaviour = mTrackableBehaviour;
                    gestureHandler.targetName = targetName;
                    gestureHandler.isFreeModel = isFreeModel;
                    gestureHandler.enabled = true;
                    //gesture [end]
                }
            }
        }
    }


    private IEnumerator RepositionAugmentation(float time)
    {
        float initialScale;
        if (m3dModel.GetComponentInChildren<MeshRenderer>())
        {
            m3dModel.GetComponentInChildren<MeshRenderer>().enabled = true;
            initialScale = 0.5f;
        }
        else
        {
            m3dModel.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            initialScale = 0.3f;
        }

        if (transform.localScale.x > 500)
            initialScale *= 0.5f;

        m3dModel.AddComponent<BoxCollider>().size = new Vector3(2, 2, 2);

        Vector3 startScaling = new Vector3(0.01f, 0.01f, 0.01f);
        Vector3 newScaling = new Vector3(initialScale, initialScale, initialScale);

        //Object Reflect
        if (arManager.isFrontCamera)
            newScaling = new Vector3(initialScale, initialScale, initialScale * -1f);

        //lerping
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            if (m3dModel == null)
                break;

            m3dModel.transform.localScale = Vector3.Lerp(startScaling, newScaling, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (m3dModel != null)
            m3dModel.transform.localScale = newScaling;

        yield return m3dModel;
    }
    #endregion

    public void OnMRFound(string str, bool isFree)
    {
        base.OnTrackingFound();
        if (isFree)
        {
            isEndAR = prefabLoader.isEndAR;

            canvasManager.OnTrackingFound(false);

            if (!isModelLoading && !isEndAR)
            {

                //Debug.Log("        found " + targetName);


                StartCoroutine(MRloadModelAsync(str, isFree));
                isModelLoading = true;
            }
        }
        else
        {
            bool isExist = false;
            bool isConfirm = false;
            for (int i = 0; i < prefabShelter.tmModel.Length; i++)
            {
                if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                {
                    if (prefabShelter.tmModel[i].model.name.Equals(str))
                    {
                        isExist = true;
                        isConfirm = prefabShelter.tmModel[i].isConfirm;
                        ReturnDB.UserName = AccountManager.RETURNNAME;
                        ReturnDB.Code = 0;
                        ReturnDB.TargetCode = i;
                        ReturnDB db = new ReturnDB();
                        db.SendDB();
                        break;
                    }
                }
            }

            if (isConfirm)
            {
                isEndAR = prefabLoader.isEndAR;

                canvasManager.OnTrackingFound(false);

                if (!isModelLoading && !isEndAR)
                {
                    //Debug.Log("        found " + targetName);

                    StartCoroutine(MRloadModelAsync(str, isFree));
                    isModelLoading = true;
                }
            }
            else
            {
                canvasManager.OnInfoSerial(isExist);
            }
        }
    }

    public IEnumerator MRloadModelAsync(string str, bool isFree)
    {
        if (prefabLoader.isTargetoff)
            yield return prefabLoader.DestroyObj();

        if (m3dModel == null && !isEndAR)
        {
            if (isFree)
            {
                string lang = LocalizationManager.CurrentLanguage;
                LocalizationManager.CurrentLanguage = "book";
                string bookNum = LocalizationManager.GetTermTranslation(str);
                //GameObject go = Resources.Load<GameObject>(string.Format("objects/{0}", targetName));
                GameObject go = Resources.Load<GameObject>(string.Format("objects/Book{0}/{1}", bookNum, str));
                LocalizationManager.CurrentLanguage = lang;
                m3dModel = Instantiate(go, transform, false);
            }
            else
            {
                for (int i = 0; i < prefabShelter.tmModel.Length; i++)
                {
                    if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                    {
                        if (prefabShelter.tmModel[i].model.name.Equals(str))
                        {
                            m3dModel = Instantiate(prefabShelter.tmModel[i].model, transform, false);
                            RendererSet(m3dModel);
                            break;
                        }
                    }
                }
            }

            if (m3dModel != null)
            {
                m3dModel.tag = "augmentation";

                m3dModel.transform.Rotate(0, 270, -90, Space.Self); // side  10.18.2017

                StartCoroutine(RepositionAugmentation(0.3f));

                yield return m3dModel;

                if (m3dModel != null)
                {
                    //gestures  [start]
                    TapGesture tagGesture = m3dModel.AddComponent<TapGesture>();
                    tagGesture.NumberOfTapsRequired = 1;
                    tagGesture.TimeLimit = 1;

                    ScaleGesture scaleGesture = m3dModel.AddComponent<ScaleGesture>();
                    PanGesture panGesture = m3dModel.AddComponent<PanGesture>();

                    scaleGesture.AddFriendlyGesture(panGesture);
                    panGesture.AddFriendlyGesture(scaleGesture);

                    TSGestureHandler gestureHandler = m3dModel.AddComponent<TSGestureHandler>();
                    //바꿔야합니다0627
                    //gestureHandler.mTrackableBehaviour = mTrackableBehaviour;
                    gestureHandler.targetName = str;
                    gestureHandler.isFreeModel = isFree;
                    gestureHandler.enabled = true;
                    //gesture [end]

                    StartCoroutine(gestureHandler.MRDoubleTapEvent());
                }
            }
        }
    }

    void RendererSet(GameObject obj)
    {
        Renderer[] renderer = obj.transform.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer item in renderer)
        {
            if (item.materials != null)
            {
                foreach (Material mat in item.materials)
                {
                    Shader sha = mat.shader;
                    Debug.Log(sha.name);
                    mat.shader = Shader.Find(sha.name);
                }
            }
        }
    }
}

public class ReturnDB : MonoBehaviour
{
    public static string UserName { get; set; }
    public static int Code { get; set; }
    public static int TargetCode { get; set; }
    public static int Language { get; set; }


    public int TransLanguage()
    {
        string language = I2.Loc.LocalizationManager.CurrentLanguage;
        int lang = -1;
        switch (language)
        {
            case "kor":
                lang = 0;
                break;
            case "eng":
                lang = 1;
                break;
            case "chn":
                lang = 2;
                break;
            case "are":
                lang = 3;
                break;
            case "deu":
                lang = 4;
                break;
            case "esp":
                lang = 5;
                break;
            case "fra":
                lang = 6;
                break;
            case "heb":
                lang = 7;
                break;
            case "hin":
                lang = 8;
                break;
            case "ind":
                lang = 9;
                break;
            case "ita":
                lang = 10;
                break;
            case "jpn":
                lang = 11;
                break;
            case "pol":
                lang = 12;
                break;
            case "rus":
                lang = 13;
                break;
            case "tha":
                lang = 14;
                break;
            case "vie":
                lang = 15;
                break;
        }
        return lang;
    }

    public void SendDB()
    {
        string str = string.Format("{0}/{1}/{2}/{3}", UserName, Code, TargetCode, TransLanguage());
        Debug.Log(str);
    }
}