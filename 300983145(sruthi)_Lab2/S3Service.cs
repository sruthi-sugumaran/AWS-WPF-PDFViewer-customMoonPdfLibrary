using _300983145_sruthi__Lab2;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _300983145_Sruthi__Lab2
{
    class S3Service
    {
        private readonly string bucketName;
        private readonly string keyName;
        private readonly string filePath;
        private readonly RegionEndpoint bucketRegion;
        private readonly IAmazonS3 s3Client;


        public S3Service(RegionEndpoint bucketRegion, string bucketName, string keyName, string filePath)
        {
            this.bucketRegion = bucketRegion;
            this.bucketName = bucketName;
            this.keyName = keyName;
            this.filePath = filePath;
            var credentials = new BasicAWSCredentials(ConfigurationManager.AppSettings["AWSAccessKey"],
                ConfigurationManager.AppSettings["AWSSecretKey"]);
            s3Client = new AmazonS3Client(credentials, this.bucketRegion);
        }

        public async Task UploadFileAsync(string emailId)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    FilePath = filePath,
                    BucketName = bucketName,
                    Key = keyName,
                    CannedACL = S3CannedACL.PublicRead
                };
                var fileTransferUtility = new TransferUtility(s3Client);
                await Task.Run(() =>
                {
                    Task upld = fileTransferUtility.UploadAsync(uploadRequest);
                    while (!upld.IsCompleted) { upld.Wait(25); if (upld.IsFaulted || upld.IsCanceled) return; }
                    Trace.WriteLine(String.Format("Upload status: {0} \n Upload Complete: {1}", upld.Status, upld.IsCompleted));
                    AWSConnectionService.getInstance().ListPDFFilesforUser(emailId);
                    Console.WriteLine("File Upload completed.. Check bucket via console");
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        new Welcome(emailId).Show();
                    });
                });



            }
            catch (AmazonS3Exception e)
            {
                Trace.WriteLine(String.Format("Error encountered on server. Message: '{0}' when writing an object", e.Message));
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format("Unknown encountered on server. Message: '{0}' when writing an object", e.Message));
            }
        }
        
        public string Download_FileAsync(string emailId)
    {

            //DirectoryExample.getAccess(mydocumentsPath, WindowsIdentity.GetCurrent().Name);
            //getAccessToCurrentUsersMyDocuments(mydocumentsPath);
            try
                {
                   var currentDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
                var directoryInfo = Directory.CreateDirectory(currentDirectory + "\\Download");
                string path = currentDirectory + "\\Download";
                GrantAccess(path);
                path= path+ "\\" + keyName;
                //checks if file is already available in local and retuns the path if not proceeds
                //if (File.Exists(path)) return path;
                    var downloadRequest = new TransferUtilityDownloadRequest
                {
                    FilePath  = path,
                    BucketName = bucketName,
                    Key = keyName
                };
                var fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.Download(downloadRequest);
                using (FileStream fileDownloaded = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return path;
                }
            }
        catch (AmazonS3Exception e)
        {
            Trace.WriteLine(String.Format("Error encountered on server. Message: '{0}' when writing an object", e.Message));
                return "";
        }
        catch (Exception e)
        {
            Trace.WriteLine(String.Format("Unknown encountered on server. Message: '{0}' when writing an object", e.Message));
                return "";
        }
    }

        public static void getAccessToCurrentUsersMyDocuments(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var directorySecurity = directoryInfo.GetAccessControl();
            var currentUserIdentity = WindowsIdentity.GetCurrent();
            var fileSystemRule = new FileSystemAccessRule(currentUserIdentity.Name,
                                                          FileSystemRights.FullControl,
                                                          InheritanceFlags.ObjectInherit |
                                                          InheritanceFlags.ContainerInherit,
                                                          PropagationFlags.None,
                                                          AccessControlType.Allow);
            directorySecurity.AddAccessRule(fileSystemRule);
            directoryInfo.SetAccessControl(directorySecurity);
        }

        public bool GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }

        public void downloadPDF(string key, string gKey)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = bucketName;
                request.Key = gKey;
                GetObjectResponse response = s3Client.GetObject(request);
                var currentDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
                var directoryInfo = Directory.CreateDirectory(currentDirectory + "\\Download");
                string path = currentDirectory + "\\Download";
                GrantAccess(path);
                path= path+ "\\" + gKey; 
                response.WriteResponseStreamToFile(path);
            }catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}