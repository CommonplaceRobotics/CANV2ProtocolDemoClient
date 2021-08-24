namespace CANV2ProtocolDemoClient
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelConnection = new System.Windows.Forms.Label();
            this.labelJointStatus = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.buttonForward = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonBackwards = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonEnable = new System.Windows.Forms.Button();
            this.buttonSetToZero = new System.Windows.Forms.Button();
            this.buttonStartReferencing = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelPosSetPoint = new System.Windows.Forms.Label();
            this.labelMotorCurrent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(38, 126);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelConnection
            // 
            this.labelConnection.AutoSize = true;
            this.labelConnection.Location = new System.Drawing.Point(35, 88);
            this.labelConnection.Name = "labelConnection";
            this.labelConnection.Size = new System.Drawing.Size(147, 17);
            this.labelConnection.TabIndex = 1;
            this.labelConnection.Text = "Connection Status: na";
            // 
            // labelJointStatus
            // 
            this.labelJointStatus.AutoSize = true;
            this.labelJointStatus.Location = new System.Drawing.Point(308, 88);
            this.labelJointStatus.Name = "labelJointStatus";
            this.labelJointStatus.Size = new System.Drawing.Size(106, 17);
            this.labelJointStatus.TabIndex = 2;
            this.labelJointStatus.Text = "Joint Status: na";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(308, 141);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(129, 17);
            this.labelPosition.TabIndex = 3;
            this.labelPosition.Text = "CurrentPosition: na";
            // 
            // buttonForward
            // 
            this.buttonForward.Location = new System.Drawing.Point(311, 211);
            this.buttonForward.Name = "buttonForward";
            this.buttonForward.Size = new System.Drawing.Size(75, 23);
            this.buttonForward.TabIndex = 4;
            this.buttonForward.Text = "Forward";
            this.buttonForward.UseVisualStyleBackColor = true;
            this.buttonForward.Click += new System.EventHandler(this.buttonForward_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(392, 211);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 5;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonBackwards
            // 
            this.buttonBackwards.Location = new System.Drawing.Point(473, 211);
            this.buttonBackwards.Name = "buttonBackwards";
            this.buttonBackwards.Size = new System.Drawing.Size(75, 23);
            this.buttonBackwards.TabIndex = 6;
            this.buttonBackwards.Text = "Backwards";
            this.buttonBackwards.UseVisualStyleBackColor = true;
            this.buttonBackwards.Click += new System.EventHandler(this.buttonBackwards_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(38, 211);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 7;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonEnable
            // 
            this.buttonEnable.Location = new System.Drawing.Point(38, 240);
            this.buttonEnable.Name = "buttonEnable";
            this.buttonEnable.Size = new System.Drawing.Size(75, 23);
            this.buttonEnable.TabIndex = 8;
            this.buttonEnable.Text = "Enable";
            this.buttonEnable.UseVisualStyleBackColor = true;
            this.buttonEnable.Click += new System.EventHandler(this.buttonEnable_Click);
            // 
            // buttonSetToZero
            // 
            this.buttonSetToZero.Location = new System.Drawing.Point(38, 269);
            this.buttonSetToZero.Name = "buttonSetToZero";
            this.buttonSetToZero.Size = new System.Drawing.Size(75, 23);
            this.buttonSetToZero.TabIndex = 9;
            this.buttonSetToZero.Text = "Set To Zero";
            this.buttonSetToZero.UseVisualStyleBackColor = true;
            this.buttonSetToZero.Click += new System.EventHandler(this.buttonSetToZero_Click);
            // 
            // buttonStartReferencing
            // 
            this.buttonStartReferencing.Location = new System.Drawing.Point(38, 298);
            this.buttonStartReferencing.Name = "buttonStartReferencing";
            this.buttonStartReferencing.Size = new System.Drawing.Size(75, 23);
            this.buttonStartReferencing.TabIndex = 10;
            this.buttonStartReferencing.Text = "StartReferencing";
            this.buttonStartReferencing.UseVisualStyleBackColor = true;
            this.buttonStartReferencing.Click += new System.EventHandler(this.buttonStartReferencing_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelPosSetPoint
            // 
            this.labelPosSetPoint.AutoSize = true;
            this.labelPosSetPoint.Location = new System.Drawing.Point(308, 114);
            this.labelPosSetPoint.Name = "labelPosSetPoint";
            this.labelPosSetPoint.Size = new System.Drawing.Size(135, 17);
            this.labelPosSetPoint.TabIndex = 11;
            this.labelPosSetPoint.Text = "SetPointPosition: na";
            // 
            // labelMotorCurrent
            // 
            this.labelMotorCurrent.AutoSize = true;
            this.labelMotorCurrent.Location = new System.Drawing.Point(308, 168);
            this.labelMotorCurrent.Name = "labelMotorCurrent";
            this.labelMotorCurrent.Size = new System.Drawing.Size(119, 17);
            this.labelMotorCurrent.TabIndex = 12;
            this.labelMotorCurrent.Text = "Motor Current: na";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.labelMotorCurrent);
            this.Controls.Add(this.labelPosSetPoint);
            this.Controls.Add(this.buttonStartReferencing);
            this.Controls.Add(this.buttonSetToZero);
            this.Controls.Add(this.buttonEnable);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonBackwards);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonForward);
            this.Controls.Add(this.labelPosition);
            this.Controls.Add(this.labelJointStatus);
            this.Controls.Add(this.labelConnection);
            this.Controls.Add(this.buttonConnect);
            this.Name = "Form1";
            this.Text = "na";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelConnection;
        private System.Windows.Forms.Label labelJointStatus;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Button buttonForward;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonBackwards;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonEnable;
        private System.Windows.Forms.Button buttonSetToZero;
        private System.Windows.Forms.Button buttonStartReferencing;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelPosSetPoint;
        private System.Windows.Forms.Label labelMotorCurrent;
    }
}

