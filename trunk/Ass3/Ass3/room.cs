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
    class room : DrawableGameComponent
    {

        // A variable containing our content manager
        private ContentManager _content;
        // Our model
        public Model _model;
        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] _boneTransforms;
        // Store the original transform matrix for each animating bone.

        private static room activeroom = null;
        public Matrix front;
         private Vector3 position = new Vector3();


        BoundingSphere bounds;

        public BoundingSphere Bounds//Jo restructured it now it returns bouds of right position on map
        {
            get
            {
                BoundingSphere b = new BoundingSphere(Position, 1);

                return b;
            }
            set { bounds = value; }
        }
        public static room Activeroom
        {
            get { return activeroom; }
            set { activeroom = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Model Model
        {
            get { return _model; }

        }



        public room(Game game, ContentManager content, Vector3 po)
            : base(game)
        {


            if (Activeroom == null)
                Activeroom = this;

            position = po;
            _content = content;
        }
        protected void collides(Model m)
        {

        }

        protected override void LoadContent()
        {

            _model = _content.Load<Model>("School");

            // Allocate the transform matrix array.
            _boneTransforms = new Matrix[_model.Bones.Count];


            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {


            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {

            SamplerState sample = new SamplerState();

            sample.AddressU = TextureAddressMode.Wrap;
            sample.AddressV = TextureAddressMode.Wrap;
            

            //_model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
            foreach (ModelMesh mesh in _model.Meshes)
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

            base.Draw(gameTime);
        }
    }
}
