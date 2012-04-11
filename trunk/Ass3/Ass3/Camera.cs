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
    public class Camera : DrawableGameComponent
    {
        private static Camera activeCamera = null;

        public Vector3 CameraPosition = new Vector3(0, 40, 30);
        public Vector3 CameraLookAt = new Vector3(0, 0, 0);
        public Vector3 CameraZoom = new Vector3(0,00,0);
        // View and projection
        protected Matrix projection = Matrix.Identity;
        private Matrix view = Matrix.Identity;
        private Vector3 position = new Vector3(0, 200, 0);
        Vector3 up = new Vector3(0,1,0);
        public Vector3 dudepo = new Vector3(0);
        public float dudeang =0;
        public static Camera ActiveCamera
        {
            get { return activeCamera; }
            set { activeCamera = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Matrix View
        {
            get { return view; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float PosY
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

    

        public Camera(Game game)
            : base(game)
        {
            if (ActiveCamera == null)
                ActiveCamera = this;
        }

        public override void Initialize()
        {
            int centerX = Game.Window.ClientBounds.Width;
            int centerY = Game.Window.ClientBounds.Height;

            Mouse.SetPosition(centerX, centerY);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            float ratio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1000f / 600f, .5f, 100000);
            //projection = Matrix.CreateOrthographic(150, 90, 0.1f, 2000);
            base.LoadContent();
        }
      
        public override void Update(GameTime gameTime)
        {
            view = Matrix.CreateLookAt(CameraPosition, CameraLookAt, up);
            base.Update(gameTime);
        }
    }

}
