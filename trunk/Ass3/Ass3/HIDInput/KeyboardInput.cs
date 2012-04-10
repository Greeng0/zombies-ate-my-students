using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Entities;

namespace HIDInput
{
    static class KeyboardInput
    {
        static public bool ProcessInput(Keys key, Hero player)
        {
            switch (player.animState)
            {
                case Entity.AnimationState.Idle:
                    {
                        if (player.Stance == AnimationStance.Standing)
                        {
                            return true;
                        }
                        else
                        {
                            switch (key)
                            {
                                case Keys.Left:
                                case Keys.Right:
                                case Keys.Space:
                                case Keys.W:
                                case Keys.Tab:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
                        }
                    }
                case Entity.AnimationState.Walking:
                    {
                        if (player.Stance == AnimationStance.Standing)
                        {
                            switch (key)
                            {
                                case Keys.Up:
                                case Keys.Down:
                                case Keys.Left:
                                case Keys.Right:
                                case Keys.Tab:
                                case Keys.Space:
                                case Keys.W:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
                        }
                        else
                        {
                            switch (key)
                            {
                                case Keys.Left:
                                case Keys.Right:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
                        }
                    }
                case Entity.AnimationState.Shooting:
                    {
                        if (player.Stance == AnimationStance.Shooting)
                        {
                            switch (key)
                            {
                                case Keys.Tab:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Entity.AnimationState.Hurt:
                    {
                        switch (key)
                        {
                            case Keys.W:
                            case Keys.Tab:
                                {
                                    return true;
                                }
                            default:
                                {
                                    return false;
                                }
                        }
                    }
                case Entity.AnimationState.StanceChange:
                    {
                            switch (key)
                            {
                                case Keys.Left:
                                case Keys.Right:
                                case Keys.W:
                                case Keys.Tab:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
    
                    }
                case Entity.AnimationState.UseItem:
                    {
                        if (player.Stance == AnimationStance.Standing)
                        {
                            switch (key)
                            {
                                case Keys.Tab:
                                case Keys.W:
                                    {
                                        return true;
                                    }
                                default:
                                    {
                                        return false;
                                    }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                case Entity.AnimationState.Dying:
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
