using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    public class ModelChangeManager : MonoBehaviour
    {
        public GameObject DefaultModel;
        protected Animator _defaultAnimator;
        public List<PlayerModels> Models;
        public Transform ModelParent;

        private void Start()
        {
            Init();
        }

        protected void Init()
        {
            _defaultAnimator = GetComponent<Animator>();
        }

        //추후 리스트에서 변신하는 모델 정보를 받을 수 있도록
        public void ChangeModel(int i)
        {
            if(Models ==null && Models.Count <= 0) { return; }

            DefaultModel.SetActive(false);
            //_defaultAnimator.enabled = false;
            GetComponent<ThirdPersonController>().enabled = false;

            var model = Instantiate(Models[i].Model);
            model.transform.SetParent(ModelParent);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0, 0, 0);
            var component = Models[i].ModelController;
            //해당 모델에 맞는 컨트롤러를 가져옴
            gameObject.AddComponent(component.GetType());            

        }

    }
    [System.Serializable]
    public class PlayerModels
    {
        public GameObject Model;
        public Animator Animator;
        public ThirdPersonController ModelController;
    }
}