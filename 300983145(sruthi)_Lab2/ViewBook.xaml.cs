using _300983145_Sruthi__Lab2;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MoonPdfLib.Virtualizing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _300983145_sruthi__Lab2
{
    /// <summary>
    /// Interaction logic for ViewBook.xaml
    /// </summary>
    public partial class ViewBook : Window
    {
        private string emailId;
        private string filePath;
        private bool _isLoaded = false;
        private string key;
        private int currentpagenumber;

        public ViewBook(string filePath, FileModel file)
        {
            
            this.filePath = filePath;
            emailId = file.EmailId;
            key = file.KeyName;
            currentpagenumber = file.CurrentPageNumber;
            InitializeComponent();
            
        }

        private void load()
        {
            
            try
            {
                //MessageBox.Show(filePath);
                moonPdfPanel.OpenFile(filePath);
                _isLoaded = true;
                moonPdfPanel.GotoPage(currentpagenumber);
                //subscribing to last loaded page on current pdf
                SingletonPublisherClass singletonPublisherClass = SingletonPublisherClass.Instance;
                Subscriber subscriber = new Subscriber(this);
                subscriber.Listener(singletonPublisherClass);
            }
            catch (Exception)
            {
                _isLoaded = false;
            }
        }
        
        private void backToHome(object sender, RoutedEventArgs e)
        {
            new Welcome(emailId).Show();
            this.Hide();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                moonPdfPanel.ZoomIn();
            }
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                moonPdfPanel.ZoomOut();
            }
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                moonPdfPanel.Zoom(1.0);
            }
        }


        private void FitToHeightButton_Click(object sender, RoutedEventArgs e)
        {
            moonPdfPanel.ZoomToHeight();
        }

        private void FacingButton_Click(object sender, RoutedEventArgs e)
        {
            moonPdfPanel.ViewType = MoonPdfLib.ViewType.Facing;
        }

        private void SinglePageButton_Click(object sender, RoutedEventArgs e)
        {
            moonPdfPanel.ViewType = MoonPdfLib.ViewType.SinglePage;
        }
        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            bookmark(moonPdfPanel.GetCurrentPageNumber());
        }

        public void bookmark(int pagenumber)
        {
            AWSConnectionService instance = AWSConnectionService.getInstance();
            AmazonDynamoDBClient client = new AmazonDynamoDBClient(instance.credentials, AWSConnectionService.dynamoDbRegion);
            DynamoDBContext context = new DynamoDBContext(client);
            FileModel pdfRecordRetrived = context.Load<FileModel>(emailId, key);
            Trace.WriteLine(pdfRecordRetrived.ToString());
            pdfRecordRetrived.CurrentPageNumber = pagenumber;
            context.Save<FileModel>(pdfRecordRetrived);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string selection = item.Header.ToString().ToLower();
            if (selection == "Home".ToLower())
            {
                new Welcome(emailId).Show();
                this.Hide();
            }
            else
            {
                new Login().Show();
                this.Hide();
            }
        }

        private void afterPanelLoaded(object sender, RoutedEventArgs e)
        {
            load();
        }
    }
}
