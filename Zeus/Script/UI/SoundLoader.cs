using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class SoundLoader : MonoBehaviour
    {
        public float DelayTime;
        public int SoundTableID;
        public bool UsePosition = true;
        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForSeconds(DelayTime);

            yield return new WaitUntil(()=> SoundManager.Instance != null);

            if (UsePosition) 
            {
                Debug.Log("Position Sound");
                SoundManager.Instance.Play(SoundTableID, transform.position);
            }
            else
            {
                Debug.Log($"Sound Play ID : {SoundTableID}");
                SoundManager.Instance.Play(SoundTableID);
            }
        }
    }
}