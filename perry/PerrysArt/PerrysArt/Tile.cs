﻿using System.Drawing;

namespace PerrysArt
{
    public class Lava : Tile
    {
        public const char TileLetter = 'L';

        public Lava() : base() { }
        public Lava(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.OrangeRed;
    }

    public class Grass : Tile
    {
        public const char TileLetter = 'G';

        public Grass() : base() { }
        public Grass(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.Lime;
    }

    public class Water : Tile
    {
        public const char TileLetter = 'W';

        public Water() : base() { }
        public Water(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.Aqua;
    }

    public class Bridge : Tile
    {
        public const char TileLetter = 'E';

        public Bridge() : base() { }
        public Bridge(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.SaddleBrown;
    }
    public class Sand : Tile
    {
        public const char TileLetter = 'S';

        public Sand() : base() { }
        public Sand(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.Tan;
    }
    public class Wall : Tile
    {
        public const char TileLetter = 'B';

        public Wall() : base() { }
        public Wall(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.SlateGray;
    }
    public class Border : Tile
    {
        public static Bitmap _BorderImage = new Bitmap("PerrysArtBorder.png");
        public const char TileLetter = 'O';

        public Border() : base() { }
        public Border(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }
        public override void DrawMe(Graphics g, float zoom = 1)
        {
            //base.DrawMe(g, zoom);
            g.DrawImage(_BorderImage, GetRect(zoom));
        }
        public override Brush TileBrush => Brushes.Black;
    }
    public class DeepWater : Tile
    {
        public const char TileLetter = 'Q';

        public DeepWater() : base() { }
        public DeepWater(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.Navy;
    }
    public class Door : Tile
    {
        public static Bitmap _DoorImage = new Bitmap("ClosedDoor.png");
        public static Bitmap _DoorImage2 = new Bitmap("ClosedDoor2.png");        
        public const char TileLetter = 'D';

        public Door() : base() { }
        public Door(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.Brown;
    }

    public class AmmoPack : DrawableObject
    {
        public const char TileLetter = 'A';
        private Brush BackgroundBrush { get { return Brushes.Green; } }
        private Pen OutlinePen { get { return Pens.Black; } }

        public AmmoPack() 
        {
            this.Size = 10;
            this.IsAlive = false;
        }

        public int Ammo = 100;
        public int ReviveAmmo = 0;

        public override void DrawMe(Graphics g, float zoom = 1)
        {
            g.FillRectangle(BackgroundBrush, GetRect(zoom));
            g.DrawRectangle(OutlinePen, GetRect(zoom));
        }
    }
    public class Gold : DrawableObject
    {
        public const char GrassTileLetter = 'g';
        public const char NoTileLetter = 't';
        private Brush BackgroundBrush { get { return Brushes.Gold; } }

        public Gold()
        {
            this.Size = 10;
            this.IsAlive = false;
        }

        public int Coins = 100;

        public override void DrawMe(Graphics g, float zoom = 1)
        {
            g.FillEllipse(BackgroundBrush, GetRect(zoom));
        }
    }
}
