using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _300983145_Sruthi__Lab2
{
    [DynamoDBTable("RegistrationTable")]
    class RegistrationTable
    {
        [DynamoDBHashKey]
        public string EmailId { get; set; }
        [DynamoDBProperty]
        public string Password { get; set; }
        [DynamoDBProperty]
        public string Name { get; set; }

        //a default constructor must be present to use persistance (DynamoDBContext) feature
        public RegistrationTable() { }
        public RegistrationTable(string emailId, string password, string name)
        {
            this.EmailId = emailId;
            this.Password = password;
            this.Name = name;
        }
    }
}

