using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildList", menuName = "BuildabeObjects/BuildList")]
public class BuildList : ScriptableObject
{
    [SerializeField] private List<GameObject> buildableObjects = new();

    public List<GameObject> GetBuildList() { return buildableObjects; }
}
