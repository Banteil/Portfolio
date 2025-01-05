using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class CallAPI : MonoBehaviour
    {
        #region API URL
        /// <summary>
        /// 신규 시스템 아이디 발급 <br>
        /// POST</br>
        /// </summary>
        public const string API_InsertUser = "/user/insertUser";
        /// <summary>
        /// 사용자 조회 <br>
        /// GET param : string sid, string sidTarget </br>
        /// </summary>
        public const string API_SelectUser = "/user/selectUser";
        /// <summary>
        /// 사용자 등록 <br>
        /// POST param : string sid, string uid, string pwd, string nickname, string email </br>
        /// </summary>
        public const string API_UpdateUser = "/user/updateUser";
        /// <summary>
        /// 사용자 아이디 수정 <br>
        /// POST param : string sid, string uid </br>
        /// </summary>
        public const string API_UpdateUserUid = "/user/updateUserUid";
        /// <summary>
        /// 사용자 패스워드 수정 <br>
        /// POST param : string sid, string pwdOld, string pwdNew </br>
        /// </summary>
        public const string API_UpdateUserPwd = "/user/updateUserPwd";
        /// <summary>
        /// 사용자 닉네임 수정 <br>
        /// POST param : string sid, string nickname </br>
        /// </summary>
        public const string API_UpdateUserNickname = "/user/updateUserNickname";
        /// <summary>
        /// 사용자 이메일 수정 <br>
        /// POST param : string sid, string email </br>
        /// </summary>
        public const string API_UpdateUserEmail = "/user/updateUserEmail";
        /// <summary>
        /// 사용자 프로필 이미지 수정 <br>
        /// POST param : string sid, string profileImage </br>
        /// </summary>
        public const string API_UpdateUserProfileImage = "/user/updateUserProfileImage";
        /// <summary>
        /// 사용자 일반 게임 전적 조회 <br>
        /// POST param : string sid, string sidTarget </br><br>
        /// </summary>
        public const string API_SelectUserNormalGameResult = "/user/selectUserNormalGameResult";
        /// <summary>
        /// 사용자 랭크 게임 전적 수정 <br>
        /// POST param : string sid, string sidTarget </br><br>
        /// </summary>
        public const string API_SelectUserRankGameResult = "/user/selectUserRankGameResult";
        /// <summary>
        /// 사용자 아이디 유효성 검사 <br>
        /// POST param : string sid, string uid </br>
        /// </summary>
        public const string API_ValidateUid = "/user/validateUid";
        /// <summary>
        /// 이메일 검증 코드 전송 <br>
        /// POST param : string sid, string email </br>
        /// </summary>
        public const string API_SendEmailVerificationCode = "/common/sendEmailVerificationCode";
        /// <summary>
        /// 이메일 검증 코드 확인 <br>
        /// POST param : string sid, string email, string code </br>
        /// </summary>
        public const string API_VerifyEmailVerificationCode = "/common/verifyEmailVerificationCode";
        /// <summary>
        /// 게임 룸 생성 및 정보 획득 <br>
        /// POST param : string sid, string sidTarget, string roomId, int gameType </br>
        /// </summary>
        public const string API_GameRoom_InitGame = "/gameroom/initGame";
        /// <summary>
        /// 인게임 액션 정보 전송 <br>
        /// POST param : string sid, sting roomId, int roundNo, int gameType, string actionCd, string cardCd, string expressionCd </br>
        /// </summary>
        public const string API_GameRoom_InsertGameAction = "/gameroom/insertGameAction";
        /// <summary>
        /// 라운드 종료 시 라운드 결과 전송 <br>
        /// POST param : string sid, string sidTarget, sting roomId, int roundNo, int gameType, string sidBlue, string sidRed, string cardArray, string submitBlue, string submitRed, string sidWinner, string sidLose </br>
        /// </summary>
        public const string API_GameRoom_FinishRound = "/gameroom/finishRound";
        /// <summary>
        /// 게임 종료 시 게임 결과 전송 <br>
        /// POST param : string sid, string sidTarget, sting roomId, int gameType, string sidWinner, string sidLose </br>
        /// </summary>
        public const string API_GameRoom_FinishGame = "/gameroom/finishGame";
        /// <summary>
        /// 사용자 아이디로 로그인 <br>
        /// POST param : string sid, string sidTarget, sting pwd </br>
        /// </summary>
        public const string API_Login = "/login/doLogin";
        /// <summary>
        /// 사용자 SNS 아이디로 로그인 <br>
        /// </br>
        /// </summary>
        public const string API_SNS_Login = "/login/snsLogin";
        /// <summary>
        /// 랭크 게임 랭킹 탑 리스트 조회 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_SelectRankRankingsTopList = "/rankings/selectRankRankingsTopList";
        /// <summary>
        /// 랭크 게임 랭킹 탑 리스트 조회 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_SelectNormalRankingsTopList = "/rankings/selectNormalRankingsTopList";
        /// <summary>
        /// 랭크 게임 랭킹 탑 리스트 조회 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_SelectSingleRankingsTopList = "/rankings/selectSingleRankingsTopList";
        /// <summary>
        /// 게임룸 사용자 대전기록 리스트 조회 <br>
        /// GET param : string sid, string sidTarget, int pageSize, int pageNum </br>
        /// </summary>
        public const string API_SelectGameRoomHistoryList = "/gameroom/selectGameRoomHistoryList";
        /// <summary>
        /// 게임룸 사용자 대전기록 리스트 마지막 페이지 번호 조회 <br>
        /// GET param : string sid, string sidTarget, int pageSize </br>
        /// </summary>
        public const string API_SelectGameRoomHistoryListLastPageNum = "/gameroom/selectGameRoomHistoryListLastPageNum";
        /// <summary>
        /// 게임 라운드 리스트 조회 <br>
        /// GET param : string sid, string roomId </br>
        /// </summary>
        public const string API_SelectGameRoundList = "/gameroom/selectGameRoundList";
        /// <summary>
        /// 사용자 제출 통계 조회 <br>
        /// GET param : string sid, string sidTarget </br>
        /// </summary>
        public const string API_SelectGameRoundUserSubmitStatistics = "/gameroom/selectGameRoundUserSubmitStatistics";
        /// <summary>
        /// 키밸류 조회 <br>
        /// GET param : string sid, string cdKey </br>
        /// </summary>
        public const string API_SelectKeyValue = "/common/selectKeyValue";
        /// <summary>
        /// 랭크게임 조건 만족여부 체크 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_CheckUserRankPrecondition = "/user/checkUserRankPrecondition";
        /// <summary>
        /// 프로필 디폴트 리스트 조회 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_SelectUserProfileDefaultList = "/user/selectUserProfileDefaultList";
        /// <summary>
        /// 광고매체 조회 <br>
        /// GET param : string sid </br>
        /// </summary>
        public const string API_SelectAdMedium = "/admedium/selectAdMedium";
        /// <summary>
        /// 광고 보상 처리 <br>
        /// POST param : string sid </br>
        /// </summary>
        public const string API_UpdateUserAdReward = "/user/updateUserAdReward";
        /// <summary>
        /// 상점 아이템 리스트 마지막 페이지 번호 조회 <br>
        /// GET param : string sid, int pageSize, int itemType, string keyword </br>
        /// </summary>
        public const string API_SelectItemListLastPageNum = "/item/selectItemListLastPageNum";
        /// <summary>
        /// 상점 아이템 리스트 조회 <br>
        /// GET param : string sid, int pageSize, int pageNum, int itemType, string locale, string keyword </br>
        /// </summary>
        public const string API_SelectItemList = "/item/selectItemList";
        /// <summary>
        /// 사용자 아이템 리스트 마지막 페이지 번호 조회 <br>
        /// GET param : string sid, int pageSize, int itemType </br>
        /// </summary>
        public const string API_SelectItemUserListLastPageNum = "/itemuser/selectItemUserListLastPageNum";
        /// <summary>
        /// 사용자 아이템 리스트 조회 <br>
        /// GET param : string sid, int pageSize, int pageNum, string locale, int itemType </br>
        /// </summary>
        public const string API_SelectItemUserList = "/itemuser/selectItemUserList";
        /// <summary>
        /// 아이템 구매 <br>
        /// POST param : string sid, int itemSeq, int amount, string locale </br>
        /// </summary>
        public const string API_BuyItem = "/item/buyItem";
        /// <summary>
        /// 사용자 아이템 사용중 수정 <br></br>
        /// POST param : string sid, int itemUserSeq, int itemType <br>
        /// inUse : 0(미사용), 1(사용중) </br>
        /// </summary>
        public const string API_UpdateItemUserInUse = "/itemuser/updateItemUserInUse";
        /// <summary>
        /// 사용자 국가 수정 <br>
        /// POST param : string sid, int countrySeq </br>
        /// </summary>
        public const string API_UpdateUserCountrySeq = "/user/updateUserCountrySeq";
        /// <summary>
        /// 사용자 스테이지 수정
        /// POST param : string sid, int singleStage <br>
        /// singleStage: 현재까지 클리어한 스테이지(0부터 시작) </br>
        /// </summary>
        public const string API_UpdateUserSingleStage = "/user/updateUserSingleStage";
        /// <summary>
        /// 사용자 젬 수정
        /// POST param : string sid, int addGemAmount, int gemLogType <br>
        /// gemLogType: 3(싱글플레이 중간보스 보상), 4(싱글플레이 중간보스 광고 보상), 5(싱글플레이 보스 보상), 6(싱글플레이 보스 광고 보상) </br>
        /// </summary>
        public const string API_UpdateUserGemAmount = "/user/updateUserGemAmount";
        /// <summary>
        /// KPI 로그 등록
        /// POST param : string sid, int action_no, string action_cd, string params <br>
        /// 파라미터 여러개일 경우 | 로 파싱해서 쓸 수 있도록 보냄 </br>
        /// </summary>
        public const string API_InsertKpiLog = "/log/insertKpiLog";
        #endregion

        async public static UniTask APIInsertUser(Action<string> callback)
        {
            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var sid = (string)responseData.returnValue;
                    callback(sid);
                }
                else
                {
                    Debug.LogError("Insert User FAIL");
                    callback(null);
                }
            }, API_InsertUser);
        }

        async public static UniTask APISelectUser(string sid, string sidTarget, Action<UserData> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback(userData);
                }
                else
                {
                    Debug.LogError("Select User FAIL");
                    callback(null);
                }
            }, API_SelectUser, $"sid={sid}", $"sidTarget={sidTarget}");
        }

        async public static UniTask APIUpdateUser(string sid, string uid, string pwd, string email, int loginType, Action<string> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("uid", uid);
            form.AddField("pwd", pwd);
            form.AddField("email", email);
            form.AddField("loginType", loginType);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var returnValue = (string)(responseData.returnValue);
                    callback?.Invoke(returnValue);
                    Debug.Log("Update User SUCCESS");
                }
                else
                {
                    Debug.LogError("Update User FAIL");
                }
            }, API_UpdateUser, form);
        }

        async public static UniTask APIUpdateUserUid(string sid, string uid, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("uid", uid);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                callback(responseData.returnCd);
            }, API_UpdateUserUid, form);
        }

        async public static UniTask APIUpdateUserPassword(string sid, string pwdOld, string pwdNew, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("pwdOld", pwdOld);
            form.AddField("pwdNew", pwdNew);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                callback(responseData.returnCd);
            }, API_UpdateUserPwd, form);
        }

        async public static UniTask APIUpdateUserNickName(string sid, string nickName, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("nickname", nickName);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                callback(responseData.returnCd);
            }, API_UpdateUserNickname, form);
        }

        async public static UniTask APIUpdateUserEmail(string sid, string email, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("email", email);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                callback(responseData.returnCd);
            }, API_UpdateUserEmail, form);
        }

        async public static UniTask APIUpdateUserProfileImage(string sid, string profile_image, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("profileImage", profile_image);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                callback(responseData.returnCd);
            }, API_UpdateUserProfileImage, form);
        }

        async public static UniTask APIValidateUid(string sid, string uid, Action<int> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                var returnCd = responseData.returnCd;
                callback(returnCd);
            }, API_ValidateUid, $"sid={sid}", $"uid={uid}");
        }

        async public static UniTask APISelectUserNormalGameResult(string sid, string sidTarget, Action<NormalGameResultData> callback = null)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var normalGameResultData = Util.CastingJsonObject<NormalGameResultData>(responseData.returnValue);
                    callback?.Invoke(normalGameResultData);
                }
                else
                {
                    Debug.LogError("Select User FAIL");
                }
            }, API_SelectUserNormalGameResult, $"sid={sid}", $"sidTarget={sidTarget}");
        }

        async public static UniTask APISelectUserRankGameResult(string sid, string sidTarget, Action<RankGameResultData> callback = null)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var rankGameResultData = Util.CastingJsonObject<RankGameResultData>(responseData.returnValue);
                    callback?.Invoke(rankGameResultData);
                }
                else
                {
                    Debug.LogError("Select User FAIL");
                }
            }, API_SelectUserRankGameResult, $"sid={sid}", $"sidTarget={sidTarget}");
        }

        async public static UniTask APISendEmailVerificationCode(string sid, string email, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("email", email);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                var returnCd = responseData.returnCd;
                callback(returnCd);
            }, API_SendEmailVerificationCode, form);
        }

        async public static UniTask APIVerifyEmailVerificationCode(string sid, string email, string code, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("email", email);
            form.AddField("code", code);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log(responseData.resultMsg);
                var returnCd = responseData.returnCd;
                callback(returnCd);
            }, API_VerifyEmailVerificationCode, form);
        }

        async public static UniTask APIDoLogin(UserData userData, string uid, string password, Action<UserData> callback, Action<int> returnCdCallback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("uid", uid);
            form.AddField("pwd", password);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback?.Invoke(userData);
                }
                else
                {
                    Debug.LogError("User Log In FAIL");
                }
                returnCdCallback?.Invoke(responseData.returnCd);
            }, API_Login, form);
        }

        async public static UniTask APIDoSNSLogin(UserData userData, Define.LoginType loginType, string email, string profileImage, string snsToken, string snsName, Action<UserData> callback, Action<int> returnCdCallback)
        {
            if (profileImage == string.Empty || profileImage == null)
            {
                profileImage = UserDataManager.Instance.MyData.profile_image;
            }

            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("loginType", (int)loginType);
            form.AddField("email", email);
            form.AddField("profileImage", profileImage);
            form.AddField("snsToken", snsToken);
            form.AddField("snsName", snsName);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback?.Invoke(userData);
                }
                else
                {
                    Debug.LogError("User Log In FAIL");
                }
                returnCdCallback?.Invoke(responseData.returnCd);
            }, API_SNS_Login, form);
        }

        async public static UniTask APIGameRoomInitGame(UserData userData, UserData opponentData)
        {
            GameManager.Instance.InitializeGameRoom(null);
            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("sidTarget", opponentData.sid);
            form.AddField("roomId", NetworkManager.Instance.RoomId);
            form.AddField("gameType", GameManager.Instance.GetAPIGameTypeFromGameMode());
            form.AddField("region", NetworkManager.Instance.MyRunner.SessionInfo.Region);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                if (www == null)
                {
                    Debug.LogError("FAIL");
                    return;
                }

                var json = www.downloadHandler.text;
                Debug.Log($"<color=green>json {json}</color>");
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log($"<color=green>responseData {responseData}</color>");
                Debug.Log($"<color=green>responseData {responseData.resultCd}</color>");
                Debug.Log($"<color=green>responseData {responseData.resultMsg}</color>");
                Debug.Log($"<color=green>responseData {responseData.returnCd}</color>");
                Debug.Log($"<color=green>returnValue {responseData.returnValue}</color>");
                if (responseData.resultCd == SUCCESS)
                {
                    var gameRoomData = Util.CastingJsonObject<GameRoomData>(responseData.returnValue);
                    Debug.Log($"<color=white>seq:{gameRoomData.seq} id:{gameRoomData.id} title:{gameRoomData.title} blue:{gameRoomData.sid_blue} red:{gameRoomData.sid_red}</color>");
                    GameManager.Instance.InitializeGameRoom(gameRoomData);
                }
                else
                {
                    Debug.LogError("FAIL");
                }
            }, API_GameRoom_InitGame, form);
        }

        public static IEnumerator APIGameRoomInsertGameAction(UserData userData, string actionCd, Define.CardType cardType, int cardNo, string expressionCd = "")
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("roomId", NetworkManager.Instance.RoomId);
            form.AddField("roundNo", InGameManager.Instance.Round);
            form.AddField("gameType", GameManager.Instance.GetAPIGameTypeFromGameMode());
            form.AddField("actionCd", actionCd);
            form.AddField("cardCd", GetAPICardCdFromCardType(cardType));
            form.AddField("cardNo", cardNo);
            form.AddField("expressionCd", expressionCd);

            yield return NetworkManager.Instance.PostRequestCoroutine((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    Debug.Log("SUCCESS");
                }
                else
                {
                    Debug.LogError("FAIL");
                }
            }, API_GameRoom_InsertGameAction, form);
        }

        private static string GetAPICardCdFromCardType(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Slave:
                    return ((char)APICardCd.Slave).ToString();
                case CardType.King:
                    return ((char)APICardCd.King).ToString();
                case CardType.Citizen:
                    return ((char)APICardCd.Citizen).ToString();
                default:
                    return string.Empty;
            }
        }

        async public static void APIGameRoomFinishRound(UserData userData, UserData opponentData, string cardArray, string submitBlue, string submitRed, string sidWinner, string sidLoser)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("sidTarget", opponentData.sid);
            form.AddField("roomId", NetworkManager.Instance.RoomId);
            form.AddField("roundNo", InGameManager.Instance.Round);
            form.AddField("gameType", GameManager.Instance.GetAPIGameTypeFromGameMode());
            form.AddField("sidBlue", GameManager.Instance.RoomData.sid_blue);
            form.AddField("sidRed", GameManager.Instance.RoomData.sid_red);
            form.AddField("cardArray", cardArray);
            form.AddField("submitBlue", submitBlue);
            form.AddField("submitRed", submitRed);
            form.AddField("sidWinner", sidWinner);
            form.AddField("sidLoser", sidLoser);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                Debug.Log("[Round Finish]" + json);

                if (responseData.resultCd == SUCCESS)
                {
                    Debug.Log("[Round Finish] SUCCESS");
                }
                else
                {
                    Debug.LogError("FAIL");
                }
            }, API_GameRoom_FinishRound, form);
        }

        async public static UniTask APIGameRoomFinishGame(UserData userData, UserData opponentData, string sidWinner, string sidLoser, int scoreWinner, int scoreLoser)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", userData.sid);
            form.AddField("sidTarget", opponentData.sid);
            form.AddField("roomId", NetworkManager.Instance.RoomId);
            form.AddField("gameType", GameManager.Instance.GetAPIGameTypeFromGameMode());
            form.AddField("sidWinner", sidWinner);
            form.AddField("sidLoser", sidLoser);
            form.AddField("scoreWinner", Mathf.Clamp(scoreWinner, 0, DEFAULT_WIN_CONDITION_SCORE));
            form.AddField("scoreLoser", Mathf.Clamp(scoreLoser, 0, DEFAULT_WIN_CONDITION_SCORE));

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    Debug.Log("SUCCESS");
                }
                else
                {
                    Debug.LogError("FAIL");
                }
            }, API_GameRoom_FinishGame, form);
        }

        async public static UniTask APISelectRankRankingsTopList(string sid, Action<List<UserData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<List<UserData>>(responseData.returnValue);
                    callback(userData);
                }
                else
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                }
            }, API_SelectRankRankingsTopList, $"sid={sid}");
        }

        async public static UniTask APISelectNormalRankingsTopList(string sid, Action<List<UserData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<List<UserData>>(responseData.returnValue);
                    callback(userData);
                }
                else
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                }
            }, API_SelectNormalRankingsTopList, $"sid={sid}");
        }

        async public static UniTask APISelectSingleRankingsTopList(string sid, Action<List<UserData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var userData = Util.CastingJsonObject<List<UserData>>(responseData.returnValue);
                    callback(userData);
                }
                else
                {
                    Debug.LogError("SelectRankRankingsTopList FAIL");
                }
            }, API_SelectSingleRankingsTopList, $"sid={sid}");
        }

        async public static UniTask APISelectGameRoomHistoryList(string sid, string sidTarget, int pageSize, int pageNum, Action<List<GameResultData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectGameRoomHistoryList FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var gameResultData = Util.CastingJsonObject<List<GameResultData>>(responseData.returnValue);
                    callback(gameResultData);
                }
                else
                {
                    Debug.LogError("SelectGameRoomHistoryList FAIL");
                }
            }, API_SelectGameRoomHistoryList, $"sid={sid}", $"sidTarget={sidTarget}", $"pageSize={pageSize}", $"pageNum={pageNum}");
        }

        async public static UniTask APISelectGameRoomHistoryListLastPageNum(string sid, string sidTarget, int pageSize, Action<int> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectGameRoomHistoryListLastPageNum FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var lastPageNum = Convert.ToInt32(responseData.returnValue);
                    callback(lastPageNum);
                }
                else
                {
                    Debug.LogError("SelectGameRoomHistoryListLastPageNum FAIL");
                }
            }, API_SelectGameRoomHistoryListLastPageNum, $"sid={sid}", $"sidTarget={sidTarget}", $"pageSize={pageSize}");
        }

        async public static UniTask APISelectGameRoundList(string sid, string roomId, Action<List<RoundData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                if (json == null)
                {
                    Debug.LogError("SelectGameRoundList FAIL");
                    return;
                }
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var roundDataList = Util.CastingJsonObject<List<RoundData>>(responseData.returnValue);
                    callback(roundDataList);
                }
                else
                {
                    Debug.LogError("SelectGameRoundList FAIL");
                }
            }, API_SelectGameRoundList, $"sid={sid}", $"roomId={roomId}");
        }

        async public static UniTask APISelectUserSubmitStatistics(string sid, string sidTarget, Action<SubmitStatisticsData> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<SubmitStatisticsData>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"Select Statistics FAIL, sidTarget: {sidTarget}");
                }
            }, API_SelectGameRoundUserSubmitStatistics, $"sid={sid}", $"sidTarget={sidTarget}");
        }

        async public static UniTask APISelectKeyValue(string sid, string cdKey, Action<object> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    callback?.Invoke(responseData.returnValue);
                }
                else
                {
                    Debug.LogError($"SelectKeyValue FAIL");
                    callback?.Invoke(null);
                }
            }, API_SelectKeyValue, $"sid={sid}", $"cdKey={cdKey}");
        }

        async public static UniTask APICheckUserRankPrecondition(string sid, Action<UserRankPreconditionData> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<UserRankPreconditionData>(responseData.returnValue);
                    data.returnCd = responseData.returnCd;
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"SelectKeyValue FAIL");
                }
            }, API_CheckUserRankPrecondition, $"sid={sid}");
        }

        //async public static UniTask APISelectUserProfileDefaultList(string sid, Action<List<ProfileImageData>> callback)
        //{
        //    await NetworkManager.Instance.GetRequestTask((www) =>
        //    {
        //        var json = www.downloadHandler.text;
        //        var responseData = Util.JsonToObject<ResponseData>(json);
        //        if (responseData.resultCd == SUCCESS)
        //        {
        //            var data = Util.CastingJsonObject<List<ProfileImageData>>(responseData.returnValue);
        //            callback?.Invoke(data);
        //        }
        //        else
        //        {
        //            Debug.LogError($"SelectUserProfileDefaultList FAIL");
        //        }
        //    }, API_SelectUserProfileDefaultList, $"sid={sid}");
        //}

        /// <summary>
        /// 광고(카드 뒷면)매체 조회 함수
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async public static UniTask APISelectAdMedium(string sid, Action<AdMediumData> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<AdMediumData>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"SelectAdMedium FAIL");
                }
            }, API_SelectAdMedium, $"sid={sid}");
        }

        /// <summary>
        /// 구글 애드몹 리워드 광고 보상 처리 API 함수
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async public static UniTask APIUpdateUserAdReward(string sid, Action<UserData> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"UpdateUserAdReward FAIL");
                    callback?.Invoke(null);
                }
            }, API_UpdateUserAdReward, form);
        }

        async public static UniTask APISelectItemListLastPageNum(string sid, int pageSize, int itemType, string keyword, Action<int> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var lastPageNum = Convert.ToInt32(responseData.returnValue);
                    callback(lastPageNum);
                }
                else
                {
                    Debug.LogError($"SelectItemListLastPageNum FAIL");
                    callback?.Invoke(-1);
                }
            }, API_SelectItemListLastPageNum, $"sid={sid}", $"pageSize={pageSize}", $"itemType={itemType}", $"keyword={keyword}");
        }

        async public static UniTask APISelectItemList(string sid, int pageSize, int pageNum, int itemType, string locale, string keyword, Action<List<ItemData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<List<ItemData>>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"SelectItemList FAIL");
                    callback?.Invoke(null);
                }
            }, API_SelectItemList, $"sid={sid}", $"pageSize={pageSize}", $"pageNum={pageNum}", $"itemType={itemType}", $"locale={locale}", $"keyword={keyword}");
        }

        async public static UniTask APISelectItemUserListLastPageNum(string sid, int pageSize, int itemType, Action<int> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var lastPageNum = Convert.ToInt32(responseData.returnValue);
                    callback(lastPageNum);
                }
                else
                {
                    Debug.LogError($"SelectItemUserListLastPageNum FAIL");
                    callback?.Invoke(-1);
                }
            }, API_SelectItemUserListLastPageNum, $"sid={sid}", $"pageSize={pageSize}", $"itemType={itemType}");
        }

        async public static UniTask APISelectItemUserList(string sid, int pageSize, int pageNum, string locale, int itemType, Action<List<ItemData>> callback)
        {
            await NetworkManager.Instance.GetRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<List<ItemData>>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"SelectItemUserList FAIL");
                    callback?.Invoke(null);
                }
            }, API_SelectItemUserList, $"sid={sid}", $"pageSize={pageSize}", $"pageNum={pageNum}", $"locale={locale}", $"itemType={itemType}");
        }

        async public static UniTask APIBuyItem(string sid, int itemSeq, int amount, string locale, Action<UserData> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("itemSeq", itemSeq);
            form.AddField("amount", amount);
            form.AddField("locale", locale);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"BuyItem FAIL");
                    callback?.Invoke(null);
                }
            }, API_BuyItem, form);
        }

        async public static UniTask APIUpdateItemUserInUse(string sid, int itemUserSeq, int itemSeq, int itemType, string locale, Action<List<ItemData>> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("itemUserSeq", itemUserSeq);
            form.AddField("itemSeq", itemSeq);
            form.AddField("itemType", itemType);
            form.AddField("locale", locale);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<List<ItemData>>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"UpdateItemUserInUse FAIL");
                    callback?.Invoke(null);
                }
            }, API_UpdateItemUserInUse, form);
        }

        async public static UniTask APIUpdateUserCountrySeq(string sid, int countrySeq, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("countrySeq", countrySeq);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    callback?.Invoke(responseData.returnCd);
                }
                else
                {
                    Debug.LogError($"UpdateItemUserInUse FAIL");
                    callback?.Invoke(responseData.returnCd);
                }
            }, API_UpdateUserCountrySeq, form);
        }

        async public static UniTask APIUpdateUserSingleStage(string sid, int stageIndex, Action<int> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("singleStage", stageIndex);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    callback?.Invoke(responseData.returnCd);
                }
                else
                {
                    Debug.LogError($"UpdateUserSingleStage FAIL");
                    callback?.Invoke(responseData.returnCd);
                }
            }, API_UpdateUserSingleStage, form);
        }

        async public static UniTask APIUpdateUserGemAmount(string sid, int addGemAmount, int gemLogType, Action<UserData> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("addGemAmount", addGemAmount);
            form.AddField("gemLogType", gemLogType);

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    var data = Util.CastingJsonObject<UserData>(responseData.returnValue);
                    callback?.Invoke(data);
                }
                else
                {
                    Debug.LogError($"UpdateUserGemAmount FAIL");
                    callback?.Invoke(null);
                }
            }, API_UpdateUserGemAmount, form);
        }

        async public static void APIInsertKpiLog(string sid, int actionNo, string actionCd, string actionParams)
        {
            var currentTime = DateTime.UtcNow;
            var actionTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            WWWForm form = new WWWForm();
            form.AddField("sid", sid);
            form.AddField("actionNo", actionNo);
            form.AddField("actionCd", actionCd);
            form.AddField("actionTime", actionTime);
            form.AddField("params", actionParams == null ? "" : actionParams);

            Debug.Log($"LogData : {sid}, {actionNo}, {actionCd}, {actionTime}, {actionParams}");

            await NetworkManager.Instance.PostRequestTask((www) =>
            {
                var json = www.downloadHandler.text;
                var responseData = Util.JsonToObject<ResponseData>(json);
                if (responseData.resultCd == SUCCESS)
                {
                    Debug.Log($"InsertActionLog SUCCESS");
                }
                else
                {
                    Debug.LogError($"InsertActionLog FAIL");
                }
            }, API_InsertKpiLog, form);
        }
    }
}
