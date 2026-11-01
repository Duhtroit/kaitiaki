using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "isHungryAction", story: "[Agent] is Hungry", category: "Action", id: "5be57a2ceb91fc484b61c2c84be89c6c")]
public partial class isHungryAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        animal animalScript = Agent.Value.GetComponent<animal>();
        return animalScript.isHungry() ? Status.Success : Status.Failure;
    }
}

