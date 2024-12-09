using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Managers;
using Game.Interfaces;
using TMPro;
using System.Linq;

public class StatsCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] List<TextMeshProUGUI> info;
    [SerializeField] GridSystem grid;
    [SerializeField] InputManager inputManager;
    RectTransform rect;
    private Vector2 dimensions;
    private void Awake()
    {
       rect = GetComponent<RectTransform>();
        dimensions = rect.anchorMax - rect.anchorMin;
    }
    private void OnEnable()
    {
        GetSelectedObjectAndDisplayValues();
    }

    private void Update()
    {
        if(MouseCursorOverCard() && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(MoveWithMouse());
        }

        //Refreshes the card if another object is selected while the card is still active
       else if (Input.GetMouseButtonDown(0))
        {
            GetSelectedObjectAndDisplayValues();
        }
    }

    /// <summary>
    /// Finds the object at the selected grid, gets the stat card information, and displays it as it was given
    /// </summary>
    public void GetSelectedObjectAndDisplayValues()
    {
 
        GameObject obj = inputManager.FocusedTile?.GetObjectAtPos(grid.MouseGridPosition);
        if (obj == null ||!obj.TryGetComponent(out IStatsCardObject stats))
        {
            gameObject.SetActive(false);
            return;
        }

        var values = stats.AskForValues();
        Title.text = new string($"{values["Name"]}");
        Title.gameObject.SetActive(true);
        values.Remove("Name");

       for(int i = 0; i < info.Count; i++)
        {
            if (i == values.Count()) break;

            var item = values.ElementAt(i);
            info[i].text = new string($"{item.Key}: {item.Value}");
            info[i].gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// Checks that the mouse position is within the rect bounds
    /// </summary>
    /// <returns></returns>
    private bool MouseCursorOverCard()
    {
        var pos = ScreenSpaceMousePos();
        return (pos.x >= rect.anchorMin.x && pos.x <= rect.anchorMax.x && pos.y >= rect.anchorMin.y && pos.y <= rect.anchorMax.y) ;
    }

    /// <summary>
    /// Calculates a 0 to 1 representation of the mouses position on the screen.
    /// Must be used for calculating relative position to the anchor points
    /// </summary>
    /// <returns></returns>
    private Vector2 ScreenSpaceMousePos()
    {
        float x = Input.mousePosition.x / Screen.width;
        float y = Input.mousePosition.y / Screen.height;
        return new Vector2(x, y);
    }
    /// <summary>
    /// Moves the card relative to the mouse position
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveWithMouse()
    {
        Vector2 MousePos = ScreenSpaceMousePos();
        Vector2 minXOffset = rect.anchorMin - MousePos;
        Vector2 MaxYOffset = rect.anchorMax - MousePos;
        while (Input.GetMouseButton(0))
        {
                Vector2 updatedMousePos = ScreenSpaceMousePos();
                rect.anchorMin = updatedMousePos + minXOffset;
                rect.anchorMax = updatedMousePos + MaxYOffset;
            CheckThatCardStaysOnScreen();
            yield return null;
        }
    }

    /// <summary>
    /// Ensures all bounds of the card stay onscreen
    /// </summary>
    private void CheckThatCardStaysOnScreen()
    {
        if (rect.anchorMin.x < 0)
        {
            rect.anchorMin = new Vector2(0, rect.anchorMin.y);
            rect.anchorMax = rect.anchorMin + dimensions;
        }
        else if (rect.anchorMax.x > 1)
        {
            rect.anchorMax = new Vector2(1, rect.anchorMax.y);
            rect.anchorMin = rect.anchorMax - dimensions;
        }

        if (rect.anchorMin.y < 0)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0);
            rect.anchorMax = rect.anchorMin + dimensions;
        }
       else if (rect.anchorMax.y > 1)
        {
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1);
            rect.anchorMin = rect.anchorMax - dimensions;
        }
    }
    
}
