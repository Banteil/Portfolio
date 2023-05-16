using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class SpotlightEffect : MonoBehaviour
{
    const float maxValue = 0.1f;
    const float minValue = 0.01f;

    enum RenderType { SPRITE, IMAGE, RAWIMAGE }
    RenderType renderType;

    PhotonView pV;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    [HideInInspector]
    public Image image;
    [HideInInspector]
    public RawImage rawImage;

    float radValue;
    bool isHighlightSpot;
    public bool IsHighlightSpot
    {
        get { return isHighlightSpot; }
        set
        {
            isHighlightSpot = value;
            
            if (isHighlightSpot)
            {
                radValue = maxValue / 2f;
                //Cursor.visible = false;
            }
            else
            {
                radValue = 1f;
                //Cursor.visible = true;
            }

            switch (renderType)
            {
                case RenderType.SPRITE:
                    spriteRenderer.material.SetFloat("_Radius", radValue);
                    break;
                case RenderType.IMAGE:
                    image.material.SetFloat("_Radius", radValue);
                    break;
                case RenderType.RAWIMAGE:
                    rawImage.material.SetFloat("_Radius", radValue);
                    break;
            }
            
            pV.RPC("RadiusValue", RpcTarget.Others, radValue);
        }
    }

    void Initialization()
    {
        switch (renderType)
        {
            case RenderType.SPRITE:
                spriteRenderer.material.SetFloat("_CenterX", 0.25f);
                spriteRenderer.material.SetFloat("_CenterY", 0.15f);
                spriteRenderer.material.SetFloat("_Radius", 1f);
                break;
            case RenderType.IMAGE:
                image.material.SetFloat("_CenterX", 0.25f);
                image.material.SetFloat("_CenterY", 0.15f);
                image.material.SetFloat("_Radius", 1f);
                break;
            case RenderType.RAWIMAGE:
                rawImage.material.SetFloat("_CenterX", 0.25f);
                rawImage.material.SetFloat("_CenterY", 0.15f);
                rawImage.material.SetFloat("_Radius", 1f);
                break;
        }
    }

    void Start()
    {
        pV = GetComponent<PhotonView>();
        if (GetComponent<SpriteRenderer>() != null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            renderType = RenderType.SPRITE;
        }
        else if (GetComponent<Image>() != null)
        {
            image = GetComponent<Image>();
            renderType = RenderType.IMAGE;
        }
        else if (GetComponent<RawImage>() != null)
        {
            rawImage = GetComponent<RawImage>();
            renderType = RenderType.RAWIMAGE;
        }
        Initialization();
    }

    void Update()
    {
        //x축 줄어들 때 y축 비율이 늘어나 마우스 좌표가 틀어지는 문제 해결 필요
        if (isHighlightSpot)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * 0.02f;
            ResizeProcess(scroll);

            float x, y;
            if (Application.platform.Equals(RuntimePlatform.WebGLPlayer))
            {
                x = Camera.main.ScreenToViewportPoint(Input.mousePosition).x / 2f;
                y = Camera.main.ScreenToViewportPoint(Input.mousePosition).y / 3.5f;
            }
            else 
            {
                x = Camera.main.ScreenToViewportPoint(Input.mousePosition).x / 2f;
                y = (Camera.main.ScreenToViewportPoint(Input.mousePosition).y / 3.5f - 0.28f) * -1;
            }

            Debug.Log(Camera.main.ScreenToViewportPoint(Input.mousePosition).x + "," + Camera.main.ScreenToViewportPoint(Input.mousePosition).y);
            
            switch (renderType)
            {
                case RenderType.SPRITE:
                    spriteRenderer.material.SetFloat("_CenterX", x);
                    spriteRenderer.material.SetFloat("_CenterY", y);
                    break;
                case RenderType.IMAGE:
                    image.material.SetFloat("_CenterX", x);
                    image.material.SetFloat("_CenterY", y);
                    break;
                case RenderType.RAWIMAGE:
                    rawImage.material.SetFloat("_CenterX", x);
                    rawImage.material.SetFloat("_CenterY", y);
                    break;
            }            
            pV.RPC("HighlightPos", RpcTarget.Others, x, y);
        }
    }

    void ResizeProcess(float scroll)
    {
        if (CommonInteraction.Instance.isUIControl)
            return;

        radValue += scroll;
        if (radValue > maxValue) radValue = maxValue;
        else if (radValue < minValue) radValue = minValue;

        pV.RPC("RadiusValue", RpcTarget.All, radValue);        
    }

    [PunRPC]
    void HighlightPos(float x, float y)
    {
        switch (renderType)
        {
            case RenderType.SPRITE:
                spriteRenderer.material.SetFloat("_CenterX", x);
                spriteRenderer.material.SetFloat("_CenterY", y);
                break;
            case RenderType.IMAGE:
                image.material.SetFloat("_CenterX", x);
                image.material.SetFloat("_CenterY", y);
                break;
            case RenderType.RAWIMAGE:
                rawImage.material.SetFloat("_CenterX", x);
                rawImage.material.SetFloat("_CenterY", y);
                break;
        }
    }

    [PunRPC]
    void RadiusValue(float value)
    {
        switch (renderType)
        {
            case RenderType.SPRITE:
                spriteRenderer.material.SetFloat("_Radius", value);
                break;
            case RenderType.IMAGE:
                image.material.SetFloat("_Radius", value);
                break;
            case RenderType.RAWIMAGE:
                rawImage.material.SetFloat("_Radius", value);
                break;
        }
    }
}
