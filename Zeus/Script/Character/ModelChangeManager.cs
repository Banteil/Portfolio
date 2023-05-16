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

        //���� ����Ʈ���� �����ϴ� �� ������ ���� �� �ֵ���
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
            //�ش� �𵨿� �´� ��Ʈ�ѷ��� ������
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