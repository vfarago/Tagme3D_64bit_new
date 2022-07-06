using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentsBlockerEssentialContainer : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] Button[] btns;
    public Text GetText { get => text; }
    public Button[] GetBtns { get => btns; }
}
