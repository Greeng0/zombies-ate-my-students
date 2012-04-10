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
using Entities;
using System.Diagnostics;

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

        List<Box> CollisionBoxes = new List<Box>();

        HUD hud;

        Model School;
        Model HeroModel;

        //4 animations for zombies
        Model ZombieWalk;
        Model ZombieAttack;
        Model ZombieHurt;
        Model ZombieDie;

        //3 animations for zombies
        Model HeroWalk;
        Model HeroHurt;
        Model HeroDie;

        int ButtonTimer = 0;

        BasicEffect globalEffect;

        QuadTree LevelQuadTree;

        Hero Player;
        List<Zombie> zombies;

        int scrollWheel = 0;
        int scrollWheelLow = 0;
        int scrollWheelHigh = 300;

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

            hud = new HUD(this, Content, graphics);
            this.Components.Add(hud);

            zombies = new List<Zombie>();

            base.Initialize();
        }
      
        protected override void LoadContent()
        {

            Font1 = Content.Load<SpriteFont>("Arial");
            School = Content.Load<Model>("School");
          
            //zombie animations
            ZombieWalk = Content.Load<Model>("ZombieWalk");
            ZombieHurt = Content.Load<Model>("ZombieHurt");
            ZombieDie = Content.Load<Model>("ZombieDie");
            ZombieAttack = Content.Load<Model>("ZombieAttack");

            //hero animations
            HeroWalk = Content.Load<Model>("HeroWalk");
            HeroHurt = Content.Load<Model>("HeroHurt");
            HeroDie = Content.Load<Model>("HeroDead");
          
            // TODO: Initialize quad tree and insert all objects into it********************************************
            //QuadTree = new QuadTree(centerPosition, size, depth);

            Player = new Hero(1, 1000, ref HeroWalk, ref HeroDie, ref HeroHurt, DoAction);
            Player.Position = new Vector3(-15, 0, 1);

            /*Zombie z1 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction);
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
            zombies.Add(z11);*/

            CollisionBoxes.Add(new Box(new Vector3(10, 0, 0), new Vector3(0), new Vector3(10, 40, 10)));


            LevelQuadTree = new QuadTree(new Vector2(0, 0), 20, 5);
            
            foreach (Box box in CollisionBoxes)
            {
                LevelQuadTree.Insert(box);
            }

            globalEffect = new BasicEffect(GraphicsDevice);
            globalEffect.VertexColorEnabled = true;
            globalEffect.View = Camera.ActiveCamera.View;
            globalEffect.World = world;
            globalEffect.Projection = Camera.ActiveCamera.Projection;

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            #region Update hud
            HUD.ActiveHUD.playerhealth = Player.HealthPoints;
            HUD.ActiveHUD.p = Player.Position;
            HUD.ActiveHUD.angle = (float) Player.Rotation;
            Camera.ActiveCamera.dudeang = (float) Player.Rotation;

            mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Player.HealthPoints = 0;
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

            //Rotate World with Arrow Keys
            if (keyboard.IsKeyDown(Keys.K))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position += new Vector3(0, 0, 0.5f);
            }
            if (keyboard.IsKeyDown(Keys.I))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position -= new Vector3(0, 0, 0.5f);
            }
            if (keyboard.IsKeyDown(Keys.L))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position += new Vector3(0.5f, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.J))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position -= new Vector3(0.5f, 0, 0);
            }

            if (keyboard.IsKeyDown(Keys.Y))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size += new Vector3(0.5f, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.U))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size -= new Vector3(0.5f, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.O))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size += new Vector3(0, 0, 0.5f);
            }
            if (keyboard.IsKeyDown(Keys.P))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size -= new Vector3(0, 0, 0.5f);
            }

            if (keyboard.IsKeyDown(Keys.Enter) && ButtonTimer <= 0)
            {
                Debug.WriteLine("CollisionBoxes.Add(new Box(new Vector3(" + CollisionBoxes[CollisionBoxes.Count-1].Position.X + ", " +  CollisionBoxes[CollisionBoxes.Count-1].Position.Y + ", " + CollisionBoxes[CollisionBoxes.Count-1].Position.Z +") , new Vector3(0), new Vector3(" + CollisionBoxes[CollisionBoxes.Count-1].Size.X + ", 20 , " + CollisionBoxes[CollisionBoxes.Count-1].Size.Z +")));");
                CollisionBoxes.Add(new Box(CollisionBoxes[CollisionBoxes.Count-1].Position,new Vector3(0),CollisionBoxes[CollisionBoxes.Count-1].Size));

                LevelQuadTree.Insert(CollisionBoxes[CollisionBoxes.Count - 1]);

                CollisionBoxes.Add(new Box(new Vector3(Player.Position.X,Player.Position.Y,Player.Position.Z), new Vector3(0),new Vector3(10,20,10)));
                ButtonTimer = 10;
            }

            if (ButtonTimer > 0)
                ButtonTimer -= 1;
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
                        {
                            Player.MoveForward();
                            walk = true;
                        }
                        break;
                    case Keys.Down:
                        if (KeyboardInput.ProcessInput(key, Player))
                        {
                            Player.MoveBackward();
                            walk = true;
                        }
                        break;
                    case Keys.Left:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.TurnLeft();
                        break;
                    case Keys.Right:
                        if (KeyboardInput.ProcessInput(key, Player))
                            Player.TurnRight();
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
            else if (Player.animState != Entity.AnimationState.Hurt)
                Player.animState = Entity.AnimationState.Idle;
            if (Player.HealthPoints <= 0 || Player.animState == Entities.Entity.AnimationState.Dying)//start dying animation
            {
                Player.animState = Entity.AnimationState.Dying;
            }
        
           
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
           
            globalEffect.View = Camera.ActiveCamera.View;
            globalEffect.World = world;
            globalEffect.Projection = Camera.ActiveCamera.Projection;

            base.Update(gameTime);
        }

        private void CheckCollisions()
        {
            
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

           /* #region Zombie collisions

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
            //-contact.ContactNormal * (Vector3.Dot(-contact.ContactNormal, ent.Velocity));
            Vector3 closingVelocity = contact.ContactNormal * contact.PenetrationDepth;
            
            //The Y axis vector value of the position should always remain zero.
            closingVelocity.Y = 0; 
            ent.Position += closingVelocity;
            
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
                
                // apply silencer if possible
                if (weapon.weaponType == Entities.WeaponType.Handgun9mm && (actionCaster as Hero).PowerupsList.Contains(Powerups.Silencer))
                {
                    CastSoundWave(weapon.SoundRadius / 3);
                }
                else
                {
                    CastSoundWave(weapon.SoundRadius);
                }

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
                Sphere soundSphere = new Sphere(Player.Position, new Vector3(), radius);
                foreach (Zombie z in zombies)
                {
                    Sphere zs = new Sphere(z.Position, z.Velocity, z.modelRadius);
                    if (zs.Collides(soundSphere) != null)
                        z.Alert(Player);
                }
            }
        }
      
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            graphics.GraphicsDevice.Viewport = frontViewport;

            DrawSchool();
            DrawModel(Player);

            DrawBox(CollisionBoxes[0],Color.Red);
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
                    effect.World = Matrix.CreateRotationY((float)(hero.Rotation - Math.PI)) * Matrix.CreateScale(hero.scale) * Matrix.CreateTranslation(hero.Position);// 
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
                    effect.World = effect.World;
                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }

        public void DrawSchool()
        {

            SamplerState sample = new SamplerState();

            sample.AddressU = TextureAddressMode.Wrap;
            sample.AddressV = TextureAddressMode.Wrap;


            //_model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
            foreach (ModelMesh mesh in School.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.Identity;


                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    switch (mesh.ParentBone.Parent.Name)
                    {
                        case "Urinals":
                        case "Toilet":
                        case "Computers":
                        case "BigDesks":
                        case "StudentDesks":
                        case "GymCourt":
                        case "TVHolder":
                        case "BigTable":
                            {
                                break;
                            }
                        default:
                            {
                                effect.TextureEnabled = true;
                                break;
                            }
                    }

                    effect.EnableDefaultLighting();
                    effect.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

                }

                mesh.Draw();
            }
        }

        private void DrawBox(Box box, Color color, bool OutlineOnly = false)
        {
            if (box != null)
            {
                //Draw as triangle list with potential solid display
                if (!OutlineOnly)
                {
                    VertexBuffer vertexBuffer;

                    VertexPositionColor[] cubeVertices = new VertexPositionColor[36];

                    Vector3[] Vertices = box.GetVertices();

                    //Front
                    cubeVertices[0] = new VertexPositionColor(Vertices[0], color);
                    cubeVertices[1] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[2] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[3] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[4] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[5] = new VertexPositionColor(Vertices[3], color);

                    //Top
                    cubeVertices[6] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[7] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[8] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[9] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[10] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[11] = new VertexPositionColor(Vertices[7], color);

                    //Back
                    cubeVertices[12] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[13] = new VertexPositionColor(Vertices[7], color);
                    cubeVertices[14] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[15] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[16] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[17] = new VertexPositionColor(Vertices[4], color);

                    //Bottom
                    cubeVertices[18] = new VertexPositionColor(Vertices[4], color);
                    cubeVertices[19] = new VertexPositionColor(Vertices[0], color);
                    cubeVertices[20] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[21] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[22] = new VertexPositionColor(Vertices[0], color);
                    cubeVertices[23] = new VertexPositionColor(Vertices[2], color);

                    //Left Side
                    cubeVertices[24] = new VertexPositionColor(Vertices[4], color);
                    cubeVertices[25] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[26] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[27] = new VertexPositionColor(Vertices[4], color);
                    cubeVertices[28] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[29] = new VertexPositionColor(Vertices[0], color);

                    //Right Side
                    cubeVertices[30] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[31] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[32] = new VertexPositionColor(Vertices[7], color);
                    cubeVertices[33] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[34] = new VertexPositionColor(Vertices[7], color);
                    cubeVertices[35] = new VertexPositionColor(Vertices[6], color);


                    globalEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                    globalEffect.CurrentTechnique.Passes[0].Apply();

                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, 36, BufferUsage.None);
                    vertexBuffer.SetData<VertexPositionColor>(cubeVertices);

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, 0, 12);
                    vertexBuffer.Dispose();
                }
                else
                {
                    //Draw as line list for hollow display

                    VertexBuffer vertexBuffer;

                    VertexPositionColor[] cubeVertices = new VertexPositionColor[24];

                    Vector3[] Vertices = box.GetVertices();

                    //Front
                    cubeVertices[0] = new VertexPositionColor(Vertices[0], color);
                    cubeVertices[1] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[2] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[3] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[4] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[5] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[6] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[7] = new VertexPositionColor(Vertices[0], color);

                    cubeVertices[8] = new VertexPositionColor(Vertices[0], color);
                    cubeVertices[9] = new VertexPositionColor(Vertices[4], color);
                    cubeVertices[10] = new VertexPositionColor(Vertices[1], color);
                    cubeVertices[11] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[12] = new VertexPositionColor(Vertices[2], color);
                    cubeVertices[13] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[14] = new VertexPositionColor(Vertices[3], color);
                    cubeVertices[15] = new VertexPositionColor(Vertices[7], color);

                    cubeVertices[16] = new VertexPositionColor(Vertices[4], color);
                    cubeVertices[17] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[18] = new VertexPositionColor(Vertices[5], color);
                    cubeVertices[19] = new VertexPositionColor(Vertices[7], color);
                    cubeVertices[20] = new VertexPositionColor(Vertices[7], color);
                    cubeVertices[21] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[22] = new VertexPositionColor(Vertices[6], color);
                    cubeVertices[23] = new VertexPositionColor(Vertices[4], color);

                    globalEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                    globalEffect.CurrentTechnique.Passes[0].Apply();

                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, 24, BufferUsage.None);
                    vertexBuffer.SetData<VertexPositionColor>(cubeVertices);

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, 0, 12);
                    vertexBuffer.Dispose();
                }
            }
        }
    }
}