using UnityEngine;

namespace Game.UI
{
    public class SelectionVisualizer : MonoBehaviour
    {
        [SerializeField] Color invalidSelectionColor;
        private Renderer _shader;
        private Color defaultColor;
        private string color = "_Color";
        private void Start()
        {
            _shader = GetComponent<Renderer>();
            defaultColor = _shader.material.color;
        }
        /// <summary>
        /// Sets the material color based on true or false values
        /// </summary>
        /// <param name="isValid"></param>
        public void IsValidSelection(bool isValid)
        {
            if (isValid)
            {
                _shader.material.SetColor(color, defaultColor);
            }
            else
            {
                _shader.material.SetColor(color, invalidSelectionColor);
            }
        }

        public void Reset()
        {
            _shader?.material.SetColor(color, defaultColor);
        }
    }
}
