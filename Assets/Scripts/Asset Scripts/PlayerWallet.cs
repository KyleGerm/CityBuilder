using Game.Delegates;
using Game.Managers;
using Game.Interfaces;
namespace Game
{
    public class PlayerWallet : Singleton<PlayerWallet>, IPlayerWallet, ITaxWallet
    {
        private readonly Wallet wallet = new();
        private readonly Wallet TaxWallet = new();
        private event IntDelegate Update;
        
        public int Money { get => wallet.Money; }

        public int Funds { get=> TaxWallet.Money; }


        void IPlayerWallet.AddFunds(int amount)
        {
            wallet.AddMoney(amount);
            Update?.Invoke(Money);
        }
      
        void ITaxWallet.AddFunds(int amount)
        {
            TaxWallet.AddMoney(amount);
            GameManager.Instance.UIManager.UpdateSliderMaxValue();
        }
      
        void IPlayerWallet.RemoveFunds(int amount)
        {
            //ensures that any accumilated money is taken out of the taxable income value before removing money, since tax is not applied to this wallet. 
            if (wallet.ResetWeeklyCount().RemoveMoney(amount))
            {                              
                Update?.Invoke(Money);
            }
        }
        /// <summary>
        /// Subscribes to the event, and invokes it immediately.
        /// </summary>
        /// <param name="sub"></param>
        public void Subscribe(IntDelegate sub)
        {
            Update += sub;
            Update?.Invoke(Money);
        }
        /// <summary>
        /// Removes delegate from the event
        /// </summary>
        /// <param name="sub"></param>
        public void Unsubscribe(IntDelegate sub)
        {
            Update -= sub;
        }

        void ITaxWallet.RemoveFunds(int amount)
        {
            TaxWallet.ResetWeeklyCount().RemoveMoney(amount);
            GameManager.Instance.UIManager.UpdateSliderMaxValue();
        }

       
    }
}
