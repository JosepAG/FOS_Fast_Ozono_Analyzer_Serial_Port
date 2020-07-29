﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Hosting;

namespace FOS_serial_port
{
    public partial class Form1 : Form
    {
        String data;
        String temp;
        String Ozone;
        String flowrate;
        DateTime DateNow;
        String DateNowfff;
        float OzoneNum;
        String DataOut;
        String pathSave;
        int LineBaud;
        String LinePort;
        int Cont;
        public Form1()
        {
            InitializeComponent();
            getAvailablePorts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Cont = 0;

            StreamReader sw = new StreamReader("Path.txt");
            pathSave = sw.ReadLine();
            sw.Close();
            button2.Text = "Data is saved in: " + pathSave;

            int start = 1;
            if (start == 1)
            {
                ///////Leer Configuración inicial Port/Bauds///////////////////////
                try
                {
                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader("Port.txt");
                    //Read the first line of text
                    LinePort = sr.ReadLine();
                    comboBox1.Text = LinePort;
                    String LineBaudS = sr.ReadLine();
                    LineBaud = Int32.Parse(LineBaudS);
                    comboBox1.Text = LinePort;
                    comboBox2.Text = LineBaudS;
                    data_tb.Text = "Wait 1 minute to load the data plot.";
                    //data_tb2.Text = "Wait to get the 30 minutes average data plot.";
                    sr.Close();


                }
                catch (Exception)
                {
                    Console.WriteLine("Introduce the Start-up port and Bauds in the Port.txt");
                }
                finally
                {
                    Console.WriteLine("Conected to " + LinePort + " with " + LineBaud + " Baudrate");
                }
                serialPort1.PortName = LinePort;
                serialPort1.Close();
                serialPort1.BaudRate = LineBaud;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.DataBits = 8;
                serialPort1.Handshake = Handshake.None;
                serialPort1.RtsEnable = false;

                serialPort1.DataReceived += new
                    SerialDataReceivedEventHandler(port_DataReceived);
                if (SerialPort.GetPortNames().ToList().Contains(LinePort))
                {
                    serialPort1.Open();
                }
                else
                {
                    MessageBox.Show("The predetermined COM Port is not available, please change it.", "COM port error", MessageBoxButtons.OK);

                }
                timer1.Enabled = true;
                //value_pb.Value = 100;
                stop_btn.Enabled = true;
                start_btn.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;

                start = 0;


            }
        }
        void getAvailablePorts()
        {
            String[] ports = SerialPort.GetPortNames();

            comboBox1.Items.AddRange(ports);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.Text == "" || comboBox2.Text == "")
                {
                    data_tb.Text = "Please select port settings";

                }
                else
                {

                    StreamWriter ss = new StreamWriter("Port.txt");

                    ss.WriteLine(comboBox1.Text);
                    ss.WriteLine(comboBox2.Text);
                    ss.Close();
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.Close();
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.DataBits = 8;
                    serialPort1.Handshake = Handshake.None;
                    serialPort1.RtsEnable = false;

                    serialPort1.DataReceived += new
                        SerialDataReceivedEventHandler(port_DataReceived);
                    serialPort1.Open();
                    timer1.Enabled = true;
                    //value_pb.Value = 100;
                    stop_btn.Enabled = true;
                    start_btn.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Introduce the Start-up port and Bauds in the Port.txt");
            }
            finally
            {
                Console.WriteLine("Conected to " + " with " + " Baudrate");
            }
        }

        private void port_DataReceived(object sender,
                         SerialDataReceivedEventArgs e)
        {
            data = serialPort1.ReadLine();
;
            if (data.Substring(0, 1) == "0")
            {
                Console.WriteLine("dentro 0");
                Console.WriteLine(data);
                Ozone = data.Substring(2, 8);
                Console.WriteLine("Ozone is: "+Ozone);

            }
            if (data.Substring(0, 1) == "1")
            {
                Console.WriteLine("dentro 1");
                Console.WriteLine(data);
                Ozone = data.Substring(2, 8);
                flowrate = data.Substring(13, 6);
                Console.WriteLine("Flowrate is: "+flowrate);

            }
            if (data.Substring(0,1) == "2")
            {
                Console.WriteLine("dentro 2");
                Console.WriteLine(data);
                Ozone = data.Substring(2, 8);                
                temp = data.Substring(13,7);
                Console.WriteLine("Temperature is: "+temp);
            }
            Cont = Cont + 1;
            if(Cont >= 3)
            {
                Cont = 3;
                OzoneNum = float.Parse(Ozone);
                DateNow = DateTime.Now;
                DateNowfff = DateNow.ToString("d/MM/yyyy HH:mm:ss.fff");
                DataOut = Ozone + "," + temp + "," + flowrate+","+DateNowfff;
                File.AppendAllText("Data.txt", DataOut + Environment.NewLine);
                File.WriteAllText("DataForCsv.txt", File.ReadAllText("Headers.txt") + Environment.NewLine + File.ReadAllText("Data.txt"));

                this.Invoke(new EventHandler(Showdata));
            }
        }
        private void Showdata(object sender, EventArgs e)
        {
            
          
            String DateNow2 = DateNow.ToString("ss:fff ");

            chart1.Series["Ozone"].Points.AddXY(DateNow2, OzoneNum);
            if (chart1.Series["Ozone"].Points.Count > 10000)
            {
                chart1.Series["Ozone"].Points.RemoveAt(0);
                chart1.ResetAutoValues();
            }


            data_tb.Text = data;

        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            timer1.Enabled = false;
            //value_pb.Value = 0;
            start_btn.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            stop_btn.Enabled = false;
        }

        private void data_tb_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog In_path = new FolderBrowserDialog();
            if (In_path.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show("The data in .txt format will be saved in: " + In_path.SelectedPath);
                pathSave = In_path.SelectedPath;
                StreamWriter sp = new StreamWriter("Path.txt");
                sp.WriteLine(pathSave);
                sp.Close();
                button2.Text = "Data is saved in: " + In_path.SelectedPath;
            }

        }

        private void save_btn_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "DataForCsv.txt");
        }
    }
}