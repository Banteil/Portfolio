using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PetController : MonoBehaviour
{
    AIDestinationSetter aiDestination;
    AIPath aiPath;

    // Start is called before the first frame update
    void Awake()
    {
        aiDestination = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();        
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (GameManager.Instance.Player != null)
            aiDestination.target = GameManager.Instance.Player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Player.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }


        Vector2 dir = (aiDestination.target.position - transform.position).normalized;
        if (dir.x > 0)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        else if (dir.x < 0)
            transform.eulerAngles = Vector3.zero;

        float dis = Vector3.Distance(transform.position, aiDestination.target.position);
        if (dis > 10f) transform.position = aiDestination.target.position + (Vector3)Random.insideUnitCircle;
        else if (dis < 2f) aiPath.canMove = false;
        else aiPath.canMove = true;
    }
}
