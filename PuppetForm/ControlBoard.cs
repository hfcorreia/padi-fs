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
                puppetMaster.createClient(Int32.Parse(clientPortTextBox.Text), Int32.Parse(clientNameTextBox.Text));
            }
        }

        private void startMetadataServersButton_Click(object sender, EventArgs e)
        {
            puppetMaster.startMetaDataServers(NUM_METADATA_SERVERS);

        }

        private void createDataServerButton_Click(object sender, EventArgs e)
        {
            puppetMaster.createDataServer(Int32.Parse(dataServerPortTextBox.Text), Int32.Parse(dataServerIdTextBox.Text));
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            puppetMaster.test();
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
            puppetMaster.open(Int32.Parse(clientNameTextBox.Text), FileNameTextBox.Text);
        }

        private void closeFileButton_Click(object sender, EventArgs e)
        {
            puppetMaster.close(Int32.Parse(clientNameTextBox.Text), FileNameTextBox.Text);
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            int clientId = Int32.Parse(clientNameTextBox.Text);
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
                puppetMaster.exeScript("change this", fileDialog.FileName);
            }
        }

    

    }
}
