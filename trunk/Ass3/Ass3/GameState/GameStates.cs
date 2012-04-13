using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameStates
{
    public class GameStates
    {
        public enum GameState
        {
            Start,
            Controls,
            InGame,
            End
        }

        
        public static GameState ZombieGameState;
    }
}
