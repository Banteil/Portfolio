using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class UserDataManager : Singleton<UserDataManager>
    {
        private UserData myData;
        public UserData MyData { get { return myData; } set { myData = value; } }
        public string MySid { get { return myData.sid; } }

        private Texture myProfileImage;
        public Texture MyProfileImage
        {
            get { return myProfileImage; }
            set
            {
                myProfileImage = value;
                ChangeMyProfileImageCallback?.Invoke();
            }
        }
        public string MyCardSkinImageUrl;
        public Sprite MyCardSkinImage;

        public List<UserData> OpponentDataList = new List<UserData>();
        public List<Texture> OpponentProfileImageList = new List<Texture>();
        public List<Sprite> OpponentCardSkinImageList = new List<Sprite>();

        #region Caching My Item List
        private List<ItemData> myItemDataList = new List<ItemData>();

        public int MyGem
        {
            get
            {
                return MyData.gem_amount;
            }
            set
            {
                MyData.gem_amount = value;
                SetMyGemCallback?.Invoke();
            }
        }

        public event Action SetMyGemCallback;
        public event Action RecachingCallback;
        #endregion

        #region Callback
        public event Action CompleteSetMyDataCallback;
        public event Action ChangeMyProfileImageCallback;
        public event Action LoginCallback;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            CompleteSetMyDataCallback += ItemCachingProcess;
            LoginCallback += ItemCachingProcess;
            LocalizationSettings.SelectedLocaleChanged += LocaleRecachingProcess;
            SetMyData();
        }

        async private void SetMyData()
        {
            if (PlayerPrefs.HasKey(KEY_SID))
            {
                var mySid = PlayerPrefs.GetString(KEY_SID);
                await CallAPI.APISelectUser(mySid, mySid, (userData) =>
                {
                    myData = userData;
                });

                if (myData == null)
                    await InsertUser();
                else if(!string.IsNullOrEmpty(myData.del_id))
                    await InsertUser();
            }
            else
            {
                await InsertUser();
            }
            Debug.Log($"sid : {MySid}");
            await CacheMyProfileImage();
            CompleteSetMyDataCallback?.Invoke();
            CompleteSetMyDataCallback = null;

            LogManager.Instance.InsertActionLog(46);
        }

        async private UniTask InsertUser()
        {
            var mySid = "";
            await CallAPI.APIInsertUser((sid) =>
            {
                PlayerPrefs.SetString(KEY_SID, sid);
                mySid = sid;
            });

            if (!string.IsNullOrEmpty(mySid))
            {
                await CallAPI.APISelectUser(mySid, mySid, (userData) =>
                {
                    myData = userData;
                });
            }
        }

        async public UniTask LogIn(string uid, string password, Action<int> logInResultCallback)
        {
            await CallAPI.APIDoLogin(myData, uid, Util.SHA512Hash(password), async (userData) =>
            {
                myData = userData;
                PlayerPrefs.SetString(KEY_SID, MySid);
                await CacheMyProfileImage();
                LoginCallback?.Invoke();
            }, logInResultCallback);
        }

        async public UniTask SNSLogIn(LoginType loginType, string email, string profileImage, string snsToken, string snsName, Action<int> logInResultCallback)
        {
            await CallAPI.APIDoSNSLogin(myData, loginType, email, profileImage, snsToken, snsName, async (userData) =>
            {
                myData = userData;
                var profile = FindObjectOfType<UIUserProfile>();
                if (profile != null)
                    profile.SetUserData(userData);
                PlayerPrefs.SetString(KEY_SID, MySid);
                await CacheMyProfileImage();
                LoginCallback?.Invoke();
            }, logInResultCallback);
        }

        async public UniTask CacheMyProfileImage()
        {
            await NetworkManager.Instance.GetTextureTask((texture) =>
            {
                MyProfileImage = texture;
                if (MyProfileImage == null)
                    Debug.Log($"[GetProfileImageFailed]MyProfileImage : {MyProfileImage}");
            }, myData.profile_image);
        }

        async public void UpdateMyNormalGameData()
        {
            await CallAPI.APISelectUserNormalGameResult(myData.sid, myData.sid, (data) =>
            {
                myData.normal_win = data.normal_win;
                myData.normal_lose = data.normal_lose;
                myData.normal_total = data.normal_total;
            });

            Debug.Log($"normalWin:{myData.normal_win}, normalLose:{myData.normal_lose}");
        }

        async public void UpdateMyRankGameData()
        {
            await CallAPI.APISelectUserRankGameResult(myData.sid, myData.sid, (data) =>
            {
                myData.rank_win = data.rank_win;
                myData.rank_lose = data.rank_lose;
                myData.rank_total = data.rank_total;
                myData.rank_division = data.rank_division;
                myData.rank_tier = data.rank_tier;
                myData.rank_point = data.rank_point;
                myData.rank_point_hidden = data.rank_point_hidden;
                myData.mmr = data.mmr;
                myData.promo_yn = data.promo_yn;
                myData.promo_win = data.promo_win;
                myData.promo_lose = data.promo_lose;
                myData.promo_total = data.promo_total;
                myData.promo_result1 = data.promo_result1;
                myData.promo_result2 = data.promo_result2;
                myData.promo_result3 = data.promo_result3;
                myData.promo_result4 = data.promo_result4;
                myData.promo_result5 = data.promo_result5;
            });

            Debug.Log($"rankWin:{myData.rank_win} , rankLose: {myData.rank_lose}");
        }

        /// <summary>
        /// 서버의 실제 젬 정보 Get
        /// </summary>
        /// <returns></returns>
        public async UniTask<int> GetMyGemAsync()
        {
            var tcs = new UniTaskCompletionSource<int>();

            await CallAPI.APISelectUser(MySid, MySid, (data) =>
            {
                if (data != null)
                    tcs.TrySetResult(data.gem_amount);
                else
                    tcs.TrySetResult(-1);
            });

            return await tcs.Task;
        }

        async private void ItemCachingProcess() => myItemDataList = await CallMyItemList(ItemType.All);

        public void LocalRecachingList(List<ItemData> dataList)
        {
            myItemDataList = dataList.ToList();
            RecachingCallback?.Invoke();
        }

        private void LocaleRecachingProcess(Locale locale) => ItemCachingProcess();

        public List<ItemData> GetItemTypeList(ItemType type)
        {
            var itemTypeLIst = myItemDataList.Where((data) => data.type == (int)type).ToList();
            return itemTypeLIst;
        }

        public List<ItemData> GetInGameExpressionList()
        {
            var list = GetItemTypeList(ItemType.Expression);
            var sortingList = list.OrderByDescending((data) => data.order_no).ToList();
            return sortingList;
        }

        async private UniTask<List<ItemData>> CallMyItemList(ItemType itemType)
        {
            var tcs = new UniTaskCompletionSource<List<ItemData>>();

            var type = (int)itemType;
            var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
            await CallAPI.APISelectItemUserListLastPageNum(MySid, 1, type, async (pageNum) =>
            {
                await CallAPI.APISelectItemUserList(MySid, pageNum, 1, i18n, type, (data) =>
                {
                    tcs.TrySetResult(data.ToList());
                });
            });

            return await tcs.Task;
        }

        async public void CheckWithdrawal()
        {
            await CallAPI.APISelectUser(MySid, MySid, (data) =>
            {
                var delId = data.del_id;
                if(!string.IsNullOrEmpty(delId))
                {
                    UIManager.Instance.ShowWarningUI("isWithdrawal", true, () =>
                    {
                        PlayerPrefs.DeleteKey(KEY_SID);
                        Application.Quit();
                    });
                }
            });
        }

        async public void ClearStage(int stageIndex, Action callback)
        {
            // 클리어한 스테이지 갱신
            await CallAPI.APIUpdateUserSingleStage(MySid, stageIndex, (data) =>
            {
                if (data != (int)Define.APIReturnCd.OK)
                {
                    UIManager.Instance.ShowWarningUI("Failure to update stage", false);
                    return;
                }

                MyData.single_stage = stageIndex;
            });

            // 보스 스테이지에서는 젬 획득
            if (MyData.single_stage % Define.FINAL_BOSS_STAGE == 0)
            {
                // 최종 보스 클리어
                await CallAPI.APIUpdateUserGemAmount(MySid, Define.FINAL_BOSS_REWARD_GEM, (int)Define.GemLogType.BossClear, (data) =>
                {
                    if (data != null)
                    {
                        MyGem = data.gem_amount;
                    }
                });
            }
            else if (MyData.single_stage % Define.FINAL_BOSS_STAGE == Define.MIDDLE_BOSS_STAGE)
            {
                // 중간 보스 클리어
                await CallAPI.APIUpdateUserGemAmount(MySid, Define.MIDDLE_BOSS_REWARD_GEM, (int)Define.GemLogType.MiddleBossClear, (data) =>
                {
                    if (data != null)
                    {
                        MyGem = data.gem_amount;
                    }
                });
            }

            callback?.Invoke();
        }
    }
}