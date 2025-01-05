namespace starinc.io
{
    public class PlayOneShotEffect : BaseEffect
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            _startPlayback = true;
        }

        protected override void ChangeSpriteProcess()
        {
            _currentFrame++;
            if(_currentFrame >= _sprites.Count)
            {
                Stop();
                DestroyEffect();
                return;
            }
            _spriteRenderer.sprite = _sprites[_currentFrame];
        }
    }
}