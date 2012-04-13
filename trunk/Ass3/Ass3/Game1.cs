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
using PathFinding;
using Particles;

namespace zombies
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteFont Font1;
        SpriteFont SplashFont;
        SpriteBatch spriteBatch;

        MouseState mouseState;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        public Viewport frontViewport;
        public Viewport Viewport = new Viewport(new Rectangle(0, 0, 1300, 700));

        List<Box> CollisionBoxes = new List<Box>();
        List<Primitive> TotalNearbyBoxes = new List<Primitive>();

        #region PathFinding

        List<PathFinding.Node> PathFindingNodes = new List<PathFinding.Node>();
        VertexPositionColor[] vpc;
        //Total Link counts for graph nodes
        int LinksCount = 0;

        //A* object
        AStar path = new AStar();

        int nodeIndex = 0;

        int currentNode = 0;
        int DestinationNode = 1;

        //List of nodes for the path to follow obtained from A* computation
        List<PathFinding.Node> NodeList;

        #endregion

        #region Particles

        ParticleEmitter FireEmitter;
        ParticleEmitter FireEmitter2;
        ParticleEmitter FireEmitter3;
        ParticleEmitter FireEmitter4;
        ParticleEmitter ChemicalsEmitter;

        Texture2D fire;
        Texture2D smoke;
        #endregion

        HUD hud;

        #region Models
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
        Model Silenced9mm;

        Model NodeModel;
        Model StartNode;
        Model EndNode;

        //weapon/item/powerup models
        Model handgunModel;
        Model magnumModel;
        Model medkitModel;
        Model extinguisherModel;
        Model keyModel;
        Model silencerModel;
        Model sneakerModel;
        #endregion

        int ButtonTimer = 0;

        BasicEffect globalEffect;
        QuadTree LevelQuadTree;

        Texture2D Splash;
        Texture2D Controls;

        public static GameStates.GameStates.GameState ZombieGameState = GameStates.GameStates.GameState.Start;
        bool WireFrameCollisionBoxes = false;
        bool ShowQuadBoundaries = false;
        bool ShowCollisionBoxes = false;
        bool ShowPathFindingGraph = false;

        int StartGameOption = 0;

        Hero Player;
        List<Zombie> zombies;
        List<Box> fireHazards;

        List<Entity> PickupableObjects;

        Weapon magnum;
        Weapon socom;
        Powerup silencer;
        Powerup sneakers;
        Item medkit1;
        Item medkit2;
        Item medkit3;
        Item extinguisher;
        Item key1;
        Item key2;

        int scrollWheel = 0;
        int scrollWheelLow = 0;
        int scrollWheelHigh = 50;

        const int SIGHT_RADIUS = 60;
        const float COLLISON_SOUND_RADIUS = 30;
        const int COLLISION_ITEM_RANGE = 5;
        const float WALK_SOUND_RADIUS = 25;

        float ItemRotation = 0f;
        float ItemHeight = 0f;

        //sound
        Sounds.Sounds sound;

        int fireDamageDelay = 0;
        bool released = true;

        Sphere EscapeSpot = new Sphere(new Vector3(175.4254f, 0f, -103.5071f), Vector3.Zero, 5);
        
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
            PickupableObjects = new List<Entity>();
         
            base.Initialize();
        }
      
        protected override void LoadContent()
        {
            hud.ContentLoad();
            PickupableObjects.Clear();
            CollisionBoxes.Clear();
            PathFindingNodes.Clear();
            zombies.Clear();
            fireHazards.Clear();

            //sounds
       
            sound = new Sounds.Sounds(this, Content);
            sound.LoadSounds();
            this.Components.Add(sound);

            Font1 = Content.Load<SpriteFont>("Arial");
            SplashFont = Content.Load<SpriteFont>("SplashFont");
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
            
            NodeModel = Content.Load<Model>("Pyramid");
            StartNode = Content.Load<Model>("SPyramid");
            EndNode = Content.Load<Model>("EPyramid");

            smoke = Content.Load<Texture2D>("Smoke2");
            fire = Content.Load<Texture2D>("Fire2");

            //weapon/item/powerup models
            Silenced9mm = Content.Load<Model>("socom9mmsilencer");
            magnumModel = Content.Load<Model>("Magnum");
            handgunModel = Content.Load<Model>("socom9mm");
            silencerModel = Content.Load<Model>("Silencer");
            medkitModel = Content.Load<Model>("MedKit");
            keyModel = Content.Load<Model>("Key");
            extinguisherModel = Content.Load<Model>("Extinguisher");
            sneakerModel = Content.Load<Model>("Sneakers");

            Splash = Content.Load<Texture2D>("Splash");
            Controls = Content.Load<Texture2D>("keyboard");
            
            magnum = new Weapon(WeaponType.Magnum, ref magnumModel);
            magnum.Position = new Vector3(70f, 0, -15);
            socom = new Weapon(WeaponType.Handgun9mm, ref handgunModel);
            socom.Position = new Vector3(293.3976f, 0, 123.4541f);
            silencer = new Powerup(PowerupType.Silencer, ref silencerModel);
            silencer.Position = new Vector3(-6.841653f, 0, 191.983f);
            sneakers = new Powerup(PowerupType.Sneakers, ref sneakerModel);
            sneakers.Position = new Vector3(80.49309f, 0, -13.14439f);
            medkit1 = new Item(ItemType.MedPack, ref medkitModel);
            medkit1.Position = new Vector3(335.4893f, 0, -14.48104f);
            medkit2 = new Item(ItemType.MedPack, ref medkitModel);
            medkit2.Position = new Vector3(18.04729f,0,-14.43179f);
            medkit3 = new Item(ItemType.MedPack, ref medkitModel);
            medkit3.Position = new Vector3(60.17641f,0,206.8075f);
            key1 = new Item(ItemType.Key, ref keyModel);
            key1.Position = new Vector3(323.8057f, 0, -8.779925f);
            key2 = new Item(ItemType.Key, ref keyModel);
            extinguisher = new Item(ItemType.Extinguisher, ref extinguisherModel);
            extinguisher.Position = new Vector3(-21.04327f, 0, 79.15403f);

            PickupableObjects.Add(socom);
            PickupableObjects.Add(magnum);
            PickupableObjects.Add(silencer);
            PickupableObjects.Add(sneakers);
            PickupableObjects.Add(medkit1);
            PickupableObjects.Add(medkit2);
            PickupableObjects.Add(medkit3);
            PickupableObjects.Add(key1);
            PickupableObjects.Add(key2);
            PickupableObjects.Add(extinguisher);
            Player = new Hero(1000, 1000, ref HeroWalk, ref HeroDie, ref HeroHurt, DoAction);
            Player.Position = new Vector3(316.9466f, 0, 202.9034f);

            #region Zombie placement
            Zombie z1 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z1.Position = new Vector3(301.519f, 0, 145.7045f);
            Zombie z2 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z2.Position = new Vector3(269.3711f, 0, 190.6429f);
            Zombie z3 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z3.Position = new Vector3(261.2204f, 0, 93.19714f);
            Zombie z4 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z4.Position = new Vector3(336.6563f, 0, 97.20895f);
            Zombie z5 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z5.Position = new Vector3(336.6563f, 0, 97.20895f);
            Zombie z6 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z6.Position = new Vector3(329.1409f, 0, 131.5164f);
            Zombie z7 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z7.Position = new Vector3(301.1594f, 0, 65.94746f);
            Zombie z8 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z8.Position = new Vector3(328.5325f, 0, 25.91457f);
            Zombie z9 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z9.Position = new Vector3(303.7029f, 0, 9.12639f);
            Zombie z10 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z10.Position = new Vector3(317.9062f, 0, -3.754462f);
            Zombie z11 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z11.Position = new Vector3(249.0774f, 0, 8.751559f);
            Zombie z12 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z12.Position = new Vector3(249.0774f, 0, 8.751559f);
            Zombie z13 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z13.Position = new Vector3(247.3187f, 0, -14.76119f);
            Zombie z14 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z14.Position = new Vector3(258.4365f, 0, -18.24927f);
            Zombie z15 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z15.Position = new Vector3(258.4365f, 0, -18.24927f);
            Zombie z16 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z16.Position = new Vector3(239.0731f, 0, 60.01192f);
            Zombie z17 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z17.Position = new Vector3(232.3299f, 0, -7.815142f);
            Zombie z18 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z18.Position = new Vector3(183.7789f, 0, -19.10403f);
            Zombie z19 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z19.Position = new Vector3(187.8999f, 0, -4.894011f);
            Zombie z20 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z20.Position = new Vector3(188.1283f, 0, 9.704206f);
            Zombie z21 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z21.Position = new Vector3(200.033f, 0, 2.194498f);
            Zombie z22 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z22.Position = new Vector3(196.9581f, 0, 9.796185f);
            Zombie z23 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z23.Position = new Vector3(185.8409f, 0, 99.64986f);
            Zombie z24 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z24.Position = new Vector3(180.9995f, 0, 169.6039f);
            Zombie z25 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z25.Position = new Vector3(167.7615f, 0, 172.059f);
            Zombie z26 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z26.Position = new Vector3(122.0838f, 0, 144.119f);
            Zombie z27 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z27.Position = new Vector3(123.5005f, 0, 176.1559f);
            Zombie z28 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z28.Position = new Vector3(213.768f, 0, 135.7222f);
            Zombie z29 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z29.Position = new Vector3(246.0834f, 0, 138.0532f);
            Zombie z30 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z30.Position = new Vector3(214.1074f, 0, 171.7199f);
            Zombie z31 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z31.Position = new Vector3(206.5015f, 0, 175.7075f);
            Zombie z32 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z32.Position = new Vector3(165.1259f, 0, 88.55122f);
            Zombie z33 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z33.Position = new Vector3(145.6952f, 0, 103.8975f);
            Zombie z34 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z34.Position = new Vector3(135.175f, 0, 91.71438f);
            Zombie z35 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z35.Position = new Vector3(208.605f, 0, 123.8601f);
            Zombie z36 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z36.Position = new Vector3(62.17467f, 0, 206.7475f);
            Zombie z37 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z37.Position = new Vector3(41.59457f, 0, 193.5849f);
            Zombie z38 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z38.Position = new Vector3(28.12794f, 0, 176.6967f);
            Zombie z39 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z39.Position = new Vector3(-5.158183f, 0, 177.9941f);
            Zombie z40 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z40.Position = new Vector3(-19.26436f, 0, 180.8879f);
            Zombie z41 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z41.Position = new Vector3(-18.53246f, 0, 204.0769f);
            Zombie z42 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z42.Position = new Vector3(-8.234371f, 0, 206.5888f);
            Zombie z43 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z43.Position = new Vector3(41.16431f, 0, 142.7086f);
            Zombie z44 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z44.Position = new Vector3(24.07458f, 0, 113.5347f);
            Zombie z45 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z45.Position = new Vector3(-13.25363f, 0, 102.294f);
            Zombie z46 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z46.Position = new Vector3(-10.16963f, 0, 68.20435f);
            Zombie z47 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z47.Position = new Vector3(57.56277f, 0, 78.36703f);
            Zombie z48 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z48.Position = new Vector3(33.51163f, 0, 4.10241f);
            Zombie z49 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z49.Position = new Vector3(64.64824f, 0, -2.653803f);
            Zombie z50 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z50.Position = new Vector3(11.22738f, 0, -0.01383802f);
            Zombie z51 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z51.Position = new Vector3(-5.455381f, 0, -17.11485f);
            Zombie z52 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z52.Position = new Vector3(-12.90771f, 0, 3.014349f);
            Zombie z53 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z53.Position = new Vector3(135.701f, 0, -78.65513f);
            Zombie z54 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z54.Position = new Vector3(110.1617f, 0, -78.243f);
            Zombie z55 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z55.Position = new Vector3(98.60985f, 0, -99.87992f);
            Zombie z57 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z57.Position = new Vector3(154.5468f, 0, -104.6467f);
            Zombie z58 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z58.Position = new Vector3(164.7363f, 0, -92.75577f);
            Zombie z59 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z59.Position = new Vector3(166.331f, 0, -70.69667f);
            Zombie z60 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z60.Position = new Vector3(136.4978f, 0, -66.72479f);
            Zombie z61 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z61.Position = new Vector3(107.2373f, 0, -69.5914f);
            Zombie z62 = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z62.Position = new Vector3(158.4751f, 0, -46.98093f);
            // BOSS
            Zombie z56 = new Zombie(500, 500, ZombieType.Boss, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);
            z56.Position = new Vector3(125.8402f, 0, -105.1298f);
            zombies.Add(z1);
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
            zombies.Add(z12);
            zombies.Add(z13);
            zombies.Add(z14);
            zombies.Add(z15);
            zombies.Add(z16);
            zombies.Add(z17);
            zombies.Add(z18);
            zombies.Add(z19);
            zombies.Add(z20);
            zombies.Add(z21);
            zombies.Add(z22);
            zombies.Add(z23);
            zombies.Add(z24);
            zombies.Add(z25);
            zombies.Add(z26);
            zombies.Add(z27);
            zombies.Add(z28);
            zombies.Add(z29);
            zombies.Add(z30);
            zombies.Add(z31);
            zombies.Add(z32);
            zombies.Add(z33);
            zombies.Add(z34);
            zombies.Add(z35);
            zombies.Add(z36);
            zombies.Add(z37);
            zombies.Add(z38);
            zombies.Add(z39);
            zombies.Add(z40);
            zombies.Add(z41);
            zombies.Add(z42);
            zombies.Add(z43);
            zombies.Add(z44);
            zombies.Add(z45);
            zombies.Add(z46);
            zombies.Add(z47);
            zombies.Add(z48);
            zombies.Add(z49);
            zombies.Add(z50);
            zombies.Add(z51);
            zombies.Add(z52);
            zombies.Add(z53);
            zombies.Add(z54);
            zombies.Add(z55);
            zombies.Add(z56);
            zombies.Add(z57);
            zombies.Add(z58);
            zombies.Add(z59);
            zombies.Add(z60);
            zombies.Add(z61);
            zombies.Add(z62);
            #endregion


            fireHazards.Add(new Box(new Vector3(90, 0f, 32f), new Vector3(0), new Vector3(15, 20, 15)));
            fireHazards[fireHazards.Count - 1].Tag = "Fire1";
            fireHazards.Add(new Box(new Vector3(284f, 0f, 48f), new Vector3(0), new Vector3(15, 20, 15)));
            fireHazards[fireHazards.Count - 1].Tag = "Fire2";
            fireHazards.Add(new Box(new Vector3(87, 0f, 193f), new Vector3(0), new Vector3(15, 20, 15)));
            fireHazards[fireHazards.Count - 1].Tag = "Fire3";
            fireHazards.Add(new Box(new Vector3(332, 0, -11), new Vector3(0), new Vector3(25, 20, 25)));
            fireHazards[fireHazards.Count - 1].Tag = "Fire4";

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

            #region Tile Graph Node Positions & Links

            #region PathFinding Nodes
            
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-15.35164f, 0, -16.44704f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(0.09557578f, 0f, -20.07885f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(14.09468f, 0f, -18.19448f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(19.43151f, 0f, 20.17436f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(7.879409f, 0f, 14.97002f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-16.22752f, 0f, 18.68802f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(7.390346f, 0f, 25.63806f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-20.79065f, 0f, 32.00419f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-20.75066f, 0f, 45.63241f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(7.696711f, 0f, 37.37004f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(46.32244f, 0f, 37.41763f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(26.91913f, 0f, 46.73075f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(28.07413f, 0f, 29.63637f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(47.71189f, 0f, 22.71434f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(29.15321f, 0f, 17.05916f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(30.50124f, 0f, -16.75489f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(47.85756f, 0f, -16.7792f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(40.40689f, 0f, -2.579709f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(55.86055f, 0f, -17.48518f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(79.4602f, 0f, -17.38075f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(78.78133f, 0f, 2.807838f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(66.7085f, 0f, 4.118504f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(57.2519f, 0f, 4.920552f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(57.36026f, 0f, 21.7656f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(67.16962f, 0f, 19.8223f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(77.17921f, 0f, 20.61698f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(66.22379f, 0f, -17.24237f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(79.04694f, 0f, 37.10854f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(103.3171f, 0f, 42.96046f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(129.9222f, 0f, 46.12717f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(155.4046f, 0f, 43.6843f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(153.7745f, 0f, 57.98138f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(129.8762f, 0f, 56.97039f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(101.5925f, 0f, 62.08232f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(104.4327f, 0f, 91.63914f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(105.2114f, 0f, 114.8853f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(105.9136f, 0f, 139.8976f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(103.5688f, 0f, 162.7132f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(100.8868f, 0f, 186.1718f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(105.3463f, 0f, 200.2062f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(85.76019f, 0f, 194.5767f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(71.48508f, 0f, 195.1986f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(80.72269f, 0f, 210.341f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(80.26165f, 0f, 171.8739f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(54.21544f, 0f, 172.5661f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(44.02921f, 0f, 196.6723f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(28.31092f, 0f, 172.9682f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(18.9328f, 0f, 196.3713f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(3.482738f, 0f, 193.8009f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(4.279698f, 0f, 168.9292f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-21.46803f, 0f, 170.9293f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-21.48966f, 0f, 191.0246f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-20.88673f, 0f, 210.1434f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(2.76957f, 0f, 211.4009f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-5.185376f, 0f, 191.7433f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(13.86967f, 0f, 211.8432f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(10.95009f, 0f, 168.4767f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(143.0915f, 0f, 198.9829f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(180.9991f, 0f, 200.3684f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(224.952f, 0f, 197.0084f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(268.947f, 0f, 201.1505f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(268.0017f, 0f, 158.8194f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(289.7387f, 0f, 157.5166f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(292.8582f, 0f, 140.9566f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(337.0265f, 0f, 141.5117f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(326.0737f, 0f, 166.3145f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.7442f, 0f, 166.2047f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(295.5839f, 0f, 166.7941f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(296.0285f, 0f, 177.8808f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(312.9738f, 0f, 177.7108f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(326.3319f, 0f, 178.5773f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(325.5595f, 0f, 189.9125f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.5378f, 0f, 189.738f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(295.7057f, 0f, 188.7495f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(290.6207f, 0f, 212.0189f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.6078f, 0f, 210.2791f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(325.8346f, 0f, 201.5484f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(312.9616f, 0f, 200.5446f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(335.1603f, 0f, 212.6843f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(269.7701f, 0f, 104.5813f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(290.9751f, 0f, 88.02885f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.615f, 0f, 88.79352f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(325.2033f, 0f, 88.25986f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.5295f, 0f, 89.24023f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.6768f, 0f, 103.1268f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.4766f, 0f, 103.9006f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.2179f, 0f, 103.2805f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(302.0779f, 0f, 103.7604f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(291.9414f, 0f, 109.4172f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(291.6013f, 0f, 131.1754f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(303.2078f, 0f, 129.8515f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(302.8248f, 0f, 117.1258f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.5263f, 0f, 116.647f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.5162f, 0f, 116.1869f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.3043f, 0f, 115.6441f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.5527f, 0f, 130.4421f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.3277f, 0f, 129.9752f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.1287f, 0f, 129.8491f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(269.7205f, 0f, 46.69055f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(289.7581f, 0f, 45.82553f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(292.522f, 0f, 79.15257f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(302.4849f, 0f, 77.97589f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.0146f, 0f, 77.19353f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.4106f, 0f, 78.33173f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.5006f, 0f, 78.12605f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(335.5583f, 0f, 63.79735f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.607f, 0f, 64.07042f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.8558f, 0f, 63.03982f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(302.0575f, 0f, 63.25717f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(301.5067f, 0f, 50.7742f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(291.9287f, 0f, 58.09966f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.3836f, 0f, 50.41451f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(325.5244f, 0f, 50.34003f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.1642f, 0f, 49.91796f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.0596f, 0f, 35.55597f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(324.8378f, 0f, 37.24081f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(313.9566f, 0f, 36.86293f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(302.5924f, 0f, 35.966f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(291.1385f, 0f, -20.99491f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(337.463f, 0f, -19.98341f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(336.322f, 0f, 5.703799f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(314.6233f, 0f, 5.471754f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(291.4373f, 0f, 5.287608f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(316.0869f, 0f, -19.96726f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(292.6128f, 0f, 28.76913f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(312.7319f, 0f, 27.88435f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(334.8865f, 0f, 27.83924f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(256.3216f, 0f, 25.41761f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(258.6771f, 0f, 12.24492f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(282.0535f, 0f, 19.45084f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(265.5045f, 0f, 20.15305f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(261.9563f, 0f, -16.23082f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(245.892f, 0f, -18.344f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(266.2276f, 0f, -20.95153f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(283.0675f, 0f, -21.06761f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(215.7262f, 0f, 43.4309f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(216.6951f, 0f, 24.70111f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(217.2321f, 0f, 15.51678f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(233.9272f, 0f, 20.7707f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(233.2054f, 0f, 1.989445f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(217.4633f, 0f, -1.502066f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(224.8316f, 0f, -17.12519f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(209.1892f, 0f, -17.53447f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(196.42f, 0f, -16.20626f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.1741f, 0f, -15.39113f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(184.6422f, 0f, 3.079296f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(195.5628f, 0f, 3.325037f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(206.0919f, 0f, 4.740668f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(204.814f, 0f, -13.38856f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(205.8079f, 0f, 20.6386f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(196.3882f, 0f, 19.62914f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(186.0146f, 0f, 20.36477f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.9839f, 0f, 25.16362f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.5764f, 0f, 45.5938f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.0919f, 0f, 92.88574f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(201.3233f, 0f, 92.29105f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(210.6758f, 0f, 91.57107f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(203.9354f, 0f, 75.52847f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(248.1698f, 0f, 75.63219f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(248.9581f, 0f, 96.1467f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(248.6911f, 0f, 108.8801f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(248.9927f, 0f, 123.6975f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(238.176f, 0f, 124.2997f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(237.6409f, 0f, 110.368f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(237.8323f, 0f, 97.6687f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(238.2452f, 0f, 83.76459f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(227.2731f, 0f, 84.54256f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(225.6237f, 0f, 96.60104f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(227.2222f, 0f, 109.9483f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(225.9362f, 0f, 123.4073f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(215.0047f, 0f, 124.7495f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(205.2747f, 0f, 124.7764f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(204.8032f, 0f, 105.0259f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(214.34f, 0f, 104.7689f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.4061f, 0f, 101.4895f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(170.9995f, 0f, 102.3751f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(161.1836f, 0f, 101.223f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(166.6859f, 0f, 95.41547f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(165.8621f, 0f, 76.26211f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(156.483f, 0f, 75.68566f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.2826f, 0f, 76.64617f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.1965f, 0f, 76.90995f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(122.2537f, 0f, 76.51424f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(123.2586f, 0f, 90.13202f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.6898f, 0f, 90.51395f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.1965f, 0f, 90.14644f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(155.7425f, 0f, 91.05353f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(155.7425f, 0f, 103.0533f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.1694f, 0f, 103.803f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.2829f, 0f, 103.9742f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(122.8179f, 0f, 104.949f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(124.1806f, 0f, 118.9617f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(135.0462f, 0f, 119.2009f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.7175f, 0f, 119.362f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(156.7208f, 0f, 119.3727f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(165.8442f, 0f, 118.1872f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(184.7513f, 0f, 139.6407f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(185.1329f, 0f, 149.9482f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(201.2546f, 0f, 150.9124f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(210.0058f, 0f, 150.9036f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(205.2545f, 0f, 157.6009f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(204.9647f, 0f, 176.8693f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(215.1606f, 0f, 176.5713f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(215.4383f, 0f, 168.8105f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(215.7268f, 0f, 155.3898f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(213.5495f, 0f, 141.7627f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(203.652f, 0f, 131.5897f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(224.7f, 0f, 140.6317f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(236.9744f, 0f, 140.289f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(248.4261f, 0f, 140.5326f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(250.3596f, 0f, 154.8559f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(236.6506f, 0f, 154.8793f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(226.3227f, 0f, 154.2897f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(226.9562f, 0f, 168.0395f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(237.4015f, 0f, 167.6012f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(249.6414f, 0f, 167.7346f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(183.6748f, 0f, 158.4645f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(170.6797f, 0f, 160.8093f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(162.0857f, 0f, 160.1247f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(156.9202f, 0f, 156.3934f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(155.561f, 0f, 143.2933f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(166.5939f, 0f, 147.5809f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(166.0895f, 0f, 128.1967f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(156.8266f, 0f, 129.7945f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.8009f, 0f, 129.3015f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.979f, 0f, 129.4422f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(123.1594f, 0f, 130.468f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(123.2231f, 0f, 142.5692f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.1954f, 0f, 143.3471f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.1313f, 0f, 143.0963f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.5974f, 0f, 156.4211f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.7168f, 0f, 155.8555f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(123.9341f, 0f, 155.9314f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(122.5569f, 0f, 177.3761f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(134.9629f, 0f, 174.0114f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(145.183f, 0f, 174.0642f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(164.3868f, 0f, 174.9372f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(86.43555f, 0f, 141.6735f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(76.27461f, 0f, 142.5661f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(83.72416f, 0f, 159.1624f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(34.66504f, 0f, 151.4563f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-13.33964f, 0f, 150.5335f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-16.41086f, 0f, 124.2231f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-15.29378f, 0f, 103.2631f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-13.22328f, 0f, 81.11709f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(-13.07589f, 0f, 59.11686f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(17.98917f, 0f, 60.06714f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(42.96103f, 0f, 57.7157f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(74.93066f, 0f, 61.32368f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(74.54794f, 0f, 79.9663f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(42.93259f, 0f, 82.06083f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(19.27526f, 0f, 81.65559f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(18.21642f, 0f, 101.3063f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(43.10327f, 0f, 102.3409f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(74.48962f, 0f, 101.4246f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(75.99798f, 0f, 124.9392f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(42.40874f, 0f, 123.7709f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(17.58837f, 0f, 123.0599f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(91.49037f, 0f, 30.78918f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(91.35677f, 0f, 24.4954f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(92.06934f, 0f, 19.20908f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(114.9633f, 0f, 18.69109f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(151.1142f, 0f, 16.16636f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(171.263f, 0f, 17.59519f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(168.2873f, 0f, -11.01371f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(149.0036f, 0f, -9.528836f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(115.5623f, 0f, -10.77762f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(94.59713f, 0f, -12.30089f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(96.04467f, 0f, -35.50829f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(114.7517f, 0f, -35.47099f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(148.8297f, 0f, -36.2703f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(168.7456f, 0f, -35.45219f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(166.8497f, 0f, -58.26693f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(148.3924f, 0f, -58.37105f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(115.0858f, 0f, -57.55426f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(95.68756f, 0f, -57.79477f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(94.79133f, 0f, -80.12727f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(114.4977f, 0f, -79.7113f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(149.5465f, 0f, -80.36234f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(168.6454f, 0f, -79.26181f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(165.7775f, 0f, -107.6688f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(149.799f, 0f, -108.095f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(114.5107f, 0f, -106.8997f)));
            PathFindingNodes.Add(new PathFinding.Node(new Vector3(92.51266f, 0f, -107.1725f)));

            #endregion

            #region PathFinding Links
            PathFindingNodes[0].Links.Add(new Link(PathFindingNodes[1]));
            PathFindingNodes[1].Links.Add(new Link(PathFindingNodes[0]));
            PathFindingNodes[0].Links.Add(new Link(PathFindingNodes[2]));
            PathFindingNodes[2].Links.Add(new Link(PathFindingNodes[0]));
            PathFindingNodes[0].Links.Add(new Link(PathFindingNodes[5]));
            PathFindingNodes[5].Links.Add(new Link(PathFindingNodes[0]));
            PathFindingNodes[1].Links.Add(new Link(PathFindingNodes[2]));
            PathFindingNodes[2].Links.Add(new Link(PathFindingNodes[1]));
            PathFindingNodes[2].Links.Add(new Link(PathFindingNodes[3]));
            PathFindingNodes[3].Links.Add(new Link(PathFindingNodes[2]));
            PathFindingNodes[2].Links.Add(new Link(PathFindingNodes[4]));
            PathFindingNodes[4].Links.Add(new Link(PathFindingNodes[2]));
            PathFindingNodes[2].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[2]));
            PathFindingNodes[3].Links.Add(new Link(PathFindingNodes[4]));
            PathFindingNodes[4].Links.Add(new Link(PathFindingNodes[3]));
            PathFindingNodes[4].Links.Add(new Link(PathFindingNodes[5]));
            PathFindingNodes[5].Links.Add(new Link(PathFindingNodes[4]));
            PathFindingNodes[4].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[4]));
            PathFindingNodes[4].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[4]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[6].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[6]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[7]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[8]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[9]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[13]));
            PathFindingNodes[13].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[10]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[11]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[12]));
            PathFindingNodes[13].Links.Add(new Link(PathFindingNodes[14]));
            PathFindingNodes[14].Links.Add(new Link(PathFindingNodes[13]));
            PathFindingNodes[13].Links.Add(new Link(PathFindingNodes[15]));
            PathFindingNodes[15].Links.Add(new Link(PathFindingNodes[13]));
            PathFindingNodes[13].Links.Add(new Link(PathFindingNodes[16]));
            PathFindingNodes[16].Links.Add(new Link(PathFindingNodes[13]));
            PathFindingNodes[13].Links.Add(new Link(PathFindingNodes[17]));
            PathFindingNodes[17].Links.Add(new Link(PathFindingNodes[13]));
            PathFindingNodes[14].Links.Add(new Link(PathFindingNodes[15]));
            PathFindingNodes[15].Links.Add(new Link(PathFindingNodes[14]));
            PathFindingNodes[14].Links.Add(new Link(PathFindingNodes[16]));
            PathFindingNodes[16].Links.Add(new Link(PathFindingNodes[14]));
            PathFindingNodes[14].Links.Add(new Link(PathFindingNodes[17]));
            PathFindingNodes[17].Links.Add(new Link(PathFindingNodes[14]));
            PathFindingNodes[15].Links.Add(new Link(PathFindingNodes[16]));
            PathFindingNodes[16].Links.Add(new Link(PathFindingNodes[15]));
            PathFindingNodes[15].Links.Add(new Link(PathFindingNodes[17]));
            PathFindingNodes[17].Links.Add(new Link(PathFindingNodes[15]));
            PathFindingNodes[15].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[15]));
            PathFindingNodes[16].Links.Add(new Link(PathFindingNodes[17]));
            PathFindingNodes[17].Links.Add(new Link(PathFindingNodes[16]));
            PathFindingNodes[16].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[16]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[19]));
            PathFindingNodes[19].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[22]));
            PathFindingNodes[22].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[23]));
            PathFindingNodes[23].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[18].Links.Add(new Link(PathFindingNodes[26]));
            PathFindingNodes[26].Links.Add(new Link(PathFindingNodes[18]));
            PathFindingNodes[19].Links.Add(new Link(PathFindingNodes[20]));
            PathFindingNodes[20].Links.Add(new Link(PathFindingNodes[19]));
            PathFindingNodes[19].Links.Add(new Link(PathFindingNodes[25]));
            PathFindingNodes[25].Links.Add(new Link(PathFindingNodes[19]));
            PathFindingNodes[19].Links.Add(new Link(PathFindingNodes[26]));
            PathFindingNodes[26].Links.Add(new Link(PathFindingNodes[19]));
            PathFindingNodes[20].Links.Add(new Link(PathFindingNodes[21]));
            PathFindingNodes[21].Links.Add(new Link(PathFindingNodes[20]));
            PathFindingNodes[20].Links.Add(new Link(PathFindingNodes[22]));
            PathFindingNodes[22].Links.Add(new Link(PathFindingNodes[20]));
            PathFindingNodes[20].Links.Add(new Link(PathFindingNodes[25]));
            PathFindingNodes[25].Links.Add(new Link(PathFindingNodes[20]));
            PathFindingNodes[20].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[20]));
            PathFindingNodes[21].Links.Add(new Link(PathFindingNodes[22]));
            PathFindingNodes[22].Links.Add(new Link(PathFindingNodes[21]));
            PathFindingNodes[21].Links.Add(new Link(PathFindingNodes[24]));
            PathFindingNodes[24].Links.Add(new Link(PathFindingNodes[21]));
            PathFindingNodes[21].Links.Add(new Link(PathFindingNodes[26]));
            PathFindingNodes[26].Links.Add(new Link(PathFindingNodes[21]));
            PathFindingNodes[22].Links.Add(new Link(PathFindingNodes[23]));
            PathFindingNodes[23].Links.Add(new Link(PathFindingNodes[22]));
            PathFindingNodes[23].Links.Add(new Link(PathFindingNodes[24]));
            PathFindingNodes[24].Links.Add(new Link(PathFindingNodes[23]));
            PathFindingNodes[23].Links.Add(new Link(PathFindingNodes[25]));
            PathFindingNodes[25].Links.Add(new Link(PathFindingNodes[23]));
            PathFindingNodes[24].Links.Add(new Link(PathFindingNodes[25]));
            PathFindingNodes[25].Links.Add(new Link(PathFindingNodes[24]));
            PathFindingNodes[24].Links.Add(new Link(PathFindingNodes[26]));
            PathFindingNodes[26].Links.Add(new Link(PathFindingNodes[24]));
            PathFindingNodes[25].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[25]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[27].Links.Add(new Link(PathFindingNodes[258]));
            PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[27]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[35]));             
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[135]));
            PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[28]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[258]));
            PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[29].Links.Add(new Link(PathFindingNodes[135]));
            PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[29]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[135]));
            PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[31]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[135]));
            PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[136]));
            PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[152]));
            PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[32]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[135]));
            PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[152]));
            PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[153]));
            PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[30]));
            PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[33]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[34]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[237]));
            PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[35]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[237]));
            PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[238]));
            PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[36]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[37]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[57]));
            PathFindingNodes[57].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[38].Links.Add(new Link(PathFindingNodes[59]));
            PathFindingNodes[59].Links.Add(new Link(PathFindingNodes[38]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[57]));
            PathFindingNodes[57].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[59]));
            PathFindingNodes[59].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[39].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[39]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[40].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[40]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[42]));
            PathFindingNodes[42].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[41].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[41]));
            PathFindingNodes[42].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[42]));
            PathFindingNodes[42].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[42]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[43].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[43]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[44].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[44]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[45].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[45]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[46].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[46]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[54]));
            PathFindingNodes[54].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[47].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[47]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[49]));
            PathFindingNodes[49].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[50]));
            PathFindingNodes[50].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[52]));
            PathFindingNodes[52].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[48].Links.Add(new Link(PathFindingNodes[54]));
            PathFindingNodes[54].Links.Add(new Link(PathFindingNodes[48]));
            PathFindingNodes[49].Links.Add(new Link(PathFindingNodes[50]));
            PathFindingNodes[50].Links.Add(new Link(PathFindingNodes[49]));
            PathFindingNodes[49].Links.Add(new Link(PathFindingNodes[54]));
            PathFindingNodes[54].Links.Add(new Link(PathFindingNodes[49]));
            PathFindingNodes[50].Links.Add(new Link(PathFindingNodes[51]));
            PathFindingNodes[51].Links.Add(new Link(PathFindingNodes[50]));
            PathFindingNodes[50].Links.Add(new Link(PathFindingNodes[52]));
            PathFindingNodes[52].Links.Add(new Link(PathFindingNodes[50]));
            PathFindingNodes[51].Links.Add(new Link(PathFindingNodes[52]));
            PathFindingNodes[52].Links.Add(new Link(PathFindingNodes[51]));
            PathFindingNodes[52].Links.Add(new Link(PathFindingNodes[53]));
            PathFindingNodes[53].Links.Add(new Link(PathFindingNodes[52]));
            PathFindingNodes[53].Links.Add(new Link(PathFindingNodes[54]));
            PathFindingNodes[54].Links.Add(new Link(PathFindingNodes[53]));
            PathFindingNodes[55].Links.Add(new Link(PathFindingNodes[56]));
            PathFindingNodes[56].Links.Add(new Link(PathFindingNodes[55]));
            PathFindingNodes[57].Links.Add(new Link(PathFindingNodes[59]));
            PathFindingNodes[59].Links.Add(new Link(PathFindingNodes[57]));
            PathFindingNodes[57].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[57]));
            PathFindingNodes[57].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[57]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[59]));
            PathFindingNodes[59].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[154]));
            PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[174]));
            PathFindingNodes[174].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[196]));
            PathFindingNodes[196].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[197]));
            PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[198]));
            PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[216]));
            PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[58].Links.Add(new Link(PathFindingNodes[217]));
            PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[58]));
            PathFindingNodes[59].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[59]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[61]));
            PathFindingNodes[61].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[79]));
            PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[60].Links.Add(new Link(PathFindingNodes[98]));
            PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[60]));
            PathFindingNodes[61].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[61]));
            PathFindingNodes[61].Links.Add(new Link(PathFindingNodes[79]));
            PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[61]));
            PathFindingNodes[61].Links.Add(new Link(PathFindingNodes[98]));
            PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[61]));
            PathFindingNodes[61].Links.Add(new Link(PathFindingNodes[127]));
            PathFindingNodes[127].Links.Add(new Link(PathFindingNodes[61]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[64]));
            PathFindingNodes[64].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[73]));
            PathFindingNodes[73].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[62].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[62]));
            PathFindingNodes[63].Links.Add(new Link(PathFindingNodes[64]));
            PathFindingNodes[64].Links.Add(new Link(PathFindingNodes[63]));
            PathFindingNodes[63].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[63]));
            PathFindingNodes[63].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[63]));
            PathFindingNodes[64].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[64]));
            PathFindingNodes[64].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[64]));
            PathFindingNodes[64].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[64]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[70]));
            PathFindingNodes[70].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[71]));
            PathFindingNodes[71].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[65].Links.Add(new Link(PathFindingNodes[76]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[65]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[73]));
            PathFindingNodes[73].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[67].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[67]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[70]));
            PathFindingNodes[70].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[73]));
            PathFindingNodes[73].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[68].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[68]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[70]));
            PathFindingNodes[70].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[71]));
            PathFindingNodes[71].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[69]));
            PathFindingNodes[69].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[66].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[66]));
            PathFindingNodes[70].Links.Add(new Link(PathFindingNodes[71]));
            PathFindingNodes[71].Links.Add(new Link(PathFindingNodes[70]));
            PathFindingNodes[70].Links.Add(new Link(PathFindingNodes[76]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[70]));
            PathFindingNodes[71].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[71]));
            PathFindingNodes[71].Links.Add(new Link(PathFindingNodes[76]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[71]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[73]));
            PathFindingNodes[73].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[72].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[72]));
            PathFindingNodes[73].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[73]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[76]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[74].Links.Add(new Link(PathFindingNodes[78]));
            PathFindingNodes[78].Links.Add(new Link(PathFindingNodes[74]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[76]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[75].Links.Add(new Link(PathFindingNodes[78]));
            PathFindingNodes[78].Links.Add(new Link(PathFindingNodes[75]));
            PathFindingNodes[76].Links.Add(new Link(PathFindingNodes[77]));
            PathFindingNodes[77].Links.Add(new Link(PathFindingNodes[76]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[79].Links.Add(new Link(PathFindingNodes[127]));
        PathFindingNodes[127].Links.Add(new Link(PathFindingNodes[79]));
        PathFindingNodes[80].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[80]));
        PathFindingNodes[80].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[80]));
        PathFindingNodes[80].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[80]));
        PathFindingNodes[80].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[80]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[81].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[81]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[82].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[82]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[83].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[83]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[84].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[84]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[85].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[85]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[86].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[86]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[88]));
        PathFindingNodes[88].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[87].Links.Add(new Link(PathFindingNodes[91]));
        PathFindingNodes[91].Links.Add(new Link(PathFindingNodes[87]));
        PathFindingNodes[88].Links.Add(new Link(PathFindingNodes[89]));
        PathFindingNodes[89].Links.Add(new Link(PathFindingNodes[88]));
        PathFindingNodes[89].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[89]));
        PathFindingNodes[89].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[89]));
        PathFindingNodes[89].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[89]));
        PathFindingNodes[89].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[89]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[91]));
        PathFindingNodes[91].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[90].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[90]));
        PathFindingNodes[91].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[91]));
        PathFindingNodes[91].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[91]));
        PathFindingNodes[91].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[91]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[92].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[92]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[93].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[93]));
        PathFindingNodes[94].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[94]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[96]));
        PathFindingNodes[96].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[95].Links.Add(new Link(PathFindingNodes[97]));
        PathFindingNodes[97].Links.Add(new Link(PathFindingNodes[95]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[99]));
        PathFindingNodes[99].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[127]));
        PathFindingNodes[127].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[135]));
        PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[152]));
        PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[30]));
        PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[98].Links.Add(new Link(PathFindingNodes[31]));
        PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[98]));
        PathFindingNodes[99].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[99]));
        PathFindingNodes[99].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[99]));
        PathFindingNodes[99].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[99]));
        PathFindingNodes[99].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[99]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[100].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[100]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[101].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[101]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[102].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[102]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[103].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[103]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[104].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[104]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[105].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[105]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[106].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[106]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[107].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[107]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[108].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[108]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[109].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[109]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[110].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[110]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[111].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[111]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[112].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[112]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[113].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[113]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[114].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[114]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[115].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[115]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[116].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[116]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[117].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[117]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[118].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[118]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[120].Links.Add(new Link(PathFindingNodes[119]));
        PathFindingNodes[119].Links.Add(new Link(PathFindingNodes[120]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[121].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[121]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[122].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[122]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[123].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[123]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[124].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[124]));
        PathFindingNodes[125].Links.Add(new Link(PathFindingNodes[126]));
        PathFindingNodes[126].Links.Add(new Link(PathFindingNodes[125]));
        PathFindingNodes[127].Links.Add(new Link(PathFindingNodes[128]));
        PathFindingNodes[128].Links.Add(new Link(PathFindingNodes[127]));
        PathFindingNodes[128].Links.Add(new Link(PathFindingNodes[130]));
        PathFindingNodes[130].Links.Add(new Link(PathFindingNodes[128]));
        PathFindingNodes[128].Links.Add(new Link(PathFindingNodes[131]));
        PathFindingNodes[131].Links.Add(new Link(PathFindingNodes[128]));
        PathFindingNodes[128].Links.Add(new Link(PathFindingNodes[133]));
        PathFindingNodes[133].Links.Add(new Link(PathFindingNodes[128]));
        PathFindingNodes[129].Links.Add(new Link(PathFindingNodes[130]));
        PathFindingNodes[130].Links.Add(new Link(PathFindingNodes[129]));
        PathFindingNodes[130].Links.Add(new Link(PathFindingNodes[131]));
        PathFindingNodes[131].Links.Add(new Link(PathFindingNodes[130]));
        PathFindingNodes[130].Links.Add(new Link(PathFindingNodes[133]));
        PathFindingNodes[133].Links.Add(new Link(PathFindingNodes[130]));
        PathFindingNodes[131].Links.Add(new Link(PathFindingNodes[132]));
        PathFindingNodes[132].Links.Add(new Link(PathFindingNodes[131]));
        PathFindingNodes[131].Links.Add(new Link(PathFindingNodes[133]));
        PathFindingNodes[133].Links.Add(new Link(PathFindingNodes[131]));
        PathFindingNodes[132].Links.Add(new Link(PathFindingNodes[133]));
        PathFindingNodes[133].Links.Add(new Link(PathFindingNodes[132]));
        PathFindingNodes[132].Links.Add(new Link(PathFindingNodes[134]));
        PathFindingNodes[134].Links.Add(new Link(PathFindingNodes[132]));
        PathFindingNodes[133].Links.Add(new Link(PathFindingNodes[134]));
        PathFindingNodes[134].Links.Add(new Link(PathFindingNodes[133]));
        PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[135]));
        PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[135]));
        PathFindingNodes[135].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[135]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[139]));
        PathFindingNodes[139].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[141]));
        PathFindingNodes[141].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[138]));
        PathFindingNodes[138].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[139]));
        PathFindingNodes[139].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[141]));
        PathFindingNodes[141].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[137].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[137]));
        PathFindingNodes[136].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[136]));
        PathFindingNodes[138].Links.Add(new Link(PathFindingNodes[139]));
        PathFindingNodes[139].Links.Add(new Link(PathFindingNodes[138]));
        PathFindingNodes[138].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[138]));
        PathFindingNodes[138].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[138]));
        PathFindingNodes[139].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[139]));
        PathFindingNodes[139].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[139]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[141]));
        PathFindingNodes[141].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[140].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[140]));
        PathFindingNodes[141].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[141]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[143]));
        PathFindingNodes[143].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[142].Links.Add(new Link(PathFindingNodes[148]));
        PathFindingNodes[148].Links.Add(new Link(PathFindingNodes[142]));
        PathFindingNodes[143].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[143]));
        PathFindingNodes[143].Links.Add(new Link(PathFindingNodes[146]));
        PathFindingNodes[146].Links.Add(new Link(PathFindingNodes[143]));
        PathFindingNodes[143].Links.Add(new Link(PathFindingNodes[148]));
        PathFindingNodes[148].Links.Add(new Link(PathFindingNodes[143]));
        PathFindingNodes[143].Links.Add(new Link(PathFindingNodes[150]));
        PathFindingNodes[150].Links.Add(new Link(PathFindingNodes[143]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[152]));
        PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[146]));
        PathFindingNodes[146].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[147]));
        PathFindingNodes[147].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[152]));
        PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[145].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[145]));
        PathFindingNodes[146].Links.Add(new Link(PathFindingNodes[147]));
        PathFindingNodes[147].Links.Add(new Link(PathFindingNodes[146]));
        PathFindingNodes[146].Links.Add(new Link(PathFindingNodes[150]));
        PathFindingNodes[150].Links.Add(new Link(PathFindingNodes[146]));
        PathFindingNodes[147].Links.Add(new Link(PathFindingNodes[148]));
        PathFindingNodes[148].Links.Add(new Link(PathFindingNodes[147]));
        PathFindingNodes[147].Links.Add(new Link(PathFindingNodes[149]));
        PathFindingNodes[149].Links.Add(new Link(PathFindingNodes[147]));
        PathFindingNodes[148].Links.Add(new Link(PathFindingNodes[144]));
        PathFindingNodes[144].Links.Add(new Link(PathFindingNodes[148]));
        PathFindingNodes[149].Links.Add(new Link(PathFindingNodes[150]));
        PathFindingNodes[150].Links.Add(new Link(PathFindingNodes[149]));
        PathFindingNodes[149].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[149]));
        PathFindingNodes[149].Links.Add(new Link(PathFindingNodes[148]));
        PathFindingNodes[148].Links.Add(new Link(PathFindingNodes[149]));
        PathFindingNodes[150].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[150]));
        PathFindingNodes[150].Links.Add(new Link(PathFindingNodes[152]));
        PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[150]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[152]));
        PathFindingNodes[152].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[151].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[151]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[155]));
        PathFindingNodes[155].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[174]));
        PathFindingNodes[174].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[196]));
        PathFindingNodes[196].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[155]));
        PathFindingNodes[155].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[156]));
        PathFindingNodes[156].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[174]));
        PathFindingNodes[174].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[196]));
        PathFindingNodes[196].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[216]));
        PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[154].Links.Add(new Link(PathFindingNodes[217]));
        PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[154]));
        PathFindingNodes[155].Links.Add(new Link(PathFindingNodes[156]));
        PathFindingNodes[156].Links.Add(new Link(PathFindingNodes[155]));
        PathFindingNodes[155].Links.Add(new Link(PathFindingNodes[172]));
        PathFindingNodes[172].Links.Add(new Link(PathFindingNodes[155]));
        PathFindingNodes[173].Links.Add(new Link(PathFindingNodes[155]));
        PathFindingNodes[156].Links.Add(new Link(PathFindingNodes[157]));
        PathFindingNodes[157].Links.Add(new Link(PathFindingNodes[156]));
        PathFindingNodes[156].Links.Add(new Link(PathFindingNodes[172]));
        PathFindingNodes[172].Links.Add(new Link(PathFindingNodes[156]));
        PathFindingNodes[156].Links.Add(new Link(PathFindingNodes[173]));
        PathFindingNodes[173].Links.Add(new Link(PathFindingNodes[156]));
        PathFindingNodes[157].Links.Add(new Link(PathFindingNodes[158]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[157]));
        PathFindingNodes[157].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[165].Links.Add(new Link(PathFindingNodes[157]));
        PathFindingNodes[157].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[157]));
        PathFindingNodes[157].Links.Add(new Link(PathFindingNodes[173]));
        PathFindingNodes[173].Links.Add(new Link(PathFindingNodes[157]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[159]));
        PathFindingNodes[159].Links.Add(new Link(PathFindingNodes[158]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[160]));
        PathFindingNodes[160].Links.Add(new Link(PathFindingNodes[158]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[158]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[158].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[158]));
        PathFindingNodes[159].Links.Add(new Link(PathFindingNodes[160]));
        PathFindingNodes[160].Links.Add(new Link(PathFindingNodes[159]));
        PathFindingNodes[159].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[159]));
        PathFindingNodes[159].Links.Add(new Link(PathFindingNodes[164]));
        PathFindingNodes[164].Links.Add(new Link(PathFindingNodes[159]));
        PathFindingNodes[159].Links.Add(new Link(PathFindingNodes[167]));
        PathFindingNodes[167].Links.Add(new Link(PathFindingNodes[159]));
        PathFindingNodes[160].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[160]));
        PathFindingNodes[160].Links.Add(new Link(PathFindingNodes[163]));
        PathFindingNodes[163].Links.Add(new Link(PathFindingNodes[160]));
        PathFindingNodes[160].Links.Add(new Link(PathFindingNodes[168]));
        PathFindingNodes[168].Links.Add(new Link(PathFindingNodes[160]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[170]));
        PathFindingNodes[170].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[161].Links.Add(new Link(PathFindingNodes[171]));
        PathFindingNodes[171].Links.Add(new Link(PathFindingNodes[161]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[163]));
        PathFindingNodes[163].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[164]));
        PathFindingNodes[164].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[165].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[170]));
        PathFindingNodes[170].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[162].Links.Add(new Link(PathFindingNodes[171]));
        PathFindingNodes[171].Links.Add(new Link(PathFindingNodes[162]));
        PathFindingNodes[163].Links.Add(new Link(PathFindingNodes[164]));
        PathFindingNodes[164].Links.Add(new Link(PathFindingNodes[163]));
        PathFindingNodes[163].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[165].Links.Add(new Link(PathFindingNodes[163]));
        PathFindingNodes[163].Links.Add(new Link(PathFindingNodes[168]));
        PathFindingNodes[168].Links.Add(new Link(PathFindingNodes[163]));
        PathFindingNodes[164].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[165].Links.Add(new Link(PathFindingNodes[164]));
        PathFindingNodes[164].Links.Add(new Link(PathFindingNodes[167]));
        PathFindingNodes[167].Links.Add(new Link(PathFindingNodes[164]));
        PathFindingNodes[165].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[165]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[167]));
        PathFindingNodes[167].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[168]));
        PathFindingNodes[168].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[166].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[166]));
        PathFindingNodes[167].Links.Add(new Link(PathFindingNodes[168]));
        PathFindingNodes[168].Links.Add(new Link(PathFindingNodes[167]));
        PathFindingNodes[167].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[167]));
        PathFindingNodes[168].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[168]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[170]));
        PathFindingNodes[170].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[169].Links.Add(new Link(PathFindingNodes[171]));
        PathFindingNodes[171].Links.Add(new Link(PathFindingNodes[169]));
        PathFindingNodes[170].Links.Add(new Link(PathFindingNodes[171]));
        PathFindingNodes[171].Links.Add(new Link(PathFindingNodes[170]));
        PathFindingNodes[170].Links.Add(new Link(PathFindingNodes[173]));
        PathFindingNodes[173].Links.Add(new Link(PathFindingNodes[170]));
        PathFindingNodes[171].Links.Add(new Link(PathFindingNodes[172]));
        PathFindingNodes[172].Links.Add(new Link(PathFindingNodes[171]));
        PathFindingNodes[172].Links.Add(new Link(PathFindingNodes[173]));
        PathFindingNodes[173].Links.Add(new Link(PathFindingNodes[172]));
        PathFindingNodes[174].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[174]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[175].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[175]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[177]));
        PathFindingNodes[177].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[176].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[176]));
        PathFindingNodes[177].Links.Add(new Link(PathFindingNodes[178]));
        PathFindingNodes[178].Links.Add(new Link(PathFindingNodes[177]));
        PathFindingNodes[177].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[177]));
        PathFindingNodes[177].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[177]));
        PathFindingNodes[178].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[178]));
        PathFindingNodes[178].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[178]));
        PathFindingNodes[178].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[178]));
        PathFindingNodes[178].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[178]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[179].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[179]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[180].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[180]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[181].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[181]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[182].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[182]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[183].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[183]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[184].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[184]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[185].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[185]));
        PathFindingNodes[186].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[186]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[187].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[187]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[188].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[188]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[189].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[189]));
        PathFindingNodes[190].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[190]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[191].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[191]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[192].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[192]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[193].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[193]));
        PathFindingNodes[194].Links.Add(new Link(PathFindingNodes[195]));
        PathFindingNodes[195].Links.Add(new Link(PathFindingNodes[194]));
        PathFindingNodes[196].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[196]));
        PathFindingNodes[196].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[196]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[216]));
        PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[197].Links.Add(new Link(PathFindingNodes[217]));
        PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[197]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[200]));
        PathFindingNodes[200].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[200]));
        PathFindingNodes[200].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[201]));
        PathFindingNodes[201].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[199].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[199]));
        PathFindingNodes[200].Links.Add(new Link(PathFindingNodes[201]));
        PathFindingNodes[201].Links.Add(new Link(PathFindingNodes[200]));
        PathFindingNodes[200].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[200]));
        PathFindingNodes[200].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[200]));
        PathFindingNodes[201].Links.Add(new Link(PathFindingNodes[202]));
        PathFindingNodes[202].Links.Add(new Link(PathFindingNodes[201]));
        PathFindingNodes[202].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[202]));
        PathFindingNodes[202].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[202]));
        PathFindingNodes[202].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[202]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[213]));
        PathFindingNodes[213].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[214]));
        PathFindingNodes[214].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[203].Links.Add(new Link(PathFindingNodes[215]));
        PathFindingNodes[215].Links.Add(new Link(PathFindingNodes[203]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[210]));
        PathFindingNodes[210].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[211]));
        PathFindingNodes[211].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[204].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[204]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[205].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[205]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[206].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[206]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[207].Links.Add(new Link(PathFindingNodes[213]));
        PathFindingNodes[213].Links.Add(new Link(PathFindingNodes[207]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[211]));
        PathFindingNodes[211].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[208].Links.Add(new Link(PathFindingNodes[214]));
        PathFindingNodes[214].Links.Add(new Link(PathFindingNodes[208]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[210]));
        PathFindingNodes[210].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[209].Links.Add(new Link(PathFindingNodes[215]));
        PathFindingNodes[215].Links.Add(new Link(PathFindingNodes[209]));
        PathFindingNodes[210].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[210]));
        PathFindingNodes[210].Links.Add(new Link(PathFindingNodes[215]));
        PathFindingNodes[215].Links.Add(new Link(PathFindingNodes[210]));
        PathFindingNodes[211].Links.Add(new Link(PathFindingNodes[214]));
        PathFindingNodes[214].Links.Add(new Link(PathFindingNodes[211]));
        PathFindingNodes[210].Links.Add(new Link(PathFindingNodes[211]));
        PathFindingNodes[211].Links.Add(new Link(PathFindingNodes[210]));
        PathFindingNodes[211].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[211]));
        PathFindingNodes[212].Links.Add(new Link(PathFindingNodes[213]));
        PathFindingNodes[213].Links.Add(new Link(PathFindingNodes[212]));
        PathFindingNodes[213].Links.Add(new Link(PathFindingNodes[214]));
        PathFindingNodes[214].Links.Add(new Link(PathFindingNodes[213]));
        PathFindingNodes[213].Links.Add(new Link(PathFindingNodes[215]));
        PathFindingNodes[215].Links.Add(new Link(PathFindingNodes[213]));
        PathFindingNodes[214].Links.Add(new Link(PathFindingNodes[215]));
        PathFindingNodes[215].Links.Add(new Link(PathFindingNodes[214]));
        PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[217]));
        PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[216]));
        PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[198]));
        PathFindingNodes[198].Links.Add(new Link(PathFindingNodes[216]));
        PathFindingNodes[216].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[216]));
        PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[217]));
        PathFindingNodes[217].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[217]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[221]));
        PathFindingNodes[221].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[218].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[218]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[221]));
        PathFindingNodes[221].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[219].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[219]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[221].Links.Add(new Link(PathFindingNodes[222]));
        PathFindingNodes[222].Links.Add(new Link(PathFindingNodes[221]));
        PathFindingNodes[222].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[222]));
        PathFindingNodes[222].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[222]));
        PathFindingNodes[222].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[222]));
        PathFindingNodes[222].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[222]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[223].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[223]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[230]));
        PathFindingNodes[230].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[224].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[224]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[231]));
        PathFindingNodes[231].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[225].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[225]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[226].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[226]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[227].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[227]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[231]));
        PathFindingNodes[231].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[228].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[228]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[230]));
        PathFindingNodes[230].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[229].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[229]));
        PathFindingNodes[230].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[230]));
        PathFindingNodes[230].Links.Add(new Link(PathFindingNodes[231]));
        PathFindingNodes[231].Links.Add(new Link(PathFindingNodes[230]));
        PathFindingNodes[230].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[230]));
        PathFindingNodes[231].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[231]));
        PathFindingNodes[231].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[231]));
        PathFindingNodes[232].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[232]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[233].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[233]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[234].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[234]));
        PathFindingNodes[235].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[235]));
        PathFindingNodes[236].Links.Add(new Link(PathFindingNodes[220]));
        PathFindingNodes[220].Links.Add(new Link(PathFindingNodes[236]));
        PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[237]));
        PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[237]));
        PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[237]));
        PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[237]));
        PathFindingNodes[237].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[237]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[239]));
        PathFindingNodes[239].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[238].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[238]));
        PathFindingNodes[239].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[239]));
        PathFindingNodes[239].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[239]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[240].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[240]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[241].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[241]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[242].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[242]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[243].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[243]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[244].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[244]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[245].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[245]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[246].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[246]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[247].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[247]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[249].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[249]));
        PathFindingNodes[248].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[248]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[250].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[250]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[251].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[251]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[252].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[252]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[253].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[253]));
        PathFindingNodes[254].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[254]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[255].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[255]));
        PathFindingNodes[256].Links.Add(new Link(PathFindingNodes[257]));
        PathFindingNodes[257].Links.Add(new Link(PathFindingNodes[256]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[7]));
        PathFindingNodes[7].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[8]));
        PathFindingNodes[8].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[9]));
        PathFindingNodes[9].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[10]));
        PathFindingNodes[10].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[11]));
        PathFindingNodes[11].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[12]));
        PathFindingNodes[12].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[28]));
        PathFindingNodes[28].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[30]));
        PathFindingNodes[30].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[31]));
        PathFindingNodes[31].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[32]));
        PathFindingNodes[32].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[33]));
        PathFindingNodes[33].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[34]));
        PathFindingNodes[34].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[35]));
        PathFindingNodes[35].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[36]));
        PathFindingNodes[36].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[37]));
        PathFindingNodes[37].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[258].Links.Add(new Link(PathFindingNodes[153]));
        PathFindingNodes[153].Links.Add(new Link(PathFindingNodes[258]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[259].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[259]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[260].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[260]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[261].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[261]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[262].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[262]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[263].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[263]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[264].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[264]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[265].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[265]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[266].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[266]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[267].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[267]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[268].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[268]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[269].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[269]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[270].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[270]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[271].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[271]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[272].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[272]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[273].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[273]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[274].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[274]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[275].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[275]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[276].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[276]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[277].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[277]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[278].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[278]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[279].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[279]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[280].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[280]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[282]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[281].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[281]));
        PathFindingNodes[282].Links.Add(new Link(PathFindingNodes[283]));
        PathFindingNodes[283].Links.Add(new Link(PathFindingNodes[282]));

            #endregion
            RecalculateNodes();
           
            #endregion


            //CollisionBoxes.Add(new Box(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), new Vector3(0), new Vector3(10, 20, 10)));
            LevelQuadTree = new QuadTree(new Vector2(156, 65), 185, 4);
            
            foreach (Box box in CollisionBoxes)
            {
                LevelQuadTree.Insert(box);
            }

            foreach (PathFinding.Node node in PathFindingNodes)
            {
                LevelQuadTree.Insert(node);
            }

            #region Particles
            globalEffect = new BasicEffect(GraphicsDevice);
            globalEffect.VertexColorEnabled = true;
            globalEffect.View = Camera.ActiveCamera.View;
            globalEffect.World = world;
            globalEffect.Projection = Camera.ActiveCamera.Projection;

            //Effects for Fire Core and Flames
            BasicEffect FireEffect = new BasicEffect(GraphicsDevice);
            FireEffect = new BasicEffect(GraphicsDevice);
            FireEffect.View = Camera.ActiveCamera.View;
            FireEffect.TextureEnabled = true;
            FireEffect.Projection = Camera.ActiveCamera.Projection;
            FireEffect.World = world;
            FireEffect.Texture = fire;
            FireEffect.Alpha = 0.6f;

            BasicEffect ChemicalEffect = new BasicEffect(GraphicsDevice);
            ChemicalEffect = new BasicEffect(GraphicsDevice);
            ChemicalEffect.View = Camera.ActiveCamera.View;
            ChemicalEffect.TextureEnabled = true;
            ChemicalEffect.Projection = Camera.ActiveCamera.Projection;
            ChemicalEffect.World = world;
            ChemicalEffect.Texture = smoke;
            ChemicalEffect.Alpha = 1.0f;


            ChemicalsEmitter = new ParticleEmitter(new Vector3(320, 0, 202), new Vector3(0), 0, 0, 1);
            ChemicalsEmitter.particleGroups.Add(new ParticleGroup("Chemicals", BlendState.Additive, DepthStencilState.DepthRead, ChemicalEffect));
            ChemicalsEmitter.particleGroups[0].controller.Alpha = 0.8f;
            ChemicalsEmitter.particleGroups[0].controller.MaxParticles = 800;
            ChemicalsEmitter.particleGroups[0].controller.ParticlePerEmission = 10;
            ChemicalsEmitter.particleGroups[0].controller.LifeSpan = 1000;
            ChemicalsEmitter.particleGroups[0].controller.Size = 6f;
            ChemicalsEmitter.particleGroups[0].controller.Velocity = new Vector3(0.04f, 0.36f, 0.04f);
            ChemicalsEmitter.particleGroups[0].controller.directionRange = new Vector3(10f, 0, 10f);
            ChemicalsEmitter.particleGroups[0].controller.directionOffset = new Vector3(2.5f, 0, 2.5f);
            ChemicalsEmitter.particleGroups[0].controller.RandomizeDirection = true;
            ChemicalsEmitter.particleGroups[0].controller.RotationVelocity = 1.0f / 60.0f;
            ChemicalsEmitter.particleGroups[0].controller.RandomizeRotation = true;
            

            FireEmitter = new ParticleEmitter(new Vector3(92,0,35), new Vector3(0), 0, 0, 1);
            FireEmitter.particleGroups.Add(new ParticleGroup("FireFlames", BlendState.Additive, DepthStencilState.DepthRead, FireEffect));
            FireEmitter.particleGroups[0].controller.MaxParticles = 50;
            FireEmitter.particleGroups[0].controller.ParticlePerEmission = 1;
            FireEmitter.particleGroups[0].controller.LifeSpan = 1000;
            FireEmitter.particleGroups[0].controller.Size = 12f;
            FireEmitter.particleGroups[0].controller.Velocity = new Vector3(0.04f, 0.36f, 0.04f);
            FireEmitter.particleGroups[0].controller.directionRange = new Vector3(6f, 1, 2f);
            FireEmitter.particleGroups[0].controller.directionOffset = new Vector3(-3f, 0, -3f);
            FireEmitter.particleGroups[0].controller.RandomizeDirection = true;
            FireEmitter.particleGroups[0].controller.RotationVelocity = 1.0f / 60.0f;
            FireEmitter.particleGroups[0].controller.RandomizeRotation = true;

            FireEmitter2 = new ParticleEmitter(new Vector3(284, 0, 53), new Vector3(0), 0, 0, 1);
            FireEmitter2.particleGroups.Add(new ParticleGroup("FireFlames", BlendState.Additive, DepthStencilState.DepthRead, FireEffect));
            FireEmitter2.particleGroups[0].controller.MaxParticles = 50;
            FireEmitter2.particleGroups[0].controller.ParticlePerEmission = 1;
            FireEmitter2.particleGroups[0].controller.LifeSpan = 1000;
            FireEmitter2.particleGroups[0].controller.Size = 12f;
            FireEmitter2.particleGroups[0].controller.Velocity = new Vector3(0.04f, 0.36f, 0.04f);
            FireEmitter2.particleGroups[0].controller.directionRange = new Vector3(6f, 1, 2f);
            FireEmitter2.particleGroups[0].controller.directionOffset = new Vector3(-3f, 0, -3f);
            FireEmitter2.particleGroups[0].controller.RandomizeDirection = true;
            FireEmitter2.particleGroups[0].controller.RotationVelocity = 1.0f / 60.0f;
            FireEmitter2.particleGroups[0].controller.RandomizeRotation = true;

            FireEmitter3 = new ParticleEmitter(new Vector3(88, 0, 200), new Vector3(0), 0, 0, 1);
            FireEmitter3.particleGroups.Add(new ParticleGroup("FireFlames", BlendState.Additive, DepthStencilState.DepthRead, FireEffect));
            FireEmitter3.particleGroups[0].controller.MaxParticles = 50;
            FireEmitter3.particleGroups[0].controller.ParticlePerEmission = 1;
            FireEmitter3.particleGroups[0].controller.LifeSpan = 1000;
            FireEmitter3.particleGroups[0].controller.Size = 12f;
            FireEmitter3.particleGroups[0].controller.Velocity = new Vector3(0.04f, 0.36f, 0.04f);
            FireEmitter3.particleGroups[0].controller.directionRange = new Vector3(6f, 1, 2f);
            FireEmitter3.particleGroups[0].controller.directionOffset = new Vector3(-3f, 0, -3f);
            FireEmitter3.particleGroups[0].controller.RandomizeDirection = true;
            FireEmitter3.particleGroups[0].controller.RotationVelocity = 1.0f / 60.0f;
            FireEmitter3.particleGroups[0].controller.RandomizeRotation = true;


            FireEmitter4 = new ParticleEmitter(new Vector3(332, 0, -6), new Vector3(0), 0, 0, 1);
            FireEmitter4.particleGroups.Add(new ParticleGroup("FireFlames", BlendState.Additive, DepthStencilState.DepthRead, FireEffect));
            FireEmitter4.particleGroups[0].controller.MaxParticles = 50;
            FireEmitter4.particleGroups[0].controller.ParticlePerEmission = 1;
            FireEmitter4.particleGroups[0].controller.LifeSpan = 1000;
            FireEmitter4.particleGroups[0].controller.Size = 12f;
            FireEmitter4.particleGroups[0].controller.Velocity = new Vector3(0.04f, 0.36f, 0.04f);
            FireEmitter4.particleGroups[0].controller.directionRange = new Vector3(6f, 1, 2f);
            FireEmitter4.particleGroups[0].controller.directionOffset = new Vector3(-3f, 0, -3f);
            FireEmitter4.particleGroups[0].controller.RandomizeDirection = true;
            FireEmitter4.particleGroups[0].controller.RotationVelocity = 1.0f / 60.0f;
            FireEmitter4.particleGroups[0].controller.RandomizeRotation = true;
            FireEmitter4.particleGroups[0].controller.Alpha = 0.8f;
            FireEmitter4.particleGroups[0].controller.MaxParticles = 100;
 
 
            FireEmitter.Start();
            FireEmitter2.Start();
            FireEmitter3.Start();
            FireEmitter4.Start();
            ChemicalsEmitter.Start();

            #endregion

            base.LoadContent();
        }

        private void RecalculateNodes()
        {
            LinksCount = 0;
            //Set weight between nodes and links for Graph
            for (int i = 0; i < PathFindingNodes.Count; i++)
            {
                LinksCount += PathFindingNodes[i].Links.Count;
                PathFindingNodes[i].ID = i;

                for (int x = 0; x < PathFindingNodes[i].Links.Count; x++)
                {
                    PathFindingNodes[i].Links[x].weight = (PathFindingNodes[i].Links[x].node.position - PathFindingNodes[i].position).Length();
                }
            }

            //Determine set of lines to draw for Graph Links
            vpc = new VertexPositionColor[LinksCount * 2];

            int vIndex = 0;
            for (int i = 0; i < PathFindingNodes.Count; i++)
            {
                for (int j = 0; j < PathFindingNodes[i].Links.Count; j++)
                {
                    vpc[vIndex] = new VertexPositionColor(PathFindingNodes[i].position, Color.Red);
                    vpc[vIndex + 1] = new VertexPositionColor((PathFindingNodes[i].Links[j].node.position), Color.Red);
                    vIndex += 2;
                }
            }
        }

        // get closest feasible pathfinding node to the given position
        public PathFinding.Node GetPathfindingNode(Vector3 position, Vector3 destination)
        {
            // perform super secret second purpose
            if (destination != Vector3.Up)
            {
                // if path between positions is clear, return origin, else return up vector
                if (CheckObstructions(position, destination))
                    return new PathFinding.Node(Vector3.Up);
                else
                    return new PathFinding.Node(Vector3.Zero);
            }
            List<PathFinding.Node> possibleMatches = new List<PathFinding.Node>();
            PathFinding.Node reachableNode = null;
            float distanceToNode = 100;

            LevelQuadTree.RetrieveNearbyObjects(position, ref possibleMatches, 2, null, 2);
            foreach (PathFinding.Node node in possibleMatches)
            {
                float separatingDistance = (node.position - position).Length();
                bool clearPath = true;

                Ray ray = new Ray(position, Vector3.Normalize(node.position - position));
                Sphere sphere = new Sphere(position, Vector3.Zero, 1);
                List<Primitive> primitives = new List<Primitive>();
                LevelQuadTree.RetrieveNearbyObjects(sphere, ref primitives);

                foreach (Box box in primitives)
                {
                    BoundingBox bbox = new BoundingBox(
                        new Vector3(box.Position.X - box.Size.X / 2, box.Position.Y - box.Size.Y / 2, box.Position.Z - box.Size.Z / 2),
                        new Vector3(box.Position.X + box.Size.X / 2, box.Position.Y + box.Size.Y / 2, box.Position.Z + box.Size.Z / 2)
                    );
                    if (ray.Intersects(bbox) != null && ray.Intersects(bbox) < separatingDistance)
                    {
                        clearPath = false;
                        break;
                    }
                }
                if (clearPath && distanceToNode > separatingDistance)
                {
                    distanceToNode = separatingDistance;
                    reachableNode = node;
                }
            }
            return reachableNode;
        }

        // returns if there are any static obstructions between the two positions
        private bool CheckObstructions(Vector3 position, Vector3 destination)
        {
            Ray ray = new Ray(position, Vector3.Normalize(destination - position));
            Sphere sphere = new Sphere(position, Vector3.Zero, 1);
            List<Primitive> primitives = new List<Primitive>();
            LevelQuadTree.RetrieveNearbyObjects(sphere, ref primitives);

            foreach (Box box in primitives)
            {
                BoundingBox bbox = new BoundingBox(
                    new Vector3(box.Position.X - box.Size.X / 2, box.Position.Y - box.Size.Y / 2, box.Position.Z - box.Size.Z / 2),
                    new Vector3(box.Position.X + box.Size.X / 2, box.Position.Y + box.Size.Y / 2, box.Position.Z + box.Size.Z / 2)
                );
                if (ray.Intersects(bbox) != null)
                    return true;
            }
            return false;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.Start)
            {
                if (keyboard.IsKeyDown(Keys.Down) && ButtonTimer <= 0)
                {
                    StartGameOption = (StartGameOption + 1) % 3;
                    ButtonTimer = 10;
                }

                if (keyboard.IsKeyDown(Keys.Up) && ButtonTimer <= 0)
                {
                    StartGameOption = (StartGameOption - 1);

                    if (StartGameOption < 0)
                    {
                        StartGameOption = 2;
                    }
                    ButtonTimer = 10;
                }

                if ((keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space)) && ButtonTimer <= 0)
                {
                    switch (StartGameOption)
                    {
                        case 0:
                            {
                                LoadContent();
                                GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.InGame;
                                break;
                            }
                        case 1:
                            {
                                GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.Controls;
                                break;
                            }
                        case 2:
                            {
                                Exit();
                                break;
                            }
                    }
                    ButtonTimer = 10;
                }
            }

            //Game Ended. Press space or enter to restart
            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.End)
            {
                if ((keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Enter)) && ButtonTimer <= 0)
                {
                    GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.Start;
                    ButtonTimer = 10;
                }
            }

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.Controls)
            {
                if ((keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space)) && ButtonTimer <= 0)
                {

                    GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.Start;
                    ButtonTimer = 10;
                }
            }

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.InGame)
            {

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                #region Update hud
                HUD.ActiveHUD.chooseslots(ref Player);
                HUD.ActiveHUD.p = Player.Position;
                HUD.ActiveHUD.angle = (float)Player.Rotation;
                HUD.ActiveHUD.playerhealth = (int)((float)Player.HealthPoints / (float)Player.MaxHealth * 100);
                Camera.ActiveCamera.dudeang = (float)Player.Rotation;

                mouseState = Mouse.GetState();

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


                if (Player.HealthPoints > 0)
                {
                    #region Collision Detection Box Placement Input

                    int modifier = 1;

                    if (keyboard.IsKeyDown(Keys.RightShift))
                    {
                        modifier = 10;
                    }

                    //Rotate World with Arrow Keys
                    /*if (keyboard.IsKeyDown(Keys.K))
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
            
                    //Create new Collision Box
                    if (keyboard.IsKeyDown(Keys.Enter) && ButtonTimer <= 0)
                    {

                        //Debug.WriteLine("Zombie z" + zCounter + " = new Zombie(500, 500, ZombieType.Adult, ref ZombieWalk, ref ZombieAttack, ref ZombieHurt, ref ZombieDie, DoAction, GetPathfindingNode);");
                        //Debug.WriteLine("z"+zCounter+".Position = new Vector3("+Player.Position.X+"f, 0, "+Player.Position.Z+"f);");
                        //Debug.WriteLine("zombies.Add(z"+zCounter+");");
                        //CollisionBoxes.Add(new Box(CollisionBoxes[CollisionBoxes.Count-1].Position,new Vector3(0),CollisionBoxes[CollisionBoxes.Count-1].Size));

                        //LevelQuadTree.Insert(CollisionBoxes[CollisionBoxes.Count - 1]);

                        //if (keyboard.IsKeyDown(Keys.RightShift))
                        //{
                        //    CollisionBoxes.Add(new Box(new Vector3(CollisionBoxes[CollisionBoxes.Count - 1].Position.X, CollisionBoxes[CollisionBoxes.Count - 1].Position.Y, CollisionBoxes[CollisionBoxes.Count - 1].Position.Z), new Vector3(0), new Vector3(CollisionBoxes[CollisionBoxes.Count - 1].Size.X, 20, CollisionBoxes[CollisionBoxes.Count - 1].Size.Z)));
                        //}
                        //else
                        //{
                        //    CollisionBoxes.Add(new Box(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), new Vector3(0), new Vector3(10, 20, 10)));
                        //}
                        ButtonTimer = 10;
                    }
                    */

                    #endregion
                    #region PathFindingNodeControls


                    /*//PathFinding Node Controls
            if (keyboard.IsKeyDown(Keys.Y) && (ButtonTimer <= 0 || keyboard.IsKeyDown(Keys.RightShift)))
            {
                currentNode -= modifier;
                if (currentNode < 0)
                    currentNode = 0;

                DestinationNode = currentNode;
                ButtonTimer = 7;
            }
            if (keyboard.IsKeyDown(Keys.U) && (ButtonTimer <= 0 || keyboard.IsKeyDown(Keys.RightShift)))
            {
                currentNode += modifier;
                

                if (currentNode >= PathFindingNodes.Count)
                    currentNode = PathFindingNodes.Count - 1;

                DestinationNode = currentNode;
                ButtonTimer = 7;
            }
            if (keyboard.IsKeyDown(Keys.O) && (ButtonTimer <= 0 || keyboard.IsKeyDown(Keys.RightShift)))
            {
                DestinationNode -= modifier;

                if (DestinationNode < 0)
                    DestinationNode = 0;
                ButtonTimer = 7;
            }
            if (keyboard.IsKeyDown(Keys.P) && (ButtonTimer <= 0 || keyboard.IsKeyDown(Keys.RightShift)))
            {
                DestinationNode += modifier;
                if (DestinationNode >= PathFindingNodes.Count)
                    DestinationNode = PathFindingNodes.Count-1;
                ButtonTimer = 7;
            }

            //Creation new PathFinding Nodes
            if (keyboard.IsKeyDown(Keys.Enter) && ButtonTimer <= 0)
            {

                Debug.WriteLine("PathFindingNodes.Add(new PathFinding.Node(new Vector3(" + Player.Position.X + "f, " + Player.Position.Y + "f, " + Player.Position.Z + "f)));");

                PathFindingNodes.Add(new PathFinding.Node(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z)));

                RecalculateNodes();
                ButtonTimer = 10;
            }

            //Creation new PathFinding Nodes
            if (keyboard.IsKeyDown(Keys.Enter) && ButtonTimer <= 0)
            {

                Debug.WriteLine("PathFindingNodes[" + currentNode + "].Links.Add(new Link(PathFindingNodes[" + DestinationNode + "]));");
                Debug.WriteLine("PathFindingNodes[" + DestinationNode + "].Links.Add(new Link(PathFindingNodes[" + currentNode + "]));");

                PathFindingNodes[currentNode].Links.Add(new Link(PathFindingNodes[DestinationNode]));
                PathFindingNodes[DestinationNode].Links.Add(new Link(PathFindingNodes[currentNode]));

                RecalculateNodes();
                ButtonTimer = 10;
            }
            */

                    #endregion

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

                    //Toggle PathFinding Graph Display
                    if (keyboard.IsKeyDown(Keys.P) && ButtonTimer <= 0)
                    {
                        ShowPathFindingGraph = !ShowPathFindingGraph;
                        ButtonTimer = 10;
                    }

                    //Toggle Collision Boxes Display
                    if (keyboard.IsKeyDown(Keys.N) && ButtonTimer <= 0)
                    {
                        ShowCollisionBoxes = !ShowCollisionBoxes;
                        ButtonTimer = 10;
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
                            case Keys.W:
                                {
                                    if (ButtonTimer <= 0)
                                    {
                                        Player.SwitchNextWeapon();
                                        ButtonTimer = 10;
                                    }
                                    break;
                                }
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
                                if (released)
                                {
                                    Player.SwitchNextItem();
                                    released = false;
                                }
                                break;

                            case Keys.Space:
                                if (KeyboardInput.ProcessInput(key, Player))
                                    Player.DoAction();
                                break;
                        }
                    }
                    if (keyboard.IsKeyUp(Keys.Tab))
                    {
                        released = true;

                    }

                    if (walk)
                        Player.animState = Entity.AnimationState.Walking;
                    else if (Player.animState != Entity.AnimationState.Hurt && Player.animState != Entity.AnimationState.Dying)
                        Player.animState = Entity.AnimationState.Idle;

                }
                else
                {
                    //Player is dead
                    if ((keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Enter)) && ButtonTimer <= 0)
                    {
                        GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.Start;
                        ButtonTimer = 10;
                    }
                }

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


                if (Player.SelectedItem != null && keyboard.IsKeyDown(Keys.Space) && Player.Stance == AnimationStance.Standing && Player.SelectedItem.itemType == ItemType.Extinguisher)
                {
                    CheckCollisions(true);
                }
                else
                {
                    CheckCollisions(false);

                }

                Camera.ActiveCamera.CameraPosition = Player.Position + new Vector3(0, 30, 30) + Camera.ActiveCamera.CameraZoom;
                Camera.ActiveCamera.CameraLookAt = Player.Position;

                globalEffect.View = Camera.ActiveCamera.View;
                globalEffect.World = world;
                globalEffect.Projection = Camera.ActiveCamera.Projection;


                FireEmitter.particleGroups[0].effect.View = Camera.ActiveCamera.View;
                FireEmitter2.particleGroups[0].effect.View = Camera.ActiveCamera.View;
                FireEmitter3.particleGroups[0].effect.View = Camera.ActiveCamera.View;
                FireEmitter4.particleGroups[0].effect.View = Camera.ActiveCamera.View;
                ChemicalsEmitter.particleGroups[0].effect.View = Camera.ActiveCamera.View;
                FireEmitter.UpdateEmitter(gameTime);
                FireEmitter2.UpdateEmitter(gameTime);
                FireEmitter3.UpdateEmitter(gameTime);
                FireEmitter4.UpdateEmitter(gameTime);

                if (Player.SelectedItem != null && keyboard.IsKeyDown(Keys.Space) && Player.Stance == AnimationStance.Standing && Player.SelectedItem.itemType == ItemType.Extinguisher)
                {
                    sound.playExtinguisher();
                    ChemicalsEmitter.Start();
                }
                else
                {
                    sound.StopExtinguisher();
                    ChemicalsEmitter.Stop();
                }

                if (fireDamageDelay > 0)
                    fireDamageDelay -= 1;

                ChemicalsEmitter.UpdateEmitter(gameTime);
                ChemicalsEmitter.particleGroups[0].controller.Velocity = Player.Velocity / (Player.Velocity.Length() * 16f);
                ChemicalsEmitter.position = Player.Position + new Vector3(0, 5, 0); ;
                ChemicalsEmitter.particleGroups[0].controller.InitialPositionOffset = (Player.Velocity / Player.Velocity.Length()) * 2;

                ItemRotation += 0.01f % ((float)Math.PI * 2);
                ItemHeight = (float)Math.Cos(ItemRotation * 4);

            }

            if (ButtonTimer > 0)
                ButtonTimer -= 1;

            base.Update(gameTime);
        }        

        private void CheckCollisions(bool Extinguisher)
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

            Contact EndingContact = heroSphere.Collides(EscapeSpot);

            if (EndingContact != null && (Player.ItemsList.ContainsKey(key1) || Player.ItemsList.ContainsKey(key2)))
            {
                GameStates.GameStates.ZombieGameState = GameStates.GameStates.GameState.End;
            }

            primitivesNearby.AddRange(fireHazards);
            foreach (Primitive p in primitivesNearby)
            {
                Contact c = heroSphere.Collides(p as Box);
                if (c != null)
                {
                    ResolveStaticCollision(c, Player, heroSphere);
                    if(fireDamageDelay <=0)
                    {
                        if(((Box)p).Tag == "Fire1" || ((Box)p).Tag == "Fire2" || ((Box)p).Tag == "Fire3" || ((Box)p).Tag == "Fire4")
                        {
                            Player.TakeDamage(25);
                            fireDamageDelay = 5;
                        }
                    }
                }
            }

            if (Extinguisher)
            {
                Sphere ExtSphere = new Sphere(Player.Position + (Player.Velocity / Player.Velocity.Length()) * 20, Vector3.One, 10);

                for (int i = 0; i < fireHazards.Count;i++)
                {
                    Sphere boxSphere = new Sphere(fireHazards[i].Position, Vector3.One, fireHazards[i].Size.X / 2);

                    Contact contact = ExtSphere.Collides(boxSphere);
                    if (contact != null)
                    {
                        if (fireHazards[i].Tag == "Fire1")
                        {
                            FireEmitter.particleGroups[0].controller.LifeSpan -= 5;
                            if (FireEmitter.particleGroups[0].controller.LifeSpan <= 0)
                            {
                                FireEmitter.Stop();
                                fireHazards.Remove(fireHazards[i]);
                            }
                        }
                        if (fireHazards.Count > 0)
                        {
                            if (fireHazards[i].Tag == "Fire2")
                            {
                                FireEmitter2.particleGroups[0].controller.LifeSpan -= 5;
                                if (FireEmitter2.particleGroups[0].controller.LifeSpan <= 0)
                                {
                                    FireEmitter2.Stop();
                                    fireHazards.Remove(fireHazards[i]);
                                }
                            }
                            if (fireHazards[i].Tag == "Fire3")
                            {
                                FireEmitter3.particleGroups[0].controller.LifeSpan -= 5;
                                if (FireEmitter3.particleGroups[0].controller.LifeSpan <= 0)
                                {
                                    FireEmitter3.Stop();
                                    fireHazards.Remove(fireHazards[i]);
                                }
                            }
                            if (fireHazards[i].Tag == "Fire4")
                            {
                                FireEmitter4.particleGroups[0].controller.LifeSpan -= 5;
                                if (FireEmitter4.particleGroups[0].controller.LifeSpan <= 0)
                                {
                                    FireEmitter4.Stop();
                                    fireHazards.Remove(fireHazards[i]);
                                }
                            }
                        }
                    }
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
                    primitives.AddRange(fireHazards);
                    foreach (Primitive p in primitives)
                    {
                        Contact c = zombieSphere.Collides(p as Box);
                        if (c != null)
                        {
                            if (z.BehaviouralState == BehaviourState.Wander)
                                z.Velocity = Vector3.Cross( z.Velocity, Vector3.Up);
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
            checkItemCollisions();
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
            else if (objectCasted == null)
            {
                // Hero is walking, generate sound radius
                foreach (Zombie z in zombies)
                {
                    float walkRadius = (((actionCaster as Hero).PowerupsList.Contains(new Powerup(PowerupType.Sneakers))) ? WALK_SOUND_RADIUS / 2 : WALK_SOUND_RADIUS);
                    if ((z.Position - actionCaster.Position).Length() < walkRadius)
                        z.Alert(actionCaster as Hero);
                }
            }
        }

        private void DoAttack(Weapon weapon, Entity actionCaster)
        {
            // apply silencer if possible
            if (weapon.weaponType == Entities.WeaponType.Handgun9mm && (actionCaster as Hero).PowerupsList.Contains(silencer))
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
                if (Player.PowerupsList.Contains(silencer))
                {
                    sound.StopSilencer();
                    //need to check the time
                    sound.playSilencer();
                }
                else
                {
                    sound.Stopgun();
                    sound.playgun();
                }
            }
            if (weapon.weaponType == WeaponType.Magnum)
            {
                sound.StopMagnum();
                //need to check the time
                sound.playMagnum();
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
        private void checkItemCollisions()
        {
            List<Entity> toRemove = new List<Entity>();
            if(PickupableObjects.Count > 0)
            {
            //check for collisions with map items
                foreach(Entity e in PickupableObjects)
                {
                    if ((e.Position - Player.Position).Length() < COLLISION_ITEM_RANGE)
                    {
                        Player.AddEquipment(e);
                        toRemove.Add(e);
                    }
                }
                foreach (Entity et in toRemove)
                {
                    PickupableObjects.Remove(et);
                }

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
                        if (Player.ItemsList.Count > 1)
                             Player.ItemsList[item]--;
                        break;
                    }
                case ItemType.Key:
                    {
                        //if (lock in range)
                        //{
                        //      unlock
                        //if(Player.ItemsList.Count > 1)
                        //Player.ItemsList[item]--;
                        //}
                        break;
                    }
                case ItemType.Extinguisher:
                    {
                        //List<Box> firesToRemove = new List<Box>();
                        //Ray ray = new Ray(actionCaster.Position, Vector3.Normalize(actionCaster.Velocity));
                        //foreach (Box hazard in fireHazards)
                        //{
                        //    if ((hazard.Position - Player.Position).Length() < item.Range)
                        //    {
                        //        BoundingBox bbox = new BoundingBox(
                        //            new Vector3(hazard.Position.X - hazard.Size.X / 2, hazard.Position.Y - hazard.Size.Y / 2, hazard.Position.Z - hazard.Size.Z / 2),
                        //            new Vector3(hazard.Position.X + hazard.Size.X / 2, hazard.Position.Y + hazard.Size.Y / 2, hazard.Position.Z + hazard.Size.Z / 2)
                        //        );
                        //        if (ray.Intersects(bbox) != null)
                        //        {
                        //            firesToRemove.Add(hazard);
                        //        }
                        //    }
                        //}
                        //// remove any intersecting fires
                        //foreach (Box toRemove in firesToRemove)
                        //{
                        //    fireHazards.Remove(toRemove);
                        //}
                        
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

            #region Start Draw

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.Start)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(Splash, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight), Color.White);

                switch (StartGameOption)
                {
                    case 0:
                        {
                            spriteBatch.DrawString(SplashFont, "<< Start Game >>", new Vector2(452, 352), Color.Black);
                            spriteBatch.DrawString(SplashFont, "<< Start Game >>", new Vector2(450, 350), Color.Red);

                            spriteBatch.DrawString(SplashFont, "    Controls    ", new Vector2(462, 392), Color.Black);
                            spriteBatch.DrawString(SplashFont, "    Controls    ", new Vector2(460, 390), Color.Red);

                            spriteBatch.DrawString(SplashFont, "   Quit Game   ", new Vector2(472, 432), Color.Black);
                            spriteBatch.DrawString(SplashFont, "   Quit Game   ", new Vector2(470, 430), Color.Red);
                            break;
                        }
                    case 1:
                        {
                            spriteBatch.DrawString(SplashFont, "   Start Game   ", new Vector2(452, 352), Color.Black);
                            spriteBatch.DrawString(SplashFont, "   Start Game   ", new Vector2(450, 350), Color.Red);

                            spriteBatch.DrawString(SplashFont, "<<  Controls  >>", new Vector2(462, 392), Color.Black);
                            spriteBatch.DrawString(SplashFont, "<<  Controls  >>", new Vector2(460, 390), Color.Red);

                            spriteBatch.DrawString(SplashFont, "   Quit Game   ", new Vector2(472, 432), Color.Black);
                            spriteBatch.DrawString(SplashFont, "   Quit Game   ", new Vector2(470, 430), Color.Red);
                            break;
                        }
                    case 2:
                        {
                            spriteBatch.DrawString(SplashFont, "   Start Game   ", new Vector2(452, 352), Color.Black);
                            spriteBatch.DrawString(SplashFont, "   Start Game   ", new Vector2(450, 350), Color.Red);

                            spriteBatch.DrawString(SplashFont, "    Controls    ", new Vector2(462, 392), Color.Black);
                            spriteBatch.DrawString(SplashFont, "    Controls    ", new Vector2(460, 390), Color.Red);

                            spriteBatch.DrawString(SplashFont, "<< Quit Game >>", new Vector2(472, 432), Color.Black);
                            spriteBatch.DrawString(SplashFont, "<< Quit Game >>", new Vector2(470, 430), Color.Red);
                            break;
                        }
                }

                spriteBatch.End();
            }

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.Controls)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(Controls, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight), Color.White);

                spriteBatch.End();
            }

            #endregion

            #region In Game Draw

            if (GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.InGame || GameStates.GameStates.ZombieGameState == GameStates.GameStates.GameState.End)
            {
                DrawSchool();
                DrawModel(Player);

                #region Collision Detection Helpers

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
                #endregion

                #region Path Finding Helpers


                if (ShowPathFindingGraph)
                {
                    for (int i = 0; i < PathFindingNodes.Count; i++)
                    {
                        //Draw Graph Nodes
                        DrawPathFindingNode(NodeModel, Matrix.CreateTranslation(PathFindingNodes[i].position) * world, globalEffect.View, globalEffect.Projection);
                    }

                    globalEffect.LightingEnabled = false;
                    globalEffect.CurrentTechnique.Passes[0].Apply();
                    //Draw Links between nodes
                    VertexPositionColor[] LinkLines;
                    LinkLines = vpc;

                    VertexDeclaration VertexDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0));
                    VertexBuffer vertexBuffer;

                    if (LinkLines.Length > 0)
                    {
                        vertexBuffer = new VertexBuffer(GraphicsDevice, VertexDecl, LinkLines.Length, BufferUsage.None);
                        vertexBuffer.SetData<VertexPositionColor>(LinkLines);

                        GraphicsDevice.SetVertexBuffer(vertexBuffer);
                        GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, 0, LinkLines.Length);

                        vertexBuffer.Dispose();
                    }

                    #region PathFinding links helper drawings


                    /*//Determine set of lines to draw for Graph Links
                vpc = new VertexPositionColor[PathFindingNodes[currentNode].Links.Count * 2];

                int vIndex = 0;

                for (int j = 0; j < PathFindingNodes[currentNode].Links.Count; j++)
                {
                    vpc[vIndex] = new VertexPositionColor(PathFindingNodes[currentNode].position, Color.Red);
                    vpc[vIndex + 1] = new VertexPositionColor((PathFindingNodes[currentNode].Links[j].node.position), Color.Red);
                    vIndex += 2;
                }

                LinkLines = vpc;

                if (LinkLines.Length > 0)
                {
                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexDecl, LinkLines.Length, BufferUsage.None);
                    vertexBuffer.SetData<VertexPositionColor>(LinkLines);

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, 0, LinkLines.Length/2);

                    vertexBuffer.Dispose();
                }
                
                vpc = new VertexPositionColor[2];
                
                vpc[0] = new VertexPositionColor(PathFindingNodes[currentNode].position+ new Vector3(0,1,0), Color.Blue);
                vpc[1] = new VertexPositionColor((PathFindingNodes[DestinationNode].position) + new Vector3(0, 1, 0), Color.Blue);

                LinkLines = vpc;

                if (LinkLines.Length > 0)
                {
                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexDecl, LinkLines.Length, BufferUsage.None);
                    vertexBuffer.SetData<VertexPositionColor>(LinkLines);

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, 0, LinkLines.Length/2);

                    vertexBuffer.Dispose();
                }

                //DrawPathFindingNode(StartNode, Matrix.CreateTranslation(PathFindingNodes[currentNode].position) * world, globalEffect.View, globalEffect.Projection,6);
                //DrawPathFindingNode(EndNode, Matrix.CreateTranslation(PathFindingNodes[DestinationNode].position) * world, globalEffect.View, globalEffect.Projection,6);
                

                BlendState blend = GraphicsDevice.BlendState;
                DepthStencilState depthState = GraphicsDevice.DepthStencilState;

                spriteBatch.Begin();


                spriteBatch.DrawString(Font1, "Current Node     : " + currentNode, new Vector2(0, 0), Color.Yellow);
                spriteBatch.DrawString(Font1, "Destination Node : " + DestinationNode, new Vector2(0, 20), Color.Yellow);
                spriteBatch.End();

                GraphicsDevice.BlendState = blend;
                GraphicsDevice.DepthStencilState = depthState;
                */
                    #endregion
                }

                #endregion

                foreach (Zombie z in zombies)
                {
                    if ((z.Position - Player.Position).Length() < SIGHT_RADIUS)
                        DrawModel(z);
                }


                foreach (Entity ent in PickupableObjects)
                {
                    if (ent is Item)
                        DrawModel(ent as Item);
                    else if (ent is Weapon)
                        DrawModel(ent as Weapon);
                    else if (ent is Powerup)
                        DrawModel(ent as Powerup);

                }

                EmitterDraw(FireEmitter);
                EmitterDraw(FireEmitter2);
                EmitterDraw(FireEmitter3);
                EmitterDraw(FireEmitter4);
                EmitterDraw(ChemicalsEmitter);
            }

            #endregion

            base.Draw(gameTime);
        }

        private void DrawPathFindingNode(Model model, Matrix world, Matrix view, Matrix projection,int scale = 2)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();


                    effect.World = Matrix.CreateScale(scale) * world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                   

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.AmbientLightColor = new Vector3(1, 1, 1);
                }

                mesh.Draw();
            }
        }

        private void DrawModel(Item ent)
        {
            float scale = 1;

            if (ent.itemType == ItemType.Extinguisher)
                scale = 4;
            else if (ent.itemType == ItemType.Key)
                scale = 2;
            else if (ent.itemType == ItemType.MedPack)
                scale = 400;

            // Render the skinned mesh
            foreach (ModelMesh mesh in ent.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY(ItemRotation) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(ent.Position + new Vector3(0, ItemHeight, 0));
                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }

        private void DrawModel(Weapon ent)
        {
            // Render the skinned mesh
            foreach (ModelMesh mesh in ent.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY(ItemRotation) * Matrix.CreateScale(4) * Matrix.CreateTranslation(ent.Position + new Vector3(0, ItemHeight, 0));
                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }

        private void DrawModel(Powerup ent)
        {
            // Render the skinned mesh
            foreach (ModelMesh mesh in ent.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationY(ItemRotation) * Matrix.CreateScale(4) * Matrix.CreateTranslation(ent.Position + new Vector3(0, ItemHeight, 0));
                    effect.View = Camera.ActiveCamera.View;

                    effect.Projection = Camera.ActiveCamera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
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

            if (Player.Stance == AnimationStance.Shooting && Player.animState == Entity.AnimationState.Idle)
            {

                if(Player.EquippedWeapon != null){
                // Render the skinned mesh
                foreach (ModelMesh mesh in hero.EquippedWeapon.model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateTranslation(hero.EquippedWeapon.offset) * Matrix.CreateRotationY((float)(hero.Rotation + Math.PI / 2)) * Matrix.CreateTranslation(hero.Position);// 

                        effect.View = Camera.ActiveCamera.View;
                        effect.Projection = Camera.ActiveCamera.Projection;

                        effect.EnableDefaultLighting();

                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }

                    mesh.Draw();
                }


              //  if it has a silencer

                if (Player.PowerupsList.Contains(silencer) && hero.EquippedWeapon == socom)
                {
                    // Render the skinned mesh
                    foreach (ModelMesh mesh in Silenced9mm.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.World = Matrix.CreateTranslation(hero.EquippedWeapon.offset) * Matrix.CreateRotationY((float)(hero.Rotation + Math.PI / 2)) * Matrix.CreateScale(1) * Matrix.CreateTranslation(hero.Position);// 

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
            if (zombie.animationPlayer != null)
            {
                Matrix[] bones = zombie.animationPlayer.GetSkinTransforms();
                float scale = .1f;
                if (zombie.zombieType == ZombieType.Boss)
                    scale = .2f;
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
                            Matrix.CreateScale(scale) * Matrix.CreateTranslation(zombie.Position);
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

        //Draw All the particles found in each particle group of the given emitter
        protected void EmitterDraw(ParticleEmitter emitter)
        {
            foreach (ParticleGroup group in emitter.particleGroups)
            {
                VertexBuffer vertexBuffer;

                GraphicsDevice.BlendState = group.blendState;
                GraphicsDevice.DepthStencilState = group.depthStencil;
                group.effect.CurrentTechnique.Passes[0].Apply();

                //Display Particles in their flat 2D form or fake 3D feel
                if (true)
                {
                    group.LoadVertexArray(ParticleAppearance.ThreeDimensional);
                }
                else
                {
                    group.LoadVertexArray(ParticleAppearance.Flat);
                }

                //Draw each particle
                if (group.vertices.Length > 0)
                {
                    vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, group.vertices.Length, BufferUsage.None);
                    vertexBuffer.SetData<VertexPositionNormalTexture>(group.vertices);

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, 0, (group.vertices.Length / 6) + 1);
                    vertexBuffer.Dispose();
                }
            }
        }
    }
}