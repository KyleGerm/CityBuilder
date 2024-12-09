using Game.Tools;
using Game.MapGeneration;
using Game.Enumerators;
using Game.Interfaces;
using UnityEngine;
using Unity.VisualScripting;

namespace Game.Managers
{
    public class GridSystem : MonoBehaviour
    {
        [SerializeField] private GameObject mouseIndicator, gridObject;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private Grid defaultGrid;
        [SerializeField] private Material worldGrid, localGrid, worldSelector, localSelector;
        private Vector3 defaultGridSize, defaultGridPosition, offset;
        private bool isFocused;
        private Grid grid;
        public Vector3 MouseGridPosition { get => mouseIndicator.transform.position; }
        public Vector3 Offsett => offset;

        private void Start()
        {
            grid = defaultGrid;
        }

        private void Update()
        {
            if(GameManager.Instance.GameIsPaused) return;

            Vector3 MousePositionInCellSpace = CentreOfSelectedCell();
            if (NotInTheSelectedGridSpace()) return;

            else if (InAMenuAndOverAnObject(MousePositionInCellSpace)) 
            {
                if (Input.GetMouseButton(0))
                {
                    mouseIndicator.transform.position = MousePositionInCellSpace;
                }
                return;
            }
            else if (inputManager.InMenu) return;

            mouseIndicator.transform.position = MousePositionInCellSpace;
        }

        private bool NotInTheSelectedGridSpace() => isFocused && !InGridBounds();

        private bool InAMenuAndOverAnObject(Vector3 currentPosition) => inputManager.InMenu && inputManager.FocusedTile != null && inputManager.FocusedTile.GetObjectAtPos(currentPosition) != null;

        /// <summary>
        /// Checks the distance between current grid pos and center of the grid. 
        /// Like a Vector2.Distance check, but for a square
        /// </summary>
        /// <returns></returns>
        private bool InGridBounds()
        {
            Vector3 currentPos = inputManager.MousePositionInWorldSpace();
            return (Mathf.Abs(currentPos.x - gridObject.transform.position.x) < (10 / grid.cellSize.x) / 2 &&
                    Mathf.Abs(currentPos.z - gridObject.transform.position.z) < (10 / grid.cellSize.x) / 2);
        }
        public Vector3 CentreOfSelectedCell() => grid.CellToWorld(MousePos()) + offset;



        /// <summary>
        /// Sets the default grids size
        /// </summary>
        /// <param name="size"></param>
        public void SetGridSize(float size)
        {
            float pos = size / 2 * 10;
            defaultGridPosition = new Vector3(pos - 5, 0.1f, pos - 5);
            defaultGridSize = new Vector3(size, 0, size);
            offset = grid != null ? new Vector3(grid.cellSize.x / 2, 0.1f, grid.cellSize.z / 2) : Vector3.zero;
            gridObject.transform.localScale = defaultGridSize;
            gridObject.transform.position = defaultGridPosition;
        }

        /// <summary>
        /// Sets focus on the given grid
        /// </summary>
        /// <param name="grid"></param>
        public void SetGrid(Grid grid)
        {
            isFocused = !isFocused;
            this.grid = grid;
            offset = new Vector3(grid.cellSize.x / 2, 0.1f, grid.cellSize.z / 2);
            gridObject.GetComponent<Renderer>().material = localGrid;
            gridObject.transform.localScale = grid.gameObject.transform.localScale;
            gridObject.transform.position = new Vector3(grid.gameObject.transform.parent.position.x, defaultGridPosition.y, grid.gameObject.transform.parent.position.z);
            mouseIndicator.transform.GetChild(0).GetComponent<Renderer>().material = localSelector;
            mouseIndicator.transform.localScale = gridObject.transform.localScale / 9;
        }

        /// <summary>
        /// Returns focus to the default grid
        /// </summary>
        public void ReturnToWorldGrid()
        {
            isFocused = !isFocused;
            grid = defaultGrid;
            offset = new Vector3(grid.cellSize.x / 2, 0.1f, grid.cellSize.z / 2);
            gridObject.GetComponent<Renderer>().material = worldGrid;
            gridObject.transform.localScale = defaultGridSize;
            gridObject.transform.position = defaultGridPosition;
            mouseIndicator.transform.GetChild(0).GetComponent<Renderer>().material = worldSelector;
            mouseIndicator.transform.localScale = new Vector3(1, 0, 1);
        }

        /// <summary>
        /// Gets cell position of where the cursor is
        /// </summary>
        /// <returns></returns>
        public Vector3Int MousePos() =>  grid.WorldToCell(inputManager.MousePositionInWorldSpace());
        

        /// <summary>
        /// Returns true if the current selected tile for building on is a valid position
        /// </summary>
        /// <returns></returns>
        public bool LegalPosition()
        {
            Tile tile = inputManager.FocusedTile;
            if (!TileMatchesGrid(tile)) return false;

            //Get the world position of the selected tile.
            Vector3 pos = grid.CellToWorld(grid.WorldToCell(mouseIndicator.transform.position));
            //Calculate offset needed
            Vector3 offset = tile.LocalPosition.MultiplyBy(tile.LocalOffsetFromRotation(tile));
            Vector3 itemPosition = pos + offset;
            // TakeAway the tiles position, plus the local offset of the grid.
            pos -= tile.Position - offset;

            //Check if the position is on either the centre of the tile, or in the line with the centre on one of the axes. 
            //Check in that direction for a socket, which indicates a road, and if there is, this is an illegal position.
            //Check that there isnt already an object at that position 
            if (pos == Vector3.zero || tile.GetObjectAtPos(itemPosition) != null) return false;

            //Mathf.Abs is used to allow for rounding errors with the above calcuations.
            if (Mathf.Abs(pos.x) <= 0.001f)
            {
                if (pos.z <= 0f && tile.Sockets[(int)Direction.DOWN] == 1) return false;
                if (pos.z >= 0f && tile.Sockets[(int)Direction.UP] == 1) return false;
            }

            if (Mathf.Abs(pos.z) <= 0.001f)
            {
                if (pos.x >= 0f && tile.Sockets[(int)Direction.RIGHT] == 1) return false;
                if (pos.x <= 0f && tile.Sockets[(int)Direction.LEFT] == 1) return false;
            }

            return true;
        }

        /// <summary>
        /// Rotates the object contained by the selected tile, at the position of the mouse
        /// </summary>
        public void Rotate()
        {
            Tile tile = inputManager.FocusedTile;
            if (tile == null) return;
            GameObject obj = tile.GetObjectAtPos(MouseGridPosition);

            if (!TileMatchesGrid(tile) || obj == null) return;

            obj.transform.Rotate(new Vector3(0, 90, 0), Space.Self);
        }

        public void LevelUpObject()
        {
            Tile tile = inputManager.FocusedTile;
            if (tile == null) return;
            if (!GameManager.Instance.UIManager.ExpSliderIsEnabled)
            {
                GameManager.Instance.UIManager.ExpSliderEnabled(true).BusinessMenuActive(true, true, false);
                return;
            }
            GameObject obj = tile.GetObjectAtPos(MouseGridPosition);
            if(obj.TryGetComponent(out IBusiness business))
            {
                business.AddEXP();
                GameManager.Instance.UIManager.ForceSliderUpdate().RefreshStatsPanel();
            }
        }

        public void RemoveObj()
        {
            Tile tile = inputManager.FocusedTile;
            tile.RemoveObjAtPos(MouseGridPosition);
            GameManager.Instance.UIManager.BusinessMenuActive(false);
        }

        /// <summary>
        /// Tries to hand the object over to the focused tile. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool HandOverObject(GameObject obj)
        {
            Tile tile = inputManager.FocusedTile;
            if (!TileMatchesGrid(tile)) return false;

            return tile.ReceiveObject(obj);
        }

        /// <summary>
        /// Returns the world position of the current cell within the global grid
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="result"></param>
        public void GetCellPositionInWorldSpace(Vector3 pos, out Vector3 result)
        {
            Grid current = grid;
            grid = defaultGrid;
            result = grid.CellToWorld(grid.WorldToCell(pos)) + new Vector3(5, 0, 5);
            grid = current;
        }
        /// <summary>
        /// Checks that the grid matches the tile's grid
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private bool TileMatchesGrid(Tile tile) => tile != null && grid == tile.Grid;
    }
}
