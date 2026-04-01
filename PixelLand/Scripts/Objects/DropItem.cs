using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : InteractionObject
{
    [SerializeField]
    ItemData itemData;
    Item _item;
    public Item Item 
    { 
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if(_item.Info.ItemType.Equals(ItemType.GOODS))
                transform.localScale = Vector3.one * 0.5f;
            objectName = _item.Info.DisplayName;
            sR.sprite = _item.Info.Sprite;
        }
    }

    Coroutine _moveRoutine;

    [SerializeField]
    bool interPossible;
    public bool InterPossible { get { return interPossible; } }

    Animation anim;
    Rigidbody2D rB;

    protected override void Awake()
    {
        base.Awake();
        rB = GetComponent<Rigidbody2D>();
        Transform body = transform.GetChild(0);
        sR = body.GetComponent<SpriteRenderer>();
        anim = body.GetComponent<Animation>();
        outline = body.GetComponent<SpriteOutline>();
        sightAct = transform.GetChild(1).GetComponent<SightAct>();

        actDescription = "습득";
    }

    private void OnEnable()
    {
        if (itemData != null)
        {
            InteractionPossible();
            return;
        }
        
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveItem());
        Invoke("DisableSelf", 50f);
    }

    private void OnDisable()
    {
        if (itemData != null) return;

        Reset();        
        MapManager.Instance.DropItemObjects.Enqueue(gameObject);
    }

    private void Start()
    {
        //임의로 아이템 생성할 때 사용
        if (itemData == null || itemData.ItemInfo == null) return;
        Item item = new Item();
        item.Info = itemData.ItemInfo;
        Item = item;
    }

    IEnumerator MoveItem()
    {
        Vector3 pos = transform.position + (Vector3)Random.insideUnitCircle;
        Vector2 movePos = (pos - transform.position).normalized;
        float speed = Random.Range(50f, 100f);
        float timer = 0f;
        while (timer < 0.5f)
        {
            rB.velocity = movePos * speed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        rB.velocity = Vector2.zero;
        InteractionPossible();
    }

    public override void Interaction(CharacterBasic character)
    {
        ItemManager.Instance.ItemAcquisition(character, _item);
        gameObject.SetActive(false);
    }

    void InteractionPossible()
    {
        anim.Play();
        interPossible = true;
    }

    void DisableSelf() => gameObject.SetActive(false);

    private void Reset()
    {
        transform.localScale = Vector3.one;
        interPossible = false;
        _item = null;
    }

    IEnumerator GetCoinDirecting()
    {        
        float timer = 0;
        Vector3 randDir = Random.insideUnitCircle;
        while(timer < 0.05f)
        {
            timer += Time.deltaTime;
            transform.position += randDir * 20f * Time.deltaTime;
            yield return null;
        }

        float dis = Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position);
        while (dis > 0.2f)
        {
            Vector3 dir = (GameManager.Instance.Player.transform.position - transform.position).normalized;
            transform.position += dir * 20f * Time.deltaTime;
            dis = Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position);
            yield return null;
        }
        GameManager.Instance.Player.Items.Coin += (int)_item.Info.Value;
        gameObject.SetActive(false);
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (!interPossible) return;
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                if (_item.Info.ItemType.Equals(ItemType.GOODS))
                {
                    interPossible = false;
                    StartCoroutine(GetCoinDirecting());
                }
                else
                    cH.InteractOn(this);
            }
        }
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (!interPossible) return;
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                if (_item.Info.ItemType.Equals(ItemType.GOODS))
                {
                    interPossible = false;
                    StartCoroutine(GetCoinDirecting());
                }
                else
                    cH.InteractOn(this);
            }
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (!interPossible) return;
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                cH.InteractOff(this);
                outline.enabled = false;
            }
        }
    }
}
