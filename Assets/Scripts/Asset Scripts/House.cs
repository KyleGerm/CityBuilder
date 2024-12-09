using Game.Interfaces;
using UnityEngine;
using Game.Tools;
using Game.Managers;
using Game.Enumerators;
using System.Collections.Generic;

namespace Game
{
    public class House : MonoBehaviour, IHouse, INeedSetUp, IBuildable, IStatsCardObject
    {
        [SerializeField] private int resources, resourceDecayRate, rentPerWeek, rentincrease, maxResourceCapacity, resourceCapIncrease, houseLevel, maxHouseLevel, resourceDecayIncrease, buildCost;
        [SerializeField] private GameObject entityPrefab;
        [SerializeField] private float BuildCostMuiltiplier;
        private IWouldLikeToKnowWhenYouHaveBeenDestroyed Spawner;
        public int depletionCount {  get; private set; }
        private bool houseRanOutOfResources;
        private IPlayerWallet player;
        private GameObject Entity;
        private IHomeOwner homeOwner;
        public int MaxHouseLvl { get => maxHouseLevel; }
        public int HouseLvl { get => houseLevel; }
        public int MaxCapacity { get => maxResourceCapacity; }
        public int Resources { get => resources; }
        public int ResourceDecayRate { get => resourceDecayRate; }
        public int Rent => rentPerWeek;
        public Vector3 Position => transform.position;
        public float BuildCost => buildCost;
        public float BuildMultiplier => BuildCostMuiltiplier;

        private void Start()
        {
            if (gameObject.transform.parent != null) Destroy(this);
        }
        public void AddResources(ref int resource)
        {
            resources += resource;
            resource = 0;
        }
        /// <summary>
        /// Sets up references between this and the entity it created
        /// </summary>
        private void LinkToEntity()
        {
            EntityBehaviour entity = Entity.GetComponent<EntityBehaviour>();
            entity.SetHouse(this);
            homeOwner = entity;
        }
        /// <summary>
        /// Reduces the house resources by the decay rate
        /// </summary>
        private void ResourceDeminish()
        {
            if (resources > resourceDecayRate) resources -= resourceDecayRate;
            else if (resources > 0)
            {
                resources -= resources;
                houseRanOutOfResources = true;
                depletionCount++;
            }
            else if (resources == 0)
            {
                depletionCount++;
            }
        }
        /// <summary>
        /// Asks the homeOwner for payment, and hands debt over if the payment isnt enough
        /// </summary>
        private void RequestWeeksRent()
        {
            int payment = homeOwner.PayRent(rentPerWeek);
            player.AddFunds(payment);
            if (payment < rentPerWeek)
            {
                GameManager.Instance.DebtManager.AddToDebt(rentPerWeek - payment);
            }
        }
        /// <summary>
        /// Checks if the house should be levelled up or down
        /// </summary>
        private void CheckHouseStatus()
        {
            if (!houseRanOutOfResources && homeOwner.Money >= rentPerWeek && houseLevel != maxHouseLevel) LevelHouse(1);
            else if (depletionCount >= 2 && houseLevel > 0) LevelHouse(-1);

            houseRanOutOfResources = false;
            depletionCount = 0;
        }
        /// <summary>
        /// Levels the house up or down by 1.
        /// Will round the given number to the closest out of -1 and 1
        /// </summary>
        /// <param name="level"></param>
        private void LevelHouse(int level)
        {
            level.RoundToClosest(-1, 1);

            houseLevel += 1 * level;
            resourceDecayRate += resourceDecayIncrease * level;
            maxResourceCapacity += resourceCapIncrease * level;
            rentPerWeek += rentincrease * level;
        }

        private void OnDestroy()
        {
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.DAY, false, ResourceDeminish);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.WEEK, false, RequestWeeksRent, CheckHouseStatus);
            homeOwner?.Destroy();
            Spawner?.ItemWasDestroyed(gameObject);
        }

        public void AddToGameSystems()
        {
            Entity = Instantiate(entityPrefab);
            Entity.transform.position = transform.position;
            LinkToEntity();
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.DAY, true, ResourceDeminish);
            //The order of assignment here should mean that rent is taken before the house status is evaluated.
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.WEEK, true, RequestWeeksRent, CheckHouseStatus);
            GameManager.Instance.GetPlayerWalletAs(out player);
        }

        public Dictionary<string, string> AskForValues()
        {
            return new Dictionary<string, string>()
            {
                { "Name",this.ToString()},
                {"Level",houseLevel.ToString()},
                {"MaxLevel",maxHouseLevel.ToString()},
                {"Resources", resources.ToString()},
                {"Resource Capacity", maxResourceCapacity.ToString()},
                {"Home Owner Money", homeOwner.Money.ToString()},
                {"Home Owner Happiness", new string($"{homeOwner.Happiness:n2}%") },
            };
        }

        public override string ToString() => "House";

        public void SetSpawner(IWouldLikeToKnowWhenYouHaveBeenDestroyed spawner)
        {
           Spawner = spawner;
        }
    }
}