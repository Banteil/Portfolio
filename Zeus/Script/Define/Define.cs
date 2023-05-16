#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Zeus
{
    public static class Define
    {
        public static SystemLanguage Language;

        public static string GetStreamingAssetsPath()
        {
            var path = string.Empty;

            if (Application.platform == RuntimePlatform.Android)
            {
                path = "jar:file://" + Application.dataPath + "!/assets/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.dataPath + "/Raw/";
            }
            else
                path = Application.streamingAssetsPath + "/";

            return path;
        }

        //public static string GetPatchPath()
        //{
        //    var path = string.Empty;

        //    //if (Application.platform == RuntimePlatform.Android)
        //    //{
        //    //    path = "jar:file://" + Application.dataPath + "!/assets/";
        //    //}
        //    //else if (Application.platform == RuntimePlatform.IPhonePlayer)
        //    //{
        //    //    path = Application.dataPath + "/Raw/";
        //    //}
        //    //else
        //    path = "Patch/";

        //    return path;
        //}

        public static string GetLocalPath()
        {
            var path = string.Empty;

            path = Application.persistentDataPath + "/";

            return path;
        }

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
		return GetPlatformForAssetBundles(Application.platform);
#endif
        }

#if UNITY_EDITOR
        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return null;
            }
        }
#endif

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                default:
                    return null;
            }
        }

    }
}