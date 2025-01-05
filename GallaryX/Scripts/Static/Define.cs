using System.Collections.Generic;

namespace starinc.io
{
    public class Define
    {
        public const int IntTrue = 1;
        public const int IntFalse = 0;

        public const string LoadingSceneName = "LoadingScene";
        public const string MainSceneName = "MainScene";

        public const string ExhibitionSeq = "exhibitionSeq";
        public const string Locale = "LOCALE";
        public const string UID = "UID";
        public const string ExhibitionType = "TYPE";

        public static readonly List<List<string>> FileExtensions = new List<List<string>>()
        {
            new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tga" },
            new List<string> { ".avi", ".asf", ".flv", ".mp4", ".mov", ".mpg", ".mpeg", ".mkv", ".wmv", ".webm" }
        };

        public static readonly List<string> FileTypeString = new List<string>() { FileType.IMAGE.ToString(), FileType.VIDEO.ToString(), FileType.AUDIO.ToString() };

        public enum UIEvent
        {
            Click,
            Drag,
            Down,
            Up,
        }

        public enum MouseEvent
        {
            Press,
            Click,
        }

        public enum FileType
        {
            EMPTY,
            IMAGE,
            VIDEO,
            AUDIO,
        }

        public enum APIKey
        {
            open_ai_key,
            google_tts_key,
            google_stt_key,
            system_message,
            assistant_message,
            project_description,
        }
    }
}
