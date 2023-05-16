using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class AnimationStateTable
    {
        public int BaseLayer { get; set; }
        public int RightArmLayer { get; set; }
        public int LeftArmLayer { get; set; }
        public int UpperBodyLayer { get; set; }
        public int UnderBodyLayer { get; set; }
        public int FullBodyLayer { get; set; }

        public Dictionary<int, List<int>> HitStateHash = new();

        readonly string[] _hitType = { "Small", "Big", "Down" };
        readonly string[] _directionType = { "Front", "Back", "Left", "Right" };

        public AnimationStateTable(Animator animator)
        {
            BaseLayer = animator.GetLayerIndex("Base Layer");
            RightArmLayer = animator.GetLayerIndex("RightArm");
            LeftArmLayer = animator.GetLayerIndex("LeftArm");
            UpperBodyLayer = animator.GetLayerIndex("UpperBody");
            UnderBodyLayer = animator.GetLayerIndex("UnderBody");
            FullBodyLayer = animator.GetLayerIndex("FullBody");

            for (int i = 0; i < _hitType.Length; i++)
            {
                HitStateHash.Add(i, new List<int>());
                for (int j = 0; j < _directionType.Length; j++)
                {
                    HitStateHash[i].Add(Animator.StringToHash($"{_hitType[i]}_Hit_{_directionType[j]}"));
                }
            }
        }
    }
}
