﻿using System;
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
        SoundEffectInstance guninst;

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


        public void LoadSounds()
        {

             gun = content.Load<SoundEffect>("gunsound1");
     
   
        }

    

    }
}
