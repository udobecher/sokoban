using System.Windows.Forms;

namespace SokobanSimple
{
    public class RollbackSet
    {
        public Keys Direction { get; set; }
        public Tile[] Tiles { get; set; }
    }
}
