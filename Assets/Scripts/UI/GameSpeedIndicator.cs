using TMPro;
using UnityEngine;
using Game.Managers;

namespace Game.UI
{
    public class GameSpeedIndicator
    {
        private TextMeshProUGUI text;
        public GameSpeedIndicator()
        {
            text = GameObject.Find("GameSpd").GetComponent<TextMeshProUGUI>();
        }
        /// <summary>
        /// Updates the value of the tick speed in the game.
        /// This should be updated each time the speed changes
        /// </summary>
        public void UpdateText()
        {
            text.text = Time.timeScale == 1.0f ? new string($"x{GameManager.Instance.TickSpeed:n2}") : new string("PAUSED");
        }
    }
}