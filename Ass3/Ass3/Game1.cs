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
using Collisions;
using SpacePartition;
using AI;

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

        HUD hud;

        Model School;
        Model HeroModel;

        //4 animations for zombies
        Model ZombieWalk;
        Model ZombieAttack;
        Model ZombieHurt;
        Model ZombieDie;

        QuadTree LevelQuadTree;

        Hero Player;
        List<Zombie> zombies;

        int scrollWheel = 0;
        int scrollWheelLow = 0;
        int scrollWheelHigh = 30;

        int radiusofsight = 60;

        const float COLLISON_SOUND_RADIUS = 15;

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
            ZombieWalk = Content.Load<Model>("ZombieWalk");
            ZombieHurt = Content.Load<Model>("ZombieHurt");
            ZombieDie = Content.Load<Model>("ZombieDie");
            ZombieAttack = Content.Load<Model>("ZombieAttack");

            // TODO: Initialize quad tree and insert all objects into it********************************************
            //QuadTree = new QuadTree(centerPosition, size, depth);

            Player = new Hero(1000, 1000, ref HeroModel, DoAction);
            
            Zombie z1 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z1.Position = new Vector3(0, 0, 10);
            Zombie z2 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z2.Position = new Vector3(0, 0, -10);
            Zombie z3 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z3.Position = new Vector3(10, 0, 0);
            Zombie z4 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z4.Position = new Vector3(-10, 0, 0);
            Zombie z5 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z5.Position = new Vector3(15, 0, 10);
            Zombie z6 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z6.Position = new Vector3(10, 0, -15);
            Zombie z7 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z7.Position = new Vector3(-15, 0, -10);
            Zombie z8 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z8.Position = new Vector3(-10, 0, 15);
            Zombie z9 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z9.Position = new Vector3(0, 0, -25);
            Zombie z10 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z10.Position = new Vector3(0, 0, -35);
            Zombie z11 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
            z11.Position = new Vector3(45, 0, -45);

            zombies.Add(z1);
            zombies.Add(z2);
            zombies.Add(z3);
            zombies.Add(z4);
            zombies.Add(z5);
            zombies.Add(z6);
            zombies.Add(z7);
            zombies.Add(z8);
            zombies.Add(z9);
            zombies.Add(z10);
            zombies.Add(z11);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            #region Update hud
            HUD.ActiveHUD.p = Player.Position;
            HUD.ActiveHUD.angle = (float) Player.Rotation;
            Camera.ActiveCamera.dudeang = (float) Player.Rotation;

            mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                foreach(Zombie z in zombies)
               z.animState = Entity.AnimationState.Idle;
            }
            if (mouseState.ScrollWheelValue < scrollWheel)
            {
                if (Camera.ActiveCamera.CameraZoom.Length() < scrollWheelHigh)
                {
                    Camera.ActiveCamera.CameraZoom += new Vector3(0, 5, 0);
                }
                scrollWheel = mouseState.ScrollWheelValue;
            }

            if (mouseState.ScrollWheelValue > scrollWheel)
            {
                if (Camera.ActiveCamera.CameraZoom.Length() > scrollWheelLow)
                {
                    Camera.ActiveCamera.CameraZoom -= new Vector3(0, 5, 0);
                }
                scrollWheel = mouseState.ScrollWheelValue;
            }
            #endregion

            #region Player input
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
            #endregion

            Player.Update(gameTime);

            //update right zombies
            foreach (Zombie z in zombies)//update zombies
            {
               
                //This checks a radius around the player to see whether or not we should be updating the zombie
                if ((z.Position - Player.Position).Length() < radiusofsight)
                {
                    z.Update(gameTime);
                }
                //If zombie is out of radius, we must still check to see if it is chasing the character. 
                //If that is the case then we still need to update, but not to draw.
                else if (z.BehaviouralState != BehaviourState.Wander)
                {
                    z.Update(gameTime);
                }
            }

            CheckCollisions();

            Camera.ActiveCamera.CameraPosition = Player.Position + new Vector3(0, 30, 30) + Camera.ActiveCamera.CameraZoom;
            Camera.ActiveCamera.CameraLookAt = Player.Position;
            
            base.Update(gameTime);
        }

        private void CheckCollisions()
        {
            /*
            #region Player collisions

            Sphere heroSphere = new Sphere(Player.Position, Player.Velocity, Player.modelRadius);
            List<Primitive> primitivesNearby = new List<Primitive>();
            LevelQuadTree.RetrieveNearbyObjects(heroSphere, ref primitivesNearby);
            foreach (Primitive p in primitivesNearby)
            {
                Contact c = heroSphere.Collides(p as Box);
                if (c != null)
                {
                    ResolveStaticCollision(c, Player, heroSphere);
                }
            }
            
            #endregion

            #region Zombie collisions

            foreach (Zombie z in zombies)
            {
                // Check for zombies in sight radius and zombies who are not wandering
                if ((z.Position - Player.Position).Length() < radiusofsight || z.BehaviouralState != BehaviourState.Wander)
                {
                    Sphere zombieSphere = new Sphere(z.Position, z.Velocity, z.modelRadius);
                    List<Primitive> primitives = new List<Primitive>();
                    LevelQuadTree.RetrieveNearbyObjects(zombieSphere, ref primitives);
                    foreach (Primitive p in primitives)
                    {
                        Contact c = zombieSphere.Collides(p as Box);
                        if (c != null)
                        {
                            ResolveStaticCollision(c, z, zombieSphere);
                        }
                    }
                }
            }

            #endregion
            */
            foreach (Zombie z1 in zombies)
            {
                if ((z1.Position - Player.Position).Length() < radiusofsight || z1.BehaviouralState != BehaviourState.Wander)
                {
                    checkZombietoPlayer(z1);
                    foreach (Zombie z2 in zombies)
                    {
                        if (!z2.Equals(z1) &&((z2.Position - Player.Position).Length() < radiusofsight || z2.BehaviouralState != BehaviourState.Wander))
                        {
                            checkZombietoZombie(z1, z2);
                        }
                    }
                }
            }
        }

        private void ResolveStaticCollision(Contact contact, Entity ent, Sphere sphere)
        {
            Vector3 closingVelocity = -contact.ContactNormal * (Vector3.Dot(-contact.ContactNormal, ent.Velocity));
            ent.Position += -closingVelocity * sphere.Mass;
            if (ent is Hero)
            {
                CastSoundWave(COLLISON_SOUND_RADIUS);
            }
        }

        //checking zombie to character
        private void checkZombietoPlayer(Zombie z)
        {
            Sphere p1 = new Collisions.Sphere(z.Position, z.Velocity, z.modelRadius);
            Sphere p2 = new Collisions.Sphere(Player.Position, Player.Velocity, Player.modelRadius);

            Contact c = p1.Collides(p2);

            if (c != null)
            {
                if (c.DeepestPoint.Length() > 0)
                {
                    if (Player.Stance == AnimationStance.Standing)//if standing, dont push player, only affect zombie
                    {
                        z.Position -= c.DeepestPoint - c.ContactPoint;
                        Player.Position += c.DeepestPoint - c.ContactPoint;
                       
                    }
                    else//push player back when walking
                    {
                        z.Position -= c.DeepestPoint - c.ContactPoint;
                    }
                }
            }
        }

        //checking zombie to zombie collisions. 
        //Model as a sphere (in reality just a cylinder but since all at same height it only checks for a circle radius around character
        private void checkZombietoZombie(Zombie z1, Zombie z2)
        {
            //creating appropriate shapes
            Sphere p1 = new Collisions.Sphere(z1.Position, z1.Velocity, z1.modelRadius);
            Sphere p2 = new Collisions.Sphere(z2.Position, z2.Velocity, z2.modelRadius);

            Contact c = p1.Collides(p2);

            if (c != null)
            {
                if (c.DeepestPoint.Length() > 0)
                {
                    z1.Position -= c.DeepestPoint - c.ContactPoint;
                    z2.Position += c.DeepestPoint - c.ContactPoint;
                }
            }
        }

        public void DoAction(Entity actionCaster, Entity objectCasted)
        {
            if (objectCasted is Weapon)
            {
                Weapon weapon = objectCasted as Weapon;
                CastSoundWave(weapon.SoundRadius);
                // TODO: check if caster is Hero or Zombie, perform attack accordingly
                switch (weapon.weaponType)
                {
                    case WeaponType.BareHands:
                        {
                            Ray ray = new Ray(actionCaster.Position, actionCaster.Velocity);
                            foreach (Zombie z in zombies)
                            {
                                if ((z.Position - actionCaster.Position).Length() < weapon.Range)
                                {
                                    BoundingSphere bs = new BoundingSphere(z.Position, z.modelRadius);
                                    if (bs.Intersects(ray) != null)
                                        z.TakeDamage(weapon.FirePower);
                                }
                            }
                            break;
                        }
                    case WeaponType.Handgun9mm:
                        {
                            // find closest zombie, if any, in the line of fire and have him take the damage
                            Ray ray = new Ray(actionCaster.Position, actionCaster.Velocity);
                            Zombie closestVictim = null;
                            float closestIntersect = 100;
                            foreach (Zombie z in zombies)
                            {
                                if ((z.Position - actionCaster.Position).Length() < weapon.Range)
                                {
                                    BoundingSphere bs = new BoundingSphere(z.Position, z.modelRadius);
                                    float? intersection = bs.Intersects(ray);
                                    if (intersection != null && intersection < closestIntersect)
                                        closestVictim = z;
                                }
                            }
                            if (closestVictim != null)
                                closestVictim.TakeDamage(weapon.FirePower);
                            break;
                        }
                    case WeaponType.Magnum:
                        {
                            // find closest zombie, if any, in the line of fire and have him take the damage
                            Ray ray = new Ray(actionCaster.Position, actionCaster.Velocity);
                            Zombie closestVictim = null;
                            float closestIntersect = 100;
                            foreach (Zombie z in zombies)
                            {
                                if ((z.Position - actionCaster.Position).Length() < weapon.Range)
                                {
                                    BoundingSphere bs = new BoundingSphere(z.Position, z.modelRadius);
                                    float? intersection = bs.Intersects(ray);
                                    if (intersection != null && intersection < closestIntersect)
                                        closestVictim = z;
                                }
                            }
                            if (closestVictim != null)
                            {
                                if (closestIntersect > 20)
                                    closestVictim.TakeDamage(weapon.FirePower / 10);
                                else if (closestIntersect > 10)
                                    closestVictim.TakeDamage(weapon.FirePower / 5);
                                else
                                    closestVictim.TakeDamage(weapon.FirePower);
                            }
                            break;
                        }
                    case WeaponType.Vomit:
                        {
                            Ray ray = new Ray(actionCaster.Position, actionCaster.Velocity);
                            if ((actionCaster.Position - Player.Position).Length() < weapon.Range)
                            {
                                BoundingSphere bs = new BoundingSphere(actionCaster.Position, actionCaster.modelRadius);
                                if (bs.Intersects(ray) != null)
                                    Player.TakeDamage(weapon.FirePower);
                            }
                            break;
                        }
                    case WeaponType.ZombieHands:
                        {
                            Ray ray = new Ray(actionCaster.Position, actionCaster.Velocity);
                            if ((actionCaster.Position - Player.Position).Length() < weapon.Range)
                            {
                                BoundingSphere bs = new BoundingSphere(actionCaster.Position, actionCaster.modelRadius);
                                if (bs.Intersects(ray) != null)
                                    Player.TakeDamage(weapon.FirePower);
                            }
                            break;
                        }
                }
            }
            else if (objectCasted is Item)
            {
                Item item = objectCasted as Item;
                CastSoundWave(item.SoundRadius);
                // TODO: perform item effect
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
                if ((z.Position - Player.Position).Length() < radiusofsight)
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
                    effect.World = Matrix.CreateRotationY((float) (hero.Rotation - Math.PI)) * 
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