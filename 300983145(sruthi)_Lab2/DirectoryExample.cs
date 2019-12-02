using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace _300145283_Sruthi__Lab2
{
    class DirectoryExample
    {
        public static void getAccess( string DirectoryName, string Account)
        {
            try
            {
                Trace.WriteLine("Adding access control entry for " + DirectoryName);
                // Add the access control entry to the directory.
                AddDirectorySecurity(DirectoryName, Account, FileSystemRights.FullControl, AccessControlType.Allow);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        public static void removeAccess(string DirectoryName, string Account)
        {
            try
            {
                Trace.WriteLine("Removing access control entry from " + DirectoryName);

                // Remove the access control entry from the directory.
                RemoveDirectorySecurity(DirectoryName, Account, FileSystemRights.FullControl, AccessControlType.Allow);

            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        // Adds an ACL entry on the specified directory for the specified account.
        private static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account,
                                                            Rights,
                                                            ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }

        // Removes an ACL entry on the specified directory for the specified account.
        private static void RemoveDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account,
                                                            Rights,
                                                            ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }
    }
}

