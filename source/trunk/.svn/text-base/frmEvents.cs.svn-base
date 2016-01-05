using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace XNAPacMan
{
    public partial class frmEvents : Form
    {
        string[] EventsList = Constants.EventsList;
        public enum events
        {
            Death, Crump_eaten, Power_Pill, Bonus_food,
            Ghost_eaten, Level_completed, Game_over, 
            TooEasy, TooHard, SpeedChange
        };

        int prevEvent;
        int freq = 0;
        bool debug = true;
        bool check = false;
        DateTime curt = DateTime.Now;
        ArrayList datalist = new ArrayList();
        ArrayList timelist = new ArrayList();
        ArrayList stamplist = new ArrayList();
        Label[] label = new Label[22];
        TextWriter outputFile = null;
        TextWriter CalibFile = null;
        TextReader CalibRead = null;
        string calibpath = "";
        int[] intensity_old = {-1000,-1000,-1000,-1000};
        int PIntensity = -1000;
        int[] PIn = { -1000, -1000, -1000, -1000 };

        public frmEvents staticVar = Constants.staticVar;

        public frmEvents()
        {
            //timestamp = getStampD(DateTime.Now);
            Console.WriteLine("Form Initialized" + getStampS(DateTime.Now));


            if (staticVar == null)
                staticVar = this;
            Constants.staticVar = staticVar;
            InitializeComponent();
            lbEvents.Items.Add("#\tEvent type\tSpree");
        }

        public void OpenFile()
        {
            try
            {
                string file = "";
                string s = txtFile.Text;
                bool exist = true;
                int i = 0;
                //Write the file to the user's directory
                //string path = Directory.GetCurrentDirectory() + "\\";
                string path = Constants.profilepath + Constants.username + "\\";
                while (exist)
                {
                    file = path + ++i + txtFile.Text;
                    if (!File.Exists(file))
                    {
                        exist = false;
                    }
                    else if (i > 100)
                    {
                        exist = false; // Als er iets fout is, geen oneindige loop
                    }
                }
                Constants.outfile = file;
                //Save the session ID to be able to link the calibration data to the event log
                Constants.sessionID = i;
                outputFile = new StreamWriter(file);
                Console.WriteLine("File opened for data dumping: " + file);
                outputFile.Write("Timestamp  Event P_Up    P_Down  P_Left  P_Right Ghostspeed");
                outputFile.WriteLine();
                outputFile.Close();
            }
            catch (System.Exception)
            {
                Console.WriteLine("File open failure");
            }
        }

        void WriteLog(int[] pressure, int eventID)
        {
            if (outputFile == null)
            {
                OpenFile();
            }
            outputFile = new StreamWriter(Constants.outfile, true);

            DateTime now = DateTime.Now;
            string towrite = getStampS(now);
            towrite += eventID.ToString().PadLeft(4);
            // Add pressure data
            foreach (int p in pressure)
            { towrite += p.ToString().PadLeft(8); }
            // Add ghost speed
            int gspeed = Ghost.GetGhostSpeed();
            towrite += gspeed.ToString().PadLeft(4) ;
            outputFile.Write(towrite);
            outputFile.WriteLine();

            outputFile.Close();
        }

        public string getStampS(DateTime t)
        {
            // post: Returns the timestamp in a formatted string of 11 characters
            double s = TimeSpan.Parse(t.TimeOfDay.ToString()).TotalSeconds;
            return s.ToString("#####.0000 ").PadLeft(11);
        }
        public double getStampD(DateTime t)
        {
            // post: Returns the timestamp as double
            return TimeSpan.Parse(t.TimeOfDay.ToString()).TotalSeconds;
        }

        public void OpenCalibFile(string filename)
        {
            //Opens the calibration log file.
            try
            {
                string directory = Constants.profilepath + Constants.username + "\\";
                calibpath = directory + filename;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                bool exist = File.Exists(calibpath);
                FileStream filestr = File.Open(calibpath, FileMode.Append, FileAccess.Write);
                //CalibFile = new StreamWriter(filestr);
                CalibFile = new StreamWriter(filestr);
                Console.WriteLine("File opened for calibration data dumping: " + calibpath);
                if (!exist)
                {
                    CalibFile.Write("// Feedback ID, EMG0, EMG1, GSR, Pressure, Ghostspeed, Time: Start, End, Interval");
                    CalibFile.WriteLine();
                }
                string mode = "CALIB";
                if (Level.GetLevelState() == Level.Mode.Adaptive)
                {   mode = "ADAPT(" + Constants.adapt_phase + ")"; }

                CalibFile.Write("<" + mode + "START> at: " + getStampS(DateTime.Now) + " Delay: " + Constants.feedbackDelay + 
                                ", Interval: " + Constants.parseInterval + ", SessionID: " + Constants.sessionID + 
                                ", SessionStart: " + getStampS(Constants.vidstarted));
                CalibFile.WriteLine();
                CalibFile.Close();
            }
            catch (System.Exception)
            {
                Console.WriteLine("Calibration file open failure");
            }
        }

        public List<DataPoint> ReadCalibData(string filename)
        {
            //Reads the data of the calibration log file.
            List<DataPoint> list = new List<DataPoint>();
            try
            {
                string directory = Constants.profilepath + Constants.username + "\\";
                calibpath = directory + filename;
                bool exist = File.Exists(calibpath);
                if (!exist)
                {   Console.WriteLine("Error: Calibration file not found"); }

                FileStream filestr = File.Open(calibpath, FileMode.Open, FileAccess.Read);
                CalibRead = new StreamReader(filestr);

                Console.WriteLine("File opened for calibration/adaptation data reading: " + calibpath);
                string data = "";
                string[] div;
                double[] numdiv = new double[10];
                DataPoint temp = new DataPoint();
                while (CalibRead.Peek() >= 0)
                {
                    data = CalibRead.ReadLine();
                    if (!data.StartsWith("<") && !data.StartsWith("//") && !data.StartsWith("4"))
                    {
                        temp = new DataPoint();
                        // Feedback ID, EMG0, EMG1, GSR, Pressure, Ghostspeed, Time: Start, End, Interval
                        // Divide the input data into parts
                        div = data.Split(' ');
                        for (int i = 1; i < div.Length; i++)
			            {
                            numdiv[i] = double.Parse(div[i]);
			            } 

                        // Write the datapoint
                        int cat = Int32.Parse(div[0]);
                        if(cat==0)      {   temp.Category = DataPoint.Categories.Easy;   }
                        else if(cat==1) {   temp.Category = DataPoint.Categories.Good;   }
                        else if(cat==2) {   temp.Category = DataPoint.Categories.Hard;   }

                        temp.EMG_0 = (int)(numdiv[1]);
                        temp.EMG_1 = (int)(numdiv[2]);
                        temp.GSR = (int)(numdiv[3]);
                        temp.ButtonPressure = (int)(numdiv[4]);
                        temp.GhostSpeed = (int)(numdiv[5]);
                        temp.Time = numdiv[7];
                        /*temp.EMG_0 = Int32.Parse(div[1]);
                        temp.EMG_1 = Int32.Parse(div[2]);
                        temp.GSR = Int32.Parse(div[3]);
                        temp.ButtonPressure = Int32.Parse(div[4]);
                        temp.GhostSpeed = Int32.Parse(div[5]);
                        temp.Time = Double.Parse(div[7]);*/
                        bool dbg = false;
                        if (dbg)
                        {
                            string test = "";
                            for (int i = 0; i < div.Length; i++)
                            {
                                test += div[i] + ", ";
                            }
                            Console.WriteLine(test);
                        }
                        list.Add(temp);
                    }
                }
                
                CalibRead.Close();

                Console.WriteLine("Number of calibration/adaptation points loaded: " + list.Count);
                return list;
            }
            catch (System.Exception)
            {
                Console.WriteLine("Calibration file open failure: " + calibpath);
                return null;
            }
        }

        public int getLowIndex(int buflen, List<DataPoint> buffer)
        {
            // Returns the index of the first item of the buffer so
            // only items of the desired range are included in the calculation.
            int i = 0;
            // Left and right bounds for searching in the buffer
            int left = 0, right = buflen - 1;
            double endtime = buffer[right].Time;
            double goal = endtime - Constants.parseInterval;
            double curtime = endtime;
            int steps = 0;
            bool end = false;
            // If there's less than 5000 milliseconds elapsed, no need to run the loop
            if (goal < buffer[0].Time)
            { end = true; }
            while (!end)
            {
                i = (int)Math.Round((double)(left + right) / 2);
                curtime = buffer[i].Time;
                // Change the bounds in which to look to get closer to the desired value.
                if (goal > curtime)
                {
                    left = i;
                }
                else if (goal < curtime)
                {
                    right = i;
                }
                // Quit if the value is not within the tolerance zone, no infinite loop
                if (right - left < 3) { end = true; }
                if (Math.Abs(curtime - goal) < Constants.tolerance)
                {
                    end = true;
                }
                steps++; // How many times did the loop run to get the "OK" value
            }
            // Debug info:
            //Console.WriteLine(curtime + ", " + goal + ", " + steps + ", " + i + ", " + (endtime - curtime));
            return i;
        }

        public DataPoint WriteCalibPoints(int start, int end, List<DataPoint> buffer, int ID)
        {
            int range = end - start;
            double cumulative = 0;
            CalibFile = new StreamWriter(calibpath,true);
            DataPoint temp = new DataPoint();
            double calc;
            if (CalibFile == null)
            {
                Console.WriteLine("Error: no calibration file to write to");
                return null;
            }
            else
            {
                string towrite = "";
                // Write ID (too easy = 0, too hard = 1, good = 2)
                towrite += ID + " ";
                if (ID == 0) { temp.Category = DataPoint.Categories.Easy; }
                if (ID == 1) { temp.Category = DataPoint.Categories.Good; }
                if (ID == 2) { temp.Category = DataPoint.Categories.Hard; }
                //  Calculate average values
                // EMG 0
                cumulative = 0;
                for (int i = start; i < end; i++)
                { cumulative += buffer[i].EMG_0; }
                calc = Math.Round((cumulative / range), 2);
                temp.EMG_0 = (int)calc;
                towrite += calc + " ";
                // EMG 1
                cumulative = 0;
                for (int i = start; i < end; i++)
                { cumulative += buffer[i].EMG_1; }
                calc = Math.Round((cumulative / range), 2);
                temp.EMG_1 = (int)calc;
                towrite += calc + " ";
                // GSR
                cumulative = 0;
                for (int i = start; i < end; i++)
                { cumulative += buffer[i].GSR; }
                calc = Math.Round((cumulative / range), 2);
                temp.GSR = (int)calc;
                towrite += calc + " ";
                // Button pressure
                cumulative = 0;
                for (int i = start; i < end; i++)
                { cumulative += buffer[i].ButtonPressure; }
                calc = Math.Round((cumulative / range), 2);
                temp.ButtonPressure = (int)calc;
                towrite += calc + " ";
                // Ghostspeed
                cumulative = 0;
                for (int i = start; i < end; i++)
                { cumulative += buffer[i].GhostSpeed; }
                calc = Math.Round((cumulative / range), 2);
                temp.GhostSpeed = (int)calc;
                towrite += calc + " ";
                // Write Times
                double sttime = buffer[start].Time;
                temp.Time = sttime;
                towrite += sttime + " ";
                towrite += buffer[end - 1].Time + " ";               
                // For reading and debugging convenience, the interval
                towrite += (buffer[end - 1].Time - buffer[start].Time);

                CalibFile.Write(towrite);
                CalibFile.WriteLine();
                CalibFile.Close();
                //Console.WriteLine(towrite);
                return temp;
            }
        }

        public void EventChange(int e, int change, double curTime, string ev_str)
        {
            // ev_str = Constants.EventsList[eventID];
            freq++;
            if (chkGroup.Checked)
                if (change == prevEvent)
                {
                    lbEvents.Items.RemoveAt(lbEvents.Items.Count - 1);
                }
                else
                    freq = 1;
            string s = e + "\t" + ev_str + "\t" + freq;
            if (change == 0)
                Console.WriteLine("DEATH");
            lbEvents.Items.Add(s); //curTime.TimeOfDay.Hours
            datalist.Add(change);
            //timelist.Add(curTime);
            stamplist.Add(getStampD(DateTime.Now));
            prevEvent = change;
            lbEvents.SelectedIndex = lbEvents.Items.Count - 1;
            WriteLog(intensity_old, change);
            check = true;
        }

        public void UpdateLabels(int x, int y, int score)
        {
            lblXpos.Text = "X Pos: " + x;
            lblYpos.Text = "Y Pos: " + y;
            lblScore.Text = "Score: " + score;
        }
        public void UpdateLabels(string[] evname, int[] evcnt)
        {
            if (evname.Length != evcnt.Length)
                Console.WriteLine("Event type and count lengths do not match");
            else
            {
                for (int i = 0; i < evname.Length; i++)
                {
                    label[i].Text = evname[i] + ": " + evcnt[i];
                }
            }
        }
        public void UpdateLabels(int[] evcnt)
        {
            for (int i = 0; i < evcnt.Length; i++)
            {
                label[i].Text = evcnt[i].ToString();
            }
        }

        public void UpdateIntensity(int intensity)
        {
            PIntensity = intensity;
        }
        public void UpdateIntensity(int[] intensity)
        {
            PIn = intensity;
            bool ok = false;
            for (int i = 0; i < 4; i++)
            {
                if(intensity_old != null && intensity[i]!=intensity_old[i])
                    ok = true;
            }
            if (ok) WriteLog(intensity, -1);

            intensity_old = intensity;
        }

        public void AddPressureInput(string direction, int intensity)
        {
            decimal pcent = (decimal)(intensity + 1000) / 20;

            lbInput.Items.Add("D-Pad " + direction + " (" + pcent + "%)");
            lbInput.SelectedItem = lbInput.Items.Count - 1;
        }
        public void AddPressureInput(string direction, DateTime t)
        {
            lbInput.Items.Add(direction + " (" + t + ")");
            lbInput.SelectedItem = lbInput.Items.Count - 1;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            label1.Text = "Ik haat Pacman";
        }

        private void frmEvents_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Events log " + sender + e);

            txtTime.Text = trkTime.Value.ToString();
            for (int i = 0; i < EventsList.Length; i++)
            {
                this.label[i] = new Label();
                label[i].Size = new Size(38, 13);
                label[i].Location = new System.Drawing.Point(400, 107 + 17 * i);//4(233,70+17*i);
                label[i].Visible = true;
                this.Controls.Add(label[i]);
            }
            int t = EventsList.Length;
            for (int i = 0; i < EventsList.Length; i++)
            {
                this.label[t] = new Label();
                label[t].Text = EventsList[i];
                label[t].Size = new Size(167, 13);
                label[t].Location=new System.Drawing.Point(233,107+17*i);//4(233,70+17*i);
                label[t].Visible = true;
                this.Controls.Add(label[t]);
                t++;
            }
        }

        private void lbEvents_Click(object sender, EventArgs e)
        {
            check = true;
            //prevEvent=lbEvents.SelectedIndex
        }

        private void lbEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chkTimelog.Checked)
            {

                int dc = datalist.Count;
                int s = 1;

                if (check)
                {
                    double T;
                    lbCheck.Items.Clear();
                    for (int i = 0; i < dc; i++)
                        if ((int)datalist[i] == prevEvent)//((string)datalist[i] == prevEvent)
                        {
                            T = (double)stamplist[i];
                            lbCheck.Items.Add(s++ + ". " + T.ToString("#####.0000 ").PadLeft(11));
                        }
                    lbCheck.SelectedIndex = lbCheck.Items.Count - 1;
                }
                check = false;
            }
        }

        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            freq = 0;
            lbEvents.Items.Clear();
            lbEvents.Items.Add("#\tEvent type\tSpree");
            object pe = 0;
            string ev_str;
            string s;
            for (int i = 0; i < datalist.Count; i++)
            {
                ev_str = Constants.EventsList[(int)datalist[i]];
                freq++;
                s = i+1 + "\t" + ev_str + "\t" + freq;
                    
                if (chkGroup.Checked && datalist[i] == pe)
                {
                    lbEvents.Items.RemoveAt(lbEvents.Items.Count-1);
                }
                if (datalist[i]!=pe)
                {
                    freq = 1;
                    s = i + "\t" + ev_str + "\t" + freq;
                }   
                
                lbEvents.Items.Add(s);
                pe = datalist[i];   
             }
        }

        public void TimerInit()
        {
            curt = DateTime.Now;
        }

        public class Event
        {
            // Each single event is saved in a list of name+time
            ArrayList strArray = new ArrayList();
            ArrayList timeArray = new ArrayList();
            string eventType;
            int EventCount = 0;
            public static int evID = 0;
            int eventID;
            double tsInit, tsNow;

            public Event(string eventtype)
            {
                if (eventtype.StartsWith("Death"))
                    evID = 0; // Every new instance of frmEvents should start with the same event numbers
                eventID = evID;
                //initTime = DateTime.Now;
                tsInit = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;
                
                eventType = eventtype;
                evID++;
                if (Constants.debug)
                    Console.WriteLine("Event " + evID + "(" + eventType + ") initialized at: " + tsInit.ToString("#####.0000 ").PadLeft(11));
            }

            public string Name
            {
                get { return eventType; }
            }

            public int Count
            {
                get { return EventCount; }
            }  

            public int CheckTimePeriod(int eventID, decimal t)
            {
                int counter = 0;
                foreach (DateTime ti in timeArray)
                    if (ti > DateTime.Now)
                        counter++;
                return counter;
            }

            public void AddEvent(frmEvents fe)
            {
                string newevent = this.eventType;
                EventCount++;
                tsNow = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;//DateTime.Now;
                timeArray.Add(tsNow);
                strArray.Add(newevent);
                fe.EventChange(EventCount, eventID, tsNow, newevent);
            }
            public void AddEvent(frmEvents fe, int gspeed)
            {
                string newevent = this.eventType + " (" + gspeed + ")";
                EventCount++;
                tsNow = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;//DateTime.Now;
                timeArray.Add(tsNow);
                strArray.Add(newevent);
                fe.EventChange(EventCount, eventID, tsNow, newevent);
            }

            static int CheckFrequency(string eventtype, ArrayList sta)
                {
                    // Check the entire eventlist for the active event
                    // to get the frequency of that specific event
                    int freq = 0;
                    foreach (string s in sta)
                        if (s==eventtype)
                            freq++;
                    return freq;
                }
            }

            private void chkTimelog_CheckedChanged(object sender, EventArgs e)
            {
                if (!chkTimelog.Checked)
                    lbCheck.Items.Clear();
            }

            private void trkTime_Scroll(object sender, EventArgs e)
            {
                txtTime.Text = trkTime.Value.ToString();
            }

            private void txtTime_TextChanged(object sender, EventArgs e)
            {
                try
                {
                    int i = Convert.ToInt32(txtTime.Text);
                    if (i > 30)
                        txtTime.Text = "5";
                    trkTime.Value = i;
                }
                catch (FormatException) 
                { 
                    txtTime.Text = "5";
                    if (debug) { Console.WriteLine("Bad time input"); }
                }
            }

            private void tmrUpdate_Tick(object sender, EventArgs e)
            {
                Constants.tmrTick = true;
                double elapsedtime = (DateTime.Now - curt).TotalSeconds;

                //Average about 700 updates/second:
                //lblTime.Text = "Elapsed Time: (" + (Constants.frames / elapsedtime) + ") " + Convert.ToString(elapsedtime);
                lblTime.Text = "Elapsed Time: " + Convert.ToString(elapsedtime);
                string toprint = "";
                for (int i = 0; i < 4; i++)
                {
                    toprint += ((PIn[i] + 1000) / 20) + "% ";
                }
                //lblPressure.Text = "Pressure: " + ((PIntensity + 1000) / 20) + " %";
                lblPressure.Text = "Pressure: " + toprint;
            }
    }
}