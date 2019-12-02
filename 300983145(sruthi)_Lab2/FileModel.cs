using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _300983145_Sruthi__Lab2
{
    [DynamoDBTable("FileTable")]
    public class FileModel
    {
        [DynamoDBHashKey]
        public string EmailId { get; set; }
        [DynamoDBRangeKey]
        public string KeyName { get; set; }
        [DynamoDBProperty]
        public string GeneratedKeyName { get; set; }
        [DynamoDBProperty]
        public int CurrentPageNumber { get; set; }
        [DynamoDBProperty]
        public DateTime LastAccessedTime { get; set; }


        public FileModel() { }

        public FileModel(string EmailId, string KeyName)
        {
            this.EmailId = EmailId;
            this.KeyName = KeyName;
            GeneratedKeyName = (DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                Regex.Replace(this.EmailId, @"\p{P}", "") + "_" + KeyName.ToLowerInvariant());
            CurrentPageNumber = 0;
        }
    }
}
