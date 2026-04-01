using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private void OnEnable()
    {
        transform.SetAsLastSibling();
    }
}
