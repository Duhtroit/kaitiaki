using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "moveToWaypoint", story: "[Agent] moves to waypoint", category: "Action", id: "567caa9f7b77cae52241ddca8d8fd386")]
public partial class moveToWaypointAction : Action
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
        return animalScript.moveToDestination(true) ? Status.Success : Status.Running;
    }
}

