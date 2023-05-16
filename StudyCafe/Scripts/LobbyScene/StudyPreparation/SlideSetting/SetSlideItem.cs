using UnityEngine;
using UnityEngine.UI;

public class SetSlideItem : MonoBehaviour
{
    public Text fileNameText;
    public Text indexText;
    public Image iconImage;
    public RawImage thumbnail;
    public SlideFileControl slideFileControl;
    public FileType fileType;
    int index;
    
    public int Index
    {
        get { return index; }
        set
        {
            index = value;
            indexText.text = (index + 1).ToString();
        }
    }

    public void SetFileInfo(string fileName)
    {
        fileNameText.text = fileName;
        string extension = FileBrowserDialogLib.GetFileResolution(fileName);
        Sprite icon = Resources.Load<Sprite>("UISprite/FileFormat/" + extension);
        if (icon != null)
            iconImage.sprite = icon;
        else
            iconImage.sprite = Resources.Load<Sprite>("UISprite/FileFormat/blank");

        fileType = DataManager.Instance.CheckFileType(extension);
    }

    public void ClickButton() => slideFileControl.SelectSlideButton(index);
}