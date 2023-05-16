using UnityEngine;
using UnityEngine.UI;

public class SlideFileItem : MonoBehaviour
{
    public Text fileNameText;
    public Image iconImage;
    public SlideFileControl slideFileControl;

    public void SetFileInfo(string fileName)
    {
        fileNameText.text = fileName;
        string extension = FileBrowserDialogLib.GetFileResolution(fileName);
        Sprite icon = Resources.Load<Sprite>("UISprite/FileFormat/" + extension);
        if (icon != null)
            iconImage.sprite = icon;
        else
            iconImage.sprite = Resources.Load<Sprite>("UISprite/FileFormat/blank");
    }

    public void ClickButton() => slideFileControl.SelectFileButton(this);
}
