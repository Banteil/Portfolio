using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    SpriteRenderer sR;
    public SpriteRenderer SR { get { return sR; } }

    [SerializeField]
    Transform target;
    public Transform Target 
    { 
        set 
        { 
            target = value;
            Vector3 targetPos = target.position;
            targetPos.y += target.GetComponent<Character>().SR.sprite.rect.height * 0.5f * 0.01f;
            float angle = Mathf.Atan2(targetPos.y - transform.position.y, targetPos.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        } 
    }

    ElementalProperties propertie;
    public ElementalProperties Propertie { set { propertie = value; } }

    float damage;
    public float Damage { set { damage = value; } }

    [SerializeField]
    float speed;

    private void Awake()
    {
        sR = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = target.position;
        targetPos.y += target.GetComponent<Character>().SR.sprite.rect.height * 0.5f * 0.01f;
        transform.position += (targetPos - transform.position).normalized * speed * Time.deltaTime;

        float dis = Vector2.Distance(transform.position, targetPos);
        if (dis <= 0.3f)
        {
            target.GetComponent<Character>().Hit(damage, propertie, 0f, 0f);
            Destroy(gameObject);
        }
    }
}
