using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class ROOSceneManager : ZeusSceneManager
    {
        public MoviePlayer[] Player;

        private const int _bossBGMID = 103;

        protected override IEnumerator Start()
        {
            yield return base.Start();
            yield return new WaitUntil(() => ZoneDataManager.Get().IsInitializedComplete);
            yield return new WaitUntil(() => PlayerUIManager.Get() != null);
            
            CharacterObjectManager.Get().CallCharacterAdd += InactiveBoss;
            var bossTypeAI = FindObjectOfType<BossTypeAI>();
            if (bossTypeAI != null)
            {
                var boss = bossTypeAI.GetComponent<Character>();
                InactiveBoss(boss);
            }

            if (TableManager.IsNewGame)
            {
                MoviePlay(0);
                yield break;
            }
            else
                FadeManager.Instance.DoFade(false, 1f, 0f, () => StartFX());
        }

        private void StartFX()
        {
            //InputReader.Instance.EnableMapPlayerControls(true, false);
            //InputReader.Instance.EnableMapBattleMod(true, false);
            InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);
            InputReader.Instance.Enable = true;
            var zoneStringID = ZoneDataManager.Get().GetZoneString(TableManager.CurrentPlayerData.SaveZoneData.ZoneID);
            var str = TableManager.GetString(zoneStringID);
            PlayerUIManager.Get().GetUI<PlayerTextTypeUI>(TypePlayerUI.AREA).SetVisible(str);
            PlayerUIManager.Get().HUDVisible(true, 0.1f);
            PlaySceneBGM();
        }

        internal void MoviePlay(int index)
        {
            FadeManager.Instance.DoFade(true, 1f, 0f, () =>
            {
                SoundManager.Instance.BGMFade(true, 0.5f);
                PlayerUIManager.Get().HUDVisible(false, 0.1f);
                //InputReader.Instance.EnableMapPlayerControls(false, true);
                //InputReader.Instance.EnableMapBattleMod(false, true);
                //InputReader.Instance.EnableMapUI(false, true);
                InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE | TypeInputActionMap.UI);
                Player[index].Play();
            });
        }

        public void ActiveBoss()
        {
            var boss = FindObjectOfType<BossTypeAI>();
            boss.ActiveBossEvent();
            boss.AfterDieEvent.AddListener(() => FadeManager.Instance.DoFade(true, 1f, 0f, () => MoviePlay(1)));
            SoundManager.Instance.BGMFade(true, 3f, () =>
            {
                SoundManager.Instance.PlayAsync(_bossBGMID, Vector3.zero, true);
            });
        }

        void InactiveBoss(Character character)
        {
            var boss = character.GetComponent<BossTypeAI>();
            if (boss == null) return;
            boss.InactiveBossEvent();
        }

        public void StartMovie()
        {
            FadeManager.Instance.DoFade(false, 0.5f);
        }

        public void FinishMovie(int index)
        {
            if (index == 0)
            {
                FadeManager.Instance.DoFade(true, 0.5f, 0f, () =>
                {
                    Player[index].MovieRenderer.enabled = false;
                    FadeManager.Instance.DoFade(false, 1f, 0f, () =>
                    {
                        StartFX();
                    });
                });
            }
            else if (index == 1)
            {
                SceneLoadManager.Instance.LoadScene("TitleScene");
            }
        }
    }
}