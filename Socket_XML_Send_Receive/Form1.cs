using System;
using System.Text;
using System.Windows.Forms;
using System.IO; // for File stream
using System.Net; // for IpEndPoint
using System.Net.Sockets; // for Sockets
using System.Threading; // for Threads
using System.Xml; // for XmlTextReader and XmlValidatingReader
using System.Xml.Schema; // for XmlSchemaCollection

namespace Socket_XML_Send_Receive
{
    public partial class Form1 : Form
    {
        // variabile, constante
        private Socket client1, server1, server2;
        string ipExt, dt;
        int portSendExt, portListenInt;
        private Thread workerThread1;
        private const int BufsizeFull = 8192; // dimensiunea completa a buffer-ului pentru socket
        private const int Bufsize = BufsizeFull - 4;
        private const int Backlog = 5; // dimensiunea cozii de asteptare pentru socket
        private static bool isValid = true;      // validare cu schema a unui XML
        // XmlSchemaCollection cache = new XmlSchemaCollection(); //cache XSD schema
        // cache.Add("urn:MyNamespace", "C:\\MyFolder\\Product.xsd"); // add namespace XSD schema


        // metode complementare
        private string GetCurrentDT()
        {
            DateTime time = DateTime.Now;
            string format = "dd.MM.yyyy HH:mm:ss"; //27.12.2011 18:34:55
            return (time.ToString(format));
        }
        private string FindLocalIP()
        {
            string strHostName;
            strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }
        private void Debug(string str)
        {
            Action appendToTextBox = () =>
            {
                dt = GetCurrentDT();
                debugTextBox.AppendText(dt + " - " + str + ".\n");
                debugTextBox.ScrollToCaret();
            };

            debugTextBox.Invoke(appendToTextBox);
        }
        public void MyValidationEventHandler(object sender, ValidationEventArgs args)
        {
            isValid = false;
            Debug("SERVER: Validation event\n" + args.Message);
        }
        private bool Validation(string file)
        {
            XmlTextReader r = new XmlTextReader(file);
#pragma warning disable 618
            XmlValidatingReader v = new XmlValidatingReader(r);
#pragma warning restore 618
            //v.Schemas.Add(cache);
            if (radioButton1.Checked)
            {
                v.ValidationType = ValidationType.Schema;
            }
            else if (radioButton2.Checked)
            {
                v.ValidationType = ValidationType.DTD;
            }
            else //(radioButton3.Checked)
            {
#pragma warning disable 618
                v.ValidationType = ValidationType.XDR;
#pragma warning restore 618
            }
            v.ValidationEventHandler += MyValidationEventHandler;
            while (v.Read())
            {
                // Can add code here to process the content
                // bool Success = true;
                // Console.WriteLine("Validation finished. Validation {0}", (Success == true ? "successful" : "failed"));
                // Path.GetExtension(label11.Text).Substring(1).ToUpper()
            }
            v.Close();
            if (isValid)
            {
                return true; //Document is valid
            }
            return false;//Document is invalid
            
        }
        private void Listen()
        {
            server1 = null;
            var rcvBufferFull = new byte[BufsizeFull];
            var rcvBufferPartial = new byte[Bufsize];
            portListenInt = Convert.ToInt32(textBox3.Text);
            using (server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    server1.Bind(new IPEndPoint(IPAddress.Parse(textBox4.Text), portListenInt));
                    server1.Listen(Backlog);
                    Debug("SERVER: socket <" + textBox4.Text + ":" + portListenInt + "> deschis");
                }
                catch (Exception ex)
                {
                    Debug("SERVER: probleme creare server socket <" + textBox4.Text + ":" + portListenInt + ">");
                    Debug(ex.ToString());
                }
                while (true)
                {
                    client1 = null;
                    int totalBytesReceived = 0;
                    try
                    {
                        using (client1 = server1.Accept())
                        {
                            Debug("SERVER: client socket <" + client1.RemoteEndPoint + "> conectat");
                            int bytesRcvd;
                            while ((bytesRcvd = client1.Receive(rcvBufferFull, 0, rcvBufferFull.Length, SocketFlags.None)) > 0)
                            {
                                if (totalBytesReceived >= rcvBufferFull.Length)
                                {
                                    break;
                                }
                                totalBytesReceived += bytesRcvd;
                            }
                            Array.Copy(rcvBufferFull, 4, rcvBufferPartial, 0, totalBytesReceived - 4);
                            if (addMessageLengthCheckBox.Checked)
                            {
                                switch (comboBox1.Text)
                                {
                                    case "ASCII":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.ASCII.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.ASCII.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                        }
                                        break;
                                    case "UTF7":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF7.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF7.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                        }
                                        break;
                                    case "UTF8":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF8.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF8.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                        }
                                        break;
                                    case "Unicode":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.Unicode.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.Unicode.GetString(rcvBufferPartial, 0, (totalBytesReceived - 4));
                                        }
                                        break;
                                        //
                                }
                                Debug("SERVER: receptionat " + (totalBytesReceived - 4) + " bytes");
                                if (checkBox3.Checked)
                                {
                                    /*
                                    client1.Send(rcvBuffer_partial, 0, rcvBuffer_partial.Length, SocketFlags.None);
                                    Debug("SERVER: expediat echo data catre client.");
                                    */
                                }
                            }
                            else
                            {
                                switch (comboBox1.Text)
                                {
                                    case "ASCII":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.ASCII.GetString(rcvBufferFull, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.ASCII.GetString(rcvBufferFull, 0, totalBytesReceived);
                                        }
                                        break;
                                    case "UTF7":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF7.GetString(rcvBufferFull, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF7.GetString(rcvBufferFull, 0, totalBytesReceived);
                                        }
                                        break;
                                    case "UTF8":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF8.GetString(rcvBufferFull, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF8.GetString(rcvBufferFull, 0, totalBytesReceived);
                                        }
                                        break;
                                    case "Unicode":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.Unicode.GetString(rcvBufferFull, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            }
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.Unicode.GetString(rcvBufferFull, 0, totalBytesReceived);
                                        }
                                        break;
                                        //
                                }
                                Debug("SERVER: receptionat " + totalBytesReceived + " bytes");
                                if (checkBox3.Checked)
                                {
                                    /*
                                    client1.Send(rcvBuffer_partial, 0, rcvBuffer_partial.Length, SocketFlags.None);
                                    Debug("SERVER: expediat echo data catre client.");
                                    */
                                }
                            }
                        }
                        if (client1 != null)
                        {
                            client1.Close();
                        }
                        Debug("SERVER: client socket deconectat");
                    }
                    catch (Exception ex)
                    {
                        Debug(ex.ToString());
                    }
                    finally
                    {
                        if (client1 != null)
                        {
                            client1.Close();
                        }
                    }
                }
            }
        }
        private void Send()
        {
            ipExt = textBox1.Text;
            portSendExt = Convert.ToInt32(textBox2.Text);
            server2 = null;
            using (server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    var serverEndPoint = new IPEndPoint(IPAddress.Parse(ipExt), portSendExt);
                    server2.Connect(serverEndPoint);
                    Debug("CLIENT: conectat la server socket <" + ipExt + ":" + portSendExt + ">");

                    var textBytes = ConvertStringToBytes(richTextBox1.Text, (Encoding) comboBox1.SelectedItem);
                    server2.Send(CreateByteArrayToSend(textBytes), SocketFlags.None);
                   
                    Debug("CLIENT: date expediate de la client la server socket.");
                }
                catch (Exception ex)
                {
                    Debug("CLIENT: probleme conectare/trimitere de la client la server socket <" + ipExt + ":" + portSendExt + ">");
                    Debug(ex.ToString());
                }
                finally
                {
                    if (server2 != null)
                    {
                        server2.Close();
                        ((IDisposable)server2).Dispose();
                        Debug("CLIENT: deconectat de la server socket");
                    }
                }
            }
        }

        private byte[] CreateByteArrayToSend(byte[] textBytes)
        {
            if (addMessageLengthCheckBox.Checked)
            {
                int reqLen = richTextBox1.Text.Length;
                int reqLenH2N = IPAddress.HostToNetworkOrder(reqLen * 2);
                byte[] reqLenArray = BitConverter.GetBytes(reqLenH2N);
                var buffPartial = new byte[reqLen * 2 + 4];
                reqLenArray.CopyTo(buffPartial, 0);
                textBytes.CopyTo(buffPartial, 4);
                return buffPartial;
            }
            return textBytes;
        }

        private byte[] ConvertStringToBytes(string text, Encoding encoding)
        {
            byte[] stringBytes = encoding.GetBytes(text);
            
            return stringBytes;
        }

        // metode de baza
        public Form1()
        {
            InitializeComponent();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                button5.Enabled = true;
                button2.Enabled = true;
                richTextBox1.ReadOnly = true;
                button7.Enabled = false;
                groupBox1.Enabled = true;
            }
            else
            {
                button5.Enabled = false;
                button2.Enabled = false;
                button7.Enabled = true;
                richTextBox1.ReadOnly = false;
                groupBox1.Enabled = false;
                richTextBox4.Clear();
                label3.Text = "";
                label11.Text = "";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            textBox1.Text = FindLocalIP();
            textBox4.Text = FindLocalIP();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            var fDialog = new OpenFileDialog
            {
                Title = "Select XSD/DTD/XDR File",
                Filter = "XSD Files|*.xsd|DTD Files|*.dtd|XDR Files|*.xdr",
                ShowHelp = false,
                CheckFileExists = true,
                CheckPathExists = true,
                AddExtension = true,
                InitialDirectory = @Application.StartupPath
            };
            //fDialog.Filter = "XSD Files|*.xsd|DTD Files|*.dtd|XDR Files|*.xdr|All Files|*.*";
            //fDialog.InitialDirectory =@Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToString();
            //fDialog.InitialDirectory = @"C:\";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                label3.Text = fDialog.FileName;
                try
                {
                    richTextBox4.Clear();
                    richTextBox4.Text = File.ReadAllText(fDialog.FileName);
                }
                catch (Exception ex)
                {
                    Debug("Probleme incarcare/deschidere fisier XSD/DTD");
                    Debug(ex.ToString());
                }
            }
        }
        private void listenButton_Click(object sender, EventArgs e)
        {
            if (listenButton.Text == "Listen ON")
            {
                listenButton.Text = "Listen OFF";
                //button1.Enabled = true;
                workerThread1 = new Thread(Listen);
                workerThread1.Start();

            }
            else
            {
                listenButton.Text = "Listen ON";
                //button1.Enabled = false;
                try
                {
                    if (server1 != null)
                    {
                        server1.Close();
                        ((IDisposable)server1).Dispose();
                        Debug("SERVER: socket <" + textBox4.Text + ":" + portListenInt + "> inchis");
                    }
                }
                catch (Exception ex)
                {
                    Debug(ex.ToString());
                }
                finally
                {
                    workerThread1.Abort();
                }
            }
        }
        private void sendButton_Click(object sender, EventArgs e)
        {
            Send();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            debugTextBox.Clear();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            var fDialog = new OpenFileDialog
            {
                Title = "Select XML File",
                Filter = "XML Files|*.xml",
                ShowHelp = false,
                CheckFileExists = true,
                CheckPathExists = true,
                AddExtension = true,
                InitialDirectory = @Application.StartupPath
            };
            //fDialog.Filter = "XML Files|*.xml|All Files|*.*";
            //fDialog.InitialDirectory =@Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToString();
            //fDialog.InitialDirectory = @"C:\";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                isValid = true;
                label11.Text = fDialog.FileName;
                try
                {
                    richTextBox1.Clear();
                    richTextBox1.Text = File.ReadAllText(fDialog.FileName);
                }
                catch (Exception ex)
                {
                    Debug("Probleme incarcare/deschidere fisier XML");
                    Debug(ex.ToString());
                }
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox5.Clear();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (listenButton.Text == "Listen OFF")
            {
                listenButton_Click(sender, e);
            }
        }
    }
}