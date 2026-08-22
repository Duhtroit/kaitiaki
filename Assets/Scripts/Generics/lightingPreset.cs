using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptables/lighting Preset")]
public class lightingPreset : ScriptableObject
{    
    public Gradient ambientColour;
    public Gradient directionalColour;
    public Gradient fogColour;

}
