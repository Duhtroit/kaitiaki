using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "wantsToleaveAction", story: "[Agent] wants to leave", category: "Action", id: "c2d0d81fa8b85461375f916d5bd1ab7e")]
public partial class WantsToleaveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    protected override Status OnStart()
    {
        animal animalScript = Agent.Value.GetComponent<animal>();
        return animalScript.wantsToLeave() ? Status.Success : Status.Failure;
    }
}

