using Game.Managers;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ShowPlayerMoneyOnUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI PlayerWallet;
        [SerializeField] private TextMeshProUGUI DebtWallet;
        [SerializeField] private TextMeshProUGUI Tax;
        [SerializeField] private PlayerWallet player;
        // Start is called before the first frame update
        void Start()
        {
            player.Subscribe(UpdateMoneyWallet);
            GameManager.Instance.DebtManager.GetUpdatesForDebtChanges(UpdateDebt);
            GameManager.Instance.DebtManager.GetUpdatesOnTaxReview(UpdateTax);
        }
        /// <summary>
        /// Updates the money value on the player UI. Updates each time the value is changed.
        /// </summary>
        /// <param name="value"></param>
        private void UpdateMoneyWallet(int value)
        {
            PlayerWallet.text = new string($"Money: {value}");
        }

        private void UpdateDebt(int value)
        {
            DebtWallet.text = new string($"Debt: {value}");
        }

        private void UpdateTax(float value)
        {
            Tax.text = new string($"Tax: {value*100:n2}%");
        }
    }
}