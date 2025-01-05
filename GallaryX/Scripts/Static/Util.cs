using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using HtmlAgilityPack;

namespace starinc.io
{
    public static class Util
    {
        public static bool IsMobileWebPlatform;
        public static bool IsLandscape { get { return Screen.width >= Screen.height; } }
        // Unity TextMeshPro에서 지원하는 태그 목록
        private static readonly string[] SupportedTags = {
            "b", "i", "u", "color", "size", "align", "mark", "sprite", "link", "sub", "sup"
        };

        /// <summary>
        /// 오브젝트의 자식들 중 컴포넌트를 가지고, name 변수와 이름이 일치하는 오브젝트를 찾아 반환하는 함수.
        /// recursive가 true면 모든 자식들을 싹싹 찾아서 검사.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static T FindChild<T>(GameObject obj, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            if (obj == null) return null;

            if (!recursive)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform transform = obj.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || transform.name == name)
                    {
                        T component = transform.GetComponent<T>();
                        if (component != null)
                            return component;
                    }
                }
            }
            else
            {
                foreach (T component in obj.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        return component;
                }
            }

            return null;
        }

        /// <summary>
        /// 컴포넌트 없는 GameObject를 찾아 반환하는 함수.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static GameObject FindChild(GameObject obj, string name = null, bool recursive = false)
        {
            Transform transform = FindChild<Transform>(obj, name, recursive);
            if (transform == null)
                return null;

            return transform.gameObject;
        }

        /// <summary>
        /// 오브젝트의 컴포넌트를 가져오는데, 없으면 Add해서 가져오는 함수.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(GameObject obj) where T : UnityEngine.Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        public static Transform FindOrAddChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if(child == null)
            {
                child = new GameObject(name).transform;
                child.parent = parent;
            }

            return child;
        }

        /// <summary>
        /// 부모 객체들을 탐색하여 원하는 타입의 Component 반환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentTransform"></param>
        /// <returns></returns>
        public static T FindComponentInParents<T>(Transform currentTransform) where T : Component
        {
            while (currentTransform != null)
            {
                T component = currentTransform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        /// <summary>
        /// object에 담긴 json 정보를 특정 타입으로 캐스팅하여 반환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CastingJsonObject<T>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 특정 오브젝트를 Json 데이터로 변환(직렬화)하는 함수
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json 데이터를 클래스로 변환(역직렬화)하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public static void DontDestroyObject(GameObject obj)
        {
            if (obj.transform.parent != null)
                obj.transform.SetParent(null, false);
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }

        public static int GetSceneIndexByName(string name)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (scenePath.Contains(name)) return i;
            }
            return -1;
        }

        public static int GetSelectedLocaleIndex()
        {
            var currentSelectedLocale = LocalizationSettings.SelectedLocale;
            var locales = LocalizationSettings.AvailableLocales.Locales;

            for (int i = 0; i < locales.Count; i++)
            {
                if (currentSelectedLocale.LocaleName == locales[i].LocaleName)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void ExcludingCloneName(GameObject obj)
        {
            int index = obj.name.IndexOf("(Clone)");
            if (index > 0)
                obj.name = obj.name.Substring(0, index);
        }

        public static Vector3 GetDirectionWithoutYAxis(Vector3 targetPos, Vector3 viewerPos)
        {
            targetPos.y = 0f;
            viewerPos.y = 0f;
            var dir = targetPos - viewerPos;

            return dir;
        }

        public static bool IsUIFocusing
        {
            get 
            {
                if (EventSystem.current == null) return false;
                return EventSystem.current.currentSelectedGameObject != null; 
            }
        }        

        public static void UnfocusUI()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public static Define.FileType GetFileTypeByExtension(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            for (int i = 0; i < Define.FileExtensions.Count; i++)
            {
                if (Define.FileExtensions[i].Contains(extension))
                    return (Define.FileType)(i + 1);
            }

            return Define.FileType.EMPTY;
        }

        public static Define.FileType GetFileType(string type)
        {
            var typeUpper = type.ToUpper();
            for (int i = 0; i < Define.FileTypeString.Count; i++)
            {
                var fileType = Define.FileTypeString[i];
                if (fileType.Contains(typeUpper))
                    return (Define.FileType)(i + 1);
            }

            return Define.FileType.EMPTY;
        }

        public static Texture2D ByteToTexture2D(byte[] imageData)
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;

            texture.LoadImage(imageData);
            texture = CompressTexture(texture);
            return texture;
        }

        public static Texture2D CompressTexture(Texture2D texture)
        {
            //var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            texture = ResizeTextureWithAspect(texture, 512);
            if (!CheckTextureSizeToMultipleOfFour(texture))
                texture = AddPaddingToTexture(texture);
            texture.Compress(true);
            texture.Apply();

            //stopwatch.Stop();
            //Debug.Log("압축 실행 시간: " + stopwatch.ElapsedMilliseconds + "ms");
            return texture;
        }

        private static bool CheckTextureSizeToMultipleOfFour(Texture2D texture)
        {
            var checkWidth = texture.width % 4;
            var checkHeight = texture.height % 4;
            return (checkWidth + checkHeight) == 0;
        }

        private static Texture2D AddPaddingToTexture(Texture2D original)
        {
            // 원본 텍스쳐의 크기 가져오기
            int originalWidth = original.width;
            int originalHeight = original.height;
            int widthPadding = 4 - original.width % 4;
            int heightPadding = 4 - original.height % 4;

            // 패딩을 추가한 텍스쳐 생성
            Texture2D paddedTexture = new Texture2D(originalWidth + widthPadding, originalHeight + heightPadding);

            // 투명한 픽셀로 패딩 초기화
            Color32[] transparentPixels = new Color32[(originalWidth + widthPadding) * (originalHeight + heightPadding)];
            for (int i = 0; i < transparentPixels.Length; i++)
            {
                transparentPixels[i] = new Color32(0, 0, 0, 0); // R, G, B, A
            }
            paddedTexture.SetPixels32(transparentPixels);

            // 원본 텍스쳐를 패딩된 텍스쳐로 복사
            for (int x = 0; x < originalWidth; x++)
            {
                for (int y = 0; y < originalHeight; y++)
                {
                    Color pixel = original.GetPixel(x, y);
                    paddedTexture.SetPixel(x + widthPadding, y + heightPadding, pixel);
                }
            }
            paddedTexture.Apply();
            return paddedTexture;
        }

        private static Texture2D ResizeTextureWithAspect(Texture2D sourceTexture, int maxSize)
        {
            if (sourceTexture.width <= maxSize && sourceTexture.height <= maxSize) return sourceTexture;

            float widthRatio = (float)maxSize / sourceTexture.width;
            float heightRatio = (float)maxSize / sourceTexture.height;
            float minRatio = Mathf.Min(widthRatio, heightRatio);

            int newWidth = Mathf.RoundToInt(sourceTexture.width * minRatio);
            int newHeight = Mathf.RoundToInt(sourceTexture.height * minRatio);

            RenderTexture rt = new RenderTexture(newWidth, newHeight, 0);
            RenderTexture.active = rt;
            Graphics.Blit(sourceTexture, rt);
            Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
            resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resizedTexture.Apply();
            RenderTexture.active = null;
            return resizedTexture;
        }

        public static Vector3 GetRatioProportionalToTextureSize(Transform tr, float width, float height, float sizeScale = 1f)
        {
            var result = tr.localScale;
            if (width > height)
            {
                var h = height / width;
                result = new Vector3(tr.localScale.x * sizeScale, tr.localScale.y * h * sizeScale, tr.localScale.z);
            }
            else if (height > width)
            {
                var w = width / height;
                result = new Vector3(tr.localScale.x * w * sizeScale, tr.localScale.y * sizeScale, tr.localScale.z);
            }

            return result;
        }

        public static float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / 2];
            for (int i = 0; i < floatArr.Length; i++)
            {
                floatArr[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
            }

            return floatArr;
        }

        public static byte[] ExtractWavData(byte[] wavFile)
        {
            // WAV 파일의 헤더는 일반적으로 44바이트입니다.
            const int headerSize = 44;
            var data = new byte[wavFile.Length - headerSize];
            Array.Copy(wavFile, headerSize, data, 0, wavFile.Length - headerSize);
            return data;
        }

        public static byte[] ToWav(this AudioClip audioClip)
        {
            var samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);

            WriteWavHeader(writer, audioClip.channels, audioClip.frequency, audioClip.samples);

            foreach (var sample in samples)
            {
                writer.Write((short)(sample * short.MaxValue));
            }

            byte[] wavBytes = memoryStream.ToArray();

            writer.Close();
            memoryStream.Close();

            return wavBytes;
        }

        private static void WriteWavHeader(BinaryWriter writer, int channels, int frequency, int sampleCount)
        {
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });

            writer.Write(36 + sampleCount * 2);

            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });

            writer.Write(16);
            writer.Write((short)1); // PCM format
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2); // byte rate
            writer.Write((short)(channels * 2)); // block align
            writer.Write((short)16); // bits per sample

            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(sampleCount * 2);
        }

        public static string ExtractingPatternString(string str)
        {
            string pattern = @"\{(.+?)\}";
            Match match = Regex.Match(str, pattern);
            if (match.Success)
            {
                string extractedString = match.Groups[1].Value;
                return extractedString;
            }
            else
                return null;
        }

        public static string ReplacePatternString(string str)
        {
            string pattern = @"\{(.+?)\}";
            string result = Regex.Replace(str, pattern, "");
            return result;
        }

        public static string GetLocalizedString(string tableName, string key)
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(key);
                if (entry != null)
                {
                    return entry.GetLocalizedString();
                }
            }
            return null;
        }

        public static string FilterUnsupportedTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var doc = new HtmlDocument();
            doc.LoadHtml(input);
            return doc.DocumentNode.InnerText;
        }

        public static float GetTextureSizeInMB(Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogWarning("Texture is null.");
                return 0f;
            }

            int textureWidth = texture.width;
            int textureHeight = texture.height;
            int bitsPerPixel = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Color32)) * 8;
            int textureSizeInBytes = textureWidth * textureHeight * bitsPerPixel / 8;

            // 바이트를 MB로 변환
            float textureSizeInMB = textureSizeInBytes / (1024.0f * 1024.0f);
            return textureSizeInMB;
        }

        public static bool IsPointerOverUI()
        {
            // 터치가 있는 경우
            for (int i = 0; i < Input.touchCount; i++)
            {
                bool isOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId);    
                if (isOverUI) return true;
            }

            bool isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
            return isMouseOverUI;
        }
    }
}
