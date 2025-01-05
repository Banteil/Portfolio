using System;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class LocalPushManager : Singleton<LocalPushManager>
    {
        [SerializeField]
        private NotificationTable _notificationTableOrigin;
        private NotificationTable _notificationTable;

        protected override void OnAwake()
        {
            base.OnAwake();
            _notificationTable = Instantiate(_notificationTableOrigin);
#if UNITY_ANDROID
            foreach (var data in _notificationTable.NotificationDatas)
            {
                if (PlayerPrefs.HasKey(data.Id))
                {
                    var notificationId = PlayerPrefs.GetInt(data.Id);
                    var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(notificationId);

                    if (notificationStatus == NotificationStatus.Delivered || notificationStatus == NotificationStatus.Unknown)
                    {
                        PlayerPrefs.DeleteKey(data.Id);
                    }
                }
                CreateNotificationChannel(data);
            }
            PlayerPrefs.Save();
#endif
            CancelAllScheduledNotifications();

            var settingData = Util.LoadSettingData();
            var isAccept = settingData == null ? true : settingData.AdAccept;
            if (isAccept)
                SendNotification(GetNotificationData("request_access_7days"));
        }

        public NotificationData GetNotificationData(string id)
        {
            var resultData = _notificationTable.NotificationDatas.Find((data) => string.Equals(data.Id, id));
            if (resultData != null)
                return new NotificationData(resultData);
            else
                return null;
        }

#if UNITY_ANDROID
        private void CreateNotificationChannel(NotificationData data)
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = data.Id,
                Name = data.Name,
                Importance = (Importance)data.Importance,
                Description = data.Description,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            Debug.Log($"Create Notification Compelte : {data.Id}");
        }
#endif

        public void SendNotification(NotificationData data)
        {
            var settingData = Util.LoadSettingData();
            var isAccept = settingData == null ? true : settingData.AdAccept;
            if (!isAccept) return;

            var fireTime = DateTime.Now + data.GetFireTimeSpan();
            var repeatInterval = data.IsRepeat ? data.GetFireTimeSpan() : TimeSpan.Zero;
            try
            {
#if UNITY_ANDROID
                var notification = new AndroidNotification()
                {
                    Title = Util.GetLocalizationTableString(Define.PushTable, data.Title),
                    Text = Util.GetLocalizationTableString(Define.PushTable, data.Explain),
                    FireTime = fireTime,
                    RepeatInterval = repeatInterval,
                    SmallIcon = "icon_0",
                    LargeIcon = "icon_0",
                    Style = NotificationStyle.BigTextStyle,
                    ShowInForeground = false
                };
                var id = AndroidNotificationCenter.SendNotification(notification, data.Id);
                PlayerPrefs.SetInt(data.Id, id);
                PlayerPrefs.Save();
#elif UNITY_IOS
                var timeInterval = data.GetFireTimeSpan();
                var timeTrigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = (timeInterval.TotalSeconds > 0) ? timeInterval : TimeSpan.FromSeconds(1),
                    Repeats = data.IsRepeat
                };

                var notification = new iOSNotification()
                {
                    Identifier = data.Id,
                    Title = Util.GetLocalizationTableString(Define.PushTable, data.Title),
                    Body = Util.GetLocalizationTableString(Define.PushTable, data.Explain),
                    //Subtitle = "subtitle",
                    ShowInForeground = false,
                    ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                    CategoryIdentifier = "category_a",
                    ThreadIdentifier = "thread1",
                    Trigger = timeTrigger,
                };

                iOSNotificationCenter.ScheduleNotification(notification);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Local Push Error : {ex}");
            }
        }

        public void CancelAllScheduledNotifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            foreach (var data in _notificationTable.NotificationDatas)
            {
                if (PlayerPrefs.HasKey(data.Id))
                    PlayerPrefs.DeleteKey(data.Id);
            }
            PlayerPrefs.Save();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        public void CancelScheduledNotification(NotificationData data)
        {
#if UNITY_ANDROID
            if (PlayerPrefs.HasKey(data.Id))
            {
                var id = PlayerPrefs.GetInt(data.Id);
                AndroidNotificationCenter.CancelScheduledNotification(id);
                PlayerPrefs.DeleteKey(data.Id);
                PlayerPrefs.Save();
            }
            else
                Debug.LogError($"등록된 {data.Id} 알림이 존재하지 않습니다.");
#elif UNITY_IOS
            iOSNotificationCenter.RemoveScheduledNotification(data.Id);
#endif
        }
    }

    [Serializable]
    public class NotificationData
    {
        public string Id;
        public string Name = "Temp Name";
        public int Importance = 3;
        public string Description = "Temp Description";

        public string Title = "Temp Title";
        public string Explain = "Temp Explain";
        public int FireTimeDays = 0;
        public int FireTimeHours = 0;
        public int FireTimeMinutes = 0;
        public int FireTimeSeconds = 10;
        public bool IsRepeat = false;

        public TimeSpan GetFireTimeSpan()
        {
            var fireTime = new TimeSpan(FireTimeDays, FireTimeHours, FireTimeMinutes, FireTimeSeconds);
            return fireTime;
        }

        public NotificationData(string id)
        {
            this.Id = id;
        }

        public NotificationData(NotificationData other)
        {
            this.Id = other.Id;
            this.Name = other.Name;
            this.Importance = other.Importance;
            this.Description = other.Description;
            this.Title = other.Title;
            this.Explain = other.Explain;
            this.FireTimeDays = other.FireTimeDays;
            this.FireTimeHours = other.FireTimeHours;
            this.FireTimeMinutes = other.FireTimeMinutes;
            this.FireTimeSeconds = other.FireTimeSeconds;
            this.IsRepeat = other.IsRepeat;
        }
    }
}
