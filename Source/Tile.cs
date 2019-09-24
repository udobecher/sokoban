namespace SokobanSimple
{
    public class Tile
    {
        public static Tile Empty
        {
            get
            {
                return new Tile(0, 0, 0, ' ');
            }
        }
        public Tile(int level, int x, int y, char symbol)
        {
            Level = level;
            X = x;
            Y = y;
            Symbol = symbol;
        }        
        public int Level { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public char Symbol { get; set; }
        public Tile CreateCopy()
        {
            return (Tile)MemberwiseClone();
        }
        public override string ToString()
        {
            return $"{Level},{X},{Y},{Symbol}";
        }
    }
}
