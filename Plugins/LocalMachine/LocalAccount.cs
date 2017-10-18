﻿/*
	Copyright (c) 2017, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors
		  may be used to endorse or promote products derived from this software without
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Net;

using pGina.Shared.Types;
using Abstractions;

namespace pGina.Plugin.LocalMachine
{
    public class LocalAccount
    {
        private static ILog m_logger = null;
        private static DirectoryEntry m_sam = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
        private static PrincipalContext m_machinePrincipal = new PrincipalContext(ContextType.Machine);
        private static Random randGen = new Random();

        public class GroupSyncException : Exception
        {
            public GroupSyncException(Exception e)
            {
                RootException = e;
            }

            public Exception RootException { get; private set; }
        };

        private UserInformation m_userInfo = null;
        public UserInformation UserInfo
        {
            get { return m_userInfo; }
            set
            {
                m_userInfo = value;
                m_logger = LogManager.GetLogger(string.Format("LocalAccount[{0}]", m_userInfo.Username));
            }
        }

        static LocalAccount()
        {
            m_logger = LogManager.GetLogger("LocalAccount");
        }

        public LocalAccount(UserInformation userInfo)
        {
            UserInfo = userInfo;
        }

        /// <summary>
        /// Finds and returns the UserPrincipal object if it exists, if not, returns null.
        /// This method uses PrincipalSearcher because it is faster than UserPrincipal.FindByIdentity.
        /// The username comparison is case insensitive.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The UserPrincipal object, or null if not found.</returns>
        public static UserPrincipal GetUserPrincipal(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;

            // Since PrincipalSearcher is case sensitive, and we want a case insensitive
            // search, we get a list of all users and compare the names "manually."
            using (PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(m_machinePrincipal)))
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is UserPrincipal)
                    {
                        UserPrincipal user = (UserPrincipal)p;
                        if (user.Name.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                            return user;
                    }
                }
            }

            return null;
        }

        public static UserPrincipal GetUserPrincipal(SecurityIdentifier sid)
        {
            // This could be updated to use PrincipalSearcher, but the method is currently
            // unused.
            return UserPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Sid, sid.ToString());
        }

        /// <summary>
        /// Finds and returns the GroupPrincipal object if it exists, if not, returns null.
        /// This method uses PrincipalSearcher because it is faster than GroupPrincipal.FindByIdentity.
        /// The group name comparison is case insensitive.
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public static GroupPrincipal GetGroupPrincipal(string groupname)
        {
            if (string.IsNullOrEmpty(groupname)) return null;

            // In order to do a case insensitive search, we need to scan all
            // groups "manually."
            using(PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(m_machinePrincipal)))
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is GroupPrincipal)
                    {
                        GroupPrincipal group = (GroupPrincipal)p;
                        if (group.Name.Equals(groupname, StringComparison.CurrentCultureIgnoreCase) || group.Sid.ToString().Equals(groupname, StringComparison.CurrentCultureIgnoreCase))
                            return group;
                    }
                }
            }
            return null;
        }

        public static GroupPrincipal GetGroupPrincipal(SecurityIdentifier sid)
        {
            using (PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(m_machinePrincipal)))
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is GroupPrincipal)
                    {
                        GroupPrincipal group = (GroupPrincipal)p;
                        if (group.Sid == sid)
                            return group;
                    }
                }
            }
            return null;
        }

        public static DirectoryEntry GetUserDirectoryEntry(string username)
        {
            return m_sam.Children.Find(username, "User");
        }

        public void EnableDisableAccount(string username, bool enable)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, username);
            if (user != null)
            {
                user.Enabled = enable;
                user.Save();
            }
        }

        public void ScrambleUsersPassword(string username)
        {
            using (DirectoryEntry userDe = GetUserDirectoryEntry(username))
            {
                userDe.Invoke("SetPassword", GenerateRandomPassword(30));
                userDe.CommitChanges();
            }
        }

        /// <summary>
        /// Generates a random password that meets most of the requriements of Windows
        /// Server when password policy complexity requirements are in effect.
        ///
        /// http://technet.microsoft.com/en-us/library/cc786468%28v=ws.10%29.aspx
        ///
        /// This generates a string with at least two of each of the following character
        /// classes: uppercase letters, lowercase letters, and digits.
        ///
        /// However, this method does not check for the existence of the username or
        /// display name within the
        /// generated password.  The probability of that occurring is somewhat remote,
        /// but could happen.  If that is a concern, this method could be called repeatedly
        /// until a string is returned that does not contain the username or display name.
        /// </summary>
        /// <param name="length">Password length, must be at least 6.</param>
        /// <returns>The generated password</returns>
        public static string GenerateRandomPassword(int length)
        {
            if (length < 6) throw new ArgumentException("length must be at least 6.");

            StringBuilder pass = new StringBuilder();

            // Temporary array containing our character set:
            // uppercase letters, lowercase letters, and digits
            char[] charSet = new char[62];
            for( int i = 0; i < 26; i++) charSet[i] = (char)('A' + i);  // Uppercase letters
            for( int i = 26; i < 52; i++) charSet[i] = (char)('a' + i - 26); // Lowercase letters
            for( int i = 52; i < 62; i++) charSet[i] = (char)('0' + i - 52); // Digits

            // We generate two of each character class.
            pass.Append(charSet[randGen.Next(0,26)]);    // uppercase
            pass.Append(charSet[randGen.Next(0, 26)]);   // uppercase
            pass.Append(charSet[randGen.Next(26, 52)]);  // lowercase
            pass.Append(charSet[randGen.Next(26, 52)]);  // lowercase
            pass.Append(charSet[randGen.Next(52,62)]);   // digit
            pass.Append(charSet[randGen.Next(52,62)]);   // digit

            // The rest of the password is randomly generated from the full character set
            for (int i = pass.Length; i < length; i++ )
            {
                pass.Append(charSet[randGen.Next(0, charSet.Length)]);
            }

            // Shuffle the password using the Fisher-Yates random permutation technique
            for (int i = 0; i < pass.Length; i++ )
            {
                int j = randGen.Next(i, pass.Length);
                // Swap i and j
                char tmp = pass[i];
                pass[i] = pass[j];
                pass[j] = tmp;
            }

            return pass.ToString();
        }

        // Non recursive group check (immediate membership only currently)
        private bool IsUserInGroup(string username, string groupname)
        {
            using(GroupPrincipal group = GetGroupPrincipal(groupname))
            {
                if (group == null) return false;

                using(UserPrincipal user = GetUserPrincipal(username))
                {
                    if (user == null) return false;

                    return IsUserInGroup(user, group);
                }
            }
        }

        // Non recursive group check (immediate membership only currently)
        private static bool IsUserInGroup(UserPrincipal user, GroupPrincipal group)
        {
            if (user == null || group == null) return false;

            // This may seem a convoluted and strange way to check group membership.
            // Especially because I could just call user.IsMemberOf(group).
            // The reason for all of this is that IsMemberOf will throw an exception
            // if there is an unresolvable SID in the list of group members.  Unfortunately,
            // even looping over the members with a standard foreach loop doesn't allow
            // for catching the exception and continuing.  Therefore, we need to use the
            // IEnumerator object and iterate through the members carefully, catching the
            // exception if it is thrown.  I throw in a sanity check because there's no
            // guarantee that MoveNext will actually move the enumerator forward when an
            // exception occurs, although it has done so in my tests.
            //
            // For additional details, see the following bug:
            // https://connect.microsoft.com/VisualStudio/feedback/details/453812/principaloperationexception-when-enumerating-the-collection-groupprincipal-members

            PrincipalCollection members = group.Members;
            bool ok = true;
            int errorCount = 0;  // This is a sanity check in case the loop gets out of control
            IEnumerator<Principal> membersEnum = members.GetEnumerator();
            while (ok)
            {
                try { ok = membersEnum.MoveNext(); }
                catch (PrincipalOperationException)
                {
                    m_logger.ErrorFormat("PrincipalOperationException when checking group membership for user {0} in group {1}." +
                        "  This usually means that you have an unresolvable SID as a group member." +
                        "  I strongly recommend that you fix this problem as soon as possible by removing the SID from the group. " +
                        "  Ignoring the exception and continuing.",
                        user.Name, group.Name);

                    // Sanity check to avoid infinite loops
                    errorCount++;
                    if (errorCount > 1000) return false;

                    continue;
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    m_logger.ErrorFormat("COMException when checking group membership for user {0} in group {1}." +
                        "  This usually means that you have a domain user/group in your local group. but you are running pGina-service with a local/machine admin." +
                        "  I strongly recommend that you fix this problem as soon as possible by delete the domain user/group or run de service as a domain user with admin rights on this pc.",
                        user.Name, group.Name);
                    m_logger.Error(e);
                    // Sanity check to avoid infinite loops
                    errorCount++;
                    if (errorCount > 1000) return false;

                    continue;
                }
                if (ok)
                {
                    Principal principal = membersEnum.Current;

                    if (principal is UserPrincipal && principal.Sid == user.Sid)
                        return true;
                }
            }

            return false;
        }

        private bool IsUserInGroup(UserPrincipal user, GroupInformation groupInfo)
        {
            using (GroupPrincipal group = GetGroupPrincipal(groupInfo.Name))
            {
                return IsUserInGroup(user, group);
            }
        }

        private GroupPrincipal CreateOrGetGroupPrincipal(GroupInformation groupInfo)
        {
            GroupPrincipal group = null;

            // If we have a SID, use that, otherwise name
            group = GetGroupPrincipal(groupInfo.Name);

            if (group == null)
            {
                // We create the GroupPrincipal, but https://connect.microsoft.com/VisualStudio/feedback/details/525688/invalidoperationexception-with-groupprincipal-and-sam-principalcontext-for-setting-any-property-always
                // prevents us from then setting stuff on it.. so we then have to locate its relative DE
                // and modify *that* instead.  Oi.
                using (group = new GroupPrincipal(m_machinePrincipal))
                {
                    group.Name = groupInfo.Name;
                    group.Save();

                    using (DirectoryEntry newGroupDe = m_sam.Children.Add(groupInfo.Name, "Group"))
                    {
                        if (!string.IsNullOrEmpty(groupInfo.Description))
                        {
                            newGroupDe.Properties["Description"].Value = groupInfo.Description;
                            newGroupDe.CommitChanges();
                        }
                    }

                    // We have to re-fetch to get changes made via underlying DE
                    return GetGroupPrincipal(group.Name);
                }
            }

            return group;
        }

        private UserPrincipal CreateOrGetUserPrincipal(UserInformation userInfo)
        {
            UserPrincipal user = null;
            if ( ! LocalAccount.UserExists(userInfo.Username) )
            {
                // See note about MS bug in CreateOrGetGroupPrincipal to understand the mix of DE/Principal here:
                using (user = new UserPrincipal(m_machinePrincipal))
                {
                    user.Name = userInfo.Username;
                    user.SetPassword(userInfo.Password);
                    user.Description = "pGina created";
                    userInfo.Description = user.Description;
                    if (userInfo.PasswordEXP)
                        user.ExpirePasswordNow();
                    user.Save();

                    // Sync via DE
                    SyncUserPrincipalInfo(userInfo);

                    // We have to re-fetch to get changes made via underlying DE
                    return GetUserPrincipal(user.Name);
                }
            }

            user = GetUserPrincipal(userInfo.Username);
            if (user == null)
                m_logger.ErrorFormat("Unable to get user principal for account that apparently exists: {0}", userInfo.Username);

            return user;
        }

        private void SyncUserPrincipalInfo(UserInformation info)
        {
            using(DirectoryEntry userDe = m_sam.Children.Find(info.Username, "User"))
            {
                if (!string.IsNullOrEmpty(info.Description)) userDe.Properties["Description"].Value = info.Description;
                if (!string.IsNullOrEmpty(info.Fullname)) userDe.Properties["FullName"].Value = info.Fullname;
                if (!string.IsNullOrEmpty(info.usri4_home_dir)) userDe.Properties["HomeDirectory"].Value = info.usri4_home_dir.Replace("%u", info.Username);
                if (!string.IsNullOrEmpty(info.usri4_home_dir_drive)) userDe.Properties["HomeDirDrive"].Value = info.usri4_home_dir_drive;
                if (!info.Description.Contains("pgSMB"))
                    if (!string.IsNullOrEmpty(info.usri4_profile)) userDe.Properties["Profile"].Value = info.usri4_profile;
                userDe.Invoke("SetPassword", new object[] { info.Password });
                userDe.Properties["PasswordExpired"].Value = Convert.ToInt32(info.PasswordEXP);
                userDe.CommitChanges();
            }
        }

        private void AddUserToGroup(UserPrincipal user, GroupPrincipal group)
        {
            group.Members.Add(user);
            group.Save();
        }

        private void RemoveUserFromGroup(UserPrincipal user, GroupPrincipal group)
        {
            group.Members.Remove(user);
            group.Save();
        }

        public void SyncToLocalUser()
        {
            m_logger.Debug("SyncToLocalUser()");

            using (UserPrincipal user = CreateOrGetUserPrincipal(UserInfo))
            {
                // Force password and fullname match (redundant if we just created, but oh well)
                SyncUserPrincipalInfo(UserInfo);

                try
                {
                    List<SecurityIdentifier> ignoredSids = new List<SecurityIdentifier>(new SecurityIdentifier[] {
                        new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),    // "Authenticated Users"
                        new SecurityIdentifier("S-1-1-0"),                                      // "Everyone"
                    });

                    // First remove from any local groups they aren't supposed to be in
                    m_logger.Debug("Checking for groups to remove.");
                    List<GroupPrincipal> localGroups = LocalAccount.GetGroups(user);
                    foreach (GroupPrincipal group in localGroups)
                    {
                        m_logger.DebugFormat("Remove {0}?", group.Name);
                        // Skip ignored sids
                        if (!ignoredSids.Contains(group.Sid))
                        {
                            GroupInformation gi = new GroupInformation() { Name = group.Name, SID = group.Sid, Description = group.Description };
                            if (!UserInfo.InGroup(gi))
                            {
                                m_logger.DebugFormat("Removing user {0} from group {1}", user.Name, group.Name);
                                RemoveUserFromGroup(user, group);
                            }
                        }
                        group.Dispose();
                    }

                    // Now add to any they aren't already in that they should be
                    m_logger.Debug("Checking for groups to add");
                    foreach (GroupInformation groupInfo in UserInfo.Groups)
                    {
                        m_logger.DebugFormat("Add {0}?", groupInfo.Name);
                        if (!IsUserInGroup(user, groupInfo))
                        {
                            using (GroupPrincipal group = CreateOrGetGroupPrincipal(groupInfo))
                            {
                                m_logger.DebugFormat("Adding user {0} to group {1}", user.Name, group.Name);
                                AddUserToGroup(user, group);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new GroupSyncException(e);
                }
            }

            //set ntuser.dat permissions
            if (!String.IsNullOrEmpty(UserInfo.usri4_profile) && !UserInfo.Description.Contains("pgSMB"))
            {
                Abstractions.WindowsApi.pInvokes.structenums.OSVERSIONINFOW verinfo = Abstractions.WindowsApi.pInvokes.VersionsInfo();
                if (verinfo.dwMajorVersion == 0)
                {
                    m_logger.WarnFormat("SyncToLocalUser: VersionsInfo() failed. I'm unable to detect OS beyond Windows 8.0");
                    verinfo.dwBuildNumber = Environment.OSVersion.Version.Build;
                    verinfo.dwMajorVersion = Environment.OSVersion.Version.Major;
                    verinfo.dwMinorVersion = Environment.OSVersion.Version.Minor;
                    verinfo.dwPlatformId = Environment.OSVersion.Version.Build;
                }
                string ProfileExtension = (Environment.OSVersion.Version.Major == 6) ? (verinfo.dwMinorVersion > 3)/*greater than 8.1*/ ? ".V5" : ".V2" : "";

                if (Connect2share(UserInfo.usri4_profile + ProfileExtension, UserInfo.Username, UserInfo.Password, 3, false))
                {
                    if (File.Exists(UserInfo.usri4_profile + ProfileExtension + "\\NTUSER.DAT"))
                    {
                        SetACL(UserInfo, ProfileExtension);
                        Connect2share(UserInfo.usri4_profile + ProfileExtension, null, null, 0, true);
                    }
                    else
                    {
                        Connect2share(UserInfo.usri4_profile + ProfileExtension, null, null, 0, true);
                    }
                }
            }

            m_logger.Debug("End SyncToLocalUser()");
        }

        public static void SyncUserInfoToLocalUser(UserInformation userInfo)
        {
            LocalAccount la = new LocalAccount(userInfo);
            la.SyncToLocalUser();
        }

        // Load userInfo.Username's group list and populate userInfo.Groups accordingly
        public static void SyncLocalGroupsToUserInfo(UserInformation userInfo)
        {
            ILog logger = LogManager.GetLogger("LocalAccount.SyncLocalGroupsToUserInfo");
            try
            {
                SecurityIdentifier EveryoneSid = new SecurityIdentifier("S-1-1-0");
                SecurityIdentifier AuthenticatedUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

                if (LocalAccount.UserExists(userInfo.Username))
                {
                    using (UserPrincipal user = LocalAccount.GetUserPrincipal(userInfo.Username))
                    {
                        foreach (GroupPrincipal group in LocalAccount.GetGroups(user))
                        {
                            // Skip "Authenticated Users" and "Everyone" as these are generated
                            if (group.Sid == EveryoneSid || group.Sid == AuthenticatedUsersSid)
                                continue;

                            userInfo.AddGroup(new GroupInformation()
                            {
                                Name = group.Name,
                                Description = group.Description,
                                SID = group.Sid
                            });
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.ErrorFormat("Unexpected error while syncing local groups, skipping rest: {0}", e);
            }
        }

        public void RemoveUserAndProfile(string user, int sessionID)
        {
            // First we have to work out where the users profile is on disk.
            try
            {
                if (!String.IsNullOrEmpty(UserInfo.LocalProfilePath))
                {
                    // instead of while (true)
                    if (File.Exists(UserInfo.LocalProfilePath + "\\NTUSER.DAT"))
                    {
                        bool inuse = true;
                        for (int x = 0; x < 60; x++)
                        {
                            try
                            {
                                using (FileStream isunloaded = File.Open(UserInfo.LocalProfilePath + "\\NTUSER.DAT", FileMode.Open, FileAccess.Read))
                                {
                                    inuse = false;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                m_logger.DebugFormat("loop{1}:{0}", ex.Message, x);
                                Thread.Sleep(1000);
                            }
                        }
                        if (inuse)
                        {
                            return;
                        }
                    }
                    m_logger.DebugFormat("User {0} has profile in {1}, giving myself delete permission", user, UserInfo.LocalProfilePath);
                    try { Directory.Delete(UserInfo.LocalProfilePath, true); }
                    catch { }
                    RecurseDelete(UserInfo.LocalProfilePath);
                }
                // Now remove it from the registry as well
                Abstractions.WindowsApi.pInvokes.DeleteProfile(UserInfo.SID);
                Abstractions.Windows.User.DelProfileList(UserInfo.SID.ToString());
            }
            catch (KeyNotFoundException)
            {
                m_logger.DebugFormat("User {0} has no disk profile, just removing principal", user);
            }

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData\" + sessionID.ToString() + "'");
            try{Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData\" + sessionID.ToString(), false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\NetCache\PurgeAtNextLogoff\" + UserInfo.SID + "'");
            try{Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\NetCache\PurgeAtNextLogoff\", true).DeleteValue(UserInfo.SID.ToString(), false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Status\" + UserInfo.SID + "'");
            try{Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Status\" + UserInfo.SID, false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\" + UserInfo.SID + "'");
            try{Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\" + UserInfo.SID, false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\" + UserInfo.SID + "'");
            try{Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\" + UserInfo.SID, false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\Windows\CurrentVersion\GameUX\" + UserInfo.SID + "'");
            try {Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\GameUX\" + UserInfo.SID, false);}
            catch {}

            m_logger.Debug(@"removing SessionData 'SOFTWARE\Microsoft\IdentityStore\Cache\" + UserInfo.SID + "'");
            try {Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\IdentityStore\Cache\" + UserInfo.SID, false);}
            catch {}

            using (UserPrincipal userPrincipal = GetUserPrincipal(user))
            {
                try
                {
                    userPrincipal.Delete();
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("userPrincipal.Delete error:{0}", ex.Message);
                }
            }

            m_logger.Debug(@"removing Profile 'SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + UserInfo.SID + "'");
            try { Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + UserInfo.SID, false); }
            catch { }
        }

        private static void RecurseDelete(string directory)
        {
            // m_logger.DebugFormat("Dir: {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }

            DirectorySecurity dirSecurity = Directory.GetAccessControl(directory);
            dirSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
            Directory.SetAccessControl(directory, dirSecurity);
            File.SetAttributes(directory, FileAttributes.Normal);

            DirectoryInfo di = new DirectoryInfo(directory);
            if ((di.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                // m_logger.DebugFormat("{0} is a reparse point, just deleting without recursing", directory);
                Directory.Delete(directory, false);
                return;
            }

            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            // Files
            foreach (string file in files)
            {
                try
                {
                    // m_logger.DebugFormat("File: {0}", file);
                    FileSecurity fileSecurity = File.GetAccessControl(file);
                    fileSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(file, fileSecurity); // Set the new access settings.
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    m_logger.Debug(ex.Message);
                }
            }

            // Recurse each dir
            foreach (string dir in dirs)
            {
                try
                {
                    RecurseDelete(dir);
                }
                catch (Exception ex)
                {
                    m_logger.Debug(ex.Message);
                }
            }
            try
            {
                Directory.Delete(directory, false);
            }
            catch (Exception ex)
            {
                m_logger.Debug(ex.Message);
            }
        }

        private static Boolean Connect2share(string share, string username, string password, uint retry, Boolean DISconnect)
        {
            if (DISconnect)
            {
                m_logger.DebugFormat("Disconnect from {0}", share);
                if (!Abstractions.WindowsApi.pInvokes.DisconnectNetworkDrive(share))
                {
                    m_logger.WarnFormat("unable to disconnect from {0}", share);
                }

                return true;
            }
            else
            {
                string[] server = SMBserver(share, false);
                if (String.IsNullOrEmpty(server[0]))
                {
                    m_logger.ErrorFormat("Can't extract SMB server from {0}", share);
                    return false;
                }

                server[0] += "\\" + username;

                for (int x = 1; x <= retry; x++)
                {
                    try
                    {
                        m_logger.DebugFormat("{0}. try to connect to {1} as {2}", x, share, server[0]);
                        if (!Abstractions.WindowsApi.pInvokes.MapNetworkDrive(share, server[0], password))
                        {
                            m_logger.ErrorFormat("Failed to connect to share {0}", share);
                        }
                        if (Directory.Exists(share))
                        {
                            return true;
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 30));
                    }
                    catch (Exception ex)
                    {
                        m_logger.Error(ex.Message);
                    }
                }

                return false;
            }
        }

        private static Boolean SetACL(UserInformation userInfo, string ProfileExtension)
        {
            if (!Abstractions.WindowsApi.pInvokes.RegistryLoad(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, userInfo.Username, userInfo.usri4_profile + ProfileExtension/*.V2|.V5*/ + "\\NTUSER.DAT"))
            {
                m_logger.WarnFormat("Can't load regfile {0}", userInfo.usri4_profile + ProfileExtension/*.V2|.V5*/ + "\\NTUSER.DAT");
                return false;
            }
            if (!Abstractions.Windows.Security.RegSec(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, userInfo.Username, userInfo.Username))
            {
                m_logger.WarnFormat("Can't set ACL for regkey {0}\\{1}", Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE.ToString(), userInfo.Username);
                return false;
            }
            if (!Abstractions.WindowsApi.pInvokes.RegistryUnLoad(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, userInfo.Username))
            {
                m_logger.WarnFormat("Can't unload regkey {0}\\{1}", userInfo.usri4_profile + ProfileExtension/*.V2|.V5*/, "NTUSER.DAT");
                return false;
            }

            return true;
        }

        private static string[] SMBserver(string share, Boolean visversa)
        {
            string[] ret = { null, null };
            string[] server;
            try
            {
                server = share.Trim('\\').Split('\\');
            }
            catch
            {
                m_logger.DebugFormat("can't split servername {0}", share);
                return ret;
            }

            if (!String.IsNullOrEmpty(server[0]))
            {
                ret[0] = server[0];
                ret[1] = server[0];
                if (!visversa)
                    return ret;
                try
                {
                    IPHostEntry hostFQDN = Dns.GetHostEntry(server[0]);
                    if (hostFQDN.HostName.Equals(server[0], StringComparison.CurrentCultureIgnoreCase))
                    {
                        IPAddress[] hostIPs = Dns.GetHostAddresses(server[0]);
                        ret[1] = hostIPs[0].ToString();
                    }
                    else
                    {
                        ret[1] = hostFQDN.HostName;
                    }
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("can't resolve FQDN of {0}:{1}", server[0], ex.Message);
                    return new string[] { null, null };
                }
            }
            else
                m_logger.DebugFormat("first token of servername {0} is null", share);

            return ret;
        }

        /// <summary>
        /// This is a faster technique for determining whether or not a user exists on the local
        /// machine.  UserPrincipal.FindByIdentity tends to be quite slow in general, so if
        /// you only need to know whether or not the account exists, this method is much
        /// faster.
        /// </summary>
        /// <param name="strUserName">The user name</param>
        /// <returns>Whether or not the account with the given user name exists on the system</returns>
        public static bool UserExists(string strUserName)
        {
            try
            {
                using (DirectoryEntry userEntry = LocalAccount.GetUserDirectoryEntry(strUserName))
                {
                    return userEntry != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a list of groups of which the user is a member.  It does so in a fashion that
        /// may seem strange since one can call UserPrincipal.GetGroups, but seems to be much faster
        /// in my tests.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static List<GroupPrincipal> GetGroups(UserPrincipal user)
        {
            List<GroupPrincipal> result = new List<GroupPrincipal>();
            PrincipalSearchResult<Principal> sResult;
            try
            {
                // Get all groups using a PrincipalSearcher and
                GroupPrincipal filter = new GroupPrincipal(m_machinePrincipal);
                using (PrincipalSearcher searcher = new PrincipalSearcher(filter))
                {
                    sResult = searcher.FindAll();
                }
            }
            catch
            {
                // fallback for machine running pgina as local/machine admin and have groups from a domain.
                // Failed to process gateway for ******* message: Unable to sync users local group membership: System.Runtime.InteropServices.COMException (0x80070035): The network path was not found.
                sResult = user.GetGroups();
            }

            foreach (Principal p in sResult)
            {
                if (p is GroupPrincipal)
                {
                    GroupPrincipal gp = (GroupPrincipal)p;
                    if (LocalAccount.IsUserInGroup(user, gp))
                        result.Add(gp);
                    else
                        gp.Dispose();
                }
                else
                {
                    p.Dispose();
                }
            }

            return result;
        }
    }
}
