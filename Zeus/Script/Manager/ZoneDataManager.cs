using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class ZoneDataManager : BaseObject<ZoneDataManager>
    {
        internal List<ZoneData> ZoneDataList = new List<ZoneData>();
        internal bool IsInitializedComplete { get; private set; }

        public Action ZoneClearCallback;
        public Action ZoneInitializedCallback;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => PlayerUIManager.Get() != null);

            var datas = FindObjectsOfType<ZoneData>();
            if (datas != null)
            {
                ZoneDataList = datas.ZToList();
            }

            if (!string.IsNullOrEmpty(TableManager.CurrentPlayerData.SaveZoneData.SceneName) && TableManager.CurrentPlayerData.SaveZoneData.SceneName.Equals(SceneLoadManager.NextScene) == false)
            {
                TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs.Clear();
            }
            CharacterObjectManager.Get().CallCharacterRemove += ZoneClearCheck;

            SetPlayerPosition();
        }

        void SetPlayerPosition()
        {
            var findIndex = -1;
            findIndex = ZoneDataList.FindIndex((x) => x.Info.ZoneID == TableManager.CurrentPlayerData.SaveZoneData.ZoneID);
            if (findIndex < 0)
            {
                IsInitializedComplete = true;
                Debug.LogError("Not Found ZoneData Find ZoneID is === " + TableManager.CurrentPlayerData.SaveZoneData.ZoneID);
                InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);
                return;
            }

            var startPosTr = ZoneDataList[findIndex].PlayerStartPosition;
            if (startPosTr == null)
            {
                IsInitializedComplete = true;
                Debug.LogError("PlayerStartPosition is null!!!");
                return;
            }

            var player = FindObjectOfType<ThirdPersonController>();
            if (player == null) 
            {
                Debug.LogError("Not Found Player");
                return; 
            }
            player.ZoneID = TableManager.CurrentPlayerData.SaveZoneData.ZoneID;

            player.transform.SetPositionAndRotation(startPosTr.position, startPosTr.rotation);
            var cam = CameraManager.Get().GetCamera(TypeCamera.DEFAULT);
            var oldDamping = Vector3.zero;
            var camPersonFollow = cam.VCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (camPersonFollow != null)
            {
                oldDamping = camPersonFollow.Damping;
                camPersonFollow.Damping = Vector3.zero;
            }

            if (cam != null)
            {
                cam.VCamera.transform.forward = startPosTr.forward;
                var camfollow = cam.VCamera.Follow;
                camfollow.LookAt(startPosTr.forward);
                var tI = player.GetComponent<ThirdPersonInput>();
                tI.TargetYRotation = camfollow.eulerAngles.y;
            }

            if (camPersonFollow != null)
            {
                camPersonFollow.Damping = oldDamping;
            }

            IsInitializedComplete = true;

            if (ZeusSceneManager.Get() == null)
            {
                Debug.LogWarning("Not Found ZeusSceneManager");
                InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);
            }
        }

        internal int GetZoneString(int zoneID) 
        {
            foreach (var item in ZoneDataList)
            {
                if (item.Info.ZoneID == zoneID)
                {
                    return item.Info.AreaID;
                }
            }

            return 0;
        }

        private void ZoneClearCheck(Character character)
        {
            if (IsInitializedComplete == false || !Application.isPlaying || TableManager.ApplicationIsQuitting)
            {
                Debug.Log("Zone not cleared");
                return;
            }

            var removeCharacterZoneID = character.ZoneID;
            var array = CharacterObjectManager.Get().GetZoneCharacter(removeCharacterZoneID, TypeCharacter.AI);
            if (array.Length == 0)
            {
                var findData = ZoneDataList.Find(_ => _.Info.ZoneID == removeCharacterZoneID);
                if (findData != null)
                {
                    findData.CharacterClear = true;
                    TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs.Add(findData.Info.ZoneID);
                    TableManager.AutoSave();
                    Debug.Log($"Zone {findData.Info.ZoneID} is clear! : {Time.time}");
                    ZoneClearCallback?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying || TableManager.ApplicationIsQuitting)
            {
                return;
            }

            if (CharacterObjectManager.Get() != null)
                CharacterObjectManager.Get().CallCharacterRemove -= ZoneClearCheck;
        }

        internal void Initialized()
        {
            IsInitializedComplete = false;
            if (ZoneDataList == null)
            {
                Debug.LogError("ZoneDataList null");
                return;
            }

            GameTimeManager.Instance.RevertTime();
            GameTimeManager.Instance.ClearCoolTime();
            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null)
            {
                Debug.LogError("player is null");
                return;
            }

            var index = ZoneDataList.FindIndex((x) => x.Info.ZoneID == player.ZoneID);
            if (index == -1)
            {
                Debug.LogError("Not Found ZoneData Find ZoneID is === " + player.ZoneID);
                return;
            }

            var startPosTr = ZoneDataList[index].PlayerStartPosition;
            if (startPosTr == null)
            {
                Debug.LogError("PlayerStartPosition is null!!!");
                return;
            }

            for (int i = 0; i < ZoneDataList.Count; i++)
            {
                var zone = ZoneDataList[i];
                if (zone != null)
                {
                    zone.Initialized();
                }
            }

            player.ResetHealth();
            player.IsImmortal = true;
            player.transform.SetPositionAndRotation(startPosTr.position, startPosTr.rotation);

            var playerCombatManager = player.GetComponent<PlayerCombatManager>();
            if (playerCombatManager != null)
            {
                //들고있던 무기 꺼준다.
                playerCombatManager.WeaponDissolve(0.05f, TypeGrapHand.RIGHT, 0.01f);
                playerCombatManager.WeaponDissolve(0.05f, TypeGrapHand.LEFT, 0.01f);
            }

            var thirdPersonController = player as ThirdPersonController;
            if (thirdPersonController != null)
            {
                thirdPersonController.RotateToDirection(startPosTr.forward, 100f);
            }

            CameraManager.Get().TurnOnCamera(TypeCamera.DEFAULT);
            var cam = CameraManager.Get().GetCamera(TypeCamera.DEFAULT);
            var oldDamping = Vector3.zero;
            var camPersonFollow = cam.VCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (camPersonFollow != null)
            {
                oldDamping = camPersonFollow.Damping;
                camPersonFollow.Damping = Vector3.zero;
            }

            if (cam != null)
            {
                cam.VCamera.transform.forward = startPosTr.forward;
                var camfollow = cam.VCamera.Follow;
                camfollow.LookAt(startPosTr.forward);
                var tI = player.GetComponent<ThirdPersonInput>();
                tI.TargetYRotation = camfollow.eulerAngles.y;
            }

            FadeManager.Instance.DoFade(false, 1f , 1f, () => 
            {
                player.IsImmortal = false;
                if (camPersonFollow != null)
                {
                    camPersonFollow.Damping = oldDamping;
                }

                InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);

                ZoneInitializedCallback?.Invoke();
                IsInitializedComplete = true;
            });
        }
    }
}