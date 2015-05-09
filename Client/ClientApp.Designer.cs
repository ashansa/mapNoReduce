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
            this.txMapperPath = new System.Windows.Forms.TextBox();
            this.txtComplete = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMapperName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtContactWorker = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input File Path :";
            // 
            // txInputPath
            // 
            this.txInputPath.Location = new System.Drawing.Point(105, 66);
            this.txInputPath.Name = "txInputPath";
            this.txInputPath.Size = new System.Drawing.Size(344, 20);
            this.txInputPath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output File Path :";
            // 
            // txOutputPath
            // 
            this.txOutputPath.Location = new System.Drawing.Point(105, 103);
            this.txOutputPath.Name = "txOutputPath";
            this.txOutputPath.Size = new System.Drawing.Size(344, 20);
            this.txOutputPath.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "No of Splits :";
            // 
            // txSplits
            // 
            this.txSplits.Location = new System.Drawing.Point(105, 137);
            this.txSplits.Name = "txSplits";
            this.txSplits.Size = new System.Drawing.Size(50, 20);
            this.txSplits.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 209);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Mapper Path";
            // 
            // btSubmit
            // 
            this.btSubmit.Location = new System.Drawing.Point(105, 257);
            this.btSubmit.Name = "btSubmit";
            this.btSubmit.Size = new System.Drawing.Size(75, 23);
            this.btSubmit.TabIndex = 7;
            this.btSubmit.Text = "Submit";
            this.btSubmit.UseVisualStyleBackColor = true;
            this.btSubmit.Click += new System.EventHandler(this.btSubmit_Click);
            // 
            // txMapperPath
            // 
            this.txMapperPath.Location = new System.Drawing.Point(105, 208);
            this.txMapperPath.Name = "txMapperPath";
            this.txMapperPath.Size = new System.Drawing.Size(240, 20);
            this.txMapperPath.TabIndex = 8;
            // 
            // txtComplete
            // 
            this.txtComplete.Location = new System.Drawing.Point(25, 297);
            this.txtComplete.Multiline = true;
            this.txtComplete.Name = "txtComplete";
            this.txtComplete.Size = new System.Drawing.Size(386, 64);
            this.txtComplete.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 179);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Mapper Name";
            // 
            // txtMapperName
            // 
            this.txtMapperName.Location = new System.Drawing.Point(105, 174);
            this.txtMapperName.Name = "txtMapperName";
            this.txtMapperName.Size = new System.Drawing.Size(170, 20);
            this.txtMapperName.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Contact worker";
            // 
            // txtContactWorker
            // 
            this.txtContactWorker.Location = new System.Drawing.Point(106, 26);
            this.txtContactWorker.Name = "txtContactWorker";
            this.txtContactWorker.Size = new System.Drawing.Size(344, 20);
            this.txtContactWorker.TabIndex = 13;
            // 
            // ClientApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 435);
            this.Controls.Add(this.txtContactWorker);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtMapperName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtComplete);
            this.Controls.Add(this.txMapperPath);
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
        private System.Windows.Forms.TextBox txMapperPath;
        private System.Windows.Forms.TextBox txtComplete;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMapperName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtContactWorker;
    }
}