using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Zeus
{
    public class DefaultPlayerExcution : ExcutionBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private CinemachineVirtualCamera _camera;

        private Character _player;
        private Character _enemy;
        private ExcutionData _excutionData;
        private RuntimeAnimatorController _animator;

        private bool _isPlaying;

        private void Update()
        {
            if (_director == null) return;
            if (!_isPlaying) return;

            if (_director.duration > _director.time)
                _director.time += GameTimeManager.Instance.DeltaTime;
        }

        public override void Play(Character player, Character enemy, ExcutionData excutionData, Action onFinish)
        {
            _isPlaying = false;
            _player = player;
            _enemy = enemy;
            _excutionData = excutionData;

            StartCoroutine(PlayCO(onFinish));
        }

        private IEnumerator PlayCO(Action onFinish)
        {
            yield return new WaitForEndOfFrame();
            var playerTransform = _player.transform;
            var playerRot = _player.transform.rotation;
            var targetTransform = _enemy.transform;
            targetTransform.LookAt(playerTransform);
            playerTransform.LookAt(targetTransform);
            playerTransform.DOMove(_excutionData.OwnerPoint.position, 0.2f);

            var behaviourTransform = this.transform;
            behaviourTransform.SetParent(playerTransform);
            behaviourTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            _player.IsInvincibility = true;
            _player.Rigidbody.useGravity = false;

            var enemyAI = _enemy.GetComponent<ZeusAIController>();
            enemyAI.IsStop = true;

            _player.Animator.SetInteger("ExcutionNum", (int)_excutionData.DirectionType);
            _player.Animator.SetTrigger("ExcutionTrigger");

            _animator = _enemy.Animator.runtimeAnimatorController;
            var enemyAnimator = TableManager.GetAnimator(_excutionData.AnimatorPath);
            _enemy.Animator.runtimeAnimatorController = enemyAnimator;
            AnimatorSpeedManager.Get().AddAnimator(_enemy.Animator);
            _enemy.Animator.Rebind();


            var defaultCamera = CameraManager.Get().GetCamera(TypeCamera.DEFAULT);
            var isThirdPersonViewCamera = _player.GetComponent<ThirdPersonController>().StrafeSpeed.RotateWithCamera;
            Debug.Log("isThirdPersonViewCamera : " + isThirdPersonViewCamera);
            if (isThirdPersonViewCamera)
            {
                var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
                var timelineAsset = _director.playableAsset as TimelineAsset;
                var tracks = timelineAsset.GetOutputTracks();
                foreach (var track in tracks)
                {
                    if (track is CinemachineTrack cinemachineTrack)
                    {
                        _director.SetGenericBinding(track, cameraBrain);
                    }
                }

                defaultCamera.VCamera.enabled = false;
                _camera.enabled = true;
            }
            else
                _camera.enabled = false;

            bool isFinish = false;
            //_director.playableGraph.GetRootPlayable(0).SetSpeed(GameTimeManager.Instance.WorldTimeScale);
            _director.stopped += (director) =>
            {
                if (isThirdPersonViewCamera)
                {
                    _camera.enabled = false;
                    defaultCamera.VCamera.enabled = true;
                    _player.GetComponent<ThirdPersonInput>().TargetYRotation = _camera.transform.eulerAngles.y;
                    defaultCamera.VCamera.ForceCameraPosition(_camera.transform.position, _camera.transform.rotation);
                }
                _player.transform.rotation = playerRot;
                _player.IsInvincibility = false;

                var collider = _player.GetComponent<CapsuleCollider>();
                _player.Rigidbody.useGravity = true;

                onFinish?.Invoke();
                isFinish = true;
            };

            _isPlaying = true;
            _director.Play();
            _enemy.ChangeHealth(1);

            yield return new WaitUntil(() => isFinish);
            yield return new WaitForEndOfFrame();

            _enemy.ChangeHealth(0);

            yield return new WaitForSeconds(1f);

            Destroy(this.gameObject);
        }

        public void EnabledEnemyTrigger(bool enabled)
        {
            _enemy.Rigidbody.useGravity = !enabled;
            var collider = _enemy.GetComponent<Collider>();
            collider.isTrigger = enabled;
        }
    }
}
