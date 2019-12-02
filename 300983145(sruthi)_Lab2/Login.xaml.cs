
using _300983145_Sruthi__Lab2;
using Amazon.DynamoDBv2.Model;
using EmailValidation;
using System;
using System.Diagnostics;
using System.Windows;

namespace _300983145_sruthi__Lab2
{
        /// <summary>
        /// Interaction logic for Login.xaml
        /// </summary>
        public partial class Login : Window
        {

            public Login()
            {
                InitializeComponent();
                Trace.WriteLine("Calling amazon service...");
                //new ViewBook().load("", "C:\\Users\\Owner\\Documents\\300983145(sruthi)_Lab2\\300983145(sruthi)_Lab2\\Download\\20191015004235_demodemocom_itinerary_ chennai.pdf");
                //AWSConnectionService.getInstance().exampleManager();
                //new LowLevelTableExample().exampleManager();
            }

            private void button2_Click(object sender, RoutedEventArgs e)
            {
                Register reg = new Register();
                reg.Show();
                this.Hide();
            }
            public static Welcome instance;
            private void button1_Click(object sender, RoutedEventArgs e)
            {
                string emailId = txtemail.Text.ToLower();
                if (emailId == null || !EmailValidator.Validate(emailId))
                {
                    txtemail.Background = _300983145_Sruthi__Lab2.Colors.errorBackGround;
                    txtemail.Foreground = Colors.errorForeGround;
                    return;
                }
                string password = txtpassword.Password;
                AWSConnectionService db = AWSConnectionService.getInstance();
                try
                {
                    bool loginsuccess = db.loginIntoApplication(emailId, password);

                    if (loginsuccess)
                    {
                        db.createBucket();
                        db.createFileTableIfNotExists(AWSConnectionService.fileTableName);
                        instance = new Welcome(emailId);
                        instance.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show(this, "Login Failed", "Incorrect Email or Password");
                    }
                }
                catch (ResourceNotFoundException)
                { MessageBox.Show("Account doesn't exist"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }

            private void modifyEmailField(object sender, System.Windows.Controls.TextChangedEventArgs e)
            {
                txtemail.Background = Colors.clearBackGround;
                txtemail.Foreground = Colors.clearForeGround;
            }
        }
    }

