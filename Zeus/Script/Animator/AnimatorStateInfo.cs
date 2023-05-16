using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public interface IAnimatorStateInfoController
    {
        AnimatorStateInfos AnimatorStateInfos { get; }
    }

    public static class IAnimatorStateInfoHelper
    {
        public static void Register(this IAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.IsValid())
            {
                animatorStateInfos.AnimatorStateInfos.RegisterListener();
            }
        }

        public static void UnRegister(this IAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.IsValid())
            {
                animatorStateInfos.AnimatorStateInfos.RegisterListener();
            }
        }


        public static bool IsValid(this IAnimatorStateInfoController animatorStateInfos)
        {
            return animatorStateInfos != null && animatorStateInfos.AnimatorStateInfos != null && animatorStateInfos.AnimatorStateInfos.Animator != null;
        }
    }



    [System.Serializable]
    public class AnimatorStateInfos
    {
        public bool UseDebug;
        public Animator Animator;

        public AnimatorStateInfos(Animator animator)
        {
            Animator = animator;

            Init();
        }

        public void Init()
        {
            if (Animator)
            {
                StateInfos = new StateInfo[Animator.layerCount];
                for (int i = 0; i < StateInfos.Length; i++)
                {
                    StateInfos[i] = new StateInfo(i);
                }
            }
        }

        public void RegisterListener()
        {
            var bhv = Animator.GetBehaviours<AnimatorTagBase>();
            for(int i = 0; i < bhv.Length; i++)
            {
                bhv[i].RemoveStateInfoListener(this);
                bhv[i].AddStateInfoListener(this);
            }
            if (UseDebug)
            {
                Debug.Log($"Listeners Registered", Animator);
            }
        }

        public void RemoveListener()
        {
            if (Animator)
            {
                var bhv = Animator.GetBehaviours<AnimatorTagBase>();
                for(int i = 0; i < bhv.Length; i++)
                {
                    bhv[i].RemoveStateInfoListener(this);
                }
                if (UseDebug)
                {
                    Debug.Log($"Listeners Removed", Animator);
                }
            }
        }


        public StateInfo[] StateInfos = new StateInfo[0];
        [System.Serializable]
        public class StateInfo
        {
            public int Layer;
            public int ShortPathHash;
            public float NormalizedTime;
            public List<string> Tags = new List<string>();
            public StateInfo(int layer)
            {
                this.Layer = layer;
            }
        }

        public void AddStateInfo(string tag, int layer)
        {
            if(StateInfos.Length>0 && layer < StateInfos.Length)
            {
                StateInfo info = StateInfos[layer];
                info.Tags.Add(tag);
                info.ShortPathHash = 0;
                info.NormalizedTime = 0;
            }
            if (UseDebug)
            {
                Debug.Log($"<color=green>Add tag : <b><i>{tag}</i></b></color>,in the animator layer :{layer}", Animator);
            }
        }

        public void UpdateStateInfo(int layer, float normalizedTime, int fullPathHash)
        {
            if(StateInfos.Length >0 && layer < StateInfos.Length)
            {
                StateInfo info = StateInfos[layer];
                info.NormalizedTime = normalizedTime;
                info.ShortPathHash = fullPathHash;
            }
        }

        public void RemoveStateInfo(string tag, int layer)
        {
            if(StateInfos.Length >0 && layer < StateInfos.Length)
            {
                StateInfo info = StateInfos[layer];
                if (info.Tags.Contains(tag))
                {
                    info.Tags.Remove(tag);
                    if(info.Tags.Count == 0)
                    {
                        info.ShortPathHash = 0;
                        info.NormalizedTime = 0;
                    }
                    if (UseDebug)
                    {
                        Debug.Log($"<color=red>Remove tag : <b><i>{tag}</i></b></color>, in the animator layer :{layer}", Animator);
                    }
                }
            }
        }

        public bool HasTag(string tag)
        {
            return System.Array.Exists(StateInfos, info => info.Tags.Contains(tag));
        }

        public bool HasAllTags(params string[] tags)
        {
            var has = tags.Length > 0 ? true : false;
            for(int i=0; i<tags.Length; i++)
            {
                if (!HasTag(tags[i]))
                {
                    has = false;
                    break;
                }
            }
            return has;
        }

        public bool HasAnyTag(params string[] tags)
        {
            var has = false;
            for(int i = 0; i<tags.Length; i++)
            {
                if (HasTag(tags[i]))
                {
                    has = true;
                    break;
                }
            }
            return has;
        }

        public StateInfo GetStateInfoUsingTag(string tag)
        {
            return System.Array.Find(StateInfos, info => info.Tags.Contains(tag));
        }

        public float GetCurrentNormalizedTime(int layer)
        {
            if(StateInfos.Length >0 && layer < StateInfos.Length)
            {
                StateInfo info = StateInfos[layer];
                return info.NormalizedTime;
            }

            return 0;
        }
    }
}


