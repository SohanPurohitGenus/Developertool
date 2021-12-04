﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace Developer_Tools
{
    public partial class Form1 : Form
    {
        /* global variables */
        string newline = Environment.NewLine;
        public static bool login_success = false;

        /* communication traffic */
        static int traffic_mode;
        static string traffic_string = String.Empty;

        /* configuration file */
        string config_file_name;

        /* communication */
        byte[] temp_b_array = new byte[1000];
        int temp_b_array_length;
        byte[] tx_buffer = new byte[550];
        byte[] rx_buffer = new byte[550];


        /* Serial Port */
        DS_Serial serial_port = new DS_Serial();

        /******************************** Energy Meter Variables ********************************/
        /* input signal */
        public static double ip_vol_r, ip_vol_y, ip_vol_b, ip_curr_r, ip_curr_y, ip_curr_b, ip_curr_n_calculated;
        public static double ip_ang_r, ip_ang_y, ip_ang_b, ip_ang_n_calculated, ip_ang_ry, ip_ang_rb;
        public static double ip_freq;
        public static double ip_watt_r, ip_watt_y, ip_watt_b, ip_watt_total_fwd, ip_watt_total_net, ip_var_r, ip_var_y, ip_var_b, ip_var_total_fwd, ip_var_total_net, ip_va_r, ip_va_y, ip_va_b, ip_va_total_fwd, ip_va_total_net;
        public static double ip_pf_r, ip_pf_y, ip_pf_b, ip_pf_fwd, ip_pf_net;
        public static int metering_mode;
        /* error calculation */
        public static int ip_avg_samples;
        public static bool cal_accuracy;

        /* instant frame variables */
        public static double VolR, VolY, VolB, CurrRSigned, CurrYSigned, CurrBSigned, CurrN, CurrNVector, VolRY, VolYB, VolBR;
        public static double VolRdc, VolYdc, VolBdc, CurrRdc, CurrYdc, CurrBdc, CurrNdc;
        public static double PFR, PFY, PFB, PFNet;
        public static double AnglePFR, AnglePFY, AnglePFB, AngleNVector, AngleRY, AngleYB, AngleBR;
        public static double WattR, WattY, WattB, WattNet;
        public static double VARR, VARY, VARB, VARNet;
        public static double VAR, VAY, VAB, VANet;
        public static double FreqR, FreqY, FreqB, FreqNet;
        public static int QuadrantR, QuadrantY, QuadrantB, QuadrantNet;
        public static int SamplesR, SamplesY, SamplesB, SamplesN, SamplesPerSec;
        public static double THDVr, THDVy, THDVb, THDIr, THDIy, THDIb;
        public static double EnergyWhR_imp, EnergyWhY_imp, EnergyWhB_imp, EnergyWhTotal_imp;
        public static double EnergyWhR_exp, EnergyWhY_exp, EnergyWhB_exp, EnergyWhTotal_exp;
        public static double EnergyVARhR_q1, EnergyVARhY_q1, EnergyVARhB_q1, EnergyVARhTotal_q1;
        public static double EnergyVARhR_q2, EnergyVARhY_q2, EnergyVARhB_q2, EnergyVARhTotal_q2;
        public static double EnergyVARhR_q3, EnergyVARhY_q3, EnergyVARhB_q3, EnergyVARhTotal_q3;
        public static double EnergyVARhR_q4, EnergyVARhY_q4, EnergyVARhB_q4, EnergyVARhTotal_q4;
        public static double EnergyVAhR_imp, EnergyVAhY_imp, EnergyVAhB_imp, EnergyVAhTotal_imp;
        public static double EnergyVAhR_exp, EnergyVAhY_exp, EnergyVAhB_exp, EnergyVAhTotal_exp;
        public static double EnergyFWhR_imp, EnergyFWhY_imp, EnergyFWhB_imp, EnergyFWhTotal_imp;
        public static double EnergyFWhR_exp, EnergyFWhY_exp, EnergyFWhB_exp, EnergyFWhTotal_exp;
        public static int pulse_EnergyWhR_imp, pulse_EnergyWhY_imp, pulse_EnergyWhB_imp, pulse_EnergyWhTotal_imp;
        public static int pulse_EnergyWhR_exp, pulse_EnergyWhY_exp, pulse_EnergyWhB_exp, pulse_EnergyWhTotal_exp;
        public static int pulse_EnergyVARhR_q1, pulse_EnergyVARhY_q1, pulse_EnergyVARhB_q1, pulse_EnergyVARhTotal_q1;
        public static int pulse_EnergyVARhR_q2, pulse_EnergyVARhY_q2, pulse_EnergyVARhB_q2, pulse_EnergyVARhTotal_q2;
        public static int pulse_EnergyVARhR_q3, pulse_EnergyVARhY_q3, pulse_EnergyVARhB_q3, pulse_EnergyVARhTotal_q3;
        public static int pulse_EnergyVARhR_q4, pulse_EnergyVARhY_q4, pulse_EnergyVARhB_q4, pulse_EnergyVARhTotal_q4;
        public static int pulse_EnergyVAhR_imp, pulse_EnergyVAhY_imp, pulse_EnergyVAhB_imp, pulse_EnergyVAhTotal_imp;
        public static int pulse_EnergyVAhR_exp, pulse_EnergyVAhY_exp, pulse_EnergyVAhB_exp, pulse_EnergyVAhTotal_exp;
        public static int pulse_EnergyFWhR_imp, pulse_EnergyFWhY_imp, pulse_EnergyFWhB_imp, pulse_EnergyFWhTotal_imp;
        public static int pulse_EnergyFWhR_exp, pulse_EnergyFWhY_exp, pulse_EnergyFWhB_exp, pulse_EnergyFWhTotal_exp;

        public static string Time;
        public static int powerUpSec, reactiveSamples, reactiveTimer, reactiveTimeDelay, reactiveTimeDeviation;
        public static long LoopCycles;
        public static double battery_voltage, battery_voltage_rtc, temperature_tlv;
        public static string MISCData, TamperStatus;

        public static int METER_CONST = 1200, PULSE = 6;
        public double QUANTA = 0.005;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /* changing colors of fonts */
            menuStripMain.ForeColor = Color.Blue;

            /* creating files at startup */
            DS_App.startUpFilesChecking();

            /* Loading data from JSON */
            DS_App.loadParametersFromJson();

            timer1sec.Enabled = true;
            timer10ms.Enabled = true;
            timer500ms.Enabled = true;
            timer100ms.Enabled = true;
            if (DS_Serial.GetPortNames().Length != 0)
            {
                comboBox_SerialSingleCOMPORT.Text = DS_Serial.GetPortNames()[0];
            }
            else
            {
                comboBox_SerialSingleCOMPORT.Text = string.Empty;
            }
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* opens save file dialog */
            saveJSONFileDialog.Filter = "JSON files (*.json)|*.json";
            saveJSONFileDialog.InitialDirectory = "D:\\DevelopersTool";
            saveJSONFileDialog.Title = "Write the name of the configuration file.";
            saveJSONFileDialog.ShowDialog();

            /* creates a new json file */
            string fileName = saveJSONFileDialog.FileName;
            if (fileName.Length != 0)
            {
                try
                {
                    DS_JSON.createNewFile(fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* opens open file dialog */
            openJSONFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openJSONFileDialog.InitialDirectory = "D:\\DevelopersTool";
            openJSONFileDialog.Title = "Select the file to view";
            openJSONFileDialog.ShowDialog();

            /* displays the file in notepad */
            string fileName = openJSONFileDialog.FileName;
            if (fileName.Length != 0)
            {
                System.Diagnostics.Process.Start(@fileName);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* opens open file dialog */
            openJSONFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openJSONFileDialog.InitialDirectory = "D:\\DevelopersTool";
            openJSONFileDialog.Title = "Select the file to load";
            openJSONFileDialog.ShowDialog();

            /* reading the file into string */
            string fileName = openJSONFileDialog.FileName;
            if (fileName.Length != 0)
            {
                config_file_name = fileName;
                string data = DS_JSON.readFile(config_file_name);

                /* parsing JSON data from string */
                var jsonData = JObject.Parse(data);

                if (jsonData.HasValues == false)
                {
                    MessageBox.Show("Empty JSON file");
                }
                else
                {

                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (config_file_name == null)
            {
                MessageBox.Show("Configuration file not loaded");
            }
            else
            {
                /* save all the date into JSON file here */



                /* updating the text back to normal */
                if (saveToolStripMenuItem.Text == "Save'")
                {
                    saveToolStripMenuItem.Text = "Save";
                }
            }
        }

        private void writeUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string address = "mailto: dheeraj.singhal @genus.in?cc = dheerajsinghal01@gmail.com & subject = Developers % 20Tool % 20remarks % 3A";
            System.Diagnostics.Process.Start(@address);
        }

        private void webPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string address = "https://in.linkedin.com/in/dheerajsinghal";
            System.Diagnostics.Process.Start(@address);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("An ultimate tool to do the job easily. write us your feedback/suggestions or request new feature @ dheeraj.singhal@genus.in");
        }

        private void buttonToolsInputTextBoxPaste_Click(object sender, EventArgs e)
        {
            textBox_ToolsInputString.Text = Clipboard.GetText();
        }

        private void buttonToolsInputTextBoxClear_Click(object sender, EventArgs e)
        {
            textBox_ToolsInputString.Text = String.Empty;
        }

        private void buttonToolsOutputTextBoxCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox_ToolsOutputString.Text);
        }

        private void buttonToolsOutputTextBoxClear_Click(object sender, EventArgs e)
        {
            textBox_ToolsOutputString.Text = String.Empty;
        }

        private void button_SendRepeatStop_Click(object sender, EventArgs e)
        {
            serial_port.SendRepeatEnable = false;
        }

        private void button_Read_Click(object sender, EventArgs e)
        {
            /* Frame Creation */
            temp_b_array[0] = 0x27;
            temp_b_array[1] = 0xFF;
            temp_b_array[2] = 0x01;
            temp_b_array[3] = 0x00;
            temp_b_array[4] = 0x00;
            temp_b_array[5] = 0x00;
            temp_b_array[6] = 0x00;
            temp_b_array[7] = 0x00;
            temp_b_array[8] = 0x00;
            temp_b_array[9] = 0x00;
            temp_b_array[10] = DS_CRC.CRC_BCC_XOR(temp_b_array, 1, 9);
            temp_b_array_length = 11;

            /* Data sending with repeat facility */
            if (checkBox_SendRepeat.Checked == true)
            {
                serial_port.write(temp_b_array, 0, temp_b_array_length, false, true, Convert.ToInt32(textBox_SendRepeatTime.Text), Convert.ToInt32(textBox_SendRepeatNoOfTimes.Text));
            }
            else
            {
                serial_port.write(temp_b_array, 0, temp_b_array_length, false);
            }
        }

        private void buttonStringFilterConvert_Click(object sender, EventArgs e)
        {
            string input_ascii_string = String.Empty;
            string output_string = String.Empty;
            string output_format = String.Empty;

            /* getting output format */
            if (radiobuttonToolsOutputTextBoxHEX.Checked == true)
            {
                output_format = "HEX";
            }
            else if (radiobuttonToolsOutputTextBoxHEXSpaced.Checked == true)
            {
                output_format = "HEXSpaced";
            }
            else if (radiobuttonToolsOutputTextBoxASCII.Checked == true)
            {
                output_format = "ASCII";
            }

            /* Verify valid input data and making ascii string */
            if (radioButtonToolsInputTextBoxHEX.Checked == true)
            {
                if (DS_Functions.CheckValidHexString(textBox_ToolsInputString.Text) == true)
                {
                    input_ascii_string = DS_Functions.hex_string_to_ascii_string(textBox_ToolsInputString.Text);
                }
            }
            else if (radioButtonToolsInputTextBoxHEXSpaced.Checked == true)
            {
                if (DS_Functions.CheckValidHexString(DS_Functions.string_subtract(textBox_ToolsInputString.Text, " ", "")) == true)
                {
                    input_ascii_string = DS_Functions.hex_string_to_ascii_string(DS_Functions.string_subtract(textBox_ToolsInputString.Text, " ", ""));
                }
            }
            else if (radioButtonToolsInputTextBoxASCII.Checked == true)
            {
                input_ascii_string = textBox_ToolsInputString.Text;
            }


            if (string.IsNullOrEmpty(input_ascii_string) == false)
            {
                string ascii_str = string.Empty;


                /* removing the desired characters */
                if (checkBoxStringFilterRemoveSpace.Checked == true)
                {
                    ascii_str = DS_Functions.string_subtract(input_ascii_string, " ", "");
                }
                if (checkBoxStringFilterRemoveCR.Checked == true)
                {
                    ascii_str = DS_Functions.string_subtract(ascii_str, "\r", "");
                }
                if (checkBoxStringFilterRemoveLF.Checked == true)
                {
                    ascii_str = DS_Functions.string_subtract(ascii_str, "\n", "");
                }
                if (checkBoxStringFilterRemoveTab.Checked == true)
                {
                    ascii_str = DS_Functions.string_subtract(ascii_str, "\t", "");
                }


                /* getting output formatter */
                if (string.Equals(output_format, "HEX"))
                {
                    output_string = DS_Functions.ascii_string_to_hex_string(ascii_str);
                }
                else if (string.Equals(output_format, "HEXSpaced"))
                {
                    output_string = DS_Functions.ascii_string_to_hex_string_spaced(ascii_str);
                }
                else if (string.Equals(output_format, "ASCII"))
                {
                    output_string = ascii_str;
                }

                textBox_ToolsOutputString.Text = output_string;
            }
            else
            {
                MessageBox.Show("Invalid input String", "Error");
            }
        }

        private void button_ReadStop_Click(object sender, EventArgs e)
        {

        }

        private void buttonWordwrapConvert_Click(object sender, EventArgs e)
        {
            string input_string, output_string = "";
            input_string = textBox_ToolsInputString.Text;
            int wrap_len = Convert.ToInt16(textBoxWordwrapLength.Text);

            for (int i = 0; i < 1 + (input_string.Length / wrap_len); i++)
            {
                if ((i + 1) * wrap_len > input_string.Length)
                {
                    output_string += input_string.Substring(i * wrap_len, input_string.Length - i * wrap_len);
                }
                else
                {
                    output_string += input_string.Substring(i * wrap_len, wrap_len);
                }
                output_string += newline;
            }
            textBox_ToolsOutputString.Text = output_string;

        }

        /* calculate checsum/fcs etc */
        private void button_ToolsCalculateChecksum_Click(object sender, EventArgs e)
        {
            if (textBox_ToolsInputString.Text != String.Empty)
            {
                byte[] barray = null;
                bool input_string_error = false;
                if (radioButtonToolsInputTextBoxHEX.Checked == true || radioButtonToolsInputTextBoxHEXSpaced.Checked == true)
                {
                    if (DS_Functions.CheckValidHexSpacedString(textBox_ToolsInputString.Text) == true)
                    {
                        barray = DS_Functions.hex_string_to_byte_array(textBox_ToolsInputString.Text.Replace(" ", ""));
                    }
                    else
                    {
                        input_string_error = true;
                    }
                }
                else
                {
                    barray = DS_Functions.ascii_string_to_byte_array(textBox_ToolsInputString.Text);
                }

                if (input_string_error == false)
                {
                    textBox_ToolsChecksumMemory.Text = DS_Functions.byte_to_hex(DS_CRC.CRC_MEM(barray, 0, barray.Length));
                    textBox_ToolsChecksumBCCXOR.Text = DS_Functions.byte_to_hex(DS_CRC.CRC_BCC_XOR(barray, 0, barray.Length));
                    textBox_ToolsChecksumCRC16.Text = DS_Functions.int_to_hex(DS_CRC.get_fcs(barray, 0, barray.Length));
                }
                else
                {
                    MessageBox.Show("Error in input string", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void button_SendFramePaste_Click(object sender, EventArgs e)
        {
            textBox_SendFrame.Text = Clipboard.GetText();
        }

        private void button_SendFrameClear_Click(object sender, EventArgs e)
        {
            textBox_SendFrame.Text = String.Empty;
        }

        private void buttonDataTrafficCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox_DataTraffic.Text);
        }

        private void buttonDataTrafficClear_Click(object sender, EventArgs e)
        {
            textBox_DataTraffic.Text = String.Empty;
            DS_Serial.totalRxBytes = 0;
            DS_Serial.totalTxBytes = 0;
            textBox_DataTrafficTxBytesTotal.Text = "0";
            textBox_DataTrafficRxBytesTotal.Text = "0";
        }

        private void ToolStripMenuItem_Connect_Click(object sender, EventArgs e)
        {
            if (radioButton_CommunicationSerial.Checked == true)
            {
                if (serial_port.Connect(comboBox_SerialSingleCOMPORT.Text, Convert.ToInt32(comboBox_SerialSingleBaudRate.Text)) == true)
                {

                }
            }
        }

        private void ToolStripMenuItem_Disconnect_Click(object sender, EventArgs e)
        {
            if (radioButton_CommunicationSerial.Checked == true)
            {
                if (serial_port.Disconnect() == true)
                {

                }
            }
        }
        private void timer1sec_Tick(object sender, EventArgs e)
        {

        }

        private void timerText500ms_Tick(object sender, EventArgs e)
        {
            bool connection_status;

            /* enable/disable functionlity as per flags */
            if (checkBox_SendRepeat.Checked == true)
            {
                textBox_SendRepeatTime.Enabled = true;
                textBox_SendRepeatNoOfTimes.Enabled = true;
                if (serial_port.SendRepeatEnable == true)
                {
                    button_SendRepeatStop.Enabled = true;
                }
                else
                {
                    button_SendRepeatStop.Enabled = false;
                }
            }
            else
            {
                textBox_SendRepeatTime.Enabled = false;
                textBox_SendRepeatTime.Text = "1000";
                textBox_SendRepeatNoOfTimes.Enabled = false;
                textBox_SendRepeatNoOfTimes.Text = "100";
            }

            /* pop up notification when a port is connected or disconnected */
            DS_Serial.update_port_list();


            /* fill up information periodically */
            if (radioButton_CommunicationSerial.Checked == true && serial_port.IsOpen == true)
            {
                connection_status = true;
            }
            else
            {
                connection_status = false;
            }
            textBox_DataTrafficTxBytesTotal.Text = DS_Serial.totalTxBytes.ToString();
            textBox_DataTrafficRxBytesTotal.Text = DS_Serial.totalRxBytes.ToString();

            if (connection_status == true)                  /* Connected */
            {
                progressBar_connectionStatus.Value = 100;
                button_Send.Enabled = true;
                ToolStripMenuItem_Connect.Enabled = false;
                ToolStripMenuItem_Disconnect.Enabled = true;
            }
            else                                            /* Disconnected */
            {
                progressBar_connectionStatus.Value = 0;
                button_Send.Enabled = false;
                button_SendRepeatStop.Enabled = false;
                ToolStripMenuItem_Connect.Enabled = true;
                ToolStripMenuItem_Disconnect.Enabled = false;
            }
            try
            {
                textBox_SendRepeatSentCounter.Text = serial_port.SendRepeatSentCounter.ToString();
            }
            catch (Exception ex)
            {
                textBox_SendRepeatSentCounter.Text = ex.Message;
            }
            if (radioButton_DataTrafficFormatASCII.Checked == true)       /* ASCII */
            {
                traffic_mode = 1;
            }
            else if (radioButton_DataTrafficFormatHEX.Checked == true)  /* HEX */
            {
                traffic_mode = 2;
            }
            else if (radioButton_DataTrafficFormatHEXSpaced.Checked == true)  /* HEX Spaced */
            {
                traffic_mode = 3;
            }


            /* Metrology Data */
            textBox_VolR.Text = VolR.ToString("0.00");
            textBox_VolY.Text = VolY.ToString("0.00");
            textBox_VolB.Text = VolB.ToString("0.00");
            textBox_VolRdc.Text = VolRdc.ToString();
            textBox_VolYdc.Text = VolYdc.ToString();
            textBox_VolBdc.Text = VolBdc.ToString();

            textBox_CurrR.Text = CurrRSigned.ToString("0.000");
            textBox_CurrY.Text = CurrYSigned.ToString("0.000");
            textBox_CurrB.Text = CurrBSigned.ToString("0.000");
            textBox_CurrN.Text = CurrN.ToString("0.000");

            textBox_CurrRdc.Text = CurrRdc.ToString();
            textBox_CurrYdc.Text = CurrYdc.ToString();
            textBox_CurrBdc.Text = CurrBdc.ToString();
            textBox_CurrNdc.Text = CurrNdc.ToString();

            //textBox_PFR.Text = PFR.ToString("0.000");
            //textBox_PFY.Text = PFY.ToString("0.000");
            //textBox_PFB.Text = PFB.ToString("0.000");
            //textBox_PFNet.Text = PFNet.ToString("0.000");
            //
            //textBox_AnglePFRph.Text = AnglePFR.ToString("0.00");
            //textBox_AnglePFYph.Text = AnglePFY.ToString("0.00");
            //textBox_AnglePFBph.Text = AnglePFB.ToString("0.00");
            //
            //textBox_WattR.Text = WattR.ToString("0.0");
            //textBox_WattY.Text = WattY.ToString("0.0");
            //textBox_WattB.Text = WattB.ToString("0.0");
            //textBox_WattNet.Text = WattNet.ToString("0.0");
            //
            //textBox_VARR.Text = VARR.ToString("0.0");
            //textBox_VARY.Text = VARY.ToString("0.0");
            //textBox_VARB.Text = VARB.ToString("0.0");
            //textBox_VARNet.Text = VARNet.ToString("0.0");
            //
            //textBox_VAR.Text = VAR.ToString("0.0");
            //textBox_VAY.Text = VAY.ToString("0.0");
            //textBox_VAB.Text = VAB.ToString("0.0");
            //textBox_VANet.Text = VANet.ToString("0.0");
            //
            //textBox_FreqR.Text = FreqR.ToString("0.000");
            //textBox_FreqY.Text = FreqY.ToString("0.000");
            //textBox_FreqB.Text = FreqB.ToString("0.000");
            //textBox_FreqNet.Text = FreqNet.ToString("0.000");
            //
            //textBox_QuadR.Text = QuadrantR.ToString();
            //textBox_QuadY.Text = QuadrantY.ToString();
            //textBox_QuadB.Text = QuadrantB.ToString();
            //textBox_QuadNet.Text = QuadrantNet.ToString();
            //
            //textBox_CalAngleActR.Text = CalAngleActR.ToString("0.00");
            //textBox_CalAngleActY.Text = CalAngleActY.ToString("0.00");
            //textBox_CalAngleActB.Text = CalAngleActB.ToString("0.00");
            //
            //textBox_CalAngleReactR.Text = CalAngleReactR.ToString("0.00");
            //textBox_CalAngleReactY.Text = CalAngleReactY.ToString("0.00");
            //textBox_CalAngleReactB.Text = CalAngleReactB.ToString("0.00");
            //
            //textBox_SamplesR.Text = SamplesR.ToString();
            //textBox_SamplesY.Text = SamplesY.ToString();
            //textBox_SamplesB.Text = SamplesB.ToString();
            //textBox_SamplesPerSec.Text = SamplesPerSec.ToString();
            //textBox_SamplesN.Text = SamplesN.ToString();
            //
            //textBox_CurrNeuVector.Text = CurrNVector.ToString("0.000");
            //textBox_AngleNeuVector.Text = AngleNVector.ToString("0.00");
            //
            //textBox_Time.Text = Time;
            //
            //textBox_VolRY.Text = VolRY.ToString("0.00");
            //textBox_VolYB.Text = VolYB.ToString("0.00");
            //textBox_VolBR.Text = VolBR.ToString("0.00");
            //
            //textBox_AngleRY.Text = AngleRY.ToString("0.00");
            //textBox_AngleYB.Text = AngleYB.ToString("0.00");
            //textBox_AngleBR.Text = AngleBR.ToString("0.00");
            //
            //textBox_EnergyWhR.Text = EnergyWhR.ToString("0.0000");
            //textBox_EnergyWhY.Text = EnergyWhY.ToString("0.0000");
            //textBox_EnergyWhB.Text = EnergyWhB.ToString("0.0000");
            //textBox_EnergyWhTotal.Text = EnergyWhTotal.ToString("0.0000");
            //textBox_EnergyVARhLagTotal.Text = EnergyVARhLagTotal.ToString("0.0000");
            //textBox_EnergyVARhLeadTotal.Text = EnergyVARhLeadTotal.ToString("0.0000");
            //textBox_EnergyVAhTotal.Text = EnergyVAhTotal.ToString("0.0000");
            //
            //labelMetrologyTimer.Text = metrology_timer.ToString();
            //
            //textBox_ErrorActR.Text = error_act_r.ToString("0.00");
            //textBox_ErrorActY.Text = error_act_y.ToString("0.00");
            //textBox_ErrorActB.Text = error_act_b.ToString("0.00");
            //textBox_ErrorActTotal.Text = error_act_total.ToString("0.00");
            //
            //textBox_ErrorReactR.Text = error_react_r.ToString("0.00");
            //textBox_ErrorReactY.Text = error_react_y.ToString("0.00");
            //textBox_ErrorReactB.Text = error_react_b.ToString("0.00");
            //textBox_ErrorReactTotal.Text = error_react_total.ToString("0.00");
            //
            //textBox_ErrorAppR.Text = error_app_r.ToString("0.00");
            //textBox_ErrorAppY.Text = error_app_y.ToString("0.00");
            //textBox_ErrorAppB.Text = error_app_b.ToString("0.00");
            //textBox_ErrorAppTotal.Text = error_app_total.ToString("0.00");
            //
            //textBox_TempTLV.Text = temperature_tlv.ToString();
            //textBox_BatteryVoltage.Text = battery_voltage.ToString("0.00");
            //
            //textBox_ReactiveSamples.Text = reactiveSamples.ToString();
            //textBox_ReactiveTimer.Text = reactiveTimer.ToString();
            //textBox_ReactiveTimeDelay.Text = reactiveTimeDelay.ToString();
            //textBox_ReactiveTimeDeviation.Text = reactiveTimeDeviation.ToString();
            //
            //textBox_THDVr.Text = THDVr.ToString("0.0");
            //textBox_THDVy.Text = THDVy.ToString("0.0");
            //textBox_THDVb.Text = THDVb.ToString("0.0");
            //textBox_THDIr.Text = THDIr.ToString("0.0");
            //textBox_THDIy.Text = THDIy.ToString("0.0");
            //textBox_THDIb.Text = THDIb.ToString("0.0");
            //
            //textBox_EnergyWhTotalFunda.Text = EnergyWhTotalFunda.ToString("0.0000");
            //textBox_LoopCycles.Text = LoopCycles.ToString();
            //
            //textBox_TamperStatus.Text = "";
            //if (DS_Functions.checkBit(tamper_status[7], 0x80) == true) { textBox_TamperStatus.Text += " | bit63"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x40) == true) { textBox_TamperStatus.Text += " | bit62"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x20) == true) { textBox_TamperStatus.Text += " | bit61"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x10) == true) { textBox_TamperStatus.Text += " | bit60"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x08) == true) { textBox_TamperStatus.Text += " | bit59"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x04) == true) { textBox_TamperStatus.Text += " | bit58"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x02) == true) { textBox_TamperStatus.Text += " | bit57"; }
            //if (DS_Functions.checkBit(tamper_status[7], 0x01) == true) { textBox_TamperStatus.Text += " | bit56"; }
            //
            //if (DS_Functions.checkBit(tamper_status[6], 0x80) == true) { textBox_TamperStatus.Text += " | bit55"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x40) == true) { textBox_TamperStatus.Text += " | bit54"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x20) == true) { textBox_TamperStatus.Text += " | bit53"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x10) == true) { textBox_TamperStatus.Text += " | bit52"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x08) == true) { textBox_TamperStatus.Text += " | bit51"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x04) == true) { textBox_TamperStatus.Text += " | bit50"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x02) == true) { textBox_TamperStatus.Text += " | bit49"; }
            //if (DS_Functions.checkBit(tamper_status[6], 0x01) == true) { textBox_TamperStatus.Text += " | bit48"; }
            //
            //if (DS_Functions.checkBit(tamper_status[5], 0x80) == true) { textBox_TamperStatus.Text += " | bit47"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x40) == true) { textBox_TamperStatus.Text += " | bit46"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x20) == true) { textBox_TamperStatus.Text += " | bit45"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x10) == true) { textBox_TamperStatus.Text += " | bit44"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x08) == true) { textBox_TamperStatus.Text += " | bit43"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x04) == true) { textBox_TamperStatus.Text += " | bit42"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x02) == true) { textBox_TamperStatus.Text += " | bit41"; }
            //if (DS_Functions.checkBit(tamper_status[5], 0x01) == true) { textBox_TamperStatus.Text += " | Faulty Capacitor"; }
            //
            //if (DS_Functions.checkBit(tamper_status[4], 0x80) == true) { textBox_TamperStatus.Text += " | RTC Battery Low"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x40) == true) { textBox_TamperStatus.Text += " | Over Cureent B"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x20) == true) { textBox_TamperStatus.Text += " | Over Current Y"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x10) == true) { textBox_TamperStatus.Text += " | Over Current R"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x08) == true) { textBox_TamperStatus.Text += " | Abnormal Frequency"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x04) == true) { textBox_TamperStatus.Text += " | Two wire"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x02) == true) { textBox_TamperStatus.Text += " | RTC Reading Error"; }
            //if (DS_Functions.checkBit(tamper_status[4], 0x01) == true) { textBox_TamperStatus.Text += " | bit32"; }
            //
            //if (DS_Functions.checkBit(tamper_status[3], 0x80) == true) { textBox_TamperStatus.Text += " | 35KV/ESD"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x40) == true) { textBox_TamperStatus.Text += " | Invalid phase association"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x20) == true) { textBox_TamperStatus.Text += " | Invalid voltage"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x10) == true) { textBox_TamperStatus.Text += " | High Neutral Current"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x08) == true) { textBox_TamperStatus.Text += " | Wrong connection"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x04) == true) { textBox_TamperStatus.Text += " | Main battery low"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x02) == true) { textBox_TamperStatus.Text += " | Low Load"; }
            //if (DS_Functions.checkBit(tamper_status[3], 0x01) == true) { textBox_TamperStatus.Text += " | Over Load"; }
            //
            //if (DS_Functions.checkBit(tamper_status[2], 0x80) == true) { textBox_TamperStatus.Text += " | EEPROM Fail"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x40) == true) { textBox_TamperStatus.Text += " | B Phase Active Export"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x20) == true) { textBox_TamperStatus.Text += " | Y Phase Active Export"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x10) == true) { textBox_TamperStatus.Text += " | R Phase Active Export"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x08) == true) { textBox_TamperStatus.Text += " | Phase sequence reverse"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x04) == true) { textBox_TamperStatus.Text += " | Top Cover"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x02) == true) { textBox_TamperStatus.Text += " | Low PF"; }
            //if (DS_Functions.checkBit(tamper_status[2], 0x01) == true) { textBox_TamperStatus.Text += " | Neutral Disturb"; }
            //
            //if (DS_Functions.checkBit(tamper_status[1], 0x80) == true) { textBox_TamperStatus.Text += " | Magnet"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x40) == true) { textBox_TamperStatus.Text += " | Over Current"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x20) == true) { textBox_TamperStatus.Text += " | CT Bypass"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x10) == true) { textBox_TamperStatus.Text += " | Current Unbalance"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x08) == true) { textBox_TamperStatus.Text += " | CY Open B"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x04) == true) { textBox_TamperStatus.Text += " | CT Open Y"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x02) == true) { textBox_TamperStatus.Text += " | CT Open R"; }
            //if (DS_Functions.checkBit(tamper_status[1], 0x01) == true) { textBox_TamperStatus.Text += " | CT Reverse B"; }
            //
            //if (DS_Functions.checkBit(tamper_status[0], 0x80) == true) { textBox_TamperStatus.Text += " | CT Reverse Y"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x40) == true) { textBox_TamperStatus.Text += " | CT Reverse R"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x20) == true) { textBox_TamperStatus.Text += " | Vol Unbalance"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x10) == true) { textBox_TamperStatus.Text += " | Vol Low"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x08) == true) { textBox_TamperStatus.Text += " | Vol High"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x04) == true) { textBox_TamperStatus.Text += " | Vol Miss B"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x02) == true) { textBox_TamperStatus.Text += " | Vol Miss Y"; }
            //if (DS_Functions.checkBit(tamper_status[0], 0x01) == true) { textBox_TamperStatus.Text += " | Vol Miss R"; }
            //
            //
            //textBox_MISCData.Text = MISCData;
            //
            ///* Vector Diagram Display */
            //if (ShowVectorDiag == true)
            //{
            //    if (v_diag.Visible == false)
            //    {
            //        v_diag.ShowDialog();
            //    }
            //    v_diag.Invalidate();  // request a delayed Repaint by the normal MessageLoop system    
            //    v_diag.Update();      // forces Repaint of invalidated area 
            //    v_diag.Refresh();     // Combines Invalidate() and Update()
            //}
        }

        private void comboBox_SerialSingleCOMPORT_Click(object sender, EventArgs e)
        {
            comboBox_SerialSingleCOMPORT.Items.Clear();
            comboBox_SerialSingleCOMPORT.Items.AddRange(DS_Serial.GetPortNames());
        }


        private void timer10ms_Tick(object sender, EventArgs e)
        {
            serial_port.serial_loop_10ms();
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            int send_frame_length = 0;

            /* validate the input frame */
            if (radioButton_SendFrameFormatHex.Checked == true)
            {
                if (DS_Functions.CheckValidHexSpacedString(textBox_SendFrame.Text) == false)
                {
                    MessageBox.Show("Invalid Input Data..!!");
                    return;
                }
            }

            /* calculating the length */
            send_frame_length = textBox_SendFrame.Text.Replace(" ", "").Length;

            if (radioButton_SendFrameFormatHex.Checked == true)
            {
                send_frame_length /= 2;
            }

            /* checking the length */
            if (checkBox_SendFrameHDLC.Checked == true)
            {
                if (send_frame_length >= 550 - 6)       /* 7E 7E A0 LEN FCS FCS */
                {
                    MessageBox.Show("Large Input Data..!!");
                    return;
                }
            }
            else
            {
                if (send_frame_length >= 550)
                {
                    MessageBox.Show("Large Input Data..!!");
                    return;
                }
            }

            /* making a byte aray from input data */
            if (radioButton_SendFrameFormatHex.Checked == true)
            {
                temp_b_array = DS_Functions.hex_string_to_byte_array(textBox_SendFrame.Text.Replace(" ", ""));
                temp_b_array_length = send_frame_length;
            }
            else
            {
                temp_b_array = DS_Functions.ascii_string_to_byte_array(textBox_SendFrame.Text);
                temp_b_array_length = send_frame_length;
            }

            if (checkBox_SendRepeat.Checked == true)
            {
                serial_port.write(temp_b_array, 0, temp_b_array_length, checkBox_SendFrameHDLC.Checked, true, Convert.ToInt32(textBox_SendRepeatTime.Text), Convert.ToInt32(textBox_SendRepeatNoOfTimes.Text));
            }
            else
            {
                serial_port.write(temp_b_array, 0, temp_b_array_length, checkBox_SendFrameHDLC.Checked);
            }
        }

        private void timer100ms_Tick(object sender, EventArgs e)
        {
            string s = String.Empty;// traffic_string;
            if (traffic_string.Length != 0)
            {
                s = traffic_string;
                traffic_string = String.Empty;
            }
            textBox_DataTraffic.AppendText(s);
            traffic_string = String.Empty;
        }

        private void textBox_InputVr_Click(object sender, EventArgs e)
        {
            textBox_InputVr.SelectAll();
        }
        private void textBox_InputVy_Click(object sender, EventArgs e)
        {
            textBox_InputVy.SelectAll();
        }
        private void textBox_InputVb_Click(object sender, EventArgs e)
        {
            textBox_InputVb.SelectAll();
        }
        private void textBox_InputIr_Click(object sender, EventArgs e)
        {
            textBox_InputIr.SelectAll();
        }
        private void textBox_InputIy_Click(object sender, EventArgs e)
        {
            textBox_InputIy.SelectAll();
        }
        private void textBox_InputIb_Click(object sender, EventArgs e)
        {
            textBox_InputIb.SelectAll();
        }
        private void textBox_InputAngr_Click(object sender, EventArgs e)
        {
            textBox_InputAngr.SelectAll();
        }
        private void textBox_InputAngy_Click(object sender, EventArgs e)
        {
            textBox_InputAngy.SelectAll();
        }
        private void textBox_InputAngb_Click(object sender, EventArgs e)
        {
            textBox_InputAngb.SelectAll();
        }

        private void textBox_InputAngRY_Click(object sender, EventArgs e)
        {
            textBox_InputAngRY.SelectAll();
        }

        private void textBox_InputAngRB_Click(object sender, EventArgs e)
        {
            textBox_InputAngRB.SelectAll();
        }

        private void textBox_InputFreq_Click(object sender, EventArgs e)
        {
            textBox_InputFreq.SelectAll();
        }

        private void textBox_ErrorAvg_Click(object sender, EventArgs e)
        {
            textBox_ErrorAvg.SelectAll();
        }

        private void button_InputUpdate_Click(object sender, EventArgs e)
        {
            ip_vol_r = Convert.ToDouble(textBox_InputVr.Text);
            ip_vol_y = Convert.ToDouble(textBox_InputVy.Text);
            ip_vol_b = Convert.ToDouble(textBox_InputVb.Text);

            ip_curr_r = Convert.ToDouble(textBox_InputIr.Text);
            ip_curr_y = Convert.ToDouble(textBox_InputIy.Text);
            ip_curr_b = Convert.ToDouble(textBox_InputIb.Text);

            ip_ang_r = Convert.ToDouble(textBox_InputAngr.Text);
            ip_ang_y = Convert.ToDouble(textBox_InputAngy.Text);
            ip_ang_b = Convert.ToDouble(textBox_InputAngb.Text);

            ip_freq = Convert.ToDouble(textBox_InputFreq.Text);
            ip_ang_ry = Convert.ToDouble(textBox_InputAngRY.Text);
            ip_ang_rb = Convert.ToDouble(textBox_InputAngRB.Text);
            ip_avg_samples = Convert.ToInt16(textBox_ErrorAvg.Text);

            if (radioButton_InputModeFwd.Checked == true)
            {
                metering_mode = 1;                                          /* Forwarded Metering */
            }
            else if (radioButton_InputModeNet.Checked == true)
            {
                metering_mode = 2;                                          /* Net Metering */
            }
            else
            {
                metering_mode = 0;
            }

            cal_accuracy = checkBox_ErrorCalculateEnable.Checked;

            ip_watt_r = ip_vol_r * ip_curr_r * Math.Cos(Math.PI * ip_ang_r / 180.0);
            ip_watt_y = ip_vol_y * ip_curr_y * Math.Cos(Math.PI * ip_ang_y / 180.0);
            ip_watt_b = ip_vol_b * ip_curr_b * Math.Cos(Math.PI * ip_ang_b / 180.0);

            ip_var_r = ip_vol_r * ip_curr_r * Math.Sin(Math.PI * ip_ang_r / 180.0);
            ip_var_y = ip_vol_y * ip_curr_y * Math.Sin(Math.PI * ip_ang_y / 180.0);
            ip_var_b = ip_vol_b * ip_curr_b * Math.Sin(Math.PI * ip_ang_b / 180.0);

            ip_va_r = Math.Sqrt(ip_watt_r * ip_watt_r + ip_var_r * ip_var_r);
            ip_va_y = Math.Sqrt(ip_watt_y * ip_watt_y + ip_var_y * ip_var_y);
            ip_va_b = Math.Sqrt(ip_watt_b * ip_watt_b + ip_var_b * ip_var_b);

            if (ip_va_r != 0)
            {
                ip_pf_r = ip_watt_r / ip_va_r;
            }
            else
            {
                ip_pf_r = 1;
            }
            if (ip_va_y != 0)
            {
                ip_pf_y = ip_watt_y / ip_va_y;
            }
            else
            {
                ip_pf_y = 1;
            }
            if (ip_va_b != 0)
            {
                ip_pf_b = ip_watt_b / ip_va_b;
            }
            else
            {
                ip_pf_b = 1;
            }
            int qr, qy, qb;
            if (ip_watt_r >= 0 && ip_var_r >= 0)
            {
                qr = 1;
            }
            else if (ip_watt_r < 0 && ip_var_r >= 0)
            {
                qr = 2;
            }
            else if (ip_watt_r < 0 && ip_var_r < 0)
            {
                qr = 3;
            }
            else
            {
                qr = 4;
            }
            if (ip_watt_y >= 0 && ip_var_y >= 0)
            {
                qy = 1;
            }
            else if (ip_watt_y < 0 && ip_var_y >= 0)
            {
                qy = 2;
            }
            else if (ip_watt_y < 0 && ip_var_y < 0)
            {
                qy = 3;
            }
            else
            {
                qy = 4;
            }
            if (ip_watt_b >= 0 && ip_var_b >= 0)
            {
                qb = 1;
            }
            else if (ip_watt_b < 0 && ip_var_b >= 0)
            {
                qb = 2;
            }
            else if (ip_watt_b < 0 && ip_var_b < 0)
            {
                qb = 3;
            }
            else
            {
                qb = 4;
            }

            ip_watt_total_fwd = Math.Abs(ip_watt_r) + Math.Abs(ip_watt_y) + Math.Abs(ip_watt_b);
            double temp = 0;
            if (qr == 1 || qr == 3)
                temp += Math.Abs(ip_var_r);
            else
                temp -= Math.Abs(ip_var_r);

            if (qy == 1 || qy == 3)
                temp += Math.Abs(ip_var_y);
            else
                temp -= Math.Abs(ip_var_y);

            if (qb == 1 || qb == 3)
                temp += Math.Abs(ip_var_b);
            else
                temp -= Math.Abs(ip_var_b);

            ip_var_total_fwd = temp;
            ip_va_total_fwd = Math.Sqrt(ip_watt_total_fwd * ip_watt_total_fwd + ip_var_total_fwd * ip_var_total_fwd);
            ip_pf_fwd = ip_watt_total_fwd / ip_va_total_fwd;

            ip_watt_total_net = ip_watt_r + ip_watt_y + ip_watt_b;
            ip_var_total_net = ip_var_r + ip_var_y + ip_var_b;
            ip_va_total_net = Math.Sqrt(ip_watt_total_net * ip_watt_total_net + ip_var_total_net * ip_var_total_net);
            ip_pf_net = ip_watt_total_net / ip_va_total_net;

            /* neutral current calculation Lag angle is +, Lead is - */
            double angle, mag;
            angle = 0;
            mag = ip_curr_r;
            Vector vect_ref = new Vector(mag * Math.Cos(angle * Math.PI / 180.0), mag * Math.Sin(angle * Math.PI / 180.0)); //ref is Vr

            angle = ip_ang_r;
            mag = ip_curr_r;
            Vector vect_ir = new Vector(mag * Math.Cos(angle * Math.PI / 180.0), mag * Math.Sin(angle * Math.PI / 180.0));

            angle = ip_ang_y + ip_ang_ry;
            mag = ip_curr_y;
            Vector vect_iy = new Vector(mag * Math.Cos(angle * Math.PI / 180.0), mag * Math.Sin(angle * Math.PI / 180.0));

            angle = ip_ang_b - ip_ang_rb;
            mag = ip_curr_b;
            Vector vect_ib = new Vector(mag * Math.Cos(angle * Math.PI / 180.0), mag * Math.Sin(angle * Math.PI / 180.0));

            Vector vect_in = Vector.Add(vect_ir, vect_iy);
            vect_in = Vector.Add(vect_in, vect_ib);
            ip_curr_n_calculated = vect_in.Length;
            if (ip_curr_n_calculated != 0)
            {
                ip_ang_n_calculated = Vector.AngleBetween(vect_in, vect_ref);
            }
            else
            {
                ip_ang_n_calculated = 0;
            }
            /* updating text boxes */
            textBox_InputWattR.Text = ip_watt_r.ToString("0.0");
            textBox_InputWattY.Text = ip_watt_y.ToString("0.0");
            textBox_InputWattB.Text = ip_watt_b.ToString("0.0");
            textBox_InputWattTotalFwd.Text = ip_watt_total_fwd.ToString("0.0");
            textBox_InputWattTotalNet.Text = ip_watt_total_net.ToString("0.0");

            textBox_InputVARR.Text = ip_var_r.ToString("0.0");
            textBox_InputVARY.Text = ip_var_y.ToString("0.0");
            textBox_InputVARB.Text = ip_var_b.ToString("0.0");
            textBox_InputVARTotalFwd.Text = ip_var_total_fwd.ToString("0.0");
            textBox_InputVARTotalNet.Text = ip_var_total_net.ToString("0.0");

            textBox_InputVAR.Text = ip_va_r.ToString("0.0");
            textBox_InputVAY.Text = ip_va_y.ToString("0.0");
            textBox_InputVAB.Text = ip_va_b.ToString("0.0");
            textBox_InputVATotalFwd.Text = ip_va_total_fwd.ToString("0.0");
            textBox_InputVATotalNet.Text = ip_va_total_net.ToString("0.0");

            textBox_InputPFR.Text = ip_pf_r.ToString("0.000");
            textBox_InputPFY.Text = ip_pf_y.ToString("0.000");
            textBox_InputPFB.Text = ip_pf_b.ToString("0.000");
            textBox_InputPFFwd.Text = ip_pf_fwd.ToString("0.000");
            textBox_InputPFNet.Text = ip_pf_net.ToString("0.000");

            textBox_NeuCurrentCalculated.Text = ip_curr_n_calculated.ToString("0.000");
            textBox_NeuCurrentAngleCalculated.Text = ip_ang_n_calculated.ToString("0.00");
        }

        public static void fillTrafficString(string header, byte[] data, int length)
        {
            traffic_string += Environment.NewLine;
            traffic_string += header;
            if (traffic_mode == 1)       /* ASCII */
            {
                traffic_string += DS_Functions.byte_array_to_ascii_string(data, length);

            }
            else if (traffic_mode == 2)  /* HEX */
            {
                traffic_string += DS_Functions.byte_array_to_hex_string(data, length);
            }
            else if (traffic_mode == 3)  /* HEX Spaced */
            {
                traffic_string += DS_Functions.byte_array_to_hex_string_spaced(data, length);
            }
        }
        public static void ProcessForm1String(byte[] b_array, int b_array_len)
        {
            ushort arr_ptr = 0;
            /* Checking for valid HDLC Frame */
            if (DS_HDLC.verify_hdlc_frame(b_array, b_array_len) == true)
            {
                DS_HDLC.decode_hdlc_frame(b_array);
                if (DS_HDLC.HdlcCommandCode == 0)                        /* waveform capture */
                {
                    //debug_string_for_textbox = DS_Functions.Byte_Array_To_HEX_String(b_array, 0, b_array_len);
                    //update_wf_capture_info = true;
                }
                else if (DS_HDLC.HdlcCommandCode == 15)
                {
                    //debug_string_for_textbox = DS_Functions.Byte_Array_To_HEX_String(b_array, 9, 288);
                    //update_cal_data = true;
                }
                else if (DS_HDLC.HdlcCommandCode == 1)               /* Instant Data frame */
                {
                    /* reading data into variables */

                    /* Displaying in the checkbox */

                    /* showing vector diagram */

                    /* taking log into file */
                    arr_ptr = 9;
                    VolR = DS_Functions.ByteArrayToUs32(b_array, arr_ptr, 100); arr_ptr += 4;
                    VolY = DS_Functions.ByteArrayToUs32(b_array, arr_ptr, 100); arr_ptr += 4;
                    VolB = DS_Functions.ByteArrayToUs32(b_array, arr_ptr, 100); arr_ptr += 4;

                    VolRdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 100); arr_ptr += 4;
                    VolYdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 100); arr_ptr += 4;
                    VolBdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 100); arr_ptr += 4;

                    CurrRSigned = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrYSigned = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrBSigned = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrN = DS_Functions.ByteArrayToUs32(b_array, arr_ptr, 1000); arr_ptr += 4;

                    CurrRdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrYdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrBdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;
                    CurrNdc = DS_Functions.ByteArrayToS32(b_array, arr_ptr, 1000); arr_ptr += 4;

                    //PFR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 59), 2) / 1000.0);
                    //PFY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 61), 2) / 1000.0);
                    //PFB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 63), 2) / 1000.0);
                    //PFNet = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 65), 2) / 1000.0);
                    //
                    //AnglePFR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 67), 2) / 100.0);
                    //AnglePFY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 69), 2) / 100.0);
                    //AnglePFB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 71), 2) / 100.0);
                    //
                    //WattR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 73), 4) / 10.0);
                    //WattY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 77), 4) / 10.0);
                    //WattB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 81), 4) / 10.0);
                    //WattNet = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 85), 4) / 10.0);
                    //
                    //VARR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 89), 4) / 10.0);
                    //VARY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 93), 4) / 10.0);
                    //VARB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 97), 4) / 10.0);
                    //VARNet = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 101), 4) / 10.0);
                    //
                    //VAR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 105), 4) / 10.0);
                    //VAY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 109), 4) / 10.0);
                    //VAB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 113), 4) / 10.0);
                    //VANet = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 117), 4) / 10.0);
                    //
                    //FreqR = (DS_Functions.ByteArrayToInt(b_array, 121) / 1000.0);
                    //FreqY = (DS_Functions.ByteArrayToInt(b_array, 123) / 1000.0);
                    //FreqB = (DS_Functions.ByteArrayToInt(b_array, 125) / 1000.0);
                    //FreqNet = (DS_Functions.ByteArrayToInt(b_array, 127) / 1000.0);
                    //
                    //QuadrantR = b_array[129];
                    //QuadrantY = b_array[130];
                    //QuadrantB = b_array[131];
                    //QuadrantNet = b_array[132];
                    //
                    //CalAngleActR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 131), 2) / 100.0);
                    //CalAngleActY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 135), 2) / 100.0);
                    //CalAngleActB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 137), 2) / 100.0);
                    //
                    //CalAngleReactR = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 139), 2) / 100.0);
                    //CalAngleReactY = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 141), 2) / 100.0);
                    //CalAngleReactB = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 143), 2) / 100.0);
                    //
                    //SamplesR = DS_Functions.ByteArrayToInt(b_array, 145);
                    //SamplesY = DS_Functions.ByteArrayToInt(b_array, 147);
                    //SamplesB = DS_Functions.ByteArrayToInt(b_array, 149);
                    //SamplesPerSec = DS_Functions.ByteArrayToInt(b_array, 151);
                    //SamplesN = DS_Functions.ByteArrayToInt(b_array, 153);
                    //
                    //CurrNVector = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToLong(b_array, 155), 4) / 1000.0);
                    //AngleNVector = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 159), 2) / 100.0);
                    //
                    //Time = receive_data[161].ToString("D2") + ":" + receive_data[162].ToString("D2") + ":" + receive_data[163].ToString("D2") + " " +
                    //    receive_data[164].ToString("D2") + "/" + receive_data[165].ToString("D2") + "/" + receive_data[166].ToString("D2");
                    //
                    //VolRY = (DS_Functions.ByteArrayToInt(b_array, 167) / 100.0);
                    //VolYB = (DS_Functions.ByteArrayToInt(b_array, 169) / 100.0);
                    //VolBR = (DS_Functions.ByteArrayToInt(b_array, 171) / 100.0);
                    //
                    //AngleRY = DS_Functions.ByteArrayToInt(b_array, 173) / 100.0;
                    //AngleYB = DS_Functions.ByteArrayToInt(b_array, 175) / 100.0;
                    //AngleBR = DS_Functions.ByteArrayToInt(b_array, 177) / 100.0;
                    //
                    //EnergyWhR = (DS_Functions.ByteArrayToLong(b_array, 179) / 1000.0);
                    //EnergyWhY = (DS_Functions.ByteArrayToLong(b_array, 183) / 1000.0);
                    //EnergyWhB = (DS_Functions.ByteArrayToLong(b_array, 187) / 1000.0);
                    //EnergyWhTotal = (DS_Functions.ByteArrayToLong(b_array, 191) / 1000.0);
                    //EnergyVARhLagTotal = (DS_Functions.ByteArrayToLong(b_array, 195) / 1000.0);
                    //EnergyVARhLeadTotal = (DS_Functions.ByteArrayToLong(b_array, 199) / 1000.0);
                    //EnergyVAhTotal = (DS_Functions.ByteArrayToLong(b_array, 203) / 1000.0);
                    //
                    //pulseEnergyWhR = b_array[207];
                    //pulseEnergyWhY = b_array[208];
                    //pulseEnergyWhB = b_array[209];
                    //pulseEnergyWhTotal = b_array[210];
                    //pulseEnergyVARhLagTotal = b_array[211];
                    //pulseEnergyVARhLeadTotal = b_array[212];
                    //pulseEnergyVAhTotal = b_array[213];
                    //
                    //EnergyWhR += QUANTA * (pulseEnergyWhR / PULSE);
                    //EnergyWhY += QUANTA * (pulseEnergyWhY / PULSE);
                    //EnergyWhB += QUANTA * (pulseEnergyWhB / PULSE);
                    //EnergyWhTotal += QUANTA * (pulseEnergyWhTotal / PULSE);
                    //EnergyVARhLagTotal += QUANTA * (pulseEnergyVARhLagTotal / PULSE);
                    //EnergyVARhLeadTotal += QUANTA * (pulseEnergyVARhLeadTotal / PULSE);
                    //EnergyVAhTotal += QUANTA * (pulseEnergyVAhTotal / PULSE);
                    //
                    //EnergyWhR += GetPulseWeight[pulseEnergyWhR % PULSE];
                    //EnergyWhY += GetPulseWeight[pulseEnergyWhY % PULSE];
                    //EnergyWhB += GetPulseWeight[pulseEnergyWhB % PULSE];
                    //EnergyWhTotal += GetPulseWeight[pulseEnergyWhTotal % PULSE];
                    //EnergyVARhLagTotal += GetPulseWeight[pulseEnergyVARhLagTotal % PULSE];
                    //EnergyVARhLeadTotal += GetPulseWeight[pulseEnergyVARhLeadTotal % PULSE];
                    //EnergyVAhTotal += GetPulseWeight[pulseEnergyVAhTotal % PULSE];
                    //
                    //metrology_timer = DS_Functions.ByteArrayToInt(b_array, 214);
                    //
                    //temperature_tlv = (DS_Functions.CheckForNegativeValue(DS_Functions.ByteArrayToInt(b_array, 216), 2));
                    //battery_voltage = DS_Functions.ByteArrayToInt(b_array, 218) / 100.0;
                    //
                    //reactiveSamples = b_array[220];
                    //reactiveTimer = DS_Functions.ByteArrayToInt(b_array, 221);
                    //reactiveTimeDelay = DS_Functions.ByteArrayToInt(b_array, 223);
                    //reactiveTimeDeviation = DS_Functions.ByteArrayToInt(b_array, 225);
                    //
                    //THDVr = DS_Functions.ByteArrayToInt(b_array, 227) / 10.0;
                    //THDVy = DS_Functions.ByteArrayToInt(b_array, 229) / 10.0;
                    //THDVb = DS_Functions.ByteArrayToInt(b_array, 231) / 10.0;
                    //THDIr = DS_Functions.ByteArrayToInt(b_array, 233) / 10.0;
                    //THDIy = DS_Functions.ByteArrayToInt(b_array, 235) / 10.0;
                    //THDIb = DS_Functions.ByteArrayToInt(b_array, 237) / 10.0;
                    //
                    //EnergyWhTotalFunda = (DS_Functions.ByteArrayToLong(b_array, 239) / 1000.0);
                    //pulseEnergyWhTotalFunda = b_array[243];
                    //EnergyWhTotalFunda += QUANTA * (pulseEnergyWhTotalFunda / PULSE);
                    //EnergyWhTotalFunda += GetPulseWeight[pulseEnergyWhTotalFunda % PULSE];
                    //LoopCycles = DS_Functions.ByteArrayToLong(b_array, 244);
                    //tamper_status[7] = b_array[248];
                    //tamper_status[6] = b_array[249];
                    //tamper_status[5] = b_array[250];
                    //tamper_status[4] = b_array[251];
                    //tamper_status[3] = b_array[252];
                    //tamper_status[2] = b_array[253];
                    //tamper_status[1] = b_array[254];
                    //tamper_status[0] = b_array[255];
                    //
                    //if (b_array_len >= 256)
                    //{
                    //    MISCData = DS_Functions.Byte_Array_To_ASCII_String(b_array, 256, b_array_len - 256);
                    //}
                    //else
                    //{
                    //    MISCData = string.Empty;
                    //}

                    ///* Calculating Error */
                    //error_act_r = 0; error_act_y = 0; error_act_b = 0; error_act_total = 0;
                    //error_react_r = 0; error_react_y = 0; error_react_b = 0; error_react_total = 0;
                    //error_app_r = 0; error_app_y = 0; error_app_b = 0; error_app_total = 0;
                    //
                    //if (ical_accuracy == true)
                    //{
                    //    if (iwattR != 0)
                    //    {
                    //        error_act_r = (WattR - iwattR) * 100 / iwattR;
                    //    }
                    //    if (iwattY != 0)
                    //    {
                    //        error_act_y = (WattY - iwattY) * 100 / iwattY;
                    //    }
                    //    if (iwattB != 0)
                    //    {
                    //        error_act_b = (WattB - iwattB) * 100 / iwattB;
                    //    }
                    //    if (iwattNet != 0)
                    //    {
                    //        error_act_total = (WattNet - iwattNet) * 100 / iwattNet;
                    //    }
                    //    if (ivarR != 0)
                    //    {
                    //        error_react_r = (VARR - ivarR) * 100 / ivarR;
                    //    }
                    //    if (ivarY != 0)
                    //    {
                    //        error_react_y = (VARY - ivarY) * 100 / ivarY;
                    //    }
                    //    if (ivarB != 0)
                    //    {
                    //        error_react_b = (VARB - ivarB) * 100 / ivarB;
                    //    }
                    //    if (ivarNet != 0)
                    //    {
                    //        error_react_total = (VARNet - ivarNet) * 100 / ivarNet;
                    //    }
                    //
                    //    if (ivaR != 0)
                    //    {
                    //        error_app_r = (VAR - ivaR) * 100 / ivaR;
                    //    }
                    //    if (ivaY != 0)
                    //    {
                    //        error_app_y = (VAY - ivaY) * 100 / ivaY;
                    //    }
                    //    if (ivaB != 0)
                    //    {
                    //        error_app_b = (VAB - ivaB) * 100 / ivaB;
                    //    }
                    //    if (ivaNet != 0)
                    //    {
                    //        error_app_total = (VANet - ivaNet) * 100 / ivaNet;
                    //    }
                    //}



                    ///* Data logging */
                    //if (logData_f == true)
                    //{
                    //    string pathName;
                    //    if (log_to_newfile_f == true)
                    //    {
                    //        pathName = "D:\\SerialTools\\" + newlogfileName + ".txt";
                    //    }
                    //    else
                    //    {
                    //        pathName = "D:\\SerialTools\\" + "SerialToolsLogFile" + ".txt";
                    //    }
                    //    string dir = @"D:\SerialTools";
                    //    if (!Directory.Exists(dir))
                    //    {
                    //        Directory.CreateDirectory(dir);
                    //    }
                    //    using (StreamWriter sw = File.AppendText(pathName))
                    //    {
                    //        sw.Write(metrology_timer.ToString()); sw.Write(tab);
                    //        sw.Write(Time); sw.Write(tab);
                    //        sw.Write(VolR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(VolY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(VolB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(VolRdc.ToString()); sw.Write(tab);
                    //        sw.Write(VolYdc.ToString()); sw.Write(tab);
                    //        sw.Write(VolBdc.ToString()); sw.Write(tab);
                    //        sw.Write(CurrR.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(CurrY.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(CurrB.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(CurrN.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(CurrRdc.ToString()); sw.Write(tab);
                    //        sw.Write(CurrYdc.ToString()); sw.Write(tab);
                    //        sw.Write(CurrBdc.ToString()); sw.Write(tab);
                    //        sw.Write(CurrNdc.ToString()); sw.Write(tab);
                    //        sw.Write(PFR.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(PFY.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(PFB.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(PFNet.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(AnglePFR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(AnglePFY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(AnglePFB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(WattR.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(WattY.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(WattB.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(WattNet.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VARR.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VARY.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VARB.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VARNet.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VAR.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VAY.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VAB.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(VANet.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(FreqR.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(FreqY.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(FreqB.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(FreqNet.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(QuadrantR.ToString()); sw.Write(tab);
                    //        sw.Write(QuadrantY.ToString()); sw.Write(tab);
                    //        sw.Write(QuadrantB.ToString()); sw.Write(tab);
                    //        sw.Write(QuadrantNet.ToString()); sw.Write(tab);
                    //        sw.Write(CalAngleActR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(CalAngleActY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(CalAngleActB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(CalAngleReactR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(CalAngleReactY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(CalAngleReactB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(SamplesR.ToString()); sw.Write(tab);
                    //        sw.Write(SamplesY.ToString()); sw.Write(tab);
                    //        sw.Write(SamplesB.ToString()); sw.Write(tab);
                    //        sw.Write(SamplesPerSec.ToString()); sw.Write(tab);
                    //        sw.Write(SamplesN.ToString()); sw.Write(tab);
                    //        sw.Write(CurrNVector.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(AngleNVector.ToString("0.000")); sw.Write(tab);
                    //        sw.Write(VolRY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(VolYB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(VolBR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(AngleRY.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(AngleYB.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(AngleBR.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(EnergyWhR.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyWhY.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyWhB.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyWhTotal.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyVARhLagTotal.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyVARhLeadTotal.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(EnergyVAhTotal.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(error_act_r.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_act_y.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_act_b.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_act_total.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_react_r.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_react_y.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_react_b.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_react_total.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_app_r.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_app_y.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_app_b.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(error_app_total.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(temperature_tlv.ToString()); sw.Write(tab);
                    //        sw.Write(battery_voltage.ToString("0.00")); sw.Write(tab);
                    //        sw.Write(reactiveSamples.ToString()); sw.Write(tab);
                    //        sw.Write(reactiveTimer.ToString()); sw.Write(tab);
                    //        sw.Write(reactiveTimeDelay.ToString()); sw.Write(tab);
                    //        sw.Write(reactiveTimeDeviation.ToString()); sw.Write(tab);
                    //        sw.Write(THDVr.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(THDVy.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(THDVb.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(THDIr.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(THDIy.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(THDIb.ToString("0.0")); sw.Write(tab);
                    //        sw.Write(EnergyWhTotalFunda.ToString("0.0000")); sw.Write(tab);
                    //        sw.Write(LoopCycles.ToString()); sw.Write(tab);
                    //        sw.Write(DS_Functions.Byte_Array_To_HEX_String(tamper_status)); sw.Write(tab);
                    //        sw.Write(MISCData); sw.Write(tab);
                    //        sw.Write(newline);
                    //        sw.Close();
                    //        sw.Dispose();
                    //    }
                    //}
                }
            }
        }
    }
}
