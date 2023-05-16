using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    public class CutSceneCameraControl : MonoBehaviour
    {
        private CinemachineVirtualCamera vCam;
        private ThirdPersonController player;

        private void Start()
        {
            vCam = GetComponent<CinemachineVirtualCamera>();
            SearchPlayer();
        }

        private void SearchPlayer()
        {
            player = FindObjectOfType<ThirdPersonController>();
        }

        public void SetActive(bool playerControl)
        {
            gameObject.SetActive(true);
            player.CantInput = !playerControl;
            Debug.Log(player.CantInput);
            if (PlayerUIManager.Get() != null)
            {
                PlayerUIManager.Get().HUDVisible(false,0);
            }
        }

        public void SetDeActive(bool playerControl)
        {
            gameObject.SetActive(false);
            player.CantInput = !playerControl;
            if (PlayerUIManager.Get() != null)
            {
                PlayerUIManager.Get().HUDVisible(true);
            }
        }
    }
}