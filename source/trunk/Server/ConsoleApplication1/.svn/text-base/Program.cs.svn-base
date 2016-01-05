using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerApplication
{
    class MobiServer
    {
        //Variables
        static int COMport = 3;

        static void Connect()
        { 
        
        }

        static void Start()
        { }
        

        static void ReadSettings()
        {
            // Read all settings like the COMport of the Mobi. If no settings file can be found, make one.

            const string fileName = "settings.txt";
            List<string> settings;
            settings = new List<string>();
            if (File.Exists(fileName))
            {
                settings = File.ReadAllLines(fileName).ToList<string>();
                COMport = Convert.ToInt32(settings[0]);
            }
            else
            {
                File.WriteAllLines(fileName, new string[] { "3" });
            }

            //return COMport;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ReadSettings();

            Console.ReadLine();
        }
    }
}
