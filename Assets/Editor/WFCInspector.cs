using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Game.MapGeneration;

/// <summary>
/// Used to create new iterations of the map. 
/// Can be used in the inspector and at runtime. 
/// Maos generated in the editor will be desroyed on play, and recovered when playmode is stopped.
/// </summary>
[CustomEditor(typeof(WFCGenerator))]
public class WFCInspector : Editor
{
    WFCGenerator generator;

    public void OnEnable()
    {
       generator = target.GameObject().GetComponent<WFCGenerator>();
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Map"))
        {
            generator.Start();
            while (!generator.done)
            {
                generator.Collapse();
            }
        }

        if (Application.isPlaying) return;

        if(GUILayout.Button("Clear Grid"))
        {   
            generator.RemoveInspectorGrid();
        }
    }
}
