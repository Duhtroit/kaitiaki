using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "idleAction", story: "[Agent] is idel", category: "Action", id: "53f6ff7b6d73e446ad3b4e27a1ec33dc")]
public partial class idleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    float timeLeft;

    protected override Status OnStart()
    {
        animal animalScript = Agent.Value.GetComponent<animal>();

        animalScript.setAnimState(animalAnimState.idle);

        Vector2 range = animalScript.getData().idleTimeRange;

        timeLeft = UnityEngine.Random.Range(range.x, range.y);
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timeLeft -= Time.deltaTime;
        return timeLeft <= 0f ? Status.Success : Status.Running;
    }
}

