using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    public Color color = Color.white;

    [Range(0, 16)]
    public int outlineSize = 1;

    SpriteRenderer sR;

    private void Awake()
    {
        sR = GetComponent<SpriteRenderer>();        
    }

    void OnEnable()
    {
        UpdateOutline(true);
    }

    void OnDisable()
    {
        UpdateOutline(false);
    }

    void Update()
    {
        UpdateOutline(true);
    }

    void UpdateOutline(bool outline)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        sR.GetPropertyBlock(mpb);
        mpb.SetColor("_OutlineColor", color);
        mpb.SetFloat("_OutlineThickness", outline ? outlineSize : 0);
        sR.SetPropertyBlock(mpb);
    }
}
