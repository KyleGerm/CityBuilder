using Codice.Client.Common.GameUI;
using Game;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityBehaviour))]
public class EntityInspector : Editor
{
    private EntityBehaviour entityBehaviour;
    private SerializedProperty houseLvl;
    private SerializedProperty resourceLvl;
    private SerializedProperty wage;
    private SerializedProperty tax;
    private List<SerializedProperty> contributions = new List<SerializedProperty>();
    List<int> values = new List<int>();
    // Start is called before the first frame update
    public void OnEnable()
    {
        wage = serializedObject.FindProperty("wageContributionPercentToHappiness");
        houseLvl = serializedObject.FindProperty("houseLevelContributionPercentToHappiness");
        tax = serializedObject.FindProperty("taxContributionPercentToHappiness");
        resourceLvl = serializedObject.FindProperty("resourceContributionPercentToHappiness");
        contributions.AddRange(new List<SerializedProperty>() { wage, houseLvl, tax, resourceLvl});
        entityBehaviour = target.GameObject().GetComponent<EntityBehaviour>();
    }

    public override void OnInspectorGUI()
    {
        SliderInspector();
        base.OnInspectorGUI();
        WalletInspection();
    }
    /// <summary>
    /// Shows the value of the Money property in a non-editable format
    /// If more than one entity is selected, this will not show.
    /// </summary>
    private void WalletInspection()
    {
        if (targets.Length > 1) return;
        try
        {
            EditorGUILayout.LabelField($"Money: {entityBehaviour.Money}");
        }
        catch
        {
            return;
        }
    }

    /// <summary>
    /// Limits the values of all the sliders in the inspector to a total value of 100,
    ///so the values can be used for percentages
    /// </summary>
    private void SliderInspector()
    {
       
        int total = 0;
        contributions.ForEach(x => total += x.intValue);


        if (total >= 100 && values.Count == contributions.Count)
        {
            for (int i = 0; i < contributions.Count; i++)
            {
                if (values[i] != contributions[i].intValue)
                {
                    contributions[i].intValue -= Mathf.Abs(100 - total);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        values = new();
        values.AddRange(contributions.ConvertAll(x => x.intValue));
    }
}
