
using UnityEngine;
using Game.Managers;
using Game.Tools;

namespace Game
{
    public class HappiniessCalculator
    {
        private EntityBehaviour entity;

        public HappiniessCalculator(EntityBehaviour entity) => this.entity = entity;


        public float CalculateHappiness(int affordability, int tax, int resource, int houseLevel)
        {
           float a = AffordabilityHappiness() * affordability;
            float b = TaxHappiness() * tax;
            float c = ResourceHappiness() * resource;
            float d = HouseLevelHappiness() * houseLevel;
          //  Debug.Log($"Affordability: {a}\nTax: {b}\nResources: {c}\nHouseLevel: {d}\nTotal{a+b+c+d}");
            return a + b + c + d;
        }
        /// <summary>
        /// How happy is the entity with their wages?
        /// </summary>
        /// <returns></returns>
        private float AffordabilityHappiness()
        {
            //Start with the money they earnt this week
            float wages = entity.Wallet.TaxableMoney;
            //if 0, exit early
            if (wages == 0)
            {
                return 0f;
            }
            //Take away all weekly expenses
              wages -= (entity.house.Rent + (entity.Wallet.TaxableMoney * GameManager.Instance.DebtManager.Tax) + (entity.ItemsToBuy * GameManager.Instance.GetAverageShopPrices()));

            if (wages <= 0) return 0f;
            //return a vale between 0 and 1 
            return  (wages / entity.house.Rent).Limit(1.0f,0f);
        }

        /// <summary>
        /// The more the entity runs out of food in a week, the less happy they will be. 
        /// </summary>
        /// <returns></returns>
        private float ResourceHappiness() => (1.0f - ((1.0f / 7) * entity.house.depletionCount)).LimitLower(0f);

        /// <summary>
        /// How happy the entity is with their current house
        /// </summary>
        /// <returns></returns>
        private float HouseLevelHappiness() => entity.house.HouseLvl / entity.house.MaxHouseLvl;

        /// <summary>
        /// How happy the entity is with tax
        /// </summary>
        /// <returns></returns>
        private float TaxHappiness()
        {
            var debt = GameManager.Instance.DebtManager;
            float debtTax = debt.MaximumTax - debt.MinimumTax;
            //Retutns a value closer to 1, the lower the initial value is 
            return (((debt.MinimumTax - debt.Tax)/debtTax) + 1).Limit(1.0f,0f);
        }  
    }
}
