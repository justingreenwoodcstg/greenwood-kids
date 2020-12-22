﻿using System.Drawing;

namespace PerrysGame
{
    public class Bridge : Tile
    {
        public const char TileLetter = 'E';

        public Bridge() : base() { }
        public Bridge(int colIndex, int rowIndex) : base(colIndex, rowIndex) { }

        public override Brush TileBrush => Brushes.SaddleBrown;
    }
}
