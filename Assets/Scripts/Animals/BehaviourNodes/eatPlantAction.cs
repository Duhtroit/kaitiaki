using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "eatPlant", story: "[Agent] eats the plant", category: "Action", id: "9179d0d4f5b4a9e4ac66813f73812807")]
public partial class eatPlantAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    animal animalScript;
    float biteTimer;

    protected override Status OnStart()
    {
        animalScript = Agent.Value.GetComponent<animal>();
        if (animalScript == null || !animalScript.hasValidTarget())
        {
            return Status.Failure;
        }
        animalScript.setAnimState(animalAnimState.eating);
        biteTimer = animalScript.getData().eatInterval; //wait one interval before the first bite so the eat animation plays first
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!animalScript.hasValidTarget())
        {
            return Status.Failure;
        }

        biteTimer -= Time.deltaTime;
        if (biteTimer > 0f)
        {
            return Status.Running;
        }

        biteTimer = animalScript.getData().eatInterval;
        bool full = animalScript.bite();
        return full ? Status.Success : Status.Running;
    }
    protected override void OnEnd()
    {
        if (animalScript != null)
        {
            animalScript.clearTarget();
            animalScript.setAnimState(animalAnimState.idle);
        }
    }
}

