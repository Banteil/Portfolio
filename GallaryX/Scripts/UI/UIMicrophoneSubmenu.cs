using System.Linq;
using TMPro;
using UnityEngine;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace starinc.io.gallaryx
{
    public class UIMicrophoneSubmenu : UISubmenu
    {
        enum MicrophoneDropdown
        {
            MicrophoneDropdown,
        }

        protected override void OnStart()
        {
            base.OnStart();
            Bind<TMP_Dropdown>(typeof(MicrophoneDropdown));
            CheckMicrophoneState();
        }

        private void CheckMicrophoneState()
        {
            var devicesDropdown = Get<TMP_Dropdown>((int)MicrophoneDropdown.MicrophoneDropdown);
            devicesDropdown.onValueChanged.AddListener(DevicesDropdownValueChangedHandler);
            AIManager.Instance.SelectedDevice = string.Empty;

            Microphone.RecordStreamDataEvent += RecordStreamDataEventHandler;
            Microphone.PermissionChangedEvent += PermissionChangedEvent;

            RefreshMicrophoneDevicesButtonOnclickHandler();
        }


        private void RefreshMicrophoneDevicesButtonOnclickHandler()
        {
            var devicesDropdown = Get<TMP_Dropdown>((int)MicrophoneDropdown.MicrophoneDropdown);
            devicesDropdown.ClearOptions();
            devicesDropdown.AddOptions(Microphone.devices.ToList());
            DevicesDropdownValueChangedHandler(0);
        }

        private void DevicesDropdownValueChangedHandler(int index)
        {
            if (index < Microphone.devices.Length)
            {
                AIManager.Instance.SelectedDevice = Microphone.devices[index];
            }
        }

        private void RecordStreamDataEventHandler(Microphone.StreamData streamData)
        {
            // handle streaming recording data
        }

        private void PermissionChangedEvent(bool granted)
        {
            // handle current permission status

            if (AIManager.Instance.PermissionGranted != granted)
                RefreshMicrophoneDevicesButtonOnclickHandler();

            AIManager.Instance.PermissionGranted = granted;
            Debug.Log($"Permission state changed on: {granted}");
        }
    }
}
