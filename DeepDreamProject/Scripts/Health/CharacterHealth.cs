using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : Health
{
	[Header("Character Health Info")]
	public Character Character;
	public int VitalityIncreaseRate = 5;

	public override float MaximumHP
    {
		get
        {
			if(Character != null)
            {
				float statValue = InitialHP + ((Character.CharacterStat ? Character.CharacterStat.VitalityStat.Value : 0) * VitalityIncreaseRate);
				return statValue;
			}
			else
            {
				return InitialHP;
            }
		}
		set
        {
			InitialHP = value;
		}
    }

	/// <summary>
	/// Grabs useful components, enables damage and gets the inital color
	/// </summary>
	public override void Initialization()
	{		
		if(Character == null)
			Character = this.gameObject.GetComponentInParent<Character>();

		base.Initialization();
	}

	protected override void BindAnimator()
	{
		if (TargetAnimator != null) return;
		if (Character != null)
		{
			if (Character.Animator != null)
				TargetAnimator = Character.Animator;
			else
				TargetAnimator = GetComponentInChildren<Animator>();
		}
		else
		{
			TargetAnimator = GetComponentInChildren<Animator>();
		}
	}
}
