using Game.Managers;
using System.Linq;
using TMPro;
using UnityEngine;


namespace Game.UI
{
    public class Date_And_Time : MonoBehaviour
    {
        private TextMeshProUGUI Day;
        private TextMeshProUGUI Time;
        private int day = 0;
        // Start is called before the first frame update
        void Start()
        {
            var objects = transform.GetComponentsInChildren<TextMeshProUGUI>();
            Day = objects.Where(obj => obj.gameObject.name == "Day").First();
            Time = objects.Where(obj => obj.gameObject.name == "Time").First();

            GameManager.Instance.AddToEvent(Game.Enumerators.EntityDelegateSubscriptionType.TICK, true, UpdateOnTick);
        }

        private void UpdateOnTick()
        {
            int time = GameManager.Instance.TimeOfDay;
            Time.text = new string($"Time: {time}:00");

            if (time == 0)
            {
                day++;
                UpdateDay();
            }

        }

        private void UpdateDay()
        {
            Day.text = (day % 7) switch
            {
                0 => new string("Monday"),
                1 => new string("Tuesday"),
                2 => new string("Wednesday"),
                3 => new string("Thursday"),
                4 => new string("Friday"),
                5 => new string("Saturday"),
                6 => new string("Sunday"),
                _ => default
            };
        }

    }
}
