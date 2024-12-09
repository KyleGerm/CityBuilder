using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using Game;

//#pragma warning disable 0618 
//EditorGUILayout.ObjectField flags a warning on the console for being obsolete. This disables it.
//warning relates to a bool parameter only.
//Setting this to false does nothing since the field is disabled, but assigning a value does stop the warning. 
[CustomEditor(typeof(Business),true)]
public class BusinessInspector : Editor
{
    private Business business;
    private SerializedProperty theFloat;
    private SerializedProperty theInt;
    public void OnEnable()
    {
        business = target.GameObject().GetComponent<Business>();
        theFloat = serializedObject.FindProperty("interactionTime");
        theInt = serializedObject.FindProperty("interactionValue");

    }
    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        if (theFloat != null) EditorGUILayout.PropertyField(theFloat,new GUIContent("Wait Time"));
        if (business.GetType() == typeof(Company))
        {
            EditorGUILayout.PropertyField(theInt, new GUIContent("Payment"));
        }
        else EditorGUILayout.PropertyField(theInt, new GUIContent("Cost of Goods"));
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
        
    }
}
