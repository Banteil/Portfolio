using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class AnimatorParameter
    {
        readonly AnimatorControllerParameter _parameter;

        public static implicit operator int(AnimatorParameter a)
        {
            if (a.isValid) return a._parameter.nameHash;
            else
                return -1;
        }
        public readonly bool isValid;

        public AnimatorParameter(Animator animator, string parameter)
        {
            if (animator && animator.ContainsParam(parameter))
            {
                _parameter = animator.GetValidParameter(parameter);
                this.isValid = true;
            }

            else this.isValid = false;
        }
    }

    public static class AnimatorParameterHelper
    {
        public static AnimatorControllerParameter GetValidParameter(this Animator anim, string paramName)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName) return param;
            }
            return null;
        }

        public static bool ContainsParam(this Animator anim, string paramName)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }

        public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
        {
            if (null == self)
            {
                return false;
            }

            var parameters = self.parameters;
            foreach (var currParam in parameters)
            {
                if (currParam.type == type && currParam.name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
