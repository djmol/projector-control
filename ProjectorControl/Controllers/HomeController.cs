using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using ProjectorControl.Models;
using System.Configuration;

namespace ProjectorControl.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            User user = new User();

            // Permission groups declared in Web.config
            Dictionary<String, int> permGroups = new Dictionary<String, int>()
            {
                { "RoomPermissions", 1 },
                { "FullPermissions", 2 }
            };

            // Date and time
            ViewBag.CurrentTime = DateTime.Now.ToString("F");

            // Catch WindowsPrincipal errors
            if (user.getUserPrincipal() == null)
            {
                ViewBag.userAuth = "ERROR: UserPrincipal is null";
            }
            else if (user.getUserPrincipal().Identity == null)
            {
                ViewBag.userAuth = "ERROR: Identity is null";
            }
            else if (!user.getUserPrincipal().Identity.IsAuthenticated)
            {
                ViewBag.userAuth = "ERROR: User not authenticated";
            }
            else
            {
                ViewBag.userAuth = "Logged in as " + user.getUsername();

                foreach (KeyValuePair<String, int> permGroup in permGroups)
                    user.setPermissions(permGroup.Key, permGroup.Value);

                // FOR DEVELOPMENT -- Full user/impersonation check
                ViewBag.LogonUID = System.Web.HttpContext.Current.Request.LogonUserIdentity.Name.ToString();
                ViewBag.IsAuth = System.Web.HttpContext.Current.Request.IsAuthenticated.ToString();
                ViewBag.IDName = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                ViewBag.EnvUN = System.Environment.UserName.ToString();

                // FOR DEVELOPMENT -- Impersonated user
                ViewBag.WinIDCurName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                // FOR DEVELOPMENT -- Store all groups for display
                //ViewBag.UserGroups = userGroups.ToArray();

                // FOR DEVELOPMENT -- PC Name
                ViewBag.PCName = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName.ToUpper();

                // If authorized, give link to Control page; otherwise, link redirects to Index
                if (user.getUserAuthLevel() > 0)
                {
                    ViewBag.Destination = "Control";
                }
                else
                {
                    ViewBag.Destination = "Index";
                }
            }

            // Obtain client PC name
            string pcName;
            string pcDomainName = ".PCTI.TEC.NJ.US";
            string fullPcName = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName.ToUpper();
            if (fullPcName.Contains(pcDomainName))
            {
                pcName = fullPcName.Remove(fullPcName.IndexOf(pcDomainName), pcDomainName.Length);
            }
            else
            {
                pcName = fullPcName;
            }

            ViewBag.pcName = pcName;

            // Get all Projector IPs from XML list in App_Data
            Projector projectorList = new Projector(pcName);
            ViewBag.deviceIp = projectorList.GetProjectorList(user.getUserAuthLevel());

            return View();
        }

        [Authorize]
        public ViewResult Control(string deviceIp)
        {
            // If user reaches Control page without selecting device IP, throw exception.
            if (String.IsNullOrEmpty(deviceIp))
            {
                deviceIp = "127.0.0.1";
                throw new Exception("No device IP selected.");
            }

            RemoteConnection remoteConnection = new RemoteConnection();
            remoteConnection.SetIp(deviceIp);
            ViewBag.deviceIp = deviceIp;
            
            return View();
        }

        public ViewResult Close()
        {
            return View();
        }

        public void SendRemoteCommand(string commandId)
        {
            // To hold the command (all remote commands are exactly 8 bytes long)
            byte[] command = new byte[8];

            // Send commandId to RemoteCommander, which applies the appropriate data and header and returns it as a byte array, ready for sending
            RemoteCommander commander = new RemoteCommander();
            commander.SetCommand(commandId);
            command = commander.GetCommand();

            // Send command byte array with RemoteConnection
            // Each command is sent over an independent TCP stream
            RemoteConnection remoteConnection = new RemoteConnection();
            remoteConnection.Connect();
            remoteConnection.Write(command);
            remoteConnection.Close();
        }
	}
}