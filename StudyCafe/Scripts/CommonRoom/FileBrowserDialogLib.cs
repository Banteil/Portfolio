using System;
using System.IO;
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
using System.Runtime.InteropServices;
#endif
using UnityEngine;

public class FileBrowserDialogLib : MonoBehaviour
{
    /// <summary>
    /// byte[] - File Data
    /// string - File Name
    /// string - File Resolution
    /// </summary>
    public static event Action<byte[], string, string> FileWasOpenedEvent;

    public static event Action FilePopupWasClosedEvent;

#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
        private static string _FileName;
        private static bool _IsAlreadyActive;
        private static bool _ItWasInFullScreen;
#endif

    private static FileBrowserDialogLib _Instance;

#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)

        [DllImport("__Internal")]
        private static extern int GetFileDataLength(string name);

        [DllImport("__Internal")]
        private static extern void FreeFileData(string name);

        [DllImport("__Internal")]
        private static extern IntPtr GetFileData(string name);

        [DllImport("__Internal")]
        private static extern void OpenFilePopup(string types);

        [DllImport("__Internal")]
        private static extern void HideFilePopup();

        [DllImport("__Internal")]
        private static extern void SaveFile(string fileName, string data);

        [DllImport("__Internal")]
        private static extern void SaveUrlFile(string fileDir, string fileName);
        
#endif
    private void Awake()
    {
        if (_Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void FileUpload(string file)
    {
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            var length = GetFileDataLength(file);
            var ptr = GetFileData(file);
            var data = new byte[length];
            Marshal.Copy(ptr, data, 0, data.Length);      
            
            HideFileDialog();
            FreeFileData(file);

            if (_ItWasInFullScreen)
                Screen.fullScreen = true;

            if (FileWasOpenedEvent != null)
                FileWasOpenedEvent(data, _FileName, GetFileResolution(_FileName));

            _IsAlreadyActive = false;
#endif
    }

#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
        private void ApplyFileName(string name)
        {
            Debug.Log("ApplyFileName : " + name);
            var split = name.Replace(@"\", "/").Split("/"[0]);
            _FileName = split[split.Length - 1];
            Debug.Log(_FileName);
        }
#endif

    /// <summary>
    /// Opens Native File Browser Dialog
    /// </summary>
    public static void OpenFileDialog(string types)
    {
        Debug.Log("OpenFileDialog");
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            if (_Instance == null)
                _Instance = new GameObject("FileBrowserDialogLib").AddComponent<FileBrowserDialogLib>();

            if (_IsAlreadyActive)
                return;

            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
                _ItWasInFullScreen = true;
            }
            else _ItWasInFullScreen = false;

            _IsAlreadyActive = true;

            OpenFilePopup(types);
#endif
    }

    /// <summary>
    /// Hides Native File Browser Dialog
    /// </summary>
    public static void HideFileDialog()
    {
        Debug.Log("HideFileDialog");
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            HideFilePopup();
#endif
    }

    /// <summary>
    /// Saves text to file with 'filename' name and resolution (not supported yet)
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="text"></param>
    public static void SaveTextToFile(string fileName, string text)
    {
        Debug.Log("SaveTextToFile");
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            SaveFile(fileName, text);
#endif
    }

    /// <summary>
    /// Saves bytes to file with 'filename' name and resolution (not supported yet)
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="text"></param>
    public static void SaveBytesToFile(string fileName, byte[] bytes)
    {
        Debug.Log("SaveBytesToFile : " + bytes.Length);        
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            SaveFile(fileName, System.Text.Encoding.ASCII.GetString(bytes));
#endif
    }

    public static void SaveUrlToFile(string fileDir, string fileName)
    {
        Debug.Log("SaveUrlToFile : " + fileDir + ", 파일 이름 : " + fileName);
#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
            SaveUrlFile(fileDir, fileName);
#endif
    }

#if (UNITY_WEBGL && !UNITY_EDITOR) || (FG_FB_WEBGL && !UNITY_EDITOR)
        private void CloseFilePopupDialog()
        {
            Debug.Log("CloseFilePopupDialog");
            if (_ItWasInFullScreen)
                Screen.fullScreen = true;

            _IsAlreadyActive = false;

            if (FilePopupWasClosedEvent != null)
                FilePopupWasClosedEvent();

            HideFilePopup();
        }
#endif

    public static string GetFileResolution(string name)
    {
        string[] split = name.Split('.');

        if (split.Length > 0)
            return split[split.Length - 1];
        else
            return string.Empty;
    }

    public static Texture2D GetTexture2D(byte[] data, string name = "custom")
    {
        if (data == null)
            return null;

        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);

        texture.name = name;
        texture.LoadImage(data);
        texture.Apply();

        return texture;
    }

    public static Sprite GetSprite(byte[] data, string name = "custom")
    {
        if (data == null)
            return null;

        var texture = GetTexture2D(data, name);

        var sprite = Sprite.Create(texture,
                                    new Rect(Vector2.zero, new Vector2(texture.width, texture.height)),
                                    Vector2.one / 2f,
                                    100,
                                    1,
                                    SpriteMeshType.FullRect,
                                    Vector4.zero);
        return sprite;
    }

    public static AudioClip GetAudioClip(byte[] data, string name = "clipSound")
    {       
        if (data == null)
            return null;

        WAV wav = new WAV(data);
        AudioClip audioClip = AudioClip.Create(name, wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);

        return audioClip;
    }

    public static string GetBase64StringFromImage(Texture2D texture, bool isJPEG = true)
    {
        if (texture == null)
            return string.Empty;

        if (isJPEG)
            return Convert.ToBase64String(texture.EncodeToJPG());
        else
            return Convert.ToBase64String(texture.EncodeToPNG());
    }
}