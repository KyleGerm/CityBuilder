using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Delegates;
using Game.Interfaces;
using Game.Managers;
using Game.Enumerators;

namespace Game
{
    public class EntityBehaviour : MonoBehaviour, IWorker, IShopper, IHomeOwner
    {
        [SerializeField] private float speed;
        [SerializeField, Range(0, 100)] private int wageContributionPercentToHappiness;
        [SerializeField,Range(0, 100)] private int resourceContributionPercentToHappiness;
        [SerializeField, Range(0, 100)] private int houseLevelContributionPercentToHappiness;
        [SerializeField, Range(0, 100)] private int taxContributionPercentToHappiness;
        private Wallet wallet;
        private Vector3? HomePos, placeOfWork = null, shopLocation = null;
        private Vector3 firstPos, targetPos;
        private List<Vector3> path = new List<Vector3>();
        private GridSystem system;
        private bool arrived, beenToWork, doingAThing;
        private int shoppingBasket;
        private Coroutine coroutine;
        private HappiniessCalculator happiniess;
        public IHouse house { get; private set; }
        public Wallet Wallet { get => wallet; }
        public int Money { get => wallet.Money; }
        public int ItemsToBuy => house.MaxCapacity - house.Resources;

        public float Happiness { get; private set; }
        private event EntityDelegate AtWork, AtShop;
        private void Start()
        {
            Setup();

        }
        /// <summary>
        /// Sets up any references and subscriptions
        /// </summary>
        private void Setup()
        {
            happiniess = new(this);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.TICK,true, Evaluate);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.DAY, true, ReadyToWork);
            GameManager.Instance.UIManager.AddToEvent(CalculateHappiness);
            GameManager.Instance.DebtManager.SignUpToPayTaxes(PayTaxes);
            system = GameObject.Find("Grid").gameObject.GetComponent<GridSystem>();
            wallet = new(this, ref AtWork, ref AtShop);
            system.GetCellPositionInWorldSpace(transform.position, out Vector3 pos);
            HomePos = pos;
        }

        private float CalculateHappiness()
        {
            float happy = happiniess.CalculateHappiness(wageContributionPercentToHappiness,
                                                          taxContributionPercentToHappiness,
                                                          resourceContributionPercentToHappiness, 
                                                          houseLevelContributionPercentToHappiness);
            //Debug.Log($"I am {happy}% happy");
            return happy;
        }

        /// <summary>
        /// Validates the entities options and takes the most valuable one.
        /// </summary>
        private void Evaluate()
        {
            if (coroutine != null || doingAThing) return;

            if (CanIGoToWork() || CanIGoToTheShop()) return;
            else GoHome();
        }
        /// <summary>
        /// Confirms the entity can go to work, and goes there.
        /// </summary>
        /// <returns></returns>
        private bool CanIGoToWork()
        {
            if (!beenToWork)
            {
                return CompleteAction(in placeOfWork, ApplyForAJob, GoToWork());
            }
            return false;
        }
        /// <summary>
        /// Confirms the entity can go to a shop, and goes there.
        /// </summary>
        /// <returns></returns>
        private bool CanIGoToTheShop()
        {
            if (shoppingBasket >= ItemsToBuy) return false;
            if (house.Resources < house.ResourceDecayRate * 3 && wallet.Budget > 10)
            {
                return CompleteAction(in shopLocation, FindAShop, GoToTheShop());
            }
            return false;
        }
        /// <summary>
        /// Uses the Path Finder to get a new path, and starts the movement routine.
        /// </summary>
        /// <param name="target"></param>
        private void SetTarget(Vector3 target)
        {
            system.GetCellPositionInWorldSpace(transform.position, out firstPos);
            path = GameManager.Instance.PathFinder.FindPath(firstPos, target);

            coroutine = StartCoroutine(MoveToTarget());
        }

        /// <summary>
        /// Uses the path List to move through specified points toward a target. 
        /// When there is no more points in the list, it stops at that position.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveToTarget()
        {
            doingAThing = true;
            arrived = false;
            targetPos = path[0];

            transform.LookAt(targetPos);
            while (transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, GameManager.Instance.TickSpeed * speed * Time.deltaTime);
                yield return null;
            }
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }

            transform.Find("Car").gameObject.SetActive(true);
            transform.Find("Person").gameObject.SetActive(false);
    
            while (path.Count > 0)
            {
                targetPos = path[0];

                transform.LookAt(targetPos);
                while (transform.position != targetPos)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, GameManager.Instance.TickSpeed * speed * Time.deltaTime);
                    yield return null;
                }
                if (path.Count > 0)
                {
                    path.RemoveAt(0);
                }
            }
            Vector3 PositionToGoToOnFoot = WhereAmIGoing();
            transform.LookAt(PositionToGoToOnFoot);
            transform.Find("Car").gameObject.SetActive(false);
            transform.Find("Person").gameObject.SetActive(true);
            while (transform.position != PositionToGoToOnFoot)
            {
                transform.position = Vector3.MoveTowards(transform.position, PositionToGoToOnFoot, GameManager.Instance.TickSpeed * speed * Time.deltaTime);
                yield return null;
            }
            coroutine = null;
            arrived = true;
        }
        /// <summary>
        /// Generates a random number between 0 and the max num defined by the gameManager. 
        /// Sends the number to the gameManager and gets a boolean response.
        /// If true, this will give a new position to the entity for where they should be going to work.
        /// </summary>
        private void ApplyForAJob()
        {
            int applicationNum = Random.Range(0, 100 / GameManager.Instance.jobSuccessRate);
            if (GameManager.Instance.LookForWork(applicationNum, out Vector3 JobPosition, this))
            {
                placeOfWork = JobPosition;
            }
        }
        /// <summary>
        /// Does actions that should happen when at work
        /// </summary>
        private void ArriveAtWork()
        {
            AtWork?.Invoke();
        }
        /// <summary>
        /// Does actions that need to take place when at the shop
        /// </summary>
        private void ArriveAtShop()
        {
            shopLocation = null;
            AtShop?.Invoke();
        }

        /// <summary>
        /// Checks a location for a value, completes an action if not, and begins a coroutine if successful
        /// </summary>
        /// <param name="location">Vector3 value to check. Needs to bo nullable</param>
        /// <param name="action">Void action to perform if location is null</param>
        /// <param name="routine">Coroutine to begin if location is not null. This should directly use the location. Using a coroutine for a different Vector3 is likely to cause NullReferenceException</param>
        private bool CompleteAction(in Vector3? location, EntityDelegate action, IEnumerator routine)
        {
            if (location == null) action();
            if (location == null) return false;
            
             StartCoroutine(routine);
             return true;
        }
        /// <summary>
        /// Looks for a shop and sets the shopLocation
        /// </summary>
        private void FindAShop()
        {
            if (GameManager.Instance.GotToTheShop(out Vector3? ShopPosition, this) && ShopPosition != null)
            {
                shopLocation = ShopPosition;
            }
        }
        /// <summary>
        /// Sets the target to the shop location, then waits to get there, and performs necessary actions
        /// </summary>
        /// <returns></returns>
        private IEnumerator GoToTheShop()
        {
            system.GetCellPositionInWorldSpace((Vector3)shopLocation, out Vector3 pos);
            SetTarget(pos);
            while (!arrived)
            {
                yield return null;
            }

            ArriveAtShop();
        }
        /// <summary>
        /// Sets the target to the Work position and waits to get there, then performs necessary actions
        /// </summary>
        /// <returns></returns>
        private IEnumerator GoToWork()
        {
            system.GetCellPositionInWorldSpace((Vector3)placeOfWork, out Vector3 pos);
            SetTarget(pos);

            while (!arrived)
            {
                yield return null;
            }

            ArriveAtWork();
            BeenToWork();
        }
        /// <summary>
        /// First checks that entity isnt already at home, then begins the going Home coroutine
        /// </summary>
        private void GoHome()
        {
            system.GetCellPositionInWorldSpace(transform.position, out Vector3 pos);
            if ( transform.position == house.Position)
            {
                return;
            }
            StartCoroutine(GoingHome());
        }
        /// <summary>
        /// Sets the target to home, and waits to get there. Then performs any necessary actions
        /// </summary>
        /// <returns></returns>
        private IEnumerator GoingHome()
        {
            if (coroutine != null) yield break;
            SetTarget((Vector3)HomePos);
            while (!arrived)
            {
                yield return null;
            }
            if (shoppingBasket > 0)
            {
                house.AddResources(ref shoppingBasket);
            }
            doingAThing = false;
        }
        public void AddToBasket(int amount)
        {
            if (amount <= 0) return;
            shoppingBasket += amount;
        }

        public void InteractionFinished()
        {
            doingAThing = false;
            Evaluate();
        }

        public void BeenToWork()
        {
            beenToWork = true;
        }
        /// <summary>
        /// Sets the entity to be able to go to work
        /// </summary>
        public void ReadyToWork()
        {
            beenToWork = false;
        }
        /// <summary>
        /// Sets the house which belongs to this entity. Once set, it cannot be unset
        /// </summary>
        /// <param name="house"></param>
        public void SetHouse(IHouse house)
        {
            if (this.house != null) return;
            this.house = house;
        }

        public int PayRent(int amount)
        {
            if (wallet.Budget >= amount)
            {
                wallet.RemoveMoney(amount);
                return amount;
            }
            else
            {
                int payment = wallet.Budget;
                wallet.RemoveMoney(payment);
                return payment;
            }
        }

        /// <summary>
        /// Takes the taxable money from the wallet and hands it to the debt manager to calculate how much is owed, 
        /// Then removes this much money from its wallet, and resets the taxable counter.
        /// </summary>
        public void PayTaxes()
        {  
            float payment = wallet.TaxableMoney;
            GameManager.Instance.DebtManager.PayOffDebt(ref payment);
            wallet.ResetWeeklyCount().RemoveMoney((int)payment);
        }

        public void UnsubscribeFrom(Business business)
        {
            if (business.transform.position == placeOfWork)
            {
                placeOfWork = null;
            }
        }

        public void Destroy()
        {
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.TICK, false, Evaluate);
            GameManager.Instance.AddToEvent(EntityDelegateSubscriptionType.DAY, false, ReadyToWork);
            if (placeOfWork != null)
            {
                system.GetCellPositionInWorldSpace((Vector3)placeOfWork, out Vector3 tilePos);
                GameManager.Instance.FindTileAt(tilePos)?.GetObjectAtPos((Vector3)placeOfWork)?.GetComponent<Company>()?.QuitWork(this);
            }
            //For instances where the engine has destroyed this object before this.Destroy has been called
            try { GameObject.Destroy(gameObject); }
            catch { }
        }

        private Vector3 WhereAmIGoing()
        {
            if (shopLocation != null) return (Vector3)shopLocation;
            if (!beenToWork) return (Vector3)placeOfWork;
            return house.Position;
        }

    }
}