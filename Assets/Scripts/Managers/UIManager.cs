using Game.UI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.Managers
{
    public class UIManager
    {
        private GameObject canvas;
        private GameObject BusinessMenu;
        private StatsCard StatsPanel;
        private ExpSlider_Controller slider;
        private GameSpeedIndicator gameSpeedIncator;
        private ToggleButtons toggle;

        private TextMeshProUGUI cityHappy;

        public delegate float Happiness();
        private event Happiness happinessRoundUp;


        public int SliderValue => slider.GetSliderValue();
        public bool ExpSliderIsEnabled => slider.isActive;
        public UIManager()
        {
            string[] ObjectNames = { "Build Menu", "Slider" };
            canvas = GameObject.Find(ObjectNames[1]);
            toggle = GameObject.Find(ObjectNames[0]).GetComponent<ToggleButtons>();
            BusinessMenu = GameObject.Find("BuildingMenu");
            slider = BusinessMenu.transform.Find("ExpValueSlider").GetComponent<ExpSlider_Controller>();
            gameSpeedIncator = new GameSpeedIndicator();
            cityHappy = GameObject.Find("CityHappiness").gameObject.GetComponent<TextMeshProUGUI>();
            StatsPanel = GameObject.Find("StatCard").gameObject.GetComponent<StatsCard>();
            GameManager.Instance.AddToEvent(Enumerators.EntityDelegateSubscriptionType.WEEK, true, UpdateHappiness);
        }
        /// <summary>
        /// Turns BuildMenu on or off
        /// </summary>
        /// <param name="isActive">Is the menu meant to be active?</param>
        public void BuildMenuActive(bool isActive)
        {
            canvas.SetActive(isActive);
            if (isActive || BusinessMenuActive())
            {
                toggle.TurnOnBackButton();
                return;
            }
            toggle.TurnOnTaxButton();
            return ;
        }

        public UIManager BusinessMenuActive(bool isActive, bool includeLvlUpButton = false, bool standardMenuActive = true)
        {
            BusinessMenu.SetActive(isActive);
            StatsPanel.gameObject.SetActive(isActive);
            BuildMenuActive(!isActive);
            if (!isActive) return this;
            BusinessMenu.transform.Find("Remove").gameObject.SetActive(standardMenuActive);
            BusinessMenu.transform.Find("RotateButton").gameObject.SetActive(standardMenuActive);
            BusinessMenu.transform.Find("LevelUpButton").gameObject.SetActive(includeLvlUpButton);
            return this;
        }

        private void UpdateHappiness()
        {
            float total = 0;
            //cast the event into its list of delegates, then invoke each, and add the result to the total.
            happinessRoundUp.GetInvocationList()
                            .Cast<Happiness>()
                            .ToList()
                            .ForEach(x => total += x.Invoke());

            total /= happinessRoundUp.GetInvocationList().Count();

            cityHappy.text = new string($"City Happiness: {total:n2}%");
        }

        public void AddToEvent(Happiness action)
        {
            happinessRoundUp += action;
        }

        public bool BusinessMenuActive() => BusinessMenu.activeInHierarchy;
        public UIManager ExpSliderEnabled(bool enabled)
        {
            slider.Enable(enabled);
            return this;
        }

        public UIManager ForceSliderUpdate()
        {
            slider.ManualUpdate();
            return this;
        }
        

        public UIManager UpdateSliderMaxValue() 
        {
            slider.UpdateMaxValue();
            return this;
        }


        public UIManager UpdateGameSpeed()
        {
            gameSpeedIncator.UpdateText();
            return this;
        }

        public UIManager RefreshStatsPanel() 
        {
            StatsPanel.GetSelectedObjectAndDisplayValues();
            return this;
        }
    }
}
