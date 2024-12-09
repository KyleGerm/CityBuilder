
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapSizeCommunicator : MonoBehaviour
{
    [SerializeField] MapSizeContainer mapSizeContainer;
    [SerializeField] GameObject startButton;
   [SerializeField] private int size;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(PassValue);
    }

    private void PassValue()
    {
        mapSizeContainer.SetMapSize(size);
        startButton.SetActive(true);
    }
}
