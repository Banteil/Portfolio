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
        ///// ���� ����ڰ� �α��εǾ� �ִ��� ���θ� ��ȯ�մϴ�.
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
//        /// Firebase ���� �ν��Ͻ��� �ʱ�ȭ�մϴ�.
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
//        /// �̸��ϰ� ��й�ȣ�� ����Ͽ� Firebase ������ ���� ����ڸ� �α��ν�ŵ�ϴ�.
//        /// </summary>
//        /// <param name="email">������� �̸��� �ּ�.</param>
//        /// <param name="password">������� ��й�ȣ.</param>
//        public void SignInWithEmail(string email, string password)
//        {
//            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
//            {
//                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
//                {
//                    Debug.Log(email + " �� �α��� �ϼ̽��ϴ�.");
//                    FirebaseUser signInUser = task.Result.User;
//                    OnFirebaseAuthComplete(signInUser);
//                }
//                else
//                {
//                    Debug.LogError("�α��ο� �����ϼ̽��ϴ�.");
//                }
//            });
//        }

//        /// <summary>
//        /// �̸��ϰ� ��й�ȣ�� ����Ͽ� ���ο� ����ڸ� Firebase ������ ����մϴ�.
//        /// </summary>
//        /// <param name="email">������� �̸��� �ּ�.</param>
//        /// <param name="password">������� ��й�ȣ.</param>
//        public void RegisterWithEmail(string email, string password)
//        {
//            //����� �ƹ� ���� ���� ��Ͻ�Ű����, ���Ŀ��� ȸ�� �̸��� ���� �ý����� Ȱ���ϴ� ������ ���� �ʿ�
//            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
//            {
//                if (!task.IsCanceled && !task.IsFaulted)
//                {
//                    Debug.Log(email + "�� ȸ������ ����");
//                    SignInWithEmail(email, password);
//                }
//                else
//                {
//                    Debug.LogError("ȸ������ ����");
//                }
//            });
//        }

//        #region Google
//        /// <summary>
//        /// Google Sign-In ������ �����մϴ�.
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
//        /// Google Sign-In ���μ����� �����մϴ�.
//        /// </summary>
//        public void SignInWithGoogle()
//        {
//            if (!_isFirebaseStarted)
//            {
//                Debug.Log("���̾�̽� ���� �ȵ�");
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
//        /// Google Sign-In ����� ó���ϴ� �ݹ� �Լ��Դϴ�.
//        /// </summary>
//        /// <param name="task">Google Sign-In���κ����� �۾� ���.</param>
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
//                    Debug.LogError("Firebase ���� ����.");
//                    return;
//                }

//                FirebaseUser newUser = credentialTask.Result;
//                OnFirebaseAuthComplete(newUser);
//            });
//        }
//        #endregion

//        #region Apple
//        /// <summary>
//        /// Apple Sign-In ���μ����� �����մϴ� (iOS ����).
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
//        /// �������� Apple Sign-In ����� ó���ϴ� �ݹ� �Լ��Դϴ�.
//        /// </summary>
//        /// <param name="credential">Apple Sign-In ���μ����κ��� ���� �ڰ� ����.</param>
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
//                        Debug.LogError("Firebase ���� ����.");
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
//        /// ������ Apple Sign-In ����� ó���ϴ� �ݹ� �Լ��Դϴ�.
//        /// </summary>
//        /// <param name="error">Apple Sign-In �� �߻��� ����.</param>
//        private void OnAppleSignInFailure(IAppleError error)
//        {

//            Debug.LogError($"Apple Sign-In failed: {error.LocalizedDescription}");

//    }
//#endif
//        #endregion

//        /// <summary>
//        /// Firebase ���� ����� ó���ϴ� �ݹ� �Լ��Դϴ�.
//        /// </summary>
//        /// <param name="user">Firebase �������κ����� ����� ����.</param>
//        private void OnFirebaseAuthComplete(FirebaseUser user)
//        {
//            Debug.LogFormat("����ڰ� ���������� �α����߽��ϴ�: {0} ({1})", user.DisplayName, user.UserId);
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