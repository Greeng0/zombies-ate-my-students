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
        SpriteFont Font1;
        SpriteBatch spriteBatch;

        MouseState mouseState;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        public Viewport frontViewport;
        public Viewport Viewport = new Viewport(new Rectangle(0, 0, 1500, 900));  

       
        public dude big;
        HUD hud;

        Model School;

        int scrollWheel = 0;

        public Game1()
        {
            mouseState = new MouseState();

            Mouse.WindowHandle = this.Window.Handle;
           // this.IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1500; 
            graphics.PreferredBackBufferHeight = 900; 

            device = graphics.GraphicsDevice;
            
            Content.RootDirectory = "Content";

        }

     
        protected override void Initialize()
        {
            

            spriteBatch = new SpriteBatch(GraphicsDevice);

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

            Font1 = Content.Load<SpriteFont>("Arial");
            School = Content.Load<Model>("School");

            base.LoadContent();

        }
      
    

        protected override void Update(GameTime gameTime)
        {
            //updatehud
            HUD.ActiveHUD.p = big.Position;
            Camera.ActiveCamera.dudeang = big.Angle;

            mouseState = Mouse.GetState();

            if (mouseState.ScrollWheelValue < scrollWheel)
            {
                Camera.ActiveCamera.CameraZoom += new Vector3(0, 5, 0);
                scrollWheel = mouseState.ScrollWheelValue;
            }

            if (mouseState.ScrollWheelValue > scrollWheel)
            {
                Camera.ActiveCamera.CameraZoom -= new Vector3(0, 5, 0);
                scrollWheel = mouseState.ScrollWheelValue;
            }

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


            Camera.ActiveCamera.CameraPosition = big.Position + new Vector3(0, 30, 30) + Camera.ActiveCamera.CameraZoom;
            Camera.ActiveCamera.CameraLookAt = big.Position;
                

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

                    effect.TextureEnabled = true;
                }

                mesh.Draw();
            }


        }

    }
}