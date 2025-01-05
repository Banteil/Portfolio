using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class ShopManager : Singleton<ShopManager>
    {
        private List<ItemData> shopItemDataList = new List<ItemData>();

        protected override void OnAwake()
        {
            base.OnAwake();
            LocalizationSettings.SelectedLocaleChanged += LocaleRecachingProcess;
            UserDataManager.Instance.CompleteSetMyDataCallback += CachingProcess;
        }

        async private void CachingProcess() => shopItemDataList = await CallItemList(ItemType.All);

        async public UniTask RecachingList() => shopItemDataList = await CallItemList(ItemType.All);

        private void LocaleRecachingProcess(Locale locale) => CachingProcess();

        public List<ItemData> GetItemTypeList(ItemType type)
        {
            var itemTypeLIst = shopItemDataList.Where((data) => data.type == (int)type).ToList();
            return itemTypeLIst;
        }

        async private UniTask<List<ItemData>> CallItemList(ItemType itemType)
        {
            var tcs = new UniTaskCompletionSource<List<ItemData>>();

            var cardSkinType = (int)itemType;
            var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
            await CallAPI.APISelectItemListLastPageNum(UserDataManager.Instance.MySid, 1, cardSkinType, null, async (pageNum) =>
            {
                await CallAPI.APISelectItemList(UserDataManager.Instance.MySid, pageNum, 1, cardSkinType, i18n, null, (data) =>
                {
                    tcs.TrySetResult(data.ToList());
                });
            });

            return await tcs.Task;
        }
    }
}
