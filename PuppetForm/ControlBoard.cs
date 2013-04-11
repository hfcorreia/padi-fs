using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonTypes;

namespace PuppetForm
{
    public partial class ControlBoard : Form
    {
        #region variables

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the variables                      ///
        ////////////////////////////////////////////////////////////////////////////

        private PuppetMaster puppetMaster;
        private static int NUM_METADATA_SERVERS = 3;
        #endregion variables

        #region initialization

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the initialization                 ///
        ////////////////////////////////////////////////////////////////////////////

        public ControlBoard()
        {
            InitializeComponent();
            try
            {
                puppetMaster = new PuppetMaster(this);
                puppetMaster.startMetaDataServers(NUM_METADATA_SERVERS);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error starting Metadata servers " + exception.Message +" :\n" + exception.StackTrace);
            }
        }

        #endregion initialization

        #region verifications

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the verfications                   ///
        ////////////////////////////////////////////////////////////////////////////

        private void verifyStringRegisterIdSelection()
        {
            if (clientStringRegisterListBox.Items.Count == 0 || clientStringRegisterListBox.SelectedIndex < 0)
            {
                throw new Exception("Please select a string register");
            }
        }

        private void verifyClientSelection()
        {
            if (ClientsListBox.Items.Count == 0 || ClientsListBox.SelectedIndex < 0)
            {
                throw new Exception("Please select a user");
            }
        }

        private void verifyNewClientName()
        {
            if (String.IsNullOrEmpty(clientNameTextBox.Text))
            {
                throw new Exception("Please enter a file name");
            }
        }

        private void verifyFileRegisterIdSelection()
        {
            if (clientFileRegisterlistBox.Items.Count == 0 || clientFileRegisterlistBox.SelectedIndex < 0)
            {
                throw new Exception("Please select a file register");
            }
        }

        private void verifyNewFileName()
        {
            if (String.IsNullOrEmpty(fileNameTextBox.Text))
            {
                throw new Exception("Please specify a valid name for the file");
            }
        }

        private void verifyNewFileQuorunsAndServers()
        {
            int tempValue;
            if (String.IsNullOrEmpty(NumDsTextBox.Text) ||
                String.IsNullOrEmpty(ReadQuorumTextBox.Text) ||
                String.IsNullOrEmpty(WriteQuorumTextBox.Text) ||
                !Int32.TryParse(NumDsTextBox.Text, out tempValue) ||
                !Int32.TryParse(ReadQuorumTextBox.Text, out tempValue) ||
                !Int32.TryParse(WriteQuorumTextBox.Text, out tempValue))
            {
                throw new Exception("Please specify valid values for the quoruns." + "\n" +
                    "Given - #DS: " + NumDsTextBox.Text + ", #readQ: " + ReadQuorumTextBox.Text + ", #writeQ: " + WriteQuorum.Text);
            }
        }
        private void verifyNewByteArrayText()
        {
            if (String.IsNullOrEmpty(byteArrayTextBox.Text))
            {
                System.Windows.Forms.MessageBox.Show("Error: Please enter the content you want to write in the file.");
            }
        }

        private void verifyDataServerIdSelection()
        {
            if (dataServersListBox.Items.Count == 0)
            {
                throw new Exception("There are no dataservers yet");
            }

            if (dataServersListBox.SelectedIndex < 0)
            {
                throw new Exception("Please select a dataserver");
            }
        }

        private void verifyFullStringRegister()
        {
            if (clientStringRegisterListBox.Items.Count > 9)
            {
                System.Windows.Forms.MessageBox.Show("The string registers are all full, please specify one to be replaced");
                return;
            }
        }

        #endregion verifications

        #region events

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the events                        ///
        ////////////////////////////////////////////////////////////////////////////

        private void clientFileRegisterlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateClientStringRegister(getSelectedClient());
        }

        private void clientStringRegisterListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DO NOTHING
        }

        private void ClientsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateClientFileRegister(getSelectedClient());
            updateClientStringRegister(getSelectedClient());
        }

        private void readFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                verifyClientSelection();
                verifyFileRegisterIdSelection();

                int stringRegisterId;

                string processId = getSelectedClient();
                int fileRegisterId = clientFileRegisterlistBox.SelectedIndex;

                string readSemantics = "NOT DONE";

                if (replaceStringRegisterCheckBox.Checked)
                {
                    stringRegisterId = clientStringRegisterListBox.SelectedIndex;
                    //we want to substitute the selected string register
                    verifyStringRegisterIdSelection();
                }
                else
                {
                    verifyFullStringRegister();
                    //the string register is not full and we dont want to replace, so write on the next free position
                    stringRegisterId = clientStringRegisterListBox.Items.Count + 1;
                }

                puppetMaster.read(processId, fileRegisterId, readSemantics, stringRegisterId);
                updateClientStringRegister(processId);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.StackTrace);
            }
        }



        private void writeFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                verifyFileRegisterIdSelection();
                if (newContentCheckBox.Checked)
                {
                    verifyNewByteArrayText();
                    int selectedFileRegisterId = clientFileRegisterlistBox.SelectedIndex;
                    puppetMaster.write(getSelectedClient(), selectedFileRegisterId, byteArrayTextBox.Text);
                    updateClientStringRegister(getSelectedClient());
                }
                else
                {
                    verifyStringRegisterIdSelection();
                    int selectedFileRegisterId = clientFileRegisterlistBox.SelectedIndex;
                    int selectedStringRegisterId = clientStringRegisterListBox.SelectedIndex;

                    puppetMaster.write(getSelectedClient(), selectedFileRegisterId, selectedStringRegisterId);
                    updateClientStringRegister(getSelectedClient());
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error writing file:" + exception.Message +" :\n" + exception.StackTrace);
            }
        }

        private void openFileByNameCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            fileNameTextBox.Enabled = openFileByNameCheckbox.Checked;
        }

        private void createClientButton_Click(object sender, EventArgs e)
        {
            try
            {
                verifyNewClientName();
                string clientId = clientNameTextBox.Text;
                puppetMaster.createClient(clientId);
                updateClientsList();
                setNextClientId();
                selectClient(clientId);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error creating client:" + exception.Message + " :\n" + exception.StackTrace);
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
                System.Windows.Forms.MessageBox.Show("Error creating DS:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }



        private void exitButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.exitAll();
                Application.Exit();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error exiting:" + exception.Message + " :\n" + exception.StackTrace);
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
            try
            {
                verifyClientSelection();
                if (openFileByNameCheckbox.Checked)
                {
                    verifyNewClientName();
                    puppetMaster.open(getSelectedClient(), fileNameTextBox.Text);
                }
                else
                {
                    verifyFileRegisterIdSelection();
                    puppetMaster.open(getSelectedClient(), getSelectedFileRegisterText());
                }
                updateClientFileRegister(getSelectedClient());
                updateClientStringRegister(getSelectedClient());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error opening file:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void closeFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                verifyClientSelection();
                if (openFileByNameCheckbox.Checked)
                {
                    verifyNewFileName();
                    puppetMaster.close(getSelectedClient(), getSelectedFileName());
                }
                else
                {
                    verifyFileRegisterIdSelection();
                    puppetMaster.close(getSelectedClient(), getSelectedFileRegisterText());
                }

                updateClientFileRegister(getSelectedClient());
                updateClientStringRegister(getSelectedClient());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error closing file:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                verifyClientSelection();
                verifyNewFileName();
                verifyNewFileQuorunsAndServers();

                string clientId = getSelectedClient();
                string fileName = getSelectedFileName();
                int nDS = Int32.Parse(NumDsTextBox.Text);
                int rQ = Int32.Parse(ReadQuorumTextBox.Text);
                int wQ = Int32.Parse(WriteQuorumTextBox.Text);

                puppetMaster.create(clientId, fileName, nDS, rQ, wQ);

                updateClientFileRegister(clientId);
                updateClientStringRegister(clientId);
                selectFileRegister(fileName);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error creating file:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }



        private void client_deleteFile(object sender, EventArgs e)
        {
            try
            {
                verifyFileRegisterIdSelection();
                puppetMaster.delete(getSelectedClient(), getSelectedFileRegisterText());
                updateClientFileRegister(getSelectedClient());
                updateClientStringRegister(getSelectedClient());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error deleting file :" + exception.Message + " :\n" + exception.StackTrace);
            }

        }

        private void loadScript_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    puppetMaster.ScriptExecutor = new PuppetScriptExecutor(puppetMaster, fileDialog.FileName);
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void runScript_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.exeScript(false);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error executing script:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void nextStepButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.exeScript(true);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void failMetaDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.failMD(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing button:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void recoverMetadataButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.recoverMD(mdIdTextBox.Text);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering metadataserver :" + exception.Message + " :\n" + exception.StackTrace);
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
                System.Windows.Forms.MessageBox.Show("Error freezing DS :" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void UnfreezeDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.unfreeze(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error unfreezing DS :" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void FailDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.failDS(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error failing DS:" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void RecoverDSButton_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.recoverDS(getSelectedDS());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering DS :" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        private void newContentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            byteArrayTextBox.Enabled = newContentCheckBox.Checked;
        }

        private void dumpAllMds_Click(object sender, EventArgs e)
        {
            try
            {
                puppetMaster.dumpAllMds();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
            }
        }


        #endregion events

        #region getters

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the getters                        ///
        ////////////////////////////////////////////////////////////////////////////

        private string getSelectedFileName()
        {
            return fileNameTextBox.Text;
        }

        private string getSelectedDS()
        {
            return (string)dataServersListBox.Items[dataServersListBox.SelectedIndex];
        }

        private string getSelectedClient()
        {
            return (string)ClientsListBox.Items[ClientsListBox.SelectedIndex];
        }

        private string getSelectedStringRegisterText()
        {
            return (string)clientStringRegisterListBox.Items[getSelectedStringRegister()];
        }

        private string getSelectedFileRegisterText()
        {
            return (string)clientFileRegisterlistBox.Items[getSelectedFileRegister()];
        }

        private int getSelectedStringRegister()
        {
            return clientStringRegisterListBox.SelectedIndex;
        }

        private int getSelectedFileRegister()
        {
            return clientFileRegisterlistBox.SelectedIndex;
        }

        private int getCopySourceRegisterId()
        {
            int source;
            if (Int32.TryParse(copySourceFileRegisterTextBox.Text, out source))
            {
                return source;
            }
            else
            {
                return -1;
            }
        }
        private int getCopyTargetRegisterId()
        {
            int target;
            if (Int32.TryParse(copyTargetFileRegisterTextBox.Text, out target))
            {
                return target;
            }
            else
            {
                return -1;
            }
        }
        private string getCopySalt()
        {
            return copySaltTextBox.Text;
        }
        private string getCopySemantics()
        {
            return (string) copySemanticsComboBox.Items[copySemanticsComboBox.SelectedIndex];
        }

        #endregion getters

        #region setters

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the setters                        ///
        ////////////////////////////////////////////////////////////////////////////

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

        private void setNextClientId()
        {
            string clientPrefix = "c-";
            clientNameTextBox.Text = clientPrefix + (ClientsListBox.Items.Count + 1);
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

        #endregion setters

        #region updates

        ////////////////////////////////////////////////////////////////////////////
        ///                        region for the updates                        ///
        ////////////////////////////////////////////////////////////////////////////

        private void updateDataServersList()
        {
            dataServersListBox.Items.Clear();
            foreach (string dataserverId in puppetMaster.dataServers.Keys)
            {
                dataServersListBox.Items.Add(dataserverId);
            }
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
                //isto pode causar problemas quando formos ler o valor da lista
                String registerValue = String.IsNullOrEmpty(register) ? "Empty" : register;
                clientStringRegisterListBox.Items.Add(registerValue);
            }
        }

        private void updateClientsList()
        {
            ClientsListBox.Items.Clear();
            foreach (string clientId in puppetMaster.clients.Keys)
            {
                ClientsListBox.Items.Add(clientId);
            }
        }


        #endregion updates

        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void CopyFileButtons_Click(object sender, EventArgs e)
        {
            try
            {
                verifyClientSelection();

                int sourceFileRegisterId = getCopySourceRegisterId();
                int targetFileRegisterId = getCopyTargetRegisterId();
                string semantics = "DEFAULT SEMANTICS";
                string salt = getCopySalt();

                System.Windows.Forms.MessageBox.Show("Copy: from " + sourceFileRegisterId + " to " + targetFileRegisterId + " with salt " + salt);

                puppetMaster.copy(getSelectedClient(), sourceFileRegisterId, semantics, targetFileRegisterId, salt);

                updateClientFileRegister(getSelectedClient());
                updateClientStringRegister(getSelectedClient());
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
            }
        }

        public void printCommand(String s)
        {
            shell.Text += "> " + s + "\r\n";
        }

        private void dumpDSs_Click(object sender, EventArgs e)
        {
            foreach (ServerObjectWrapper obj in puppetMaster.dataServers.Values)
            {
                try
                {
                    IRemote remoteObj = obj.getObject<IRemote>();
                    remoteObj.dump();
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
                }
            }
        }

        private void dumpCLIENT_Click(object sender, EventArgs e)
        {
            foreach (ServerObjectWrapper obj in puppetMaster.clients.Values)
            {
                try
                {
                    IRemote remoteObj = obj.getObject<IRemote>();
                    remoteObj.dump();
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show("Error loading script::" + exception.Message + " :\n" + exception.StackTrace);
                }
            }
        }

        private void shell_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

    }
}
