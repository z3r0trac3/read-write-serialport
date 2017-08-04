using System;
using System.Drawing;
using System.Management;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace EdiSerialComms
{
    public partial class Form1 : Form
    {
        

        SerialPort ComPort = new SerialPort();
        
        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);
        internal delegate void SerialPinChangedEventHandlerDelegate(object sender, SerialPinChangedEventArgs e);
        private SerialPinChangedEventHandler SerialPinChangedEventHandler1;
        delegate void SetTextCallback(string text);
        string InputData = String.Empty;
        
        public Form1()
        {
            InitializeComponent();
            SerialPinChangedEventHandler1 = new SerialPinChangedEventHandler(PinChanged);
            ComPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e) {
            InputData = ComPort.ReadExisting();
            if (InputData != String.Empty) {
                this.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        string ExtractString(string s, string tag)
        {
            // You should check for errors in real-world code, omitted for brevity
            var startTag = "<" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }

        private void SetText(string text) {
            this.rtbIncoming.Text += text;

            Regex pattern = new Regex(@"bat1:(?<bat1v1>\d+),(?<bat1v2>\d+);bat2:(?<bat2v1>\d+),(?<bat2v2>\d+)");
            Match match = pattern.Match(text);

            double vbat1v1 = int.Parse(match.Groups["bat1v1"].Value);
            vbat1v1 = vbat1v1 * 100.2333;
            int vbat1v2 = int.Parse(match.Groups["bat1v2"].Value);
            //vbat1v2 = vbat1v1 * 2; asdasd1231231231
            int vbat2v1 = int.Parse(match.Groups["bat2v1"].Value);
            int vbat2v2 = int.Parse(match.Groups["bat2v2"].Value);

            label3.Text = vbat1v1.ToString();
            label4.Text = vbat1v2.ToString();
            label5.Text = vbat2v1.ToString();
            label6.Text = vbat2v2.ToString();



        }

        internal void PinChanged(object sender, SerialPinChangedEventArgs e) {
            SerialPinChange SerialPinChange1 = 0;
            bool signalState = false;

            SerialPinChange1 = e.EventType;
            lblCTSStatus.BackColor = Color.Green;
            lblDSRStatus.BackColor = Color.Green;
            lblRIStatus.BackColor = Color.Green;
            lblBreakStatus.BackColor = Color.Green;
            switch (SerialPinChange1) {
                case SerialPinChange.Break:
                    lblBreakStatus.BackColor = Color.Red;
                    //MessageBox.Show("Break is Set");
                    break;
                case SerialPinChange.CDChanged:
                    signalState = ComPort.CtsHolding;
                    //  MessageBox.Show("CD = " + signalState.ToString());
                    break;
                case SerialPinChange.CtsChanged:
                    signalState = ComPort.CDHolding;
                    lblCTSStatus.BackColor = Color.Red;
                    //MessageBox.Show("CTS = " + signalState.ToString());
                    break;
                case SerialPinChange.DsrChanged:
                    signalState = ComPort.DsrHolding;
                    lblDSRStatus.BackColor = Color.Red;
                    // MessageBox.Show("DSR = " + signalState.ToString());
                    break;
                case SerialPinChange.Ring:
                    lblRIStatus.BackColor = Color.Red;
                    //MessageBox.Show("Ring Detected");
                    break;
            }
        }

        private void rtbOutgoing_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)13) { // enter 
                ComPort.Write("\r\n");
                rtbOutgoing.Text = "";
            } else if (e.KeyChar < 32 || e.KeyChar > 126) {
                e.Handled = true; // ignores anything else outside printable ASCII range  
            } else {
                ComPort.Write(e.KeyChar.ToString());
            }
        }
      
        private void btnGetSerialPorts_Click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

//Com Ports
            ArrayComPortsNames = SerialPort.GetPortNames();

            if (ArrayComPortsNames == null || ArrayComPortsNames.Length == 0)
            {
                MessageBox.Show("Comm ports not found!");
                return;
            }

            do
            {
                index += 1;
                cboPorts.Items.Add(ArrayComPortsNames[index]);

            } while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));
            Array.Sort(ArrayComPortsNames);

            if (index == ArrayComPortsNames.GetUpperBound(0))
            {
                ComPortName = ArrayComPortsNames[0];
            }

            cboPorts.Text = ArrayComPortsNames[0];
//Baud Rate
            cboBaudRate.Items.Add(9600);
            //cboBaudRate.Items.Add(14400);
            //cboBaudRate.Items.Add(19200);
            //cboBaudRate.Items.Add(38400);
            //cboBaudRate.Items.Add(57600);
            cboBaudRate.Items.Add(115200);
            cboBaudRate.Items.ToString();
            cboBaudRate.Text = cboBaudRate.Items[0].ToString();
//Data Bits
            // cboDataBits.Items.Add(7);
            cboDataBits.Items.Add(8);
            cboDataBits.Text = cboDataBits.Items[0].ToString();
//Stop Bits
            cboStopBits.Items.Add("One");
            //cboStopBits.Items.Add("OnePointFive");
            //cboStopBits.Items.Add("Two");
            cboStopBits.Text = cboStopBits.Items[0].ToString();
//Parity 
            cboParity.Items.Add("None");
            //boParity.Items.Add("Even");
            //cboParity.Items.Add("Mark");
            //cboParity.Items.Add("Odd");
            //cboParity.Items.Add("Space");
            cboParity.Text = cboParity.Items[0].ToString();
//Handshake
            cboHandShaking.Items.Add("None");
            //cboHandShaking.Items.Add("XOnXOff");
            //cboHandShaking.Items.Add("RequestToSend");
            //cboHandShaking.Items.Add("RequestToSendXOnXOff");
            cboHandShaking.Text = cboHandShaking.Items[0].ToString();

            // enable buttons
            btnPortState.Enabled = true;
            btnHello.Enabled = true;
            btnHyperTerm.Enabled = true;
            // set label
            labelstatus.Text = "Config settings and press 'Open' button!";

        }

        private void btnPortState_Click_1(object sender, EventArgs e)
        {
            if (btnPortState.Text == "Open")
            {
                //MessageBox.Show("aici ?");
                ComPort.PortName = Convert.ToString(cboPorts.Text);
                ComPort.BaudRate = Convert.ToInt32(cboBaudRate.Text);
                ComPort.DataBits = Convert.ToInt16(cboDataBits.Text);
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStopBits.Text);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), cboHandShaking.Text);
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
                ComPort.Open();
                labelstatus.Text = "Connected!";
                btnPortState.Text = "Closed";
            }
            else if (btnPortState.Text == "Closed")
            {
                //MessageBox.Show("aici2 ?");
                ComPort.Close();
                labelstatus.Text = "Disconnected!";
                btnPortState.Text = "Open";
            }
            else
            {
                MessageBox.Show("aici3 ?" + btnPortState.Text);
            }
        }

        private void btnHyperTerm_Click_1(object sender, EventArgs e)
        {
            string Command1 = txtCommand.Text;
            string CommandSent;
            int Length, j = 0;

            Length = Command1.Length;

            for (int i = 0; i < Length; i++)
            {
                CommandSent = Command1.Substring(j, 1);
                ComPort.Write(CommandSent);
                j++;
            }
        }

        private void btnHello_Click_1(object sender, EventArgs e)
        {
            ComPort.Write("Hello World!");
        }
    }

}
