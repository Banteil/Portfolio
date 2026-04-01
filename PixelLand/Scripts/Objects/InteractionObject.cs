using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionObject : MonoBehaviour
{
    [SerializeField]
    protected string objectID;
    public string ObjectID { get { return objectID; } set { objectID = value; } }

    [SerializeField]
    protected string objectName = "";
    [SerializeField]
    protected string interactionKey = "F";
    public string InteractionKey { get { return interactionKey; } }
    [SerializeField]
    protected string actDescription = "";
    public string ActDescription { get { return actDescription; } }
    [SerializeField]
    protected int switchNum = 0;
    [SerializeField]
    protected bool selfSwitch = false;
    [SerializeField]
    protected float sight; //시야 범위
    public float Sight { get { return sight; } }
    protected SightAct sightAct;

    protected SpriteRenderer sR;
    public SpriteRenderer SR { get { return sR;} }
    protected Animator aT;
    public Animator AT { get { return aT; } }
    protected SpriteOutline outline;
    public SpriteOutline Outline { get { return outline; } }

    abstract public void Interaction(CharacterBasic character);

    abstract public void ObjectDetection(GameObject detectionObj);
    abstract public void ObjectDetecting(GameObject detectionObj);
    abstract public void OutOfDetection(GameObject detectionObj);

    public Action<GameObject> OnDetectionEnter;
    public Action<GameObject> OnDetectionStay;
    public Action<GameObject> OnDetectionEnd;

    public void EnalbeOutline()
    { 
        if (outline == null) return;
        outline.enabled = true;
    }

    protected virtual void Awake() { }
    protected virtual void OnEnable() 
    {
        //if (_detectionCoroutine != null)
        //    StopCoroutine(_detectionCoroutine);
        //_detectionCoroutine = Detection();
        //StartCoroutine(_detectionCoroutine); 
    }
    protected virtual void OnDisable() 
    {
        //if (_detectionCoroutine != null)
        //{
        //    StopCoroutine(_detectionCoroutine);
        //    _detectionCoroutine = null;
        //}
    }
    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void OnDestroy() { }


    //private IEnumerator Detection()
    //{
    //    WaitForSeconds wfs = new WaitForSeconds(_detectionInterval);
    //    while (true)
    //    {
    //        var colliders = Physics2D.OverlapCircleAll(transform.position, sight);
    //        for (int i = 0; i < colliders.Length; i++)
    //        {
    //            var collider = colliders[i];
    //            var str = _targetTags.FirstOrDefault(x => collider.CompareTag(x));
    //            if (str == null && _detectionTarget != null)
    //            {
    //                OnDetectionEnd?.Invoke(_detectionTarget);
    //                _detectionTarget = null;
    //            }
    //            if (str != null && _detectionTarget.Equals(collider.gameObject))
    //            {
    //                OnDetectionStay?.Invoke(_detectionTarget);
    //            }
    //            if (str != null && _detectionTarget.Equals(collider.gameObject))
    //            {
    //                _detectionTarget = collider.gameObject;
    //                OnDetectionEnter?.Invoke(_detectionTarget);
    //            }
    //        }
    //        yield return wfs;
    //    }
    //}
}
