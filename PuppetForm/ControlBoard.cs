using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PuppetForm
{
    public partial class ControlBoard : Form
    {

        private PuppetMaster puppetMaster = new PuppetMaster();
        private static int NUM_METADATA_SERVERS = 3;

        public ControlBoard()
        {
            InitializeComponent();
        }

        private void createClientButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(clientPortTextBox.Text) && !String.IsNullOrEmpty(clientNameTextBox.Text))
            {
                puppetMaster.createClient(Int32.Parse(clientPortTextBox.Text), clientNameTextBox.Text);
            }
        }

        private void startMetadataServersButton_Click(object sender, EventArgs e)
        {
            puppetMaster.startMetaDataServers(NUM_METADATA_SERVERS);

        }

        private void createDataServerButton_Click(object sender, EventArgs e)
        {
            puppetMaster.createDataServer(Int32.Parse(dataServerPortTextBox.Text), dataServerIdTextBox.Text);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            puppetMaster.exitAll();
        }

        private void ControlBoard_Load(object sender, EventArgs e)
        {

        }

        private void ControlBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            //puppetMaster.exitAll();
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            puppetMaster.open(clientNameTextBox.Text, FileNameTextBox.Text);
        }

        private void closeFileButton_Click(object sender, EventArgs e)
        {
            puppetMaster.close(clientNameTextBox.Text, FileNameTextBox.Text);
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            string clientId = clientNameTextBox.Text;
            int nDS = Int32.Parse(NumDsTextBox.Text);
            int rQ = Int32.Parse(ReadQuorumTextBox.Text);
            int wQ = Int32.Parse(WriteQuorumTextBox.Text);

            puppetMaster.create(clientId, CreateFileNameTextBox.Text, nDS, rQ, wQ);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                puppetMaster.LoadedScriptReader = new System.IO.StreamReader(fileDialog.FileName);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            puppetMaster.delete(clientNameTextBox.Text, FileNameTextBox.Text);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("This is a stub, does nothing!");
        }

        private void nextStepButton_Click(object sender, EventArgs e)
        {
            if (puppetMaster.LoadedScriptReader != null)
            {
                String line = puppetMaster.LoadedScriptReader.ReadLine();

                if (line != null)
                {
                    puppetMaster.exeScriptCommand(line);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("End of file!");
                    puppetMaster.LoadedScriptReader.Close();
                    puppetMaster.LoadedScriptReader = null;
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Script file not loaded");
            }
        }

    }
}
