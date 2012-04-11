using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;

// Sound Management Class
namespace Sounds 
{ 
  public class Sounds : DrawableGameComponent
    {
    
      //soundeffects
        private SoundEffect gun ;
        private SoundEffect magnumSound;
        private SoundEffect ExtinguisherSound;
        private SoundEffect SilencerSound;

        SoundEffectInstance guninst;
        SoundEffectInstance Extinst;
        SoundEffectInstance Maginst;
        SoundEffectInstance Silinst;

        private ContentManager content;
     
        public Sounds(Game game, ContentManager content) : base(game)
        {

            this.content = content;
        }
    
        public void playgun()
        {
            if (guninst == null || guninst.State == SoundState.Stopped)
            {
                guninst = gun.CreateInstance();
                guninst.Play();
            }
        }

        public void Stopgun()
        {
            if (guninst != null)
            {
                guninst.Stop(true);
            }
        }

        public void playExtinguisher()
        {
            if (Extinst == null || Extinst.State == SoundState.Stopped)
            {
                Extinst = ExtinguisherSound.CreateInstance();
                Extinst.Play();
            }
        }

        public void StopExtinguisher()
        {
            if (Extinst != null)
            {
                Extinst.Stop(true);
            }
        }

        public void playMagnum()
        {
            if (Maginst == null || Maginst.State == SoundState.Stopped)
            {
                Maginst = magnumSound.CreateInstance();
                Maginst.Play();
            }
        }

        public void StopMagnum()
        {
            if (Maginst != null)
            {
                Maginst.Stop(true);
            }
        }

        public void playSilencer()
        {
            if (Silinst == null || Silinst.State == SoundState.Stopped)
            {
                Silinst = SilencerSound.CreateInstance();
                Silinst.Play();
            }
        }

        public void StopSilencer()
        {
            if (Silinst != null)
            {
                Silinst.Stop(true);
            }
        }

        public void LoadSounds()
        {
            gun = content.Load<SoundEffect>("gunsound1");
            magnumSound = content.Load<SoundEffect>("MagnumSound");
            ExtinguisherSound = content.Load<SoundEffect>("ExtinguisherSound");
            SilencerSound = content.Load<SoundEffect>("SilencerSound");
        }
    }
}
