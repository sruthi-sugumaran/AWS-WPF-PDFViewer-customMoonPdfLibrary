using _300983145_Sruthi__Lab2;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using EmailValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _300983145_sruthi__Lab2
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Login log = new Login();
            log.Show();
            this.Hide();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (txtemail.Text.Trim() != "" && txtname.Text.Trim() != "" && txtpassword.Password.Trim() != "")
            {

                string emailId = txtemail.Text.Trim().ToLower();
                if (emailId == null || !EmailValidator.Validate(emailId))
                {
                    txtemail.Background = Colors.errorBackGround;
                    txtemail.Foreground = Colors.errorForeGround;
                    return;
                }
                string name = txtname.Text.Trim();
                string password = txtpassword.Password.Trim();
                try
                {
                    AWSConnectionService db = AWSConnectionService.getInstance();
                    db.createRegistrationTableIfNotExists();
                    db.insertIntoRegistrationTableIfNotExist(emailId, password, name);
                    new Login().Show();
                    this.Hide();
                    Console.WriteLine("To continue, press Enter");
                    Console.ReadLine();
                }
                catch (AmazonDynamoDBException ex) { Console.WriteLine(ex.Message); }
                catch (AmazonServiceException ex) { Console.WriteLine(ex.Message); }
            }
            else
            {
                MessageBox.Show("Please fill all details");
            }
        }

        private void modifyEmailField(object sender, TextChangedEventArgs e)
        {
            txtemail.Background = Colors.clearBackGround;
            txtemail.Foreground = Colors.clearForeGround;
        }
    }
}