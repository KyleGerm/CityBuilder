using Game.Delegates;
using Game.Interfaces;
using Game.Managers;

namespace Game
{
    public class Wallet
    {
        private int taxableMoney, money;
        private event EntityDetails AtWork, AtShop;
        private IWorker owner;
        /// <summary>
        /// Total money in the wallet
        /// </summary>
        public int Money { get { return money; } }
        /// <summary>
        /// Weekly running total of how the money which will be taxed. This should be reset each week after paying tax.
        /// </summary>
        public int TaxableMoney { get { return taxableMoney; } }
        public int Budget => Money - (int)(TaxableMoney * GameManager.Instance.DebtManager.Tax);
        public Wallet(IWorker owner, ref EntityDelegate workEvent, ref EntityDelegate shopEvent)
        {
            this.owner = owner;
            workEvent += ArriveAtWork;
            shopEvent += ArriveAtShop;
        }

        public Wallet() { }

        /// <summary>
        /// Adds an amount to the wallet. 
        /// If the amount is 0 or less, the method will exit early.
        /// </summary>
        /// <param name="payment"></param>
        public void AddMoney(int payment)
        {
            if (payment <= 0) return;
            taxableMoney += payment;
            money += payment;
        }
        /// <summary>
        /// Invokes the AtWork event
        /// </summary>
        private void ArriveAtWork()
        {
            AtWork?.Invoke(owner);
        }
        /// <summary>
        /// Invokes the AtShop event
        /// </summary>
        private void ArriveAtShop()
        {
            AtShop?.Invoke(owner);
        }

        /// <summary>
        /// Checks that funds will not go negative first, and applies the reduction if it can.
        /// Returns the outcome of the attempt.
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public bool RemoveMoney(int cost)
        {
            //If youre left wil less money than will be taxed at the end of the week, the payment cannot happen.
            if (cost > Budget)
            {
                return false;
            }

            money -= cost;
            return true;
        }
        /// <summary>
        /// Subscribes to the Work event
        /// </summary>
        /// <param name="listener"></param>
        public void AddCompanyListener(EntityDetails listener)
        {
            AtWork += listener;
        }
        /// <summary>
        /// Subscribes to the Shop event
        /// </summary>
        /// <param name="listener"></param>
        public void AddShopListener(EntityDetails listener)
        {
            AtShop += listener;
        }
        /// <summary>
        /// Removes listener from the shop event
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveShopListener(EntityDetails listener)
        {
            AtShop -= listener;
        }
        /// <summary>
        /// Removes listener from the work event
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveCompanyListener(EntityDetails listener)
        {
            AtWork -= listener;
        }
        /// <summary>
        /// Resets the taxable money count to zero
        /// </summary>
        public Wallet ResetWeeklyCount()
        {
            taxableMoney = 0;
            return this;
        }
    }
}

