using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "choseWaypoint", story: "[Agent] chooses waypoint", category: "Action", id: "7ad383714a73b04407b51d775122ec0c")]
public partial class chooseWaypointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        animal animalScript = Agent.Value.GetComponent<animal>();
        animalScript.chooseWaypoint();
        return Status.Success;
    }
}

