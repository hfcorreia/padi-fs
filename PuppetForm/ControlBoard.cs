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
            try
            {
                puppetMaster.startMetaDataServers(NUM_METADATA_SERVERS);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error starting Metadata servers " + exception.Message);
            }
        }

        private void createClientButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(clientNameTextBox.Text))
            {
                try
                {
                    puppetMaster.createClient(clientNameTextBox.Text);
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show("Error creating client Metadata servers " + exception.Message);
                }
            }
        }

        private void createDataServerButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.createDataServer(dataServerIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error creating DS" + exception.Message);
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.exitAll();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error exiting" + exception.Message);
            }
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
            try {
                puppetMaster.open(clientNameTextBox.Text, FileNameTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error opening file" + exception.Message);
            }
        }

        private void closeFileButton_Click(object sender, EventArgs e)
        {
            try{
            puppetMaster.close(clientNameTextBox.Text, FileNameTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error closing file " + exception.Message);
            }
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            try{
            string clientId = clientNameTextBox.Text;
            int nDS = Int32.Parse(NumDsTextBox.Text);
            int rQ = Int32.Parse(ReadQuorumTextBox.Text);
            int wQ = Int32.Parse(WriteQuorumTextBox.Text);

            puppetMaster.create(clientId, CreateFileNameTextBox.Text, nDS, rQ, wQ);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error creating file \n" + exception.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try{
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    puppetMaster.LoadedScriptReader = new System.IO.StreamReader(fileDialog.FileName);
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error loading script" + exception.Message);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.delete(clientNameTextBox.Text, FileNameTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error deleting file" + exception.Message);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
           
           System.Windows.Forms.MessageBox.Show("This is a stub, does nothing!");
        }

        private void nextStepButton_Click(object sender, EventArgs e)
        {
            try{
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
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error executing next step of script" + exception.Message);
            }
        }

        private void failMetaDataButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.fail(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing button" + exception.Message);
            }
        }

        private void recoverMetadataButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.recover(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering metadataserver" + exception.Message);
            }
        }

        private void freezeDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.freeze(dataServerIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error freezing DS" + exception.Message);
            }
        }

        private void UnfreezeDSButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.unfreeze(dataServerIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error unfreezing DS" + exception.Message);
            }
        }

        private void FailDSButton_Click(object sender, EventArgs e) 
        {
            try{
                puppetMaster.fail(dataServerIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing DS" + exception.Message);
            }
        }

        private void RecoverDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.recover(dataServerIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering DS" + exception.Message);
            }
        }


    }
}
