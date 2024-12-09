using UnityEngine;
using UnityEngine.EventSystems;
using Game.MapGeneration;
using Game.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game.Managers
{
    //TODO: Fix the events in Input Manager. Make them not static. Add a Generic subscription method 
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private GridSystem gridSystem;
        private Vector3 lastpos;
        private Vector2 screenSize;
        private Vector3 mousePos;
        private Tile focusedTile;
        //Returns true if the cursor is over a button
        public static bool OverButton { get => EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.activeInHierarchy; }

        public delegate void CameraZoom(float amount);
        public delegate void CameraVectors(Vector3 pos);
        public delegate Vector3 CameraInput();
        public delegate void CameraFocus(bool focus);
        public delegate Tile TileRequest(Vector3 pos);

        private event CameraZoom OnZoomChanged;
        private event CameraVectors OnCameraMovement;
        private event CameraInput OnMousePosRequest;
        private event CameraFocus OnFocus;
        private event TileRequest OnTileRequest;
        public Tile FocusedTile { get { return focusedTile; } }
        public bool InMenu => GameManager.Instance.UIManager.BusinessMenuActive();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            screenSize = new Vector2(Screen.width, Screen.height);
        }
        /// <summary>
        /// Returns the last selected cell
        /// </summary>
        /// <returns></returns>
        public Vector3 MousePositionInWorldSpace()
        {
            Vector3? pos = OnMousePosRequest?.Invoke();
            lastpos = pos != null ? (Vector3)pos : lastpos;
            return lastpos;
        }

        private void Update()
        {
            if(GameManager.Instance.GameIsPaused) return;
            CheckForPause();
            mousePos = Input.mousePosition;

            if(MouseIsNotInScreenSpace()) return;

            if (Input.GetMouseButtonDown(0))
            {
                Interact();
            }
            QueryMovement();
            QueryZoom();
        }
        private void CheckForPause()
        {
            if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.PauseGame();
            }
        }
        private bool MouseIsNotInScreenSpace()
        {
            return (mousePos.x < screenSize.x * -0.05f || mousePos.x > screenSize.x * 1.05f
               || mousePos.y < screenSize.y * -0.05f || mousePos.y > screenSize.y * 1.05f);
        }
        /// <summary>
        /// Checks if the camera should be moving, and executes the action
        /// </summary>
        private void QueryMovement()
        {
            if (OverButton) return;
            Vector3 xMovement = Vector3.zero;
            Vector3 zMovement = Vector3.zero;
            if (mousePos.x < screenSize.x * 0.05f || Input.GetKey(KeyCode.LeftArrow))
            {
                xMovement = new Vector3(-movementSpeed, 0, 0);
            }
            else if (mousePos.x > screenSize.x * 0.95f || Input.GetKey(KeyCode.RightArrow))
            {
                xMovement = new Vector3(movementSpeed, 0, 0);
            }

            if (mousePos.y < screenSize.y * 0.05f || Input.GetKey(KeyCode.DownArrow))
            {
                zMovement = new Vector3(0, 0, -movementSpeed);
            }
            else if (mousePos.y > screenSize.y * 0.95f || Input.GetKey(KeyCode.UpArrow))
            {
                zMovement = new Vector3(0, 0, movementSpeed);
            }

            OnCameraMovement?.Invoke(xMovement + zMovement);
        }

        /// <summary>
        /// Checks if the camera should be zoomed, and executes the action
        /// </summary>
        private void QueryZoom()
        {
            if (Input.mouseScrollDelta.y > 0f)
            {
                OnZoomChanged?.Invoke(-zoomSpeed);
            }
            else if (Input.mouseScrollDelta.y < 0f)
            {
                OnZoomChanged?.Invoke(zoomSpeed);
            }
        }

        /// <summary>
        /// Decides what to when the mouse is pressed, and makes the appropriate action.
        /// </summary>
        private void Interact()
        {
            if (OverButton || InMenu) return;
            Vector3 gridPosition = gridSystem.MouseGridPosition;
            gridPosition.y = 0;

            //If already focused on a tile, and we arent placing something down, we want to rotate the object selected.
            if (focusedTile != null)
            {
                bool isActive = false;
                bool includeLvlUpButton = false;
                if (!BuildMenu.PlacingObject)
                {
                    GameObject obj= focusedTile.GetObjectAtPos(gridPosition);
                    if (obj != null)
                    {
                        isActive = true;
                        includeLvlUpButton = obj.TryGetComponent(out Business _);
                    } 
                    GameManager.Instance.UIManager.BusinessMenuActive(isActive, includeLvlUpButton);
                }

                return;
            }

            
            OnFocus?.Invoke(true);
           
            //MouseGridPosition is the position of the Yellow selection grid, which sits just abouve the tiles actual location.
            //All of the calculations for the location of the tile have been done by this point, so all that is left is to make the yPos 0.
            //This will ensure consitency with calculations as well
           
            focusedTile = OnTileRequest?.Invoke(gridPosition);
            GameManager.Instance.UIManager.BuildMenuActive(true);

            if (focusedTile != null)
            {
                gridSystem.SetGrid(focusedTile.GetGrid());
            }
        }
        /// <summary>
        /// Performs a set of actions depending on what part of the menu is currently active.
        /// </summary>
        public void Return()
        {
            if (BuildMenu.PlacingObject) return;
            if (GameManager.Instance.UIManager.BusinessMenuActive())
            {
                if (GameManager.Instance.UIManager.ExpSliderIsEnabled)
                {
                    GameManager.Instance.UIManager.ExpSliderEnabled(false);
                    GameManager.Instance.UIManager.BusinessMenuActive(true, true);
                    return;
                }
                GameManager.Instance.UIManager.BusinessMenuActive(false);
                return;
            }
            OnFocus?.Invoke(false);

            gridSystem.ReturnToWorldGrid();
            focusedTile.RemoveGrid();
            focusedTile = null;
            GameManager.Instance.UIManager.BuildMenuActive(false);
        }

        public void Subscribe(CameraZoom method)
        {
            OnZoomChanged += method;
        }
        public void Subscribe(CameraVectors method)
        {
            OnCameraMovement += method;
        }
        public void Subscribe(CameraInput method)
        {
            OnMousePosRequest += method;
        }
        public void Subscribe(CameraFocus method)
        {
            OnFocus += method;
        }
        public void Subscribe(TileRequest method)
        {
            OnTileRequest += method;
        }

        private void OnDestroy()
        {
            OnZoomChanged = null;
            OnCameraMovement = null;
            OnMousePosRequest = null;
            OnFocus = null;
            OnTileRequest = null;
        }
    }
}
