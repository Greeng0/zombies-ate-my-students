using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Text;
using Entities;

namespace zombies
{
    class HUD : DrawableGameComponent
    {
        private static HUD activeHUD = null;
        private ContentManager _content;
        GraphicsDevice device;

        // Screen Writing
        SpriteFont Font1;

        SpriteBatch spriteBatch;
        private int windowH;
        private int windowW;
        private int fps;
        private int frames;
        private double lasttime;
        public Vector3 p;
        public float angle;

        //calculating scale of blood for hud info
        public int min = 13;
        public int max = 290;
        public float rate = 2.73f;
        public int playerhealth = 100;

        public float healthscale = 273;
        public int healthx = 40;
        public int healthsizex = 30;

        //in game hud pictures

        //textures
        Texture2D fire;
        Texture2D gun;
        Texture2D gun2;
        Texture2D med;
        Texture2D health;
        Texture2D keys;
        Texture2D powerup;
        Texture2D healthbar;
        
        //slots holding texures
        Texture2D slot1;
        Texture2D slot2;
        Texture2D slot3;
        Texture2D slot4;
        Texture2D slot5;

        //adding their rectangles

        Rectangle slot1rec = new Rectangle(150, 10, 70, 70);
        Rectangle slot2rec = new Rectangle(270, 10, 70, 70);
        Rectangle slot3rec = new Rectangle(390, 10, 70, 70);
        Rectangle slot4rec = new Rectangle(510, 10, 70, 70);
        Rectangle slot5rec = new Rectangle(630, 10, 70, 70);

        int fires = 0;
        int meds = 0;
        int keyss = 0;
    
        //powerups
        Texture2D silencer;
        Texture2D shoes;

        Rectangle silencerrec = new Rectangle(750, 0, 70, 70);
        Rectangle shoerec = new Rectangle(750, 70, 70, 70);

        bool drawsilencer = false;
        bool drawshoes = false;

        bool drawselectedwep = false;
        Texture2D selectedwep;
        Rectangle selectedweprec = new Rectangle(0, 0, 90, 90);

        bool drawselectedeq = false;
        Texture2D selectedeq;
        Rectangle selectedeqrec = new Rectangle(0, 0, 90, 90);

        public int selecteditem = 0;

        public bool useweapons = false;
        Texture2D temp;
        Texture2D letter;
        Texture2D a;
        Texture2D b;
        Texture2D c;
        Texture2D d;
        Texture2D e;
        Texture2D f;

        Texture2D empty;

        Texture2D died;
        Texture2D escaped;

        float FadeValue = 0;

        public static HUD ActiveHUD
        {
            get { return activeHUD; }
            set { activeHUD = value; }
        }

        public HUD(Game game, ContentManager content, GraphicsDeviceManager graphics)
            : base(game)
        {
            _content = content;
            device = graphics.GraphicsDevice;

            windowW = graphics.PreferredBackBufferWidth;
            windowH = graphics.PreferredBackBufferHeight;

            if (ActiveHUD == null)
                ActiveHUD = this;
        }


        public void ContentLoad()
        {
            spriteBatch = new SpriteBatch(device);
            Font1 = _content.Load<SpriteFont>("Arial");
            empty = _content.Load<Texture2D>("empty");
            fire = _content.Load<Texture2D>("fireext");
            gun = _content.Load<Texture2D>("gun");
            gun2 = _content.Load<Texture2D>("gun2");
            health = _content.Load<Texture2D>("health");
            keys = _content.Load<Texture2D>("keys");
            powerup = _content.Load<Texture2D>("powerup");
            med = _content.Load<Texture2D>("med");
            healthbar = _content.Load<Texture2D>("healthbar");
            died = _content.Load<Texture2D>("Died");
            escaped = _content.Load<Texture2D>("Escaped");

            //for letters 
            a = _content.Load<Texture2D>("a");
            b = _content.Load<Texture2D>("b");
            c = _content.Load<Texture2D>("c");
            d = _content.Load<Texture2D>("d");
            e = _content.Load<Texture2D>("e");
            f = _content.Load<Texture2D>("f");

            letter = a;

            selectedwep = _content.Load<Texture2D>("selectedweapon");
            selectedeq = _content.Load<Texture2D>("selecteditem");

            //fill slots with inventory
            slot1 = gun;
            slot2 = gun2;
            slot3 = fire;
            slot4 = keys;
            slot5 = med;

            fires = 0;
            meds = 0;
            keyss = 0;

            silencer = _content.Load<Texture2D>("powerup");
            shoes = _content.Load<Texture2D>("shoe");

            base.LoadContent();
        }

        protected override void LoadContent()
        {
            ContentLoad();
        }
        public void chooseslots(ref Entities.Hero p)
        {
            // choosing weapons
            drawselectedwep = true;
            if (p.WeaponsList.Count == 2)//if have both weapons need to decide which one to draw green around
            {
                //draw both pictures
                slot1 = gun;
                slot2 = gun2;

                if (p.EquippedWeapon.weaponType == Entities.WeaponType.Handgun9mm)
                {
                    selectedweprec.X = slot1rec.X - 10;
                    selectedweprec.Y = slot1rec.Y - 10;
                }
                else if (p.EquippedWeapon.weaponType == Entities.WeaponType.Magnum)
                {
                    //inset green outline on second slot
                    selectedweprec.X = slot2rec.X - 10;
                    selectedweprec.Y = slot2rec.Y - 10;
                }
            }

            else if (p.WeaponsList.Count == 1)
            {
                //draw one pictures

                if (p.EquippedWeapon.weaponType == Entities.WeaponType.Handgun9mm)
                {
                    selectedweprec.X = slot1rec.X - 10;
                    selectedweprec.Y = slot1rec.Y - 10;
                    slot1 = gun;
                    slot2 = empty;

                }
                else if (p.EquippedWeapon.weaponType == Entities.WeaponType.Magnum)
                {
                    slot1 = empty;
                    slot2 = gun2;
                    selectedweprec.X = slot2rec.X - 10;
                    selectedweprec.Y = slot2rec.Y - 10;
                }
            }
            else
            {
                slot1 = empty;
                slot2 = empty;
                selectedweprec.X = slot1rec.X - 10;
                selectedweprec.Y = slot1rec.Y - 10;
                drawselectedwep = false;
            }
            //end of choosing weapons

            //choosing items
            if (p.ItemsList.ContainsKey(new Item(ItemType.Extinguisher)))
            {
                fires = p.ItemsList[new Item(ItemType.Extinguisher)];
            }
            if (p.ItemsList.ContainsKey(new Item(ItemType.MedPack)))
            {
                meds = p.ItemsList[new Item(ItemType.MedPack)];
            }
            if (p.ItemsList.ContainsKey(new Item(ItemType.Key)))
            {
                keyss = p.ItemsList[new Item(ItemType.Key)];
            }

            int current = p.current;
            if (fires + meds + keyss > 0)
                drawselectedeq = true;
            else 
                drawselectedeq = false;

            if (fires > 0)
            {
                if (current % 3 == 0)
                {
                    selectedeqrec.X = slot3rec.X - 10;
                    selectedeqrec.Y = slot3rec.Y - 10;
                }
                slot3 = fire;
            }
            else
            {
                slot3 = empty;
            }
                
            if (keyss > 0)
            {
                if (current % 3 == 1 || (meds == 0 && current == 2))
                {
                    selectedeqrec.X = slot4rec.X - 10;
                    selectedeqrec.Y = slot4rec.Y - 10;
                }
                slot4 = keys;
            }
            else
            {
                slot4 = empty;
            }
            if (meds > 0)
            {
                if (current % 3 == 2 || (keyss == 0 && current == 1))
                {
                    selectedeqrec.X = slot5rec.X - 10;
                    selectedeqrec.Y = slot5rec.Y - 10;
                }
                slot5 = med;
            }
            else
            {
                slot5 = empty;
            }
            
            //choose to draw special weapons
            if (p.PowerupsList.Count >= 2)
            {
                //draw both

                drawshoes = true;
                drawsilencer = true;
            }
            else if (p.PowerupsList.Count == 1)
            {
                //find out which to draw
                if (p.PowerupsList.First().Type == Entities.PowerupType.Silencer)
                {
                    drawsilencer = true;
                    drawshoes = false;
                }
                else
                {
                    drawshoes = true;
                    drawsilencer = false;
                }
            }
            else{
                drawshoes = false;
                drawsilencer = false;
            }
        }


        public override void Update(GameTime gameTime)
        {
            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.InGame)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds > lasttime + 1000)
                {
                    fps = frames;
                    frames = 0;
                    lasttime = gameTime.TotalGameTime.TotalMilliseconds;
                }
                //update letter

                if (playerhealth > 75)
                    letter = a;
                else if (playerhealth > 50)
                    letter = b;
                else if (playerhealth > 25)
                    letter = c;
                else if (playerhealth > 15)
                    letter = d;
                else
                    letter = f;
            }
            
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.End)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                spriteBatch.Draw(escaped, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight), new Color(255, 255, 255, FadeValue));
                spriteBatch.End();

                FadeValue += 0.01f;
            }

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.InGame)
            {
                frames++;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                string out2 = "        ";

                healthscale = rate * playerhealth;
                int diff = min + (int)(rate * (100 - playerhealth));

                //draw health bar
                spriteBatch.Draw(healthbar, new Rectangle(healthx, diff, healthsizex, max - diff), Color.White);
                spriteBatch.Draw(health, new Rectangle(0, 0, 100, 400), Color.White);
                spriteBatch.Draw(letter, new Rectangle(0, 276, 100, 100), Color.White);

                //numbers


                out2 += "    ";
                if (keyss > 0)
                    out2 += keyss;
                out2 += "            ";
                if (meds > 0)
                    out2 += meds;

                Vector2 pos2 = new Vector2(400, 100);
                spriteBatch.DrawString(Font1, out2, pos2, Color.Red, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);


                //draw slots

                spriteBatch.Draw(slot1, slot1rec, Color.White);
                spriteBatch.Draw(slot2, slot2rec, Color.White);
                spriteBatch.Draw(slot3, slot3rec, Color.White);
                spriteBatch.Draw(slot4, slot4rec, Color.White);
                spriteBatch.Draw(slot5, slot5rec, Color.White);

                if(playerhealth <=0)
                    spriteBatch.Draw(died, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight), Color.White);

                if (drawselectedwep)
                {
                    spriteBatch.Draw(selectedwep, selectedweprec, Color.White);
                }

                if (drawselectedeq)
                {
                    temp = selectedeq;
                }
                else
                {
                    temp = empty;
                }

                spriteBatch.Draw(temp, selectedeqrec, Color.White);

                //draw powerups
                if (drawsilencer)
                    temp = silencer;
                else
                    temp = empty;
                spriteBatch.Draw(temp, silencerrec, Color.White);

                if (drawshoes)
                    temp = shoes;
                else
                    temp = empty;
                spriteBatch.Draw(temp, shoerec, Color.White);

                spriteBatch.End();

                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

        }
    }
}
