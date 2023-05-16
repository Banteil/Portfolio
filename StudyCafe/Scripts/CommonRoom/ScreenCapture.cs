using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCapture : MonoBehaviour
{
    public Camera _3DgameCamera;                // 게임화면(3D)을 보여주는 카메라
    public Camera _uiCamera;                    // ui를 보여주는 카메라
    public RawImage tempImage;
    [HideInInspector]
    public byte[] _data;        // 캡쳐가 완료된 Texture2D 를 보관하기 위해 사용

    public bool nowCapturing { get; private set; }

    public void ScreenCaptureButton()
    {
        StartCoroutine(ScreenCaptureProcess());        
    }

    IEnumerator ScreenCaptureProcess()
    {
        nowCapturing = true;
        yield return StartCoroutine(RenderToTexture(1920, 1080));
        tempImage.texture = FileBrowserDialogLib.GetTexture2D(_data);
    }

    private IEnumerator RenderToTexture(int renderSizeX, int renderSizeY)
    {
        // 캡처를 하려면 되도록 WaitForEndOfFrame 이후에 해야 합니다.
        // 그렇지 않으면 운이 나쁜 경우, 다 출력 되지 않은 상태의 화면을 찍게 될 수 있습니다.
        yield return new WaitForEndOfFrame();

        //RenderTexture 생성
        RenderTexture rt = new RenderTexture(renderSizeX, renderSizeY, 24);
        //RenderTexture 저장을 위한 Texture2D 생성
        Texture2D screenShot = new Texture2D(renderSizeX, renderSizeY, TextureFormat.ARGB32, false);

        // 카메라에 RenderTexture 할당
        _3DgameCamera.targetTexture = rt;
        _uiCamera.targetTexture = rt;

        // 각 카메라가 보고 있는 화면을 랜더링 합니다.
        _3DgameCamera.Render();
        _uiCamera.Render();

        // read 하기 위해, 랜더링 된 RenderTexture를 RenderTexture.active에 설정
        RenderTexture.active = rt;

        // RenderTexture.active에 설정된 RenderTexture를 read 합니다.
        screenShot.ReadPixels(new Rect(0, 0, renderSizeX, renderSizeY), 0, 0);
        screenShot.Apply();

        // 캡쳐가 완료 되었습니다.
        // 이제 캡쳐된 Texture2D 를 가지고 원하는 행동을 하면 됩니다.

        // File로 쓰고 싶다면 아래처럼 하면 됩니다.
        _data = screenShot.EncodeToPNG();

        // 사용한 것들 해제
        RenderTexture.active = null;
        _3DgameCamera.targetTexture = null;
        _uiCamera.targetTexture = null;
        Destroy(rt);

        // 동기화 플래그 해제
        nowCapturing = false;

        yield return 0;
    }
}
