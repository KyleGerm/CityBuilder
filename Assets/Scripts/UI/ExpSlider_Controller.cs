using Game.Interfaces;
using Game.Managers;
using Game.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{

    public class ExpSlider_Controller : MonoBehaviour
    {
        private TextMeshProUGUI ExpText;
        private Slider slider;
        private ITaxWallet player;
        public int ExpValue { get => slider.value.ToInt(Round.Down); }
        public bool isActive { get => gameObject.activeInHierarchy; }
        void Awake()
        {
            ExpText = transform.Find("ExpValueVisualizer").GetComponent<TextMeshProUGUI>();
            GameManager.Instance.GetPlayerWalletAs(out player);
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(UpdateTextInfo);
            slider.wholeNumbers = true;
            UpdateTextInfo(slider.value);

        }
        private void Start()
        {
          gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            if (player != null)
                UpdateMaxValue();
        }

        private void UpdateTextInfo(float value)
        {
            ExpText.text = new string($"{value} / {slider.maxValue}");
        }
        public int GetSliderValue() => slider.value.ToInt(Round.Down);

        public void Enable(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        public void ManualUpdate()
        {
            slider.value = 0;
            UpdateMaxValue();
            UpdateTextInfo(GetSliderValue());
        }

        public void UpdateMaxValue()
        {
            if (!slider.gameObject.activeInHierarchy) return;
            slider.maxValue = player.Funds;
            UpdateTextInfo(GetSliderValue());
        }
    }
}
