using Game.Enumerators;
namespace Game.MapGeneration
{
    /// <summary>
    /// Data container for Tiles and possible types
    /// </summary>
    public class Cell
    {
        public bool collapsed = false;
        public RoadType[] options;
        public Tile tile;
        public Cell(int value)
        {
            options = new RoadType[value];
            for (int i = 0; i < value; i++)
            {
                options[i] = (RoadType)i;
            }
        }

        public Cell(int[] options)
        {
            if (options == null) return;
            this.options = new RoadType[options.Length];
            foreach (int i in options)
            {
                this.options[i] = (RoadType)i;
            }
        }
    }
}
