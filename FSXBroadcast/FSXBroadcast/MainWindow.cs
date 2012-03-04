using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FSXBroadcast
{
    public partial class MainWindow : Form
    {
        delegate void SetTextCallback(string text);

        FSXConnect connect = null;
        Server server = null;

        public MainWindow()
        {
            InitializeComponent();
            connect = new FSXConnect();
            connect.FSXConnectionChanged += new FSXConnectDelegate(OnConnectionChanged);
            connect.connect(this.Handle);
            server = new Server();
            server.Start();
            server.ClientCountChanged += new ServerDelegate(OnClientsChanged);
            connect.FSXDataReceived += new FSXMessageDelegate(server.Write);
        }

        private void connect_Click(object sender, EventArgs e)
        {
            if (connect.isConnected())
            {
                connect.disconnect();
            }
            else
            {
                connect.connect(this.Handle);
            }
        }

        public void OnConnectionChanged()
        {
            if (connect.isConnected())
            {
                connectBtn.Text = "Disconnect";
                statusLbl.Text = "Connected to FSX";
            }
            else
            {
                connectBtn.Text = "Connect";
                statusLbl.Text = "Not connected to FSX";
            }
        }

        private void OnClientsChanged()
        {
            int count = server.ClientCount();
            if (count > 0)
            {
                this.SetText(string.Format("{0} clients connected", count));
            }
            else
            {
                this.SetText("No clients connected");
            }
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.clientsLbl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.clientsLbl.Text = text;
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == FSXConnect.WM_USER_SIMCONNECT)
            {
                if (connect.isConnected())
                {
                    connect.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
        }
    }
}
