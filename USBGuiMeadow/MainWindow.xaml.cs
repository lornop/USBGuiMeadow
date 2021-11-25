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
    /// Loren Olsen
    /// Nov 25 2021
    /// ECET 230 
    /// Solar Panel and LED Control via USB serial protocol
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool bPortOpen = false;
        private int newPacketNumber, chkSumError, OverFlowNumber = 0;

        SerialPort serialPort = new SerialPort();

        SolarCalc solarCalc = new SolarCalc();

        StringBuilder stringBuilderSend = new StringBuilder("###1111196");


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
                    int numberPackets = Convert.ToInt32(txtPacketNum.Text);
                    
                    if (numberPackets == 999)
                    {
                        OverFlowNumber++;
                        txtOverFlow.Text = Convert.ToString(OverFlowNumber);
                    }
                    
                    newPacketNumber = Convert.ToInt32(txtPacketNum.Text);

                    l = 4;  //Analog ins are 4 chars each

                    txtAN0.Text = newPacket.Substring((nextIndex(i, l)), l);    //Thermistor
                    txtAN1.Text = newPacket.Substring((nextIndex(i, l)), l);    //Solar Panel
                    txtAN2.Text = newPacket.Substring((nextIndex(i, l)), l);    //Capacitor
                    txtAN3.Text = newPacket.Substring((nextIndex(i, l)), l);    //LED1
                    txtAN4.Text = newPacket.Substring((nextIndex(i, l)), l);    //LED2
                    txtAN5.Text = newPacket.Substring((nextIndex(i, l)), l);    //Nothing
                    txtBIN.Text = newPacket.Substring((nextIndex(i, l)), l);

                    l = 3;  //Checksum is the last 3 digits. Shouldnt reallly need this but just in case we add to the protocol ....
                    txtRXChkSum.Text = newPacket.Substring((nextIndex(i, l)), l);

                    for (i = 3; i < 34; i++)
                    {
                        calChkSum += (byte)newPacket[i];
                    }
                    calChkSum %= 1000;  //To get the last threee digits like in the recieved protocol

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
                else
                {
                    chkSumError++;
                    txtChkSumError.Text = Convert.ToString(chkSumError);
                }
                
            }
        }


        private void DisplaySolarData(string newPacket)
        {
            solarCalc.ParseSolarData(newPacket);
            txtSolarVoltage.Text = solarCalc.GetVoltage(solarCalc.analogVoltage[0]);
            txtBatVolt.Text = solarCalc.GetVoltage(solarCalc.analogVoltage[2]);
            txtBatCurrent.Text = solarCalc.GetCurrent(solarCalc.analogVoltage[1], solarCalc.analogVoltage[2]);
            txtLED1Current.Text = solarCalc.GetCurrent(solarCalc.analogVoltage[1], solarCalc.analogVoltage[4]);
            txtLED2Current.Text = solarCalc.GetCurrent(solarCalc.analogVoltage[1], solarCalc.analogVoltage[3]);



            //I did this, Wayne got some other stuff in his vids!
            //int solarADCValue = Convert.ToInt32(txtAN1.Text);
            //double solarVoltage = 0;

            //solarVoltage = 5.5 / (3300.00 / solarADCValue);          //Test this to find math ????
            ////solarVoltage = 5.5 / (solarADCValue / 3300);


            //if (solarVoltage >= 1.50)
            //{
            //    ButtonClicked(0, 1);
            //}
            //else
            //{
            //    ButtonClicked(0, 0);
            //}

            //string txtSolarVol = solarVoltage.ToString("0.000");
            //txtSolarVoltage.Text = txtSolarVol;
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



        private void comboBox1_MouseEnter(object sender, MouseEventArgs e)
        {
            setSerialPort();

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            sendPacket();
        }

        private void sendPacket()
        {
            try
            {
                int txChkSum = 0;
                for (int i = 3; i < 7; i++)
                {
                    txChkSum += (byte)stringBuilderSend[i];
                }
                txChkSum %= 1000;
                stringBuilderSend.Remove(7, 3);
                stringBuilderSend.Insert(7, txChkSum.ToString("D3"));
                txtSend.Text = stringBuilderSend.ToString();


                string messageOut = stringBuilderSend.ToString();

                messageOut += "\r\n";                   //add CR and LF
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);   //Convert the textbox to an array of bytes(UTF8)
                serialPort.Write(messageBytes, 0, messageBytes.Length);     //Write to the serial port the bytes, 0 offset, how many bytes are in the array
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void txtSend_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtRecieved.Clear();
        }

        private void btnBit3_Click(object sender, RoutedEventArgs e)
        {
            ButtonClicked(3, 3);
        }

        private void btnBit2_Click(object sender, RoutedEventArgs e)
        {
            ButtonClicked(2, 3);
        }

        private void btnBit1_Click(object sender, RoutedEventArgs e)
        {
            ButtonClicked(1, 3);
        }

        private void btnBit0_Click(object sender, RoutedEventArgs e)
        {
            ButtonClicked(0, 3);
        }

        private void ButtonClicked(int v, int state)                //States are 0 = off, 1 = On and 3 = toggle
        {
            Button[] btnBit = new Button[] { btnBit0, btnBit1, btnBit2, btnBit3 };
            
            if (state == 0)
            {
                btnBit[v].Content = "0";
                stringBuilderSend[v + 3] = '0';
            }
            
            if (state == 1)
            {
                btnBit[v].Content = "1";
                stringBuilderSend[v + 3] = '1';
            }
            
            if (state == 3)
            {
                if (btnBit[v].Content.ToString() == "0")
                {
                    btnBit[v].Content = "1";
                    stringBuilderSend[v + 3] = '1';

                }
                else
                {
                    btnBit[v].Content = "0";
                    stringBuilderSend[v + 3] = '0';
                }
            }

            sendPacket();
        }

        private void txtSend_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
