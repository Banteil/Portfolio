using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class RankGameBackground : MonoBehaviour
    {
        private const string STAR_EFFECT_TRIGGER_NAME = "OnEffect";
        private Animator[] starAnimators;

        private void Awake()
        {
            starAnimators = GetComponentsInChildren<Animator>().Where(go => go.gameObject != gameObject).ToArray();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(StarEffectCoroutine());
        }

        private IEnumerator StarEffectCoroutine()
        {
            int randomIndex;
            float randomAnimSpeed;
            float randomSize;
            float randomTime;

            while (true)
            {
                randomIndex = Random.Range(0, starAnimators.Length);
                randomAnimSpeed = Random.Range(0.5f, 1.5f);
                randomSize = Random.Range(0.33f, 1.66f);
                randomTime = Random.Range(1f, 7f);

                starAnimators[randomIndex].transform.localScale = new Vector3(randomSize, randomSize, 1);
                starAnimators[randomIndex].speed = randomAnimSpeed;
                starAnimators[randomIndex].SetTrigger(STAR_EFFECT_TRIGGER_NAME);

                Debug.Log($"Star: {starAnimators[randomIndex]}, {randomAnimSpeed}, {randomSize}, {randomTime}");

                yield return new WaitForSeconds(randomTime);
            }
        }
    }
}