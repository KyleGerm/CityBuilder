using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine.Windows;

public class ComponentManager : EditorWindow
{
    [MenuItem("Tools/Bulk Component Editor")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow(typeof(ComponentManager));
        wnd.titleContent = new GUIContent("Component Manager");
    }

    List<System.Type> list = new();
    List<string> selectedComponents = new List<string>();
    TextField input;
    ListView leftPane;
    ListView rightPane;
    string previous = string.Empty;
    List<string> names;
    Button removeButton;

    private void OnEnable()
    {
        list = new(System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(assembly => assembly.GetTypes())
    .Where(type => type.IsClass && type.IsSubclassOf(typeof(Component))));
    }

    public void CreateGUI()
    {
        rootVisualElement.Add(new HelpBox("Search and select the components you wish to add to your gameObject.\n" +
            "Selected components will appear on the right side of the panel.\n" +
            "Selecting the items on the right side will remove them from the list.\n" +
            "Press the 'Add Components' button to apply all components to each selected item in the scene.",HelpBoxMessageType.Info));

        var splitView = new TwoPaneSplitView(0,250,TwoPaneSplitViewOrientation.Horizontal);
        rootVisualElement.Add(splitView);

        leftPane = new ListView();
        leftPane.selectionType = SelectionType.Single;
        rightPane = new ListView();
        rightPane.selectionType = SelectionType.Single;

        splitView.Add(leftPane);
        splitView.Add(rightPane);

        input = new TextField("Component Search");
        leftPane.hierarchy.Insert(0, input);
        leftPane.selectionChanged += OnComponentSelection;
        rightPane.selectionChanged += RemoveSelection;

        removeButton = new Button();
        removeButton.text = "Remove All Components";
        rightPane.hierarchy.Insert(1, removeButton);
        removeButton.clicked += RemoveAllComponents;
        var accept = new Button();
        accept.text = "Add Components";
        rightPane.hierarchy.Insert(0,accept);
        accept.clicked += ApplySelection;
    }

    private void OnGUI()
    {


        if (input.value != string.Empty && input.value.ToLower() != previous)
        { 
            
            names = new List<string>();
            foreach (var item in list)
            {
                if (item.Name.Contains(input.value,System.StringComparison.OrdinalIgnoreCase))
                {
                    names.Add(item.Name);
                }
            }
            previous = input.value.ToLower();
            if (names.Count > 0)
            {
                leftPane.itemsSource = names;
                leftPane.makeItem = () => new Label();
                leftPane.bindItem = (item, index) =>
                {
                    if (index < names.Count)
                    {
                        (item as Label).text = names[index];
                    };
                };
            }
        }

        if(Selection.objects.Length > 0 && !removeButton.visible )
        {
            removeButton.visible = true;
        }
        else if(Selection.objects.Length == 0 && removeButton.visible) 
        {
            removeButton.visible = false;
        }
        
    }

    private void OnComponentSelection(IEnumerable<object> selected)
    {
     
        foreach (var component in selected)
        {
            if(!selectedComponents.Contains(component.ToString()))
            selectedComponents.Add(component.ToString());
        }
        rightPane.itemsSource = selectedComponents;
        rightPane.makeItem = () => new Label();
        rightPane.bindItem = (item, index) =>
        {
            if (index < selectedComponents.Count)
            (item as Label).text = selectedComponents[index]; 
        };

        rightPane.Rebuild();
        leftPane.selectedIndex = -1;
    }

    private void ApplySelection()
    {
        Object[] obj = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable);

       foreach(var component in list)
        {
            if (selectedComponents.Contains(component.Name))
            {
                foreach(var item in obj)
                {
                    item.GameObject().AddComponent(component);
                }
            }
        }
    }

    private void RemoveSelection(IEnumerable<object> selected)
    {
        
        foreach (var component in selected)
        {
            if (selectedComponents.Contains(component.ToString()))
                selectedComponents.Remove(component.ToString());
        }
        rightPane.itemsSource = selectedComponents;
        rightPane.makeItem = () => new Label();
        rightPane.bindItem = (item, index) =>
        {
            if (index < selectedComponents.Count)
                (item as Label).text = selectedComponents[index];
        };

        rightPane.Rebuild();
        rightPane.selectedIndex = -1;
    }


    /// <summary>
    /// Removes all components on a GameObject Except the transform component.
    /// </summary>
    private void RemoveAllComponents()
    {
        //Gets a list of selected objects which are editable
        Object[] obj = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable);
        for (int i = 0; i < obj.Length; ++i)
        {
            //Gets a list of components on each gameObject, and checks that the component is not a Transform, and 
            //that the dependancies the component has is either none, or something other than a Transform.
            //These need to be deleted first
            Component[] components = obj[i].GameObject().GetComponents<Component>().Where(c => c.GetType() != typeof(Transform)).ToArray();
            for (int j = 0; j < components.Length; j++)
            {
                foreach (var attribute in components[j].GetType().GetCustomAttributes(true))
                {
                    if (attribute is RequireComponent)
                    {
                        var requireComponent = (RequireComponent)attribute;

                        if (requireComponent.m_Type0 != null && requireComponent.m_Type0 != typeof(Transform) ||
                            requireComponent.m_Type1 != null && requireComponent.m_Type1 != typeof(Transform) ||
                            requireComponent.m_Type2 != null && requireComponent.m_Type2 != typeof(Transform))
                        {
                            DestroyImmediate(components[j]);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }


            //Now all dependant components are removed, do the same thing again, removing everything but the transform.
            components = obj[i].GameObject().GetComponents<Component>().Where(c => c.GetType() != typeof(Transform)).ToArray();
            foreach (var component in components)
            {
                 DestroyImmediate(component);    
            }
        }
    }

}
