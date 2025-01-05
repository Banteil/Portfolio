#if UNITY_ANDROID
using Google;
#endif
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
#endif
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class FirebaseLogIn : Singleton<FirebaseLogIn>
    {
        public Firebase.Auth.FirebaseAuth auth;
        public Firebase.Auth.FirebaseUser user;

#if UNITY_ANDROID
        private const string GOOGLE_WEB_API = "18456262716-fgqotnb99hftvdc1mvc812f99poj6d8u.apps.googleusercontent.com";
        private const string GOOGLE_PROVIDER_ID = "google.com";
        private GoogleSignInConfiguration configuration;
#endif

#if UNITY_IOS
        private const string APPLE_PROVIDER_ID = "apple.com";
        private IAppleAuthManager appleAuthManager;
#endif

        private void Awake()
        {
            auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
#if UNITY_ANDROID
            InitGoogleAuth();
#endif
        }

#region Google

        public void OnGoogleSignIn()
        {
#if !UNITY_ANDROID
            return;
#else
            if (!FirebaseManager.Instance.isFirebaseStarted)
            {
                Debug.Log("파이어베이스 시작 안됨");
                return;
            }

            Debug.Log(
                $"WebClientId: {GoogleSignIn.Configuration.WebClientId}, " +
                $"UserGameSignIn: {GoogleSignIn.Configuration.UseGameSignIn}, " +
                $"RequestAuthCode: {GoogleSignIn.Configuration.RequestAuthCode}, " +
                $"AccountName: {GoogleSignIn.Configuration.AccountName}, " +
                $"RequestEamil: {GoogleSignIn.Configuration.RequestEmail}, " +
                $"RequestIdToken: {GoogleSignIn.Configuration.RequestIdToken}, " +
                $"RequestProfile: {GoogleSignIn.Configuration.RequestProfile}, " +
                $"ForceTokenRefresh: {GoogleSignIn.Configuration.ForceTokenRefresh}");
            //GoogleSignIn.Configuration.UseGameSignIn = false;
            //GoogleSignIn.Configuration.RequestIdToken = true;
            //GoogleSignIn.Configuration.RequestEmail = true;

            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthenticatedFinished);
#endif
        }

#if UNITY_ANDROID
        //private void OnGoogleSignOut()
        //{
        //    GoogleSignIn.DefaultInstance?.SignOut();
        //    auth?.SignOut();
        //}

        void InitGoogleAuth()
        {
            configuration = new GoogleSignInConfiguration
            {
                WebClientId = GOOGLE_WEB_API,
                UseGameSignIn = false,
                RequestIdToken = true,
                RequestEmail = true,
                RequestProfile = true,
                ForceTokenRefresh = true
            };
            if (GoogleSignIn.Configuration == null)
            {
                GoogleSignIn.Configuration = configuration;
            }
        }

        void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> googleTask)
        {            
            Debug.Log($"Google Authenticated Finished : {googleTask.Result.DisplayName}");

            if (googleTask.IsFaulted)
            {
                Debug.LogError("Fault");
            }
            else if (googleTask.IsCanceled)
            {
                Debug.LogError("Login Cancel");
            }
            else
            {
                Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(googleTask.Result.IdToken, null);

                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(firebaseTask =>
                {
                    if (firebaseTask.IsCanceled)
                    {
                        Debug.LogError("SignInWithCredentialAsync was canceled");
                        return;
                    }

                    if (firebaseTask.IsFaulted)
                    {
                        Debug.LogError("SignInWithCredentialAsync was encountered an error: " + firebaseTask.Exception);
                        return;
                    }

                    //Uri userPhotoUrl = null;

                    Debug.LogWarning("!!! " + auth);
                    user = auth.CurrentUser;
                    if (user == null)
                    {
                        Debug.LogWarning("user null");
                        return;
                    }

                    string email = string.Empty;
                    string displayName = user.DisplayName;
                    string photoUrl = string.Empty;
                    photoUrl = user.PhotoUrl?.ToString();
                    string token = user.UserId;
                    //string token = await user.TokenAsync(false);
                    foreach (var data in user.ProviderData)
                    {
                        if (data.ProviderId == GOOGLE_PROVIDER_ID)
                        {
                            email = data.Email;
                        }

                        //Debug.Log("DisplayName: " + data.DisplayName);
                        //Debug.Log("Email: " + data.Email);
                        //Debug.Log("PhotoUrl: " + data.PhotoUrl);
                        //Debug.Log("ProviderId: " + data.ProviderId);
                        //Debug.Log("UserId: " + data.UserId);
                        //Debug.Log("UserId?: " + auth.CurrentUser.UserId);
                    }

                    LoginUser(email, Define.LoginType.Google, displayName, photoUrl, token);
                });
            }
        }
#endif
#endregion

#region Apple
        public void OnAppleSignInWithAppleId()
        {
#if !UNITY_IOS
            return;
#else
            if (!AppleAuthManager.IsCurrentPlatformSupported || !FirebaseManager.Instance.isFirebaseStarted) return;

            //auth?.SignOut();
            InitAppleAuth();

            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

            var loginArgs = new AppleAuthLoginArgs(
                LoginOptions.IncludeEmail | LoginOptions.IncludeFullName,
                nonce);

            appleAuthManager.LoginWithAppleId(loginArgs, credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    Debug.Log("Login");
                    PerformFirebaseAuthentication(appleIdCredential, rawNonce);
                }
            },
            error =>
            {
                Debug.LogError("Login failed " + error);
            });
#endif
        }

#if UNITY_IOS
        void InitAppleAuth()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                appleAuthManager = new AppleAuthManager(deserializer);
            }
        }

        private static string GenerateRandomString(int length)
        {
            if (length <= 0)
            {
                throw new Exception("Expected nonce to have positive length");
            }

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-_";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length)
                    {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }

            return result;
        }

        private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
        {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            var result = string.Empty;
            for (var i = 0; i < hash.Length; i++)
            {
                result += hash[i].ToString("x2");
            }

            return result;
        }

        private void PerformFirebaseAuthentication(IAppleIDCredential appleIdCredential, string rawNonce)
        {
            var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
            var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
            var firebaseCredential = OAuthProvider.GetCredential(
                APPLE_PROVIDER_ID,
                identityToken,
                rawNonce,
                authorizationCode);

            string token = identityToken;
            string displayName = string.Empty;
            string photoUrl = string.Empty;
            string email = string.Empty;
            auth.SignInWithCredentialAsync(firebaseCredential).ContinueWithOnMainThread(firebaseTask =>
            {
                if (firebaseTask.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled");
                    return;
                }

                if (firebaseTask.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync was encountered an error: " + firebaseTask.Exception);
                    return;
                }

                user = auth.CurrentUser;
                if (user == null)
                {
                    Debug.LogWarning("user null");
                    return;
                }

                displayName = user.DisplayName;
                photoUrl = user.PhotoUrl?.ToString();
                token = user.UserId;

                //token = user.UserId;
                foreach (var data in user.ProviderData)
                {
                    if (data.ProviderId == APPLE_PROVIDER_ID)
                    {
                        email = data.Email;
                    }
                }

                LoginUser(email, Define.LoginType.Apple, displayName, photoUrl, token);
            });
        }

        private void Update()
        {
            appleAuthManager?.Update();
        }
#endif
        #endregion

        async public void LoginUser(string email, Define.LoginType loginType, string name, string profileUrl, string token)
        {
            //await CallAPI.APIUpdateUser(UserDataManager.Instance.MyData.sid, userID, password, email, (int)loginType, null);
            //await UserDataManager.Instance.LogIn(userID, password, (returnCd) =>
            //{
            //    if (returnCd == (int)Define.APIReturnCd.OK)
            //    {
            //        Debug.Log("LOG-IN SUCCESS");
            //        var profile = FindObjectOfType<UIUserProfile>();
            //        if (profile != null)
            //        {
            //            profile.SetUserData(UserDataManager.Instance.MyData);
            //        }
            //        UIManager.Instance.CloseRangeUI(1);
            //    }
            //    else
            //    {
            //        Debug.LogError("LOG-IN FAIL");
            //        UIManager.Instance.ShowWarningUI("warningFailedLogin");
            //    }
            //});
            Debug.Log("Login User");
            await UserDataManager.Instance.SNSLogIn(loginType, email, profileUrl, token, name, (returnCd) =>
            {
                if (returnCd != 0)
                {
                    auth?.SignOut();
                }
                else
                {
                    UIManager.Instance.CloseRangeUI(1);
                    UIManager.Instance.ShowWarningUI("loginComplete");
                }
            });
        }
    }
}