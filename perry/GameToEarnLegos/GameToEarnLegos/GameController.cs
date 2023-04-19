﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GameToEarnLegos.Badguys;
using GameToEarnLegos.Tiles;

namespace GameToEarnLegos
{
    public class GameController : IGameController
    {
        private Random random = new Random();
        private const float TileSize = 20f;
        private float scaleFactor = 0.8f;
        private FormTriangleTrees _form;
        private bool gameOver = false;
        private bool isPaused = false;
        private bool EscapeKeyIsUsed = false;
        public bool goToMenu = false;
        private bool EscapeKeyIsPressed = false;
        Badguy Boss = null;

        public bool CHEATS => _form.CHEATS;
        public float ScaleFactor => scaleFactor;
        private ILevel _currentLevel => _form.currentLevel;

        int AliveBadguys;
        int BurningTrees;
        int AliveTrees;
        int NeededTrees;
        Player player;
        List<Water> waters = new List<Water>();
        List<DeepWater> deepWaters = new List<DeepWater>();
        List<Block> blocks = new List<Block>();
        List<Badguy> badguys = new List<Badguy>();
        List<Tile> tiles = new List<Tile>();
        List<Gold> golds = new List<Gold>();
        List<AmmoPack> ammoPacks = new List<AmmoPack>();
        List<Door> doors = new List<Door>();
        List<Badguy> Bosses = new List<Badguy>();
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
            foreach (Tile tile in tiles.Where(t=>  (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))) && t.Tag != "door" && t.Tag != "border"))
            {                       
                DrawScaledTiles(g, tile);
            }

            foreach (Water water in waters.Where(t => (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor)))))
            {
                DrawScaledTiles(g, water);
            }
            foreach (Border border in tiles.Where(t => (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))) && t.Tag == "border"))
            {
                DrawScaledTiles(g, border);
            }
            foreach (Gold gold in golds.Where(t => (t.IsPickedUp == false) && (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor)))))
            {
                DrawScaledTiles(g, gold);
            }
            foreach (AmmoPack ammopack in ammoPacks.Where(t => (t.IsPickedUp == false) && (SeenRect(scaleFactor).Contains(t.Rect(scaleFactor)))))
            {
                DrawScaledTiles(g, ammopack);
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
            foreach(Badguy boss in Bosses.Where(t => SeenRect(scaleFactor).Contains(t.Rect(scaleFactor))))
            {
                DrawScaledTiles(g, boss);
            }
            if (CHEATS)
            {
                g.DrawString($"IsRunning:{player.IsRunning} isEscape {EscapeKeyIsUsed} IsInWater:{player.IsInWater} IsShooting:{player.IsShooting} " +
                    $"Ammo: {player.CurrentTypeOfAmmo} Water: {player.WAmmo} Normal Ammo: {player.NAmmo} Score: {_currentLevel.CurrentScore}/{_currentLevel.Score} Badguys: {AliveBadguys} Health: {player.Health} HasUsed: {player.HasUsed} UsingKey: {player.UsingKey}",
                    SystemFonts.DefaultFont, Brushes.LightGray, 5, 5);
            }
            else if(goal == "Extinguish")
            {
                g.DrawString($" Health: {player.Health} Kind of Ammo: {player.CurrentTypeOfAmmo} Water: {player.WAmmo} Weapon: {player.NAmmo} Score {_currentLevel.CurrentScore}/{_currentLevel.Score} Badguys: {AliveBadguys} Alive Trees: {AliveTrees} Burning Trees: {BurningTrees} Trees Needed Alive: {NeededTrees}",
                   SystemFonts.DefaultFont, Brushes.LightGray, 5, 5);
            }
            else
            {
                g.DrawString($" Health: {player.Health} Kind of Ammo: {player.CurrentTypeOfAmmo} Water: {player.WAmmo} Weapon: {player.NAmmo} Score {_currentLevel.CurrentScore}/{_currentLevel.Score} Badguys: {AliveBadguys}",
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
                    if (e.KeyCode == Keys.R)
                    {

                        player.UsingRefillKey = true;
                    }
                    if (e.KeyCode == Keys.D1)
                    {
                        player.CurrentTypeOfAmmo = "normal";
                    }
                    if (e.KeyCode == Keys.D2)
                    {
                        player.CurrentTypeOfAmmo = "water";
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
                    if (player.CurrentTypeOfAmmo == "normal")
                    {
                        if (player.NAmmo > 0)
                        {
                            player.IsShooting = true;
                            if (ShootingCoolDown == 0)
                            {
                                player.NAmmo -= 1;
                                ammunitions.Add(new Ammunition(player.X, player.Y, player.LastWentDirection, "normal"));
                                ShootingCoolDown = 10;
                            }
                        }
                    }
                    else if(player.CurrentTypeOfAmmo == "water")
                    {
                        if (player.WAmmo > 0)
                        {
                            player.IsShooting = true;
                            if (ShootingCoolDown == 0)
                            {
                                player.WAmmo -= 1;
                                ammunitions.Add(new Ammunition(player.X, player.Y, player.LastWentDirection, "water"));
                                ShootingCoolDown = 10;
                            }
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
            if (e.KeyCode == Keys.R)
            {
                player.UsingRefillKey = false;
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
                            waters.Add(new Water(column, row));
                            break;
                        case 'E':
                            waters.Add(new Water(column, row));
                            break;
                        case 'L':
                            tiles.Add(new Block(column, row, Color.OrangeRed));
                            break;
                        case 'S':
                            tiles.Add(new Sand(column, row, "normal"));
                            break;          
                        case 'Q':
                            tiles.Add(new DeepWater(column, row));
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
                            badguys.Add(new Deer(column, row,1));
                            break;
                        case 'E':
                            tiles.Add(new Water(column, row));
                            if (lastLetter == 'W' || lastLetter == 'Q')
                                tiles.Add(new Bridge(column, row, false));
                            else
                                tiles.Add(new Bridge(column, row, true));
                            break;
                        case 'v':
                            badguys.Add(new Deer(column, row, 2));
                            break;
                        case 't':
                            badguys.Add(new FlameTower(column, row));
                            break;
                        case '0':
                            badguys.Add(new DeerKing(column, row));
                            break;
                        case '1':
                            Bosses.Add(new Dragon(column, row));
                            break;
                        case 'g':
                            golds.Add(new Gold(column, row));
                            break;
                        case 'A':
                            ammoPacks.Add(new AmmoPack(column, row));
                            break;
                        case 'T':
                            tiles.Add(new Tree(column, row));
                            break;
                        case 'F':
                            tiles.Add(new Tree(column, row, 'F'));
                            break;
                        case 'f':
                            tiles.Add(new Tree(column, row, 'f'));
                            break;
                        case 'D':
                            if(lastLetter == 'B')
                            tiles.Add(new Door(column, row, true,'N'));
                            else
                                tiles.Add(new Door(column, row, false, 'N'));
                            break;
                        case 'd':
                            if (lastLetter == 'B')
                                tiles.Add(new Door(column, row, true, 'B'));
                            else
                                tiles.Add(new Door(column, row, false, 'B'));
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

            foreach (Tree tree in tiles.Where(t => t.Tag == "tree"))
            {
                AliveTrees++;
            }
            decimal d = AliveTrees / 3;
            NeededTrees = ((int)Math.Round(d));
            foreach (Water water in waters.Where(w => w.Tag == "water"))
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
                bool SandDown = false;
                bool SandUp = false;
                bool SandRight = false;
                bool SandLeft = false;
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
                foreach (Sand sand in tiles.Where(w => w.Tag == "sand"))
                {
                    if (sand.Kind == "normal")
                    {
                        if (water.CheckDownRect(scaleFactor).IntersectsWith(sand.Rect(scaleFactor)))
                        {
                            SandDown = true;
                        }
                        if (water.CheckUpRect(scaleFactor).IntersectsWith(sand.Rect(scaleFactor)))
                        {
                            SandUp = true;
                        }
                        if (water.CheckLeftRect(scaleFactor).IntersectsWith(sand.Rect(scaleFactor)))
                        {
                            SandLeft = true;
                        }
                        if (water.CheckRightRect(scaleFactor).IntersectsWith(sand.Rect(scaleFactor)))
                        {
                            SandRight = true;
                        }
                    }
                }

                if (GrassUp && GrassDown)
                {

                    if (GrassLeft && GrassRight)
                    {
                        //  Not  Added  Yet  !!!!!
                        //water.image = Resources.Image_WaterTopBottomLeftRight;
                    }
                    else if (GrassLeft)
                    {
                        water.image = Resources.Image_WaterTopBottomLeft;
                    }
                    else if (GrassRight)
                    {
                        water.image = Resources.Image_WaterTopBottomRight;
                    }
                    else
                    {
                        water.image = Resources.Image_WaterTopBottom;
                    }
                }
                else
                {
                    if (GrassUp && GrassRight && GrassLeft)
                    {
                        water.image = Resources.Image_WaterTopLeftRight;
                    }
                    else if (GrassUp && GrassRight)
                    {
                        water.image = Resources.Image_WaterTopRight;
                    }
                    else if (GrassUp && GrassLeft)
                    {
                        water.image = Resources.Image_WaterTopLeft;
                    }
                    else if (GrassUp)
                    {
                        water.image = Resources.Image_WaterTop;
                    }
                    else if (GrassDown && GrassRight && GrassLeft)
                    {
                        water.image = Resources.Image_WaterBottomLeftRight;
                    }
                    else if (GrassDown && GrassRight)
                    {
                        water.image = Resources.Image_WaterBottomRight;
                    }
                    else if (GrassDown && GrassLeft)
                    {
                        water.image = Resources.Image_WaterBottomLeft;
                    }
                    else if (GrassDown)
                    {
                        water.image = Resources.Image_WaterBottom;
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
                if (SandDown || SandLeft || SandRight || SandUp)
                {
                    int col = Convert.ToInt32(water.X) / 20;
                    int row = Convert.ToInt32(water.Y) / 20;
                    tiles.Add(new Sand(col, row, "water"));
                    if (SandUp && SandDown)
                    {

                        if (SandLeft && SandRight)
                        {
                            //  Not  Added  Yet  !!!!!
                            //water.image = Resources.Image_WaterTopBottomLeftRight;
                        }
                        else if (SandLeft)
                        {
                            water.image = Resources.Image_WaterLeftBlank;
                        }
                        else if (SandRight)
                        {
                            water.image = Resources.Image_WaterRightBlank;
                        }
                        else
                        {
                            water.image = Resources.Image_WaterTopBottomBlank;
                        }
                    }
                    else
                    {
                        if (SandUp && SandRight && SandLeft)
                        {
                            water.image = Resources.Image_WaterTopLeftRightBlank;
                        }
                        else if (SandUp && SandRight)
                        {
                            water.image = Resources.Image_WaterTopRightBlank;
                        }
                        else if (SandUp && SandLeft)
                        {
                            water.image = Resources.Image_WaterTopLeftBlank;
                        }
                        else if (SandUp)
                        {
                            water.image = Resources.Image_WaterTopBlank;
                        }
                        else if (SandDown && SandRight && SandLeft)
                        {
                            water.image = Resources.Image_WaterBottomLeftRightBlank;
                        }
                        else if (SandDown && SandRight)
                        {
                            water.image = Resources.Image_WaterBottomRightBlank;
                        }
                        else if (SandDown && SandLeft)
                        {
                            water.image = Resources.Image_WaterBottomLeftBlank;
                        }
                        else if (SandDown)
                        {
                            water.image = Resources.Image_WaterBottomBlank;
                        }
                        else if (SandRight && SandLeft)
                        {
                            water.image = Resources.Image_WaterLeftRightBlank;
                        }
                        else if (SandRight)
                        {
                            water.image = Resources.Image_WaterRightBlank;
                        }
                        else if (SandLeft)
                        {
                            water.image = Resources.Image_WaterLeftBlank;
                        }

                    }
                }
            }
            if(goal == "Extinguish")
            {
                player.WAmmo = player.MaxWAmmo;
            }
            if (CHEATS)
            {
                player.Health = 9999;
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
                int aliveBosses = 0;

                int amountOfBurningTrees = 0;
                int amountOfAliveTrees = 0;
                
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

                            if(badguy.canShoot == true)
                            {
                                if (badguy.CheckIfNoticed(player) && badguy.ShootingCoolDown == 0) 
                                {
                                    ammunitions.Add(badguy.Shoot(player));
                                    badguy.ShootingCoolDown = badguy.BaseShootingCoolDown;
                                }
                                else if(badguy.ShootingCoolDown > 0)
                                {
                                    badguy.ShootingCoolDown -= 1;
                                }
                            }

                            if(badguy.IsBoss == true)
                            {
                                aliveBosses++;
                                Boss = badguy;
                            }

                            aliveBadguys++;
                            if (badguy.canMove == true)
                            {
                                badguy.Move(scaleFactor, player);
                                foreach (Tile blocker in tiles.Where(t => t.IsBlocker))
                                {
                                    if (badguy.Rect(scaleFactor).IntersectsWith(blocker.Rect(scaleFactor)))
                                    {

                                        if (badguy.isFollowing || badguy.isWanderer == false)
                                        {
                                            badguy.RevertMove();
                                        }
                                        else
                                        {
                                            badguy.Reverse();
                                            badguy.Move(scaleFactor, player);
                                        }
                                    }
                                }

                                foreach (Door door in tiles.Where(t => t.Tag == "door"))
                                {
                                    if (door.isBossDoor == false)
                                    {
                                        if (badguy.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)))
                                        {
                                            if (door.IsClosed == true)
                                            {
                                                door.IsClosed = false;
                                            }

                                        }
                                    }

                                    if (door.IsClosed == false && badguy.IsBoss && badguy.NoticedPlayer)
                                    {
                                        door.IsClosed = true;
                                    }

                                }
                                foreach (Water water in waters.Where(w => (w.Tag == "water")))
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
                                    
                                }
                                badguy.UpdateAnimationState();
                            }
                        }
                    }
                    foreach (Badguy boss in Bosses.Where(b=> b.IsDead == false))
                    {
                        aliveBosses++;
                        aliveBadguys++;
                        if (boss.canShoot == true)
                        {
                            if(boss.BossCoolDown >= 1)
                            {
                                boss.BossCoolDown--;
                            }
                            else if (boss.CheckIfNoticed(player) && boss.ShootingCoolDown == 0)
                            {
                                ammunitions.Add(boss.Shoot(player));
                                boss.ShootingCoolDown = boss.BaseShootingCoolDown;
                                boss.BossCoolDown = 15;
                            }
                            else if (boss.ShootingCoolDown > 0)
                            {
                                boss.ShootingCoolDown --;

                                boss.Move(scaleFactor, player);
                                foreach (Border blocker in tiles.Where(t => t.Tag == "border"))
                                {
                                    if (boss.Rect(scaleFactor).IntersectsWith(blocker.Rect(scaleFactor)))
                                    {

                                        if (boss.isFollowing || boss.isWanderer == false)
                                        {
                                            boss.RevertMove();
                                        }
                                        else
                                        {
                                            boss.Reverse();
                                            boss.Move(scaleFactor, player);
                                        }
                                    }
                                }
                                boss.UpdateAnimationState();
                            }
                        }
                        if (boss.Rect(scaleFactor).IntersectsWith(player.Rect(scaleFactor)))
                        {
                            player.IsAlive = false;
                        }

                    }
                    foreach (Water water in waters)
                    {
                        if (player.WaterCheckRect(scaleFactor).IntersectsWith(water.Rect(scaleFactor)) && water.HasBridge == false)
                        {
                            player.IsInWater = true;
                            if (player.IsOnFire)
                                player.IsOnFire = false;
                            if(player.UsingRefillKey == true)
                            {
                                player.WAmmo = player.MaxWAmmo;
                            }
                            break;
                        }
                        else
                        {
                            player.IsInWater = false;
                        }

                    }
                    foreach (Tree tree in tiles.Where(w => w.Tag == "tree"))
                    {
                        if(tree.isOnFire == true)
                        {

                            tree.health--;
                            if(tree.health <= 0)
                            {
                                tree.isDead = true;
                                tree.image = tree.DeadImage;
                            }
                            else
                            {
                                if(tree.image != tree.OnFireImage)
                                {
                                    tree.image = tree.OnFireImage;
                                }
                                amountOfAliveTrees++;
                                amountOfBurningTrees++;
                            }
                            foreach(Tree otherTree in tiles.Where(t => t.Tag == "tree" && t != tree && t.Rect(scaleFactor).IntersectsWith(tree.CheckAroundRect(scaleFactor))))
                            {
                                int rand = random.Next(1000);
                                
                                if(rand == 10)
                                {
                                    otherTree.isOnFire = true;
                                }
                            }

                        }
                        else if(tree.isDead == false)
                        {
                            amountOfAliveTrees++;
                        }
                    }
                    foreach (Ammunition ammunition in ammunitions)
                    {
                        ammunition.Move(scaleFactor);                        
                        foreach(Tree tree in tiles.Where(t => t.Tag == "tree"))
                        {
                            if (ammunition.BadguyAmmo)
                            {
                                if (ammunition.Rect(scaleFactor).IntersectsWith(tree.Rect(scaleFactor)))
                                {
                                    if(tree.isOnFire == false && tree.isDead == false)
                                    {
                                        tree.isOnFire = true;
                                        tree.image = tree.OnFireImage;
                                    }
                                    
                                }
                            }
                            else if(ammunition.TypeOfAmmo == "water")
                            {
                                if (ammunition.Rect(scaleFactor).IntersectsWith(tree.Rect(scaleFactor)))
                                {
                                    if (tree.isOnFire && tree.isDead == false)
                                    {
                                        tree.isOnFire = false;
                                        tree.image = tree.AliveImage;
                                    }
                                    
                                }
                            }



                        }
                        if (ammunition.IsDead == false)
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
                        if (ammunition.BadguyAmmo == false)
                        {
                            foreach (Badguy badguy in badguys.Where(b => !b.IsDead))
                            {

                                if (ammunition.Rect(scaleFactor).IntersectsWith(badguy.Rect(scaleFactor)))
                                {
                                    ammunition.IsDead = true;
                                    if (ammunition.TypeOfAmmo == "normal" || (badguy.typeOfBadguy == "tower" && ammunition.TypeOfAmmo == "water"))
                                    {                                        
                                        badguy.Health -= ammunition.Damage;
                                    }
                                    else if(badguy.typeOfBadguy != "tower" && ammunition.TypeOfAmmo == "water")
                                    {
                                        badguy.Health -= 1;
                                    }
                                    if (badguy.Health <= 0)
                                    {
                                        badguy.IsDead = true;
                                        aliveBadguys--;
                                        if (badguy.IsBoss)
                                        {
                                            _currentLevel.CurrentScore += 10;
                                        }
                                        else
                                        {
                                            _currentLevel.CurrentScore++;
                                        }
                                    }
                                }
                            }
                            foreach (Badguy badguy in Bosses.Where(b => !b.IsDead))
                            {

                                if (ammunition.Rect(scaleFactor).IntersectsWith(badguy.Rect(scaleFactor)))
                                {
                                    ammunition.IsDead = true;
                                    if (ammunition.TypeOfAmmo == "normal")
                                    {
                                        badguy.Health -= ammunition.Damage;
                                    }
                                    else if (ammunition.TypeOfAmmo == "water")
                                    {
                                        badguy.Health -= 1;
                                    }
                                    if (badguy.Health <= 0)
                                    {
                                        badguy.IsDead = true;
                                        aliveBosses--;
                                        _currentLevel.CurrentScore += 15;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ammunition.Rect(scaleFactor).IntersectsWith(player.Rect(scaleFactor)))
                            {
                                ammunition.IsDead = true;
                                if(player.IsOnFire == false)
                                {
                                    player.IsOnFire = true;
                                    player.FireCoolDown = 10;
                                }
                                player.Health-= 3;
                                
                            }
                        }
                        if (ammunition.IsDead == true)
                        {
                            ammunitions.Remove(ammunition);
                            break;
                        }

                    }

                    AliveBadguys = aliveBadguys;
                    AliveTrees = amountOfAliveTrees;
                    BurningTrees = amountOfBurningTrees;

                    //AmountOfGold = amountOfGold;

                    if (player.UsingKey == true)
                    {
                        foreach (Door door in tiles.Where(t => t.Tag == "door"))
                        {
                            if (!(door.isBossDoor == true && Boss.NoticedPlayer == true && Boss.IsDead == false))
                            {
                                if (PlayerUseRect(scaleFactor).Contains(door.Rect(scaleFactor)) && !player.Rect(scaleFactor).IntersectsWith(door.Rect(scaleFactor)))
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
                    }
                    if (player.IsOnFire && player.FireCoolDown == 0)
                    {
                        player.Health--;
                        player.FireCoolDown = player.BaseCoolDown;
                    }
                    else
                    {
                        player.FireCoolDown--;

                    }
                    if (player.Health <= 0)
                    {
                        player.Health = 0;
                        player.IsAlive = false;
                    }
                    if (player.IsAlive == false)
                    {
                        gameOver = true;
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
                    foreach (AmmoPack ammoPack in ammoPacks.Where(t => t.IsPickedUp == false))
                    {
                        if (player.Rect(scaleFactor).IntersectsWith(ammoPack.Rect(scaleFactor)))
                        {
                            ammoPack.IsPickedUp = true;
                            player.NAmmo += ammoPack.ammountOfAmmo;
                        }
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
                    else if(goal == "Completion" && AliveBadguys == 0 && amountOfGold == 0)
                    {
                        _currentLevel.IsWon = true;
                        if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                            _currentLevel.HighScore = _currentLevel.CurrentScore;
                        gameOver = true;
                    }
                    else if (goal == "Extinguish")
                    {
                        if(AliveTrees < NeededTrees)
                        {
                            gameOver = true;
                        }
                        else if(BurningTrees == 0 && AliveBadguys == 0)
                        {
                            _currentLevel.IsWon = true;
                            if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                                _currentLevel.HighScore = _currentLevel.CurrentScore;
                            gameOver = true;
                        }
                    }
                    else if (goal == "?????????" && aliveBosses == 0)
                    {
                        _currentLevel.IsWon = true;
                        if (_currentLevel.HighScore < _currentLevel.CurrentScore)
                            _currentLevel.HighScore = _currentLevel.CurrentScore;
                        gameOver = true;
                    }
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

            //RectangleF drawnRect;
            //if (t != player)
            //{
            //    if(t is Tile)
            //        drawnRect = new RectangleF((963/2+t.X) * scaleFactor, (543/2+t.Y) * scaleFactor, (20) * scaleFactor, (20) * scaleFactor);
            //    else
            //        drawnRect = new RectangleF((player.X / 2-t.X) * scaleFactor, (player.Y / 2-t.Y) * scaleFactor, (20) * scaleFactor, (20) * scaleFactor);
            //    if (t.Brush.Color == Color.Black)
            //    {
            //        g.DrawImage(t.Image, drawnRect);
            //    }
            //    else if (t.Brush.Color == Color.White)
            //    {

            //    }
            //    else
            //    {
            //        g.FillRectangle(t.Brush, drawnRect);
            //    }
            //}
            //else
            //{
               
            //    //963, 543
            //    g.DrawImage(t.Image, player.StationedRect(ScaleFactor));

            //}

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
            Bosses.Clear();
            tiles.Clear();
            golds.Clear();
            ammunitions.Clear();
            ammoPacks.Clear();
            AliveTrees = 0;
            _currentLevel.CurrentScore = 0;
            gameOver = false;
            isPaused = false;
        }

    }
}
