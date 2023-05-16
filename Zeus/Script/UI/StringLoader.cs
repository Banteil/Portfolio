using TMPro;
using UnityEngine;

namespace Zeus
{
    public class StringLoader : MonoBehaviour
    {
        private TextMeshProUGUI TextField;
        public int StringTableID;
        // Start is called before the first frame update
        void Start()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (TextField == null)
                TextField = GetComponent<TextMeshProUGUI>();

            if (TextField == null)
            {
                Debug.LogError("Text Field Not Found : GameObject Name is === " + gameObject.name);
                return;
            }

            var str = TableManager.GetString(StringTableID);
            if (string.IsNullOrEmpty(str))
            {
                Debug.LogError("String is null Or Empty! string ID is === " + StringTableID + "\n Object Name ==== " + transform.parent.name);
            }

            TextField.SetText(str);
        }

        private void OnEnable()
        {
            if (TextField == null || string.IsNullOrEmpty(TextField.text))
            {
                Initialized();
            }
        }
    }
}