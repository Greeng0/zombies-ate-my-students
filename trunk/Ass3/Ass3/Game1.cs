using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;

namespace zombies
{
   
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
  
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        public Viewport frontViewport;
        public Viewport Viewport = new Viewport(new Rectangle(0, 0, 1200, 700));  

       
        public dude big;
        HUD hud;

   
 
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1200; 
            graphics.PreferredBackBufferHeight = 700; 

            device = graphics.GraphicsDevice;
            Content.RootDirectory = "Content";

        }

     
        protected override void Initialize()
        {


            graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.Viewport.Width;   
            graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.Viewport.Height;  
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            device = graphics.GraphicsDevice;


            int HalfWidth = graphics.PreferredBackBufferWidth / 2;
            int HalfHeight = graphics.PreferredBackBufferHeight / 2;

        
            this.Components.Add(new Camera(this));

            frontViewport = new Viewport();

            frontViewport.X = 0;
            frontViewport.Y = 0;
            frontViewport.Width = graphics.PreferredBackBufferWidth - 1;
            frontViewport.Height = graphics.PreferredBackBufferHeight - 1;
            frontViewport.MinDepth = .1f;
            frontViewport.MaxDepth = .8f;


           
            room r = new room(this, Content, new Vector3(0));
            this.Components.Add(r);


            big = new dude(this, Content, 0, new Vector3(0));
        
            this.Components.Add(big);

           

            hud = new HUD(this, Content, graphics);
            this.Components.Add(hud);

            base.Initialize();

        }
      
        
      
        protected override void LoadContent()
        {


            base.LoadContent();

        }
      
    

        protected override void Update(GameTime gameTime)
        {


       
            //updatehud
            HUD.ActiveHUD.p = big.Position;
            Camera.ActiveCamera.dudeang = big.Angle;


            //endupdatehud
            KeyboardState k = Keyboard.GetState();
            bool walk = false;

            //translate
            if (k.IsKeyDown(Keys.Up))
            {
                big.Position -= big.speed * new Vector3((float)Math.Sin(big.Angle), 0, (float)Math.Cos(big.Angle));
               walk = true;
            }

             if (k.IsKeyDown(Keys.Down))
            {
                big.Position += big.speed * new Vector3((float)Math.Sin(big.Angle), 0, (float)Math.Cos(big.Angle));

                walk = true;
            }
             if (k.IsKeyDown(Keys.Left))
            {
                big.Angle += .1f;

                walk = true;
            }
             if (k.IsKeyDown(Keys.Right))
            {
                big.Angle -= .1f;
                walk = true;
            }
             if (walk)
                 big.walking = true;
             else
                 big.walking = false;


            if (k.IsKeyDown(Keys.Escape))
            {
                Exit();

            }
       


                 Camera.ActiveCamera.dudepo = big.Position;
     

                base.Update(gameTime);
         
        }

      
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.Viewport = frontViewport;


            base.Draw(gameTime);



        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {


            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }


        }

    }
}