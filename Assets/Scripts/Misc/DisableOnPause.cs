
using UnityEngine;
using Game.Managers;
using UnityEngine.UI;

public class DisableOnPause : MonoBehaviour
{
    [SerializeField] Button button;
    private bool active = true;
    void Start()
    {
        if (button == null)
        {
            GameManager.Instance.DisableOnPause(gameObject.SetActive);
        }
        else
        {
            button.onClick.AddListener(ToggleActive);
        }
    }

    private void ToggleActive()
    {
        active = !active;
        gameObject.SetActive(active);
    }
}