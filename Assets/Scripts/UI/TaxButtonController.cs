using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TaxButtonController : MonoBehaviour
    {

        private Button button;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;
        private bool selected = false;
        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(ToggleButton);
        }

        private void ToggleButton()
        {
            ToggleSelected();
            if (selected)
            {
                button.targetGraphic.color = activeColor;
            }
            else
            {
                button.targetGraphic.color = inactiveColor;
            }
        }

        private void ToggleSelected() => selected = !selected;
    }
}
