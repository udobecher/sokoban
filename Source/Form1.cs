using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SokobanSimple
{
    public partial class Form1 : Form
    {
        #region "Variables"
        Tile[] _GameTiles = LoadGameTiles(@".\Resources\Levels\levels.txt");
        Tile[] _LevelTiles;
        Stack<RollbackSet> _Rollbacks = new Stack<RollbackSet>();

        Tile[] _BoxTiles { get => _LevelTiles.Where(n => n.Symbol == '$').ToArray(); }
        Tile[] _WallTiles { get => _LevelTiles.Where(n => n.Symbol == '#').ToArray(); }
        Tile[] _GoalTiles { get => _LevelTiles.Where(n => n.Symbol == '.').ToArray(); }
        Tile _Player { get => _LevelTiles.Where(n => n.Symbol == '@').Single(); }
    
        Keys _LastKeyPressed = Keys.None;
        
        int _TileSize = 40;
        int _Level = -1;
        #endregion

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            MinimumSize = new Size(200, 200);
            Level = 0;

        }
        private int MaxLevels
        {
            get
            {
                return _GameTiles.Max(n => n.Level);
            }
        }                  
        public int Level
        {
            get
            {
                return _Level;
            }
            set
            {
                if (value == _Level || value < 0 || value > MaxLevels)
                    return;

                _Level = value;
                InitializeLevel();
            }        
        }
        public void InitializeLevel()
        {
            _LevelTiles = _GameTiles.Where(n => n.Level == Level).Select(n => n.CreateCopy()).ToArray();                       
            _LastKeyPressed = Keys.Down;
            Text = $"Sokoban - Level {Level + 1}";
            _Rollbacks.Clear();            
            Invalidate();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var allowedKeys = new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Escape, Keys.Add, Keys.Subtract,Keys.OemMinus,Keys.Oemplus,Keys.Delete };
            if (!allowedKeys.Contains(e.KeyCode))
            {
                return;
            }
            else if (e.KeyCode == Keys.Escape) 
            {
                Retry();
                return;
            }
            else if(e.KeyCode == Keys.Add || e.KeyCode ==  Keys.Oemplus)
            {
                Level++;
                return;
            }
            else if(e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                Level--;
                return;
            }
            else if(e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                Level--;
                return;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_Rollbacks.Count == 0) return;
                var rollbackSet = _Rollbacks.Pop();
                _LevelTiles = rollbackSet.Tiles;
                _LastKeyPressed = rollbackSet.Direction;
                Invalidate();
                return;
            }

            int newX = _Player.X;
            int newY = _Player.Y;

            if (e.KeyCode == Keys.Left) newX--;
            else if (e.KeyCode == Keys.Right) newX++;
            else if (e.KeyCode == Keys.Up) newY--;
            else if (e.KeyCode == Keys.Down) newY++;

            var levelTmp = _LevelTiles.Select(n => n.CreateCopy()).ToArray();
            if (!TryMove(e.KeyCode, newX, newY))
                return;
            
            _Rollbacks.Push(new RollbackSet() {Direction = _LastKeyPressed,Tiles = levelTmp });
            
            _LastKeyPressed = e.KeyCode;            
            Invalidate();
            CheckWin();
        }
        private bool TryMove(Keys key, int newX, int newY)
        {
            var tileAtNewPos = GetTile(newX, newY) ?? Tile.Empty;

            if (tileAtNewPos.Symbol == '#')
                return false;

            if (tileAtNewPos.Symbol == '$')
            {
                int xBehindBox = tileAtNewPos.X;
                int yBehindBox = tileAtNewPos.Y;

                if (key == Keys.Left) xBehindBox--;
                else if (key == Keys.Right) xBehindBox++;
                else if (key == Keys.Up) yBehindBox--;
                else if (key == Keys.Down) yBehindBox++;
                var tileBehindBox = GetTile(xBehindBox, yBehindBox) ?? Tile.Empty;
                if (tileBehindBox.Symbol == ' ')
                {
                    tileAtNewPos.X = xBehindBox;
                    tileAtNewPos.Y = yBehindBox;                    
                }
                else
                {
                    return false;
                }

            }

            _Player.X = newX;
            _Player.Y = newY;
            
            return true;
        }
        private void CheckWin()
        {
            var goals = _LevelTiles.Where(n => n.Symbol == '.');
            var boxes = _LevelTiles.Where(n => n.Symbol == '$');
            var win = boxes.All(b => goals.Any(g => g.X == b.X && g.Y == b.Y));
            if (win)
            {
                if (Level < MaxLevels)
                {
                    Level++;
                }
                else
                {
                    if (MessageBox.Show(this, "Du hast alle Levels abgeschlossen...\r\nNochmal spielen?", "Herzliche Glückwünsche!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Level = 0;
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }
        private Tile GetTile(int x, int y)
        {
            return _LevelTiles.
                Where(n => n.Level == _Level && n.X == x && n.Y == y && n.Symbol != '.').
                FirstOrDefault();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            int imageWidth = _LevelTiles.Max(n => n.X + 1) * _TileSize;
            int imageHeight = _LevelTiles.Max(n => n.Y + 2) * _TileSize;

            Image canvas = new Bitmap(imageWidth, imageHeight);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            base.OnPaint(e);

            var rectGoals = _GoalTiles.Select(n => new Rectangle(n.X * _TileSize, n.Y * _TileSize, _TileSize, _TileSize)).ToList();
            foreach (Rectangle rect in rectGoals)
            {                
                g.DrawImage(Images.Goal, rect);
            }            

            var rectBoxes = _BoxTiles.Select(n => new Rectangle(n.X * _TileSize, n.Y * _TileSize, _TileSize, _TileSize)).ToList();
            foreach(Rectangle rect in rectBoxes)
            {
                rect.Inflate(-3, -3);
                g.DrawImage(Images.Box, rect);
            }           

            var rectWalls = _WallTiles.Select(n => new Rectangle(n.X * _TileSize, n.Y * _TileSize, _TileSize, _TileSize)).ToList();
            rectWalls.ForEach(n => g.DrawImage(Images.Wall, n));

            var rectPlayer = new Rectangle(_Player.X * _TileSize, _Player.Y * _TileSize, _TileSize, _TileSize);
            rectPlayer.Inflate(-6, -6);
            Image playerImage;

            if (_LastKeyPressed == Keys.Up) playerImage = Images.PlayerUp;
            else if (_LastKeyPressed == Keys.Left) playerImage = Images.PlayerLeft;
            else if (_LastKeyPressed == Keys.Right) playerImage = Images.PlayerRight;
            else playerImage = Images.PlayerDown;
                        
            g.DrawImage(playerImage, rectPlayer);
            pictureBox.Image = canvas;

            // Draw Text
            Image imageText = new Bitmap(pictureBoxText.Width, pictureBoxText.Height);
            Graphics gText = Graphics.FromImage(imageText);
            gText.Clear(Color.CornflowerBlue);
            RectangleF rectTxt = new RectangleF(0, 0, pictureBoxText.Width, pictureBoxText.Height);
            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap)
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            gText.DrawString("'Esc' Zurücksetzen | '-' Vorheriger Level | '+' Nächster Level | 'Entf' Bewegung zurücksetzen", this.Font, Brushes.DarkBlue, rectTxt, sf);
            pictureBoxText.Image = imageText;

        }
        private static Tile[] LoadGameTiles(string path)
        {
            var line = string.Empty;
            int lineCount = -1;
            int symbolCount = 0;
            int levelCount = -1;
            List<Tile> tileList = new List<Tile>();

            using (StreamReader streamReader = new StreamReader(path))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Ignore Komments
                    if (line.StartsWith(";"))
                        continue;

                    // Empty line = new level.
                    if (string.IsNullOrWhiteSpace(line.Trim()))
                    {
                        levelCount++;
                        lineCount = 0;
                    }
                    else
                    {
                        lineCount++;
                    }

                    line = line.TrimEnd();
                    symbolCount = 0;

                    foreach (char symbol in line.ToArray())
                    {
                        if (symbol == '+')
                        {
                            // Player on goal.
                            tileList.Add(new Tile(levelCount, symbolCount, lineCount, '@'));
                            tileList.Add(new Tile(levelCount, symbolCount, lineCount, '.'));
                        }
                        else if (symbol == '*')
                        {
                            // Box on goal.
                            tileList.Add(new Tile(levelCount, symbolCount, lineCount, '$'));
                            tileList.Add(new Tile(levelCount, symbolCount, lineCount, '.'));
                        }
                        else if(symbol != ' ')
                        {
                            tileList.Add(new Tile(levelCount, symbolCount, lineCount, symbol));
                        }
                        symbolCount++;
                    }
                    
                }
            }
            return tileList.ToArray();
        }
        private void Retry()
        {
            InitializeLevel();
        }
    }
}

