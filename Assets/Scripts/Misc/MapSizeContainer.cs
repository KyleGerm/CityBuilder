
using UnityEngine;

   public class MapSizeContainer : MonoBehaviour
{
    public int MapSize {  get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMapSize(int mapSize) => MapSize = mapSize;
}
