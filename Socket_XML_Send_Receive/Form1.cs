using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        string ip_ext, dt, ClientIP_int;
        int port_send_ext, port_listen_int;
        private Thread workerThread1;
        private const int BUFSIZE_FULL = 8192; // dimensiunea completa a buffer-ului pentru socket
        private const int BUFSIZE = BUFSIZE_FULL - 4;
        private const int BACKLOG = 5; // dimensiunea cozii de asteptare pentru socket
        private const int TIMELIMIT = 30000; // timpul limita de ascultare pentru un client (3 sec.)
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
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }
        private void Debug (string str)
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
            XmlValidatingReader v = new XmlValidatingReader(r);
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
                v.ValidationType = ValidationType.XDR;
            };
            v.ValidationEventHandler += new ValidationEventHandler(MyValidationEventHandler);
            while (v.Read())
            {
                // Can add code here to process the content
                // bool Success = true;
                // Console.WriteLine("Validation finished. Validation {0}", (Success == true ? "successful" : "failed"));
                // Path.GetExtension(label11.Text).Substring(1).ToUpper()
            };
            v.Close();
            if (isValid)
            {
                return true; //Document is valid
            }
            else
            {                                                                     
                return false;//Document is invalid
            };
        }
        private void Listen()
        {
            server1 = null;
            byte[] rcvBuffer_full = new byte[BUFSIZE_FULL];
            byte[] rcvBuffer_partial = new byte[BUFSIZE];
            port_listen_int = System.Convert.ToInt32(textBox3.Text);
            using (server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    server1.Bind(new IPEndPoint(IPAddress.Parse(textBox4.Text), port_listen_int));
                    server1.Listen(BACKLOG);
                    Debug("SERVER: socket <" + textBox4.Text + ":" + port_listen_int.ToString() + "> deschis");
                }
                catch (Exception ex)
                {
                    Debug("SERVER: probleme creare server socket <" + textBox4.Text + ":" + port_listen_int.ToString() + ">");
                    //Debug(ex.ToString());
                };
                while (true)
                {
                    client1 = null;
                    ClientIP_int = null;
                    int bytesRcvd = 0, totalBytesReceived = 0;
                    try
                    {
                        using (client1 = server1.Accept())
                        {
                            Debug("SERVER: client socket <" + client1.RemoteEndPoint.ToString() + "> conectat");
                            ClientIP_int = (client1.RemoteEndPoint.ToString()).Split(':')[0];
                            while ((bytesRcvd = client1.Receive(rcvBuffer_full, 0, rcvBuffer_full.Length, SocketFlags.None)) > 0)
                            {
                                if (totalBytesReceived >= rcvBuffer_full.Length)
                                {
                                    break;
                                };
                                totalBytesReceived += bytesRcvd;
                            };
                            Array.Copy(rcvBuffer_full, 4, rcvBuffer_partial, 0, totalBytesReceived - 4);
                            if (checkBox1.Checked)
                            {
                                switch (comboBox1.Text)
                                {
                                    case "ASCII":
                                        if ((checkBox2.Checked) && (label11.Text!=""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.ASCII.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.ASCII.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                        };
                                        break;
                                    case "UTF7":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF7.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF7.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                        };
                                        break;
                                    case "UTF8":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF8.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF8.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                        };
                                        break;
                                    case "Unicode":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.Unicode.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.Unicode.GetString(rcvBuffer_partial, 0, (totalBytesReceived - 4));
                                        };
                                        break;
                                    default:
                                        //
                                        break;
                                };
                                Debug("SERVER: receptionat " + (totalBytesReceived - 4) + " bytes");
                                if (checkBox3.Checked)
                                {
                                    /*
                                    client1.Send(rcvBuffer_partial, 0, rcvBuffer_partial.Length, SocketFlags.None);
                                    Debug("SERVER: expediat echo data catre client.");
                                    */
                                };
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
                                                richTextBox2.Text = Encoding.ASCII.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.ASCII.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                        };
                                        break;
                                    case "UTF7":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF7.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF7.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                        };
                                        break;
                                    case "UTF8":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.UTF8.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.UTF8.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                        };
                                        break;
                                    case "Unicode":
                                        if ((checkBox2.Checked) && (label11.Text != ""))
                                        {
                                            if (Validation(label11.Text))
                                            {
                                                richTextBox2.Text = Encoding.Unicode.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                            }
                                            else
                                            {
                                                Debug("SERVER: eroare parsare XML via schema inclusa in antet");
                                            };
                                        }
                                        else
                                        {
                                            richTextBox2.Text = Encoding.Unicode.GetString(rcvBuffer_full, 0, totalBytesReceived);
                                        };
                                        break;
                                    default:
                                        //
                                        break;
                                };
                                Debug("SERVER: receptionat " + totalBytesReceived + " bytes");
                                if (checkBox3.Checked)
                                {
                                    /*
                                    client1.Send(rcvBuffer_partial, 0, rcvBuffer_partial.Length, SocketFlags.None);
                                    Debug("SERVER: expediat echo data catre client.");
                                    */
                                };
                            };
                        };
                        if (client1 != null)
                        {
                            client1.Close();
                        };
                        Debug("SERVER: client socket deconectat");
                    }
                    catch (Exception ex)
                    {
                        //Debug(ex.ToString());
                    }
                    finally
                    {
                        if (client1 != null)
                        {
                            client1.Close();
                        };
                    };
                };
            };
        }
        private void Send()
        {
            ip_ext = textBox1.Text;
            port_send_ext = System.Convert.ToInt32(textBox2.Text);
            server2 = null;
            using (server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip_ext), port_send_ext);
                    server2.Connect(serverEndPoint);
                    Debug("CLIENT: conectat la server socket <" + ip_ext + ":" + port_send_ext.ToString() + ">");

                    byte[] buff_full = null;
                    buff_full = ConvertStringToBytes(richTextBox1.Text);

                    if (checkBox1.Checked)
                    {
                        int reqLen = richTextBox1.Text.Length;
                        int reqLenH2N = IPAddress.HostToNetworkOrder(reqLen * 2);
                        byte[] reqLenArray = BitConverter.GetBytes(reqLenH2N);
                        byte[] buff_partial = new byte[reqLen * 2 + 4];
                        reqLenArray.CopyTo(buff_partial, 0);
                        buff_full.CopyTo(buff_partial, 4);
                        server2.Send(buff_partial, 0, buff_partial.Length, SocketFlags.None);           
                    }
                    else
                    {
                        server2.Send(buff_full, 0, buff_full.Length, SocketFlags.None);                             
                    }
                    Debug("CLIENT: date expediate de la client la server socket.");
                }
                catch (Exception ex)
                {
                    Debug("CLIENT: probleme conectare/trimitere de la client la server socket <" + ip_ext + ":" + port_send_ext.ToString() + ">");
                    //Debug(ex.ToString());
                }
                finally
                {
                    if (server2 != null)
                    {
                        server2.Close();
                        ((IDisposable)server2).Dispose();
                        Debug("CLIENT: deconectat de la server socket");
                    };
                };
            }
        }

        private byte[] ConvertStringToBytes(string text)
        {
            byte[] stringBytes = null;
            switch (comboBox1.Text)
            {
                case "ASCII":
                    stringBytes = Encoding.ASCII.GetBytes(text);
                    break;
                case "UTF7":
                    stringBytes = Encoding.UTF7.GetBytes(text);
                    break;
                case "UTF8":
                    stringBytes = Encoding.UTF8.GetBytes(text);
                    break;
                case "Unicode":
                    stringBytes = Encoding.Unicode.GetBytes(text);
                    break;
                default:
                    //
                    break;
            }

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
            };
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            textBox1.Text = FindLocalIP();
            textBox4.Text = FindLocalIP();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int size = -1;
            OpenFileDialog fDialog = new OpenFileDialog();
            fDialog.Title = "Select XSD/DTD/XDR File";
            fDialog.Filter = "XSD Files|*.xsd|DTD Files|*.dtd|XDR Files|*.xdr";
            //fDialog.Filter = "XSD Files|*.xsd|DTD Files|*.dtd|XDR Files|*.xdr|All Files|*.*";
            fDialog.ShowHelp = false;
            fDialog.CheckFileExists = true;
            fDialog.CheckPathExists = true;
            fDialog.AddExtension = true;
            fDialog.InitialDirectory = @Application.StartupPath;
            //fDialog.InitialDirectory =@Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToString();
            //fDialog.InitialDirectory = @"C:\";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                label3.Text = fDialog.FileName.ToString();
                try
                {
                    richTextBox4.Clear();
                    richTextBox4.Text = File.ReadAllText(fDialog.FileName.ToString());
                    size = richTextBox4.Text.Length;
                }
                catch (Exception ex)
                {
                    Debug("Probleme incarcare/deschidere fisier XSD/DTD");
                    //Debug(ex.ToString());
                };
            };
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
                        Debug("SERVER: socket <" + textBox4.Text + ":" + port_listen_int.ToString() + "> inchis");
                    };
                }
                catch (Exception ex)
                {
                    Debug(ex.ToString());
                }
                finally
                {
                    workerThread1.Abort();
                };
            };
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
            int size = -1;
            OpenFileDialog fDialog = new OpenFileDialog();
            fDialog.Title = "Select XML File";
            fDialog.Filter = "XML Files|*.xml";
            //fDialog.Filter = "XML Files|*.xml|All Files|*.*";
            fDialog.ShowHelp = false;
            fDialog.CheckFileExists = true;
            fDialog.CheckPathExists = true;
            fDialog.AddExtension = true;
            fDialog.InitialDirectory = @Application.StartupPath;
            //fDialog.InitialDirectory =@Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToString();
            //fDialog.InitialDirectory = @"C:\";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                isValid = true;
                label11.Text = fDialog.FileName.ToString();
                try
                {
                    richTextBox1.Clear();
                    richTextBox1.Text = File.ReadAllText(fDialog.FileName.ToString());
                    size = richTextBox1.Text.Length;
                }
                catch (Exception ex)
                {
                    Debug("Probleme incarcare/deschidere fisier XML");
                    //Debug(ex.ToString());
                };
            };
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
            };
        }
    }
}