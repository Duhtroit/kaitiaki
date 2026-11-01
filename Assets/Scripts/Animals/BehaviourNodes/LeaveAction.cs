using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "leaveAction", story: "Agent leaves", category: "Action", id: "d05137f1ff07b092ccebf4fc8edae16c")]
public partial class LeaveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    animal animalScript; 
    protected override Status OnStart()
    {
        animalScript = Agent.Value.GetComponent<animal>();
        if (animalScript == null)
        {
            Debug.Log("animal script still null");
            return Status.Failure;
        }
        animalScript.startLeaving();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // the animal can be gone by now, so check before using it
        if (animalScript == null)
        {
            return Status.Failure;
        }

        if (animalScript.moveToDestination(false))
        {
            animalScript.despawn();
            return Status.Success;
        }
        return Status.Running;
    }
}
