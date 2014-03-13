using System;
using System.Windows.Forms;
namespace NIPlayRoomNetServer
{
    partial class Server
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
            this.app_notify = new System.Windows.Forms.NotifyIcon(this.components);
            this.LogWIndow = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // app_notify
            // 
            this.app_notify.Icon = ((System.Drawing.Icon)(resources.GetObject("app_notify.Icon")));
            this.app_notify.Text = "触角跨进程消息服务";
            this.app_notify.Visible = true;
            this.app_notify.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.app_notify_MouseDoubleClick);
            // 
            // LogWIndow
            // 
            this.LogWIndow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LogWIndow.Location = new System.Drawing.Point(13, 13);
            this.LogWIndow.Multiline = true;
            this.LogWIndow.Name = "LogWIndow";
            this.LogWIndow.ReadOnly = true;
            this.LogWIndow.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogWIndow.Size = new System.Drawing.Size(454, 375);
            this.LogWIndow.TabIndex = 0;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(479, 400);
            this.Controls.Add(this.LogWIndow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Server";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.@__FromClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Form Event
        private void __InitFormEvents()
        {
            this.SizeChanged += this.__FromSizeChanged;
            this.Shown += __InitGUI;
            Application.Idle += this.__AppIdleEvtHandler;
        }

        private void __FromSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                app_notify.Visible = true;
            }
        }

        private void __FromClosing(object sender, FormClosingEventArgs e)
        {
            this.Stop();
        }

        private void app_notify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.show() 与 this.WindowState 置位次序不能颠倒，否则窗体只会在任务栏出现，
                this.Show();
                this.WindowState = FormWindowState.Normal;
                app_notify.Visible = false;
            }
        }

        private void __AppIdleEvtHandler(object sender, EventArgs e)
        {
            while(0 < _logMsgSeq.Count)
            {
                string msgItem = _logMsgSeq.Dequeue();
                this.LogWIndow.AppendText("\r\n");
                this.LogWIndow.AppendText(msgItem);
                this.LogWIndow.AppendText("\r\n");
            }   
        }
        #endregion

        #region GUI
        private void __InitGUI(object sender, EventArgs e)
        {
            //默认隐藏到系统托盘
            this.Hide();

            string strFormTitle = "触角跨进程消息服务-控制台" + " " + "v" + this.ProductVersion;
            this.Text = strFormTitle;
            this.app_notify.Text = "触角跨进程消息服务" + " " + "v" + this.ProductVersion;
        }
        #endregion

        private NotifyIcon app_notify;
        private TextBox LogWIndow;
    }
}

