﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GameToEarnLegos.Tiles;

namespace GameToEarnLegos
{
    public class GameController : IGameController
    {
        private const float TileSize = 20f;
        private float scaleFactor = 0.8f;
        private FormTriangleTrees _form;
        private bool gameOver = false;
        private bool isPaused = false;
        private bool EscapeKeyIsUsed = false;
        public bool goToMenu = false;
        private bool EscapeKeyIsPressed = false;
        public bool CHEATS => _form.CHEATS;
        public float ScaleFactor => scaleFactor;
        private ILevel _currentLevel => _form.currentLevel;
        int AliveBadguys;
        Player player;
        List<Water> waters = new List<Water>();
        List<DeepWater> deepWaters = new List<DeepWater>();
        List<Block> blocks = new List<Block>();
        List<Badguy> badguys = new List<Badguy>();
        List<Tile> tiles = new List<Tile>();
        List<Gold> golds = new List<Gold>();
        List<Door> doors = new List<Door>();

        List<Ammunition> ammunitions = new List<Ammunition>();
        string[] levelTop => _currentLevel.levelTop;
        string goal => _currentLevel.Goal;
        private int ShootingCoolDown = 5;
        public GameController(FormTriangleTrees form)
        {
            _form = form;
        }

        public PointF CenterPoint => new PointF(player.X + (player.Width) / 2, player.Y + (player.Height / 2));
        
        private RectangleF SeenRect(float scale)
        {
            return new RectangleF(CenterPoint.X * scale- (0.5f * (TileSize * scale * 10)), CenterPoint.Y * scale - (0.5f * (TileSize * scale * 10)), TileSize * scale * 10, TileSize * scale * 10);
        }
        private RectangleF PlayerUseRect(float scale)
        {
            return new RectangleF(CenterPoint.X * scale - (0.5f * (TileSize * scale * 4)), CenterPoint.Y * scale - (0.5f * (TileSize * scale * 4)), TileSize * scale * 4, TileSize * scale * 4);
        }
        public void DrawTheGame(Graphics g)
        {
            foreach (Tile tile in tiles.Where(t=>  (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))) && t.Tag != "door" ))
            {                  
                
                DrawScaledTiles(g, tile);
            }
            foreach (Gold gold in golds.Where(t => (t.IsPickedUp == false) && (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor)))))
            {
                DrawScaledTiles(g, gold);
            }
            foreach (Badguy badguy in badguys.Where(t => SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))))
            {
                DrawScaledTiles(g, badguy);
            }
            if (player.IsAlive)
                DrawScaledTiles(g, player);            
            foreach (Ammunition ammunition in ammunitions.Where(t => SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))))
            {
                DrawScaledTiles(g, ammunition);
            }
            foreach (Door door in tiles.Where(t => (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))) && t.Tag == "door"))
            {
                if (door.IsClosed == true)
                {
                    door.image = door.closedImage;
                }
                else
                    door.image = door.openImage;
                DrawScaledTiles(g, door);
            }
            if (CHEATS)
            {
                g.DrawString($"IsRunning:{player.IsRunning} isEscape {EscapeKeyIsUsed} IsInWater:{player.IsInWater} IsShooting:{player.IsShooting} " +
                    $"Ammo:{player.ammunition} Score: {_currentLevel.CurrentScore}/{_currentLevel.Score} Badguys: {AliveBadguys} HasUsed: {player.HasUsed} UsingKey: {player.UsingKey}",
                    SystemFonts.DefaultFont, Brushes.LightGray, 5, 5);
            }
            else
            {
                g.DrawString($"Ammo:{player.ammunition} Score {_currentLevel.CurrentScore}/{_currentLevel.Score} Badguys: {AliveBadguys}",
                    SystemFonts.DefaultFont, Brushes.LightGray, 5, 5);
            }

            if(gameOver == true)
            {
                g.DrawString($"Press 'Enter' to continue.",
                    SystemFonts.DefaultFont, Brushes.LightGray, 700, 400);
            }
            else if (isPaused)
            {
                g.DrawString($"Do you want to exit level? (Y)es (N)o",
                    SystemFonts.DefaultFont, Brushes.LightGray, 700, 400);
            }

        }

        public void KeyDown(object sender, KeyEventArgs e)
        {

            if (player.IsAlive && gameOver == false)
            {
                if (isPaused == false)
                {
                    if (e.KeyCode == Keys.ShiftKey)
                    {
                        // player.Speed = player.RunSpeed;
                        player.IsRunning = true;
                    }
                    if (e.KeyCode == Keys.Z && scaleFactor < 1.2f)
                    {
                        scaleFactor += 0.1f;
                    }
                    if (e.KeyCode == Keys.X && scaleFactor > 0.8f)
                    {
                        scaleFactor -= 0.1f;
                    }
                    if (e.KeyCode == Keys.S)
                    {
                        player.GoingDown = true;
                        player.GoingUp = false;
                    }
                    if (e.KeyCode == Keys.W)
                    {
                        player.GoingUp = true;
                        player.GoingDown = false;
                    }
                    if (e.KeyCode == Keys.A)
                    {
                        player.GoingLeft = true;
                        player.GoingRight = false;
                    }
                    if (e.KeyCode == Keys.D)
                    {
                        player.GoingRight = true;
                        player.GoingLeft = false;
                    }
                    if (e.KeyCode == Keys.F12 && CHEATS)
                    {
                        foreach (Badguy badguy in badguys)
                        {
                            badguy.IsDead = true;
                        }
                        _currentLevel.CurrentScore = 999;
                    }
                    if (e.KeyCode == Keys.E)
                    {

                        player.UsingKey = true;
                    }
                }
                else
                {
                    if(e.KeyCode == Keys.N)
                    {
                        isPaused = false;
                    }
                    else if (e.KeyCode == Keys.Y)
                    {
                        goToMenu = true;
                    }
                }
                if(e.KeyCode == Keys.Escape)
                {
                    EscapeKeyIsPressed = true;
                }
                //Shooting Direction
                {
                    if (player.GoingDown && player.GoingRight)
                    {
                        player.LastWentDirection = "southeast";
                    }
                    else if (player.GoingDown && player.GoingLeft)
                    {
                        player.LastWentDirection = "southwest";
                    }
                    else if (player.GoingUp && player.GoingRight)
                    {
                        player.LastWentDirection = "northeast";
                    }
                    else if (player.GoingUp && player.GoingLeft)
                    {
                        player.LastWentDirection = "northwest";
                    }
                    else if (player.GoingDown)
                    {
                        player.LastWentDirection = "south";
                    }
                    else if (player.GoingLeft)
                    {
                        player.LastWentDirection = "west";
                    }
                    else if (player.GoingRight)
                    {
                        player.LastWentDirection = "east";
                    }
                    else if (player.GoingUp)
                    {
                        player.LastWentDirection = "north";
                    }
                }

                if (e.KeyCode == Keys.Space)
                {
                    if (player.ammunition > 0)
                    {                        
                        player.IsShooting = true;
                        if (ShootingCoolDown == 0)
                        {                           
                            player.ammunition -= 1;
                            ammunitions.Add( new Ammunition(player.X, player.Y, player.LastWentDirection));                            
                            ShootingCoolDown = 15;
                        }
                    }
                }
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (gameOver)
                {
                    goToMenu = true;
                }
            }

            player.UpdateAnimationState();
            //this._form.Invalidate();
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                // player.Speed = player.NormalSpeed;
                player.IsRunning = false;
            }
            if (e.KeyCode == Keys.Space)
            {
                player.IsShooting = false;
            }
            if (e.KeyCode == Keys.S)
            {
                player.GoingDown = false;
            }
            if (e.KeyCode == Keys.W)
            {
                player.GoingUp = false;
            }
            if (e.KeyCode == Keys.A)
            {
                player.GoingLeft = false;
            }
            if (e.KeyCode == Keys.D)
            {
                player.GoingRight = false;
            }
            if (e.KeyCode == Keys.E)
            {               
                player.UsingKey = false;
                player.HasUsed = false;
            }
            if (e.KeyCode == Keys.Escape)
            {
                EscapeKeyIsPressed = false;
                EscapeKeyIsUsed = false;
            }
            player.UpdateAnimationState();
            //this._form.Invalidate();
        }

        private bool IsBlocked
        {
            get 
            {
                var isBlocked = false;
                foreach (Tile blocker in tiles.Where(t => t.IsBlocker))
                {
                    if (player.Rect(scaleFactor).IntersectsWith(blocker.Rect(scaleFactor)))
                    {
                        isBlocked = true;
                        break;
                    }
                }
                foreach(Door door in tiles.Where(t => t.Tag == "door"))
                {
                    if (player.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)) && door.IsClosed == true)
                    {
                        isBlocked = true;
                        break;
                    }
                }
                return isBlocked;
            }
        }
        public void MouseDown(object sender, MouseEventArgs e)
        {
        }

        public void Start(string startInfo = null)
        {
            int checkRow = levelTop.Length - 1;

            char lastLetter = 'z';
            Refresh();
            for (int row = 0; row < levelTop.Length; row++)
            {
                var higherLevelRow = levelTop[row];
                for (int column = 0; column < higherLevelRow.Length; column++)
                {
                    var letter = higherLevelRow[column];
                    int checkColumn = higherLevelRow.Length - 1;
                    switch (letter)
                    {
                        case 'W':
                            tiles.Add(new Water(column, row));
                            break;
                        case 'E':
                            tiles.Add(new Water(column, row));
                            break;
                        case 'L':
                            tiles.Add(new Block(column, row, Color.OrangeRed));
                            break;
                        case 'S':
                            tiles.Add(new Sand(column, row));
                            break;          
                        case 'Q':
                            tiles.Add(new DeepWater(column, row, Color.Navy));
                            break;
                        default:
                            tiles.Add(new Grass(column, row));
                            break;
                    }
                    switch (letter)
                    {
                        case 'P':
                            player = new Player(column, row);
                            break;
                        case 'V':
                            badguys.Add(new Badguy(column, row));
                            break;
                        case 'E':
                            tiles.Add(new Bridge(column, row, Color.Brown));
                            break;
                        case 'v':
                            badguys.Add(new Badguy(column, row, "follower"));
                            break;
                        case 'g':
                            golds.Add(new Gold(column, row));
                            break;
                        case 'T':
                            tiles.Add(new Tree(column, row));
                            break;
                        case 'D':
                            if(lastLetter == 'B')
                            tiles.Add(new Door(column, row, true));
                            else
                                tiles.Add(new Door(column, row, false));
                            break;
                        case 'O':
                            tiles.Add(new Border(column, row));
                            break;
                        case 'B':
                            tiles.Add(new Wall(column, row));
                            break;
                        

                    }
                    if(letter != 'O' && (row == 0 || column == 0 || row == checkRow || column == checkColumn))
                    {
                        tiles.Add(new Border(column, row));
                    }
                    lastLetter = letter;
                    
                }

            }


            foreach (Water water in tiles.Where(w => w.Tag == "water"))
            {
                foreach (Bridge bridge in tiles.Where(b => b.Tag == "bridge"))
                {
                    if (water.Rect(scaleFactor).IntersectsWith(bridge.Rect(scaleFactor)))
                    {
                        water.HasBridge = true;
                    }
                }

                bool GrassDown = false;
                bool GrassUp = false;
                bool GrassRight = false;
                bool GrassLeft = false;
                foreach (Tile grass in tiles.Where(w => (w.Tag == "grass")))
                {

                    if (water.CheckDownRect(scaleFactor).IntersectsWith(grass.Rect(scaleFactor)))
                    {
                        GrassDown = true;
                    }
                    if (water.CheckUpRect(scaleFactor).IntersectsWith(grass.Rect(scaleFactor)))
                    {
                        GrassUp = true;
                    }
                    if (water.CheckLeftRect(scaleFactor).IntersectsWith(grass.Rect(scaleFactor)))
                    {
                        GrassLeft = true;
                    }
                    if (water.CheckRightRect(scaleFactor).IntersectsWith(grass.Rect(scaleFactor)))
                    {
                        GrassRight = true;
                    }

                }

                if (GrassUp && GrassDown)
                {

                    if (GrassLeft && GrassRight)
                    {
                    }
                    else if (GrassLeft)
                    {

                    }
                    else if (GrassRight)
                    {

                    }
                    else
                    {
                        water.image = Resources.Image_WaterUpDown;
                    }
                }
                else
                {
                    if (GrassUp && GrassRight && GrassLeft)
                    {
                        water.image = Resources.Image_WaterLeftRightUp;
                    }
                    else if (GrassUp && GrassRight)
                    {

                    }
                    else if (GrassUp && GrassLeft)
                    {

                    }
                    else if (GrassUp)
                    {
                        water.image = Resources.Image_WaterUp;
                    }
                    else if (GrassDown && GrassRight && GrassLeft)
                    {
                        water.image = Resources.Image_WaterLeftRightDown;
                    }
                    else if (GrassDown && GrassRight)
                    {

                    }
                    else if (GrassDown && GrassLeft)
                    {

                    }
                    else if (GrassDown)
                    {
                        water.image = Resources.Image_WaterDown;
                    }
                    else if (GrassRight && GrassLeft)
                    {
                        water.image = Resources.Image_WaterLeftRight;
                    }
                    else if (GrassRight)
                    {
                        water.image = Resources.Image_WaterRight;
                    }
                    else if (GrassLeft)
                    {
                        water.image = Resources.Image_WaterLeft;
                    }

                }


            }


        }

        public void Stop()
        {
            
        }

        public void Tick()
        {
            if (gameOver == false && isPaused == false)
            {
                int aliveBadguys = 0;
                int amountOfGold = 0;
                if (player.IsAlive)
                {
                    player.AnimationTick();
                    

                    if (ShootingCoolDown > 0)
                        ShootingCoolDown--;
                    foreach (Badguy badguy in badguys)
                    {
                        if (badguy.IsDead == false)
                        {
                            badguy.AnimationTick();

                            aliveBadguys++;
                            badguy.Move(scaleFactor);
                            foreach (Tile blocker in tiles.Where(t => t.IsBlocker))
                            {
                                if (badguy.Rect(scaleFactor).IntersectsWith(blocker.Rect(scaleFactor)))
                                {
                                    badguy.Reverse();
                                    badguy.Move(scaleFactor);
                                }
                            }
                            foreach (Door door in tiles.Where(t => t.Tag == "door"))
                            {
                                if (badguy.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)))
                                {
                                    if (door.IsClosed == true)
                                    {
                                        door.IsClosed = false;
                                    }

                                }
                            }
                            foreach (Water water in tiles.Where(w => (w.Tag == "water")))
                            {
                                if (badguy.Rect(scaleFactor).IntersectsWith(water.Rect(scaleFactor)) && water.HasBridge == false)
                                {
                                    badguy.IsInWater = true;
                                    break;
                                }
                                else
                                    badguy.IsInWater = false;
                            }

                            if (badguy.Rect(scaleFactor).IntersectsWith(player.Rect(scaleFactor)))
                            {
                                player.IsAlive = false;
                                gameOver = true;
                            }
                            badguy.UpdateAnimationState();
                        }
                    }
                    foreach (Water water in tiles.Where(w => w.Tag == "water"))
                    {
                        if (player.WaterCheckRect(scaleFactor).IntersectsWith(water.Rect(scaleFactor)) && water.HasBridge == false)
                        {
                            player.IsInWater = true;
                            break;
                        }
                        else
                        {
                            player.IsInWater = false;
                        }

                    }
                    foreach (Ammunition ammunition in ammunitions)
                    {
                        ammunition.Move(scaleFactor);
                        foreach (Tile blocker in tiles.Where(t => t.IsBlocker))
                        {
                            if (ammunition.Rect(scaleFactor).IntersectsWith(blocker.Rect(scaleFactor)))
                            {
                                ammunition.IsDead = true;
                                break;
                            }
                        }
                        foreach (Door door in tiles.Where(t => t.Tag == "door"))
                        {

                            if (ammunition.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)) && door.IsClosed == true)
                            {
                                ammunition.IsDead = true;
                                break;
                            }

                        }
                        foreach (Badguy badguy in badguys.Where(b => !b.IsDead))
                        {
                            if (ammunition.Rect(scaleFactor).IntersectsWith(badguy.Rect(scaleFactor)))
                            {
                                ammunition.IsDead = true;
                                badguy.Health -= 3;
                                if (badguy.Health <= 0)
                                {
                                    badguy.IsDead = true;
                                    aliveBadguys--;
                                    _currentLevel.CurrentScore++;
                                }                            
                            }
                        }
                        if (ammunition.IsDead == true)
                        {
                            ammunitions.Remove(ammunition);
                            break;
                        }

                    }

                    AliveBadguys = aliveBadguys;
                    //AmountOfGold = amountOfGold;

                    if (player.UsingKey == true)
                    {
                        foreach (Door door in tiles.Where(t => t.Tag == "door"))
                        {                            
                            if (PlayerUseRect(scaleFactor).Contains(door.Rect(scaleFactor))&& !player.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)))
                            {
                                if (player.HasUsed == false)
                                {
                                    if (door.IsClosed == true)
                                    {
                                        door.IsClosed = false;
                                    }
                                    else
                                    {
                                        door.IsClosed = true;
                                    }
                                    player.HasUsed = true;                                   
                                }    
                            }
                            
                        }
                    }
                    

                    float currentX = player.X, currentY = player.Y;
                    if (player.GoingUp)
                    {
                        player.Y -= player.Speed;
                        if (IsBlocked) player.Y = currentY;
                    }
                    if (player.GoingDown)
                    {
                        player.Y += player.Speed;
                        if (IsBlocked) player.Y = currentY;
                    }
                    if (player.GoingRight)
                    {
                        player.X += player.Speed;
                        if (IsBlocked) player.X = currentX;
                    }
                    if (player.GoingLeft)
                    {
                        player.X -= player.Speed;
                        if (IsBlocked) player.X = currentX;
                    }
                    foreach (Gold gold in golds.Where(t => t.IsPickedUp == false))
                    {
                        amountOfGold++;
                        if (player.Rect(scaleFactor).IntersectsWith(gold.Rect(scaleFactor)))
                        {
                            gold.IsPickedUp = true;
                            _currentLevel.CurrentScore += 5;
                        }
                    }
                    if (goal == "Elimination" && AliveBadguys == 0)
                    {
                        _currentLevel.IsWon = true;
                        if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                            _currentLevel.HighScore = _currentLevel.CurrentScore;
                        gameOver = true;
                    }
                    else if(goal == "Treasure Hunt" && amountOfGold == 0)
                    {
                        _currentLevel.IsWon = true;
                        if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                            _currentLevel.HighScore = _currentLevel.CurrentScore;
                        gameOver = true;
                    }
                    //if(_currentLevel.IsWon == true)
                    //{
                    //    if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                    //        _currentLevel.HighScore = _currentLevel.CurrentScore;
                    //    gameOver = true;
                    //}
                }
            }
            if (EscapeKeyIsPressed == true)
            {

                if (EscapeKeyIsUsed == false)
                {
                    if (isPaused == true)
                    {
                        isPaused = false;
                    }
                    else
                    {
                        isPaused = true;
                    }
                    EscapeKeyIsUsed = true;

                }
            }
            _form.Invalidate();
        }

        public void DrawScaledTiles(Graphics g, SolidBrush brush, Bitmap image, int x = 0, int y = 0, float xFloat = 100, float yFloat = 0, float imgH = 20, float imgW = 20, float scale = 1.0f)
        {

            if (xFloat == 100)
            {
                xFloat = x;
                yFloat = y;
            }
            var imageH = imgH * scale;
            var imageW = imgW * scale;
            var imageX = xFloat * 20 * scale;
            var imageY = yFloat * 20 * scale;
            if (brush.Color == Color.Black)
            {
                g.DrawImage(image, imageX, imageY, imageW, imageH);
            }
            else if (brush.Color == Color.White)
            {

            }
            else
            {
                g.FillRectangle(brush, imageX, imageY, imageW, imageH);
            }
        }

        public void DrawScaledTiles(Graphics g, IDrawable t)
        {
            if (t.Brush.Color == Color.Black)
            {
                g.DrawImage(t.Image, t.Rect(ScaleFactor));
            }
            else if (t.Brush.Color == Color.White)
            {

            }
            else
            {
                g.FillRectangle(t.Brush, t.Rect(ScaleFactor));
            }
        }

        
        private void Refresh()
        {
            waters.Clear();
            blocks.Clear();
            badguys.Clear();
            tiles.Clear();
            golds.Clear();
            ammunitions.Clear();
            _currentLevel.CurrentScore = 0;
            gameOver = false;
            isPaused = false;
        }

    }
}
