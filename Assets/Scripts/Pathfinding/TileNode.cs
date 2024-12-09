using UnityEngine;
using Game.MapGeneration;

namespace Game.Pathfinding
{
    public class TileNode
    {
        public Tile tile { get; private set; }
        public TileNode Parent { get; private set; }
        public int Steps { get; private set; }
        public bool visited;
        public float DistanceToTarget { get; private set; }
        public Vector3 Postition { get => tile.Position; }
        public TileNode(Tile tile, Vector3 targetPos)
        {
            this.tile = tile;
            Steps = 0;
            DistanceToTarget = Vector3.Distance(Postition, targetPos);
        }
        /// <summary>
        /// Sets the steps to a value
        /// </summary>
        /// <param name="steps"></param>
        public void SetSteps(int steps)
        {
            Steps = steps;
        }
        /// <summary>
        /// Sets the parent to a given TileNode
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(TileNode parent)
        {
            Parent ??= parent;
        }
    }
}
