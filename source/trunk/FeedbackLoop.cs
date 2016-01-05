using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;
using System.Globalization;

namespace XNAPacMan
{
    public class FeedbackLoop
    {
        void distance(string[] args)
        {
            int x = getNumber;
            int y = getNumber;
            int a = 2, b = 3;

            double test = Distance(x, y, a, b);
            Console.WriteLine("Distance between " + a + "," + b + " and " + x + "," + y + " = " + test);
        }

        static int getNumber
        {
            get
            {
                int a;
                try
                {
                    string s = Console.ReadLine();
                    a = Int32.Parse(s);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid integer");
                    a = 0;
                }
                return a;
            }
        }

        static double Distance(double xa, double ya, double xb, double yb)
        {   //pre: (x_a, y_a), (x_b, y_b) \in Z
            //post: Sqrt(((xa - xb) ^ 2) + ((ya - yb) ^ 2)) ((euclidian distance ))
            double result;                                                                 //init hulpvar
            double tx = Math.Pow(xa - xb, 2);
            double ty = Math.Pow(ya - yb, 2);
            result = Math.Sqrt(tx + ty);
            //result = Math.Sqrt(((xa - xb) ^ 2) + ((ya - yb) ^ 2));
            return result;
        }

        static double Distance(double xa, double ya, double za, double qa, double ra,
                                 double xb, double yb, double zb, double qb, double rb)
        {   //pre: (x_a, y_a, z_a, q_a) (x_b, y_b, z_b, z_b), \in Z
            //post: Sqrt(((xa - xb) ^ 2) + ((ya - yb) ^ 2) + ((za - zb) ^ 2) + ((qa - qb) ^ 2)) ((euclidian distance ))
            double result;                                                                 //init hulpvar
            double tx = Math.Pow(xa - xb, 2);           
            double ty = Math.Pow(ya - yb, 2);           
            double tz = Math.Pow(za - zb, 2);           
            double tq = Math.Pow((qa - qb) / 1000, 2);  
            double tr = Math.Pow(ra - rb, 2);           
            result = Math.Sqrt(tx + ty + tz + tq + tr);
            return result;
        }

        static double Distance(double xa, double ya, double za,
                               double xb, double yb, double zb)
        {   //pre: (x_a, y_a, z_a, q_a) (x_b, y_b, z_b, z_b), \in Z
            //post: Sqrt(((xa - xb) ^ 2) + ((ya - yb) ^ 2) + ((za - zb) ^ 2) + ((qa - qb) ^ 2)) ((euclidian distance ))
            double result;                                                                 //init hulpvar
            double tx = Math.Pow(xa - xb, 2);
            double ty = Math.Pow((ya - yb) / 1000, 2);      // Normalize button pressure
            double tz = Math.Pow(za - zb, 2); 
            result = Math.Sqrt(tx + ty + tz);
            return result;
        }

        public static string KNearestNeighbour(ArrayList training, ArrayList test)
        {   //pre:
            //post:
                 // init hulp var
            string result = "";
            ArrayList distance = new ArrayList();
            ArrayList closestSet = new ArrayList();

            foreach (double z in test)
            { //foreach test example z
                foreach (double j in training)
                { //compute distance between z and each element j in training
                    // compute distance between i and test
                    //distance.Add(Distance(z, j));

                }
                
                //Sort and Select the k closest;
                distance.Sort();
                for (int i = 0; i < Constants.k; i++)
                {
                    closestSet.Add(distance[i]);
                }
                
                // Determine overall category
                    //TODO!

                // assign category to elements in test
                    //TODO!
            }

            return result;
        }

        public static int[] KNearestNeighbour(List<DataPoint> caliblist, DataPoint a)
        {   //pre:
            //post:0=too easy, 1=good, 2=too hard
            // init hulp var
            int k = Constants.k;
            double absdistance = 0;
            List<double> distance = new List<double>();
            int[] counter = new int[3];

            foreach (DataPoint d in caliblist)
            {
                //NO EMG
                //absdistance = Distance(d.EMG_0, d.EMG_1, d.GSR, d.ButtonPressure, d.GhostSpeed,
                //                       a.EMG_0, a.EMG_1, a.GSR, a.ButtonPressure, a.GhostSpeed);

                //absdistance = Distance(d.GSR, d.ButtonPressure, d.GhostSpeed,
                //                       a.GSR, a.ButtonPressure, a.GhostSpeed);
                absdistance = Distance(d.ButtonPressure, 0,
                                       a.ButtonPressure, 0);
                distance.Add(absdistance);
            }

            int tempID;
            for (int i = 0; i < k; i++)
            {
                tempID = distance.IndexOf(distance.Min());
                //Add an item to the category list
                switch (caliblist[tempID].Category)
                {
                    case DataPoint.Categories.Easy:
                        counter[0]++; break;
                    case DataPoint.Categories.Good:
                        counter[1]++; break;
                    case DataPoint.Categories.Hard:
                        counter[2]++; break;
                }
                distance[tempID] = 1e11;
            }
            // If hard or easy are dominant, choose that result. Otherwise -> good.
            //string r = "";  // For debugging only

            /*int result = -1;
            if (counter[0] > counter[2] && counter[0] >= (k / 2))
            { result = 0; r = "Too easy"; }
            else if (counter[0] < counter[2] && counter[2] >= (k / 2))
            { result = 2; r = "Too hard"; }
            else
            { result = 1; r = "Good"; }*/

            ChangeGameSpeed(counter, k);
            Console.WriteLine("Phase " + Constants.adapt_phase + ". Speed: " + Level.GetSpeedChangeDir());
            Console.WriteLine("Categories: " + counter[0] + ", " + counter[1] + ", " + counter[2] +
                ". Absdistance: " + absdistance);
            return counter;
        }

        static void ChangeGameSpeed(int[] c, int k)
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

        public static DataPoint Parse(int pressure)
        {
            DataPoint result = new DataPoint();
            result.EMG_0 = 0;
            result.EMG_1 = 0;
            result.GSR = 0;
            result.ButtonPressure = pressure;
            result.Time = DateTime.Now.TimeOfDay.TotalMilliseconds;
            //timestamp = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;
            result.GhostSpeed = Ghost.GetGhostSpeed();
            
            return result;
        }

        static Boolean ReadLog(ref List<string[]> lines)
        {
            bool result = false;
            char[] sep = { ' ' };               //seperator character
            string name = "temp";           //name of file to read
            string extension = "_fys.txt";
            FileInfo[] files;
            bool seenSpace = false;
            bool first = true;
            bool readLastTime = true;
            float lastTime = 0;
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", true);

            //Go to Desktop, get all files on desktop, take the only MobiHTI log file
            string infopath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //path to file to read
            DirectoryInfo dir = new DirectoryInfo(infopath);
            files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains(extension)) { name = file.Name; }
            }


            //Start reading backwards

            try
            {

                FileStream fs = new FileStream(infopath + "/" + name, FileMode.Open, FileAccess.Read);

                long offset;
                string readLine = "";

                // read the file backwards using SeekOrigin.Begin...
                for (offset = fs.Length - 1; offset >= 0; offset--)
                {

                    fs.Seek(offset, SeekOrigin.Begin);
                    char readChar = Convert.ToChar(fs.ReadByte());

                    if (readChar == '\n' || (seenSpace && readChar == ' '))
                    {
                        continue;
                    }
                    else
                    {
                        if (alpha.Contains(readChar))
                        {
                            break;
                        }
                        else
                        {
                            if (readChar == '\r' && !first)
                            {
                                // splitten, checken 
                                string[] splitLine;

                                splitLine = readLine.Split(sep, 8); //split into 7 parts
                                lines.Add(splitLine);           //Add it to the list of all input strings
                                //Console.WriteLine("done reading line, outputting to:");
                                //Console.WriteLine(splitLine);

                                readLine = "";
                                //if this is the last sample we need to read, update last sample and break 
                                if (readLastTime)
                                {
                                    lastTime = Convert.ToSingle(splitLine[2]);
                                    readLastTime = false;
                                }
                                else
                                {
                                    if (!readLastTime && (lastTime - Convert.ToSingle(splitLine[2])) > 10)
                                    {
                                        break;
                                    }
                                }

                            }
                            else
                            {
                                if (readChar == '\r' && first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    if (readChar == ' ' && !seenSpace)
                                    {
                                        seenSpace = true;
                                        //add char to front of line reading
                                        readLine = readChar + readLine;
                                    }
                                    else
                                    {
                                        seenSpace = false;
                                        //add char to front of line reading
                                        readLine = readChar + readLine;
                                    }
                                }
                            }
                        }
                    }
                }//end for       
                Console.WriteLine("Read file. ");
                Console.WriteLine("Lines Count: " + lines.Count.ToString());
                //Console.WriteLine("Last Lines: " + lines.Last().ToString());
                fs.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("Failed to read:", e);
                //throw;
                Thread.Sleep(500);
                lines.Clear();
                result = true;

            }


            return result;
        }

        //public static DataPoint ParseLog(List<string[]> list, int pressure, Level.UserInput userInput, double time)
        //{
        //    //hulp vars
        //    DataPoint result = new DataPoint(); //create result
        //    float gsr = 0;
        //    int count = 0;
            
        //    //Fill in pressure 
        //    if (pressure > 0) { result.ButtonPressure = pressure; } else { result.ButtonPressure = 0;} //copy first 
            
        //    //Fill in GSR, average
        //    // List({}, sample#, timestamp, emg, ecg, gsr, marker, sawtooth)
        //    if (list.Count > 0)
        //    {
        //        foreach (string[] punt in list)
        //        {
        //           // if (Convert.ToSingle(punt[5]) > 0 )
        //           // {
        //                gsr = gsr + Convert.ToSingle(punt[5]);
        //                count = count + 1;
        //           // }
                    
        //        }

        //        gsr = gsr / count;
        //    }

        //    result.GSR = gsr;

        //    if (Level.GetLevelState() == Level.Mode.Calibration)
        //    {   //In calibration mode weten we (hopelijk) de categorie van het punt
        //        switch (userInput)
        //        {
        //            case Level.UserInput.TooEasy:
        //                result.Category = DataPoint.Categories.Easy;
        //                break;
        //            case Level.UserInput.Good:
        //                result.Category = DataPoint.Categories.Good;
        //                break;
        //            case Level.UserInput.TooHard:
        //                result.Category = DataPoint.Categories.Hard;
        //                break;
        //            case Level.UserInput.none:
        //                result.Category = DataPoint.Categories.invalid;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {  //In adaptive mode moeten we kNN loslaten om te zien welke categorie dit nieuwe punt heeft

        //    }



        //    result.EMG_0 = Convert.ToInt32(list.Last()[7]);

        //    return result;
        //}


        public static void ReadParseMobiLog()
        {   
            /*
            //init help vars
            DataPoint result = new DataPoint(); //result var
            Boolean fileIOError;

            List<string[]> lines = new List<string[]>();    //save all readlines to a list
            
            Level.UserInput input = Level.UserInput.none;
            int pressure = 0;

            //Get current userinput
            lock(Constants._lockerInput) { input = Constants.userInput; }

            //Get the associated pressure
            lock (Constants._lockerPressure) { pressure = Constants.pressure; }
	        

            //Get the current time of day
            double t = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;

            ReadFile: //Label
            fileIOError = ReadLog(ref lines);
            if (fileIOError)
            {   //An IO Error happened while reading the file, try again
                goto ReadFile;
            }
            //lines now contains the read input
            Console.WriteLine("Read Log file");

            //lines now contains all lines that were written since last call

            result = ParseLog(lines, pressure, input, t);
            Console.WriteLine("Log Parsed");

            
            lock(Constants._lockerBuffer)
            {
                Constants.buffer.Add(result);
            }

            //buffer.Clear();
            //return result;
            */
        }
    }
    
    public class DataPoint
    {
        //Fields (accessed through properties)
        private double time;
        private float emg_0;
        private float emg_1;
        private float gsr;
        private int pressure;
        private int speed;
        private Categories category;

        //Properties
        public double Time {
            get { return time; }
            set { time = value; }
        }    
        public float EMG_0
        {
            get { return emg_0; }
            set { emg_0 = value; }
        }
        public float EMG_1 {
            get { return emg_1; }
            set { emg_1 = value; }
        }
        public float GSR {
            get { return gsr; }
            set { gsr = value; } 
        }
        public int ButtonPressure {
            get { return pressure; }
            set { pressure = value; }
        }
        public int GhostSpeed {
            get { return speed; }
            set { speed = value; }
        }
        public Categories Category
        {
            get { return category; }
            set
            {   //post: if {(emg_0 || emg_1||gsr||pressure) != 0} then category = value else category = invalid;

                    category = value;
                
            }
        }

        public enum Categories { Easy, Good, Hard, invalid };
    }
}
