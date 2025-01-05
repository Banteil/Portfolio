using System;
//using UnityEngine;
//using System.Threading.Tasks;
//using Google;
//using Firebase.Auth;
//using Firebase.Extensions;
//using Firebase;
//#if UNITY_IOS
//using AppleAuth;
//using AppleAuth.Interfaces;
//using AppleAuth.Native;
//using AppleAuth.Enums;
//#endif

namespace starinc.io
{
    public class UserManager : BaseManager
    {
        //private const string GOOGLE_PROVIDER_ID = "google.com";
        //private const string USER_INFO_KEY = "userInfo";

        //private FirebaseAuth _auth;
        //private FirebaseUser _user;
        //private GoogleSignInConfiguration _googleConfiguration;

        #region Cache
        private UserInfo _userInfo;
        public string SID { get { return _userInfo != null ? _userInfo.sid : "guest"; } }
        public bool IsRemoveAds { get { return _userInfo != null ? _userInfo.isRemoveAds : false; } }
        public bool CompletePreparedUserInfo { get; private set; }
        #endregion
        ///// <summary>
        ///// 현재 사용자가 로그인되어 있는지 여부를 반환합니다.
        ///// </summary>
        //public bool IsSignIn
        //{
        //    get
        //    {
        //        if (_auth == null) return false;
        //        return _auth.CurrentUser != null;
        //    }
        //}

        //private bool _isFirebaseStarted;

        #region Callback
        public event Action OnActiveRemoveAds;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            SettingUserGUID();
            //InitializeFirebase();            
        }

        private async void SettingUserGUID()
        {
            _userInfo = Manager.Game.LoadData<UserInfo>();
            if (string.IsNullOrEmpty(_userInfo.sid))
            {
                var newSid = await CallAPI.InsertUser();
                _userInfo.sid = newSid;
                Manager.Game.SaveData(_userInfo);
            }
            CompletePreparedUserInfo = true;
        }

        public void ActiveRemoveAds()
        {
            _userInfo.isRemoveAds = true;
            Manager.Game.SaveData(_userInfo);
            OnActiveRemoveAds?.Invoke();
        }

//        #region FirebaseAuth
//        /// <summary>
//        /// Firebase 인증 인스턴스를 초기화합니다.
//        /// </summary>
//        private void InitializeFirebase()
//        {
//            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
//            {
//                var dependencyStatus = task.Result;
//                if (dependencyStatus == DependencyStatus.Available)
//                {
//                    _auth = FirebaseAuth.DefaultInstance;
//                    ConfigureGoogleSignIn();
//                    _isFirebaseStarted = true;
//                }
//                else
//                {
//                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
//                }
//            });
//        }

//        /// <summary>
//        /// 이메일과 비밀번호를 사용하여 Firebase 인증을 통해 사용자를 로그인시킵니다.
//        /// </summary>
//        /// <param name="email">사용자의 이메일 주소.</param>
//        /// <param name="password">사용자의 비밀번호.</param>
//        public void SignInWithEmail(string email, string password)
//        {
//            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
//            {
//                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
//                {
//                    Debug.Log(email + " 로 로그인 하셨습니다.");
//                    FirebaseUser signInUser = task.Result.User;
//                    OnFirebaseAuthComplete(signInUser);
//                }
//                else
//                {
//                    Debug.LogError("로그인에 실패하셨습니다.");
//                }
//            });
//        }

//        /// <summary>
//        /// 이메일과 비밀번호를 사용하여 새로운 사용자를 Firebase 인증에 등록합니다.
//        /// </summary>
//        /// <param name="email">사용자의 이메일 주소.</param>
//        /// <param name="password">사용자의 비밀번호.</param>
//        public void RegisterWithEmail(string email, string password)
//        {
//            //현재는 아무 조건 없이 등록시키지만, 추후에는 회사 이메일 인증 시스템을 활용하는 식으로 구현 필요
//            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
//            {
//                if (!task.IsCanceled && !task.IsFaulted)
//                {
//                    Debug.Log(email + "로 회원가입 성공");
//                    SignInWithEmail(email, password);
//                }
//                else
//                {
//                    Debug.LogError("회원가입 실패");
//                }
//            });
//        }

//        #region Google
//        /// <summary>
//        /// Google Sign-In 설정을 구성합니다.
//        /// </summary>
//        private async void ConfigureGoogleSignIn()
//        {
//            var webClientId = await CallAPI.GetAPIKey("web_client_id");
//            Debug.Log($"WebClientId : {webClientId}");
//            _googleConfiguration = new GoogleSignInConfiguration
//            {
//                WebClientId = webClientId,
//                UseGameSignIn = false,
//                RequestIdToken = true,
//                RequestEmail = true,
//                RequestProfile = true,
//                ForceTokenRefresh = true
//            };

//            if (GoogleSignIn.Configuration == null)
//                GoogleSignIn.Configuration = _googleConfiguration;

//            Debug.Log("ConfigureGoogleSignIn Complete");
//        }

//        /// <summary>
//        /// Google Sign-In 프로세스를 시작합니다.
//        /// </summary>
//        public void SignInWithGoogle()
//        {
//            if (!_isFirebaseStarted)
//            {
//                Debug.Log("파이어베이스 시작 안됨");
//                return;
//            }
//            Debug.Log(
//                $"WebClientId: {GoogleSignIn.Configuration.WebClientId}, " +
//                $"UserGameSignIn: {GoogleSignIn.Configuration.UseGameSignIn}, " +
//                $"RequestAuthCode: {GoogleSignIn.Configuration.RequestAuthCode}, " +
//                $"AccountName: {GoogleSignIn.Configuration.AccountName}, " +
//                $"RequestEamil: {GoogleSignIn.Configuration.RequestEmail}, " +
//                $"RequestIdToken: {GoogleSignIn.Configuration.RequestIdToken}, " +
//                $"RequestProfile: {GoogleSignIn.Configuration.RequestProfile}, " +
//                $"ForceTokenRefresh: {GoogleSignIn.Configuration.ForceTokenRefresh}");
//            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleSignIn);
//        }

//        /// <summary>
//        /// Google Sign-In 결과를 처리하는 콜백 함수입니다.
//        /// </summary>
//        /// <param name="task">Google Sign-In으로부터의 작업 결과.</param>
//        private void OnGoogleSignIn(Task<GoogleSignInUser> task)
//        {
//            if (task.IsCanceled || task.IsFaulted)
//            {
//                Debug.LogError("Google Sign-In failed.");
//                return;
//            }

//            var googleUser = task.Result;
//            if (googleUser == null)
//            {
//                Debug.LogError("Google User Empty");
//                return;
//            }
//            var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

//            _auth.SignInWithCredentialAsync(credential).ContinueWith(credentialTask =>
//            {
//                if (credentialTask.IsCanceled || credentialTask.IsFaulted)
//                {
//                    Debug.LogError("Firebase 인증 실패.");
//                    return;
//                }

//                FirebaseUser newUser = credentialTask.Result;
//                OnFirebaseAuthComplete(newUser);
//            });
//        }
//        #endregion

//        #region Apple
//        /// <summary>
//        /// Apple Sign-In 프로세스를 시작합니다 (iOS 전용).
//        /// </summary>
//        public void SignInWithApple()
//        {
//#if UNITY_IOS
//            if (AppleAuthManager.IsCurrentPlatformSupported)
//            {
//                var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
//                AppleAuthManager appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
//                appleAuthManager.LoginWithAppleId(loginArgs, OnAppleSignInSuccess, OnAppleSignInFailure);
//            }
//#endif
//        }

//#if UNITY_IOS
//        /// <summary>
//        /// 성공적인 Apple Sign-In 결과를 처리하는 콜백 함수입니다.
//        /// </summary>
//        /// <param name="credential">Apple Sign-In 프로세스로부터 얻은 자격 증명.</param>
//        private void OnAppleSignInSuccess(ICredential credential)
//        {

//            if (credential is IAppleIDCredential appleIdCredential)
//            {
//                string idToken = System.Text.Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
//                Credential firebaseCredential = OAuthProvider.GetCredential("apple.com", idToken, null, null);
//                _auth.SignInWithCredentialAsync(firebaseCredential).ContinueWith(credentialTask =>
//                {
//                    if (credentialTask.IsCanceled || credentialTask.IsFaulted)
//                    {
//                        Debug.LogError("Firebase 인증 실패.");
//                        return;
//                    }

//                    FirebaseUser newUser = credentialTask.Result;
//                    OnFirebaseAuthComplete(newUser);
//                });
//            }
//        }
//#endif

//#if UNITY_IOS
//        /// <summary>
//        /// 실패한 Apple Sign-In 결과를 처리하는 콜백 함수입니다.
//        /// </summary>
//        /// <param name="error">Apple Sign-In 중 발생한 오류.</param>
//        private void OnAppleSignInFailure(IAppleError error)
//        {

//            Debug.LogError($"Apple Sign-In failed: {error.LocalizedDescription}");

//    }
//#endif
//        #endregion

//        /// <summary>
//        /// Firebase 인증 결과를 처리하는 콜백 함수입니다.
//        /// </summary>
//        /// <param name="user">Firebase 인증으로부터의 사용자 정보.</param>
//        private void OnFirebaseAuthComplete(FirebaseUser user)
//        {
//            Debug.LogFormat("사용자가 성공적으로 로그인했습니다: {0} ({1})", user.DisplayName, user.UserId);
//            _user = user;
//            if (_user == null)
//            {
//                Debug.LogWarning("Firebase User is Null");
//                return;
//            }

//            string email = string.Empty;
//            string displayName = _user.DisplayName;
//            string photoUrl = _user.PhotoUrl?.ToString();
//            string token = _user.UserId;

//            foreach (var data in _user.ProviderData)
//            {
//                if (data.ProviderId == GOOGLE_PROVIDER_ID)
//                {
//                    email = data.Email;
//                    break;
//                }
//            }

//            Debug.Log(email);
//            Debug.Log(displayName);
//            Debug.Log(photoUrl);
//            Debug.Log(token);
//        }
//        #endregion
    }


    [Serializable]
    public class UserInfo
    {
        public string sid;
        public string nickname;
        public string id;
        public string email;
        public string photoURL;
        public bool isRemoveAds = false;
    }
}