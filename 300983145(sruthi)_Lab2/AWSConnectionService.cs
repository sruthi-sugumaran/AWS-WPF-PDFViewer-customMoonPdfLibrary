using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _300983145_Sruthi__Lab2
{
    public class AWSConnectionService
    {
        public static AmazonDynamoDBClient client = null;

        public readonly static string tablename = "ExampleTable";
        public readonly static string registrationTableName = "RegistrationTable";
        public readonly static string fileTableName = "FileTable";
        public readonly static RegionEndpoint dynamoDbRegion = RegionEndpoint.CACentral1;

        public const string s3StorageBucketName = "assignment2-pdf-bucket-sruthi";
        public readonly static RegionEndpoint s3StorageBucketRegion = RegionEndpoint.CACentral1;
        public DynamoDBContext context { get; }
        public BasicAWSCredentials credentials { get; }


        private static AWSConnectionService singletonInstance;
        

        private AWSConnectionService()
        {
            credentials = new BasicAWSCredentials(ConfigurationManager.AppSettings["AWSAccessKey"],
                ConfigurationManager.AppSettings["AWSSecretKey"]);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.CACentral1);
                context = new DynamoDBContext(client);
        }



        public static AWSConnectionService getInstance()
        {
            if (singletonInstance != null)
                return singletonInstance;
            else{
                singletonInstance = new AWSConnectionService();
                return singletonInstance;
            }
        }

        public void exampleManager()
        {

            try
            {
                createRegistrationTableIfNotExists();
                string emailId = "sruthis0411@gmail.com";
                string password = "qwerty";
                string name = "Sruthi Sugumaran";
                insertIntoRegistrationTableIfNotExist(emailId, password, name);
                createFileTableIfNotExists(fileTableName);
                insertIntoFileTableAndUpload(emailId, s3StorageBucketRegion,
                   s3StorageBucketName, "C:\\Users\\Owner\\Desktop\\Itinerary_ Chennai.pdf");
                Console.WriteLine("To continue, press Enter");
                Console.ReadLine();

            }
            catch (AmazonDynamoDBException e) { Console.WriteLine(e.Message); }
            catch (AmazonServiceException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        public void createBucket()
        {
            PutBucketRequest request = new PutBucketRequest();
            request.BucketName = s3StorageBucketName;
            AmazonS3Client client = new AmazonS3Client(credentials,s3StorageBucketRegion);
        }
        public void WaitUntilTableReady(string tableName)
        {
            string status = null;
            // Let us wait until table is created. Call DescribeTable.
            do
            {
                System.Threading.Thread.Sleep(5000); // Wait 5 seconds.
                try
                {
                    var res = client.DescribeTable(new DescribeTableRequest
                    {
                        TableName = tableName
                    });

                    Trace.WriteLine(String.Format("Table name: {0}, status: {1}",
                              res.Table.TableName,
                              res.Table.TableStatus));
                    status = res.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // DescribeTable is eventually consistent. So you might
                    // get resource not found. So we handle the potential exception.
                }
            } while (status != "ACTIVE");
        }

        public void ListMyTables()
        {
            Console.WriteLine("\n*** listing tables ***");
            string lastTableNameEvaluated = null;
            do
            {
                var request = new ListTablesRequest
                {
                    Limit = 2,
                    ExclusiveStartTableName = lastTableNameEvaluated
                };
                var response = client.ListTables(request);
                foreach (string name in response.TableNames)
                    Console.WriteLine(name);
                lastTableNameEvaluated = response.LastEvaluatedTableName;
            } while (lastTableNameEvaluated != null);
        }

        public TableDescription GetTableInformation(string tableName)
        {
            Console.WriteLine("\n*** Retrieving table information ***");
            var request = new DescribeTableRequest
            {
                TableName = tableName
            };
            try
            {
                var response = client.DescribeTable(request);
                TableDescription description = response.Table;
                /*Console.WriteLine("Name: {0}", description.TableName);
                Console.WriteLine("# of items: {0}", description.ItemCount);
                Console.WriteLine("Provision Throughput (reads/sec): {0}",
                          description.ProvisionedThroughput.ReadCapacityUnits);
                Console.WriteLine("Provision Throughput (writes/sec): {0}",
                          description.ProvisionedThroughput.WriteCapacityUnits);*/
                return description;
            }
            catch (ResourceNotFoundException ex)
            {
                throw ex;
            }
        }

        private void UpdateExampleTable(string tableName)
        {
            Console.WriteLine("\n*** Updating table ***");
            var request = new UpdateTableRequest()
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput()
                {
                    ReadCapacityUnits = 6,
                    WriteCapacityUnits = 7
                }
            };

            var response = client.UpdateTable(request);
            WaitUntilTableReady(tableName);
        }

        private void DeleteExampleTable(string tableName)
        {
            Trace.WriteLine("\n*** Deleting table ***");
            var request = new DeleteTableRequest
            {
                TableName = tableName
            };

            var response = client.DeleteTable(request);

            Trace.WriteLine("Table is being deleted...");
        }

        public void createRegistrationTableIfNotExists()
        {
            try
            {
                GetTableInformation(registrationTableName);
                Trace.WriteLine(String.Format("{0} Exists", registrationTableName));
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("\n*** Creating table at {0} ***", RegionEndpoint.CACentral1);
                var request = new CreateTableRequest
                {
                    AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "EmailId",
                    AttributeType = "S"
                }
            },
                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "EmailId",
                    KeyType = "HASH" //Partition key
                }
            },
                    BillingMode = "PAY_PER_REQUEST",
                    TableName = registrationTableName
                };

                try
                {
                    var response = client.CreateTable(request);
                    var tableDescription = response.TableDescription;
                    Trace.WriteLine(String.Format("{1}: {0} \t BillingModeSummary: {2}",
                              tableDescription.TableStatus,
                              tableDescription.TableName,
                              tableDescription.BillingModeSummary));
                    string status = tableDescription.TableStatus;
                    Console.WriteLine(tablename + " - " + status);
                    WaitUntilTableReady(registrationTableName);
                }
                catch (ResourceInUseException ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

        public void insertIntoRegistrationTableIfNotExist(string emailId, string password, string name)
        {
            try
            {
                RegistrationTable registrationRecord = new RegistrationTable(emailId.ToLower(), password, name);
                Trace.WriteLine("Saving record....");
                context.Save(registrationRecord);
            }
            catch (AmazonDynamoDBException e) { Console.WriteLine(e.Message); }
        }

        public bool loginIntoApplication(string emailId, string password)
        {
            Trace.WriteLine("Retriving record....");
            RegistrationTable registrationRecordRetrived = context.Load<RegistrationTable>(emailId);
            if (registrationRecordRetrived != null &&
                registrationRecordRetrived.EmailId.ToLower().Equals(emailId) &&
                registrationRecordRetrived.Password.Equals(password))
                return true;
            else return false;
        }

        public void createFileTableIfNotExists(string fileTableName)
        {
            createBucket();
            try
            {
                GetTableInformation(fileTableName);
                Trace.WriteLine(String.Format("{0} Exists", fileTableName));
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("\n*** Creating table at {0} ***", dynamoDbRegion);
                var request = new CreateTableRequest
                {
                    AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "EmailId",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "KeyName",
                    AttributeType = "S"
                }
            },
                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "EmailId",
                    KeyType = "HASH" //Partition key
                },
                new KeySchemaElement
                {
                    AttributeName = "KeyName",
                    KeyType = "RANGE" //Sort key
                }
            },
                    BillingMode = "PAY_PER_REQUEST",
                    TableName = fileTableName
                };

                try
                {
                    var response = client.CreateTable(request);
                    var tableDescription = response.TableDescription;
                    Trace.WriteLine(String.Format("{1}: {0} \t BillingModeSummary: {2}",
                              tableDescription.TableStatus,
                              tableDescription.TableName,
                              tableDescription.BillingModeSummary));
                    string status = tableDescription.TableStatus;
                    Console.WriteLine(tableDescription.TableName + " - " + status);
                    WaitUntilTableReady(tableDescription.TableName);
                }
                catch (ResourceInUseException ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

        public async void insertIntoFileTableAndUpload(String emailId, RegionEndpoint s3StorageBucketRegion, string s3StorageBucketName, string filePath)
        {
            try
            {
                string filename = Path.GetFileName(filePath);
                string fileExtention = Path.GetExtension(filePath);
                if (!fileExtention.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase)) throw new FileFormatException("Only PDF files allowed");
                FileModel file = new FileModel(emailId, filename);
                Trace.WriteLine("Saving file record....");

                S3Service uploadService = new S3Service(s3StorageBucketRegion,
                    s3StorageBucketName, file.GeneratedKeyName, filePath);
                bool uploadComplete = false;
                await Task.Run(() => {
                    Task upload = uploadService.UploadFileAsync(emailId);
                    upload.Wait(25);
                    uploadComplete = upload.IsCompleted;
                });
                context.Save(file);
            }
            catch (AmazonDynamoDBException e) { Console.WriteLine(e.Message); }
        }

        public IEnumerable<FileModel> ListPDFFilesforUser(string emailId)
        {
            Trace.WriteLine("Retriving record....");
            IEnumerable<FileModel> files
                = context.Scan<FileModel>(new ScanCondition("EmailId", ScanOperator.Equal, emailId.ToLower()));
            if (files != null)
            {
                foreach (FileModel file in files)
                    Console.WriteLine("Email: {0}\tKey: {1}\t GKey: {2}\t CPage: {3}", file.EmailId, file.KeyName, file.GeneratedKeyName, file.CurrentPageNumber);
                return files;
            }
            else
                return null;
        }
    }

}
