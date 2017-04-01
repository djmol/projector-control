using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

namespace ProjectorControl.Models
{
    public class User
    {
        // Web configuration file
        private Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/Web.config");

        private int userAuthLevel = 0;
        private WindowsPrincipal userPrincipal;
        private String username;
        private List<String> userGroups = new List<String>();

        private List<String> loadPermGroups(string permGroup)
        {
            List<String> permGroupsList;

            if (rootWebConfig.AppSettings.Settings.Count > 0)
            {
                KeyValueConfigurationElement permGroups = rootWebConfig.AppSettings.Settings[permGroup];
                String permGroupsValue = permGroups.Value.Replace(" ", "");
                permGroupsList = new List<String>(permGroupsValue.Split(','));
            }
            else
            {
                throw new Exception("Cannot load configuration file for permissions.");
            }

            return permGroupsList;
        }

        private bool compareUserGroups(List<String> otherUserGroups)
        {
            foreach (String otherUserGroup in otherUserGroups)
            {
                foreach (String userGroup in userGroups)
                {
                    if (userGroup.Equals(otherUserGroup))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public User ()
        {
            userAuthLevel = 0;
            userPrincipal = System.Threading.Thread.CurrentPrincipal as WindowsPrincipal;

            // Truncates domain from username
            int usernameIndex = userPrincipal.Identity.Name.LastIndexOf('\\') + 1;
            username = userPrincipal.Identity.Name.Substring(usernameIndex, userPrincipal.Identity.Name.Length - usernameIndex);

            // Get all of a user's usergroups
            /*foreach (System.Security.Principal.IdentityReference group in System.Web.HttpContext.Current.Request.LogonUserIdentity.Groups)
            {
                userGroups.Add(group.Translate(typeof(System.Security.Principal.NTAccount)).ToString().ToUpper());
            }*/

            /*WindowsIdentity wi = new WindowsIdentity(WindowsIdentity.GetCurrent().Token);

            foreach (IdentityReference group in wi.Groups)
            {
                try
                {
                    userGroups.Add(group.Translate(typeof(NTAccount)).ToString().ToUpper());
                }
                catch (Exception ex) { }
            }*/

            PrincipalContext yourDomain = new PrincipalContext(ContextType.Domain,"PCTI");
                        
            // Find the user
            UserPrincipal user = UserPrincipal.FindByIdentity(yourDomain, username);

            // If user is found
            if (user != null)
            {
                // Get DirectoryEntry underlying it
                DirectoryEntry de = (user.GetUnderlyingObject() as DirectoryEntry);
                List<GroupPrincipal> userGroupPrincipals = new List<GroupPrincipal>();

                if (de != null)
                {
                    var sroles = user.GetAuthorizationGroups();
                    if (sroles != null && sroles.Count() > 0)
                    {
                        int i = 0;
                        while (i < sroles.Count())
                        {
                            try
                            {
                                var role = sroles.ElementAt(i);
                                if (role != null && role.Name != null)
                                    userGroups.Add(role.Name.ToUpper());
                            }
                            catch
                            {
                                Console.WriteLine("Bad group!");
                            }
                            i++;
                        }
                    }
                    //userGroupPrincipals.AddRange(user.GetAuthorizationGroups().OfType<GroupPrincipal>());
                    //userGroups = new List<string>(userGroupPrincipals.Cast<string>());
                    //userGroups = userGroupPrincipals.Select(s => s.ToString().ToUpper()).ToList();
                }
                else
                {
                    throw new Exception("Cannot obtain usergroups.");
                }
            }
        }

        public int getUserAuthLevel()
        {
            return userAuthLevel;
        }

        public void setUserAuthLevel(int level)
        {
            userAuthLevel = level;
        }

        public WindowsPrincipal getUserPrincipal()
        {
            return userPrincipal;
        }

        public string getUsername()
        {
            return username;
        }

        public List<String> getUserGroups()
        {
            return userGroups;
        }

        public void setPermissions(string permGroup, int permGroupAuthLevel)
        {
            // Load all user groups from indicated permission group in Web.config
            List<String> permGroups = loadPermGroups(permGroup);

            // If user is in any permission groups, set user's authorization level
            if (compareUserGroups(permGroups))
                setUserAuthLevel(permGroupAuthLevel);
        }
    }
}