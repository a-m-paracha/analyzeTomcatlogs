using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace analyzeTomcatlogs
{
    public partial class Form1 : Form
    {
        string OutPutDirectory = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox2.Text = "66.97.145.64 - - [12/Jun/2022:21:25:43 -0500] \"GET /favicon.ico HTTP/1.1\" 401 1037";
            this.textBox3.Text = "IP,Unknown,User,Date,Time,Method,URL,EsriServiceType,EsriOperation,QueryString,HTTP,Staus,Bytes";
            this.textBox4.Text = "localhost_access_log";
            this.textBox5.Text = ".txt";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string SampleTomcatLine = this.textBox2.Text;
            string SampleHeaer = this.textBox3.Text;
            SampleTomcatLine = ParsedTomcatLine(SampleTomcatLine);
            string[] Headers = SampleHeaer.Split(',');
            string[] TomcatLine = SampleTomcatLine.Split('€');
            string ParsedSampleLine = "";

            for (int i = 0; i < TomcatLine.Length; i++)
            {
                ParsedSampleLine = ParsedSampleLine + Headers[i] + ":" + TomcatLine[i] + "\n";
            }
            MessageBox.Show("Corresponding Header and Values are: \n" + ParsedSampleLine);

            //if (Headers.Length == TomcatLine.Length)
            //{

            //}

            //else
            //{
            //    MessageBox.Show("There is an issue with Parse Lenth. The parsed line is: " + SampleTomcatLine);
            //}
        }

        private static string ParsedTomcatLine(string RawLine)
        {
            //string SampleTomcatLine = RawLine.Replace(" - - ", " ").Replace("\"", "");
            string SampleTomcatLine = RawLine.Replace("\"", "");
            //SampleTomcatLine = Regex.Replace(SampleTomcatLine, @"\s+", " ");
            string ExtractedTimeStamp = SampleTomcatLine.Split('[')[1].Split(']')[0];
            string TimeStamp = ExtractedTimeStamp.Split(' ')[0];
            var dateTime = DateTime.ParseExact(TimeStamp, "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture);
            var convertedDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
           
            //ExtractedTimeStamp = ExtractedTimeStamp.Replace(' ', ',');
            SampleTomcatLine = SampleTomcatLine.Replace("[" + ExtractedTimeStamp + "]", convertedDateTime.ToString());
            SampleTomcatLine = SampleTomcatLine.Replace(' ', '€');
            string ExtractedURL = SampleTomcatLine.Split('€')[6];

            string querryString = "";

            string EsriOperation = "";

            string EsriServiceType = "";

            string BaseURL = "";

            if (ExtractedURL.Contains("?"))
            {
                querryString = ExtractedURL.Split('?')[1];
                querryString = HttpUtility.UrlDecode(querryString);
                BaseURL = ExtractedURL.Split('?')[0];
            }
            else
            {
                BaseURL = ExtractedURL;
            }

            if (ExtractedURL.Contains("Server") && ExtractedURL.Contains("rest/services"))
            {
                if (!ExtractedURL.EndsWith("Server"))
                {

                    EsriOperation = ExtractedURL.Split('?')[0];
                    string[] urlpart = EsriOperation.Split('/');
                    EsriOperation = urlpart[urlpart.Length - 1];
                    if (IsNumeric(EsriOperation))
                    {
                        EsriOperation = urlpart[urlpart.Length - 2];
                        if (EsriOperation.EndsWith("Server"))
                        {
                            EsriOperation = "Read Service Metadata";
                        }
                    }
                }
                string[] urlpartsecond = BaseURL.Split(new string[] { "Server/" }, StringSplitOptions.None);
                BaseURL = urlpartsecond[0];// + "Server";

                if (!BaseURL.EndsWith("Server"))
                {
                    BaseURL = BaseURL + "Server";
                }

                EsriServiceType = BaseURL.Split('/').Last();
            }

            string NewURLPhrase = BaseURL + "€" + EsriServiceType + "€" + EsriOperation + "€" + querryString;

            

            if(NewURLPhrase == "/€€€")
            {
                string bufferString = "";
                string[] ParsedString = SampleTomcatLine.Split('€');

                for (int ind = 0; ind< ParsedString.Length; ind++)
                {
                    if (ind != 6)
                    {
                        bufferString = bufferString + ParsedString[ind] + "€";
                    }
                    else
                    {
                        bufferString = bufferString + NewURLPhrase + "€";
                    }
                    //Console.WriteLine(bufferString);
                }
                SampleTomcatLine = bufferString.Substring(0, bufferString.Length - 1);
            }
            else
            {
                SampleTomcatLine = SampleTomcatLine.Replace(ExtractedURL, NewURLPhrase);
            }
            
            return SampleTomcatLine;
        }


        public static bool IsNumeric(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(this.textBox1.Text + "/output"))
            {
                System.IO.Directory.CreateDirectory(this.textBox1.Text + "/output");
            }

            //OutPutDirectory = Directory.GetCurrentDirectory().ToString() + "/output/" + GetTimestamp(DateTime.Now);
            OutPutDirectory = this.textBox1.Text + "/output/" + GetTimestamp(DateTime.Now);
            
            System.IO.Directory.CreateDirectory(OutPutDirectory);


            string[] logfiles = Directory.GetFiles(this.textBox1.Text);
            
            foreach (string logfilepath in logfiles)
            {
                string FileName = Path.GetFileName(logfilepath);
                if(FileName.StartsWith(this.textBox4.Text)&& FileName.EndsWith(this.textBox5.Text))
                {
                    string[] rawlines = System.IO.File.ReadAllLines(logfilepath);
                    string NewFileName = FileName.Replace(".txt", ".csv");
                    string NewFilePath = OutPutDirectory + "/" + NewFileName;
                    string SampleHeader = this.textBox3.Text;
                    File.WriteAllText(NewFilePath, SampleHeader + Environment.NewLine);
                    foreach (string rawline in rawlines)
                    {
                        string SampleTomcatLine = ParsedTomcatLine(rawline);
                        SampleTomcatLine = SampleTomcatLine.Replace('€', ',');
                        File.AppendAllText(NewFilePath, SampleTomcatLine + Environment.NewLine);
                    }
                }
            }

            MessageBox.Show("CSV Files are being generated in folder: " + OutPutDirectory);

        }

        private static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (Directory.Exists(fbd.SelectedPath))
                    {
                        textBox1.Text = fbd.SelectedPath;
                    }
                    else
                    {
                        MessageBox.Show("Directory Path is not correct");
                    }
                }
            }
        }
    }
}
