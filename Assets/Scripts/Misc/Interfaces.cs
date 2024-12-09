

using System.Collections.Generic;
using UnityEngine;

namespace Game.Interfaces
{
    /// <summary>
    /// Contains properties every entity should have 
    /// </summary>
    public interface IEntity
    {
        Wallet Wallet { get; }
        /// <summary>
        /// Invokes any events and actions involved with the entity's next action
        /// </summary>
        void InteractionFinished();

        void UnsubscribeFrom(Business business);
    }
    /// <summary>
    /// Contains actions associated with the working entity
    /// </summary>
    public interface IWorker : IEntity
    {  
        /// <summary>
       /// Tells the entity is has been to work 
       /// </summary>
        void BeenToWork();
    }
    /// <summary>
    /// Contains actions associated with the shopping entity
    /// </summary>
    public interface IShopper : IEntity
    {
        /// <summary>
        /// Adds amount to the shopping basket
        /// </summary>
        /// <param name="amount"></param>
        void AddToBasket(int amount);
        int ItemsToBuy {  get; }
    }
    /// <summary>
    /// Actions for entites who have a house
    /// </summary>
    public interface IHomeOwner
    {
        /// <summary>
        /// Pays the amount specified. Used by the owners house
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        int PayRent(int amount);
        int Money {  get; }
        void Destroy();
        float Happiness {  get; }
    }
    /// <summary>
    /// Methods to use on the Player's TaxWallet
    /// </summary>
    public interface ITaxWallet : IPlayerContainer
    {
        int Funds { get; }
        /// <summary>
        /// Passes a value into the TaxWallet
        /// </summary>
        /// <param name="amount"></param>
        void AddFunds(int amount);
        /// <summary>
        /// Removes money from the tax wallet
        /// </summary>
        /// <param name="amount"></param>
        void RemoveFunds(int amount);
    }
    /// <summary>
    /// Methods to use on the Player wallet
    /// </summary>
    public interface IPlayerWallet : IPlayerContainer
    {
        int Money { get; }
        /// <summary>
        /// Passes a value to the wallet, and updates the UI
        /// </summary>
        /// <param name="amount"></param>
        void AddFunds(int amount);
        /// <summary>
        /// Modifies how money is removed from the wallet.
        /// </summary>
        /// <param name="amount"></param>
        void RemoveFunds(int amount);
    }

    public interface IBusiness
    {
        void AddEXP();
        
    }
    /// <summary>
    /// Used for grouping specific interfaces under this identity. Should not be inherited directly
    /// </summary>
    public interface IPlayerContainer { }

    public interface INeedSetUp
    {
        /// <summary>
        /// Performs setup actions needed such as subscriptions.
        /// </summary>
        void AddToGameSystems();
    }

    public interface IBuildable
    {
        float BuildCost { get; }
        float BuildMultiplier { get; }

        /// <summary>
        /// Syncs the values given with the build cost and multiplier values for this class
        /// </summary>
        /// <param name="buildCost"></param>
        /// <param name="buildMulti"></param>
        public void SyncValues(ref float buildCost, ref float buildMulti)
        {
            buildCost = BuildCost;
            buildMulti = BuildMultiplier;
        }

        void SetSpawner(IWouldLikeToKnowWhenYouHaveBeenDestroyed spawner);
    }

    public interface IWouldLikeToKnowWhenYouHaveBeenDestroyed
    {
        void ItemWasDestroyed(GameObject item);
    }
    /// <summary>
    /// Accessible proerties contained with the house class
    /// </summary>
    public interface IHouse
    {
        Vector3 Position { get; }
        int MaxCapacity { get; }
        int Resources { get; }
        int ResourceDecayRate { get; }
        int Rent { get; }
        int depletionCount { get; }

        int HouseLvl { get; }
        int MaxHouseLvl { get; }
        /// <summary>
        /// Takes the value from amount and adds it to the house resources. Hands 0 back
        /// </summary>
        /// <param name="amount"></param>
        void AddResources(ref int amount);
    }

    public interface IStatsCardObject
    {
        Dictionary<string, string> AskForValues();
    }
}