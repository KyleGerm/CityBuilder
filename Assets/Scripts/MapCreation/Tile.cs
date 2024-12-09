using Game.Tools;
using Game.Enumerators;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Interfaces;

namespace Game.MapGeneration
{
    public class Tile
    {
        public int[] Sockets = new int[4];
        public GameObject tile;
        private WFCGenerator manager;
        public Material tileImage;
        private List<GameObject> ItemsOnTile = new List<GameObject>();
        private Dictionary<Direction, int> validDirection = new Dictionary<Direction, int>();
        public Dictionary<Direction, int> ValidDirections { get => validDirection; }
        public Grid Grid { get; private set; }
        public Vector3 Position { get => tile.transform.position; }
        public Vector3 LocalPosition { get => Grid.transform.localPosition; }
        public Dictionary<Direction, List<RoadType>> edges = new(){
                                                                { Direction.UP,    new List<RoadType>() },
                                                                { Direction.RIGHT, new List<RoadType>() },
                                                                { Direction.DOWN,  new List<RoadType>() },
                                                                { Direction.LEFT,  new List<RoadType>() },
                                                               };

        public Tile(Material mat, int[] edges)
        {
            tileImage = mat;
            Sockets = edges;
            tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tile.GetComponent<MeshRenderer>().material = mat;
            tile.transform.position = Vector3.zero;
            tile.SetActive(false);
        }
        public Tile(WFCGenerator manager, Material mat, int[] edges)
        {
            tileImage = mat;
            Sockets = edges;
            this.manager = manager;
            tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tile.GetComponent<MeshRenderer>().material = mat;
            tile.transform.position = Vector3.zero;
            tile.SetActive(false);
        }

        public Tile(Tile duplicate, int[] edges)
        {
            tile = GameObject.Instantiate(duplicate.tile);
            Sockets = edges;
        }
        /// <summary>
        /// Sets the manager this tile will use
        /// </summary>
        /// <param name="generator"></param>
        public void SetManager(WFCGenerator generator)
        {
            manager = generator;
        }
        /// <summary>
        /// Compares two values and returns the result
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool CompareEdge(int a, int b) => a == b;
        
        /// <summary>
        /// Configures all intial options based on what sockets the tile currently has
        /// </summary>
        /// <param name="Tiles"></param>
        public void Analyse(List<Tile> Tiles)
        {
            foreach (Tile tile in Tiles)
            {
                if (CompareEdge(tile.Sockets[2], Sockets[0]))
                {
                    RoadType newType = manager.GetRoadType(tile.Sockets);
                    if (!edges[Direction.UP].Contains(newType))
                    {
                        edges[Direction.UP].Add(newType);
                    }
                }
                if (CompareEdge(tile.Sockets[3], Sockets[1]))
                {
                    RoadType newType = manager.GetRoadType(tile.Sockets);
                    if (!edges[Direction.RIGHT].Contains(newType))
                    {
                        edges[Direction.RIGHT].Add(newType);
                    }
                }
                if (CompareEdge(tile.Sockets[0], Sockets[2]))
                {
                    RoadType newType = manager.GetRoadType(tile.Sockets);
                    if (!edges[Direction.DOWN].Contains(newType))
                    {
                        edges[Direction.DOWN].Add(newType);
                    }
                }
                if (CompareEdge(tile.Sockets[1], Sockets[3]))
                {
                    RoadType newType = manager.GetRoadType(tile.Sockets);
                    if (!edges[Direction.LEFT].Contains(newType))
                    {
                        edges[Direction.LEFT].Add(newType);
                    }
                }
            }
        }
        /// <summary>
        /// Looks at each neighbour and matches its own socket value with the neighbours corresponding socket, then returns a roadtype based on that result
        /// </summary>
        /// <param name="neighbours"></param>
        /// <returns></returns>
        public RoadType Finalise(Tile[] neighbours)
        {
            int[] Test = new int[4];
            for (int i = 0; i < Test.Length; i++)
            {
                if (neighbours[i] == null)
                {
                    Test[i] = 0;
                }
                else
                {
                    Test[i] = neighbours[i].Sockets[(i + 2) % Sockets.Length];
                }
            }

            return manager.GetRoadType(Test);
        }
        /// <summary>
        /// Rotates the gameobject, and changes the sockets to match this rotation
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Tile Rotate(int num)
        {
            int[] newEdges = new int[4];

            int len = Sockets.Length;
            for (int i = 0; i < len; i++)
            {
                newEdges[i] = Sockets[(i + num + len) % len];
            }

            Tile NewTile = new Tile(tileImage, newEdges);
            NewTile.tile.gameObject.transform.Rotate(Vector3.up, ((Mathf.PI / 2) * -num) * Mathf.Rad2Deg, 0);
            return NewTile;
        }
        /// <summary>
        /// Ensures the ValidDirection dictionary has the same values as the sockets
        /// </summary>
        public void SyncSockets()
        {
            for (int i = 0; i < Sockets.Length; i++)
            {
                validDirection[(Direction)i] = Sockets[i];
            }

        }
        /// <summary>
        /// Returns this tile's grid. If it does not have one, it created one and returns that
        /// </summary>
        /// <returns></returns>
        public Grid GetGrid()
        {
            if (Grid != null) return Grid;

            GameObject tileChild = new GameObject();
            tileChild.transform.parent = tile.transform;
            Grid = tileChild.AddComponent<Grid>();
            Grid.cellSize = new Vector3(10f / 9f, 10f / 9f, 10f / 9f);//new Vector3(3.3333f, 3.3333f, 3.3333f);
            Grid.transform.localPosition = new Vector3(1.66667f / 3, 0, 1.66667f / 3).MultiplyBy(LocalOffsetFromRotation(this));
            tileChild.transform.RotateAround(tileChild.transform.parent.position, Vector3.up, 0f);

            return Grid;
        }
        /// <summary>
        /// Removes the grid
        /// </summary>
        public void RemoveGrid()
        {
            if (Grid == null || tile.transform.childCount == 0) return;
            GameObject.Destroy(tile.transform.GetChild(0).gameObject);
            Grid = null;
        }
        /// <summary>
        /// Takes the object given as long as the transform does not match any other objects already owned by this tile.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool ReceiveObject(GameObject obj)
        {
            foreach (GameObject item in ItemsOnTile)
            {
                if (item.transform.position == obj.transform.position) return false;
            }
            //Calling method here ensures that the object only adds itself to the game once it has been placed on the tile
            SetUp(obj)?.AddToGameSystems();
            ItemsOnTile.Add(obj);
            return true;
        }
        /// <summary>
        /// Returns an object which is less than 0.1 away from the given position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public GameObject GetObjectAtPos(Vector3 pos)
        {
              return ItemsOnTile.Find(x => Vector3.Distance(pos, x.transform.position) <= 0.1f);
            //return ItemsOnTile.Find(x => pos.ToVector3Int() == x.transform.position.ToVector3Int());
        }

        /// <summary>
        /// Takes a tiles rotation, and gives a vector which can be used to calculate the offset, if any.
        /// Used for children of tiles where the local position is not central, and the parent is rotated.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public Vector3 LocalOffsetFromRotation(Tile tile)
        {
            return tile.tile.transform.rotation.eulerAngles.y switch
            {
                90f => new Vector3(1, 0, -1),
                180f => new Vector3(1, 0, 1),
                270f => new Vector3(-1, 0, 1),
                _ => new Vector3(-1, 0, -1),
            };
        }
        /// <summary>
        /// Removes the object at a given position if it exists on this tile.
        /// </summary>
        /// <param name="Pos"></param>
        public void RemoveObjAtPos(Vector3 Pos)
        {
            GameObject objToRemove = GetObjectAtPos(Pos);
            if(objToRemove == null) { return; }
            ItemsOnTile.Remove(objToRemove);
            GameObject.Destroy(objToRemove);
        }
        /// <summary>
        /// Checks the gameobject for the INeedSetUp interface and returns the result
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private INeedSetUp SetUp(GameObject obj)
        {
            if (obj.TryGetComponent(out INeedSetUp business)) return business;
            return default;
        }
    }
}

