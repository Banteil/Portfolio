using UnityEngine;

namespace Zeus
{
    public  class zMonoBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private bool _openCloseEvents;
        [SerializeField, HideInInspector]
        private bool _openCloseWindow;
        [SerializeField, HideInInspector]       
        private int _selectedToolbar;
    }  
}
