using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.MapGeneration;
using Game.Enumerators;

namespace Game.Pathfinding
{
    public class PathFinder
    {
        private List<Vector3> path = new();
        private WFCGenerator generator;
        private Vector3 Start, End;
        public PathFinder(WFCGenerator gen)
        {
            generator = gen;
        }
        private Dictionary<Direction, Vector3> offset = new() {
        { Direction.UP, new Vector3(0, 0, 1) },
        { Direction.RIGHT, new Vector3( 1, 0, 0 ) },
        { Direction.DOWN, new Vector3 ( 0, 0, -1 ) },
        { Direction.LEFT, new Vector3( -1, 0, 0) },
                                                              };

        /// <summary>
        /// Takes a start and end position, and returns a list of vectors
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
            path = new List<Vector3>();
            if(Start == End)
            {
                path.Add(Start);
                return path;
            }
            GetPath();
            return path;
        }
        /// <summary>
        /// Starts at the beginning tile, and wraps it in a TileNode class
        /// which helps the pathfinder generate a path
        /// </summary>
        private void GetPath()
        {
            bool EndReached = false;
            List<TileNode> queue = new List<TileNode>();
            Tile firstTile = generator.GetTile(Start);
            TileNode current = new(firstTile, End);
            List<Tile> visitedNodes = new();
            while (queue.Count > 0 || !EndReached)
            {
                foreach (Direction dir in current.tile.ValidDirections.Keys)
                {
                    if (current.tile.ValidDirections[dir] == 0) continue;

                    Tile next = generator.GetTile(current.Postition + (offset[dir] * 10));
                    if (next == null || visitedNodes.Contains(next)) continue;
                    visitedNodes.Add(next);
                    TileNode tileNode = new(next, End);
                    tileNode.SetParent(current);
                    tileNode.SetSteps(current.Steps + 1);
                    queue.Add(tileNode);

                    if (tileNode.Postition == End)
                    {
                        EndReached = true;
                        current = tileNode;
                        break;
                    }
                }
                if (EndReached) break;
                //This Sort equation is not optimal, can cause some irregular path choices, but brings the Visited Nodes list down to half, sometimes a third of the size
                //This means faster search times 
                queue.Sort((x, y) => (x.Steps + x.DistanceToTarget).CompareTo((y.Steps + y.DistanceToTarget)));
                current = queue.First();
                queue.Remove(current);
            }
            path.Add(current.Postition);
            while (current.Parent != null)
            {
                current = current.Parent;
                path.Add(current.Postition);
            }

            path.Reverse();
        }
    }
}