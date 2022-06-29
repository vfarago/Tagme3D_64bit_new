using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private static Manager instance = null;
    //private static MRPanel mrPanel;
    private static PrefabShelter prefabShelter;
    private static PrefabLoader prefabLoader;
    private static CanvasManager canvasManager;
    private static CheckCode checkCode;
    private static AnimalDataSetLoader animalDataSetLoader;
    private static FileDownloader fileDownloader;
    public static bool isMR { get; set; }
    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            Initialized();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Initialized()
    {
        //mrPanel = FindObjectOfType<MRPanel>();
        prefabShelter = FindObjectOfType<PrefabShelter>();
        prefabLoader = FindObjectOfType<PrefabLoader>();
        canvasManager = FindObjectOfType<CanvasManager>();
        checkCode = FindObjectOfType<CheckCode>();
        animalDataSetLoader = FindObjectOfType<AnimalDataSetLoader>();
        fileDownloader = FindObjectOfType<FileDownloader>();
    }

    public static Manager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    //public static MRPanel MRPanel
    //{
    //    get
    //    {
    //        if (null == mrPanel)
    //        {
    //            return null;
    //        }
    //        return mrPanel;
    //    }
    //}
    public static PrefabShelter PrefabShelter
    {
        get
        {
            if (null == prefabShelter)
            {
                return null;
            }
            return prefabShelter;
        }
    }
    public static PrefabLoader PrefabLoader
    {
        get
        {
            if (null == prefabLoader)
            {
                return null;
            }
            return prefabLoader;
        }
    }
    public static CanvasManager CanvasManager
    {
        get
        {
            if (null == canvasManager)
            {
                return null;
            }
            return canvasManager;
        }
    }
    public static CheckCode CheckCode
    {
        get
        {
            if (null == checkCode)
            {
                return null;
            }
            return checkCode;
        }
    }
    public static AnimalDataSetLoader AnimalDataSetLoader
    {
        get
        {
            if (null == animalDataSetLoader)
            {
                return null;
            }
            return animalDataSetLoader;
        }
    }
    public static FileDownloader FileDownloader
    {
        get
        {
            if (null == fileDownloader)
            {
                return null;
            }
            return fileDownloader;
        }
    }
}
