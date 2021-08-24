using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CPRCANV2Protocol;

namespace CANV2ProtocolDemoClient
{
    public partial class Form1 : Form
    {
        RobotControlLoop mainLoop;
        public Form1()
        {
            InitializeComponent();

            this.Text = "CPRCANV2 Protocol DemoClient V1.0 - Aug. 2021";

            mainLoop = new RobotControlLoop();
            mainLoop.SetOverride(50.0);

            timer1.Interval = 100;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mainLoop.hwInterface.flagConnected)
                labelConnection.Text = "Connection: connected";
            else
                labelConnection.Text = "Connection: not connected";

            double jPosSetPoint = 0.0;
            double jPosCurrent = 0.0;
            double jMotorCurrent = 0.0;
            int jErrorCode = 0;
            string jErrorCodeString = "na";
            mainLoop.GetJointValues(ref jPosSetPoint, ref jPosCurrent, ref jMotorCurrent, ref jErrorCode, ref jErrorCodeString);

            labelJointStatus.Text = "Status: " + jErrorCodeString + " (" + jErrorCode.ToString() + ")";
            labelPosSetPoint.Text = "SetPointPosition: " + jPosSetPoint.ToString("0.0") + "°";
            labelPosition.Text = "CurrentPosition: " + jPosCurrent.ToString("0.0") + "°";
            labelMotorCurrent.Text = "MotorCurrent: " + jMotorCurrent.ToString("0.0") + " mA";

        }

        


        private void buttonConnect_Click(object sender, EventArgs e)
        {
            mainLoop.hwInterface.Connect();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            mainLoop.ResetJoint();
        }

        private void buttonEnable_Click(object sender, EventArgs e)
        {
            mainLoop.EnableJoint();
        }

        private void buttonSetToZero_Click(object sender, EventArgs e)
        {
            mainLoop.hwInterface.SetJointsToZero();

        }

        private void buttonStartReferencing_Click(object sender, EventArgs e)
        {

        }




        private void buttonStop_Click(object sender, EventArgs e)
        {
            mainLoop.SetJogValue(0.0);
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            double jv = mainLoop.GetJogValue();
            jv += 10.0;
            if (jv > 100.0) jv = 100.0;
            mainLoop.SetJogValue(jv);
        }

        private void buttonBackwards_Click(object sender, EventArgs e)
        {
            double jv = mainLoop.GetJogValue();
            jv -= 10.0;
            if (jv < -100.0) jv = -100.0;
            mainLoop.SetJogValue(jv);
        }

        
    }
}
