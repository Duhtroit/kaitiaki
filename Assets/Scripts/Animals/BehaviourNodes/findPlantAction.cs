using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "findPlant", story: "[Agent] finds a plant", category: "Action", id: "36dd7f427bfe75a055aa3dba826e958f")]
public partial class findPlantAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;


    protected override Status OnStart()
    {
        animal animalScript = Agent.Value.GetComponent<animal>();
        return animalScript.foundPlant() ? Status.Success : Status.Failure;
    }
}

