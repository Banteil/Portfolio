using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoNPC : NPCBasic
{
    [Range(1, 6)]
    public int hairIndex;

    Rigidbody2D rb;
    Animator hairAT;
    Vector3 savePos;

    bool isCollision = false;

    [SerializeField]
    bool isStand;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aT = GetComponent<Animator>();
        hairAT = transform.GetChild(0).GetComponent<Animator>();
    }

    void Start()
    {
        savePos = transform.position;
        hairAT.Play("CitizenHair" + hairIndex);
        if(!isStand)
            StartCoroutine(MoveProcess());
    }

    IEnumerator MoveProcess()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            float timer = 0f;
            Vector3 targetPos = new Vector3(savePos.x + Random.Range(-3f, 3f), savePos.y + Random.Range(-3f, 3f));
            aT.SetBool("IsRun", true);
            hairAT.SetBool("IsRun", true);
            while (timer < 3f)
            {
                if(isCollision)
                {
                    isCollision = false;
                    break;
                }

                timer += Time.deltaTime;
                Vector2 dir = (targetPos - transform.position).normalized;
                rb.velocity = dir * 3f;
                if (dir.x > 0)
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                else
                    transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                float dis = Vector2.Distance(targetPos, transform.position);
                if(dis < 0.1f)
                    break;

                yield return null;
            }

            rb.velocity = Vector2.zero;
            aT.SetBool("IsRun", false);
            hairAT.SetBool("IsRun", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isCollision = true;
    }
}
