using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Zeus
{
    public static class Util
    {
        public static void SaveAssetFile(byte[] _bytes, string _fileName)
        {
            var path = Define.GetLocalPath() + _fileName;

            FileStream fs = new FileStream(path, System.IO.FileMode.Create);
            fs.Write(_bytes, 0, _bytes.Length);
            fs.Close();

            GameLog.Log("Save File : " + _fileName);
        }

        public static byte[] LoadAssetFile(string _fileName)
        {
            var path = Define.GetLocalPath() + _fileName;

            if (!File.Exists(path))
                return null;

            var array = File.ReadAllBytes(path);

            return array;
        }

        public static void DeleteAssetFile(string _fileName, bool _contains = false)
        {
            if (Directory.Exists(Define.GetLocalPath()))
            {
                var fileNames = Directory.GetFiles(Define.GetLocalPath());
                foreach (var item in fileNames)
                {
                    if (_contains)
                    {
                        if (item.Contains(_fileName))
                        {
                            File.Delete(item);
                            RefreshEditorProjectWindow();
                        }
                    }
                    else
                    {
                        if (item.Equals(_fileName))
                        {
                            File.Delete(item);
                            RefreshEditorProjectWindow();
                            break;
                        }
                    }
                }

            }
        }

        internal static void CreateJsonFile(string _createPath, string _fileName, string _jsonData)
        {
            FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", _createPath, _fileName), FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(_jsonData);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();

            Debug.Log($"Save Path : {_createPath}");
        }

        private static void RefreshEditorProjectWindow()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public static void SaveAssetFileToStreamingAsset(byte[] _bytes, string _fileName)
        {
            var path = Define.GetStreamingAssetsPath() + _fileName;

            FileStream fs = new FileStream(path, System.IO.FileMode.Create);
            fs.Write(_bytes, 0, _bytes.Length);
            fs.Close();

            GameLog.Log("Save File : " + _fileName);
        }

        public static float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }

        public static void ReAssignShader(GameObject _ob)
        {
#if UNITY_EDITOR
            Renderer[] renderers = _ob.transform.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer item in renderers)
            {
                if (item.sharedMaterials != null)
                {
                    foreach (var mat in item.sharedMaterials)
                    {
                        Shader sha = mat.shader;
                        sha = Shader.Find(sha.name);
                        mat.shader = sha;
                    }
                }
            }
#endif
        }

        public static void AddFlag(ref int _sourceFlag, int _flag)
        {
            _sourceFlag |= _flag;
        }

        public static int PlusFlag(int _flag1, int _flag2)
        {
            return _flag1 | _flag2;
        }

        public static void RemoveFlag(ref int _sourceFlag, int _flag)
        {
            _sourceFlag &= ~_flag;
        }

        public static bool CheckFlag(int _sourceFlag, int _testFlag)
        {
            return (_sourceFlag & _testFlag) == _testFlag;
        }

        public static string ByteToString(byte[] strByte)
        {
            string str = Encoding.Default.GetString(strByte);
            return str;
        }
        public static byte[] StringToByte(string str)
        {
            var byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        public static Vector3 MousePositionToWorldPosition(Camera _cam = null)
        {
            var cam = _cam == null ? Camera.main : _cam;
            var mousePosition = Input.mousePosition;
            mousePosition.z = cam.transform.position.z;
            var newPosition = cam.ScreenToWorldPoint(mousePosition);
            newPosition.x = mousePosition.z >= 0 || cam.orthographic ? newPosition.x : newPosition.x * -1f;
            newPosition.y = mousePosition.z >= 0 || cam.orthographic ? newPosition.y : newPosition.y * -1f;
            return newPosition;
        }

        internal static DateTime UnixTimeStampToDateTime(ulong _time, bool _milisecond = true)
        {
            ulong epoch = _milisecond ? _time / 1000 : _time;
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(epoch).ToLocalTime();
            return dtDateTime;
        }

        internal static float GetAnimationClipLength(Animator _anim, string _clipName)
        {
            var length = 0f;
            foreach (var item in _anim.runtimeAnimatorController.animationClips)
            {
                if (item.name.Contains(_clipName))
                {
                    length = item.length;
                    break;
                }
            }

            return length;
        }

        internal static void AddTexture2D(Texture2D _dest, Texture2D _tex2)
        {
            var cols1 = _dest.GetPixels();
            var cols2 = _tex2.GetPixels();
            for (var i = 0; i < cols1.Length; ++i)
            {
                cols1[i] += cols2[i];
            }
            _dest.SetPixels(cols1);
            _dest.Apply();
        }

        internal static bool IsCbetweenAB(Vector3 _a, Vector3 _b, Vector3 _c)
        {
            return Vector3.Dot((_b - _a).normalized, (_c - _b).normalized) < 0f && Vector3.Dot((_a - _b).normalized, (_c - _a).normalized) < 0f;
        }

        public static string GetMiddleString(string str, string begin, string end)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string result = null;
            if (str.IndexOf(begin) > -1)
            {
                str = str.Substring(str.IndexOf(begin) + begin.Length);
                if (str.IndexOf(end) > -1) result = str.Substring(0, str.IndexOf(end));
                else result = str;
            }

            return result;
        }

        public static string GetRichTextTag(string _str)
        {
            if (string.IsNullOrEmpty(_str))
            {
                return string.Empty;
            }

            var ch = _str.ToCharArray();
            var startIndex = _str.IndexOf('<');
            var endIndex = _str.IndexOf('>');

            string result = string.Empty;

            if (startIndex > -1)
            {
                var length = endIndex - startIndex + 1;
                //Debug.Log($"length : {length} / endIndex : {endIndex} / startIndex : {startIndex} / _str : {_str}");
                if (length > 0)
                    result = _str.Substring(startIndex, length);
            }

            return result;
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
            }

        }

        //16:9 강제 비율.
        public static float LandscapeWidthFixResolutionCalculator(float _width)
        {
            var r = _width / 16f;
            var w = Screen.width / 16f;
            var h = Screen.height / w;

            return h * r;
        }

        public static float PortraitWidthFixResolutionCalculator(float _width)
        {
            var r = _width / 9f;
            var w = Screen.width / 9f;
            var h = Screen.height / w;

            return h * r;
        }

        public static int PortraitHeightFixResolutionCalculator(float _height)
        {
            var r = _height / 16f;
            var h = Screen.height / 16f;
            var w = Screen.width / h;

            return (int)(w * r);
        }

        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        //read csv file
        internal static List<Dictionary<string, object>> Read(string _path)
        {
            var list = new List<Dictionary<string, object>>();
            TextAsset data = Resources.Load(_path) as TextAsset;

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) return list;

            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {

                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                    value = value.Replace("<br>", "\n"); // 추가된 부분. 개행문자를 \n대신 <br>로 사용한다.
                    value = value.Replace("<c>", ",");

                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, out f))
                    {
                        finalvalue = f;
                    }
                    entry[header[j]] = finalvalue;
                }
                list.Add(entry);
            }
            return list;
        }

        internal static bool IsTimeOver(long _binary)
        {
            var now = DateTime.UtcNow;
            var endTime = DateTime.FromBinary(_binary);
            var compare = DateTime.Compare(now, endTime);
            return compare >= 0;
        }

        //다른앱의 인스톨 여부 체크.
        public static bool IsAppInstalled(string _bundleID)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject launchIntent = null;

        //if the app is installed, no errors. Else, doesn't get past next line
        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", _bundleID);
        }
        catch (Exception ex)
        {
            Debug.Log("exception" + ex.Message);
        }

        return (launchIntent != null);
#else
            return false;
#endif
        }

        //예제.
        //public void OnClickMetaMaskOpen()
        //{
        //    //io.metamask
        //    var bundleID = Application.platform == RuntimePlatform.Android ? "io.metamask" : "io.metamask.MetaMask";
        //    //인스톨되어있는경우 앱실행 없으면 false리턴.
        //    var isInstall = Util.IsAppInstalled(bundleID);
        //    if (!isInstall)
        //    {
        //        var url = Application.platform == RuntimePlatform.Android ? "http://play.google.com/store/apps/details?id=io.metamask" : "https://itunes.apple.com/app/id1438144202?l";
        //        Application.OpenURL(url);
        //    }
        //}

        //반올림.
        internal static double MathCeiling(double _value, int _removeIndex)
        {
            if (_value == 0)
                return 0;

            var removeValue = Mathf.Pow(10, _removeIndex);
            return Math.Ceiling(_value * removeValue) / removeValue;
        }

        //버림.
        internal static double MathTruncate(double _value, int _removeIndex)
        {
            if (_value == 0)
                return 0;

            var removeValue = Mathf.Pow(10, _removeIndex);
            return Math.Truncate(_value * removeValue) / removeValue;
        }

        internal static bool CheckObjectIsInCamera(Camera camera, Vector3 position)
        {
            if (camera == null)
                return false;

            var screenPosition = camera.WorldToViewportPoint(position);

            //Debug.Log("screenPosition ==== " + screenPosition);

            bool inCamera = screenPosition.z > 0f && screenPosition.x > 0f && screenPosition.x < 1f && screenPosition.y > -0.5f && screenPosition.y < 1f;

            return inCamera;
        }
    }

    public static class GameLog
    {
        public static void Log(string _msg, LogType _type = LogType.Log)
        {
#if UNITY_EDITOR
            switch (_type)
            {
                case LogType.Error:
                    Debug.LogError(_msg);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(_msg);
                    break;
                default:
                    Debug.Log(_msg);
                    break;
            }
#endif
        }
    }
}

