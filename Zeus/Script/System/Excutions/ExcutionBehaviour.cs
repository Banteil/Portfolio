using System;
using UnityEngine;

namespace Zeus
{
	public abstract class ExcutionBehaviour : MonoBehaviour
	{
		public virtual void Play(Character owner, Character target, ExcutionData excutionData, Action onFinish) { }
	}
}
