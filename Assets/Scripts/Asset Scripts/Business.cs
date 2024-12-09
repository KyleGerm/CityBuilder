using Game.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Managers;
using Game.Tools;

namespace Game
{
    public abstract class Business : MonoBehaviour, IBusiness, IBuildable
    {
        //These need to be serialized to be accessed in custom inspector, but because base.OnInspectorGUI is called,
        //any serialized fields in this class should be hidden so they are not duplicated in the inspector.

        [SerializeField, HideInInspector] protected float interactionTime;
        [SerializeField, HideInInspector] protected int interactionValue;
        [SerializeField] protected int Exp;
        [SerializeField] protected float ExpRequiredForLelelUp;
        [SerializeField] protected int BusinessLevel = 0;
        [SerializeField] protected int MaxLevel;
        [SerializeField] protected float ExpRequirementIncreaseMultiplier;
        [SerializeField] protected int buildCost;
        [SerializeField] protected float BuildCostMultiplier;
        protected IWouldLikeToKnowWhenYouHaveBeenDestroyed Spawner;

        protected List<IEntity> entities = new List<IEntity>();

        public float BuildCost => buildCost;

        public float BuildMultiplier => BuildCostMultiplier;

        /// <summary>
        /// How this class interacts with an entity.
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void Interact(IEntity entity)
        {
            if (entity == null) return;
            StartCoroutine(BeginInteraction(entity));
        }
        /// <summary>
        /// Interact with an entity and wait for a response
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract IEnumerator BeginInteraction(IEntity entity);

        /// <summary>
        /// Adds the entity to the entityList
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void AddToList(IEntity entity)
        {
            if (entities.Contains(entity)) return;
            entities.Add(entity);
        }
        /// <summary>
        /// Wrapper for the AddToList method. Ensures the correct usage of the method.
        /// </summary>
        public void GiveAccessToBusiness()
        {
            GameManager.Instance.AskForDetails(AddToList);
        }
        /// <summary>
        /// Returns this class, and a boolean response based on a criterea
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected abstract Business GetThis(out bool response);

        public void AddEXP()
        {
            if (BusinessLevel == MaxLevel) return;
            GameManager.Instance.GetPlayerWalletAs(out ITaxWallet player);
            if (player == null)
            {
                return;
            }
            int exp = GameManager.Instance.ExpToApply;
            Exp += exp;
            int tempExpValue = exp;
            CheckExp(ref exp);
            player.RemoveFunds(tempExpValue - exp);
        }

        protected void CheckExp(ref int exp)
        {
            if (BusinessLevel == MaxLevel)
            {
                Exp = 0;
                return;
            }
            if (Exp >= ExpRequiredForLelelUp)
            {
                LevelUp(ref exp);
            }
            else
            {
                exp = 0;
            }
        }

        protected virtual void LevelUp(ref int exp)
        {
            BusinessLevel++;
            Exp -= ExpRequiredForLelelUp.ToInt(Round.Down);
            exp -= ExpRequiredForLelelUp.ToInt(Round.Down);
           ExpRequiredForLelelUp *= ExpRequirementIncreaseMultiplier;
            CheckExp(ref exp);
        }

        public void SetSpawner(IWouldLikeToKnowWhenYouHaveBeenDestroyed spawner)
        {
            Spawner = spawner;
        }

        protected virtual void OnDestroy()
        { 
            Spawner?.ItemWasDestroyed(gameObject);
        }
    }
}
