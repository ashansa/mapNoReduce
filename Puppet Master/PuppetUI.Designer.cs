namespace Puppet_Master
{
    partial class PuppetUI
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
            System.Windows.Forms.Button worker_submit;
            System.Windows.Forms.Button client_submit;
            this.txt_workerCreate = new System.Windows.Forms.TextBox();
            this.txt_clientCreate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_puppetId = new System.Windows.Forms.TextBox();
            this.init = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btn_View_Status = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            worker_submit = new System.Windows.Forms.Button();
            client_submit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // worker_submit
            // 
            worker_submit.Location = new System.Drawing.Point(36, 143);
            worker_submit.Name = "worker_submit";
            worker_submit.Size = new System.Drawing.Size(75, 23);
            worker_submit.TabIndex = 3;
            worker_submit.Text = "submit";
            worker_submit.UseVisualStyleBackColor = true;
            worker_submit.Click += new System.EventHandler(this.worker_submit_Click);
            // 
            // client_submit
            // 
            client_submit.Location = new System.Drawing.Point(36, 231);
            client_submit.Name = "client_submit";
            client_submit.Size = new System.Drawing.Size(75, 23);
            client_submit.TabIndex = 5;
            client_submit.Text = "submit";
            client_submit.UseVisualStyleBackColor = true;
            client_submit.Click += new System.EventHandler(this.client_submit_Click);
            // 
            // txt_workerCreate
            // 
            this.txt_workerCreate.Location = new System.Drawing.Point(36, 117);
            this.txt_workerCreate.Name = "txt_workerCreate";
            this.txt_workerCreate.Size = new System.Drawing.Size(390, 20);
            this.txt_workerCreate.TabIndex = 1;
            this.txt_workerCreate.Text = "Worker 1 tcp://localhost:9443/Puppet tcp://localhost:11000/Worker";
            // 
            // txt_clientCreate
            // 
            this.txt_clientCreate.Location = new System.Drawing.Point(36, 205);
            this.txt_clientCreate.Name = "txt_clientCreate";
            this.txt_clientCreate.Size = new System.Drawing.Size(390, 20);
            this.txt_clientCreate.TabIndex = 4;
            this.txt_clientCreate.Text = "Submit tcp://localhost:11000/Worker E:\\input\\chathuri.txt E:\\input 3 Mapper ..\\.." +
    "\\..\\LibMapper\\bin\\Debug\\LibMapper.dll";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 179);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Start clients";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start workers";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Puppet URL";
            // 
            // txt_puppetId
            // 
            this.txt_puppetId.Location = new System.Drawing.Point(105, 35);
            this.txt_puppetId.Name = "txt_puppetId";
            this.txt_puppetId.Size = new System.Drawing.Size(216, 20);
            this.txt_puppetId.TabIndex = 7;
            this.txt_puppetId.Text = "tcp://localhost:9443/Puppet";
            // 
            // init
            // 
            this.init.Location = new System.Drawing.Point(341, 34);
            this.init.Name = "init";
            this.init.Size = new System.Drawing.Size(75, 23);
            this.init.TabIndex = 8;
            this.init.Text = "Submit";
            this.init.UseVisualStyleBackColor = true;
            this.init.Click += new System.EventHandler(this.init_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(139, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 9;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(168, 148);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(400, 20);
            this.textBox1.TabIndex = 10;
            this.textBox1.Text = "Worker 2 tcp://localhost:9450/Puppet tcp://localhost:11100/Worker tcp://localhost" +
    ":11000/Worker";
            // 
            // btn_View_Status
            // 
            this.btn_View_Status.Location = new System.Drawing.Point(36, 285);
            this.btn_View_Status.Name = "btn_View_Status";
            this.btn_View_Status.Size = new System.Drawing.Size(96, 23);
            this.btn_View_Status.TabIndex = 11;
            this.btn_View_Status.Text = "View Job Status";
            this.btn_View_Status.UseVisualStyleBackColor = true;
            this.btn_View_Status.Click += new System.EventHandler(this.btn_View_Status_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(168, 179);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(400, 20);
            this.textBox2.TabIndex = 12;
            this.textBox2.Text = "Worker 3 tcp://localhost:9460/Puppet tcp://localhost:11500/Worker tcp://localhost" +
    ":11000/Worker";
            // 
            // PuppetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(657, 521);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btn_View_Status);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.init);
            this.Controls.Add(this.txt_puppetId);
            this.Controls.Add(this.label3);
            this.Controls.Add(client_submit);
            this.Controls.Add(this.txt_clientCreate);
            this.Controls.Add(worker_submit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_workerCreate);
            this.Controls.Add(this.label1);
            this.Name = "PuppetUI";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_workerCreate;
        private System.Windows.Forms.TextBox txt_clientCreate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_puppetId;
        private System.Windows.Forms.Button init;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btn_View_Status;
        private System.Windows.Forms.TextBox textBox2;
    }
}

