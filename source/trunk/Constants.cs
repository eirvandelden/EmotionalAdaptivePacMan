using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace XNAPacMan {
    /// <summary>
    /// This class provides global access to important game constants; what fruits appear on what levels,
    /// relative speeds of everything, timers, etc. Centralizing this data here makes it easy to change
    /// the game settings. It also relieves the data-heavy ghost and gameloop classes from a LOT of definitions.
    /// </summary>
    static class Constants {
            //Developer goodness
        public static bool debug = false;

        //Define constants for Feedbackloop
        public static int k = 6;                                                        //Define k for kNN
        public static double k_tol = 0.75;                                              //Tolerance for adaptive mode (0.75 => 75% easy requirement in phase 1)

        public static DateTime vidstarted;                                              //Log the start of session time (ie. started watching the video)
        public static bool Soundcheck = false;                                          //Soundcheck for the feedback-requesting sound
        public static readonly object _lockerBuffer = new object();
        public static readonly object _lockerInput = new object();
        public static readonly object _lockerPressure = new object();

        public static bool tmrTick = false;

        public static string username = "Albert";                                       //Name of the user
        public static string adapt_order = "132";                                       //The order in which the adaptive phase will take place
        public static int adapt_phase = -1;                                             //Current adaptation mode phase

        public static string profilepath = "C:\\ProgramData\\Pacman\\Profiles\\";       //Path in which to save the profiles
        public static string cfilename = "Calib.dat";
        public static string afilename = "Adaptive.dat";
        public static string outfile = "";

        public static int feedbackDelay = 2000;                                         //Time to delay after the feedback (too hard/easy) is given
        public static int parseInterval = 5000;                                         //The interval in which the data is reviewed
        public static int tolerance = 50;                                               //The tolerance when searching through the datapoint buffer
        public static int lastSampbleNbr = 1;
        public static int sessionID = -1;
        public static int feedbackcooldown = 1000;                                      //'Cool down' time before new user feedback is evaluated
        public static double lastfeedback = -1;

        //Global constants, needed for the threaded log parser
        public static Level.UserInput userInput = Level.UserInput.none;
        public static int pressure = 0;

        //Define constants for Calibration mode
        public static int CalibTime = 600;                                                //Calibration mode duration: 10 minutes
        
        //Define constants for Adaptive Mode
        public static int AdaptTime = 300;                                                //Adaptive mode phase duration: (3x) 5 minutes

        //Define constants used by sensors
            //Mobi
        public static List<DataPoint> buffer = new List<DataPoint>();
            //DualShock3
        public const string ds3Up = "16 ", ds3Down = "17 ", ds3Left = "18 ", ds3Right = "19 ", ds3Start = "07 ", 
                                ds3Cross = "00 ", ds3Circle = "01 ", ds3Square = "02 ", ds3Triangle = "03 ", ds3L1 = "04 ", ds3R1 = "05 ", ds3L2= "06 ", ds3R2 = "07 " ;
        
        //Define string values of the events used
        public static string[] EventsList = { "Death           ", "Crump eaten     ", "Power Pill      ",
                "Bonus food eaten", "Ghost eaten     ", "Level completed ",
                "Game finished, hiscore table", "Too Easy", "Too Hard", "Good", "Speedchange" };
        public static string[] EvLst = { "Death", "Crump", "P.Pill",
                "Bonusfood", "Ghost_eat", "Level_done","Game_over", 
                "Too Easy", "Too Hard", "Good", "Speedchange" };

        //Workaround for logging of "too easy" and "too hard" events
        public static bool LogEasy = false, LogHard = false, LogGood = false;

        //Prevent multiple instances of 2 forms
        public static frmEvents staticVar = null;
        public static DualShock3 staticJoystick = null;

        // Dispersion tiles for each ghost
        public static readonly List<Point> scatterTilesBlinky =  new List<Point> {   new Point(21, 1),   new Point(26, 1),
                                                                            new Point(26, 5),   new Point(21, 5)    
        };
        public static readonly List<Point> scatterTilesPinky = new List<Point> {   new Point(1, 1),    new Point(6, 1),
                                                                            new Point(6, 5),    new Point(1, 5)     
        };
        public static readonly List<Point> scatterTilesClyde = new List<Point> {   new Point(6, 23),   new Point(9, 23),
                                                                            new Point(9, 26),   new Point(12, 26),  
                                                                            new Point(12, 29),  new Point(1, 29),
                                                                            new Point(1, 26),   new Point(6, 26)    
        };
        public static readonly List<Point> scatterTilesInky = new List<Point> {   new Point(18, 23),  new Point(21, 23),
                                                                            new Point(21, 26),  new Point(26, 26),
                                                                            new Point(26, 29),  new Point(15, 29),
                                                                            new Point(15, 26),  new Point(18, 26)
        };

        public static List<Point> scatterTiles(Ghosts identity) {
            switch (identity) {
                case Ghosts.Blinky:
                    return scatterTilesBlinky;
                case Ghosts.Clyde:
                    return scatterTilesClyde;
                case Ghosts.Inky:
                    return scatterTilesInky;
                case Ghosts.Pinky:
                    return scatterTilesPinky;
                default:
                    throw new ArgumentException();
            }
        }

        public static readonly Position startPositionBlinky = new Position { Tile = new Point(13, 11), DeltaPixel = new Point(8, 0) };
        public static readonly Position startPositionPinky = new Position { Tile = new Point(13, 14), DeltaPixel = new Point(8, 8) };
        public static readonly Position startPositionInky = new Position { Tile = new Point(11, 13), DeltaPixel = new Point(8, 8) };
        public static readonly Position startPositionClyde = new Position { Tile = new Point(15, 13), DeltaPixel = new Point(8, 8) };
        public static Position startPosition(Ghosts identity) {
            switch (identity) {
                case Ghosts.Blinky:
                    return startPositionBlinky;
                case Ghosts.Pinky:
                    return startPositionPinky;
                case Ghosts.Clyde:
                    return startPositionClyde;
                case Ghosts.Inky:
                    return startPositionInky;
                default:
                    throw new ArgumentException();

            }
        }

        public static int InitialJumps(Ghosts ghost, bool newLevel) {
            if (newLevel) {
                switch (ghost) {
                    case Ghosts.Inky:
                        return (int)MathHelper.Clamp((20 - Level.GetLevelNumber()) / 2, 0, 10);
                    case Ghosts.Clyde:
                        return InitialJumps(Ghosts.Inky, true) + 2;
                    default:
                        return 0;
                }
            }
            else {
                switch (ghost) {
                    case Ghosts.Inky:
                        return 1;
                    case Ghosts.Clyde:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        private static int[] cruiseElroyTimers_ = { 20, 30, 40, 40, 40, 50, 50, 50, 60, 60 };
        public static int CruiseElroyTimer() {
            if (Level.GetLevelNumber() >= 10) {
                return cruiseElroyTimers_[9];
            }
            else {
                return cruiseElroyTimers_[Level.GetLevelNumber() - 1];
            }
        }

        public static Color colors(Ghosts identity) {
            switch (identity) {
                case Ghosts.Blinky:
                    return Color.Red;
                case Ghosts.Clyde:
                    return Color.Orange;
                case Ghosts.Inky:
                    return Color.LightSkyBlue;
                case Ghosts.Pinky:
                    return Color.LightPink;
                default:
                    throw new ArgumentException();
            }
        }

        private static int[] blueTimes_ = { 6, 6, 4, 3, 2, 6, 2, 2, 1, 5, 2, 1, 1, 3, 1, 1, 0, 1, 0, 0, 0 };
        public static int BlueTime() {
            return Level.GetLevelNumber() > blueTimes_.Length - 2 ? 0 : blueTimes_[Level.GetLevelNumber() - 1];
        }

        private static int[] bonusScores_ = { 100, 300, 500, 700, 700, 1000, 1000, 2000, 2000, 3000, 3000, 5000, 5000, 5000 };
        public static int BonusScores() {
            return Level.GetLevelNumber() > bonusScores_.Length - 2 ? 5000 : bonusScores_[Level.GetLevelNumber() - 1];
        }

        private static string[] bonusSprites_ = { "Cherry", "Strawberry", "Apple", "Bell", "Orange", "Pear", "Pretzel", "Bell", "Banana", "Key", "Key" };
        public static string BonusSprite() {
            return Level.GetLevelNumber() > bonusSprites_.Length - 2 ? "Key" : bonusSprites_[Level.GetLevelNumber() - 1];
        }

        private static int[] pacManSpeed_ = { 6, 6, 6, 6, 6 }; //originially: (7,9,8,8,9) Appears to go from right to left
        public static int PacManSpeed() {
            if (5 <= Level.GetLevelNumber() && Level.GetLevelNumber() <= 20) {
                return pacManSpeed_[4];
            }
            else if (5 > Level.GetLevelNumber()) {
                return pacManSpeed_[Level.GetLevelNumber() - 1];
            }
            else {
                return 10;
            }
        }
    }
}
