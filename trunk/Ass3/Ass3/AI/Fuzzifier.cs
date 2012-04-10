using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI
{
    public enum BehaviourState
    {
        MeleePursue,
        RangedPursue,
        MeleeCreep,
        RangedCreep,
        Flee,
        SlowFlee,
        Wander
    }

    enum FuzzyBehaviour
    {
        CloseIn,
        Flee,
        MeleeFight,
        MeleeTroll,
        RangedFight,
        RangedTroll
    }

    class Fuzzifier
    {
        #region Fields
        private const float HEALTH_LOW = 0.33f;             // health factor separating critical from injured status
        private const float HEALTH_HIGH = 0.66f;            // health factor separating injured from healthy status
        private const float DISTANCE_MELEE = 0.1f;          // distance factor separating melee from projectile range
        private const float DISTANCE_PROJECTILE = 0.3f;     // distance factor separating projectile from outOfRange range
        private const float DISTANCE_OUT = 0.5f;            // largest distance factor for which projectile range fuzzy set should have value

        private float Health_Critical;                      // fuzzy set for critical health value
        private float Health_Injured;                       // fuzzy set for injured health value
        private float Health_Healthy;                       // fuzzy set for healthy health value
        private float TargetHealth_Critical;                // fuzzy set for critical target health value
        private float TargetHealth_Injured;                 // fuzzy set for injured target health value
        private float TargetHealth_Healthy;                 // fuzzy set for healthy target health value
        private float Range_Melee;                          // fuzzy set for melee range value
        private float Range_Projectile;                     // fuzzy set for projectile range value
        private float Range_OutOfRange;                     // fuzzy set for outOfRange range value
        #endregion

        /// <summary>
        /// Initializes the Fuzzifier object and fuzzifies the inputs.
        /// </summary>
        /// <param name="health">Float between 0 and 1 representing the health percentage.</param>
        /// <param name="targetHealth">Float between 0 and 1 representing the target's health percentage.</param>
        /// <param name="distToTarget">Float indicating the magnitude of the distance vector between the agent and its target.</param>
        public Fuzzifier(float health, float targetHealth, float distToTarget)
        {
            FuzzifyHealth(health);
            FuzzifyTargetHealth(targetHealth);
            FuzzifyTargetDistance(distToTarget);
        }

        /// <summary>
        /// Initiates the Rule Evaluation and Defuzzification processes.
        /// </summary>
        /// <returns>BehaviourState to be adopted by the NPC</returns>
        public BehaviourState GetBehaviour()
        {
            FuzzyBehaviour highestOutput = GetFuzzyOutput();
            return Defuzzify(highestOutput);
        }

        #region Fuzzification
        /// <summary>
        /// Computes degrees of membership for health fuzzy set. Membership functions
        /// are described in the Zombies Ate My Neighbors Design Document.
        /// </summary>
        private void FuzzifyHealth(float health)
        {
            if (health < HEALTH_LOW)
            {
                Health_Critical = 1;
                Health_Injured = health / HEALTH_LOW;
                Health_Healthy = 0;
            }
            else if (health < HEALTH_HIGH)
            {
                Health_Critical = (HEALTH_HIGH - health) / HEALTH_LOW;
                Health_Injured = 1;
                Health_Healthy = (health - HEALTH_LOW) / (HEALTH_HIGH - HEALTH_LOW);
            }
            else
            {
                Health_Critical = 0;
                Health_Injured = (1 - health) / (1 - HEALTH_HIGH);
                Health_Healthy = 1;
            }
        }

        /// <summary>
        /// Computes degrees of membership for target health fuzzy set. Membership functions
        /// are described in the Zombies Ate My Neighbors Design Document.
        /// </summary>
        private void FuzzifyTargetHealth(float health)
        {
            if (health < HEALTH_LOW)
            {
                TargetHealth_Critical = 1;
                TargetHealth_Injured = health / HEALTH_LOW;
                TargetHealth_Healthy = 0;
            }
            else if (health < HEALTH_HIGH)
            {
                TargetHealth_Critical = (HEALTH_HIGH - health) / HEALTH_LOW;
                TargetHealth_Injured = 1;
                TargetHealth_Healthy = (health - HEALTH_LOW) / (HEALTH_HIGH - HEALTH_LOW);
            }
            else
            {
                TargetHealth_Critical = 0;
                TargetHealth_Injured = (1 - health) / (1 - HEALTH_HIGH);
                TargetHealth_Healthy = 1;
            }
        }

        /// <summary>
        /// Computes degrees of membership for range fuzzy set. Membership functions
        /// are described in the Zombies Ate My Neighbors Design Document.
        /// </summary>
        private void FuzzifyTargetDistance(float distance)
        {
            if (distance < DISTANCE_MELEE)
            {
                Range_Melee = 1;
                Range_Projectile = distance / DISTANCE_MELEE;
                Range_OutOfRange = 0;
            }
            else if (distance < DISTANCE_PROJECTILE)
            {
                Range_Melee = (DISTANCE_PROJECTILE - distance) / (DISTANCE_PROJECTILE - DISTANCE_MELEE);
                Range_Projectile = 1;
                Range_OutOfRange = (distance - DISTANCE_MELEE) / (DISTANCE_PROJECTILE - DISTANCE_MELEE);
            }
            else if (distance < DISTANCE_OUT)
            {
                Range_Melee = 0;
                Range_Projectile = (DISTANCE_OUT - distance) / (DISTANCE_OUT - DISTANCE_PROJECTILE);
                Range_OutOfRange = 1;
            }
            else
            {
                Range_Melee = 0;
                Range_Projectile = 0;
                Range_OutOfRange = 1;
            }
        }
        #endregion

        #region Rule Evaluation
        /// <summary>
        /// Evaluates a set of rules to assign a value to each FuzzyBehaviour.
        /// </summary>
        /// <returns>FuzzyBehaviour with the highest assigned value.</returns>
        private FuzzyBehaviour GetFuzzyOutput()
        {
            Dictionary<FuzzyBehaviour, float> fuzzyOutput = new Dictionary<FuzzyBehaviour, float>();

            float closeIn = And(Range_OutOfRange, Health_Healthy);
            fuzzyOutput.Add(FuzzyBehaviour.CloseIn, closeIn);

            float flee = Or(And(Health_Critical, TargetHealth_Healthy), And(Range_Melee, Not(Health_Healthy)));
            fuzzyOutput.Add(FuzzyBehaviour.Flee, flee);

            float meleeFight = Or(And(Range_Melee, TargetHealth_Healthy), And(Range_Projectile, TargetHealth_Critical));
            fuzzyOutput.Add(FuzzyBehaviour.MeleeFight, meleeFight);

            float meleeTroll = Or(And(Range_Melee, Not(TargetHealth_Healthy)), And(Range_Projectile, TargetHealth_Critical));
            fuzzyOutput.Add(FuzzyBehaviour.MeleeTroll, meleeTroll);

            float rangedFight = And(Range_Projectile, TargetHealth_Healthy);
            fuzzyOutput.Add(FuzzyBehaviour.RangedFight, rangedFight);

            float rangedTroll = And(Range_Projectile, Not(TargetHealth_Healthy));
            fuzzyOutput.Add(FuzzyBehaviour.RangedTroll, rangedTroll);

            // Finds and returns Key with the highest associated value in the fuzzyOutput dictionary
            return fuzzyOutput.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }
        #endregion

        #region Defuzzification
        /// <summary>
        /// Converts fuzzy output to crisp output by finding the appropriate set of crisp behaviours 
        /// associated with the fuzzy output and choosing a random behaviour from this set.
        /// </summary>
        /// <param name="fuzzyOutput">FuzzyBehaviour representing a possible set of behaviours.</param>
        /// <returns>A random BehaviourState associated to the fuzzy output</returns>
        private BehaviourState Defuzzify(FuzzyBehaviour fuzzyOutput)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int next;
            switch (fuzzyOutput)
            {
                case FuzzyBehaviour.CloseIn:
                    {
                        next = rand.Next(1, 8);
                        switch (next)
                        {
                            case 1:
                            case 2:
                            case 3:
                                return BehaviourState.MeleePursue;
                            case 4:
                            case 5:
                            case 6:
                                return BehaviourState.MeleeCreep;
                            case 7:
                                return BehaviourState.Wander;
                        }
                        break;
                    }
                case FuzzyBehaviour.Flee:
                    {
                        next = rand.Next(1, 5);
                        switch (next)
                        {
                            case 1:
                            case 2:
                            case 3:
                                return BehaviourState.Flee;
                            case 4:
                                return BehaviourState.RangedPursue;
                        }
                        break;
                    }
                case FuzzyBehaviour.MeleeFight:
                    {
                        return BehaviourState.MeleePursue;
                    }
                case FuzzyBehaviour.MeleeTroll:
                    {
                        next = rand.Next(1, 5);
                        switch (next)
                        {
                            case 1:
                                return BehaviourState.MeleePursue;
                            case 2:
                                return BehaviourState.RangedPursue;
                            case 3:
                                return BehaviourState.RangedCreep;
                            case 4:
                                return BehaviourState.Flee;
                        }
                        break;
                    }
                case FuzzyBehaviour.RangedFight:
                    {
                        next = rand.Next(1, 10);
                        switch (next)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                return BehaviourState.RangedPursue;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                return BehaviourState.RangedCreep;
                            case 9:
                                return BehaviourState.MeleePursue;
                        }
                        break;
                    }
                case FuzzyBehaviour.RangedTroll:
                    {
                        next = rand.Next(1, 6);
                        switch (next)
                        {
                            case 1:
                                return BehaviourState.MeleePursue;
                            case 2:
                                return BehaviourState.MeleeCreep;
                            case 3:
                                return BehaviourState.Wander;
                            case 4:
                                return BehaviourState.Flee;
                            case 5:
                                return BehaviourState.RangedPursue;
                        }
                        break;
                    }
            }
            return BehaviourState.Flee;
        }
        #endregion

        #region Axioms
        /// <summary>
        /// Axiom for AND operation. Assumes inputs are between 0 and 1.
        /// </summary>
        private float And(float A, float B)
        {
            return Math.Min(A, B);
        }

        /// <summary>
        /// Axiom for OR operation. Assumes inputs are between 0 and 1.
        /// </summary>
        private float Or(float A, float B)
        {
            return Math.Max(A, B);
        }
        
        /// <summary>
        /// Axiom for NOT operation. Assumes input is between 0 and 1.
        /// </summary>
        private float Not(float A)
        {
            return 1 - A;
        }
        #endregion
    }
}
