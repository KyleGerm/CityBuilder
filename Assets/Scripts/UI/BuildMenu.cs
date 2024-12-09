using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Tools;
using Game.Managers;
using Game.Interfaces;
using TMPro;

namespace Game.UI
{
    public class BuildMenu : MonoBehaviour
    {
        [SerializeField] CameraController controller;
        [SerializeField] GridSystem grid;
        [SerializeField] Slider slider;
        [SerializeField] SelectionVisualizer selectionVisualizer;
        [SerializeField] Color buttonColor = new Color(1, 1, 1, 0.18f);
        [SerializeField] TextMeshProUGUI CostText;
        private List<GameObject> buttons = new List<GameObject>();
        private List<GameObject> list;
        private List<Vector3> buttonPositions = new List<Vector3>();
        private float xPos;
        private Coroutine routine;
        private static bool placed = true;
        GameObject currentObject;
        [SerializeField] Button backButton;
        Button.ButtonClickedEvent StolenEvent;
        public static bool PlacingObject { get => !placed; }

        private void Start()
        {
            SetUpBuildMenu();
            HijackTheBackButton();
            CostText.gameObject.SetActive(false);
        }
        /// <summary>
        /// Builds the buttons and places them on the UI. Buttons will be created for each item in the "Prefabs" file
        /// </summary>
        private void SetUpBuildMenu()
        {
            list = Resources.LoadAll<GameObject>("Prefabs").ToList();
            xPos = slider.GetComponent<RectTransform>().rect.xMin;
            slider.maxValue = list.Count * 30;
            foreach (var obj in list)
            {
                GameObject button = CreateNewButton(obj, new Vector3(xPos, 20, 0));
                xPos += button.gameObject.GetComponent<RectTransform>().sizeDelta.x / 3;
                buttons.Add(button);
                buttonPositions.Add(button.transform.localPosition);
            }
            slider.onValueChanged.AddListener(MoveBuildList);
        }

        /// <summary>
        /// Builds a new Button for the UI slider, using the given object and position, and hands it back
        /// </summary>
        /// <param name="obj">The object to be displayed on the button. This will be the Object which is made when the button is pressed.</param>
        /// <param name="position">Position for the button to be placed</param>
        /// <returns></returns>
        private GameObject CreateNewButton(GameObject obj, Vector3 position)
        {
            GameObject button = new GameObject();
            UnityEngine.UI.Image image = button.AddComponent<UnityEngine.UI.Image>(); // Implicitly Adds a CanvasRender Component as well
            UnityEngine.UI.Button clicker = button.AddComponent<UnityEngine.UI.Button>();
            BuildableObject buildable = button.AddComponent<BuildableObject>();
            buildable.SetObj(obj);

            if (!button.TryGetComponent(out RectTransform rect)) rect = button.AddComponent<RectTransform>();

            image.color = buttonColor;
            button.transform.SetParent(slider.transform, false);
            Vector3 scale = slider.transform.localScale;
            Vector3 buttonScale = button.transform.localScale;
            Vector3 newScale = new Vector3(buttonScale.x / scale.x, buttonScale.y / scale.y, buttonScale.z / scale.z);
            button.transform.localScale = newScale;
            if (position.x == slider.GetComponent<RectTransform>().rect.xMin)
            {
                position += new Vector3(rect.sizeDelta.x / 2, 0, 0);
                xPos = position.x;
            }
            button.transform.localPosition = position;
            buildable.SetMenuAs(this);
            GameObject instance = Instantiate(obj, button.transform, false);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, obj.transform.rotation);
            Vector3 NewScale = new Vector3(1 / button.transform.localScale.x, 1 / button.transform.localScale.y, 1 / button.transform.localScale.z);

            instance.transform.localScale = instance.transform.localScale.MultiplyBy(NewScale);
            clicker.onClick.AddListener(buildable.BuildNewObject);

            return button;
        }

        /// <summary>
        /// Allows a dynamic placement of an object while not being in the update loop.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private IEnumerator PlaceObject(GameObject obj, BuildableObject handler)
        {
            currentObject = obj;
            placed = false;
            GameManager.Instance.GetPlayerWalletAs(out IPlayerWallet player);
            while (true)
            {
                obj.transform.position = grid.MouseGridPosition;
                bool legal = grid.LegalPosition();
                selectionVisualizer.IsValidSelection(legal);
                if (Input.GetMouseButtonDown(0) && !InputManager.OverButton && legal && player.Money >= handler.BuildCost)
                {
                    if (grid.HandOverObject(obj))
                    {
                        player.RemoveFunds(handler.BuildCost);
                        handler.CreateNewInstance(out obj).IncreaseBuildCost();
                        CostText.text = new string($"Cost: {handler.BuildCost}");
                        currentObject = obj;
                    }
                }
                yield return null;
            }
        }
        /// <summary>
        /// Moves the buttons by the amount given on the X axis
        /// </summary>
        /// <param name="amount"></param>
        private void MoveBuildList(float amount)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].transform.localPosition = buttonPositions[i] + new Vector3(-amount, 0, 0);
            }
        }
        /// <summary>
        /// Checks if it should allow the coroutine to start, and begins it.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool BeginPlacementRoutine(GameObject obj, BuildableObject handler)
        {
            if (routine != null && InputManager.OverButton) return false;

            if (routine == null)
            {
                CostText.text = new string($"Cost: {handler.BuildCost}");
                CostText.gameObject.SetActive(true);
                routine = StartCoroutine(PlaceObject(obj,handler));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Stops placing objects, and destroys the one currently being positioned
        /// </summary>
        public void EndPlacementRoutine()
        {
            StopCoroutine(routine);
            Destroy(currentObject);
            CostText.gameObject.SetActive(false);
            currentObject = null;
            placed = true;
            routine = null;
        }

        /// <summary>
        /// Performs an action when the backButton is pressed
        /// </summary>
        private void ButtonWasPressed()
        {
            if (currentObject == null) { StolenEvent.Invoke();}
            else
            {
               EndPlacementRoutine();
                selectionVisualizer.Reset();
            }
        }

        /// <summary>
        /// The only way I could think of adding this function to the back button without introducing a dependancy. 
        /// </summary>
        private void HijackTheBackButton()
        {
            StolenEvent = backButton.onClick;

            backButton.onClick = new Button.ButtonClickedEvent();
            backButton.onClick.AddListener(ButtonWasPressed);
        }
    }
}