using System.Collections.Generic;
using System.IO;
using starinc.io.kingnslave;
using UnityEngine;

namespace starinc.io
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        [SerializeField] private SoundTable _soundTablePrefab;
        [SerializeField] private SpriteTable _spriteTablePrefab;
        [SerializeField] private CardSpriteTable _cardSpriteTablePrefab;
        [SerializeField] private PrefabTable _prefabTable;
        [SerializeField] private StageInfoTable _stageInfoTableAsset;

        private SoundTable soundTable;
        private SpriteTable spriteTable;
        private CardSpriteTable cardSpriteTable;
        private PrefabTable prefabTable;
        private StageInfoTable stageInfoTable;

        [SerializeField] private GameObject connectingUI;
        public GameObject ConnectingUI { get { return connectingUI; } }

        public List<Sprite> FlagSpriteList { get { return spriteTable.CountryFlagSprites; } }

        protected override void OnAwake()
        {
            base.OnAwake();
            Initialize();
        }

        private void Initialize()
        {
#if UNITY_EDITOR
            soundTable = Instantiate(_soundTablePrefab);
            spriteTable = Instantiate(_spriteTablePrefab);
            cardSpriteTable = Instantiate(_cardSpriteTablePrefab);
            prefabTable = Instantiate(_prefabTable);
            stageInfoTable = Instantiate(_stageInfoTableAsset);
#else
            soundTable = _soundTablePrefab;
            spriteTable = _spriteTablePrefab;
            cardSpriteTable = _cardSpriteTablePrefab;
            prefabTable = _prefabTable;
            stageInfoTable = _stageInfoTableAsset;
#endif
            Debug.Log("ResourceManager Initialize Complete");
        }

        public GameObject Instantiate(string name, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var prefabData = prefabTable.PrefabObjects.Find((data) => data.Name == name);
            if (prefabData == null)
            {
                Debug.Log($"프리팹 로드에 실패하였습니다 : {name}");
                return null;
            }
            GameObject obj = Object.Instantiate(prefabData.PrefabObject, parent, instantiateInWorldSpace);
            obj.ExcludingClone();
            return obj;
        }

        public AudioClip GetBGMClip(int index)
        {
            if (index < 0 || index >= soundTable.BGMDatas.Count)
                return null;
            return soundTable.BGMDatas[index].Clip;
        }

        public AudioClip GetBGMClip(Define.BGMTableIndex index)
        {
            return GetBGMClip((int)index);
        }

        public AudioClip GetBGMClip(string name)
        {
            for (int i = 0; i < soundTable.BGMDatas.Count; i++)
            {
                if (soundTable.BGMDatas[i].Name.Contains(name))
                    return soundTable.BGMDatas[i].Clip;
            }
            return null;
        }

        public AudioClip GetSFXClip(int index)
        {
            if (index < 0 || index >= soundTable.SFXDatas.Count)
                return null;
            return soundTable.SFXDatas[index].Clip;
        }

        public AudioClip GetSFXClip(Define.SFXTableIndex index)
        {
            if ((int)index < 0 || (int)index >= soundTable.SFXDatas.Count)
                return null;
            return soundTable.SFXDatas[(int)index].Clip;
        }

        public AudioClip GetSFXClip(string name)
        {
            for (int i = 0; i < soundTable.SFXDatas.Count; i++)
            {
                if (soundTable.SFXDatas[i].Name.Contains(name))
                    return soundTable.SFXDatas[i].Clip;
            }
            return null;
        }

        public Sprite GetSprite(int index)
        {
            if (index < 0 || index >= spriteTable.SpriteDatas.Count)
                return null;
            return spriteTable.SpriteDatas[index].Sprite;
        }

        public Sprite GetSprite(string name)
        {
            for (int i = 0; i < spriteTable.SpriteDatas.Count; i++)
            {
                if (spriteTable.SpriteDatas[i].Name.Contains(name))
                    return spriteTable.SpriteDatas[i].Sprite;
            }
            return null;
        }

        public Sprite GetTierSprite(int tier, int division)
        {
            var name = $"{tier}_{division}";
            for (int i = 0; i < spriteTable.TierDatas.Count; i++)
            {
                if (spriteTable.TierDatas[i].Name.Contains(name))
                    return spriteTable.TierDatas[i].Sprite;
            }
            return null;
        }

        public Sprite GetCardCharacterSprite(Define.CardType cardIndex)
        {
            if ((int)cardIndex < 0 || (int)cardIndex >= cardSpriteTable.CharacterSpriteDataList.Count)
                return null;
            return cardSpriteTable.CharacterSpriteDataList[(int)cardIndex].Sprite;
        }

        public Sprite GetCardBackgroundSprite(Define.CardType cardIndex)
        {
            if ((int)cardIndex < 0 || (int)cardIndex >= cardSpriteTable.BackgroundSpriteDataList.Count)
                return null;
            return cardSpriteTable.BackgroundSpriteDataList[(int)cardIndex].Sprite;
        }

        public Sprite GetCardNameplateSprite(int index)
        {
            if ((int)index < 0 || (int)index >= cardSpriteTable.NameplateSpriteDataList.Count)
                return null;
            return cardSpriteTable.NameplateSpriteDataList[(int)index].Sprite;
        }

        public Sprite GetCardFrameSprite(int index)
        {
            if ((int)index < 0 || (int)index >= cardSpriteTable.FrameSpriteDataList.Count)
                return null;
            return cardSpriteTable.FrameSpriteDataList[(int)index].Sprite;
        }

        public Sprite GetCardBackSideSprite(int index)
        {
            if ((int)index < 0 || (int)index >= cardSpriteTable.BackSideSpriteDataList.Count)
                return null;
            return cardSpriteTable.BackSideSpriteDataList[(int)index].Sprite;
        }

        /// <summary>
        /// 스테이지 정보를 가져오는 메서드
        /// </summary>
        /// <param name="stage">[1, 10] 범위</param>
        /// <returns></returns>
        public StageInfoData GetStageInfoData(int stage)
        {
            if (stage <= 0 || stage > stageInfoTable.StageInfos.Count)
                return null;
            return stageInfoTable.StageInfos[--stage];
        }
    }
}
