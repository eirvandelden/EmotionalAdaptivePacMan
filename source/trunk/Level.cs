using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAPacMan
{
    public static class Level
    {   //
        //PUBLIC VARIABLES
        //
        private static int level = 0;

        //State based variables
            //Mode of the game {Calibration || Adaptive}
        public enum Mode { Calibration, Adaptive };
        private static Mode levelState;
            //Maximum levels/time that can be played
        private static int calibrationMaxLVL = 7;
        public static int calibrationLVLTime = 20000;
        private static int adaptiveMaxLVL = 21;
            //possible difficulties of the game
        public enum Difficulty { Slowest, Slow, Normal, Fast, Hyper };
        public static Difficulty levelDifficulty = Difficulty.Slowest;
        private static Difficulty previousDifficulty = Difficulty.Hyper;
            //keep track of the speed change direction
        public enum SpeedChangeDir { Harder, Easier, None};                                        //possible directions of speed
        private static SpeedChangeDir speedChangeDir = SpeedChangeDir.Harder;                     //calibration mode starts with making the speed go 
            //UserInput; too hard/ easy and when this input has been given
        public enum UserInput { TooEasy, Good, TooHard, none };
        private static UserInput userInput = UserInput.none;
        private static int userInputTime = -1;
            //Developer Goodness
        private static Boolean DeveloperMode = false;

        //
        //METHODS
        //
            //Get and Set functions for LevelState
        public static Mode GetLevelState()
        {   // pre: <none>
            // post: result = LevelState

            Mode result = Mode.Calibration;                                                           // Init result var

            // Set result = LevelState
            if (levelState == Mode.Adaptive)
            {
                result = Mode.Adaptive;
            }
            else if (levelState == Mode.Calibration)
            {
                result = Mode.Calibration;
            }

            return result;
        }
        public static void SetLevelState(Mode state)
        {   // pre: a State state in {"Calibration", "Adaptive"}
            // post: LevelState = s

            if (state == Mode.Adaptive)
            {
                levelState = Mode.Adaptive;
            }
            else if (state == Mode.Calibration)
            {
                levelState = Mode.Calibration;
            }

        }
            //Get and Set functions for levelDifficulty
        public static Difficulty GetLevelDifficulty()
        {
            return levelDifficulty;
        }
        public static void SetLevelDifficulty(Difficulty d)
        {
            levelDifficulty = d;
        }
        public static void IncreaseLevelDifficulty()
        {
            // possible directions:
            // slowest -> slow -> normal -> fast -> hyper
            // hyper -> fast -> normal -> slow -> slowest
            // outer options always go to their neighbour, inner options switch depending on the previous levelDifficulty state
            switch (levelDifficulty)
            {
                case Difficulty.Slowest:
                    levelDifficulty = Difficulty.Slow;
                    previousDifficulty = Difficulty.Slowest;
                    break;
                case Difficulty.Slow:
                    if (previousDifficulty == Difficulty.Slowest)
                    {
                        levelDifficulty = Difficulty.Normal;
                    }
                    else
                    {
                        levelDifficulty = Difficulty.Slowest;
                    }

                    previousDifficulty = Difficulty.Slow;
                    break;
                
                case Difficulty.Normal:
                    if (previousDifficulty == Difficulty.Slow)
                    {
                        levelDifficulty = Difficulty.Fast;
                    }
                    else
                    {
                        levelDifficulty = Difficulty.Slow;
                    }
                    previousDifficulty = Difficulty.Normal;
                    break;
                case Difficulty.Fast:
                    if (previousDifficulty == Difficulty.Normal)
                    {
                        levelDifficulty = Difficulty.Hyper;
                    }
                    else
                    {
                        levelDifficulty = Difficulty.Normal;
                    }
                    previousDifficulty = Difficulty.Fast;
                    break;
                case Difficulty.Hyper:
                    previousDifficulty = Difficulty.Hyper;
                    levelDifficulty = Difficulty.Slowest;
                    break;
            }
        }
            //Get and Set functions for Level
        public static int GetLevelNumber()
        {   // pre: <>
            // post: result = Level
            return level;
        }
        public static void IncreaseLevelNumber()
        {   //pre: 0 < lvl < Max
            //post: Level = Level+1
            level++;
        }
        public static void SetLevelNumber( int lvl ) 
        {   //pre: 0 < lvl < 256
            //post: Level = l

            if ((0 < lvl) && (lvl < 256))
            {
                level = lvl;
            }
        }
        //Get and Set functions for speedChangeDirection
        public static SpeedChangeDir GetSpeedChangeDir()
        {
            return speedChangeDir;
        }
        public static void SetSpeedChangeDir(UserInput input)
        {
            switch (input)
            {
                case UserInput.TooHard:
                    speedChangeDir = SpeedChangeDir.Easier;
                    break;
                case UserInput.TooEasy:
                    speedChangeDir = SpeedChangeDir.Harder;
                    break;
                case UserInput.Good:
                    if (GetLevelState() == Mode.Adaptive)
                    { speedChangeDir = SpeedChangeDir.None; }
                    break;
                default:
                    speedChangeDir = SpeedChangeDir.Harder;
                    break;
            }
        }

        public static void ChangeGameSpeed(int[] c, int k)
        {
            //The rule for speed adaptation;    
            switch (Constants.adapt_phase)
            {
                case 1:     //Easy
                    if (c[0] < (k * Constants.k_tol))  //k=6: Bij max 4x te makkelijk -> makkelijker
                    { Level.SetSpeedChangeDir(Level.UserInput.TooHard); }
                    else if (c[0] == k)   //k=6: Bij 6x te makkelijker -> iets moeilijker maken
                    { Level.SetSpeedChangeDir(Level.UserInput.TooEasy); }
                    else                        //k=6: Anders is het niveau goed 
                    { Level.SetSpeedChangeDir(Level.UserInput.Good); }
                    break;
                case 2:     //Balanced
                    if (c[1] >= (k / 2))
                    { Level.SetSpeedChangeDir(Level.UserInput.Good); }
                    else if (c[0] < c[2])
                    { Level.SetSpeedChangeDir(Level.UserInput.TooHard); }
                    else if (c[0] > c[2])
                    { Level.SetSpeedChangeDir(Level.UserInput.TooEasy); }
                    else
                    { Level.SetSpeedChangeDir(Level.UserInput.Good); }
                    break;
                case 3:     //Hard
                    if (c[2] < (k * Constants.k_tol))
                    { Level.SetSpeedChangeDir(Level.UserInput.TooEasy); }
                    else if (c[2] == k)
                    { Level.SetSpeedChangeDir(Level.UserInput.TooHard); }
                    else
                    { Level.SetSpeedChangeDir(Level.UserInput.Good); }
                    break;
            }
        }

        //Get/Set functions for UserInput: Get Input/Time. Set UserInput (and auto-time). Reset the timer
        public static UserInput GetUserInput()
        {
            return userInput;
        }
        public static int GetUserInputTime()
        {
            return userInputTime;
        }
        public static void SetUserInput(UserInput input)
        {
            userInputTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
            userInput = input;
        }
        public static void ResetUserInputTime()
        {
            userInputTime = -1;
        }

        //Get and Set functions for DeveloperMode
        public static bool GetDevMode()
        {   //pre:<>
            //result: DeveloperMode
            return DeveloperMode;
        }
        public static void SetDeveloperMode( Boolean mode )
        {   //pre:  Boolean mode
            //post: DevelopMode = mode

            DeveloperMode = mode;
        }
         
   
            //Check for win condition
        public static bool HasWon()
        {   //pre:
            //post: Adaptive => (Adaptive^MaxLvl) || Calibration =>(Calibration ^ MaxLVL)
            bool result = false;

            if (levelState == Mode.Adaptive)
            {
                result = (level == adaptiveMaxLVL);
            }
            else
            {
                result = (level == calibrationMaxLVL);
            }

            return result;
        }
    }
}
