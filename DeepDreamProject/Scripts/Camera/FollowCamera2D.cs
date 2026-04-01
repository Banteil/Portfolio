using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    public Transform Target;
    public float FollowSpeed;
    public Vector3 Offset;
    public Collider2D BoundCollider;

    public bool StopFollow;

    float _height;
    float _width;
    float _defaultZPos;

    private void Awake()
    {        
        _height = Camera.main.orthographicSize;
        _width = _height * Screen.width / Screen.height;
        _defaultZPos = transform.localPosition.z;
    }

    private void FixedUpdate()
    {
        if (StopFollow || Target == null) return;
        FollowTarget();
        LimitCameraArea();
    }

    void FollowTarget()
    {
        Vector3 targetPos = Target.position;
        targetPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPos + Offset, FollowSpeed * Time.deltaTime);
    }

    void LimitCameraArea()
    {
        if (BoundCollider == null) return;
        float clampX = Mathf.Clamp(transform.position.x, BoundCollider.bounds.min.x + _width, BoundCollider.bounds.max.x - _width);
        float clampY = Mathf.Clamp(transform.position.y, BoundCollider.bounds.min.y + _height, BoundCollider.bounds.max.y - _height);
        //Collider 사이즈가 카메라 화면보다 작으면 중앙 정렬 처리
        if (BoundCollider.bounds.min.x + _width > 0 && BoundCollider.bounds.max.x - _width < 0) clampX = BoundCollider.bounds.center.x;
        if (BoundCollider.bounds.min.y + _height > 0 && BoundCollider.bounds.max.y - _height < 0) clampY = BoundCollider.bounds.center.y;

        transform.position = new Vector3(clampX, clampY, _defaultZPos);
    }

    private void OnDrawGizmos()
    {
        if (BoundCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(BoundCollider.bounds.center, BoundCollider.bounds.size);
    }
}
