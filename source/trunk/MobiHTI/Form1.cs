using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using componentXtra;
using System.IO;
using System.Threading;

namespace MobiHTI
{
    public partial class Form1 : Form
    {
        MyMobiClass Mobi;
        bool contact;
        bool measuring;
        private bool low, empty, blinking;
        //        int[] graph=new int[14];
        private List<int> graph = new List<int>();
        int[] sample = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] subsamplefactor = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        float[] data;
        XYGraph[] myGraphArray = new XYGraph[14];
        float[] lastValueArray = new float[14];
        TextBox[] myTextBoxArray = new TextBox[14];
        CheckBox[] myCheckBoxArray = new CheckBox[14];
        GroupBox[] myGroupBoxArray = new GroupBox[14];
        VScrollBar[] myYRangeScrollbarArray = new VScrollBar[14];
        VScrollBar[] myYOffsetScrollbarArray = new VScrollBar[14];
        HScrollBar[] myXScaleScrollbarArray = new HScrollBar[14];
        float[] YRange = new float[14];
        float[] XScale = new float[14];
        float[] YOffset = new float[14];
        TextWriter outputFile = null;
        TextReader infoFileIn = null;
        TextWriter infoFileOut = null;
        double T = 0.0;//timestamp before each GetData call.
        double TS = 0.0;//first timestamp
        double TE = 0.0;//last timestamp
        double Told = 0.0;
        int SampleNumber = 0;
        List<int> graphList = new List<int>();//list with output indexes for the graphs
        const string defaultFileName = "<Prefix><DateTime>.fys";//placeholder
        string fileName;
        string path;
        string defaultOutPath;
        string prefix;
        List<string> output = new List<string>();

        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", true);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            low = false;
            empty = false;
            blinking = false;
            contact = false;
            measuring = false;
            timer1.Enabled = false;
            data = new float[100000];
            fileName = defaultFileName;
            textBox17.Text = fileName;
            textBox15.Text = "9";
            button2.Enabled = false;
            myGraphArray[0] = xyGraph1; myGraphArray[1] = xyGraph2; myGraphArray[2] = xyGraph3;
            myGraphArray[3] = xyGraph4; myGraphArray[4] = xyGraph5; myGraphArray[5] = xyGraph6;
            myGraphArray[6] = xyGraph7; myGraphArray[7] = xyGraph8; myGraphArray[8] = xyGraph9;
            myGraphArray[9] = xyGraph10;
            myGraphArray[10] = xyGraph11; myGraphArray[11] = xyGraph12; myGraphArray[12] = xyGraph13;
            myGraphArray[13] = xyGraph14;

            for (int i = 0; i < 14; i++) myGraphArray[i].XtraMinLeftMargin = 40;
            myCheckBoxArray[0] = checkBox1; myCheckBoxArray[1] = checkBox2; myCheckBoxArray[2] = checkBox3;
            myCheckBoxArray[3] = checkBox4; myCheckBoxArray[4] = checkBox5; myCheckBoxArray[5] = checkBox6;
            myCheckBoxArray[6] = checkBox7; myCheckBoxArray[7] = checkBox8; myCheckBoxArray[8] = checkBox9;
            myCheckBoxArray[9] = checkBox10;
            myCheckBoxArray[10] = checkBox11; myCheckBoxArray[11] = checkBox12; myCheckBoxArray[12] = checkBox13;
            myCheckBoxArray[13] = checkBox14;

            myTextBoxArray[0] = textBox1; myTextBoxArray[1] = textBox2; myTextBoxArray[2] = textBox3;
            myTextBoxArray[3] = textBox4; myTextBoxArray[4] = textBox5; myTextBoxArray[5] = textBox6;
            myTextBoxArray[6] = textBox7; myTextBoxArray[7] = textBox8; myTextBoxArray[8] = textBox9;
            myTextBoxArray[9] = textBox10;
            myTextBoxArray[10] = textBox19; myTextBoxArray[11] = textBox20; myTextBoxArray[12] = textBox21;
            myTextBoxArray[13] = textBox22;

            myGroupBoxArray[0] = groupBox1; myGroupBoxArray[1] = groupBox2; myGroupBoxArray[2] = groupBox3;
            myGroupBoxArray[3] = groupBox4; myGroupBoxArray[4] = groupBox5; myGroupBoxArray[5] = groupBox6;
            myGroupBoxArray[6] = groupBox7; myGroupBoxArray[7] = groupBox8; myGroupBoxArray[8] = groupBox9;
            myGroupBoxArray[9] = groupBox10;
            myGroupBoxArray[10] = groupBox17; myGroupBoxArray[11] = groupBox18; myGroupBoxArray[12] = groupBox19;
            myGroupBoxArray[13] = groupBox20;

            myYOffsetScrollbarArray[0] = OffsetScrollBar1; myYOffsetScrollbarArray[1] = OffsetScrollBar2;
            myYOffsetScrollbarArray[2] = OffsetScrollBar3; myYOffsetScrollbarArray[3] = OffsetScrollBar4;
            myYOffsetScrollbarArray[4] = OffsetScrollBar5; myYOffsetScrollbarArray[5] = OffsetScrollBar6;
            myYOffsetScrollbarArray[6] = OffsetScrollBar7; myYOffsetScrollbarArray[7] = OffsetScrollBar8;
            myYOffsetScrollbarArray[8] = OffsetScrollBar9; myYOffsetScrollbarArray[9] = OffsetScrollBar10;
            myYOffsetScrollbarArray[10] = OffsetScrollBar11; myYOffsetScrollbarArray[11] = OffsetScrollBar12;
            myYOffsetScrollbarArray[12] = OffsetScrollBar13; myYOffsetScrollbarArray[13] = OffsetScrollBar14;

            myYRangeScrollbarArray[0] = yScaleScrollBar1; myYRangeScrollbarArray[1] = yScaleScrollBar2;
            myYRangeScrollbarArray[2] = yScaleScrollBar3; myYRangeScrollbarArray[3] = yScaleScrollBar4;
            myYRangeScrollbarArray[4] = yScaleScrollBar5; myYRangeScrollbarArray[5] = yScaleScrollBar6;
            myYRangeScrollbarArray[6] = yScaleScrollBar7; myYRangeScrollbarArray[7] = yScaleScrollBar8;
            myYRangeScrollbarArray[8] = yScaleScrollBar9; myYRangeScrollbarArray[9] = yScaleScrollBar10;
            myYRangeScrollbarArray[10] = yScaleScrollBar11; myYRangeScrollbarArray[11] = yScaleScrollBar12;
            myYRangeScrollbarArray[12] = yScaleScrollBar13; myYRangeScrollbarArray[13] = yScaleScrollBar14;

            myXScaleScrollbarArray[0] = xScaleScrollBar1; myXScaleScrollbarArray[1] = xScaleScrollBar2;
            myXScaleScrollbarArray[2] = xScaleScrollBar3; myXScaleScrollbarArray[3] = xScaleScrollBar4;
            myXScaleScrollbarArray[4] = xScaleScrollBar5; myXScaleScrollbarArray[5] = xScaleScrollBar6;
            myXScaleScrollbarArray[6] = xScaleScrollBar7; myXScaleScrollbarArray[7] = xScaleScrollBar8;
            myXScaleScrollbarArray[8] = xScaleScrollBar9; myXScaleScrollbarArray[9] = xScaleScrollBar10;
            myXScaleScrollbarArray[10] = xScaleScrollBar11; myXScaleScrollbarArray[11] = xScaleScrollBar12;
            myXScaleScrollbarArray[12] = xScaleScrollBar13; myXScaleScrollbarArray[13] = xScaleScrollBar14;

            for (int i = 0; i < 14; i++)
            {
                myYOffsetScrollbarArray[i].Minimum = -1000;
                myYOffsetScrollbarArray[i].Maximum = 1000;
                myYRangeScrollbarArray[i].Minimum = -1000;
                myYRangeScrollbarArray[i].Maximum = 1000;
                myXScaleScrollbarArray[i].Minimum = -1000;
                myXScaleScrollbarArray[i].Maximum = 1000;
            }
        }

        private void ReadSettings(int type)
        {
            string name = "";
            if (type == 6)
            {
                name = "Mobi6HTI.set";
            }
            else
            {
                name = "Mobi8HTI.set";
            }
            try// now read the info from MobiHTI.info
            {
                string infopath;
                infopath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MobiHTI\\" + name;
                infoFileIn = new StreamReader(infopath);
                defaultOutPath = infoFileIn.ReadLine();//read output path
                textBox16.Text = defaultOutPath;
                string s = infoFileIn.ReadLine();
                char[] sep = { ' ' };
                string[] CheckedVar = s.Split(sep, Mobi.NumberOfChannels + 1);
                for (int i = 0; i < Mobi.NumberOfChannels; i++)
                {

                    if (CheckedVar[i] == "1" || CheckedVar[i] == "1 ") myCheckBoxArray[i].Checked = true;
                }
                if (CheckedVar[Mobi.NumberOfChannels] == "1" || CheckedVar[Mobi.NumberOfChannels] == "1 ") checkBox15.Checked = true;
                s = infoFileIn.ReadLine();
                //Set SampleRate which was set in the settings
                Mobi.SampleRate = Convert.ToSingle(s);

                textBox13.Text = Mobi.SampleRate.ToString();
                infoFileIn.Close();
            }
            catch (FileNotFoundException e)
            {
                defaultOutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                textBox16.Text = defaultOutPath;
                return;//file does not exists yet
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString() + "\nError reading" + name);
                return;
            }
        }

        private void WriteSettings(int type)
        {
            string name = "";
            if (type == 6)
            {
                name = "Mobi6HTI.set";
            }
            else
            {
                name = "Mobi8HTI.set";
            }
            try
            {
                infoFileIn.Close();
            }
            catch (System.Exception e)
            {
                //do nothing
            }
            try
            {
                string infopath;
                infopath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MobiHTI\\" + name;
                infoFileOut = new StreamWriter(infopath, false);
                infoFileOut.WriteLine(defaultOutPath.ToString());
                for (int i = 0; i < Mobi.NumberOfChannels; i++)
                {
                    if (myCheckBoxArray[i].Checked)
                        infoFileOut.Write("1 ");
                    else
                        infoFileOut.Write("0 ");
                }
                if (checkBox15.Checked)
                    infoFileOut.Write("1 ");
                else
                    infoFileOut.Write("0 ");
                infoFileOut.WriteLine();
                infoFileOut.WriteLine(Mobi.SampleRate.ToString());
                infoFileOut.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString() + "\nError writing" + name);
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!contact)
            {
                //create mobi object
                Mobi = new MyMobiClass(hScrollBar2.Value);
                //Doe iets met de Mobi
                Mobi.KeepSerialContact();

                if (Mobi.Name == null) return;                                            //if there's no Mobi, return
                //Show Mobi8 possibilities
                if (Mobi.Name == "Mobi8")
                {
                    ReadSettings(8);
                    checkBox1.Text = "BIPA";
                    checkBox2.Text = "BIPB";
                    checkBox3.Text = "BIPC";
                    checkBox4.Text = "BIPD";
                    checkBox5.Text = "AUXE";
                    checkBox6.Text = "AUXF";
                    checkBox7.Text = "AUXG";
                    checkBox8.Text = "AUXH";
                    checkBox9.Text = "SAO2";
                    checkBox10.Text = "PLETH";
                    checkBox11.Text = "HRATE";
                    checkBox12.Text = "STATUS";
                    checkBox13.Text = "MARKER";
                    checkBox14.Text = "SAW";
                }
                //show Mobi6 possibilities
                else
                {
                    ReadSettings(6);
                    checkBox1.Text = "BIP1";
                    checkBox2.Text = "BIP2";
                    checkBox3.Text = "AUX3";
                    checkBox4.Text = "AUX4";
                    checkBox5.Text = "SAO2";
                    checkBox6.Text = "PLETH";
                    checkBox7.Text = "HRATE";
                    checkBox8.Text = "STATUS";
                    checkBox9.Text = "MARKER";
                    checkBox10.Text = "SAW";
                    checkBox11.Visible = false;
                    checkBox12.Visible = false;
                    checkBox13.Visible = false;
                    checkBox14.Visible = false;
                }
                //Show some Mobi properties
                hScrollBar2.Enabled = false;
                textBox14.Text = Mobi.Name;
                textBox11.Text = Mobi.NumberOfChannels.ToString();
                textBox12.Text = Mobi.SerialNumber.ToString();
                textBox13.Text = Mobi.SampleRate.ToString();
                //Swith StartButton type and text
                button2.Enabled = true;
                button1.Text = "Release";
                //Keep track that we've just connected
                contact = true;
                //Show all possible outputs
                foreach (MyMobiClass.MOutput moutout in Mobi.AvailableOutputs)
                {
                    myCheckBoxArray[moutout.portNumber].Enabled = true;
                    myCheckBoxArray[moutout.portNumber].Text = moutout.OutDescriptor;
                }
                button2.Enabled = true;
                groupBox14.Enabled = true;
            }
            else
            {
                //Release Mobi
                Mobi.ReleaseSerialContact();

                //Update view to reflect released Mobi
                button1.Text = "Contact";
                textBox14.Text = "";
                textBox11.Text = "";
                textBox12.Text = "";
                textBox13.Text = "";
                hScrollBar2.Enabled = true;
                contact = false;
                button2.Enabled = false;
                groupBox14.Enabled = false;
            }
        }

        private void hScrollBar2_Scroll_1(object sender, ScrollEventArgs e)
        {
            textBox15.Text = hScrollBar2.Value.ToString();
        }

        private void hScrollBar1_Scroll_1(object sender, ScrollEventArgs e)
        {
            float sr = 2048.0F;
            int divider = 11 - hScrollBar1.Value;
            for (int i = 2; i <= divider; i++)
            {
                sr /= 2.0F;
            }
            Mobi.SampleRate = sr;
            textBox13.Text = Mobi.SampleRate.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string extension;
            if (!measuring)
            {   //Not measuring; start inintialising graphs
                Console.WriteLine("Not measuring; start inintialising graphs");

                //Initialise graphs
                for (int k = 0; k < 14; k++)
                {
                    myXScaleScrollbarArray[k].Enabled = false;
                    sample[k] = 0;
                    myGraphArray[k].ClearGraphs();
                }
                groupBox14.Enabled = false;
                string headerline = "SampleNBR TimeStamp ";
                //Initialise headerlines: only for checked graphs
                for (int j = 0; j < 14; j++)
                {
                    if (myCheckBoxArray[j].Checked)
                    {
                        headerline += myCheckBoxArray[j].Text + " ";
                    }
                }
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("de-DE");

                //initialise filename for output
                fileName = DateTime.Now.ToString("s", ci);
                extension = "_fys.txt";
                fileName = fileName.Replace(':', '_');                                    //replace : with _ to prevent Windows URL problems
                if (textBox18.Text != "")                                                 //show filename prefix
                {
                    prefix = textBox18.Text + "_";
                }
                else
                {
                    prefix = "";
                }
                textBox17.Text = prefix + fileName + extension;                               //show entire filename

                //start writing to output file
                path = defaultOutPath + "\\" + prefix + fileName + extension;
                outputFile = new StreamWriter(path);
                outputFile.WriteLine(headerline);
                outputFile.Close();
                Console.WriteLine("Outputfile geschreven en gesloten");

                //start Mobi
                Mobi.Start();

                TS = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;
                Told = TS;
                SampleNumber = 0;
                //                csn = 0;

                //Change text of Start button to Stop
                button2.Text = "Stop";
                textBox13.Text = Mobi.SampleRate.ToString();                              //Show Sample rate

                int i = Convert.ToInt32(100000.0F / Mobi.SampleRate);//about 100 samples per timer tick
                if (i == 0)
                {
                    timer1.Interval = 1;//minimal 1 ms
                }
                else
                {
                    if (i > 100)
                        timer1.Interval = 100;//maximal 100 ms
                    else
                        timer1.Interval = i;
                }
                //                FirstSample = true;

                //We have started measuring; set some varibales
                measuring = true;
                button1.Enabled = false;
                button3.Enabled = false;
                textBox18.Enabled = false;
                checkBox15.Enabled = false;
                timer1.Enabled = true;
                timer2.Enabled = true;
                timer4.Enabled = true;
            }
            else
            {       //We are measuring: stop measuring!
                Console.WriteLine("We are measuring: stop measuring!");
                for (int i = 0; i < 14; i++)
                {
                    myXScaleScrollbarArray[i].Enabled = true;
                }
                groupBox14.Enabled = true;
                TE = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;//end time

                //Stop the Mobi
                Mobi.Stop();

                try
                {
                    if (output.Count > 0)
                    {
                        outputFile = new StreamWriter(path, true);
                        foreach (string item in output)
                        {
                            outputFile.WriteLine(item);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("konden laatste beetje niet wegschrijven");
                    //throw;
                }



                //Show to reflect stopping the Mobi
                button2.Text = "Start";
                measuring = false;
                button1.Enabled = true;
                button3.Enabled = true;
                textBox18.Enabled = true;
                checkBox15.Enabled = true;
                timer1.Enabled = false;
                timer2.Enabled = false;
                timer4.Enabled = false;
                Console.WriteLine("Nu trachten output file te sluiten");
                outputFile.Close();
                outputFile = null;
                Console.WriteLine("Outputfile geschreven en gesloten");
                textBox17.Text = defaultFileName;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)//read samples from input buffer
        {
            int nsp;
            double dT;
            string format;
            timer1.Enabled = false;
            T = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString()).TotalSeconds;//current raw time
            //Get data from the Mobi
            data = Mobi.GetData(out nsp);//get maximum of 10000 samples per channel
            if (nsp > 0)
            {
                dT = (T - Told) / nsp;
                for (int i = 0; i < nsp; i++)
                {
                    SampleNumber++;
                    double sT = Told + i * dT;
                    //battery test:
                    low = (Convert.ToInt64(data[Mobi.NumberOfChannels - 2 + Mobi.NumberOfChannels * i]) & 2) != 0;
                    empty = (Convert.ToInt64(data[Mobi.NumberOfChannels - 2 + Mobi.NumberOfChannels * i]) & 4) != 0;
                    //sample number and timestamp:
                    string message = SampleNumber.ToString("######## ").PadLeft(9) + sT.ToString("#####.0000 ").PadLeft(11);
                    if (checkBox15.Checked)//voltages
                    {
                        foreach (MyMobiClass.MOutput moutout in Mobi.AvailableOutputs)
                        //prepare file output, ordered output by port number
                        {
                            if (myCheckBoxArray[moutout.portNumber].Checked)
                            //only checked ports will be included in the output
                            {
                                if (moutout.portNumber == Mobi.NumberOfChannels - 2) //digi
                                {
                                    message +=
                                        (Convert.ToInt64(data[moutout.portNumber + Mobi.NumberOfChannels * i]) & 1).
                                            ToString("0 ");
                                }
                                else
                                {
                                    format = "F" + moutout.precision2.ToString("0");
                                    message += (moutout.stepSize * data[moutout.portNumber + Mobi.NumberOfChannels * i]).ToString(format).PadLeft(moutout.precision1 + moutout.precision2 + 1) + " ";
                                }
                            }
                        }
                    }
                    else//integer values
                    {
                        foreach (MyMobiClass.MOutput moutout in Mobi.AvailableOutputs)
                        //prepare file output, ordered output by port number
                        {
                            if (myCheckBoxArray[moutout.portNumber].Checked)
                            //only checked ports will be included in the output
                            {
                                if (moutout.portNumber == Mobi.NumberOfChannels - 2) //digi
                                {
                                    message +=
                                        (Convert.ToInt64(data[moutout.portNumber + Mobi.NumberOfChannels * i]) & 1).
                                            ToString("0") + " ";
                                }
                                else
                                {
                                    //                             format = "0".PadLeft(moutout.precision,'#');
                                    format = "F0";
                                    message += Convert.ToInt64(data[moutout.portNumber + Mobi.NumberOfChannels * i]).ToString(format).PadLeft(moutout.precision + 1) + " ";
                                }
                            }
                        }
                    }

                    output.Add(message);

                    

                    for (int k = 0; k < graphList.Count; k++)//put data in graphs
                    {
                        float svalue;
                        if (graphList[k] == Mobi.NumberOfChannels - 2)//digi
                        {
                            svalue = Convert.ToSingle(Convert.ToInt64(data[graphList[k] + Mobi.NumberOfChannels * i]) & 1);
                        }
                        else
                        {
                            svalue = data[graphList[k] + Mobi.NumberOfChannels * i];
                        }
                        if (checkBox15.Checked)
                        {
                            //                            svalue *= Mobi.AvailableOutputs[graphList[k]].stepSize;
                            svalue *= Mobi.stepSizes[graphList[k]];
                        }
                        if (((sample[k] + i) % subsamplefactor[k]) == 0)
                        {
                            myGraphArray[k].AddValue(graph[k], Convert.ToSingle(sample[k] + i), svalue);
                            lastValueArray[k] = svalue;
                        }
                        myTextBoxArray[k].Text = svalue.ToString();
                    }
                }
                for (int i = 0; i < graphList.Count; i++) sample[i] += nsp;
            }
            for (int i = 0; i < graphList.Count; i++)
            {
                if (sample[i] / subsamplefactor[i] > 999)
                {
                    myGraphArray[i].ClearGraph(graph[i]);
                    sample[i] = 0;
                }
            }
            Told = T;
            timer1.Enabled = true;
        }


        private void timer2_Tick(object sender, EventArgs e)//graphs refresh
        {
            for (int k = 0; k < graphList.Count; k++) myGraphArray[k].DrawAll();
            if (low)
            {
                label14.Text = "Battery low";
                label14.ForeColor = Color.DarkOrange;
                blinking = true;
            }
            if (empty)
            {
                label14.Text = "Battery empty";
                label14.ForeColor = Color.Red;
                blinking = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Mobi.Name == "Mobi8") WriteSettings(8); else WriteSettings(6);
            if (outputFile != null) outputFile.Close();
            if (Mobi != null) Mobi.ReleaseSerialContact();
        }

        private void AlterGraphs(out int ii)
        {
            ii = 0;
            foreach (int i in graphList)
            {
                for (int k = 0; k < Mobi.AvailableOutputs.Count; k++)
                {
                    Mobi.AvailableOutputs[k].graphNumber = -1;//default this output does not have a graph
                    if (Mobi.AvailableOutputs[k].portNumber == i)
                    {

                        if (checkBox15.Checked)
                        {
                            myGraphArray[ii].XtraTitle = Mobi.AvailableOutputs[k].OutDescriptor + " (" +
                                                         Mobi.AvailableOutputs[k].dimension + ")";

                            myGraphArray[ii].XtraYmin = Mobi.AvailableOutputs[k].minValue *
                                                        Mobi.AvailableOutputs[k].stepSize;
                            myGraphArray[ii].XtraYmax = Mobi.AvailableOutputs[k].maxValue *
                                                        Mobi.AvailableOutputs[k].stepSize;
                        }
                        else
                        {
                            myGraphArray[ii].XtraTitle = Mobi.AvailableOutputs[k].OutDescriptor;
                            myGraphArray[ii].XtraYmin = Mobi.AvailableOutputs[k].minValue;
                            myGraphArray[ii].XtraYmax = Mobi.AvailableOutputs[k].maxValue;

                        }

                        YOffset[ii] = (myGraphArray[ii].XtraYmax + myGraphArray[ii].XtraYmin) / 2.0f;
                        YRange[ii] = myGraphArray[ii].XtraYmax - myGraphArray[ii].XtraYmin;
                        myTextBoxArray[ii].Visible = true;
                        Mobi.AvailableOutputs[k].graphNumber = ii;//set the graph number
                        ii++;
                    }
                }
            }
        }

        private void UpdateOutput(object sender, EventArgs e)//add or remove a graph depending on checkbox settings
        {
            int index = Convert.ToInt32(((CheckBox)sender).Name.Split(new char[] { 'x' })[1]) - 1;
            bool check = ((CheckBox)sender).Checked;
            if (check)
            {
                graphList.Add(index);//add output item numbered index
                myGroupBoxArray[graphList.Count - 1].Visible = true;//show next graph
            }
            else
            {
                graphList.Remove(index);//remove the output item from the list
                myGroupBoxArray[graphList.Count].Visible = false; //Hide last graph
            }
            int ii = 0;
            AlterGraphs(out ii);
            tabControl1.SelectTab((ii - 1) / 5);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                defaultOutPath = folderBrowserDialog1.SelectedPath;
                textBox16.Text = defaultOutPath;
            }
        }

        private void ToggleTextBox(object sender, EventArgs e)
        {
            ((TextBox)sender).Visible = !(((TextBox)sender).Visible);
        }

        private void xyGraph_Load(object sender, EventArgs e)
        {
            string name = "Graph" + (graph.Count + 1).ToString();
            graph.Add(((XYGraph)sender).AddGraph(name, System.Drawing.Drawing2D.DashStyle.Solid, Color.Blue, 1, false));
        }

        private void ChangeOffset(object sender, ScrollEventArgs e)
        {
            int graphindex = Convert.ToInt32(((VScrollBar)sender).Name.Split(new char[] { 'r' })[2]) - 1;

            float ymax = 0.0f;
            float ymin = 0.0f;
            float xmax = 0.0f;
            float xmin = 0.0f;
            myGraphArray[graphindex].GetMinMax(ref xmin, ref xmax, ref ymin, ref ymax);
            float offset = (ymax + ymin) / 2.0f;
            float dof = (ymax - ymin) / 2.0f;
            if (e.NewValue > e.OldValue)
            {
                ymax -= dof;
                ymin -= dof;
            }
            else
            {
                if (e.NewValue < e.OldValue)
                {
                    ymax += dof;
                    ymin += dof;
                }
            }
            myGraphArray[graphindex].SetMinMax(xmin, xmax, ymin, ymax);
            myGraphArray[graphindex].DrawAll();
        }

        private void ChangeYScale(object sender, ScrollEventArgs e)
        {
            int graphindex = Convert.ToInt32(((VScrollBar)sender).Name.Split(new char[] { 'r' })[2]) - 1;

            float ymax = 0.0f;
            float ymin = 0.0f;
            float xmax = 0.0f;
            float xmin = 0.0f;
            myGraphArray[graphindex].GetMinMax(ref xmin, ref xmax, ref ymin, ref ymax);
            float offset = (ymax + ymin) / 2.0f;
            float dof = (ymax - ymin) / 2.0f;
            if (e.NewValue > e.OldValue)
            {
                dof /= 1.5f;
            }
            else
            {
                if (e.NewValue < e.OldValue)
                {
                    dof *= 1.5f;
                }
            }
            ymax = offset + dof;
            ymin = offset - dof;
            myGraphArray[graphindex].SetMinMax(xmin, xmax, ymin, ymax);
            myGraphArray[graphindex].DrawAll();
        }

        private void ChangeXScale(object sender, ScrollEventArgs e)
        {
            int graphindex = Convert.ToInt32(((HScrollBar)sender).Name.Split(new char[] { 'r' })[2]) - 1;

            float ymax = 0.0f;
            float ymin = 0.0f;
            float xmax = 0.0f;
            float xmin = 0.0f;
            myGraphArray[graphindex].GetMinMax(ref xmin, ref xmax, ref ymin, ref ymax);
            if (e.NewValue > e.OldValue)
            {
                xmax /= 2.0f;
                subsamplefactor[graphindex] /= 2;
                if (subsamplefactor[graphindex] == 0)
                {
                    subsamplefactor[graphindex] = 1;
                    xmax = 1000;

                }
            }
            else
            {
                if (e.NewValue < e.OldValue)
                {
                    xmax *= 2.0f;
                    subsamplefactor[graphindex] *= 2;
                }
            }
            myGraphArray[graphindex].SetMinMax(xmin, xmax, ymin, ymax);
            myGraphArray[graphindex].DrawAll();

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (blinking) label14.Visible = !label14.Visible;
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            int ii = 0;
            AlterGraphs(out ii);
        }

        private void xyGraph_Click(object sender, EventArgs e)
        {
            float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int i = 0; i < myGraphArray.Length; i++)
            {
                if (sender == myGraphArray[i])
                {
                    myGraphArray[i].GetMinMax(ref x1, ref x2, ref y1, ref y2);
                    float range = y2 - y1;
                    y2 = lastValueArray[i] + range / 2.0f;
                    y1 = lastValueArray[i] - range / 2.0f;
                    myGraphArray[i].SetMinMax(x1, x2, y1, y2);
                }
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            //Schrijf mobi data weg.

            try
            {
                outputFile = new StreamWriter(path, true);
                Console.WriteLine("Outputfile opnieuw gemaakt");

                foreach (string item in output)
                {
                    outputFile.WriteLine(item);
                }

                output.Clear();

                outputFile.Close();
                Console.WriteLine("Outputfile geschreven en weer gesloten");
            }
            catch (Exception)
            {
                
                Console.WriteLine("We mochten niet schrijven, wachten op de volgende keer");
            }
            
            

        }


    }
}
