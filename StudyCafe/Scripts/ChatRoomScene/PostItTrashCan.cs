using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostItTrashCan : MonoBehaviour
{
    public PostItPanel panel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PostItGroup>() != null)
        {
            if (collision.gameObject.GetComponent<PostItGroup>().isDrag)
                GetComponent<Image>().color = Color.red;
        }
        else if (collision.gameObject.GetComponent<PostItItem>() != null)
        {
            if (collision.gameObject.GetComponent<PostItItem>().isDrag)
                GetComponent<Image>().color = Color.red;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PostItGroup>() != null)
        {
            if (!collision.gameObject.GetComponent<PostItGroup>().isDrag)
            {
                for (int i = 0; i < panel.groupList.Count; i++)
                {
                    if (panel.groupList[i].index.Equals(collision.gameObject.GetComponent<PostItGroup>().index))
                    {
                        Destroy(collision.gameObject);
                        Debug.Log(collision.gameObject.GetComponent<PostItGroup>().index + "번째 그룹 삭제");
                        GetComponent<Image>().color = Color.white;
                    }
                }
            }
        }
        else if (collision.gameObject.GetComponent<PostItItem>() != null)
        {
            if (!collision.gameObject.GetComponent<PostItItem>().isDrag)
            {
                for (int i = 0; i < panel.itemList.Count; i++)
                {
                    if (panel.itemList[i].index.Equals(collision.gameObject.GetComponent<PostItItem>().index))
                    {
                        Destroy(collision.gameObject);
                        Debug.Log(collision.gameObject.GetComponent<PostItItem>().index + "번째 아이템 삭제");
                        GetComponent<Image>().color = Color.white;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PostItGroup>() != null)
        {
            if (collision.gameObject.GetComponent<PostItGroup>().isDrag)
                GetComponent<Image>().color = Color.white;
        }
        else if (collision.gameObject.GetComponent<PostItItem>() != null)
        {
            if (collision.gameObject.GetComponent<PostItItem>().isDrag)
                GetComponent<Image>().color = Color.white;
        }
    }
}
