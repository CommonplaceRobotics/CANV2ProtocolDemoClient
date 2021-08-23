/*********************************************************************
 * Software License Agreement (BSD License)
 *
 *  Copyright (c) 2012-15, Commonplace Robotics GmbH
 *  http://www.commonplacerobotics.com
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *   * Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *   * Redistributions in binary form must reproduce the above
 *     copyright notice, this list of conditions and the following
 *     disclaimer in the documentation and/or other materials provided
 *     with the distribution.
 *   * Neither the name Commonplace Robotics nor the names of its
 *     contributors may be used to endorse or promote products derived
 *     from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 *  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 *  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 *  BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 *  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 *  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 *  LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 *  ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 *  POSSIBILITY OF SUCH DAMAGE.
 *********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Composition of the status byte in the answer from the joints:
// 0x0C: motors not enabled, comm watch dog - this can be a starting value
// 0x04: motors not enabled, but all errors reset - this is the value after pressing the reset button
// 0x00: everything fine, motor enabled - this is the value after pressing the enable button. Now the joints will move

// bit 1 BrownOut or WatchDog
// bit 2 Velocity lag
// bit 3 Motor not enabled
// bit 4 CommWatchDog
// bit 5 Position lag
// bit 6 Encoder error
// bit 7 Over current
// bit 8 CAN Error


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
        public void WriteJointSetPoints(double jointSetPoint, ref double jointCurrent, ref int errorCode, ref int digitalIn)
        {
            double gearZero = 0;
            double gearScale = 65.87;            

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
                    CANMsg.LEN = (byte)5;
                    CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

                    // write the setPoint command
                    tmpPos = gearZero + jointSetPoint * gearScale;        // generate the setPoint in encoder tics
                    CANMsg.ID = messageID;                                      // the CAN ID of the joint
                    CANMsg.DATA = new byte[8];
                    CANMsg.DATA[0] = 0x04;                                          // first byte denominates the command, here: set joint position
                    CANMsg.DATA[1] = (byte)0x00;                                    // velocity, not used
                    CANMsg.DATA[2] = (byte)(tmpPos / 256);                          // SetPoint Position high byte
                    CANMsg.DATA[3] = (byte)(tmpPos % 256);                          // SetPoint Position low byte
                    CANMsg.DATA[4] = (byte)timeStamp;                               // Time stamp, can be found in the answer
                    //CANMsg.DATA[5] = (byte)dOut[i];                               // Extension: The digital out state (not working in older firmware)
                                                                                    // LEN has to be set to 6

                    stsResult = PCANBasic.Write(m_PcanHandle, ref CANMsg);          // write to the CAN bus
                }
                catch (Exception ex)
                {
                    ;
                }

                try
                {
                    //wait a short time
                    System.Threading.Thread.Sleep(3);

                    //read the answer
                    TPCANMsg CANMsg = new TPCANMsg();
                    stsResult = PCANBasic.Read(m_PcanHandle, out CANMsg, out CANTimeStamp);
                    if (CANMsg.ID == (messageID + 1))
                    {
                        // see the documentation for the full protocol
                        errorCode = (int)CANMsg.DATA[0];

                        int p1 = CANMsg.DATA[2];                                    // the current position is found in bytes 2 and 3
                        int p2 = CANMsg.DATA[3];
                        double p = 256 * p1 + p2;
                        jointCurrent = (p - gearZero) / gearScale;
                        digitalIn = (int)CANMsg.DATA[7];

                    }
                    else
                    {
                        // if the received message is not the answer from the module we reset the adapter
                        // a better solution: implement a read-thread and store the messages in a messages buffer.
                        // then retrive the last message for the relevant ID
                        PCANBasic.Reset(m_PcanHandle);
                    }

                }
                catch (Exception ex)
                {
                    ;
                }
        }       // end of lock(this)

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

    }
}
