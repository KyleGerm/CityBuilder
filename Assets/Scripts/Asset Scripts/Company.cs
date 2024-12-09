using System.Collections;
using UnityEngine;
using Game.Interfaces;
using Game.Managers;
using System.Collections.Generic;

namespace Game
{
    public class Company : Business, INeedSetUp, IStatsCardObject
    {
        [SerializeField] private int capacity, payIncrease, capacityIncrease;
        private int successNum, ChanceRate;
        private void Start()
        {
            if (gameObject.transform.parent != null) Destroy(this); 
        }
        /// <summary>
        /// Resturns if the application is successful or not
        /// </summary>
        /// <param name="num"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        private bool Apply(int num, out Company company)
        {
            company = this;
            if (entities.Count == capacity) return false;
            InitializeNum();
            return successNum == num;
        }

        /// <summary>
        /// Checks to see if the Chancerate is still the same as the jobSuccessRate.
        /// If not it will regenerate a nember based on the new success value.
        /// </summary>
        private void InitializeNum()
        {
            int num = GameManager.Instance.jobSuccessRate;
            if (ChanceRate == num) return;

            ChanceRate = num;
            successNum = UnityEngine.Random.Range(0, 100 / ChanceRate);
        }
        protected override void AddToList(IEntity entity)
        {
            base.AddToList(entity);
            entity.Wallet.AddCompanyListener(Interact);
        }

        public void QuitWork(IEntity entiy)
        {
            entiy.Wallet.RemoveCompanyListener(Interact);
            entities.Remove(entiy);
        }

        protected override IEnumerator BeginInteraction(IEntity entity)
        {
            yield return new WaitForSeconds(interactionTime / GameManager.Instance.TickSpeed);
            entity.Wallet.AddMoney(interactionValue);
            entity.InteractionFinished();
        }

        protected override Business GetThis(out bool r)
        {
            r = entities.Count < capacity;
            return this as Company;
        }

        protected override void OnDestroy()
        {
            foreach (var entity in entities)
            {
                entity.UnsubscribeFrom(this);
                entity.Wallet.RemoveCompanyListener(Interact);
            }
            GameManager.Instance.AddToEvent(Apply, false);
            entities = null;
            base.OnDestroy();
        }

        protected override void LevelUp(ref int exp)
        {
            capacity += capacityIncrease;
            interactionValue += payIncrease;
            base.LevelUp(ref exp);
        }

        public void AddToGameSystems()
        {
            GameManager.Instance.AddToEvent(Apply);
            InitializeNum();
        }

        public Dictionary<string, string> AskForValues()
        {
            return new Dictionary<string, string>()
            {
                {"Name",this.ToString()},
                {"Level",BusinessLevel.ToString()},
                {"MaxLevel" , MaxLevel.ToString()},
                {"Exp", new string($"{Exp}/{ExpRequiredForLelelUp}")},
                {"Employees", entities.Count.ToString()},
                {"Employee Capacity", capacity.ToString()},
                {"Employee Payment", interactionValue.ToString()}
            };
        }

        public override string ToString() => "Company";
    }
}

