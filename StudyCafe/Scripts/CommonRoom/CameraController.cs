using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{      
    public Transform cameraTarget;
    Camera thisCamera;
    Vector3 worldDefalutPosition;
    float speed = 1.0f;
    float halfHeight, halfWidth;

    [HideInInspector]
    public float maxheight, maxWidth;
    [HideInInspector]
    public Vector3 minBound, maxBound;
    public bool zoomPossible;

    void Awake()
    {
        thisCamera = GetComponent<Camera>();
        worldDefalutPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * speed;
        if(zoomPossible) ZoomProcess(scroll);
        if(thisCamera.enabled && cameraTarget != null) 
            MoveProcess();
    }

    public void SetHeight(float height)
    {
        maxheight = height / 2;
        thisCamera.orthographicSize = maxheight;
    }

    void ZoomProcess(float scroll)
    {
        if (!DataManager.isLogin || CommonInteraction.Instance.isUIControl)
            return;

        //최대 줌인
        if (thisCamera.orthographicSize <= 2f && scroll < 0)
        {
            thisCamera.orthographicSize = 2f;
        }
        //최대 줌아웃
        else if (thisCamera.orthographicSize >= maxheight && scroll > 0)
        {
            thisCamera.orthographicSize = maxheight;
        }
        else
        {
            thisCamera.orthographicSize += scroll;
        }
    }

    void MoveProcess()
    {        
        if (cameraTarget && thisCamera.orthographicSize != maxheight)
        {
            transform.position = (cameraTarget.position + new Vector3(0f, 0.5f, -10f));

            halfHeight = thisCamera.orthographicSize;
            halfWidth = halfHeight * Screen.width / Screen.height;

            float clampedX = Mathf.Clamp(transform.position.x, minBound.x + halfWidth, maxBound.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, minBound.y + halfHeight, maxBound.y - halfHeight);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
        else
        {
            transform.position = worldDefalutPosition;
        }
    }
}
