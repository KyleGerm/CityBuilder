using UnityEngine;
using Game.Managers;
namespace Game.UI
{

    public class ToggleButtons : MonoBehaviour
    {

        private GameObject BackButton;
        private GameObject TaxButton;
        // Start is called before the first frame update
        void Awake()
        {
            BackButton = transform.Find("BackButton").gameObject;
            TaxButton = transform.Find("Tax-DebtPayoffButton").gameObject;
        }

        public void TurnOffBothButtons()
        {
            Buttons(false);
        }

        public void TurnOnBackButton()
        {
            Buttons(true, true, false);
        }

        public void TurnOnTaxButton()
        {
            Buttons(true, false, true);
        }

        /// <summary>
        /// Controls the parent, and all children individually
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="backBttn"></param>
        /// <param name="taxBttn"></param>
        private void Buttons(bool parent, bool? backBttn = null, bool? taxBttn = null)
        {
            gameObject.SetActive(parent);
            BackButton.SetActive(backBttn == null ? parent : (bool)backBttn);
            TaxButton.SetActive(taxBttn == null ? parent : (bool)taxBttn);
        }

    }
}
