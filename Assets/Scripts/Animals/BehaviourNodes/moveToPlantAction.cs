using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "moveToPlant", story: "[Agent] moves to plant", category: "Action", id: "8ef5e74f4119629b93b50017896be0b2")]
public partial class moveToPlantAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    animal animalScript;

    protected override Status OnStart()
    {
        animalScript = Agent.Value.GetComponent<animal>();
        animalScript.setAnimState(animalAnimState.moving);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!animalScript.hasValidTarget())
        {
            animalScript.clearTarget();
            return Status.Failure;
        }
        return animalScript.moveToTarget() ? Status.Success : Status.Running;
    }
}

