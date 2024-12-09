using Game.Delegates;
using Game.Enumerators;
using Game.Interfaces;
using Game.Tools;
using UnityEngine;

namespace Game.Managers
{
    public class DebtManager
    {
        private float debt, tax = 0.1f;
        private float minimumTax;
        private float debtCeiling;
        private ITaxWallet player;
        private event EntityDelegate PayTaxes;
        private event IntDelegate UpdateDebtWallet;
        private event FloatDelegate TaxUpdate;
        private bool devCostPaysOffDebt;

        public float Tax { get => tax; }
        public float Debt { get => debt; }

        public float MinimumTax {  get => minimumTax; }
        public float MaximumTax { get; private set; }

        public DebtManager(float baselineTax, float maxTax, int debtCeiling)
        {
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.WEEK, true, ReviewTax,CollectTax);
            GameManager.Instance.GetPlayerWalletAs(out player);
            this.tax = baselineTax;
            this.minimumTax = baselineTax;
            this.MaximumTax = maxTax;
            this.debtCeiling = debtCeiling;
            ReviewTax();
        }
        /// <summary>
        /// Takes in a value to modify. The returned value will be the remainder, after any debts are paid, like change. 
        /// </summary>
        /// <param name="amount"></param>
        public void PayOffDebt(ref float amount)
        {
            float TotalPayment = amount * tax; //work out how much will be taken in tax 
            float devcost = (amount * 0.1f);
            float debtPayment = TotalPayment - devcost;
            if (debtPayment > debt) //check if the payment is more than the current debt
            {
                debtPayment = debt;
            }
            else  if(devCostPaysOffDebt) WorkItOut(ref devcost, ref debtPayment);

            TotalPayment = devcost + debtPayment;
            UpdateDebtWallet?.Invoke((debt -= debtPayment).ToInt(Round.Up));
            player.AddFunds(devcost.ToInt(Round.Down));
            amount = TotalPayment;
           // Debug.Log($"DebtPayment: {debtPayment} Dev Cost: {devcost} Total: {TotalPayment}");
        }
        /// <summary>
        /// Checks what the value of tax should be.
        /// </summary>
        private void ReviewTax()
        {
            float intermediate = debt/debtCeiling < 1.0f? debt/debtCeiling : 1.0f;
            intermediate *= 0.3f;

            tax = minimumTax + intermediate;

            if (tax > MaximumTax) tax = MaximumTax;
            TaxUpdate?.Invoke(tax);
        }
        /// <summary>
        /// Adds the amount to the debt
        /// </summary>
        /// <param name="amount"></param>
        public void AddToDebt(float amount)
        {
            debt += amount;
            UpdateDebtWallet?.Invoke(debt.ToInt(Round.Up));
        }
        /// <summary>
        /// Subscribes to the PayTaxes event
        /// </summary>
        /// <param name="taxForm"></param>
        public void SignUpToPayTaxes(EntityDelegate taxForm)
        {
            PayTaxes += taxForm;
        }
        /// <summary>
        /// Invokes the PayTaxes event
        /// </summary>
        private void CollectTax()
        {
            PayTaxes?.Invoke();
        }

        public void GetUpdatesForDebtChanges(IntDelegate action)
        {
            UpdateDebtWallet += action;
            UpdateDebtWallet?.Invoke(debt.ToInt(Round.Up));
        }

        public void GetUpdatesOnTaxReview(FloatDelegate action)
        {
            TaxUpdate += action;
            TaxUpdate?.Invoke(tax);
        }

        private void WorkItOut(ref float devCost, ref float debtPayment)
        {
            float debt = this.debt;
            debt -= debtPayment;

            if(debt > devCost)
            {
                debtPayment += devCost;
                devCost = 0; 
            }
            else
            {
                devCost -= debt;
                debtPayment += debt;
            }
        }

        public void ToggleDevelopmemtTarget()
        {
            devCostPaysOffDebt = !devCostPaysOffDebt;
        }
    }
}
