namespace PADIMapNoReduce
{
    partial class ClientApp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txInputPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txOutputPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txSplits = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btSubmit = new System.Windows.Forms.Button();
            this.txMapper = new System.Windows.Forms.TextBox();
            this.txtComplete = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input File Path :";
            // 
            // txInputPath
            // 
            this.txInputPath.Location = new System.Drawing.Point(105, 32);
            this.txInputPath.Name = "txInputPath";
            this.txInputPath.Size = new System.Drawing.Size(167, 20);
            this.txInputPath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output File Path :";
            // 
            // txOutputPath
            // 
            this.txOutputPath.Location = new System.Drawing.Point(105, 78);
            this.txOutputPath.Name = "txOutputPath";
            this.txOutputPath.Size = new System.Drawing.Size(167, 20);
            this.txOutputPath.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "No of Splits :";
            // 
            // txSplits
            // 
            this.txSplits.Location = new System.Drawing.Point(105, 120);
            this.txSplits.Name = "txSplits";
            this.txSplits.Size = new System.Drawing.Size(50, 20);
            this.txSplits.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 159);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Mapper Path???";
            // 
            // btSubmit
            // 
            this.btSubmit.Location = new System.Drawing.Point(130, 187);
            this.btSubmit.Name = "btSubmit";
            this.btSubmit.Size = new System.Drawing.Size(75, 23);
            this.btSubmit.TabIndex = 7;
            this.btSubmit.Text = "Submit";
            this.btSubmit.UseVisualStyleBackColor = true;
            this.btSubmit.Click += new System.EventHandler(this.btSubmit_Click);
            // 
            // txMapper
            // 
            this.txMapper.Location = new System.Drawing.Point(105, 151);
            this.txMapper.Name = "txMapper";
            this.txMapper.Size = new System.Drawing.Size(100, 20);
            this.txMapper.TabIndex = 8;
            // 
            // txtComplete
            // 
            this.txtComplete.Location = new System.Drawing.Point(25, 257);
            this.txtComplete.Multiline = true;
            this.txtComplete.Name = "txtComplete";
            this.txtComplete.Size = new System.Drawing.Size(386, 64);
            this.txtComplete.TabIndex = 9;
            // 
            // ClientApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 435);
            this.Controls.Add(this.txtComplete);
            this.Controls.Add(this.txMapper);
            this.Controls.Add(this.btSubmit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txSplits);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txOutputPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txInputPath);
            this.Controls.Add(this.label1);
            this.Name = "ClientApp";
            this.Text = "ClientApp";
            this.Load += new System.EventHandler(this.ClientApp_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txInputPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txOutputPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txSplits;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btSubmit;
        private System.Windows.Forms.TextBox txMapper;
        private System.Windows.Forms.TextBox txtComplete;
    }
}