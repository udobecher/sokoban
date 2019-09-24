using System.Drawing;
using System.IO;

namespace SokobanSimple
{
    public static class Images
    {
        public static readonly Image PlayerUp = LoadImage(@".\Resources\Images\PlayerUp.png");
        public static readonly Image PlayerDown = LoadImage(@".\Resources\Images\PlayerDown.png");
        public static readonly Image PlayerLeft = LoadImage(@".\Resources\Images\PlayerLeft.png");
        public static readonly Image PlayerRight = LoadImage(@".\Resources\Images\\PlayerRight.png");
        public static readonly Image Box = LoadImage(@".\Resources\Images\Box.png");
        public static readonly Image Wall = LoadImage(@".\Resources\Images\Wall.png");
        public static readonly Image Goal = LoadImage(@".\Resources\Images\Goal.png");

        private static Image LoadImage(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Image.FromStream(stream);
            }
        }
    }
}
