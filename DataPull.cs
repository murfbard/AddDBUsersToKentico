
using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using CMS.SiteProvider;
using CMS.CMSHelper;
using CMS.DataEngine;
using System.Diagnostics;

namespace PullKentico
{
    class DataPull
    {
        #region Class Variables
        List<string> NewsLetters = new List<string>();
        bool OnlyShowTen = false;
        bool OnlyCountUsers = false;
        bool EndProgram = false;
        #endregion

        #region Class Entry Points

        public void AutomaticRun()
        {
            ClearKenticoRoles();
            AddCDBUsers();
            Console.Read();
        }

        public void ManualRun()
        {
            Console.WriteLine("You are running this application in Manual Mode");
            Console.WriteLine("You must edit the Program.cs file to run this automatically.");
            Console.WriteLine("");

            while (!EndProgram)
            {
                EndProgram = AskUser();
            }
        }

        

        #endregion

        #region Helper Functions

        private bool AskUser()
        {
            bool DoesUserWantToClose = false;

            Console.WriteLine("Would you like to:");
            Console.WriteLine("     1. Pull 10 users from the client database, sorted by newsletter.");
            Console.WriteLine("     2. List possible Newsletters.");
            Console.WriteLine("     3. Count users from the client database, sorted by newsletter.");
            Console.WriteLine("     4. Count users from Kentico, sorted by newsletter.");
            Console.WriteLine("     5. Delete all newsletter roles in Kentico.");
            Console.WriteLine("     6. Update Kentico Information with the client database users (Add/Update).");
            Console.WriteLine("");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine("");

            if (key.Key == ConsoleKey.D1)
            {
                OnlyShowTen = true;
                GetCDBUsers();
            }
            else if (key.Key == ConsoleKey.D2)
            {
                ListNewsletters();
            }
            else if (key.Key == ConsoleKey.D3)
            {
                GetCDBUsers();
            }
            else if (key.Key == ConsoleKey.D4)
            {
                OnlyCountUsers = true;
                PullKenticoUsersByRole();
            }
            else if (key.Key == ConsoleKey.D5)
            {
                ClearKenticoRoles();
            }
            else if (key.Key == ConsoleKey.D6)
            {
                AddCDBUsers();
            }
            else
            {
                Console.WriteLine("Your input was not recognized.");
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Press y to input another command.");
            ConsoleKeyInfo keyPress = Console.ReadKey();

            if (keyPress.Key == ConsoleKey.Y)
            {
                DoesUserWantToClose = false;
            }
            else
            {
                DoesUserWantToClose = true;
            }

            Console.WriteLine("");
            Console.WriteLine("");

            return DoesUserWantToClose;

        }

        private List<CDBUser> PullCDBUsers(String Key)
        {
            List<CDBUser> newList = new List<CDBUser>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ClientDatabase"].ToString()))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.CommandText = ConfigurationManager.AppSettings[Key];
                    command.Connection = connection;
                    command.Connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            newList.Add(new CDBUser(reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                        }
                    }

                    command.Connection.Close();
                }
            }

            return newList;
        }

        #endregion
        
        #region Client Functions

        private void ListNewsletters()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                Console.WriteLine(key.ToString());
            }
        }

        private void CountCDBUsers()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                List<CDBUser> CDBUserList = PullCDBUsers(key);
                Console.WriteLine("");
                Console.WriteLine(key.ToString());
                Console.Write(CDBUserList.Count.ToString());
                Console.WriteLine("");
            }
        }

        private void GetCDBUsers()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                List<CDBUser> CDBUserList = PullCDBUsers(key);
                int Max = 10;
                int Current = 0;

                Console.WriteLine("");
                Console.WriteLine(key.ToString());

                foreach (CDBUser user in CDBUserList)
                {
                    if (Current < Max)
                    {
                        Console.Write(user.Name + " " + user.Email);
                        Console.WriteLine("");

                        if (OnlyShowTen)
                        {
                            Current++;
                        }
                    }
                }
                Console.WriteLine("");
            }
        }

        private void PullKenticoUsersByRole()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                Console.WriteLine("");
                Console.WriteLine(key);

                RoleInfo Role = RoleInfoProvider.GetRoleInfo(key, CMSContext.CurrentSite.SiteID);
                if (Role != null)
                {
                    System.Data.DataTable UserTable = RoleInfoProvider.GetRoleUsers(Role.RoleID);
                    if (OnlyCountUsers)
                    {
                        Console.WriteLine(UserTable.Rows.Count.ToString());
                    }
                }
                else
                {
                    if (OnlyCountUsers)
                    {
                        Console.WriteLine("0");
                    }
                }
                Console.WriteLine("");
            }

        }

        private void ClearKenticoRoles()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                Console.WriteLine("");

                Console.WriteLine("Deleting " + key + " role from database.");

                RoleInfo Role = RoleInfoProvider.GetRoleInfo(key, CMSContext.CurrentSite.SiteID);
                RoleInfoProvider.DeleteRoleInfo(Role);

                Console.WriteLine(key + " is deleted.");
            }
        }

        /*
         * This was the old function that I revamped.  Only here so you can compare. - Keith Murphy
        private void AddCDBUsersArchivedDonNotUse()
        {
            // This is the old way, this is only in here so I can do a report on it later.  Please do not use.
            // Keith Murphy
            Stopwatch timer = new Stopwatch();

            timer.Reset();
            timer.Start();

            using (CMSConnectionScope cs = new CMSConnectionScope(ConnectionHelper.GetConnection(), true))
            {
                CMS.SettingsProvider.InfoDataSet<UserInfo> KenticoUsers = UserInfoProvider.GetAllUsers();

                foreach (string key in ConfigurationManager.AppSettings.AllKeys)
                {


                    RoleInfo Role = new RoleInfo();
                    Role.RoleName = key;
                    Role.DisplayName = key;
                    RoleInfoProvider.SetRoleInfo(Role);

                    List<CDBUser> CDBUserList = PullCDBUsers(key);

                    Console.WriteLine("");
                    Console.WriteLine(key.ToString());

                    bool FoundUser;
                    int UpdateTotals = 0;
                    int AddTotals = 0;
                    int ErrorTotals = 0;
                    int total = 0;

                    foreach (CDBUser CDBUser in CDBUserList)
                    {
                        try
                        {
                            
                            FoundUser = false;

                            foreach (UserInfo KenticoUser in KenticoUsers)
                            {
                                if (CDBUser.Name.Trim().ToLower() == KenticoUser.UserName.Trim().ToLower())
                                {
                                    // Update Kentico User
                                    UpdateTotals++;
                                    FoundUser = true;

                                    KenticoUser.Email = CDBUser.Email;

                                    UserInfoProvider.SetUserInfo(KenticoUser);
                                    UserInfoProvider.AddUserToRole(KenticoUser.UserName, key, CMSContext.CurrentSite.SiteName);

                                    if (KenticoUser.GetValue("UserPassword").ToString().Length < 1)
                                    {
                                        UserInfoProvider.SetPassword(KenticoUser.UserName, CDBUser.Password);
                                    }

                                    break;
                                }
                            }

                            if (!FoundUser)
                            {
                                // Add Kentico User

                                UserInfo newUser = new UserInfo();
                                newUser.UserName = CDBUser.Name;
                                newUser.Email = CDBUser.Email;

                                UserInfoProvider.SetUserInfo(newUser);
                                UserInfoProvider.AddUserToRole(newUser.UserName, key, CMSContext.CurrentSite.SiteName);

                                UserInfoProvider.SetPassword(newUser.UserName, CDBUser.Password);

                                AddTotals++;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorTotals++;
                        }

                    }
                    Console.WriteLine("Total " + UpdateTotals.ToString() + " updated.");
                    Console.WriteLine("Total " + AddTotals.ToString() + " added.");
                    Console.WriteLine("Total " + ErrorTotals.ToString() + " errors.");
                    Console.WriteLine("");
                }
            }

            timer.Stop();

            Console.WriteLine("Finished the old way in " + timer.Elapsed);
        }
         * */

        private void AddCDBUsers()
        {
            Stopwatch timer = new Stopwatch();

            using (CMSConnectionScope cs = new CMSConnectionScope(ConnectionHelper.GetConnection(), true))
            {
                CMS.SettingsProvider.InfoDataSet<UserInfo> KenticoUsers = UserInfoProvider.GetAllUsers();

                foreach (string key in ConfigurationManager.AppSettings.AllKeys)
                {
                    timer.Reset();
                    timer.Start();

                    RoleInfo Role = new RoleInfo();
                    Role.RoleName = key;
                    Role.DisplayName = key;
                    RoleInfoProvider.SetRoleInfo(Role);

                    List<CDBUser> CDBUserList = PullCDBUsers(key);

                    Console.WriteLine("");
                    Console.WriteLine(key.ToString());

                    int UpdateTotals = 0;
                    int AddTotals = 0;
                    int ErrorTotals = 0;
                    int Total = 0;

                    foreach (CDBUser CDBUser in CDBUserList)
                    {
                        try
                        {
                            var test = UserInfoProvider.GetUserInfo(CDBUser.Name);

                            if (test != null)
                            {
                                UpdateTotals++;

                                test.Email = CDBUser.Email;

                                UserInfoProvider.SetUserInfo(test);
                                UserInfoProvider.AddUserToRole(test.UserName, key, CMSContext.CurrentSite.SiteName);

                                if (test.GetValue("UserPassword").ToString().Length < 1)
                                {
                                    UserInfoProvider.SetPassword(test.UserName, CDBUser.Password);
                                }
                            }
                            else
                            {
                                AddTotals++;

                                UserInfo newUser = new UserInfo();
                                newUser.UserName = CDBUser.Name;
                                newUser.Email = CDBUser.Email;

                                UserInfoProvider.SetUserInfo(newUser);
                                UserInfoProvider.AddUserToRole(newUser.UserName, key, CMSContext.CurrentSite.SiteName);

                                UserInfoProvider.SetPassword(newUser.UserName, CDBUser.Password);
                            }

                        }
                        catch (Exception ex)
                        {
                            ErrorTotals++;
                        }

                        Total++;

                        if (Total % 2000 == 0)
                        {
                            Console.WriteLine(Total + " users parsed.");
                        }

                    }
                    Console.WriteLine("Total " + UpdateTotals.ToString() + " updated.");
                    Console.WriteLine("Total " + AddTotals.ToString() + " added.");
                    Console.WriteLine("Total " + ErrorTotals.ToString() + " errors.");

                    timer.Stop();

                    Console.WriteLine("This operation took: " + timer.Elapsed);
                    Console.WriteLine("");
                }
            }



        }

        #endregion

    }
}

