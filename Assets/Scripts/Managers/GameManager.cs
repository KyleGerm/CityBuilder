using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Game.Delegates;
using Game.Pathfinding;
using System.Collections;
using System.Linq;
using Game.Interfaces;
using Game.MapGeneration;
using Game.Enumerators;
using Game.Tools;

namespace Game.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField][Range(1, 100)] private int JobApplicationSuccessPercentage = 50;
        [SerializeField] private WFCGenerator finder;
        [SerializeField] private float DefaultTickRate;
        [SerializeField] private float tickIncreaseRate;
        [SerializeField] private int maxTickIncreaseLevel;
        [SerializeField] private PlayerWallet Player;
        [SerializeField] private float minTax;
        [SerializeField] private float maxTax;
        [SerializeField] private int debtCeiling;
        public delegate void PausedEvent(bool isPaused);
        private event PausedEvent isPaused;
        private event GameManagerDelegate HireAWorker;
        private event BusinessDelegate GoToShop;
        private event EntityDelegate OnTick, OnDay, OnWeek;
        private int ticks, days, ticksPerDay = 24, tickRateLevel = 1;
        private float tickRate;
        private Coroutine TickRoutine;
        private PlayerWallet PlayerWallet;
        private GameObject pauseMenu;
        /// <summary>
        /// Temporary container for a reference to be passed in AskForDetails
        /// </summary>
        private IEntity tempEntity;
        public PathFinder PathFinder { get; private set; }
        public DebtManager DebtManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public float TickSpeed { get => DefaultTickRate / tickRate; }
        public int TimeOfDay { get => ticks % ticksPerDay; }
        public int jobSuccessRate { get => JobApplicationSuccessPercentage; }
        public int ExpToApply => UIManager.SliderValue;

        public bool GameIsPaused { get; private set; }

        public void Awake()
        {
            PlayerWallet = GetComponent<PlayerWallet>();
            Instance = this;
            UIManager = new();
            tickRate = DefaultTickRate;
        }

        private void Start()
        {
            PathFinder = new(finder);
            DebtManager = new(minTax,maxTax, debtCeiling);
            TickRoutine = StartCoroutine(Tick());
            UIManager.BusinessMenuActive(false);
            UIManager.BuildMenuActive(false);
            pauseMenu = GameObject.Find("PauseMenu");
            pauseMenu.SetActive(false);
            InputManager.Instance.Subscribe(UIManager.BuildMenuActive);
        }

        /// <summary>
        /// Manually works through the event list and invokes each one by one for responses.
        /// Chooses a successful application at random and returns its position back to the caller
        /// </summary>
        /// <param name="num"></param>
        /// <param name="pos"></param>
        /// <param name="wallet"></param>
        /// <returns></returns>
        public bool LookForWork(int num, out Vector3 pos, IEntity wallet)
        {
            pos = Vector3.zero;
            tempEntity = wallet;            // Need this to pass the reference to the AskForDetails method
            List<Company> success = new();

            //  var successes = HireAWorker.GetInvocationList().Cast<GameManagerDelegate>().Where(company =>  company.Invoke(num).Equals(true));

            if (HireAWorker == null)
            {
                return false;
            }

            //ForEach statement separates the event list to individual invokes, so it can gather each response to a list.
            //Adds true results to the success list, and chooses one at random to return to caller. 
            //Invokes CompleteApplication for the selected Company to complete its internal verification
            foreach (GameManagerDelegate d in HireAWorker.GetInvocationList())
            {
                bool? indv = d.Invoke(num, out Company company);
                if (indv != null && indv == true)
                    success.Add(company);
            }
            if (success.Count == 0) return false;
            int selection = Random.Range(0, success.Count);

            pos = success.ElementAt(selection).transform.position;
            success[selection].GiveAccessToBusiness();
            //After completeApplication is called, tempWallet is returned to null so that nothing can abuse this field
            tempEntity = null;
            return true;
        }
        /// <summary>
        /// Gets all the shops and returns a random one which can accept the entity
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="wallet"></param>
        /// <returns></returns>
        public bool GotToTheShop(out Vector3? pos, IEntity wallet)
        {
            tempEntity = wallet;
            pos = null;
            if (GoToShop == null) return false;

            var shopList = GoToShop.GetInvocationList()
                                   .Cast<BusinessDelegate>().ToList();
            Shop thing = null;
            while (shopList.Count > 0)
            {
                int selection = Random.Range(0, shopList.Count);
                Shop temp = shopList.ElementAt(selection).Invoke(out bool response) as Shop;
                if (!response) shopList.RemoveAt(selection);
                else
                {
                    thing = temp;
                    break;
                }
            }
            if (thing == null) return false;

            pos = thing.transform.position;
            thing.GiveAccessToBusiness();

            tempEntity = null;

            return true;
        }

        public int GetAverageShopPrices()
        {
            var shopList = GoToShop.GetInvocationList()
                                  .Cast<BusinessDelegate>().ToList();
            int total = 0;

            for (int i = 0; i < shopList.Count; i++)
            {
                Shop shop = shopList.ElementAt(i).Invoke(out bool _) as Shop;
                total += shop.StockPrice;
            }

            return ((float)total/shopList.Count).ToInt(Round.Up);
        }

        /// <summary>
        /// Access Point for getting the entity reference.
        /// If this is called without proper use, the returning value will be null.
        /// </summary>
        public void AskForDetails(EntityDetails entity)
        {
            entity.Invoke(tempEntity);
        }

        /// <summary>
        /// Subscribe or unsubscribe a valid delegate to a GameManager event. If the delegate is not valid, nothing will happen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing"></param>
        public void AddToEvent(EntityDelegateSubscriptionType type, bool beingAdded = true, params EntityDelegate[] thing)
        {
            foreach (EntityDelegate del in thing)
            {
                switch (type)
                {
                    case EntityDelegateSubscriptionType.TICK:
                        if (!beingAdded)
                        {
                            OnTick -= del;
                            continue;
                        }
                        if (OnTick != null && OnTick.GetInvocationList().Contains(del)) continue;
                            
                        OnTick += del;
                        continue;
                        
                    case EntityDelegateSubscriptionType.DAY:
                        if (!beingAdded)
                        {
                            OnDay -= del;
                            continue;
                        }
                        if (OnDay != null && OnDay.GetInvocationList().Contains(del)) continue;
                     
                        OnDay += del;
                        continue;
                    
                    case EntityDelegateSubscriptionType.WEEK:
                        if (!beingAdded)
                        {
                            OnWeek -= del;
                            continue;
                        }
                        if (OnWeek != null && OnWeek.GetInvocationList().Contains(del)) continue;

                        OnWeek += del;
                        continue;
                    default: return;
                }
            }
        }
        /// <summary>
        /// Subscribes or unsubscribes to the Worker event
        /// </summary>
        /// <param name="thing">False if being removed from the event</param>
        public void AddToEvent(GameManagerDelegate thing, bool beingAdded = true)
        {
            if (beingAdded)
            {
                HireAWorker += thing;
                return;
            }
            HireAWorker -= thing;
        }

        /// <summary>
        /// Subscribes or unsubscribes to the shop event
        /// </summary>
        /// <param name="thing">False if being removed from the event</param>
        public void AddToEvent(BusinessDelegate thing, bool beingAdded = true)
        {
            if (beingAdded)
            {
                GoToShop += thing;
                return;
            }
            GoToShop -= thing;
        }
        /// <summary>
        /// Custom timing for actions and events in the game.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Tick()
        {
            while (true)
            {
                yield return new WaitForSeconds(tickRate);
                ticks++;
                OnTick?.Invoke();

                if (ticks % ticksPerDay == 0)
                {
                    days++;
                    OnDay?.Invoke();

                    if (days % 7 == 0)
                    {
                        OnWeek?.Invoke();
                    }
                }
            }
        }
        /// <summary>
        /// Increases the rate in which ticks happen
        /// </summary>
        public void TickRateUP()
        {
            if (tickRateLevel == maxTickIncreaseLevel || Time.timeScale != 1.0f) return;

            tickRate /=  tickIncreaseRate;
            if (tickRate < 0.1f) tickRate = 0.1f;
            tickRateLevel++;
            StopCoroutine(TickRoutine);
            TickRoutine = StartCoroutine(Tick());
            UIManager.UpdateGameSpeed();
        }
        /// <summary>
        /// Decreases the rate in which ticks happen
        /// </summary>
        public void TickRateDOWN()
        {
            if (tickRateLevel == 1 || Time.timeScale != 1.0f) return;
            tickRate *= tickIncreaseRate;
            if (tickRate > DefaultTickRate) tickRate = DefaultTickRate;
            tickRateLevel--;
            UIManager.UpdateGameSpeed();
        }

        /// <summary>
        /// Checks current pause state and switches it to the other one.
        /// </summary>
        public void TogglePause()
        {
            EntityDelegate action = Time.timeScale == 1f ? PauseTicks : ResumeTicks;
            action();
            UIManager.UpdateGameSpeed();
        }
        /// <summary>
        /// Pauses the game
        /// </summary>
        private void PauseTicks()
        {
            Time.timeScale = 0;
            StopCoroutine(TickRoutine);
        }
        /// <summary>
        /// Un-Pauses the game
        /// </summary>
        private void ResumeTicks()
        {
            Time.timeScale = 1;
            TickRoutine = StartCoroutine(Tick());
        }

        public void PauseGame()
        {
            UIManager.BusinessMenuActive(false);
            pauseMenu.SetActive(true);
            isPaused.Invoke(false);
            GameIsPaused = true;
            PauseTicks();
        }

        public void ResumeGame()
        {
            pauseMenu?.SetActive(false);
            isPaused?.Invoke(true);
            GameIsPaused = false;
            ResumeTicks();
        }
        /// <summary>
        /// Returns an interface type of the PlayerWallet.
        /// T must be a type of interface
        /// If T is passed as a class inheriting IPlayerContainer, this will return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void GetPlayerWalletAs<T>(out T wallet) where T : IPlayerContainer
        {
            if (!typeof(T).IsInterface)
            {
                wallet = default;
            }

            IPlayerContainer container = PlayerWallet;
            wallet = (T)container;
        }

        public Tile FindTileAt(Vector3 pos) => finder.GetTile(pos);

        public void ChangeDevCostTarget() => DebtManager.ToggleDevelopmemtTarget();

        public void DisableOnPause(PausedEvent item) => isPaused += item;

        public void QuitGame()
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Ensures only one version of this type will be active at once.
    /// </summary>
    /// <typeparam name="T">Type of class to be singular</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get => instance;
            set
            {
                if (instance != null)
                {
                    Destroy(value);
                    return;
                }

                instance = value;
            }
        }
    }
}


