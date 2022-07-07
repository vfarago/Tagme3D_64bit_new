using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ModeState { AR, MR, ModeEnd }

public class ObjInteraction : MonoBehaviour
{
    Camera currentcam
    {
        get
        {
            _currentCam = Camera.main;
            if (currentcam != null)
            {
                ratio = 1000 / currentcam.pixelWidth;//�ػ󵵿� ���� ������
            }
            return _currentCam;
        }
    }
    Camera _currentCam;
    Vector3 prevPos = Vector3.zero;
    Quaternion saverot = Quaternion.identity;
    float checkTime = 0;
    UnityAction updateAction;
    [SerializeField] Transform targetObjManual;
    Transform targetOBJ;
    Vector3 initScale = Vector3.zero;
    Vector3 initLocalEuler=Vector3.zero;

    bool interAction = true;

    float ratio = 1;

    //  Elon
    private PrefabLoader prefabLoader = Manager.PrefabLoader;
    private CanvasManager canvasManager = Manager.CanvasManager;
    public string targetName;
    public bool isFreeModel;
    private bool Phonics = false;
    private bool isHit = false;
    private ModeState modeState = ModeState.ModeEnd;
    private GameObject hitGameObject;


    Vector3 CamCorrection(Vector2 screenPoint, Transform targetOBJ)
    {
        float v3 = _currentCam.transform.InverseTransformPoint(targetOBJ.transform.position).z;
        //print(_currentCam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Vector3.Distance(_currentCam.transform.position, targetOBJ.position))));
        Vector3 vector = _currentCam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, v3));
        Debug.DrawLine(vector, _currentCam.transform.position, Color.blue);
        return vector;
        //ī�޶� �������� �ش�.
        //������ �𵨰� ī�޶� ����.

    }
    void ZoomInOutNRot(Transform tf, float sensitivity = 0.01f, float scaleMin = 0.5f, float scaleMax = 2, bool fixZrot = false)//�α����̻� ��ġ������ �۵��Ѵ�. ��Ÿ �������� ����Ѵ�. �Ű������� �������� �����Ѵ�.
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (targetOBJ == null)
        {
            updateAction = () => { };
            return;
        }

#if UNITY_EDITOR
        if (Input.mouseScrollDelta.y != 0)
        {
            float distance = 0;
            float preDistance = Input.mouseScrollDelta.y;


#else
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;
            float distance = (touch1.position - touch2.position).magnitude;
            float preDistance = (prevPos1 - prevPos2).magnitude;
#endif

            float magnitudeScale = (distance - preDistance) * ratio;

            float currentScale = tf.localScale.x;

            if (magnitudeScale < 0)
            {
                if (currentScale + magnitudeScale * sensitivity * initScale.z >= scaleMin * initScale.z)
                {
                    currentScale += magnitudeScale * sensitivity * initScale.z;
                }
                else
                {
                    currentScale = scaleMin * initScale.z;
                }
            }
            else if (magnitudeScale > 0)
            {
                if (currentScale + magnitudeScale * sensitivity * initScale.z <= scaleMax * initScale.z)
                {
                    currentScale += magnitudeScale * sensitivity * initScale.z;
                }
                else
                {
                    currentScale = scaleMax * initScale.z;
                }
            }
            //else
            //{
            //    scaleObject.GetComponent<RectTransform>().offsetMin += (touch - prevPos);
            //    scaleObject.GetComponent<RectTransform>().offsetMax += (touch - prevPos);
            //}

            tf.localScale = Vector3.one * currentScale;
        }
        //�ΰ��ϰ� �������� �ʰ� ����.
        else
        {
            if (!fixZrot)
            {
                if (prevPos == Vector3.zero)
                {
                    prevPos = CamCorrection(Input.mousePosition, targetOBJ);
                }
                if (saverot == Quaternion.identity)
                {
                    saverot = transform.rotation;
                }
                float _delta = Vector2.Distance(CamCorrection(Input.mousePosition, targetOBJ), prevPos) * 1;
                //Vector2 rowdir = prevPos - CamCorrection(Input.mousePosition,targetOBJ);
                Vector3 dir = prevPos - CamCorrection(Input.mousePosition, targetOBJ);
                Debug.DrawLine(dir + targetOBJ.position, targetOBJ.position, Color.red);
                dir = Vector3.Cross(dir, targetOBJ.position - _currentCam.transform.position);
                Debug.DrawLine(dir + targetOBJ.position, targetOBJ.position);
                Debug.DrawLine(targetOBJ.position, _currentCam.transform.position, Color.yellow);

                if (Input.GetMouseButtonDown(0))
                {
                    prevPos = CamCorrection(Input.mousePosition, targetOBJ);
                    //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        Debug.Log(hit.collider.gameObject.name);

                        if (hit.collider.gameObject == transform.gameObject && (hit.collider.tag.Equals("targetOff") || hit.collider.tag.Equals("augmentation")))
                        {
                            isHit = true;
                            CheckClickStart();
                            if (saveDoubleClickTimer != 0)//Ÿ�̸Ӱ� 0�̸� ��ȿ�� �ϰ� ��ŸƮ�� üũ�Ѵ�.
                            {
                                if (CheckDoubleEnd())
                                {
                                    StartCoroutine(DoubleTapEvent());

                                }
                                else
                                {
                                    CheckDoubleStart();
                                }
                            }
                            else
                                CheckDoubleStart();
                            if (gameObject != hit.collider.gameObject)
                                return;

                            switch (hit.collider.tag)
                            {
                                default:
                                    modeState = ModeState.ModeEnd;
                                    break;

                                case "targetOff":
                                    modeState = ModeState.MR;
                                    break;

                                case "augmentation":
                                    modeState = ModeState.AR;
                                    break;
                            }
                        }
                    }



                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (CheckClickEnd())
                    {
                        StartCoroutine(WaitDoubleClick(() => {
                            switch(modeState)
                            {
                                default: break;
                                case ModeState.AR:
                                    prefabLoader.TargetOffMoving(gameObject);
                                    canvasManager.OnTargetOffObject(true);
                                    break;

                                case ModeState.MR:
                                    StartReturnObjTf(targetOBJ, Vector3.one * initScale.z);
                                    break;
                            }

                        }));
                    }
                    saverot = tf.localRotation;
                    isHit = false;
                }
                else if (Input.GetMouseButton(0))
                {
                    if (isHit)
                        tf.localRotation = Quaternion.AngleAxis(-_delta, dir) * saverot;
                }


            }
            //else
            //{
            //    float delta = (Input.mousePosition.x - prevPos.x) * ratio;

            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        prevPos = Input.mousePosition;
            //        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            //        {
            //            Debug.Log(hit.collider.gameObject.name);

            //            if (gameObject != hit.collider.gameObject)
            //                return;

            //            if (hit.collider.gameObject == transform.gameObject && (hit.collider.tag.Equals("targetOff") || hit.collider.tag.Equals("augmentation")))
            //            {
            //                isHit = true;
            //                //hitGameObject = hit.collider.gameObject;
            //                switch (hit.collider.tag)
            //                {
            //                    default: break;

            //                    case "targetOff":
            //                        modeState = ModeState.MR;
            //                        break;

            //                    case "augmentation":
            //                        modeState = ModeState.AR;
            //                        break;
            //                }
            //            }
            //        }
            //    }
            //    else if (Input.GetMouseButtonUp(0))
            //    {
            //        isHit = false;

            //        //����Ŭ���� �ʱ�ȭ�����ۼ�
            //        if (Mathf.Abs(delta) < 5 * ratio && !Phonics)
            //        {
            //            if (Time.time - checkTime < 1)
            //            {
            //                StartCoroutine(DoubleTapEvent());
            //                //StartReturnObjTf(tf);
            //                //tf.localRotation = saverot = Quaternion.identity;
            //                //tf.localScale = Vector3.one;

            //            }
            //            else
            //            {
            //                checkTime = Time.time;

            //                switch(modeState)
            //                {
            //                    case ModeState.AR:
            //                        prefabLoader.TargetOffMoving(gameObject);
            //                        canvasManager.OnTargetOffObject(true);
            //                        break;

            //                    case ModeState.MR:
            //                        StartReturnObjTf(tf);
            //                        tf.localRotation = saverot = Quaternion.identity;
            //                        tf.localScale = Vector3.one;
            //                        break;
            //                }


            //            }
            //        }
            //        else
            //        {
            //            saverot = tf.localRotation;
            //        }
            //    }
            //    else if (Input.GetMouseButton(0))
            //    {
            //        if(isHit)
            //            tf.localRotation = Quaternion.AngleAxis(-delta, Vector3.up) * saverot;

            //    }
            //}

        }
    }
    Coroutine cur_Cor;
    void StartReturnObjTf(Transform tf,Vector3 toOriginScale, float duration = 1f, float frequancy = 10f)
    {
        if (interAction)
            if (cur_Cor == null)
            cur_Cor = StartCoroutine(Cor_ReturnOriginPos(tf, toOriginScale, duration, frequancy));
    }

    IEnumerator DoubleTapEvent()
    {
        //yield return new WaitForSeconds(0.02f);

        //camera changer
        ARManager.Instance.ChangeCamera("MainCamera");
        ARManager.Instance.setHintZero();

        prefabLoader.ChangePrefab(targetName, isFreeModel);

        canvasManager.OnPhonicsPanel(true);
        yield return new WaitForSeconds(0.05f);
        prefabLoader.ModelFalse();
        Phonics = true;

    }
    void CheckDoubleStart()
    {
        saveDoubleClickTimer = Time.time;
    }
    bool CheckDoubleEnd()
    {
        if (Time.time - saveDoubleClickTimer < 0.3f)
        {
            isDoubleClick=true;
            return true;
        }
        else
            return false;
    }
    bool isDoubleClick = false;
    IEnumerator WaitDoubleClick(System.Action done)
    {
        yield return new WaitForSeconds(0.6f);//��ٸ��µ��� ����Ŭ���̺�Ʈ�� �Ͼ�� ������ �����Ѵ�.
        if (!isDoubleClick)
        {
            done();
        }
    }
    Coroutine cur_click;
    Coroutine cur_doubleClick;
    float saveClickTimer = 0;
    float saveDoubleClickTimer = 0;

    void CheckClickStart()
    {
        saveClickTimer = Time.time;
    }
    bool CheckClickEnd()
    {
        if (Time.time-saveClickTimer < 0.3f)
        {
            return true;
        }
        else
        return false;
    }

    IEnumerator Cor_ReturnOriginPos(Transform tf,Vector3 toOriginScale, float duration = 1f, float frequancy = 10f)//�� �ڷ�ƾ�� �۵��ϸ� ����ġ�� ������ ���ư���. ���ư��� �߿��� ��ġ���´�.
    {
        interAction = false;
        float time = 0;
        Vector3 originrot = tf.localEulerAngles;
        Vector3 originscale = tf.localScale;

        while (time < duration)
        {
            float progress = time / duration;
            time += Time.deltaTime;
            if (initLocalEuler != Vector3.zero)
            {
                tf.localEulerAngles = Vector3.Lerp(originrot, initLocalEuler, progress);
            }
            else
            {
                tf.localEulerAngles = Vector3.Lerp(originrot, Vector3.zero, progress);
            }
            //print(initScale);
            if (toOriginScale != Vector3.one)
            {
                tf.localScale = (Mathf.Sin(progress * frequancy) * (1 - progress) / 2) * Vector3.one + Vector3.Lerp(originscale, toOriginScale, progress);
            }
            else
            {
                tf.localScale = (Mathf.Sin(progress * frequancy) * (1 - progress) / 2) * Vector3.one + Vector3.Lerp(originscale, Vector3.one, progress);
            }
            yield return new WaitForEndOfFrame();
        }
        saverot = Quaternion.Euler(initLocalEuler);
        interAction = true;
        cur_Cor = null;
    }
    private void Update()
    {
        updateAction();
    }
    /// <summary>
    /// ȸ�� ���� �ܾƿ��� ��뼳��
    /// </summary>
    /// <param name="target">���ͷ��� ����� �Ǵ� Ÿ��</param>
    /// <param name="sensitivity">�ø��� ���� ���������� ũ�� Ȯ��,ȸ���Ѵ�.</param>
    /// <param name="resetRot">�巡�� ���۰��� 000���� �ʱ�ȭ�ϰ� �����Ѵ�.</param>
    /// <param name="fixZRot">�� �����θ� ȸ���ϰ� �Ѵ�.</param>
    public void SetTargetOBJ(Transform target, float sensitivity = 0.01f, bool resetRot = false, bool fixZRot = false)
    {
        if (resetRot) ResetRot();
        targetOBJ = target;
        targetOBJ.localEulerAngles = Vector3.zero;
        initLocalEuler = target.eulerAngles;
        initLocalEuler = new Vector3(0, -140, 0);//�ӽù���
        saverot = Quaternion.Euler(initLocalEuler);
        initScale = target.localScale;
        updateAction = () => { if (interAction) ZoomInOutNRot(target, sensitivity, fixZrot: fixZRot); };
    }
    public void UnloadTargetOBJ()
    {
        updateAction = () => { };
    }
    public void ResetRot()
    {
        saverot = Quaternion.identity;
    }
    private void Start()
    {
        _currentCam = Camera.main;
        if (targetObjManual != null)
        {
            SetTargetOBJ(targetObjManual);
        }
        else if (targetOBJ)
        {

        }
        else
        {
            updateAction = () => { };
        }
    }


}
