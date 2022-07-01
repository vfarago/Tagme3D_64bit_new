using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjInteraction : MonoBehaviour
{

    Camera currentcam 
    {
        get
        {
            _currentCam =Camera.main;
            if (currentcam != null)
            {
                ratio = 1000 / currentcam.pixelWidth;//�ػ󵵿� ���� ������
            }
            return _currentCam;
        }
    }
    Camera _currentCam;
    Vector2 prevPos = Vector2.zero;
    Quaternion saverot = Quaternion.identity;
    float checkTime = 0;
    UnityAction updateAction;
    [SerializeField] Transform targetOBJ;

    bool interAction = true;

    float ratio = 1;
    void ZoomInOutNRot(Transform tf, float sensitivity = 0.01f, float scaleMin = 0.5f, float scaleMax = 2,bool fixZrot=false)//�α����̻� ��ġ������ �۵��Ѵ�. ��Ÿ �������� ����Ѵ�. �Ű������� �������� �����Ѵ�.
    {
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
                if (currentScale + magnitudeScale * sensitivity >= scaleMin)
                {
                    currentScale += magnitudeScale * sensitivity;
                }
                else
                {
                    currentScale = scaleMin;
                }
            }
            else if (magnitudeScale > 0)
            {
                if (currentScale + magnitudeScale * sensitivity <= scaleMax)
                {
                    currentScale += magnitudeScale * sensitivity;
                }
                else
                {
                    currentScale = scaleMax;
                }
            }
            //else
            //{
            //    scaleObject.GetComponent<RectTransform>().offsetMin += (touch - prevPos);
            //    scaleObject.GetComponent<RectTransform>().offsetMax += (touch - prevPos);
            //}

            tf.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        //�ΰ��ϰ� �������� �ʰ� ����.
        else
        {
            if (!fixZrot) 
            {
                float _delta = Vector3.Distance(Input.mousePosition , prevPos) * ratio;
                Vector2 rowdir = prevPos - (Vector2)Input.mousePosition;
                Vector3 dir = new Vector3(rowdir.x, rowdir.y, tf.position.z);
                dir = Vector3.Cross(dir, dir+Vector3.forward);
                if (Input.GetMouseButtonDown(0))
                {
                    prevPos = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    //����Ŭ���� �ʱ�ȭ�����ۼ�
                    if (Mathf.Abs(_delta) < 5 * ratio)
                    {
                        if (Time.time - checkTime < 1)
                        {
                            StartReturnObjTf(tf);
                            //tf.localRotation = saverot = Quaternion.identity;
                            //tf.localScale = Vector3.one;

                        }
                        else
                        {
                            checkTime = Time.time;
                        }
                    }
                    else
                    {
                        saverot = tf.localRotation;
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    tf.localRotation = Quaternion.AngleAxis(-_delta, dir) * saverot;
                }


            }
            else
            {
                float delta = (Input.mousePosition.x - prevPos.x) * ratio;

                if (Input.GetMouseButtonDown(0))
                {
                    prevPos = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    //����Ŭ���� �ʱ�ȭ�����ۼ�
                    if (Mathf.Abs(delta) < 5 * ratio)
                    {
                        if (Time.time - checkTime < 1)
                        {
                            StartReturnObjTf(tf);
                            //tf.localRotation = saverot = Quaternion.identity;
                            //tf.localScale = Vector3.one;

                        }
                        else
                        {
                            checkTime = Time.time;
                        }
                    }
                    else
                    {
                        saverot = tf.localRotation;
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    tf.localRotation = Quaternion.AngleAxis(-delta, Vector3.up) * saverot;
                }
            }
          
        }
    }
    Coroutine cur_Cor;
    void StartReturnObjTf(Transform tf, float duration = 1f, float frequancy = 10f)
    {
        if(cur_Cor==null)
        cur_Cor = StartCoroutine(Cor_ReturnOriginPos(tf,duration,frequancy));
    }
    IEnumerator Cor_ReturnOriginPos(Transform tf,float duration=1f,float frequancy=10f)//�� �ڷ�ƾ�� �۵��ϸ� ����ġ�� ������ ���ư���. ���ư��� �߿��� ��ġ���´�.
    {
        interAction = false;
        float time=0;
        Vector3 originrot= tf.localEulerAngles;
        Vector3 originscale = tf.localScale;
        
        while (time<duration)
        {
            float progress = time / duration;
            time += Time.deltaTime;
            tf.localEulerAngles = Vector3.Lerp(originrot, Vector3.zero, progress);
            tf.localScale = (Mathf.Sin(progress*frequancy)*(1-progress)/2)*Vector3.one+ Vector3.Lerp(originscale, Vector3.one, progress);
            yield return new WaitForEndOfFrame();
        }
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
    public void SetTargetOBJ(Transform target, float sensitivity = 0.01f, bool resetRot=false,bool fixZRot=false)
    {
        if (resetRot) ResetRot();
        targetOBJ = target;
        targetOBJ.localEulerAngles = Vector3.zero;
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
        if (targetOBJ != null)
        {
            SetTargetOBJ(targetOBJ);
        }
        else
        {
            updateAction = () => { };
        }
    }
    

}