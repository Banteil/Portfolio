using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeCharacterController : MonoBehaviour
{
    Animator aT;
    SpriteRenderer sR;

    [SerializeField]
    float speed = 5f;

    private void Awake()
    {
        aT = GetComponent<Animator>();
        sR = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(MoveProcess());
    }

    IEnumerator MoveProcess()
    {
        float timer = 0f;
        float moveTimer = Random.Range(1f, 3f);
        float waitTimer = Random.Range(0.1f, 3f);
        int[] dir = { -1, 1 };

        while (true)
        {
            yield return new WaitForSeconds(waitTimer);

            moveTimer = Random.Range(1f, 3f);
            aT.SetBool("IsWalk", true);
            int direction = dir[Random.Range(0, 2)];
            if (direction < 0) sR.flipX = true;
            else sR.flipX = false;

            while (timer < moveTimer)
            {
                timer += Time.deltaTime;
                if (transform.localPosition.x <= -200f)
                {
                    direction = 1;
                    sR.flipX = false;
                }
                else if (transform.localPosition.x >= 200f)
                {
                    direction = -1;
                    sR.flipX = true;
                }

                transform.localPosition += Vector3.right * speed * direction * Time.deltaTime;

                yield return null;
            }

            timer = 0f;
            aT.SetBool("IsWalk", false);
            waitTimer = Random.Range(0.1f, 3f);
        }        
    }
}
