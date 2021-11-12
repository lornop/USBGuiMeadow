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
            if (checkBoxHistory.IsChecked == true)
            {
                txtRecieved.Text = newPacket + txtRecieved.Text;
            }
            else
            {
                txtRecieved.Text = newPacket;
            }

            
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
