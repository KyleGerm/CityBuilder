using System.Collections;
using UnityEngine;
using Game.Interfaces;
using Game.Managers;
using Game.Enumerators;
using System.Collections.Generic;


namespace Game
{
    public class Shop : Business, INeedSetUp, IStatsCardObject
    {
        [SerializeField] private int availableStockPerWeek, currentStock, maxStockIncrease;
        public int StockPrice => interactionValue;
        private void Start()
        {
            if (gameObject.transform.parent != null) Destroy(this);   
        }

        protected override IEnumerator BeginInteraction(IEntity entity)
        {
            IShopper shopper = entity as IShopper;
            if (currentStock > 0)
            {
                int cost = CalculateCostofGoods(shopper, shopper.Wallet.Budget, out int goods);
                if (shopper.Wallet.RemoveMoney(cost))
                {
                    shopper.AddToBasket(goods);
                }
                yield return new WaitForSeconds(interactionTime / GameManager.Instance.TickSpeed);
            }
            else
            {
                yield return new WaitForSeconds(2f / GameManager.Instance.TickSpeed);
            }
            shopper.Wallet.RemoveShopListener(Interact);
            entities.Remove(entity);
            shopper.InteractionFinished();
        }

        protected override void AddToList(IEntity entity)
        {
            if (entity is IShopper)
            {
                base.AddToList(entity);
                entity.Wallet.AddShopListener(Interact);
            }
        }
        /// <summary>
        /// Returns how much can be bought, and how much it will cost.
        /// </summary>
        /// <param name="shopper"></param>
        /// <param name="money"></param>
        /// <param name="goods"></param>
        /// <returns></returns>
        private int CalculateCostofGoods(IShopper shopper, int money, out int goods)
        {
            if (interactionValue < 1) interactionValue = 1;
            //This should round down
            goods = money / interactionValue;
            if (goods > shopper.ItemsToBuy) goods = shopper.ItemsToBuy;

            if (goods > currentStock) goods = currentStock;

            currentStock -= goods;
            //so this will not == money exactly
            return goods * interactionValue;
        }
        /// <summary>
        /// Sets stock back to max
        /// </summary>
        private void ReplenishStock()
        {
            currentStock = availableStockPerWeek;
        }
        protected override Business GetThis(out bool r)
        {
            r = currentStock > 0;
            return this as Shop;
        }

        protected override void LevelUp(ref int exp)
        {
            availableStockPerWeek += maxStockIncrease;
            currentStock = availableStockPerWeek;
            base.LevelUp(ref exp);
        }

        protected override void OnDestroy()
        {
            foreach (var entity in entities)
            {
                entity.Wallet.RemoveShopListener(Interact);
            }
            GameManager.Instance.AddToEvent(GetThis, false);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.WEEK,false, ReplenishStock);
            base.OnDestroy();
        }
        public void AddToGameSystems()
        {
            GameManager.Instance.AddToEvent(GetThis);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.WEEK, thing: ReplenishStock);
            currentStock = availableStockPerWeek;
        }

        public Dictionary<string, string> AskForValues()
        {
            return new Dictionary<string, string>()
            {
                {"Name",this.ToString()},
                {"Level",BusinessLevel.ToString()},
                {"MaxLevel" , MaxLevel.ToString()},
                {"Exp", new string($"{Exp}/{ExpRequiredForLelelUp}")},
                {"Stock", currentStock.ToString()},
                {"Stock Capacity", availableStockPerWeek.ToString()},
                {"Cost of Items", interactionValue.ToString()}
            };
        }

        public override string ToString() => "Shop";
    }
}