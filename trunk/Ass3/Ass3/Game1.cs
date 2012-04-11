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
        List<Primitive> TotalNearbyBoxes = new List<Primitive>();

        HUD hud;

        Model School;

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
        bool WireFrameCollisionBoxes = false;
        bool ShowQuadBoundaries = false;
        bool ShowCollisionBoxes = false;

        Hero Player;
        List<Zombie> zombies;
        List<Box> fireHazards;


        List<Weapon> weapons;
        //weapon models
        Weapon magnum;
        Weapon Silencer;
        Weapon socom;
        Weapon socomsilencer;


        int scrollWheel = 0;
        int scrollWheelLow = 0;
        int scrollWheelHigh = 500;

        const int SIGHT_RADIUS = 60;
        const float COLLISON_SOUND_RADIUS = 15;


        //sound
        Sounds.Sounds sound;

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
            fireHazards = new List<Box>();

            base.Initialize();
        }
      
        protected override void LoadContent()
        {
            //sounds
       
            sound = new Sounds.Sounds(this, Content);
            sound.LoadSounds();
            this.Components.Add(sound);

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
            //weapon models

            magnum = new Weapon(WeaponType.Magnum);
            magnum.model = Content.Load<Model>("Magnum");

            socom = new Weapon(WeaponType.Handgun9mm);
            socom.model = Content.Load<Model>("Silencer");

           /* magnum = new Weapon(WeaponType.Magnum);
            magnum.model = Content.Load<Model>("Magnum");

            magnum = new Weapon(WeaponType.Magnum);
            magnum.model = Content.Load<Model>("Magnum");
           */


           
          
            Player = new Hero(1000, 1000, ref HeroWalk, ref HeroDie, ref HeroHurt, DoAction);
            Player.Position = new Vector3(-15, 0, 1);

            //add weapons
            Player.AddWeapon(magnum);
            Player.AddWeapon(socom);
            Player.EquippedWeapon = socom;




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

            #region Level Collision Detection
            CollisionBoxes.Add(new Box(new Vector3(0, 0, 0.5f), new Vector3(0), new Vector3(10, 20, 27.5f)));
            CollisionBoxes.Add(new Box(new Vector3(29.50001f, 0f, -25.4f), new Vector3(0), new Vector3(109.2f, 20, 1.899998f)));
            CollisionBoxes.Add(new Box(new Vector3(-25.10233f, 0f, 95.93351f), new Vector3(0), new Vector3(1.699998f, 20, 241.8056f)));
            CollisionBoxes.Add(new Box(new Vector3(-9.79998f, 0f, 25.00006f), new Vector3(0), new Vector3(29.50007f, 20, 0.3999981f)));
            CollisionBoxes.Add(new Box(new Vector3(17.40005f, 0f, 24.90006f), new Vector3(0), new Vector3(14.10002f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(-18.00001f, 0f, -22.40004f), new Vector3(0), new Vector3(12.60001f, 20, 3.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(24.20008f, 0f, 1.087785E-06f), new Vector3(0), new Vector3(1.299998f, 20, 48.29984f)));
            CollisionBoxes.Add(new Box(new Vector3(10.94763f, 0f, 21.71193f), new Vector3(0), new Vector3(1.299998f, 20, 5.2f)));
            CollisionBoxes.Add(new Box(new Vector3(25.70008f, 0f, -0.2000001f), new Vector3(0), new Vector3(2.599998f, 20, 19.80004f)));
            CollisionBoxes.Add(new Box(new Vector3(34.60007f, 0f, 24.90006f), new Vector3(0), new Vector3(20.70004f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(62.59964f, 0f, 25.00006f), new Vector3(0), new Vector3(24.60006f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(54.59976f, 0f, 4.599998f), new Vector3(0), new Vector3(0.9999982f, 20, 38.99998f)));
            CollisionBoxes.Add(new Box(new Vector3(50.93958f, 0f, 22.31446f), new Vector3(0), new Vector3(1.099998f, 20, 4.799996f)));
            CollisionBoxes.Add(new Box(new Vector3(52.17354f, 0f, -22.68555f), new Vector3(0), new Vector3(5.599995f, 20, 4.899996f)));
            CollisionBoxes.Add(new Box(new Vector3(79.09613f, 0f, -23.12064f), new Vector3(0), new Vector3(10.1f, 20, 3.799997f)));
            CollisionBoxes.Add(new Box(new Vector3(61.59824f, 0f, -4.24892f), new Vector3(0), new Vector3(3.099998f, 20, 8.499994f)));
            CollisionBoxes.Add(new Box(new Vector3(71.67612f, 0f, -4.251044f), new Vector3(0), new Vector3(2.999998f, 20, 8.599995f)));
            CollisionBoxes.Add(new Box(new Vector3(61.67628f, 0f, 12.74897f), new Vector3(0), new Vector3(3.099998f, 20, 8.499994f)));
            CollisionBoxes.Add(new Box(new Vector3(71.77612f, 0f, 12.84897f), new Vector3(0), new Vector3(3.199997f, 20, 8.199993f)));
            CollisionBoxes.Add(new Box(new Vector3(84.38277f, 0f, -46.47377f), new Vector3(0), new Vector3(1.299998f, 20, 143.6996f)));
            CollisionBoxes.Add(new Box(new Vector3(82.01137f, 0f, 22.5932f), new Vector3(0), new Vector3(4.499996f, 20, 5.299995f)));
            CollisionBoxes.Add(new Box(new Vector3(86.89927f, 0f, 24.90006f), new Vector3(0), new Vector3(3.599997f, 20, 0.8999982f)));
            CollisionBoxes.Add(new Box(new Vector3(138.759f, 0f, 24.91656f), new Vector3(0), new Vector3(88.59922f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(131.5181f, 0f, -118.0334f), new Vector3(0), new Vector3(95.29912f, 20, 0.5999981f)));
            CollisionBoxes.Add(new Box(new Vector3(179.0806f, 0f, -47.45309f), new Vector3(0), new Vector3(0.9999982f, 20, 143.8996f)));
            CollisionBoxes.Add(new Box(new Vector3(134.2582f, 0f, -97.64594f), new Vector3(0), new Vector3(2.599998f, 20, 4.099997f)));
            CollisionBoxes.Add(new Box(new Vector3(134.5582f, 0f, 8.853141f), new Vector3(0), new Vector3(2.999998f, 20, 4.499996f)));
            CollisionBoxes.Add(new Box(new Vector3(87.38562f, 0f, -71.61339f), new Vector3(0), new Vector3(5.899995f, 20, 54.29974f)));
            CollisionBoxes.Add(new Box(new Vector3(87.78561f, 0f, -12.91392f), new Vector3(0), new Vector3(5.799995f, 20, 54.59974f)));
            CollisionBoxes.Add(new Box(new Vector3(175.5789f, 0f, -72.12956f), new Vector3(0), new Vector3(5.999995f, 20, 54.49974f)));
            CollisionBoxes.Add(new Box(new Vector3(175.6515f, 0f, -13.14863f), new Vector3(0), new Vector3(5.399995f, 20, 53.99975f)));
            CollisionBoxes.Add(new Box(new Vector3(260.2218f, 0f, -24.30155f), new Vector3(0), new Vector3(160.9006f, 20, 1.899998f)));
            CollisionBoxes.Add(new Box(new Vector3(184.9245f, 0f, -22.25403f), new Vector3(0), new Vector3(10.8f, 20, 3.299997f)));
            CollisionBoxes.Add(new Box(new Vector3(191.5249f, 0f, -4.253981f), new Vector3(0), new Vector3(2.899998f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(191.4634f, 0f, 12.71193f), new Vector3(0), new Vector3(2.899998f, 20, 7.899993f)));
            CollisionBoxes.Add(new Box(new Vector3(201.9757f, 0f, -4.188081f), new Vector3(0), new Vector3(2.899998f, 20, 7.899993f)));
            CollisionBoxes.Add(new Box(new Vector3(201.8757f, 0f, 12.61193f), new Vector3(0), new Vector3(2.899998f, 20, 7.899993f)));
            CollisionBoxes.Add(new Box(new Vector3(199.003f, 0f, 24.80006f), new Vector3(0), new Vector3(20.20004f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(235.8793f, 0f, 24.86195f), new Vector3(0), new Vector3(34.30005f, 20, 0.7999982f)));
            CollisionBoxes.Add(new Box(new Vector3(211.3778f, 0f, 23.06194f), new Vector3(0), new Vector3(4.799996f, 20, 4.299996f)));
            CollisionBoxes.Add(new Box(new Vector3(209.4777f, 0f, 5.26189f), new Vector3(0), new Vector3(1.099998f, 20, 39.99996f)));
            CollisionBoxes.Add(new Box(new Vector3(211.5778f, 0f, -21.73815f), new Vector3(0), new Vector3(5.399995f, 20, 3.499997f)));
            CollisionBoxes.Add(new Box(new Vector3(233.7792f, 0f, -15.23813f), new Vector3(0), new Vector3(9.699999f, 20, 8.799995f)));
            CollisionBoxes.Add(new Box(new Vector3(235.4793f, 0f, -21.63815f), new Vector3(0), new Vector3(2.999998f, 20, 3.599997f)));
            CollisionBoxes.Add(new Box(new Vector3(235.8137f, 0f, -7.906326f), new Vector3(0), new Vector3(2.999998f, 20, 5.599995f)));
            CollisionBoxes.Add(new Box(new Vector3(233.6136f, 0f, -4.106329f), new Vector3(0), new Vector3(10.3f, 20, 2.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(249.9015f, 0f, -8.099995f), new Vector3(0), new Vector3(16.10002f, 20, 4.299996f)));
            CollisionBoxes.Add(new Box(new Vector3(278.4033f, 0f, -7.699995f), new Vector3(0), new Vector3(16.10002f, 20, 4.299996f)));
            CollisionBoxes.Add(new Box(new Vector3(278.4033f, 0f, 3.199999f), new Vector3(0), new Vector3(16.10002f, 20, 4.299996f)));
            CollisionBoxes.Add(new Box(new Vector3(276.2031f, 0f, 13.90002f), new Vector3(0), new Vector3(10.5f, 20, 3.999997f)));
            CollisionBoxes.Add(new Box(new Vector3(247.6014f, 0f, 3f), new Vector3(0), new Vector3(10.5f, 20, 3.999997f)));
            CollisionBoxes.Add(new Box(new Vector3(247.6014f, 0f, 13.70002f), new Vector3(0), new Vector3(10.5f, 20, 3.999997f)));
            CollisionBoxes.Add(new Box(new Vector3(274.503f, 0f, -16.10003f), new Vector3(0), new Vector3(12.00001f, 20, 3.999997f)));
            CollisionBoxes.Add(new Box(new Vector3(273.703f, 0f, 24.80006f), new Vector3(0), new Vector3(29.70008f, 20, 0.8999982f)));
            CollisionBoxes.Add(new Box(new Vector3(273.703f, 0f, 24.80006f), new Vector3(0), new Vector3(29.70008f, 20, 0.8999982f)));
            CollisionBoxes.Add(new Box(new Vector3(252.4459f, 0f, 22.42114f), new Vector3(0), new Vector3(1.499998f, 20, 3.999997f)));
            CollisionBoxes.Add(new Box(new Vector3(288.4481f, 0f, 10.0211f), new Vector3(0), new Vector3(1.099998f, 20, 66.19956f)));
            CollisionBoxes.Add(new Box(new Vector3(288.3481f, 0f, 75.02052f), new Vector3(0), new Vector3(1.099998f, 20, 51.69978f)));
            CollisionBoxes.Add(new Box(new Vector3(339.8512f, 0f, 95.32021f), new Vector3(0), new Vector3(1.099998f, 20, 242.6056f)));
            CollisionBoxes.Add(new Box(new Vector3(308.7792f, 0f, 43.83842f), new Vector3(0), new Vector3(6.099995f, 20, 7.999993f)));
            CollisionBoxes.Add(new Box(new Vector3(320.0799f, 0f, 43.83842f), new Vector3(0), new Vector3(6.099995f, 20, 7.999993f)));
            CollisionBoxes.Add(new Box(new Vector3(320.0799f, 0f, 43.83842f), new Vector3(0), new Vector3(6.099995f, 20, 7.999993f)));
            CollisionBoxes.Add(new Box(new Vector3(331.002f, 0f, 43.81123f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(330.8019f, 0f, 57.21103f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(320.4013f, 0f, 57.21103f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(309.2006f, 0f, 57.21103f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(309.2006f, 0f, 70.41083f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(320.2013f, 0f, 70.41083f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(330.8019f, 0f, 70.41083f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(308.8006f, 0f, 97.21042f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(320.5013f, 0f, 97.21042f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(330.9019f, 0f, 97.21042f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(330.9019f, 0f, 110.0102f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(320.3013f, 0f, 110.0102f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(309.1006f, 0f, 110.0102f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(309.1006f, 0f, 123.61f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(320.2013f, 0f, 123.61f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(330.9019f, 0f, 123.61f), new Vector3(0), new Vector3(6.199995f, 20, 8.399994f)));
            CollisionBoxes.Add(new Box(new Vector3(128.9896f, 0f, 84.41061f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.5903f, 0f, 84.41061f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.791f, 0f, 84.41061f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.791f, 0f, 97.51041f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.5903f, 0f, 97.51041f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(128.8896f, 0f, 97.51041f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(128.8896f, 0f, 110.5102f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.6903f, 0f, 110.5102f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.5909f, 0f, 110.5102f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(128.8896f, 0f, 137.2105f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.5903f, 0f, 137.2105f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.791f, 0f, 137.2105f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.791f, 0f, 150.7113f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.791f, 0f, 150.7113f), new Vector3(0), new Vector3(6.199995f, 20, 7.599993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.6929f, 0f, 150.6951f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(128.9923f, 0f, 150.6951f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(128.9923f, 0f, 163.3947f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(139.6929f, 0f, 163.3947f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(150.7936f, 0f, 163.3947f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(221.6979f, 0f, 174.8954f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(232.7986f, 0f, 174.8954f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.3992f, 0f, 174.8954f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.3992f, 0f, 161.7946f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(232.7986f, 0f, 161.7946f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(221.6979f, 0f, 161.7946f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(221.6979f, 0f, 148.8938f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(232.6986f, 0f, 148.8938f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.5993f, 0f, 148.8938f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(221.5979f, 0f, 116.7927f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(232.6986f, 0f, 116.7927f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.4993f, 0f, 116.7927f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.4993f, 0f, 116.7927f), new Vector3(0), new Vector3(6.099995f, 20, 7.399993f)));
            CollisionBoxes.Add(new Box(new Vector3(243.5682f, 0f, 103.3611f), new Vector3(0), new Vector3(6.299994f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(232.8676f, 0f, 103.3611f), new Vector3(0), new Vector3(6.299994f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(221.6669f, 0f, 103.2611f), new Vector3(0), new Vector3(6.099995f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(221.6669f, 0f, 90.36127f), new Vector3(0), new Vector3(6.099995f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(232.7675f, 0f, 90.36127f), new Vector3(0), new Vector3(6.099995f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(243.4682f, 0f, 90.36127f), new Vector3(0), new Vector3(6.099995f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(243.4682f, 0f, 90.36127f), new Vector3(0), new Vector3(6.099995f, 20, 7.099994f)));
            CollisionBoxes.Add(new Box(new Vector3(238.9928f, 0f, -0.3251197f), new Vector3(0), new Vector3(1.299998f, 20, 50.2998f)));
            CollisionBoxes.Add(new Box(new Vector3(291.402f, 0f, 42.84902f), new Vector3(0), new Vector3(4.799996f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(297.9024f, 0f, 68.04864f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(297.9024f, 0f, 68.04864f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(297.9024f, 0f, 120.3478f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(297.9024f, 0f, 120.3478f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(210.7007f, 0f, 113.9479f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(210.7007f, 0f, 166.2501f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(210.7007f, 0f, 166.2501f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(161.5008f, 0f, 138.8484f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(161.5008f, 0f, 86.84836f), new Vector3(0), new Vector3(3.599997f, 20, 12.20001f)));
            CollisionBoxes.Add(new Box(new Vector3(314.5873f, 0f, 83.35234f), new Vector3(0), new Vector3(52.49977f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(314.5873f, 0f, 136.7522f), new Vector3(0), new Vector3(52.49977f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(314.5873f, 0f, 136.7522f), new Vector3(0), new Vector3(52.49977f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(288.6207f, 0f, 131.2661f), new Vector3(0), new Vector3(0.9999982f, 20, 47.59985f)));
            CollisionBoxes.Add(new Box(new Vector3(288.6207f, 0f, 131.2661f), new Vector3(0), new Vector3(0.9999982f, 20, 47.59985f)));
            CollisionBoxes.Add(new Box(new Vector3(288.6207f, 0f, 188.3696f), new Vector3(0), new Vector3(0.9999982f, 20, 55.19973f)));
            CollisionBoxes.Add(new Box(new Vector3(290.7258f, 0f, 101.5162f), new Vector3(0), new Vector3(5.799995f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(290.7258f, 0f, 101.5162f), new Vector3(0), new Vector3(5.799995f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(291.0258f, 0f, 154.7158f), new Vector3(0), new Vector3(5.799995f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(291.0258f, 0f, 154.7158f), new Vector3(0), new Vector3(5.799995f, 20, 1.299998f)));
            CollisionBoxes.Add(new Box(new Vector3(306.7268f, 0f, 172.8169f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(306.7268f, 0f, 172.8169f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(319.6276f, 0f, 172.8169f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(332.9284f, 0f, 172.8169f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(332.9284f, 0f, 183.4175f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(332.9284f, 0f, 183.4175f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(319.7276f, 0f, 183.4175f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(306.7268f, 0f, 183.4175f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(306.7268f, 0f, 194.5182f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(319.6276f, 0f, 194.5182f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(332.8284f, 0f, 194.5182f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(332.8284f, 0f, 194.5182f), new Vector3(0), new Vector3(7.799993f, 20, 5.499995f)));
            CollisionBoxes.Add(new Box(new Vector3(331.2283f, 0f, 205.5189f), new Vector3(0), new Vector3(12.10001f, 20, 3.599997f)));
            CollisionBoxes.Add(new Box(new Vector3(157.1177f, 0f, 216.2195f), new Vector3(0), new Vector3(366.3131f, 20, 0.9999982f)));
            CollisionBoxes.Add(new Box(new Vector3(-12.62502f, 0f, 190.6946f), new Vector3(0), new Vector3(3.999997f, 20, 11.60001f)));
            CollisionBoxes.Add(new Box(new Vector3(-12.62502f, 0f, 190.6946f), new Vector3(0), new Vector3(3.999997f, 20, 11.60001f)));
            CollisionBoxes.Add(new Box(new Vector3(65.67467f, 0f, 207.2956f), new Vector3(0), new Vector3(3.999997f, 20, 11.60001f)));
            CollisionBoxes.Add(new Box(new Vector3(86.67435f, 0f, 206.6956f), new Vector3(0), new Vector3(0.9999982f, 20, 17.60003f)));
            CollisionBoxes.Add(new Box(new Vector3(86.67435f, 0f, 206.6956f), new Vector3(0), new Vector3(0.9999982f, 20, 17.60003f)));
            CollisionBoxes.Add(new Box(new Vector3(7.674402f, 0f, 206.6956f), new Vector3(0), new Vector3(0.9999982f, 20, 17.60003f)));
            CollisionBoxes.Add(new Box(new Vector3(7.674402f, 0f, 177.8938f), new Vector3(0), new Vector3(0.9999982f, 20, 27.20007f)));
            CollisionBoxes.Add(new Box(new Vector3(86.6745f, 0f, 177.8938f), new Vector3(0), new Vector3(0.9999982f, 20, 27.20007f)));
            CollisionBoxes.Add(new Box(new Vector3(84.1587f, 0f, 198.9931f), new Vector3(0), new Vector3(4.399996f, 20, 1.999998f)));
            CollisionBoxes.Add(new Box(new Vector3(84.1587f, 0f, 198.9931f), new Vector3(0), new Vector3(4.399996f, 20, 1.999998f)));
            CollisionBoxes.Add(new Box(new Vector3(5.259332f, 0f, 199.0932f), new Vector3(0), new Vector3(4.599996f, 20, 2.399998f)));
            CollisionBoxes.Add(new Box(new Vector3(30.85949f, 0f, 164.091f), new Vector3(0), new Vector3(112.5989f, 20, 1.399998f)));
            CollisionBoxes.Add(new Box(new Vector3(30.85949f, 0f, 52.98913f), new Vector3(0), new Vector3(112.5989f, 20, 1.399998f)));
            CollisionBoxes.Add(new Box(new Vector3(86.65821f, 0f, 96.06586f), new Vector3(0), new Vector3(1.199998f, 20, 85.59927f)));
            CollisionBoxes.Add(new Box(new Vector3(86.65821f, 0f, 154.267f), new Vector3(0), new Vector3(1.199998f, 20, 17.10003f)));
            CollisionBoxes.Add(new Box(new Vector3(84.45824f, 0f, 146.5665f), new Vector3(0), new Vector3(5.799995f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(84.45824f, 0f, 146.5665f), new Vector3(0), new Vector3(5.799995f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(167.8594f, 0f, 106.1664f), new Vector3(0), new Vector3(5.799995f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(167.9594f, 0f, 164.2671f), new Vector3(0), new Vector3(5.799995f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(167.9594f, 0f, 164.2671f), new Vector3(0), new Vector3(5.799995f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(204.1604f, 0f, 147.1657f), new Vector3(0), new Vector3(-5.300182f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(204.1604f, 0f, 88.8657f), new Vector3(0), new Vector3(-5.300182f, 20, 1.699998f)));
            CollisionBoxes.Add(new Box(new Vector3(181.061f, 0f, 22.66546f), new Vector3(0), new Vector3(4.299996f, 20, 4.199996f)));
            CollisionBoxes.Add(new Box(new Vector3(227.3041f, 0f, 70.68988f), new Vector3(0), new Vector3(51.79978f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(227.3041f, 0f, 70.68988f), new Vector3(0), new Vector3(51.79978f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(227.3041f, 0f, 129.2897f), new Vector3(0), new Vector3(51.79978f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(227.3041f, 0f, 181.7905f), new Vector3(0), new Vector3(51.79978f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(227.3041f, 0f, 181.7905f), new Vector3(0), new Vector3(51.79978f, 20, 1.599998f)));
            CollisionBoxes.Add(new Box(new Vector3(144.8806f, 0f, 181.7463f), new Vector3(0), new Vector3(52.09978f, 20, 1.499998f)));
            CollisionBoxes.Add(new Box(new Vector3(144.8806f, 0f, 123.8455f), new Vector3(0), new Vector3(52.09978f, 20, 1.499998f)));
            CollisionBoxes.Add(new Box(new Vector3(144.8806f, 0f, 71.04543f), new Vector3(0), new Vector3(52.09978f, 20, 1.499998f)));
            CollisionBoxes.Add(new Box(new Vector3(119.3302f, 0f, 126.6199f), new Vector3(0), new Vector3(0.9999982f, 20, 109.9989f)));
            CollisionBoxes.Add(new Box(new Vector3(252.631f, 0f, 126.6199f), new Vector3(0), new Vector3(0.9999982f, 20, 109.9989f)));
            CollisionBoxes.Add(new Box(new Vector3(201.5354f, 0f, 167.8129f), new Vector3(0), new Vector3(0.9999982f, 20, 28.10007f)));
            CollisionBoxes.Add(new Box(new Vector3(201.5354f, 0f, 167.8129f), new Vector3(0), new Vector3(0.9999982f, 20, 28.10007f)));
            CollisionBoxes.Add(new Box(new Vector3(170.5335f, 0f, 85.21082f), new Vector3(0), new Vector3(0.9999982f, 20, 28.10007f)));
            CollisionBoxes.Add(new Box(new Vector3(170.5335f, 0f, 131.2094f), new Vector3(0), new Vector3(0.9999982f, 20, 51.49979f)));
            CollisionBoxes.Add(new Box(new Vector3(201.6354f, 0f, 121.3093f), new Vector3(0), new Vector3(0.9999982f, 20, 51.69978f)));
            CollisionBoxes.Add(new Box(new Vector3(201.9354f, 0f, 79.80998f), new Vector3(0), new Vector3(0.9999982f, 20, 19.30004f)));
            CollisionBoxes.Add(new Box(new Vector3(170.4365f, 0f, 172.4124f), new Vector3(0), new Vector3(0.9999982f, 20, 18.40003f)));
            CollisionBoxes.Add(new Box(new Vector3(154.8109f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(154.8109f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(131.5094f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(131.5094f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(215.4115f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(238.6129f, 0f, 184.4977f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(238.6129f, 0f, 69.89781f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(238.6129f, 0f, 69.89781f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(215.4115f, 0f, 69.89781f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(154.8109f, 0f, 69.89781f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(131.7094f, 0f, 69.89781f), new Vector3(0), new Vector3(12.70001f, 20, 3.699997f)));
            CollisionBoxes.Add(new Box(new Vector3(4.032712f, 0f, 69.334f), new Vector3(0), new Vector3(20.40004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(4.032712f, 0f, 92.03365f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(4.032712f, 0f, 113.5333f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(4.032712f, 0f, 113.5333f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(4.032712f, 0f, 135.8336f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(58.53277f, 0f, 135.8336f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(58.53277f, 0f, 135.8336f), new Vector3(0), new Vector3(20.80004f, 20, 11.90001f)));
            CollisionBoxes.Add(new Box(new Vector3(58.54241f, 0f, 113.1667f), new Vector3(0), new Vector3(20.90004f, 20, 11.80001f)));
            CollisionBoxes.Add(new Box(new Vector3(58.54241f, 0f, 91.06671f), new Vector3(0), new Vector3(20.90004f, 20, 11.80001f)));
            CollisionBoxes.Add(new Box(new Vector3(58.54241f, 0f, 69.16705f), new Vector3(0), new Vector3(20.90004f, 20, 11.80001f)));
            #endregion  

            //CollisionBoxes.Add(new Box(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), new Vector3(0), new Vector3(10, 20, 10)));
            LevelQuadTree = new QuadTree(new Vector2(156, 65), 185, 4);
            
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
            HUD.ActiveHUD.p = Player.Position;
            HUD.ActiveHUD.angle = (float) Player.Rotation;
            HUD.ActiveHUD.playerhealth = (int)((float)Player.HealthPoints / (float)Player.MaxHealth * 100);
            Camera.ActiveCamera.dudeang = (float) Player.Rotation;

            mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                sound.playgun();
                Player.HealthPoints = 0;
                foreach (Zombie z in zombies)
                    z.animState = Entity.AnimationState.Attacking;
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

            
            float modifier = 0.10f;

            if(keyboard.IsKeyDown(Keys.RightShift))
            {
                modifier = 10;
            }
            //Rotate World with Arrow Keys
            if (keyboard.IsKeyDown(Keys.K))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position += new Vector3(0, 0, modifier);
            }
            if (keyboard.IsKeyDown(Keys.I))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position -= new Vector3(0, 0, modifier);
            }
            if (keyboard.IsKeyDown(Keys.L))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position += new Vector3(modifier, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.J))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Position -= new Vector3(modifier, 0, 0);
            }

            if (keyboard.IsKeyDown(Keys.Y))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size += new Vector3(modifier, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.U))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size -= new Vector3(modifier, 0, 0);
            }
            if (keyboard.IsKeyDown(Keys.O))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size += new Vector3(0, 0, modifier);
            }
            if (keyboard.IsKeyDown(Keys.P))
            {
                CollisionBoxes[CollisionBoxes.Count - 1].Size -= new Vector3(0, 0, modifier);
            }

            //Toggle Collision Boundaries Display
            if (keyboard.IsKeyDown(Keys.C) && ButtonTimer <= 0)
            {
                WireFrameCollisionBoxes = !WireFrameCollisionBoxes;
                ButtonTimer = 10;
            }

            //Toggle QuadTree Boundaries Display
            if (keyboard.IsKeyDown(Keys.B) && ButtonTimer <= 0)
            {
                ShowQuadBoundaries = !ShowQuadBoundaries;
                ButtonTimer = 10;
            }

            //Toggle Collision Boxes Display
            if (keyboard.IsKeyDown(Keys.N) && ButtonTimer <= 0)
            {
                ShowCollisionBoxes = !ShowCollisionBoxes;
                ButtonTimer = 10;
            }

            if (keyboard.IsKeyDown(Keys.Enter) && ButtonTimer <= 0)
            {
                Debug.WriteLine("CollisionBoxes.Add(new Box(new Vector3(" + CollisionBoxes[CollisionBoxes.Count-1].Position.X + "f, " +  CollisionBoxes[CollisionBoxes.Count-1].Position.Y + "f, " + CollisionBoxes[CollisionBoxes.Count-1].Position.Z +"f) , new Vector3(0), new Vector3(" + CollisionBoxes[CollisionBoxes.Count-1].Size.X + "f, 20 , " + CollisionBoxes[CollisionBoxes.Count-1].Size.Z +"f)));");
                CollisionBoxes.Add(new Box(CollisionBoxes[CollisionBoxes.Count-1].Position,new Vector3(0),CollisionBoxes[CollisionBoxes.Count-1].Size));

                LevelQuadTree.Insert(CollisionBoxes[CollisionBoxes.Count - 1]);

                if (keyboard.IsKeyDown(Keys.RightShift))
                {
                    CollisionBoxes.Add(new Box(new Vector3(CollisionBoxes[CollisionBoxes.Count - 1].Position.X, CollisionBoxes[CollisionBoxes.Count - 1].Position.Y, CollisionBoxes[CollisionBoxes.Count - 1].Position.Z), new Vector3(0), new Vector3(CollisionBoxes[CollisionBoxes.Count - 1].Size.X, 20, CollisionBoxes[CollisionBoxes.Count - 1].Size.Z)));
                }
                else
                {
                    CollisionBoxes.Add(new Box(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), new Vector3(0), new Vector3(10, 20, 10)));
                }
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
            else if (Player.animState != Entity.AnimationState.Hurt && Player.animState != Entity.AnimationState.Dying)
                Player.animState = Entity.AnimationState.Idle;
           

            #endregion
         
            if (Player.Dead)
            {
                // TODO: GameOver
            }
            else
                Player.Update(gameTime);

            //update right zombies
            List<Zombie> deadZombies = new List<Zombie>();
            foreach (Zombie z in zombies)//update zombies
            {
                //This checks a radius around the player to see whether or not we should be updating the zombie
                //If zombie is out of radius, we must still check to see if it is chasing the character. 
                //If that is the case then we still need to update, but not to draw.
                if ((z.Position - Player.Position).Length() < SIGHT_RADIUS || z.BehaviouralState != BehaviourState.Wander)
                {
                    if (z.Dead)
                        deadZombies.Add(z);
                    else
                        z.Update(gameTime);
                }
            }
            // remove any dead zombies
            foreach (Zombie dz in deadZombies)
            {
                zombies.Remove(dz);
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
            LevelQuadTree.RetrieveNearbyObjects(heroSphere, ref primitivesNearby,2);

            /*foreach (Primitive bx in primitivesNearby)
            {
                if (!TotalNearbyBoxes.Contains(bx))
                    TotalNearbyBoxes.Add(bx);
            }*/

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
                if ((z.Position - Player.Position).Length() < SIGHT_RADIUS || z.BehaviouralState != BehaviourState.Wander)
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
            
            foreach (Zombie z1 in zombies)
            {
                if ((z1.Position - Player.Position).Length() < SIGHT_RADIUS || z1.BehaviouralState != BehaviourState.Wander)
                {
                    checkZombietoPlayer(z1);
                    foreach (Zombie z2 in zombies)
                    {
                        if (!z2.Equals(z1) && ((z2.Position - Player.Position).Length() < SIGHT_RADIUS || z2.BehaviouralState != BehaviourState.Wander))
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
                DoAttack(objectCasted as Weapon, actionCaster);   
            }
            else if (objectCasted is Item)
            {
                CastItem(objectCasted as Item, actionCaster);   
            }
        }

        private void DoAttack(Weapon weapon, Entity actionCaster)
        {
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
                        Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
                        foreach (Zombie z in zombies)
                        {
                            if ((z.Position - actionCaster.Position).Length() < weapon.Range)
                            {
                                BoundingSphere bs = new BoundingSphere(z.Position, z.modelRadius);
                                if (ray.Intersects(bs) != null)
                                    z.TakeDamage(weapon.FirePower);
                            }
                        }
                        break;
                    }
                case WeaponType.Handgun9mm:
                    {
                        DoGunAttack(weapon, actionCaster);
                        break;
                    }
                case WeaponType.Magnum:
                    {
                        DoGunAttack(weapon, actionCaster);
                        break;
                    }
                case WeaponType.Vomit:
                    {
                        Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
                        if ((actionCaster.Position - Player.Position).Length() < weapon.Range)
                        {
                            BoundingSphere bs = new BoundingSphere(Player.Position, Player.modelRadius);
                            if (ray.Intersects(bs) != null)
                                Player.TakeDamage(weapon.FirePower);
                        }
                        break;
                    }
                case WeaponType.ZombieHands:
                    {
                        Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
                        if ((actionCaster.Position - Player.Position).Length() < weapon.Range)
                        {
                            BoundingSphere bs = new BoundingSphere(Player.Position, Player.modelRadius);
                            if (bs.Intersects(ray) != null)
                                Player.TakeDamage(weapon.FirePower);
                        }
                        break;
                    }
            }
        }

        private void DoGunAttack(Weapon weapon, Entity actionCaster)
        {

            //do soundeffect attached
            if (weapon.weaponType == WeaponType.Handgun9mm)
            { 
                //need to check the time
                    sound.playgun();
            }
           
   
            // find closest zombie, if any, in the line of fire and have him take the damage
            Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
            Zombie closestVictim = null;
            float? closestIntersect = 100;
            foreach (Zombie z in zombies)
            {
                if ((z.Position - actionCaster.Position).Length() < weapon.Range)
                {
                    BoundingSphere bs = new BoundingSphere(z.Position, z.modelRadius);
                    float? intersection = ray.Intersects(bs);
                    if (intersection != null && intersection < closestIntersect)
                    {
                        closestIntersect = intersection;
                        closestVictim = z;
                    }
                }
            }

            // check if ray intersects nearby primitives from quad tree
            // if so, check if intersections are closer than the closest zombie intersection
            Sphere heroSphere = new Sphere(actionCaster.Position, actionCaster.Velocity, actionCaster.modelRadius);
            List<Primitive> primitives = new List<Primitive>();
            LevelQuadTree.RetrieveNearbyObjects(heroSphere, ref primitives);

            foreach (Box box in primitives)
            {
                BoundingBox bbox = new BoundingBox(
                    new Vector3(box.Position.X - box.Size.X / 2, box.Position.Y - box.Size.Y / 2, box.Position.Z - box.Size.Z / 2), 
                    new Vector3(box.Position.X + box.Size.X / 2, box.Position.Y + box.Size.Y / 2, box.Position.Z + box.Size.Z / 2)
                );
                if (ray.Intersects(bbox) != null && ray.Intersects(bbox) < closestIntersect)
                    return;
            }
            if (closestVictim != null)
            {
                closestVictim.Alert(actionCaster as Hero);
                if (weapon.weaponType == WeaponType.Magnum && closestIntersect > 20)
                    closestVictim.TakeDamage(weapon.FirePower / 10);
                else if (weapon.weaponType == WeaponType.Magnum && closestIntersect > 10)
                    closestVictim.TakeDamage(weapon.FirePower / 5);
                else
                    closestVictim.TakeDamage(weapon.FirePower);
            }
        }

        private void CastItem(Item item, Entity actionCaster)
        {
            CastSoundWave(item.SoundRadius);

            switch (item.itemType)
            {
                case ItemType.MedPack:
                    {
                        // Regenerate anywhere between 25 and 75% of total health
                        Random rand = new Random((int)DateTime.Now.Ticks);
                        float next = rand.Next(25, 76);
                        next /= 100;
                        Player.Heal((int)(Player.MaxHealth * next));
                        Player.ItemsList[item]--;
                        break;
                    }
                case ItemType.Key:
                    {
                        //if (lock in range)
                        //{
                        //      unlock
                        Player.ItemsList[item]--;
                        //}
                        break;
                    }
                case ItemType.Extinguisher:
                    {
                        List<Box> firesToRemove = new List<Box>();
                        Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
                        foreach (Box hazard in fireHazards)
                        {
                            if ((hazard.Position - Player.Position).Length() < item.Range)
                            {
                                BoundingBox bbox = new BoundingBox(
                                    new Vector3(hazard.Position.X - hazard.Size.X / 2, hazard.Position.Y - hazard.Size.Y / 2, hazard.Position.Z - hazard.Size.Z / 2),
                                    new Vector3(hazard.Position.X + hazard.Size.X / 2, hazard.Position.Y + hazard.Size.Y / 2, hazard.Position.Z + hazard.Size.Z / 2)
                                );
                                if (ray.Intersects(bbox) != null)
                                {
                                    firesToRemove.Add(hazard);
                                }
                            }
                        }
                        // remove any intersecting fires
                        foreach (Box toRemove in firesToRemove)
                        {
                            fireHazards.Remove(toRemove);
                        }
                        Player.ItemsList[item]--;
                        break;
                    }
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

            if (ShowCollisionBoxes)
            {
                foreach (Box box in CollisionBoxes)
                {
                    DrawBox(box, Color.Red, WireFrameCollisionBoxes);
                }
            }


            if (ShowQuadBoundaries)
            {
                List<Box> QuadBoxes = new List<Box>();
                LevelQuadTree.RetrieveBoundariesFromPosition(new Sphere(Player.Position, Player.Velocity, Player.modelRadius), ref QuadBoxes);

                foreach (Box bx in QuadBoxes)
                {
                    DrawBox(bx, Color.White, true);
                }

                //Display all items currently being checked for collisions this frame for all characters

                foreach (Box box in TotalNearbyBoxes)
                {
                    DrawBox(box, Color.Chartreuse, false);
                }

                TotalNearbyBoxes.Clear();

            }

            foreach (Zombie z in zombies)
            {
                if ((z.Position - Player.Position).Length() < SIGHT_RADIUS)
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
            
            //draw rays if player in shooting stance
            if (Player.Stance == AnimationStance.Shooting)
            {
                globalEffect.View = Camera.ActiveCamera.View;
                globalEffect.Projection = Camera.ActiveCamera.Projection;
                globalEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, Player.ray, 0, 1);
            }

            float heroRotation;
            if (Player.animState == Entity.AnimationState.Hurt)
            {
                heroRotation = (float)hero.Rotation;
            }
            else
            {
                heroRotation = (float)(hero.Rotation - Math.PI);
            }

            if (Player.Stance == AnimationStance.Shooting)
            {
                // Render the skinned mesh
                foreach (ModelMesh mesh in hero.EquippedWeapon.model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateTranslation(hero.weaponoffset) * Matrix.CreateRotationY((float)(hero.Rotation + Math.PI / 2)) * Matrix.CreateTranslation(hero.Position);// 

                        effect.View = Camera.ActiveCamera.View;
                        effect.Projection = Camera.ActiveCamera.Projection;

                        effect.EnableDefaultLighting();

                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }

                    mesh.Draw();
                }
            }

            // Render the skinned mesh
            foreach (ModelMesh mesh in hero.model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY(heroRotation) * Matrix.CreateScale(hero.scale) * Matrix.CreateTranslation(hero.Position);// 
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

            float zombieRotation;
            if (zombie.animState == Entity.AnimationState.Attacking || zombie.animState == Entity.AnimationState.Hurt ||
                zombie.animState == Entity.AnimationState.Dying)
            {
                zombieRotation = (float)zombie.Rotation;
            }
            else
            {
                zombieRotation = (float)(zombie.Rotation - Math.PI);
            }

            // Render the skinned mesh
            foreach (ModelMesh mesh in zombie.model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY(zombieRotation) *
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