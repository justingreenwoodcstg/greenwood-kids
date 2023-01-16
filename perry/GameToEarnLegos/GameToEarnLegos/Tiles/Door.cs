﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameToEarnLegos.Tiles
{
    public class Door : Tile
    {
        public Bitmap closedImage = Resources.Image_ClosedDoor_Thin;
        public Bitmap openImage = Resources.Image_Player;
        private Bitmap closedImageWide = Resources.Image_ClosedDoor_Wide;
        private Bitmap openImageWide = Resources.Image_Player;
        private bool LeftRight;
        public bool IsClosed = false;
        public override string Tag => "door";
        public Door(int col, int row, bool leftRight) : base(col, row, Resources.Image_ClosedDoor_Thin) 
        {
            IsBlocker = IsClosed; 
            LeftRight = leftRight;
            if (leftRight)
            {
                closedImage = closedImageWide;
                openImage = openImageWide;
            }
            this.image = closedImage;
        }
        

    }
}
