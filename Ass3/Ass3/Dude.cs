using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Text;
using SkinnedModel;

namespace zombies
{
    public class dude : DrawableGameComponent
    {


        // A variable containing our content manager
        private ContentManager _content;
        // Our model
        public Model _model;

        Matrix[] _boneTransforms;
        AnimationPlayer animationPlayer;
        private static dude activedude = null;
     
        private Vector3 velocity = new Vector3(0, 0, 0);
        private Vector3 position = new Vector3(0);
        private float angle = 0;
        AnimationPlayer player;// This calculates the Matrices of the animation
        AnimationClip clip;// This contains the keyframes of the animation
        SkinningData skin;// This contains all the skinning data
        Model model;// The actual model
        private float scale = .1f;
        public bool walking = false;
        public float speed = 0.4f;
        public static dude Activedude
        {
            get { return activedude; }
            set { activedude = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }
        public Model Model
        {
            get { return _model; }

        }
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }


        public dude(Game game, ContentManager content, float an, Vector3 po)
            : base(game)
        {

            // Velocity = an;
            _model = this.Model;
            angle = an;
            Position = po;
            _content = content;


        }


        protected override void LoadContent()
        {
                      // Load the model.
            _model = _content.Load<Model>("HeroWalk");

            // Look up our custom skinning information.
            SkinningData skinningData = (SkinningData)_model.Tag;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip);

            base.LoadContent();
        }


       
        public override void Update(GameTime gameTime)
        {
            if (walking)
            {
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            }
            else
            {
                animationPlayer.ResetClip();
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            }
 
            base.Update(gameTime);

        }
      

        public override void Draw(GameTime gameTime)
        {


            Matrix[] bones = animationPlayer.GetSkinTransforms();
       
            if (walking)
            {
                // Render the skinned mesh. if animation
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateRotationY(angle) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(Position);
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
         else{
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateRotationY(angle) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(Position);
                        //effect.SetBoneTransforms(bones);
                        effect.View = Camera.ActiveCamera.View;

                        effect.Projection = Camera.ActiveCamera.Projection;

                        effect.EnableDefaultLighting();

                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;

                    }
               
                mesh.Draw(); 
                }
            }
         
            base.Draw(gameTime);


        }


    }
}
