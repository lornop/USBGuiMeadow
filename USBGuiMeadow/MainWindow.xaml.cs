using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace USBGuiMeadow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool bPortOpen = false;
        private int newPacketNumber, chkSumError = 0;

        SerialPort serialPort = new SerialPort();


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            serialPort.BaudRate = 115200;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += SerialPort_DataReceived;
            setSerialPort();
        }

        private void setSerialPort()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach(string port in ports)
            //{
            //    comboBox1.Items.Add(port);
            //}
            comboBox1.ItemsSource = ports;
            comboBox1.SelectedIndex = 0;

        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string text = serialPort.ReadLine();
            //txtRecieved.Text = text;

            if (txtRecieved.Dispatcher.CheckAccess())
            {
                UpdateUI(text);
            }
            else
            {
                txtRecieved.Dispatcher.Invoke(() => { UpdateUI(text); });
            }

        }

        private void UpdateUI(string newPacket)
        {
            
            //Function that calculates the current index for reading Chars from the data stream
            int i = 0;
            int nextIndex(int a, int l) //l is the length of the current word being read. 
            {
                i += l;     //Increment the index for the next time this is called. 
                return a;   //Return the index we recieved from the previous time this was called. 
            }
            // Returns the index i after the previous index is added to the word length. 



            if (checkBoxHistory.IsChecked == true)
            {
                txtRecieved.Text = newPacket + txtRecieved.Text;
            }
            else
            {
                txtRecieved.Text = newPacket;
            }
            txtPacketLength.Text = newPacket.Length.ToString();
            int calChkSum = 0;
            if (newPacket.Length > 37)
            {

                i = 0;  //start index reading at 0
                int l = 3;  //packet length is 3 chars
                if (newPacket.Substring((nextIndex(i, l)), l) == "###")
                {
                    txtPacketNum.Text = newPacket.Substring((nextIndex(i, l)), l);
                    
                    newPacketNumber = Convert.ToInt32(txtPacketNum.Text);

                    l = 4;  //Analog ins are 4 chars each

                    txtAN0.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtAN1.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtAN2.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtAN3.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtAN4.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtAN5.Text = newPacket.Substring((nextIndex(i, l)), l);
                    txtBIN.Text = newPacket.Substring((nextIndex(i, l)), l);

                    l = 3;  //Checksum is the last 3 digits. Shouldnt reallly need this but just in case we add to the protocol ....
                    txtRXChkSum.Text = newPacket.Substring((nextIndex(i, l)), l);

                    for (i = 3; i < 34; i++)
                    {
                        calChkSum += (byte)newPacket[i];
                    }
                    calChkSum %= 1000;  //To get the last threee digits like in the recieved protocol


                    //txtCalChkSum.Length sumthng sumthing == 3; for the stuff to add up correctly 
                    //recieved checkum is only last 3 digits

                    txtCalChkSum.Text = Convert.ToString(calChkSum);
                    int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
                    if (recChkSum == calChkSum)
                    {
                        DisplaySolarData(newPacket);
                    }

                    else
                    {
                        chkSumError++;
                        txtChkSumError.Text = Convert.ToString(chkSumError);           
                    }
                }
            }
        }


        private void DisplaySolarData(string newPacket)
        {
            //TODO List to display solar data
        }

        private void btnOpenClose_Click(object sender, RoutedEventArgs e)
        {
            if(!bPortOpen)
            {
                serialPort.PortName = comboBox1.Text;
                serialPort.Open();
                btnOpenClose.Content = "Close";
                bPortOpen = true;

            }

            else
            {
                serialPort.Close();
                btnOpenClose.Content = "Open";
                bPortOpen = false;

            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboBox1_MouseEnter(object sender, MouseEventArgs e)
        {
            setSerialPort();

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSend_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void txtSend_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
