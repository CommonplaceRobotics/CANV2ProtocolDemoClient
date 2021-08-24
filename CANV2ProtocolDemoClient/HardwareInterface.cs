

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/// <summary>
/// Inclusion of PEAK PCAN-Basic namespace
/// </summary>
using Peak.Can.Basic;
using TPCANHandle = System.Byte;


namespace CPRCANV2Protocol
{
    /// <summary>
    /// This class provides access to the CAN bus using the PCAN-USB adapter by Peak-System
    /// The software works for both Mover4 and Mover6, but in Joint4 there are slightly different gear scales for Mover4 and Mover6, see WriteJointSetPoints() 
    /// </summary>
    class HardwareInterface
    {
        private TPCANHandle m_PcanHandle = 81;                                  // Handle for PCAN hardware
        private TPCANBaudrate m_Baudrate = TPCANBaudrate.PCAN_BAUD_500K;        // Baudrate 500k for Mover robot
        private TPCANType m_HwType = TPCANType.PCAN_TYPE_ISA;                   // Type of hardware

        public bool flagConnected = false;
        public bool flagStopSendingPositions = false;
        private int timeStamp = 0;

        uint messageID = 0x10;
            

        public HardwareInterface()
        {
        }



        //***********************************************************************
        /// <summary>
        /// Connects to the PCAN USB adapter
        /// </summary>
        /// Only minimal error handling is implemented, see the Pead-Systeme support pages
        public void Connect(){

            TPCANStatus stsResult;
            try
            {
                // Connects a selected PCAN-Basic channel
                stsResult = PCANBasic.Initialize(m_PcanHandle, m_Baudrate, m_HwType, 100, 3);

                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    flagConnected = false;
                    System.Windows.Forms.MessageBox.Show("Error: Cannot connect to PCAN USB interface");
                }
                else
                {
                    flagConnected = true;
                }

            }
            catch (Exception e)
            {
                flagConnected = false;
                string msg = "Error: Cannot connect to PCAN USB interface: " + System.Environment.NewLine + e.Message;
                System.Windows.Forms.MessageBox.Show(msg);
            }

        }



        //********************************************************************************
        /// <summary>
        /// Writes the current joint set points to the CAN bus and reads the answers
        /// This function has to be calles frequently. When there are interruptsion the joint electronic will
        /// interprete this as a failure in the control and change into error state (comm watch dog)
        /// </summary>
        /// <param name="jointsSetPoint">set point values in degree</param>
        /// <param name="jointsCurrent">the current joint values read from the hardware in degree</param>
        /// <param name="errorCodes">the joint error codes, see file start for explanation</param>
        /// <param name="digitalIn">the digital input values of the joint modules</param>
        public void WriteJointSetPoints(double jointSetPoint, int digitalOut, ref double jointCurrent, ref int errorCode, ref string errorCodeString, ref double motorCurrent, ref int digitalIn)
        {
            double gearZero = 0;
            double gearScale = 1031.11;             // scale for iugs Rebel joint   

            if (flagStopSendingPositions)
                return;

            timeStamp = (timeStamp+1) % 256;

            double tmpPos = 0.0;
            TPCANStatus stsResult;
            TPCANTimestamp CANTimeStamp;
                        

            lock (this)
            {
                try
                {
                    TPCANMsg CANMsg = new TPCANMsg();
                    CANMsg.LEN = (byte)8;
                    CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

                    // write the setPoint command
                    // CPRCANV2 protocol:
                    // 0x14 vel pos0 pos1 pos2 pos3 timer dout
                    tmpPos = gearZero + jointSetPoint * gearScale;                  // generate the setPoint in encoder tics
                    Int32 mPos = (Int32)tmpPos;
                    CANMsg.ID = messageID;                                          // the CAN ID of the joint
                    CANMsg.DATA = new byte[8];
                    CANMsg.DATA[0] = 0x14;                                          // first byte denominates the command, here: set joint position
                    CANMsg.DATA[1] = (byte)0x00;                                    // velocity, not used
                    CANMsg.DATA[5] = (byte)(mPos & 0xFF);                           // SetPoint Position, pay attention to the indices 
                    CANMsg.DATA[4] = (byte)((mPos >> 8) & 0xFF);
                    CANMsg.DATA[3] = (byte)((mPos >> 16) & 0xFF);
                    CANMsg.DATA[2] = (byte)((mPos >> 24) & 0xFF);
                    CANMsg.DATA[6] = (byte)timeStamp;                               // Time stamp (not used)
                    CANMsg.DATA[7] = (byte)digitalOut;                              // Digital our for this module, binary coded
                                                                                    

                    stsResult = PCANBasic.Write(m_PcanHandle, ref CANMsg);          // write to the CAN bus
                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    {
                        errorCode = 0x100;      // cannot write, set errorCode to "bus dead"
                        throw (new Exception("PCAN: Cannot write: "));
                    }

                    //wait a short time
                    System.Threading.Thread.Sleep(2);

                    //read the answer
                    // please be aware: this is only a demo implementation!
                    // for a real implementation all incoming messages have to be handeled!
                    // This implementation misses some messages...
                    stsResult = PCANBasic.Read(m_PcanHandle, out CANMsg, out CANTimeStamp);
                    if (CANMsg.ID == (messageID + 1))                               // the answer is on CAN-ID+1
                    {
                        // CPRCANV2 protocol of the answer:
                        // err pos0 pos1 pos2 pos3 currentH currentL din 
                        errorCode = (int)CANMsg.DATA[0];
                        int pos = CANMsg.DATA[1] * 256 * 65536 + CANMsg.DATA[2] * 65536 + CANMsg.DATA[3] * 256 + CANMsg.DATA[4];
                        jointCurrent = (pos - gearZero) / gearScale;
                        motorCurrent = (double)CANMsg.DATA[6];
                        digitalIn = (int)CANMsg.DATA[7];

                    }
                    else
                    {
                        errorCode = 0x200;  // no answer from the module, set errorCode to "dead"
                    }

                }
                catch (Exception ex)
                {
                    ;
                }
                              

                DecodeErrorCode(errorCode, ref errorCodeString);

            }          // end of lock(this)

        }


        //**********************************************************************************
        /// <summary>
        /// Resets the errors of all joint modules. Error will be 0x04 afterwards (motors not enabled)
        /// You need to enable the motors afterwards to get the robot in running state (0x00)
        /// </summary>
        public void ResetErrors()
        {
            // Protocol: 0x01 0x06 
            TPCANMsg CANMsg = new TPCANMsg();
            CANMsg.DATA = new byte[8];
            CANMsg.LEN = (byte)2;
            CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            CANMsg.DATA[0] = 0x01;
            CANMsg.DATA[1] = 0x06;

            flagStopSendingPositions = true;
            
            CANMsg.ID = messageID;
            PCANBasic.Write(m_PcanHandle, ref CANMsg);
            System.Threading.Thread.Sleep(5);

            flagStopSendingPositions = false;

        }


        //**********************************************************************************
        /// <summary>
        /// Enables the motors, joint error state is 0x00 afterwards. 
        /// </summary>
        public void EnableMotors()
        {
            // Protocol: 0x01 0x09 to enable a joint
            //           0x01 0x0A to disable a joint
            TPCANMsg CANMsg = new TPCANMsg();
            CANMsg.DATA = new byte[8];
            CANMsg.LEN = (byte)2;
            CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            CANMsg.DATA[0] = 0x01;
            CANMsg.DATA[1] = 0x09;

            flagStopSendingPositions = true;
            
            CANMsg.ID = messageID;
            PCANBasic.Write(m_PcanHandle, ref CANMsg);
            System.Threading.Thread.Sleep(5);

            flagStopSendingPositions = false;


        }

        //**********************************************************************************
        /// <summary>
        /// Sets all joint modules to zero position (0x7D00)
        /// </summary>
        public void SetJointsToZero()
        {
            // Protokoll: 0x01 0x08 PosHigh PosLow
            TPCANMsg CANMsg = new TPCANMsg();
            CANMsg.DATA = new byte[8];
            CANMsg.LEN = (byte)4;
            CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            CANMsg.DATA[0] = 0x01;
            CANMsg.DATA[1] = 0x08;
            CANMsg.DATA[2] = 0x7D;
            CANMsg.DATA[3] = 0x00;

            flagStopSendingPositions = true;

            CANMsg.ID = messageID;
            PCANBasic.Write(m_PcanHandle, ref CANMsg);          // has to be send twice to take effect; measure to avoid unwanted reset
            System.Threading.Thread.Sleep(1);
            PCANBasic.Write(m_PcanHandle, ref CANMsg);
            System.Threading.Thread.Sleep(5);                   // wait for a short moment especially to allow Joint4 to catch up

            flagStopSendingPositions = false;
        }


        //*********************************************************
        // Setting a digital Output channel
        // Each joint module has (theoretical) four output channels
        // Physically connected are the four channels of joint 0 (as D-Out via the D-Sub connector),
        // and two channels of joint 3 (to command the gripper)
        // It is also possible to add the D-Out state as byte 6 to the position command, to increase reliability
        public void SetDigitalOut(int channel, bool state){

            // Protokoll: 0x01 0x20 (or 0x21 0x22, 0x23)
            TPCANMsg CANMsg = new TPCANMsg();
            CANMsg.DATA = new byte[8];
            CANMsg.LEN = (byte)3;
            CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            CANMsg.DATA[0] = 0x01;
            CANMsg.DATA[1] = 0x20;
            CANMsg.DATA[2] = 0x00;

            if (channel == 0)
                CANMsg.DATA[1] = 0x20;
            else if (channel == 1)
                CANMsg.DATA[1] = 0x21;
            else if (channel == 2)
                CANMsg.DATA[1] = 0x22;
            else if (channel == 3)    
                CANMsg.DATA[1] = 0x23;

            if (state) CANMsg.DATA[2] = 0x01;
            else CANMsg.DATA[2] = 0x00;
          
            flagStopSendingPositions = true;
            CANMsg.ID = messageID;
            PCANBasic.Write(m_PcanHandle, ref CANMsg);          
            flagStopSendingPositions = false;
                        
        }


        //**************************************************************************
        private void DecodeErrorCode(int code, ref string shortStatus)
        {
            string s = "";
            string ecshort = "";
            if ((code & 0x01) != 0)
            {                  // bit 1 
                s += " - EStop / temperature ";
                ecshort += " EStop/TEMP";
            }
            if ((code & 0x02) != 0)
            {             // bit 2 
                s += " - Driver Error";
                ecshort += " DRV";
            }
            if ((code & 0x04) != 0)
            {          // bit 3 Motor not enabled (not a real error)
                s += " - motor not enabled";
                ecshort += " MNE";
            }
            if ((code & 0x08) != 0)
            {          // bit 4 CommWatchDog
                s += " - comm watch dog";
                ecshort += " COM";
            }
            if ((code & 0x10) != 0)
            {          // bit 5 Schleppfehler
                s += " - position lag";
                ecshort += " LAG";
            }
            if ((code & 0x20) != 0)
            {          // bit 6 Encoderfehler
                s += " - encoder error";
                ecshort += " ENC";
            }
            if ((code & 0x40) != 0)
            {          // bit 7 OverCurrent
                s += " - over current";
                ecshort += " OC";
            }
            if ((code & 0x80) != 0)
            {          // bit 8 SVM beim Torque-Motor
                s += " - driver error";
                ecshort += " DRV";
            }

            // und die lokalen Fehlerwerte
            if ((code & 0x100) != 0)
            {          // bit 9 bus tot
                s += " - bus dead";
                ecshort += " BUS";
            }
            if ((code & 0x200) != 0)
            {          // bit 10 module tot
                s += " - module dead";
                ecshort += " DEAD";
            }


            if (s.Equals(""))
            {
                s = "no error";
                ecshort = "No Error";
                
            }

            shortStatus = ecshort;
        }

    }
}
