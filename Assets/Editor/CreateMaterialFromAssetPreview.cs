using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateMaterialFromAssetPreview : EditorWindow
{
    Texture2D tex;
    [MenuItem("Tools/Create UI Material From Asset Previews")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow(typeof(CreateMaterialFromAssetPreview));
        wnd.titleContent = new GUIContent("UI Mat Maker");
        wnd.minSize = new Vector2(280, 330);
        wnd.maxSize = wnd.minSize;
    }

    public void OnGUI()
    {
        GUILayout.Space(10);
        if(GUILayout.Button("Make Material From Selected Asset") && Selection.activeGameObject)
        {
            MakeTheMaterial();
        }
        if (Selection.activeGameObject == null) return;

        tex = AssetPreview.GetAssetPreview(Selection.activeGameObject);
        GUI.DrawTexture(new Rect(10, 50, 256, 256),tex,ScaleMode.ScaleToFit);
    }

    private void MakeTheMaterial()
    {
        Debug.Log(tex);
        Material newMat = new(Shader.Find("UI/Default"));
        newMat.mainTexture = tex;
        AssetDatabase.CreateAsset(newMat, "Assets/Resources/TileMap/UIMaterials/test-UI.mat");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
