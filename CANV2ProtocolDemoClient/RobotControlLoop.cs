
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Multimedia;       // necessary for high precision timer

namespace CPRCANV2Protocol
{
    class RobotControlLoop
    {

        private Multimedia.Timer robotMainLoop;
        private double cycleTime = 50;                          // the robot main loop runs with 20 Hz

        private double  jogValue = 0.0;                     // the commanded velocities from 0.0 to 100.0
        private double  robOverride = 30.0;                      // from 0 to 100, scales the movion velocity
        private double   jointPositionSetPoint = 0.0;        // The joint set point values in degree 
        private double   jointPositionCurrent = 0.0;         // the current joint positions in degree, loaded from the robot arm
        private int      jointErrorCode = 0;                  // the error state of each joint module
        private string   jointErrorCodeString = "na";
        private double   jointMotorCurrent = 0.0;
        private bool[]  dout = new bool[7];                      // the wanted digital out channels
        private bool[]  din = new bool[7];                       // the current digital int channels
        public HardwareInterface hwInterface;                   // the USB adapter interface


        //************* Constructor *********************************
        public RobotControlLoop()
        {
            for (int i = 0; i < 6; i++)
            {
                dout[i] = false;
            }
            for (int i = 0; i < 4; i++)
            {
                din[i] = false;
            }

            hwInterface = new HardwareInterface();

            // start the main loop
            TimerCaps caps = Multimedia.Timer.Capabilities;
            this.robotMainLoop = new Multimedia.Timer();
            this.robotMainLoop.Tick += new System.EventHandler(this.LoopMain);
            this.robotMainLoop.Mode = TimerMode.Periodic;
            this.robotMainLoop.Period = (int)cycleTime;
            this.robotMainLoop.Resolution = 1;
            
             this.robotMainLoop.Start();
        }



        //********************* Main Control Loop ***************************************************************************
        /// <summary>
        /// This is the 20Hz robot main loop, called by the multimedia-timer.
        /// In this loop we generate new joint set point values and forward them to the hardware interface
        /// </summary>
        public void LoopMain(object sender, System.EventArgs ev)
        {
            double jointMaxVelocity = 45.0;         // degree / sec
            int tmpDOut = 0;
            int tmpDIn = 0;

            lock (this)
            {

                // Generate new joint setpoints based on the old ones, the jog values and the override
                jointPositionSetPoint += (jogValue / 100.0) * (robOverride / 100.0) * (cycleTime / 1000.0) * jointMaxVelocity;       // vel ist in °/s
                
                // Forward the set point values to the hardware interface. This writes the values to the CAN field bus
                hwInterface.WriteJointSetPoints(jointPositionSetPoint, tmpDOut, ref jointPositionCurrent, ref jointErrorCode, ref jointErrorCodeString, ref jointMotorCurrent, ref tmpDIn);

                // The digital input values are coded in one int per joint. Each bit represents an input channel.
                // For the Mover4 and Mover6 robots only the first joint provides connected digital inputs
                if((tmpDIn & 0x01) == 0x01) din[0] = true; else din[0] = false;
                if ((tmpDIn & 0x02) == 0x02) din[1] = true; else din[1] = false;
                if ((tmpDIn & 0x04) == 0x04) din[2] = true; else din[2] = false;
                if ((tmpDIn & 0x08) == 0x08) din[3] = true; else din[3] = false;
                

            }
        }


        //***************************************************************
        /// <summary>
        /// Copies the current hardware position into the setpoint values and then resets the joint modules
        /// Error code afterwards is 0x04 motor not enabled. To move the robot arm the motors have to be enabled.
        /// </summary>
        public void ResetJoints()
        {
            this.SetJogValue(0);

            lock (this)
            {
                 // first copy the hardware positions to the setpoint positions
                 jointPositionSetPoint = jointPositionCurrent;

                // then reset the joints to state 0x04
                hwInterface.ResetErrors();
            }
        }


        //***************************************************************
        /// <summary>
        /// Override from 0 to 100
        /// </summary>
        public void SetOverride(double ovr)
        {
            lock (this)
            {
                robOverride = ovr;
            }
        }
        //***************************************************************
        public double GetOverride()
        {
             return robOverride; 
        }

        //***************************************************************
        /// <summary>
        /// jog value from -100 to 100
        /// </summary>
        public void SetJogValue(double jv)
        {
            lock (this)
            {
                jogValue = jv;
            }
        }
        //***************************************************************
        /// <summary>
        /// jog value from -100 to 100
        /// </summary>
        public double GetJogValue()
        {
            return jogValue;
        }

        //***************************************************************
        /// <summary>
        /// Sets the joints current value to zero. 
        /// The joint is in error mode afterwards (position lag)
        /// </summary>
        public void SetToZero(double jv)
        {
            ResetJoints();

            lock (this)
            {
                hwInterface.SetJointsToZero();
            }
        }

        //**************************************************************
        /// <summary>
        /// Provides the joint values in degree, each six values
        /// </summary>
        public void GetJointValues(ref double jPosSetPoint, ref double jPosCurrent, ref double jMotorCurrent, ref int errorCode, ref string errorCodeString)
        {
            lock (this)
            {
                jPosSetPoint = this.jointPositionSetPoint;
                jPosCurrent = this.jointPositionCurrent;
                jMotorCurrent = this.jointMotorCurrent;
                errorCode = this.jointErrorCode;
                errorCodeString = this.jointErrorCodeString;
            }
        }

        //***************************************************************
        /// <summary>
        /// Get the digital input values
        /// Returns an array of four bool values 
        /// </summary>
        /// <returns></returns>
        public bool[] GetDigitalIn()
        {
            return din;
        }

        //***************************************************************
        /// <summary>
        /// Sets the digital outputs in the base joint, or the TCP module
        /// </summary>
        /// <param name="dOutParameter">Array of six bool values</param>
        public void SetDigitalOut(bool[] dOutParameter)
        {
            for(int i=0; i<4; i++){
                if(dout[i] != dOutParameter[i]){
                    
                    dout[i] = dOutParameter[i];
                    hwInterface.SetDigitalOut(0, dout[i]); 

                }  //endofif
            } //endoffor
        }



    }
}
