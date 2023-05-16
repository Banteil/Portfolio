using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Zeus
{
    public class NorthlandSceneManager : MonoBehaviour
    {
        //[SerializeField] private List<CutsceneInfo> _cutsceneList = new List<CutsceneInfo>();

        //[SerializeField] private List<int> _playedCutscenes = new List<int>();

        //private void Awake()
        //{
        //    for (int i = 0; i < _cutsceneList.Count; i++)
        //    {
        //        var cutsceneInfo = _cutsceneList[i];
        //        cutsceneInfo.Timeline.gameObject.SetActive(false);
        //        cutsceneInfo.Timeline.stopped += (director) => { OnFinishCutscene(cutsceneInfo); };
        //    }
        //}

        //private void OnFinishCutscene(CutsceneInfo cutsceneInfo)
        //{
        //    cutsceneInfo.CallFinish.Invoke();
        //    cutsceneInfo.Timeline.gameObject.SetActive(false);
        //    if (!cutsceneInfo.PlayerControllable)
        //        InputReader.Instance.EnablePrevControl();
        //}

        //public void PlayCutscene(int id)
        //{
        //    // 이미 실행된 컷씬인지 id 확인
        //    if (_playedCutscenes.Contains(id)) return;

        //    _playedCutscenes.Add(id);

        //    var cutsceneInfo = _cutsceneList.Find(x => x.ID == id);
        //    if (cutsceneInfo == null) return;

        //    cutsceneInfo.Timeline.gameObject.SetActive(true);
        //    cutsceneInfo.Timeline.Play();
        //    if (!cutsceneInfo.PlayerControllable)
        //        InputReader.Instance.EnableMapPlayerControls(false, true);
        //}
    } 
}