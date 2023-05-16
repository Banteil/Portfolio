using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FileInfoItem : MonoBehaviour
{
    public Text fileNameText;
    public Image iconImage;
    public GameObject downloadButtonObj;
    public GameObject deleteButtonObj;
    public GameObject selectFileToggleObj;

    [Header("ViewerPanel")]
    public Image imageViewer;
    public Image videoViewer;
    public Image audioViewer;

    public StringFunction openViewerFunction;
    string fileDir;
    string studyGUID;
    public string StudyGUID
    {
        set
        {
            studyGUID = value;
            fileDir = "/../storage/data/" + studyGUID + "/";
        }
    }

    public bool IsSelectToggleOn
    {
        get
        {
            Toggle toggle = selectFileToggleObj.GetComponent<Toggle>();
            return toggle.isOn;
        }
    }

    public void SetFileInfo(string fileName)
    {
        fileNameText.text = fileName;
        string extension = FileBrowserDialogLib.GetFileResolution(fileName);
        Sprite icon = Resources.Load<Sprite>("UISprite/FileFormat/" + extension);
        if(icon != null)
            iconImage.sprite = icon;
        else
            iconImage.sprite = Resources.Load<Sprite>("UISprite/FileFormat/blank");

        FileType fileType = DataManager.Instance.CheckFileType(extension);
        CheckFileType(fileType);
    }

    /// <summary>
    /// 파일의 타입을 체크하여 타입에 맞게 뷰어를 세팅하는 함수
    /// </summary>
    void CheckFileType(FileType type)
    {
        switch (type)
        {
            case FileType.IMAGE:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                StartCoroutine(ImageSetting());
                break;
            case FileType.VIDEO:
                imageViewer.gameObject.SetActive(false);
                videoViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                break;
            case FileType.DOCUMENT:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                StartCoroutine(DocumentSetting());
                break;
            case FileType.AUDIO:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(false);
                audioViewer.gameObject.SetActive(true);
                break;
            default:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 이미지 정보를 받아와 sprite에 세팅하는 함수
    /// </summary>
    IEnumerator ImageSetting()
    {
        yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileNameText.text));
        byte[] data = DataManager.Instance.DataInfo;
        imageViewer.sprite = FileBrowserDialogLib.GetSprite(data);
    }

    /// <summary>
    /// 문서(pdf) 파일의 정보를 뷰어에 세팅하는 함수
    /// </summary>
    IEnumerator DocumentSetting()
    {
        string[] extension = fileNameText.text.Split('.');
        if (extension[1].Equals("pdf"))
        {
            int count = 1;
            string pdfImageName = extension[0] + "_" + count + ".jpg";
            yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, pdfImageName));
            byte[] pdfData = DataManager.Instance.DataInfo;
            if (pdfData == null) 
                yield break;
            else
                imageViewer.sprite = FileBrowserDialogLib.GetSprite(pdfData);
        }
    }

    public void OpenFileViewer() => openViewerFunction?.Invoke(fileNameText.text);

    public void DownloadFileButton()
    {
        //스터디 아이템 선택 과정에서 DataManager.interactionData에 스터디 GUID 정보 저장됨
        //캐비넷 타입 선택 과정에서 DataManager.interactionData에 파일 Type 정보 저장됨
        string fileName = fileNameText.text;
        StartCoroutine(DataManager.Instance.FileDownload(fileDir, fileName));
    }

    public void DeleteFileButton()
    {
        StartCoroutine(DeleteProcess());
    }

    IEnumerator DeleteProcess()
    {
        yield return StartCoroutine(DataManager.Instance.FileDelete(fileNameText.text, studyGUID));
        if(DataManager.Instance.info.Equals("SUCCESS")) Destroy(gameObject);
    }
}
