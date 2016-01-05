/*
MobiHTI is an application for reading physiological data using the TMSI Mobi-6 device.
Author: Martin C. Boschman
Copyright: Technische Universiteit Eindhoven, Human Technology Interaction
Date: 13 March 2008
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PORTISERIALLib;// dll van TMSI
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace MobiHTI
{
    public class MyMobiClass
    {
        MyMobiClass Mobi;
        public class MOutput
        {
            public string OutDescriptor;
            public string OutName;
            public int portNumber;
            public int graphNumber;
            public float minValue;
            public float maxValue;
            public float stepSize;
            public string dimension;
            public int precision;
            public int precision1;
            public int precision2;
        }

        public float[] stepSizes;

        private List<MOutput> _availableOutputs=new List<MOutput>();
	    public List<MOutput> AvailableOutputs
	    {
		    get { return _availableOutputs; }
		    set { _availableOutputs = value; }
	    }
        private SerialSourceClass _serialSource1;

        private int _comPort;//Serial port number for the bluetooth connection
        public int ComPort
        {
            get { return _comPort; }
            set { 
               if (value<1 || value>50)
               {
                   MessageBox.Show("Port number out of range 1..50");
               }
               else
               {
                   _comPort = value;
               }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { MessageBox.Show("Readonly Property"); }
        }

        private int _serialNumber;//Property SerialNumber
        public  int SerialNumber
        {
            get { return _serialNumber; }
            set { MessageBox.Show("Readonly Property");}
        }

        private int _numberOfChannels;//Property NumberOfChannels
        public int NumberOfChannels
        {
            get { return _numberOfChannels; }
            set { MessageBox.Show("Readonly Property"); }
        }

        private float _sampleRate;//Property SampleRate
        public float SampleRate
        {
            get { return _sampleRate; }
            set 
            { 
                _sampleRate = value;
                _serialSource1.SampleRate = value;
            }
        }

        public MyMobiClass()//Default constructor of this class
        {

            Initialize(4);//default COM4
        }

        public MyMobiClass(int comport)//Constructor with comport argument
        {
            Initialize(comport);
        }

        public void Initialize(int comport)
        {
            string fname;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US",true);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", true);
            _comPort = comport;
            _serialSource1 = new SerialSourceClass();
            _serialSource1.ComPort = _comPort.ToString();
            _serialNumber = _serialSource1.FrontendSerialNumber;
            if (_serialNumber == -1)//cannot open connection to Mobi
            {
                MessageBox.Show("Cannot open connection to Mobi");
                return;
            }
            else
            {
                //   MessageBox.Show("Mobi connected");
            }
            _serialSource1.GetFrontendNrOfChannels(out _numberOfChannels);
             stepSizes=new float[_numberOfChannels];
            _sampleRate = _serialSource1.SampleRate;
            _name = _serialSource1.Caption;
            if (_name=="Mobi8")
            {
                fname = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MobiHTI\\mobi8.info";
            }
            else
            {
                fname = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MobiHTI\\mobi6.info";
            }
            try// now read the info from Mobi.info
            {
                StreamReader info = new StreamReader(fname);
                for (int i=0; i<_numberOfChannels;i++)
                {
                    char[] delimiters={' ','\t'};
                    string s = info.ReadLine();
                    string[] sa=s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    MOutput mout=new MOutput();
                    if (sa.Length==6)//this output is available, add it to the list
                    {
                        mout.portNumber=i;
                        mout.OutName=sa[0];
                        mout.OutDescriptor=sa[1];
                        mout.minValue=Convert.ToSingle(sa[2]);
                        string[] spl = sa[2].Split(new char[] {'.', ','},StringSplitOptions.RemoveEmptyEntries);
                        mout.precision = spl[0].Length;
                        mout.maxValue=Convert.ToSingle(sa[3]);
                        mout.stepSize = Convert.ToSingle(sa[4]);
                        stepSizes[i] = mout.stepSize;
                        mout.precision1 =
                            (mout.minValue*mout.stepSize).ToString().Split(new char[] {'.', ','},
                                 StringSplitOptions.RemoveEmptyEntries)[0].Length;
                        mout.precision2 = sa[4].Length - 2;
                        if (mout.precision2 < 0) mout.precision2 = 0;
                        mout.dimension = sa[5];
                        mout.graphNumber = -1;//for now
                        _availableOutputs.Add(mout);
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString()+"\nError reading Mobi.info file");
                return;
            }
        }

        public void KeepSerialContact()
        {
            int error;
            _serialSource1.KeepSerialPortInUse(0,out error);
            if (error==-1)
            {
                MessageBox.Show("Error returned in KeepSerialContact");
            }
        }

        public void ReleaseSerialContact()
        {
            int error;
            _serialSource1.ReleaseSerialPort(out error);
            if (error == -1)
            {
                MessageBox.Show("Error returned in ReleaseSerialContact");
            }
        }

        public void Start() //start data acquisition
        {
            int error;
            float freq=_sampleRate;
            _serialSource1.StartAcq(ref freq, out error);
            if (error == -1)
            {
                MessageBox.Show("Error returned in Start");
            }

            //Continueing on, despite possible error ?!
            _sampleRate = _serialSource1.SampleRate;//new value
        }

        public void Stop()//stop data acquisition
        {
            int error;
            _serialSource1.StopAcq(out error);
            if (error == -1)
            {
                MessageBox.Show("Error returned in Start");
            }
        }

        public float[] GetData(out int nbrperiods)
        {
            float[] temp=new float[40000];
            nbrperiods = 10000;

            try
            {
                _serialSource1.GetStationairSampleRecords(ref nbrperiods, ref temp[0]);
            }
            catch (SystemException ex) {
                MessageBox.Show(ex.ToString());
            }
            return temp;
        }

        public void StartStopMobi(Boolean contact, int COMport)
        {   //pre: contact V !contact
            //post: if '!contact: contact` = true, Mobi Object initialised   
            //      if `contact: contact` = false Mobi Object released
            Boolean result = contact;

            if (!contact)
            {
                KeepSerialContact();
                result = true;
            }
            else
            {
                ReleaseSerialContact();
                result = false;
            }

            //return result;
        }
    
    }



}
