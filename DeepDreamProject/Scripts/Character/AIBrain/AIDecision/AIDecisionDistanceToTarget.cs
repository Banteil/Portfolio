using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDecisionDistanceToTarget : AIDecision
{
    public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreaterThan, StrictlyGreaterThan }
    public ComparisonModes ComparisonMode = ComparisonModes.GreaterThan;
    public float Distance;

    public override bool Decide()
    {
        return EvaluateDistance();
    }

    /// <summary>
    /// Brain이 지정한 타겟과의 거리를 조건으로 체크함
    /// </summary>
    /// <returns></returns>
    protected virtual bool EvaluateDistance()
    {
        if (_brain.Target == null)
        {
            return false;
        }

        float distance = Vector3.Distance(_brain.Owner.transform.position, _brain.Target.position);

        switch (ComparisonMode)
        {
            case ComparisonModes.StrictlyLowerThan:
                return distance < Distance;
            case ComparisonModes.LowerThan:
                return distance <= Distance;
            case ComparisonModes.Equals:
                return distance == Distance;
            case ComparisonModes.GreaterThan:
                return distance >= Distance;
            case ComparisonModes.StrictlyGreaterThan:
                return distance > Distance;
            default:
                return false;
        }
    }
}
