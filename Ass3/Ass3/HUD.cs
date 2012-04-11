using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Text;

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



        bool drawselectedwep = false;
        Texture2D selectedwep;
        Rectangle selectedweprec  = new Rectangle(0, 0, 90,90);


        Texture2D letter;
        Texture2D a;
        Texture2D b;
        Texture2D c;
        Texture2D d;
        Texture2D e;
        Texture2D f;

        Texture2D empty;


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


        protected override void LoadContent()
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
            //for letters 

            a = _content.Load<Texture2D>("a");
            b = _content.Load<Texture2D>("b");
            c = _content.Load<Texture2D>("c");
            d = _content.Load<Texture2D>("d");
            e = _content.Load<Texture2D>("e");
            f = _content.Load<Texture2D>("f");

            letter = a;

            selectedwep = _content.Load<Texture2D>("selectedweapon");

            //fill slots with inventory
            slot1 = gun;
            slot2 = gun2;
            slot3 = med;
            slot4 = keys;
            slot5 = fire;

            base.LoadContent();
        }
        public void chooseslots(ref Entities.Hero p)
        {
            drawselectedwep = true;
            if (p.WeaponsList.Count == 2)//if have both weapons need to decide which one to draw green around
            {
                //draw both pictures
                slot1 = gun;
                slot2 = gun2;
           
            if (p.EquippedWeapon.weaponType == Entities.WeaponType.Handgun9mm)
            {
                selectedweprec.X = slot1rec.X-5;
                selectedweprec.Y = slot1rec.Y-5;
            }
            else if (p.EquippedWeapon.weaponType == Entities.WeaponType.Magnum)
            {
                //inset green outline on second slot
                selectedweprec.X = slot2rec.X - 5;
                selectedweprec.Y = slot2rec.Y - 5;
            }
        
            }

            else if (p.WeaponsList.Count == 1)
            {
                //draw one pictures

           if (p.EquippedWeapon.weaponType == Entities.WeaponType.Handgun9mm )
           {
               selectedweprec.X = slot1rec.X - 5;
               selectedweprec.Y = slot1rec.Y - 5;
                slot1 = gun;
                slot2 = empty;

            }
            else if (p.EquippedWeapon.weaponType == Entities.WeaponType.Magnum)
            {
                slot1 = empty;
                slot2 = gun2;
                selectedweprec.X = slot2rec.X-5;
                selectedweprec.Y = slot2rec.Y-5;
            }
            }
                else
            {
                slot1 = empty;
                slot2 = empty;
                selectedweprec.X = slot1rec.X - 5;
                selectedweprec.Y = slot1rec.Y - 5;
                drawselectedwep = false;
            } 
        }
    
       
        public override void Update(GameTime gameTime)
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

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {


            frames++;
            spriteBatch.Begin();
         
                

                string out2 = "Position";
                out2 += "\n" + p ;
                out2 += "\n\n\nAngle";
                out2 += "\n" + MathHelper.ToDegrees(angle);
                out2 += "\n\n\nFps";
                out2 += "\n" + fps;


                Vector2 pos2 = new Vector2(200, 100);
                spriteBatch.DrawString(Font1, out2, pos2, Color.Red, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);

                

                healthscale = rate * playerhealth;
                int diff = min + (int)(rate * (100 - playerhealth));


            //draw health bar
               spriteBatch.Draw(healthbar, new Rectangle(healthx,  diff, healthsizex, max-diff), Color.White);
            spriteBatch.Draw(health, new Rectangle(0, 0, 100, 400), Color.White);
              spriteBatch.Draw(letter, new Rectangle(0, 276, 100, 100), Color.White);
          
          
               
                //draw slots

             spriteBatch.Draw(slot1, slot1rec, Color.White);
             spriteBatch.Draw(slot2, slot2rec, Color.White);
             spriteBatch.Draw(slot3, slot3rec, Color.White);
             spriteBatch.Draw(slot4, slot4rec, Color.White);
             spriteBatch.Draw(slot5, slot5rec, Color.White);


             Rectangle newrec = selectedweprec;
             newrec.X -= 5;
             newrec.Y -= 5;
             if (drawselectedwep)
             {
                 spriteBatch.Draw(selectedwep, newrec, Color.White);
             }
            



            spriteBatch.End();

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        }
    }
}
