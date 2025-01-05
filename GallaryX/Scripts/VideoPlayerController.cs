using starinc.io.gallaryx;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    private Exhibits _exhibits;
    private VideoPlayer _player;

    private void Start()
    {
        _exhibits = GetComponent<Exhibits>();
        _player = GetComponent<VideoPlayer>();
    }

    private void Update()
    {
        if (_exhibits.ExhibitsType == starinc.io.Define.FileType.VIDEO)
        {
            if (!_player.isPlaying)
                _player.Play();
        }
    }
}
