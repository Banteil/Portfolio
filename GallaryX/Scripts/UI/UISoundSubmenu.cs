using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UISoundSubmenu : UISubmenu
    {
        enum SoundSlider
        {
            VolumeControlSlider,
        }

        protected override void OnStart()
        {
            base.OnStart();
            Bind<Slider>(typeof(SoundSlider));
            var volumeSlider = Get<Slider>((int)SoundSlider.VolumeControlSlider);
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }

        private void ChangeVolume(float value) => GameManager.Instance.ChangeVolume(value);
    }
}
