using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Void Event", menuName = "Events/Void Event")]
public class voidEvent : abstractEvent<EmptyPayload>
{

}

public struct EmptyPayload {}
