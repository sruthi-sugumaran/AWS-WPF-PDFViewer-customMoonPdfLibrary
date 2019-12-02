using _300983145_Sruthi__Lab2;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Win32;
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
{  /// <summary>
   /// Interaction logic for Welcome.xaml
   /// </summary>
    public partial class Welcome : Window
    {
        private string useremailId;
        public Welcome(String user)
        {
            useremailId = user;
            InitializeComponent();
        }

        
        private void LoadList(string email)
        {
            AWSConnectionService db = AWSConnectionService.getInstance();
            var files = db.ListPDFFilesforUser(email);
            Console.WriteLine("\nPrinting result.....");
            var gridView = new GridView();
            this.listViewFiles.View = gridView;
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Book",
                DisplayMemberBinding = new Binding("KeyName")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "GeneratedKeyName",
                DisplayMemberBinding = new Binding("GeneratedKeyName")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "CurrentPageNumber",
                DisplayMemberBinding = new Binding("CurrentPageNumber")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "EmailId",
                DisplayMemberBinding = new Binding("EmailId")
            });

        
            gridView.Columns[0].Width = 400;
            gridView.Columns[1].Width = 0;
            gridView.Columns[2].Width = 0;
            gridView.Columns[3].Width = 0;

            var dict = new Dictionary<FileModel, DateTime>();
            foreach (var file in files)
            {
                listViewFiles.Items.Add(file);
                dict.Add(file, file.LastAccessedTime);
            }
            //using linq to find the latest accessed book
            try
            {
                dict.Values.Max();
                var keyOfMaxValue = dict.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                lblLatestBook.Text = keyOfMaxValue.KeyName;
            }
            catch (InvalidOperationException)
            {
                lblLatestBook.Text = "No books, recent books will appear here..";
            }
        }

       

        private void PDFBooksListLoaded(object sender, RoutedEventArgs e)
        {
            LoadList(useremailId);
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string selection = item.Header.ToString().ToLower();
            if (selection == "Home".ToLower())
            {
                new Welcome(useremailId).Show();
                this.Hide();
            }
            else
            {
                new Login().Show();
                this.Hide();
            }
        }

        private string filePath;

        private void BtnAddOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "*.PDF|*.pdf";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                this.filePath = openFileDialog.FileName.Trim();
                lblFileName.Content = openFileDialog.FileName;
            }
        }

        private void BtnUploadConfirmOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filePath != null || filePath.Trim() != "")
                    AWSConnectionService.getInstance().insertIntoFileTableAndUpload(useremailId, AWSConnectionService.s3StorageBucketRegion, AWSConnectionService.s3StorageBucketName, filePath);
                System.Threading.Thread.Sleep(2000);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("No file selected");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void BtnUploadFileCancelClick(object sender, RoutedEventArgs e)
        {
            filePath = "";
            lblFileName.Content = "";
        }

        void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileModel item = (FileModel)((FrameworkElement)e.OriginalSource).DataContext;
            if (item != null)
            {
                //MessageBox.Show("Item's Double Click handled!" + item.GKey);
                //new S3Service(AWSConnectionService.s3StorageBucketRegion, AWSConnectionService.s3StorageBucketName, item.GKey, null).downloadPDF(item.KeyName, item.GKey);
                string downloadPath =  new S3Service(AWSConnectionService.s3StorageBucketRegion, AWSConnectionService.s3StorageBucketName, item.GeneratedKeyName, null).Download_FileAsync(useremailId);
                ViewBook viewBook = new ViewBook(downloadPath, item);
                AWSConnectionService instance = AWSConnectionService.getInstance();
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(instance.credentials, AWSConnectionService.dynamoDbRegion);
                DynamoDBContext context = new DynamoDBContext(client);
                FileModel pdfRecordRetrived = context.Load<FileModel>(item.EmailId, item.KeyName);
                Trace.WriteLine(pdfRecordRetrived.ToString());
                pdfRecordRetrived.LastAccessedTime = DateTime.Now;
                context.Save<FileModel>(pdfRecordRetrived);
                viewBook.Show();
                this.Hide();
            }
        }
    }
}