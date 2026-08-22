using System;
using System.Globalization;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// [ExecuteAlways]
public class sunController : MonoBehaviour
{

    [SerializeField] private Light sun;
    [SerializeField] private lightingPreset preset;
    [SerializeField] Material skyBoxMaterial;

    void Update()
    {
        
    }

    public void updateLighting(float timePercent)
    {
    
        RenderSettings.ambientLight = preset.ambientColour.Evaluate(timePercent);
        RenderSettings.fogColor = preset.fogColour.Evaluate(timePercent);
        RenderSettings.fogDensity = timePercent * 100;
        

        if (sun != null)
        {
            sun.intensity = timePercent * 2;
            sun.color = preset.directionalColour.Evaluate(timePercent);
            sun.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

        if (skyBoxMaterial != null)
        {
            float dot = Vector3.Dot(sun.transform.forward, Vector3.up);
            float blend = Mathf.Lerp(0,1, dot);
            skyBoxMaterial.SetFloat("_Blend", blend);
        }
        
        Debug.Log($"Lighting Updated based on timePercent = {timePercent}");

    }

}
