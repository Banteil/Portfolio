namespace starinc.io
{
    public class Define
    {
        public enum Platform
        {
            Unknown,
            Google,
            Apple,
        }

        public const int INT_TRUE = 1;
        public const int INT_FALSE = 0;

        public const int REFERENCE_WIDTH = 1080;
        public const int REFERENCE_HEIGHT = 1920;

        //씬 이름
        public const string TITLE_SCENE_NAME = "TitleScene";
        public const string LOBBY_SCENE_NAME = "LobbyScene";
        public const string LOAD_SCENE_NAME = "LoadScene";
        public const string MINIGAME_SCENE_ADDRESS = "Minigame_";
        
        //현지화DB 테이블 이름
        public const string LOCALIZATION_TABLE_UI = "UITable";
        public const string LOCALIZATION_TABLE_MESSAGE = "MessageTable";

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
