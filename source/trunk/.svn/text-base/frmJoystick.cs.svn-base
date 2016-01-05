/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Globalization;
using System.Windows.Forms;
using SlimDX;
using SlimDX.DirectInput;
using System.Collections.Generic;
using MobiHTI;
using System.IO;

namespace XNAPacMan
{
    public partial class DualShock3 : Form
    {
        Joystick joystick;

        JoystickState state = new JoystickState();
        public DualShock3 staticJoystick = Constants.staticJoystick;
        int numPOVs;
        int SliderCount;
        string strButton = null;
        int inU, inD, inL, inR;

        //Define Vars for use with the Mobi
        public MobiHTI.MyMobiClass Mobi;
        public Boolean measuring = false;
        public Boolean contact = false;

        //Extra values
        public static float[] data;

        void CreateDevice()
        {
            #region Create JoyStick Device
            // make sure that DirectInput has been initialized
            DirectInput dinput = new DirectInput();

            // search for devices
            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                // create the device
                try
                {
                    joystick = new Joystick(dinput, device.InstanceGuid);
                    joystick.SetCooperativeLevel(this, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
                    break;
                }
                catch (DirectInputException)
                {
                }
            }

            if (joystick == null)
            {
                MessageBox.Show("There are no joysticks attached to the system.");
                return;
            }

            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

                UpdateControl(deviceObject);
            }

            // acquire the device
            joystick.Acquire();

            // set the timer to go off 12 times a second to read input
            // NOTE: Normally applications would read this much faster.
            // This rate is for demonstration purposes only.
            timer.Interval = 1000 / 1000;
            timer.Enabled = true;
            timer.Start();
#endregion


        }

        public void ReadImmediateData()
        {
            if (joystick!=null)
            try
            {
                if (joystick.Acquire().IsFailure)
                    return;

                if (joystick.Poll().IsFailure)
                    return;

                state = joystick.GetCurrentState();
                if (Result.Last.IsFailure)
                    return;

                UpdateUI();
            }
            catch (Exception)
            {
                Console.WriteLine("Joystick error");
            }
        }

        public float[] ReadMobi()
        {
            float[] result = null;

            if (timer1.Enabled)
            {
                result = data;
            }


            return result;
        }

        void ReleaseDevice()
        {
            timer.Stop();
            timer1.Stop();

            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            joystick = null;

            if (Mobi != null)
            {
                Mobi.Stop();
                Mobi.ReleaseSerialContact();
                measuring = false;
            }
        }

        #region Boilerplate

        public DualShock3()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void timer_Tick_1(object sender, EventArgs e)
        {
            ReadImmediateData();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            ReleaseDevice();
            Close();
        }

        void UpdateUI()
        {
            strButton = null;

            label_X.Text = state.X.ToString(CultureInfo.CurrentCulture);
            label_Y.Text = state.Y.ToString(CultureInfo.CurrentCulture);
            label_Z.Text = state.Z.ToString(CultureInfo.CurrentCulture);

            label_XRot.Text = state.RotationX.ToString(CultureInfo.CurrentCulture);
            label_YRot.Text = state.RotationY.ToString(CultureInfo.CurrentCulture);
            label_ZRot.Text = state.RotationZ.ToString(CultureInfo.CurrentCulture);

            int[] slider = state.GetSliders();

            label_S0.Text = slider[0].ToString(CultureInfo.CurrentCulture);
            label_S1.Text = slider[1].ToString(CultureInfo.CurrentCulture);

            int[] pov = state.GetPointOfViewControllers();

            label_P0.Text = pov[0].ToString(CultureInfo.CurrentCulture);
            label_P1.Text = pov[1].ToString(CultureInfo.CurrentCulture);
            label_P2.Text = pov[2].ToString(CultureInfo.CurrentCulture);
            label_P3.Text = pov[3].ToString(CultureInfo.CurrentCulture);

            bool[] buttons = state.GetButtons();

            for (int b = 0; b < buttons.Length; b++)
            {
                if (buttons[b])
                    strButton += b.ToString("00 ", CultureInfo.CurrentCulture);
            }
            label_ButtonList.Text = strButton;
            inU = state.RotationX;
            inD = state.RotationY;
            inL = state.RotationZ;
            inR = slider[1];
        }

        public string GetButton() { return strButton; }
        public void RemoveButton() { strButton = ""; }

        public int GetIntensityUp { get { return inU; } }
        public int GetIntensityDown { get { return inD; } }
        public int GetIntensityLeft { get { return inL; } }
        public int GetIntensityRight { get { return inR; } }
        public int[] GetIntensity
        {
            get
            {
                int[] t = {inU, inD, inL, inR};
                return t;
            }
        }

        public int getAxis(int axisIndex)
        {
            return 3;
        }

        void UpdateControl(DeviceObjectInstance d)
        {
            if (ObjectGuid.XAxis == d.ObjectTypeGuid)
            {
                label_XAxis.Enabled = true;
                label_X.Enabled = true;
            }
            if (ObjectGuid.YAxis == d.ObjectTypeGuid)
            {
                label_YAxis.Enabled = true;
                label_Y.Enabled = true;
            }
            if (ObjectGuid.ZAxis == d.ObjectTypeGuid)
            {
                label_ZAxis.Enabled = true;
                label_Z.Enabled = true;
            }
            if (ObjectGuid.RotationalXAxis == d.ObjectTypeGuid)
            {
                label_XRotation.Enabled = true;
                label_XRot.Enabled = true;
            }
            if (ObjectGuid.RotationalYAxis == d.ObjectTypeGuid)
            {
                label_YRotation.Enabled = true;
                label_YRot.Enabled = true;
            }
            if (ObjectGuid.RotationalZAxis == d.ObjectTypeGuid)
            {
                label_ZRotation.Enabled = true;
                label_ZRot.Enabled = true;
            }

            if (ObjectGuid.Slider == d.ObjectTypeGuid)
            {
                switch (SliderCount++)
                {
                    case 0:
                        label_Slider0.Enabled = true;
                        label_S0.Enabled = true;
                        break;

                    case 1:
                        label_Slider1.Enabled = true;
                        label_S1.Enabled = true;
                        break;
                }
            }

            if (ObjectGuid.PovController == d.ObjectTypeGuid)
            {
                switch (numPOVs++)
                {
                    case 0:
                        label_POV0.Enabled = true;
                        label_P0.Enabled = true;
                        break;

                    case 1:
                        label_POV1.Enabled = true;
                        label_P1.Enabled = true;
                        break;

                    case 2:
                        label_POV2.Enabled = true;
                        label_P2.Enabled = true;
                        break;

                    case 3:
                        label_POV3.Enabled = true;
                        label_P3.Enabled = true;
                        break;
                }
            }
        }

        private void createDeviceButton_Click(object sender, EventArgs e)
        {
            if (joystick == null)
                CreateDevice();
            else
                ReleaseDevice();
            UpdateUI();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseDevice();
        }

        #endregion

        private void DualShock3_Load(object sender, EventArgs e)
        {
            // One instance only
            if (staticJoystick == null)
            {
                staticJoystick = this;
                Constants.staticJoystick = staticJoystick;
            }
            try
            {
                if (joystick == null)
                {
                    CreateDevice();
                }
                else
                    ReleaseDevice();
                UpdateUI();
            }
            catch (Exception ee)
            {
                Console.WriteLine("Could not initialize joystick, error: " + ee);
            }
        }


        private void btnVideo_Click(object sender, EventArgs e)
        {
            Constants.vidstarted = DateTime.Now;
            string relaxfile = Constants.profilepath + "relax.mpg";
            if (File.Exists(relaxfile))
            {
                System.Diagnostics.Process.Start(relaxfile,"--fullscreen");
            } else {
                Console.WriteLine(relaxfile + " does not exist!");
            }

            // Only allow to play the video once
            btnVideo.Enabled = false;
        }

        private void btnSoundcheck_Click(object sender, EventArgs e)
        {
            Constants.Soundcheck = true;
        }
    }
}