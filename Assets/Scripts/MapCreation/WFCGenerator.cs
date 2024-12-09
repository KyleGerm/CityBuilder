using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Managers;
using Game.Enumerators;
namespace Game.MapGeneration
{
    public class WFCGenerator : MonoBehaviour
    {
        [SerializeField] List<Material> material;
        [SerializeField] int mapSize;
        [SerializeField] private GridSystem gridSystem;
        private List<Tile> tiles = new List<Tile>();
        private List<Cell> grid = new List<Cell>();
        private Vector3[] offsets = new Vector3[] { new Vector3(0, 0, 10), new Vector3(10, 0, 0), new Vector3(0, 0, -10), new Vector3(-10, 0, 0) };

        public List<Tile> generatedTiles = new List<Tile>();
        public Dictionary<RoadType, Material> roads = new Dictionary<RoadType, Material>();
        public bool done = false;
        public Vector3 MapSize { get => new Vector3(mapSize, 0, mapSize); }
        public Dictionary<RoadType, int[]> map = new Dictionary<RoadType, int[]>()
    {
        {RoadType.CROSSROAD, new int[]{1,1,1,1} },
        {RoadType.EMPTY, new int[]{0,0,0,0} },
        {RoadType.RIGHT_TURN, new int[]{1,1,0,0} },
        {RoadType.STRAIGHT, new int[]{1,0,1,0} },
        {RoadType.T_JUNCT_DOWN, new int[]{0,1,1,1} },
    };

        public void Awake()
        {
            try { mapSize = GameObject.Find("MapSizeObject").GetComponent<MapSizeContainer>().MapSize; }
            catch { }
        }
        public void Start()
        {
            RemoveInspectorGrid();
            gridSystem.SetGridSize(mapSize);
            Camera.main.gameObject.GetComponent<CameraController>().Setbounds(transform.position, mapSize);
            CheckInitialSetup();
            StartOver();
            done = false;
            while (!done) Collapse();
            InputManager.Instance?.Subscribe(GetTile);
        }
        /// <summary>
        /// Main method to generate a cohesive map
        /// </summary>
        public void Collapse()
        {
            if (grid.Count == 0) return;
            CheckCollapsedCells();
            SortGridCopy();
            CheckNeighbours();
        }
        /// <summary>
        /// Clears all generated content
        /// </summary>
        private void StartOver()
        {
            foreach (var tile in generatedTiles)
            {
                DestroyImmediate(tile.tile);
            }
            generatedTiles = new List<Tile>();
            grid = new();
            for (int i = 0; i < MapSize.x * MapSize.z; i++)
            {
                grid.Add(new Cell(tiles.Count));
            }
        }
        /// <summary>
        /// Checks options against the ones that are valid, and removes the ones which dont match
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="valid"></param>
        private void CheckValid(ref int[] arr, List<RoadType> valid)
        {
            List<RoadType> list = new List<RoadType>();
            foreach (int element in arr)
            {
                list.Add((RoadType)element);
            }
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                RoadType element = list[i];

                if (!valid.Contains(element))
                {
                    list.Remove(list.ElementAt(i));
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                arr[i] = (int)list[i];
            }
        }
        /// <summary>
        /// Sorts the tiles based on how many options they have left, and then chooses an option at random for one of the tiles with the least options left.
        /// </summary>
        private void SortGridCopy()
        {
            List<Cell> gridCopy = grid;
            gridCopy = gridCopy.Where(a => !a.collapsed).ToList();

            if (gridCopy.Count == 0)
            {
                FinaliseConfig();
                FinaliseConfig();
                CreateObject();
                done = true;
                return;
            }

            gridCopy.Sort((a, b) => a.options.Length - b.options.Length);

            int len = gridCopy[0].options.Length;
            int stopIndex = 0;
            for (int i = 0; i < gridCopy.Count; i++)
            {
                if (gridCopy[i].options.Length > len)
                {
                    stopIndex = i;
                    break;
                }
            }

            if (stopIndex > 0) gridCopy.RemoveRange(stopIndex, gridCopy.Count - stopIndex);

            Cell nextCell = gridCopy[UnityEngine.Random.Range(0, gridCopy.Count)];
            nextCell.collapsed = true;

            if (nextCell.options.Length < 1)
            {
                StartOver();
                return;
            }
            else
            {
                RoadType pick = nextCell.options[UnityEngine.Random.Range(0, nextCell.options.Length)];
                nextCell.options = new RoadType[] { pick };

                if (nextCell.tile != null && nextCell.tile.tile != null) GameObject.DestroyImmediate(nextCell.tile.tile);
            }
        }
        /// <summary>
        /// Goes through the neighbours in each tile position, and tries to determine its remaining options.
        /// This becomes the new starting point for the next iteration in the generation process.
        /// </summary>
        private void CheckNeighbours()
        {
            Cell[] nextGrid = new Cell[grid.Count];
            for (int j = 0; j < MapSize.z; j++)
            {
                for (int i = 0; i < MapSize.x; i++)
                {
                    int index = i + (j * (int)MapSize.z);
                    if (grid[index].collapsed)
                    {
                        nextGrid[index] = grid[index];
                    }
                    else
                    {
                        int[] options = Enumerable.Range(0, tiles.Count).ToArray();
                        //looking up
                        if (j > 0)
                        {
                            CheckNeighbour(i + (j - 1) * (int)MapSize.z, Direction.UP, ref options);
                        }
                        //looking right
                        if (i < (int)MapSize.x - 1)
                        {
                            CheckNeighbour(i + 1 + j * (int)MapSize.x, Direction.RIGHT, ref options);
                        }
                        //looking down
                        if (j < (int)MapSize.z - 1)
                        {
                            CheckNeighbour(i + (j + 1) * (int)MapSize.z, Direction.DOWN, ref options);
                        }
                        //looking left
                        if (i > 0)
                        {
                            CheckNeighbour(i - 1 + j * (int)MapSize.x, Direction.LEFT, ref options);
                        }
                        nextGrid[index] = new Cell(options);
                    }
                }
            }
            grid = nextGrid.ToList();
        }
        /// <summary>
        /// Looks in the specified direction, and modifies the options list based on its results 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        /// <param name="options"></param>
        private void CheckNeighbour(int index, Direction direction, ref int[] options)
        {
            Cell cell = grid[index];
            List<RoadType> validOptions = new List<RoadType>();
            foreach (int option in cell.options)
            {
                var valid = tiles[option].edges[direction];
                validOptions.AddRange(valid);
            }
            CheckValid(ref options, validOptions);
        }
        /// <summary>
        /// For every collapsed cell in the list, the data is finalised.
        /// </summary>
        private void CheckCollapsedCells()
        {
            for (int i = 0; i < (int)MapSize.x; i++)
            {
                for (int j = 0; j < (int)MapSize.z; j++)
                {
                    //Selects each cell as they appear on the grid
                    Cell cell = grid[j + (i * (int)MapSize.z)];
                    if (cell.collapsed)
                    {
                        RoadType index = cell.options[0];
                        if (cell.tile != null)
                        {
                            if (cell.tile.tile != null)
                            {
                                GameObject.DestroyImmediate(cell.tile.tile);
                            }
                            cell.tile.tile = GameObject.Instantiate(tiles[(int)index].tile);
                            cell.tile.tile.transform.position = new Vector3((i * 10), 0, j * 10);
                            cell.tile.Sockets = map[index];
                            cell.tile.SetManager(this);
                            cell.tile.tile.SetActive(true);
                        }
                        else
                        {
                            cell.tile = new Tile(tiles[(int)index], map[index]);
                            generatedTiles.Add(cell.tile);
                            cell.tile.SetManager(this);
                            cell.tile.tile.transform.position = new Vector3((i * 10), 0, j * 10);
                            cell.tile.tile.SetActive(true);
                        }
                    }

                }
            }
        }
        /// <summary>
        /// Final pass to make sure each tile connects properly with each of its neighbours
        /// </summary>
        private void FinaliseConfig()
        {
            foreach (Tile tile in generatedTiles)
            {
                Tile[] tiles = new Tile[4];
                for (int i = 0; i < offsets.Length; i++)
                {
                    if (GetNeighbour(tile.tile.transform.position + offsets[i], out Tile neighbour))
                    {
                        tiles[i] = neighbour;
                    }
                    else tiles[i] = null;
                }
                RoadType roadType = tile.Finalise(tiles);
                Vector3 pos = tile.tile.transform.position;
                if (roadType == RoadType.NONE)
                {
                    continue;
                }
                if (tile.tile != null)
                {
                    DestroyImmediate(tile.tile);
                }

                tile.tile = Instantiate(this.tiles[(int)roadType].tile);
                tile.tile.transform.position = pos;
                tile.Sockets = map[roadType];
                tile.SetManager(this);
                tile.SyncSockets();
                tile.tile.SetActive(true);
            }
        }
        /// <summary>
        /// Searches the tiles list for a Tile that has the given position. 
        /// Returns the result of the search, and a Tile
        /// </summary>
        /// <param name="position"></param>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        private bool GetNeighbour(Vector3 position, out Tile neighbour)
        {
            foreach (Tile tile in generatedTiles)
            {
                if (tile.tile.transform.position == position)
                {
                    neighbour = tile;
                    return true;
                }
            }
            neighbour = null;
            return false;
        }
        /// <summary>
        /// Goes through the given list, and removes any duplicates iterations of the blueprint tiles
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public List<Tile> RemoveDuplicatedTiles(List<Tile> tiles)
        {
            var uniqueTilesMap = new Dictionary<string, Tile>();
            foreach (var tile in tiles)
            {
                var key = new string($"{tile.Sockets[0]}{tile.Sockets[1]}{tile.Sockets[2]}{tile.Sockets[3]}"); // ex: "0110"

                if (!uniqueTilesMap.ContainsKey(key))
                {
                    uniqueTilesMap.Add(key, tile);
                }
                else
                {
                    DestroyImmediate(tile.tile);
                }
            }
            return uniqueTilesMap.Values.ToList();
        }

        /// <summary>
        /// Checks checks through the list of roadtype sockets, and returns the one which matches.
        /// </summary>
        /// <param name="sockets"></param>
        /// <returns></returns>
        public RoadType GetRoadType(int[] sockets)
        {
            for (int i = 0; i < map.Count; i++)
            {
                for (int j = 0; j < sockets.Length; j++)
                {
                    if (sockets[j] != map[(RoadType)i][j])
                    {
                        break;
                    }

                    if (j == 3)
                    {
                        return (RoadType)i;
                    }
                }
            }
            return RoadType.NONE;
        }
        /// <summary>
        /// Ensure all initial parameters for the map generation are correct. This is especially important if a map has been generated in the editor.
        /// </summary>
        private void CheckInitialSetup()
        {
            roads = new();
            foreach (Tile tile in tiles)
            {
                DestroyImmediate(tile.tile);
            }
            tiles = new();
            //set up the road configuration for the materials, and add each configuration to a new tile
            for (int i = 0; i < material.Count; i++)
            {
                roads.Add((RoadType)i, material[i]);
                tiles.Add(new Tile(this, roads[(RoadType)i], map[(RoadType)i]));
            }

            //Add a rotated version of each tile to the list. This also modifies the socket config to match
            int InitialTileCount = tiles.Count;
            for (int i = 0; i < InitialTileCount; i++)
            {
                List<Tile> tempTiles = new List<Tile>();
                for (int j = 1; j <= 4; j++)
                {
                    tempTiles.Add(tiles[i].Rotate(j));
                    tempTiles.Last().SetManager(this);
                }
                tiles.AddRange(tempTiles);
            }
            //remove duplicated verions of the tiles in the tiles list
            tiles = RemoveDuplicatedTiles(tiles);
            //add the modified sockets to the mapping list and add the appropriate key
            if (map.Count != 12)
            {
                for (int i = InitialTileCount; i < tiles.Count; i++)
                {
                    map.Add((RoadType)i, tiles[i].Sockets);
                }
            }
            //modify names for easy identification, and analyse tiles for their options
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile thisTile = tiles[i];
                thisTile.tile.name = new string($"Tile {(RoadType)i}");
                thisTile.Analyse(tiles);
            }
        }
        /// <summary>
        /// Adds all generated tiles into a parent object
        /// </summary>
        private void CreateObject()
        {
            GameObject obj = GameObject.Find("GridParent");
            if (obj == null)
            {
                obj = new GameObject("GridParent");
            }

            foreach (Tile tile in generatedTiles)
            {
                tile.tile.transform.parent = obj.transform;
            }
            foreach (Tile tile in tiles)
            {
                DestroyImmediate(tile.tile);
            }
        }
        /// <summary>
        /// Searches for a tile matching the position and returns it
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Tile GetTile(Vector3 position)
        {

            foreach (Tile tiles in generatedTiles)
            {
                if (tiles.tile.transform.position == position)
                {
                    return tiles;
                }
            }

            return null;
        }

        /// <summary>
        /// If there is a grid already created by the inspector tool, this will remove it.
        /// If this is called in play mode, Inspector grid will reappear when play mode is stopped. 
        /// </summary>
        public void RemoveInspectorGrid()
        {
            GameObject grid = GameObject.Find("GridParent");
            if (grid != null)
            {
                while (grid.transform.childCount > 0)
                {
                    DestroyImmediate(grid.transform.GetChild(0).gameObject);
                }
            }
        }
    }
}