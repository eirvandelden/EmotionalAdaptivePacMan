using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;


namespace XNAPacMan {

    /// <summary>
    /// Defines a position on the board where a ghost has died or a fruit was eaten, as well as the score earned.
    /// This is used for knowing where to draw those scores
    /// </summary>
    struct ScoreEvent {
        public ScoreEvent(Position position, DateTime when, int score) {
            Position = position;
            When = when;
            Score = score;
        }
        public Position Position;
        public DateTime When;
        public int Score;
    }
    /// <summary>
    /// GameLoop is the main "game" object; this is basically where the action
    /// takes place. It's responsible for coordinating broad game logic,
    /// drawing the board and scores, as well as linking with the menu.
    /// </summary>
    public class GameLoop : Microsoft.Xna.Framework.DrawableGameComponent {
        //Define Event thingies ??
        static string[] s = Constants.EventsList;
        frmEvents.Event Death      = new frmEvents.Event(s[0]);
        frmEvents.Event Eat_Crump  = new frmEvents.Event(s[1]);
        frmEvents.Event Power_Pill = new frmEvents.Event(s[2]);
        frmEvents.Event Bonus_Food = new frmEvents.Event(s[3]);
        frmEvents.Event Kill_Ghost = new frmEvents.Event(s[4]);
        frmEvents.Event Level_Done = new frmEvents.Event(s[5]);
        frmEvents.Event Game_Over  = new frmEvents.Event(s[6]);
        frmEvents.Event TooEasy    = new frmEvents.Event(s[7]);
        frmEvents.Event TooHard    = new frmEvents.Event(s[8]);
        frmEvents.Event Good       = new frmEvents.Event(s[9]);
        frmEvents.Event SpeedChange= new frmEvents.Event(s[10]);
        //Create single instances of Eventlogger/Dualshock window
        frmEvents fe = new frmEvents().staticVar;                                                 //Init single EventLogger Instance
        DualShock3 ds3 = new DualShock3().staticJoystick;                                         //Init Single DualShock3 Instance
        Thread thread;
        bool forcequit = false;                                                                   //Quits the game when a mode has ended
        int feedback_count = 0;

        List<DataPoint> calibdata = new List<DataPoint>();                                        //List to store points from mobi into
        List<double> feedbacktimer = new List<double>();
        public System.Windows.Forms.CheckBox[] myCheckBoxArray;// = new System.Windows.Forms.CheckBox[14];

        
        public GameLoop(Game game, Level.Mode state, frmEvents fe)
            : base(game)
        {
            //Show and initiate the Events Form
            fe.Show();
            fe.DesktopLocation = new System.Drawing.Point(0, 0);
            fe.TimerInit();

            // Read all settings (ie name of a participant). If no settings file can be found, make one.
            const string fileName = "settings.txt";
            string infopath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            List<string> settings;
            settings = new List<string>();
            string full_filename = Constants.profilepath + fileName;
            if (File.Exists(full_filename))
            {
                settings = File.ReadAllLines(full_filename).ToList<string>();
                Constants.username = settings[0];
                Constants.adapt_order = settings[1];
            }
            else
            {
                File.WriteAllLines(full_filename, new string[] { "Albert", "132" });
            }

            fe.OpenFile();
            // TODO: Construct any child components here

            //Set up level state, default to Calibration mode, but set to Adaptive when given
            if (state == Level.Mode.Adaptive)
            {
                calibdata = fe.ReadCalibData(Constants.cfilename);
                List<DataPoint> adaptpoints = new List<DataPoint>();
                adaptpoints = fe.ReadCalibData(Constants.afilename);
                try
                {
                    foreach (DataPoint d in adaptpoints)
                    { calibdata.Add(d); }
                    Console.WriteLine("Total datapoints loaded into memory: " + calibdata.Count);
                }
                catch (Exception)
                {
                    //No adaptation points available
                }
                // Get the adaptation phase number.
                GetAdaptPhase(Constants.adapt_order);
                
                Level.SetLevelState(state);

                // Open the log file for adaptation mode
                fe.OpenCalibFile(Constants.afilename);
            }
            else
            {
                Level.SetLevelState(state);
                fe.OpenCalibFile(Constants.cfilename); 
            }

        }

        bool GetAdaptPhase(string s)
        {
            if (s.Length > 0)
            {
                // Return the next adaptation phase
                try
                {
                    int next = Convert.ToInt16(s.Substring(0, 1));
                    Constants.adapt_order = s.Substring(1, s.Length - 1);
                    Console.WriteLine("Current adaptation phase: " + next + ", " + s + "->" + Constants.adapt_order);
                    Constants.adapt_phase = next;
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid adaptation order string: " + s);
                    return false;
                }
            }
            else
            {
                // Adaptation phases completed
                return false;
            }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize() {
            // We don't want XNA calling this method each time we resume from the menu,
            // unfortunately, it'll call it whatever we try. So the only thing
            // we can do is check if it has been called already and return. Yes, it's ugly.
            if (spriteBatch_ != null) {
                GhostSoundsManager.ResumeLoops();
                return;
            }
            // Otherwise, this is the first time this component is Initialized, so proceed.
            GhostSoundsManager.Init(Game);

            //Set DeveloperMode; this gives invinite lives and extra debug information
            Level.SetDeveloperMode(false);

            //Initialise thread for parsing the mobi log
            thread = new Thread( new ThreadStart(FeedbackLoop.ReadParseMobiLog) );
            
            Grid.Reset();
            Level.SetLevelNumber(1);
            spriteBatch_ = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            graphics_ = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));
            soundBank_ = (SoundBank)Game.Services.GetService(typeof(SoundBank));

            scoreFont_ = Game.Content.Load<SpriteFont>("Score");
            scoreEventFont_ = Game.Content.Load<SpriteFont>("ScoreEvent");
            xlife_ = Game.Content.Load<Texture2D>("sprites/ExtraLife");
            ppill_ = Game.Content.Load<Texture2D>("sprites/PowerPill");
            crump_ = Game.Content.Load<Texture2D>("sprites/Crump");
            board_ = Game.Content.Load<Texture2D>("sprites/Board");
            boardFlash_ = Game.Content.Load<Texture2D>("sprites/BoardFlash");
            bonusEaten_ = new Dictionary<string, int>();
            bonus_ = new Dictionary<string, Texture2D>(9);
            bonus_.Add("Apple", Game.Content.Load<Texture2D>("bonus/Apple"));
            bonus_.Add("Banana", Game.Content.Load<Texture2D>("bonus/Banana"));
            bonus_.Add("Bell", Game.Content.Load<Texture2D>("bonus/Bell"));
            bonus_.Add("Cherry", Game.Content.Load<Texture2D>("bonus/Cherry"));
            bonus_.Add("Key", Game.Content.Load<Texture2D>("bonus/Key"));
            bonus_.Add("Orange", Game.Content.Load<Texture2D>("bonus/Orange"));
            bonus_.Add("Pear", Game.Content.Load<Texture2D>("bonus/Pear"));
            bonus_.Add("Pretzel", Game.Content.Load<Texture2D>("bonus/Pretzel"));
            bonus_.Add("Strawberry", Game.Content.Load<Texture2D>("bonus/Strawberry"));

            scoreEvents_ = new List<ScoreEvent>(5);
            bonusPresent_ = false;
            bonusSpawned_ = 0;
            eatenGhosts_ = 0;
            Score = 0;
            xlives_ = 2;
            paChomp_ = true;
            playerDied_ = false;
            player_ = new Player(Game);
            ghosts_ = new List<Ghost> { new Ghost(Game, player_, Ghosts.Blinky), new Ghost(Game, player_, Ghosts.Clyde),
                                        new Ghost(Game, player_, Ghosts.Inky), new Ghost(Game, player_, Ghosts.Pinky)};
            ghosts_[2].SetBlinky(ghosts_[0]); // Oh, dirty hack. Inky needs this for his AI.
            soundBank_.PlayCue("Intro");
            if (Level.GetDevMode())
            {
                LockTimer = TimeSpan.FromMilliseconds(500);
            }
            else
            {
                LockTimer = TimeSpan.FromMilliseconds(4500);
            }
            
            //Init time
            sessionStartTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
            sessionTime = 0;
            lastSpeedChange = sessionTime;
            lastAdaptCheck = sessionTime;

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime) {
            // Copy TotalRealTime to Time; Copy Current Time in Milliseconds to sessionTime
            gameTimer = gameTime.TotalGameTime;
            currentTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
            sessionTime = currentTime - sessionStartTime;


            // Some events (death, new level, etc.) lock the game for a few moments.
            if (DateTime.Now - eventTimer_ < LockTimer) {
                ghosts_.ForEach(i => i.LockTimer(gameTime));
                // Check just for user input
                player_.UpdateFeedback(gameTime, fe);
                // Also we need to do the same thing for our own timer concerning bonuses
                bonusSpawnedTime_ += gameTime.ElapsedGameTime;
                return;
            }

            
            //If there has been user input: switch speedchangedirection, else check timer to see if new speed needs to be changed.
            if (Level.GetLevelState() == Level.Mode.Calibration)
            {
                //If the time is over, quit calib mode
                if (sessionTime >= Constants.CalibTime)
                { Console.WriteLine("Calibration mode finished"); forcequit = true; }

                switch (Level.GetUserInput())
                {
                    case Level.UserInput.TooEasy:
                    case Level.UserInput.TooHard:
                        if (currentTime - Level.GetUserInputTime() > 2)
                        {
                            Level.SetSpeedChangeDir(Level.GetUserInput());
                            Level.SetUserInput(Level.UserInput.none);
                            Level.ResetUserInputTime();
                        }
                        break;
                    case Level.UserInput.Good:

                    default:
                        break;
                }
            }

            //GetUserInput, needed later during the log parser
            lock(Constants._lockerInput)
            {
                Constants.userInput = Level.GetUserInput();
            }
            

            //Check calibration information to determine whether the speed should change
            if (Level.GetLevelState() == Level.Mode.Adaptive)
            {
                if (((sessionTime % 10) == 0) && ((currentTime - lastAdaptCheck) >= 5))
                {
                    // Make sure the data is reviewed after every adaptive loop cycle
                    feedbacktimer.Add(DateTime.Now.TimeOfDay.TotalMilliseconds); feedbacktimer.Add(4);
                    switch (Level.GetSpeedChangeDir())
                    {
                        case Level.SpeedChangeDir.Harder:
                            Ghost.IncreaseGhostSpeed();
                            SpeedChange.AddEvent(fe, Ghost.GetGhostSpeed());
                            lastSpeedChange = sessionTime;
                            break;
                        case Level.SpeedChangeDir.Easier:
                            Ghost.DecreaseGhostSpeed();
                            SpeedChange.AddEvent(fe, Ghost.GetGhostSpeed());
                            lastSpeedChange = sessionTime;
                            break;
                        default:
                            break;
                    }
                    lastAdaptCheck = currentTime;
                }

                //Check for a finished adaptation phase
                if (sessionTime >= Constants.AdaptTime)
                {
                    if (GetAdaptPhase(Constants.adapt_order))
                    {
                        //Restart adaptation mode if there are phases left
                        bonusSpawned_ = 0;
                        Grid.Reset();

                        fe.OpenCalibFile(Constants.afilename);
                        player_ = new Player(Game);
                        ghosts_.ForEach(i => i.Reset(true, player_));
                        //LockTimer = TimeSpan.FromSeconds(2);

                        sessionStartTime = currentTime;
                        //soundBank_.PlayCue("NewLevel");
                        return;
                    }
                    else
                    { Console.WriteLine("Adaptation mode finished"); forcequit = true; }
                }
            }


            // Log the events of user input "too hard" or "too easy"

            #region old_feedback_handling
            // WasTooHard is used to determine whether the previous event is reocurring
            // to avoid adding events while the buttons are held
            /*if (Constants.LogEasy)
            {
                if (WasTooHard)
                {
                    TooEasy.AddEvent(fe, Ghost.GetGhostSpeed()); WasTooHard = false; WasGood = false;
                    feedbacktimer.Add(DateTime.Now.TimeOfDay.TotalMilliseconds); feedbacktimer.Add(0);
                }
            }
            if (Constants.LogHard)
            {
                if (!WasTooHard)
                {
                    TooHard.AddEvent(fe, Ghost.GetGhostSpeed()); WasTooHard = true; WasGood = false;
                    feedbacktimer.Add(DateTime.Now.TimeOfDay.TotalMilliseconds); feedbacktimer.Add(2);
                }
            }
            if (Constants.LogGood)
            {
                if (!WasGood)
                {
                    Good.AddEvent(fe, Ghost.GetGhostSpeed()); WasGood = true;
                    feedbacktimer.Add(DateTime.Now.TimeOfDay.TotalMilliseconds); feedbacktimer.Add(1);
                }
            }*/
            
            #endregion

            double timenow = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (Constants.lastfeedback == -1 || timenow > Constants.lastfeedback + Constants.feedbackcooldown)
            {
                // Check whether a feedback button is pressed. If so, update and reset the last feedback time.
                if (Constants.LogEasy)
                {
                        TooEasy.AddEvent(fe, Ghost.GetGhostSpeed()); //WasTooHard = false; WasGood = false;
                        feedbacktimer.Add(timenow); feedbacktimer.Add(0);
                        Constants.lastfeedback = timenow;
                }
                if (Constants.LogGood)
                {
                        Good.AddEvent(fe, Ghost.GetGhostSpeed()); //WasGood = true;
                        feedbacktimer.Add(timenow); feedbacktimer.Add(1);
                        Constants.lastfeedback = timenow;
                }
                if (Constants.LogHard)
                {
                        TooHard.AddEvent(fe, Ghost.GetGhostSpeed()); //WasTooHard = true; WasGood = false;
                        feedbacktimer.Add(timenow); feedbacktimer.Add(2);
                        Constants.lastfeedback = timenow;
                }
                // When no feedback is given, continue.
            }
            // Do not remember the status for future use
            Constants.LogEasy = false;
            Constants.LogHard = false;
            Constants.LogGood = false;

            // Remove special events older than 5 seconds
            scoreEvents_.RemoveAll(i => DateTime.Now - i.When > TimeSpan.FromSeconds(5));

            // If the player had died, spawn a new one or end game.
            if (playerDied_) {
                // extra lives are decremented here, at the same time the pac man is spawned; this makes those 
                // events seem linked.
                xlives_--;
               // if (Level.GetDevMode())
                {
                    xlives_++; // Give infinite lives to the evil developer;                    
                }

                if (xlives_ >= 0) {
                    playerDied_ = false;
                    player_ = new Player(Game);
                    ghosts_.ForEach(i => i.Reset(false, player_));
                    scoreEvents_.Clear();
                }
                else { // The game is over

                    Game_Over.AddEvent(fe);
                    Menu.SaveHighScore(Score);

                    Game.Components.Add(new Menu(Game, null));
                    Game.Components.Remove(this);
                    GhostSoundsManager.StopLoops();
                    return;
                }
            }

#region Oude code voor calibration mode
            //We are in Calibration mode; stop the level after the timer has ended
            /* if (Level.GetLevelState() == Level.States.Calibration)
            {
                if ((currentTime - sessionStartTime) >= Constants.playTime)                            //If X has passed, with X in milliseconds 
                {
                    if (Level.HasWon())
                    { // Game over, you win.
                        Game_Over.AddEvent(fe);
                        Menu.SaveHighScore(Score);
                        Game.Components.Add(new Menu(Game, null));
                        Game.Components.Remove(this);
                        GhostSoundsManager.StopLoops();
                        return;
                    }
                    else
                    { //Go to next level
                        bonusSpawned_ = 0;
                        // Level Done Event
                        Level.IncreaseLevelDifficulty();
                        Level_Done.AddEvent(fe);
                        Grid.Reset();
                        sessionStartTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);    //Reset session timer

                        //filter on this part for a continuos level
                        player_ = new Player(Game);
                        ghosts_.ForEach(i => i.Reset(true, player_));
                        LockTimer = TimeSpan.FromSeconds(2);

                        soundBank_.PlayCue("NewLevel");
                        Level.IncreaseLevelNumber();
                        return;
                    }

                }
            }*/
#endregion

            if (forcequit)
            {
                Game_Over.AddEvent(fe);
                Menu.SaveHighScore(Score);

                Game.Components.Add(new Menu(Game, null));
                Game.Components.Remove(this);
                GhostSoundsManager.StopLoops();
                return;
            }
            // When all crumps have been eaten, wait a few seconds and then spawn a new level
            if (noCrumpsLeft()) {
                if (Level.HasWon())
                { // Game over, you win.
                    Game_Over.AddEvent(fe);
                    Menu.SaveHighScore(Score);

                    Game.Components.Add(new Menu(Game, null));
                    Game.Components.Remove(this);
                    GhostSoundsManager.StopLoops();
                    return;
                }
                else
                { //Go to next level
                    bonusSpawned_ = 0;
                    // Level Done Event
                    Level_Done.AddEvent(fe);
                    Grid.Reset();

                    //filter on this part for a continuos level
                    player_ = new Player(Game);
                    ghosts_.ForEach(i => i.Reset(true, player_));
                    LockTimer = TimeSpan.FromSeconds(2);

                    soundBank_.PlayCue("NewLevel");
                    Level.IncreaseLevelNumber();
                    return;
                }

            }

            Keys[] inputKeys = Keyboard.GetState().GetPressedKeys();
            // The user may escape to the main menu with the escape key
            if (inputKeys.Contains(Keys.Escape) || ds3.GetButton() == Constants.ds3Start) {

                Game.Components.Add(new Menu(Game, this));
                Game.Components.Remove(this);
                GhostSoundsManager.PauseLoops(); // will be resumed in Initialize(). No need for stopping them
                // if the player subsequently quits the game, since we'll re-initialize GhostSoundManager in
                // Initialize() if the player wants to start a new game.
                return;
            }

            // Eat crumps and power pills.
            if (player_.Position.DeltaPixel == Point.Zero) {
                Point playerTile = player_.Position.Tile;
                if (Grid.TileGrid[playerTile.X, playerTile.Y].HasCrump) {
                    soundBank_.PlayCue(paChomp_ ? "PacMAnEat1" : "PacManEat2");
                    paChomp_ = !paChomp_;
                    Score += 10;
                    // Crump Event
                    Eat_Crump.AddEvent(fe);
                    Grid.TileGrid[playerTile.X, playerTile.Y].HasCrump = false;
                    if (Grid.TileGrid[playerTile.X, playerTile.Y].HasPowerPill) {
                        Score += 40;
                        // Power Pill Event
                        Power_Pill.AddEvent(fe);
                        eatenGhosts_ = 0;
                        for (int i = 0; i < ghosts_.Count; i++) {
                            if (ghosts_[i].State == GhostState.Attack || ghosts_[i].State == GhostState.Scatter ||
                                ghosts_[i].State == GhostState.Blue) {
                                ghosts_[i].State = GhostState.Blue;
                            }
                        }
                        Grid.TileGrid[playerTile.X, playerTile.Y].HasPowerPill = false;
                    }

                    // If that was the last crump, lock the game for a while
                    if (noCrumpsLeft()) {
                        GhostSoundsManager.StopLoops();
                        
                        //only flash the screen when in adaptive mode
                        if (Level.GetLevelState() == Level.Mode.Adaptive)
                        {
                            LockTimer = TimeSpan.FromSeconds(2);
                        }
                        return;
                    }
                }
            }

            // Eat bonuses
            if (bonusPresent_ && player_.Position.Tile.Y == 17 &&
                ((player_.Position.Tile.X == 13 && player_.Position.DeltaPixel.X == 8) ||
                  (player_.Position.Tile.X == 14 && player_.Position.DeltaPixel.X == -8))) {
                LockTimer = TimeSpan.FromSeconds(1.5);
                Score += Constants.BonusScores();
                // Bonus food Event
                Bonus_Food.AddEvent(fe);
                scoreEvents_.Add(new ScoreEvent(player_.Position, DateTime.Now, Constants.BonusScores()));
                soundBank_.PlayCue("fruiteat");
                bonusPresent_ = false;
                if (bonusEaten_.ContainsKey(Constants.BonusSprite())) {
                    bonusEaten_[Constants.BonusSprite()]++;
                }
                else {
                    bonusEaten_.Add(Constants.BonusSprite(), 1);
                }
            }

            // Remove bonus if time's up
            if (bonusPresent_ && ((DateTime.Now - bonusSpawnedTime_) > TimeSpan.FromSeconds(10))) {
                bonusPresent_ = false;
            }

            // Detect collision between ghosts and the player
            foreach (Ghost ghost in ghosts_) {
                Rectangle playerArea = new Rectangle((player_.Position.Tile.X * 16) + player_.Position.DeltaPixel.X,
                                                     (player_.Position.Tile.Y * 16) + player_.Position.DeltaPixel.Y,
                                                      26,
                                                      26);
                Rectangle ghostArea = new Rectangle((ghost.Position.Tile.X * 16) + ghost.Position.DeltaPixel.X,
                                                    (ghost.Position.Tile.Y * 16) + ghost.Position.DeltaPixel.Y,
                                                    22,
                                                    22);
                if (!Rectangle.Intersect(playerArea, ghostArea).IsEmpty) {
                    // If collision detected, either kill the ghost or kill the pac man, depending on state.

                    if (ghost.State == GhostState.Blue) {
                        GhostSoundsManager.StopLoops();
                        soundBank_.PlayCue("EatGhost");
                        ghost.State = GhostState.Dead;
                        eatenGhosts_++;
                        int bonus = (int)(100 * Math.Pow(2, eatenGhosts_));
                        Score += bonus;
                        // Killed Ghost Event
                        Kill_Ghost.AddEvent(fe);
                        scoreEvents_.Add(new ScoreEvent(ghost.Position, DateTime.Now, bonus));
                        LockTimer = TimeSpan.FromMilliseconds(900);
                        return;
                    }
                    else if (ghost.State != GhostState.Dead ) {
                        // Death Event
                        Death.AddEvent(fe);
                        KillPacMan();
                        return;
                    }
                    // Otherwise ( = the ghost is dead), don't do anything special.
                }
            }

            // Periodically spawn a fruit, when the player isn't on the spawn location
            // otherwise we get an infinite fruit spawning bug
            if ((Grid.NumCrumps == 180 || Grid.NumCrumps == 80) && bonusSpawned_ < 2 &&
                ! (player_.Position.Tile.Y == 17 &&
                    ((player_.Position.Tile.X == 13 && player_.Position.DeltaPixel.X == 8) ||
                    (player_.Position.Tile.X == 14 && player_.Position.DeltaPixel.X == -8)))) {
                bonusPresent_ = true;
                bonusSpawned_++;
                bonusSpawnedTime_ = DateTime.Now;

            }


            // Now is the time to move player based on inputs and ghosts based on AI
            // If we have returned earlier in the method, they stay in place
            int pressure;
            pressure = player_.Update(gameTime, fe);

            lock(Constants._lockerPressure)
            {
                Constants.pressure = pressure;
            }
            
            
            ghosts_.ForEach(i => i.Update(gameTime));

            DataPoint point = new DataPoint();

            Constants.buffer.Add(FeedbackLoop.Parse(pressure));

            //Increase GhostSpeed if it hasn't been updated yet (ONLY DURING CALIBRATION)
            if (Level.GetLevelState() == Level.Mode.Calibration)
            {
                if (((sessionTime % 10) == 0) && ((sessionTime - lastSpeedChange) >= 5))
                {
                    //Start a new thread (or skip when an old one is still working) that reads the Mobi Log, then creates a datapoint based on the log
                    /*switch (thread.ThreadState)
                    {
                        case ThreadState.Stopped:
                            thread = new Thread(new ThreadStart(FeedbackLoop.ReadParseMobiLog));
                            break;
                        case ThreadState.Unstarted:
                            thread.Start();
                            break;
                        case ThreadState.AbortRequested:
                        case ThreadState.Aborted:
                        case ThreadState.Background:
                        case ThreadState.Running:
                        case ThreadState.StopRequested:
                        case ThreadState.SuspendRequested:
                        case ThreadState.Suspended:
                        case ThreadState.WaitSleepJoin:
                            break;
                        default:
                            break;
                    }*/

                    //Now, change the speed according to the user input

                    switch (Level.GetSpeedChangeDir())
                    {
                        case Level.SpeedChangeDir.Harder:
                            Ghost.IncreaseGhostSpeed();
                            SpeedChange.AddEvent(fe, Ghost.GetGhostSpeed());
                            lastSpeedChange = sessionTime;
                            break;
                        case Level.SpeedChangeDir.Easier:
                            Ghost.DecreaseGhostSpeed();
                            SpeedChange.AddEvent(fe, Ghost.GetGhostSpeed());
                            lastSpeedChange = sessionTime;
                            break;
                        case Level.SpeedChangeDir.None:
                        default:
                            break;
                    }
                    // <10: 2 seconds, <5: 1 second wait
                    if (feedback_count < 5)
                    {
                        LockTimer = TimeSpan.FromSeconds(1);
                    }
                    LockTimer = TimeSpan.FromSeconds(1);
                    feedback_count++;
                    soundBank_.PlayCue("ExtraLife");
                    ///END Switch
                }//END IF timer
            }//END IF Calibration
            else
            {
                if ((sessionTime > 0) && ((sessionTime % 10) == 0))
                {
                    //hulps vars

                    //Start a new thread (or skip when an old one is still working) that reads the Mobi Log, then creates a datapoint, with the new category
                    /*switch (thread.ThreadState)
                    {
                        case ThreadState.Stopped:
                            thread = new Thread(new ThreadStart(FeedbackLoop.ReadParseMobiLog));
                            break;
                        case ThreadState.Unstarted:
                            thread.Start();
                            break;
                        case ThreadState.AbortRequested:
                        case ThreadState.Aborted:
                        case ThreadState.Background:
                        case ThreadState.Running:
                        case ThreadState.StopRequested:
                        case ThreadState.SuspendRequested:
                        case ThreadState.Suspended:
                        case ThreadState.WaitSleepJoin:
                            break;
                        default:
                            break;
                    }*/
                    // inspect the category of the last item in buffer
                    List<DataPoint> tempBuffer = null;
                    lock (Constants._lockerBuffer)
                    {
                        tempBuffer = Constants.buffer;
                    }

                    for (int i = tempBuffer.Count; i < 1; i--)
                    {
                        if (tempBuffer[i].EMG_1 == 1)
                        {   //Determine category of last kNN-created point
                            switch (tempBuffer[i].Category)
                            {
                                case DataPoint.Categories.Easy:
                                    break;
                                case DataPoint.Categories.Good:
                                    break;
                                case DataPoint.Categories.Hard:
                                    break;
                                case DataPoint.Categories.invalid:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            }

          
            


            //Check whether the timer is set for feedback writing. If so, write the calibration file
            if (feedbacktimer.Count > 0 && Constants.buffer.Count > 0)
            {
                //Now check whether the feedback delay has elapsed
                //Only check for the first item in the feedbacktimer, as this will always be the earliest (FIFO)
                if (feedbacktimer[0] + Constants.feedbackDelay <= DateTime.Now.TimeOfDay.TotalMilliseconds)
                {
                    lock(Constants._lockerBuffer)
                    {
                        int buflen = Constants.buffer.Count;
                        int startindex = fe.getLowIndex(buflen, Constants.buffer);
                        int ID = (int)feedbacktimer[1];
                        DataPoint temp = new DataPoint();
                        temp = fe.WriteCalibPoints(startindex, buflen, Constants.buffer, ID);
                        if (ID == 4)
                        {
                            // Game speed change during adaptation mode:
                            // int[] KNN_out = [Easy, Good, Hard]
                            FeedbackLoop.KNearestNeighbour(calibdata, temp);
                        }
                        else if (Level.GetLevelState() == Level.Mode.Adaptive)
                        {
                            // Add the user feedback to the reference list during adaptation gameplay
                            // This makes the game adapt to new user feedback in real-time.

                            // Not fair for our experiment
                            //calibdata.Add(temp);
                        }
                        //Delete the points from the feedbacktimer list after this task. Also clear
                        //the relevant buffer points because they are useless by now.
                        feedbacktimer.RemoveRange(0, 2);
                        Constants.buffer.RemoveRange(0, startindex);
                    
                    }
                    
                    
                }
            }

            if (Level.GetDevMode() && Constants.tmrTick)
            {
                fe.UpdateLabels(player_.Position.Tile.X, player_.Position.Tile.Y, Score);
                int[] count = new int[] {Death.Count, Eat_Crump.Count, Power_Pill.Count,
                                  Bonus_Food.Count,Kill_Ghost.Count,Level_Done.Count,Game_Over.Count};
                fe.UpdateLabels(count);
                Constants.tmrTick = false;
                //fe.PrintTime();
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Nice to have for debug purposes. We might want the level to end early.
        /// </summary>
        /// <returns>Whether there are no crumps left on the board.</returns>
        bool noCrumpsLeft() {
            return Grid.NumCrumps == 0;
        }


        /// <summary>
        /// AAAARRRGH
        /// </summary>
        void KillPacMan() {
            player_.State = State.Dying;
            GhostSoundsManager.StopLoops();
            soundBank_.PlayCue("Death");
            LockTimer = TimeSpan.FromMilliseconds(1811);
            playerDied_ = true;
            bonusPresent_ = false;
            bonusSpawned_ = 0;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            // The GameLoop is a main component, so it is responsible for initializing the sprite batch each frame
            spriteBatch_.Begin();

            Vector2 boardPosition = new Vector2(
                (graphics_.PreferredBackBufferWidth / 2) - (board_.Width / 2),
                (graphics_.PreferredBackBufferHeight / 2) - (board_.Height / 2)
                );

            // When all crumps have been eaten, flash until new level is spawned
            if (noCrumpsLeft())
            {
                spriteBatch_.Draw(((DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) / 350) % 2 == 0 ? board_ : boardFlash_, boardPosition, Color.White);
                player_.Draw(gameTime, boardPosition);
                spriteBatch_.End();
                return;
            }

            // Draw the player and nothing else, just end the spritebatch and return.
            if (player_.State == State.Win && (Level.GetLevelState() == Level.Mode.Adaptive))
            {
                spriteBatch_.Draw(((DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) / 350) % 2 == 0 ? board_ : boardFlash_, boardPosition, Color.White);
                player_.Draw(gameTime, boardPosition);
                spriteBatch_.End();
                return;
            }
            // Otherwise...

            // Draw the board
            spriteBatch_.Draw(board_, boardPosition, Color.White);

            // Draw crumps and power pills
            Tile[,] tiles = Grid.TileGrid;
            for (int j = 0; j < Grid.Height; j++) {
                for (int i = 0; i < Grid.Width; i++) {
                    if (tiles[i, j].HasPowerPill) {
                        spriteBatch_.Draw(ppill_, new Vector2(
                            boardPosition.X + 3 + (i * 16),
                            boardPosition.Y + 3 + (j * 16)),
                            Color.White);
                    }
                    else if (tiles[i, j].HasCrump) {
                        spriteBatch_.Draw(crump_, new Vector2(
                            boardPosition.X + 5 + (i * 16),
                            boardPosition.Y + 5 + (j * 16)),
                            Color.White);
                    }
                }
            }

            // Draw extra lives; no more than 20 though
            for (int i = 0; i < xlives_ && i < 20; i++) {
                spriteBatch_.Draw(xlife_, new Vector2(boardPosition.X + 10 + (20 * i), board_.Height + boardPosition.Y + 10), Color.White);
            }

            // Draw current score
            spriteBatch_.DrawString(scoreFont_, "SCORE", new Vector2(boardPosition.X + 30, boardPosition.Y - 50), Color.White);
            spriteBatch_.DrawString(scoreFont_, Score.ToString(), new Vector2(boardPosition.X + 30, boardPosition.Y - 30), Color.White);

            // Draw current level
            spriteBatch_.DrawString(scoreFont_, "LEVEL", new Vector2(boardPosition.X + board_.Width - 80, boardPosition.Y - 50), Color.White);
            spriteBatch_.DrawString(scoreFont_, Level.GetLevelNumber().ToString(), new Vector2(boardPosition.X + board_.Width - 80, boardPosition.Y - 30), Color.White);

            // Draw a bonus fruit if any
            if (bonusPresent_) {
                spriteBatch_.Draw(bonus_[Constants.BonusSprite()], new Vector2(boardPosition.X + (13 * 16) + 2, boardPosition.Y + (17 * 16) - 8), Color.White);
            }

            // Draw captured bonus fruits at the bottom of the screen
            int k = 0;
            foreach (KeyValuePair<string, int> kvp in bonusEaten_) {
                for (int i = 0; i < kvp.Value; i++) {
                    spriteBatch_.Draw(bonus_[kvp.Key], new Vector2(boardPosition.X + 10 + (22 * (k + i)), board_.Height + boardPosition.Y + 22), Color.White);
                }
                k += kvp.Value; 
            }

            // Draw ghosts
            ghosts_.ForEach( i => i.Draw(gameTime, boardPosition));

            // Draw player
            player_.Draw(gameTime, boardPosition);

            // Draw special scores (as when a ghost or fruit has been eaten)
            foreach (ScoreEvent se in scoreEvents_) {
                spriteBatch_.DrawString(scoreEventFont_, se.Score.ToString(), new Vector2(boardPosition.X + (se.Position.Tile.X * 16) + se.Position.DeltaPixel.X + 4,
                                                                                           boardPosition.Y + (se.Position.Tile.Y * 16) + se.Position.DeltaPixel.Y + 4), Color.White);            
            }

            // Draw GET READY ! at level start
            if (player_.State == State.Start) {
                spriteBatch_.DrawString(scoreFont_, "GET READY!", new Vector2(boardPosition.X + (board_.Width / 2) - 58, boardPosition.Y + 273), Color.Yellow);
            }


            // Draw GET READY ! at level start
            if (player_.State == State.Win)
            {
                spriteBatch_.DrawString(scoreFont_, "Finished!", new Vector2(boardPosition.X + (board_.Width / 2) - 58, boardPosition.Y + 273), Color.Yellow);
            }

            

            // Display number of crumps (for debug)
            if (Level.GetDevMode())
            {
            
                //spriteBatch_.DrawString(scoreFont_, "# Buffer :" + buffer.Capacity, new Vector2(boardPosition.X - 75, boardPosition.Y - 75), Color.White);

                spriteBatch_.DrawString(scoreFont_, "bla:" + "nothing", new Vector2(boardPosition.X - 75, boardPosition.Y - 90), Color.White);

                spriteBatch_.DrawString(scoreFont_, "Last Input :\"" + "level.userinput?" + "\"", Vector2.Zero, Color.White);                

                //spriteBatch_.DrawString(scoreFont_, "Speed dir:" + Level.GetSpeedChangeDir().ToString(), new Vector2(boardPosition.X + 125, boardPosition.Y - 90), Color.White);
            
            }            

            spriteBatch_.End();
        }


        // Timer
        public TimeSpan gameTimer;

        // DRAWING
        Dictionary<string, Texture2D> bonus_;
        Texture2D xlife_;
        Texture2D board_;
        Texture2D boardFlash_;
        Texture2D crump_;
        Texture2D ppill_;
        SpriteFont scoreFont_;
        SpriteFont scoreEventFont_;
        SoundBank soundBank_;
        GraphicsDeviceManager graphics_;
        SpriteBatch spriteBatch_;

        //Extra timers: what time the match started, the time it is now and the #miliseconds played
        int sessionStartTime;
        int currentTime;
        int sessionTime;
        int lastSpeedChange;
        int lastAdaptCheck;

        // LOGIC
        List<Ghost> ghosts_;
        Player player_;
        TimeSpan lockTimer_;
        DateTime eventTimer_;
        int bonusSpawned_;
        bool bonusPresent_;
        DateTime bonusSpawnedTime_;
        Dictionary<string, int> bonusEaten_;
        bool playerDied_;
        bool paChomp_;
        int xlives_;
        int score_;
        int eatenGhosts_;
        List<ScoreEvent> scoreEvents_;


        /// <summary>
        /// The player's current score.
        /// </summary>
        public int Score {
            get { return score_; }
            private set {
                if ((value / 10000) > (score_ / 10000)) {
                    soundBank_.PlayCue("ExtraLife");
                    xlives_++;
                }
                score_ = value; 
            }
        }

        /// <summary>
        /// For how much time we want to lock the game.
        /// </summary>
        private TimeSpan LockTimer {
            get { return lockTimer_; }
            set { eventTimer_ = DateTime.Now; lockTimer_ = value; }
        }
    }
}