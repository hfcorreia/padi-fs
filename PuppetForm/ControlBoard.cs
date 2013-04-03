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
                System.Windows.Forms.MessageBox.Show("Error starting Metadata servers :\n" + exception.Message);
            }
        }

        private void createClientButton_Click(object sender, EventArgs e)
        {
            string clientId = clientNameTextBox.Text;
            if (!String.IsNullOrEmpty(clientId))
            {
                try
                {
                    puppetMaster.createClient(clientId);
                    updateClientsList();
                    setNextClientId();
                    selectClient(clientId);
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show("Error creating client:\n" + exception.Message);
                }
            }
        }

        private void createDataServerButton_Click(object sender, EventArgs e)
        {
            try
            {
                string newDataserverId = dataServerIdTextBox.Text;
                puppetMaster.createDataServer(newDataserverId);
                updateDataServersList();
                setNextDataServerId();
                selectDataServer(newDataserverId);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error creating DS:\n" + exception.Message);
            }
        }



        private void exitButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.exitAll();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error exiting:\n" + exception.Message);
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
                puppetMaster.open(getSelectedClient(), getSelectedFileRegister());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error opening file :\n" + exception.Message);
            }
        }

        private void closeFileButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.close(getSelectedClient(), getSelectedFileRegister());
                updateClientFileRegister(getSelectedClient());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error closing file :\n" + exception.Message);
            }
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            try{
                string clientId = getSelectedClient();
                string fileName = CreateFileNameTextBox.Text;
                int nDS = Int32.Parse(NumDsTextBox.Text);
                int rQ = Int32.Parse(ReadQuorumTextBox.Text);
                int wQ = Int32.Parse(WriteQuorumTextBox.Text);

                puppetMaster.create(clientId, fileName, nDS, rQ, wQ);

                updateClientFileRegister(clientId);
                selectFileRegister(fileName);
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
                System.Windows.Forms.MessageBox.Show("Error loading script:\n" + exception.Message);
            }
        }

        private void client_deleteFile(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.delete(getSelectedClient(), getSelectedFileRegister());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error deleting file :\n" + exception.Message);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {

            System.Windows.Forms.MessageBox.Show("This is a stub, does nothing! :\n");
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
                        System.Windows.Forms.MessageBox.Show("End of file! :\n");
                        puppetMaster.LoadedScriptReader.Close();
                        puppetMaster.LoadedScriptReader = null;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Script file not loaded :\n");
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error executing next step of script :\n" + exception.Message);
            }
        }

        private void failMetaDataButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.fail(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing button :\n" + exception.Message);
            }
        }

        private void recoverMetadataButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.recover(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering metadataserver :\n" + exception.Message);
            }
        }

        private void freezeDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.freeze(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error freezing DS :\n" + exception.Message);
            }
        }

        private void UnfreezeDSButton_Click(object sender, EventArgs e)
        {
            try{
                puppetMaster.unfreeze(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error unfreezing DS :\n" + exception.Message);
            }
        }

        private void FailDSButton_Click(object sender, EventArgs e) 
        {
            try{
                puppetMaster.fail(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing DS :\n" + exception.Message);
            }
        }

        private void RecoverDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.recover(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering DS :\n" + exception.Message);
            }
        }

        //selected DS
        private string getSelectedDS() 
        {
            return (string) dataServersListBox.Items[dataServersListBox.SelectedIndex];
        }

        private void updateDataServersList() 
        {
            dataServersListBox.Items.Clear();
            foreach (string dataserverId in puppetMaster.dataServers.Keys)
            {
                dataServersListBox.Items.Add(dataserverId);
            }
        }

        private void selectDataServer(string newDataserverId)
        {
            for (int index = 0; index < dataServersListBox.Items.Count; ++index)
            {
                if (newDataserverId.Equals(dataServersListBox.Items[index]))
                {
                    dataServersListBox.SetSelected(index, true);
                    break;
                }
            }
        }
        private void setNextDataServerId()
        {
            string dataserverPrefix = "d-";
            dataServerIdTextBox.Text = dataserverPrefix + (dataServersListBox.Items.Count + 1);
        }

        //selected Client
        private string getSelectedClient()
        {
            return (string) ClientsListBox.Items[ClientsListBox.SelectedIndex];
        }

        private void updateClientsList()
        {
            ClientsListBox.Items.Clear();
            foreach (string clientId in puppetMaster.clients.Keys)
            {
                ClientsListBox.Items.Add(clientId);
            }
        }

        private void selectClient(string clientId)
        {
            for (int index = 0; index < ClientsListBox.Items.Count; ++index)
            {
                if (clientId.Equals(ClientsListBox.Items[index]))
                {
                    ClientsListBox.SetSelected(index, true);
                    break;
                }
            }
            updateClientFileRegister(clientId);
            updateClientStringRegister(clientId);
        }

        private void selectFileRegister(string fileName)
        {
            for (int index = 0; index < clientFileRegisterlistBox.Items.Count; ++index)
            {
                if (fileName.Equals(clientFileRegisterlistBox.Items[index]))
                {
                    clientFileRegisterlistBox.SetSelected(index, true);
                    break;
                }
            }
        }

        private void selectStringRegister(string fileName)
        {
            for (int index = 0; index < clientStringRegisterListBox.Items.Count; ++index)
            {
                if (fileName.Equals(clientStringRegisterListBox.Items[index]))
                {
                    clientStringRegisterListBox.SetSelected(index, true);
                    break;
                }
            }
        }

        private string getSelectedStringRegister()
        {
            return (string) clientStringRegisterListBox.Items[clientStringRegisterListBox.SelectedIndex];
        }

        private string getSelectedFileRegister()
        {
            return (string) clientFileRegisterlistBox.Items[clientFileRegisterlistBox.SelectedIndex];
        }


        private void setNextClientId()
        {
            string clientPrefix = "c-";
            clientNameTextBox.Text = clientPrefix + (ClientsListBox.Items.Count + 1);
        }

        private void updateClientFileRegister(string clientId) 
        {
            clientFileRegisterlistBox.Items.Clear();
            foreach (string register in puppetMaster.fileRegistersForClient(clientId))
            {
                clientFileRegisterlistBox.Items.Add(register);
            }
        }

        private void updateClientStringRegister(string clientId)
        {
            clientStringRegisterListBox.Items.Clear();
            foreach (string register in puppetMaster.stringRegistersForClient(clientId))
            {
                clientStringRegisterListBox.Items.Add(register);
            }
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void clientFileRegisterlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //updateClientFileRegister(getSelectedClient());
            updateClientStringRegister(getSelectedClient());
        }

        private void clientStringRegisterListBox_SelectedIndexChanged(object sender, EventArgs e){}

        private void ClientsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateClientFileRegister(getSelectedClient());
            updateClientStringRegister(getSelectedClient());
        }

        private void readFileButton_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void writeFileButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(byteArrayTextBox.Text))
            {
                puppetMaster.write(getSelectedClient(), getSelectedFileRegister(), byteArrayTextBox.Text);
                //updateClientFileRegister(getSelectedClient());
                updateClientStringRegister(getSelectedClient());
            }
            else {
                System.Windows.Forms.MessageBox.Show("Error: Please enter the content you want to write in the file.");
            }
        }

    }
}
