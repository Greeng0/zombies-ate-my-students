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
using HIDInput;
using Entities;

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
        Model HeroModel;
        Model ZombieModel;

        Hero Player;
        List<Zombie> zombies;

        int scrollWheel = 0;

        public Game1()
        {
            mouseState = new MouseState();

            Mouse.WindowHandle = this.Window.Handle;
           // this.IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1300; 
            graphics.PreferredBackBufferHeight = 700; 

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


            room[] r = new room[1];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = new room(this, Content, new Vector3(0));
                this.Components.Add(r[i]);
            }
                

            big = new dude(this, Content, 0, new Vector3(47, 0, 8));
        
            this.Components.Add(big);

           

            hud = new HUD(this, Content, graphics);
            this.Components.Add(hud);

            zombies = new List<Zombie>();

            base.Initialize();
        }
      
        protected override void LoadContent()
        {

            Font1 = Content.Load<SpriteFont>("Arial");
            School = Content.Load<Model>("School");
            HeroModel = Content.Load<Model>("HeroWalk");
            ZombieModel = Content.Load<Model>("ZombieWalk");

            Player = new Hero(1000, 1000, ref HeroModel, DoAction);
            
            Zombie z1 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z1.Position = new Vector3(0, 0, 30);
            Zombie z2 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z2.Position = new Vector3(0, 0, -30);
            Zombie z3 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z3.Position = new Vector3(30, 0, 0);
            Zombie z4 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z4.Position = new Vector3(-30, 0, 0);
            Zombie z5 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z5.Position = new Vector3(10, 0, 10);
            Zombie z6 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAction);
            z6.Position = new Vector3(10, 0, -10);
            //Zombie z7 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAttack);
            //z7.Position = new Vector3(-10, 0, -10);
            //Zombie z8 = new Zombie(500, 500, ZombieType.Adult, ref ZombieModel, DoAttack);
            //z8.Position = new Vector3(-10, 0, 10);
            zombies.Add(z1);
            zombies.Add(z2);
            zombies.Add(z3);
            zombies.Add(z4);
            zombies.Add(z5);
            zombies.Add(z6);
            //zombies.Add(z7);
            //zombies.Add(z8);

            //z1.targetslot = Player.reserveSlot(z1);
            //z2.targetslot = Player.reserveSlot(z2);
            //z3.targetslot = Player.reserveSlot(z3);
            //z4.targetslot = Player.reserveSlot(z4);
            //z5.targetslot = Player.reserveSlot(z5);
            //z6.targetslot = Player.reserveSlot(z6);
            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            //updatehud
            HUD.ActiveHUD.p = Player.Position;
            HUD.ActiveHUD.angle = (float) Player.Rotation;
            Camera.ActiveCamera.dudeang = (float) Player.Rotation;

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
            //end updatehud

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyboard.IsKeyDown(Keys.LeftShift))
                Player.Stance = AnimationStance.Shooting;
            else
                Player.Stance = AnimationStance.Standing;

            bool walk = false;
            Keys[] keysPressed = keyboard.GetPressedKeys();

            foreach (Keys key in keysPressed)
            {
                switch (key)
                {
                    case Keys.LeftShift:
                        break;
                    case Keys.Up:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.MoveForward();
                        walk = true;
                        break;
                    case Keys.Down:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.MoveBackward();
                        walk = true;
                        break;
                    case Keys.Left:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.TurnLeft();
                        walk = true;
                        break;
                    case Keys.Right:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.TurnRight();
                        walk = true;
                        break;
                    case Keys.Tab:
                        Player.SwitchNextItem();
                        break;
                    case Keys.W:
                        Player.SwitchNextWeapon();
                        break;
                    case Keys.Space:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.DoAction();
                        break;
                }
            }

            if (walk)
                Player.animState = Entity.AnimationState.Walking;
            else
                Player.animState = Entity.AnimationState.Idle;

            Player.Update(gameTime);
            foreach (Zombie z in zombies)
            {
                z.Update(gameTime);
            }
            
            Camera.ActiveCamera.CameraPosition = Player.Position + new Vector3(0, 30, 30) + Camera.ActiveCamera.CameraZoom;
            Camera.ActiveCamera.CameraLookAt = Player.Position;
            
            base.Update(gameTime);
        }

        public void DoAction(Entity actionCaster, Entity objectCasted)
        {
            if (objectCasted is Weapon)
            {
                Weapon weapon = objectCasted as Weapon;
                CastSoundWave(weapon.SoundRadius);
                // TODO: check if caster is Hero or Zombie, perform attack accordingly
            }
            else if (objectCasted is Item)
            {
                Item item = objectCasted as Item;
                CastSoundWave(item.SoundRadius);
            }
        }

        // Creates a bounding sphere with the specified radius. Any Zombie intersecting the
        // bounding sphere will be alerted to the Hero's presence
        private void CastSoundWave(float radius)
        {
            if (radius > 0)
            {
                BoundingSphere soundWave = new BoundingSphere(Player.Position, radius);
                foreach (Zombie z in zombies)
                {
                    // ****************THIS IS JUST PLACEHOLDER FOR TESTING**************************
                    BoundingSphere zb = z.model.Meshes[0].BoundingSphere;
                    zb.Transform(Matrix.CreateTranslation(z.Position));
                    if (zb.Intersects(soundWave))
                        z.Alert(Player);
                }
            }
        }
      
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            graphics.GraphicsDevice.Viewport = frontViewport;

            DrawModel(Player);
            foreach (Zombie z in zombies)
            {
                DrawModel(z);
            }
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

        private void DrawModel(Hero hero)
        {
            Matrix[] bones = hero.animationPlayer.GetSkinTransforms();

            // Render the skinned mesh
            foreach (ModelMesh mesh in hero.model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY((float) hero.Rotation) * 
                        Matrix.CreateScale(hero.scale) * Matrix.CreateTranslation(hero.Position);
                    effect.SetBoneTransforms(bones);
                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }

        private void DrawModel(Zombie zombie)
        {
            Matrix[] bones = zombie.animationPlayer.GetSkinTransforms();

            // Render the skinned mesh
            foreach (ModelMesh mesh in zombie.model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY((float)(zombie.Rotation - Math.PI)) * 
                        Matrix.CreateScale(zombie.scale) * Matrix.CreateTranslation(zombie.Position);
                    effect.SetBoneTransforms(bones);
                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }
    }
}