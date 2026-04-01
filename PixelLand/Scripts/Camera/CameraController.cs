using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{    
    Transform target;
    public Transform Target { get { return target; } set { target = value; } }
    float height, width;
    [SerializeField]
    float speed = 10f;
    float shakeAmount;
    bool isShake;

    private void Start()
    {
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(transform.position, target.position + new Vector3(0f, 1f, -10f), Time.deltaTime * speed);

        float lx = MapManager.Instance.biggerTilemap.localBounds.size.x * 0.5f - width;
        float clampX = Mathf.Clamp(transform.position.x, -lx + (MapManager.Instance.biggerTilemap != null ? MapManager.Instance.biggerTilemap.localBounds.center.x : 0), 
            lx + (MapManager.Instance.biggerTilemap != null ? MapManager.Instance.biggerTilemap.localBounds.center.x : 0));

        float ly = MapManager.Instance.biggerTilemap.localBounds.size.y * 0.5f - height;
        float clampY = Mathf.Clamp(transform.position.y, -ly + (MapManager.Instance.biggerTilemap != null ? MapManager.Instance.biggerTilemap.localBounds.center.y : 0), 
            ly + (MapManager.Instance.biggerTilemap != null ? MapManager.Instance.biggerTilemap.localBounds.center.y : 0));

        if(isShake)
            transform.position = new Vector3(clampX, clampY, -10f) + (Random.insideUnitSphere * shakeAmount);
        else
            transform.position = new Vector3(clampX, clampY, -10f);
    }

    public void StartVibrate(float shakeAmount, float time)
    {
        this.shakeAmount = shakeAmount;
        isShake = true;
        CancelInvoke("EndShake");
        Invoke("EndShake", time);
    }

    void EndShake() => isShake = false;
}
