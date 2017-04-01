using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace ProjectorControl.Models
{
    public class Projector
    {
        private List<SelectListItem> ipList;
        private string pcName;
        XmlDocument doc;

        public Projector ()
        {
            ipList = new List<SelectListItem>();
            pcName = "";
            doc = new XmlDocument();   
        }

        public Projector (string pc)
        {
            ipList = new List<SelectListItem>();
            pcName = pc;
            doc = new XmlDocument();   
        }

        public List<SelectListItem> GetProjectorList(int userAuthLevel)
        {
            // Load list of all projector IPs from XML file into ipList
            doc.Load(HttpContext.Current.Server.MapPath("~/Content/SecureProjectors.xml"));

            //XmlDocument myDestDoc = encryptProjectorInfo();
            XmlDocument myDestDoc = decryptProjectorInfo();

            XmlNodeList ipNodes = myDestDoc.SelectNodes("projectors/ip");
            
            //bool projectorListSet = false;

            // Full projector permission
            /*if (userAuthLevel == 2 && projectorListSet == false)
            {
                // Group found in user's credentials, set projector list accordingly.
                foreach (XmlNode node in ipNodes)
                {
                    // Add projector name if available, otherwise just use IP
                    if (node.Attributes["pc"] != null)
                    {
                        string nodePc = node.Attributes["pc"].Value.ToUpper();
                        ipList.Add(new SelectListItem
                        {
                            Text = nodePc + " (" + node.InnerText + ")",
                            Value = node.InnerText
                        });
                    }
                    else
                    {
                        ipList.Add(new SelectListItem
                        {
                            Text = node.InnerText,
                            Value = node.InnerText
                        });
                    }
                }
                // Projector list has been set.
                projectorListSet = true;
            }
            // Room projector permission
            else if (userAuthLevel == 1 && projectorListSet == false)
            {
                // Group found in user's credentials, set projector list accordingly.
                foreach (XmlNode node in ipNodes)
                {
                    // Only adds projector if name is available and matches computer in use
                    if (node.Attributes["pc"] != null)
                    {
                        string nodePc = node.Attributes["pc"].Value.ToUpper();
                        if (nodePc.Equals(pcName))
                        {
                            ipList.Add(new SelectListItem
                            {
                                Text = nodePc + " (" + node.InnerText + ")",
                                Value = node.InnerText
                            });
                        }
                    }
                }
                // Projector list has been set.
                projectorListSet = true;
            }*/

            // Full permissions
            if (userAuthLevel == 2)
            {
                foreach (XmlNode node in ipNodes)
                {
                    addProjector(pcName, node);
                }
            }
            // Room permissions
            else if (userAuthLevel == 1)
            {
                foreach (XmlNode node in ipNodes)
                {
                    addProjectorIfNameEquals(pcName, node);
                }
            }
            else
            {
                /*User user = new User();
                System.Text.StringBuilder allGroups = new System.Text.StringBuilder();
                foreach (String groupName in user.getUserGroups())
                {
                   allGroups.Append(groupName + ", ");
                }*/

                throw new Exception("No authorized usergroup found on your Windows account. Unable to set authorization level.");
            }
           
            return ipList;
        }

        private XmlDocument encryptProjectorInfo()
        {
            // Encrypt projector info
            XmlDocument newDoc = new XmlDocument();
            XmlTextReader myReader = new XmlTextReader(HttpContext.Current.Server.MapPath("~/App_Data/ProjectorIPs.xml"));
            XmlDocument myDestDoc = new XmlDocument();
            myDestDoc.Load(myReader);
            myReader.Close();
            XmlNode rootDest = myDestDoc["projectors"];
            foreach (XmlNode childNode in doc["projectors"].ChildNodes)
            {
                XmlNode nodeOrig = XmlCrypt.EncryptNode(childNode);
                XmlNode nodeDest = myDestDoc.ImportNode(nodeOrig, true);
                rootDest.AppendChild(nodeDest);
            }

            XmlTextWriter myWriter = new XmlTextWriter(HttpContext.Current.Server.MapPath("~/Content/SecureProjectors.xml"), System.Text.Encoding.UTF8);
            myWriter.Formatting = Formatting.Indented;
            myDestDoc.WriteTo(myWriter);
            myWriter.Close();

            return myDestDoc;
        }

        private XmlDocument decryptProjectorInfo()
        {
            // Decrypt projector info
            XmlDocument newDoc = new XmlDocument();
            XmlTextReader myReader = new XmlTextReader(HttpContext.Current.Server.MapPath("~/Content/SecureProjectors.xml"));
            XmlDocument myDestDoc = new XmlDocument();
            myDestDoc.Load(myReader);
            myReader.Close();
            XmlNode rootDest = myDestDoc["projectors"];
            foreach (XmlNode childNode in doc["projectors"].ChildNodes)
            {
                XmlNode nodeOrig = XmlCrypt.DecryptNode(childNode);
                XmlNode nodeDest = myDestDoc.ImportNode(nodeOrig, true);
                rootDest.AppendChild(nodeDest);
            }

            return myDestDoc;
        }

        public void addProjector(string pcName, XmlNode node)
        {
            // Add projector name if available, otherwise just use IP
            if (node.Attributes["pc"] != null)
            {
                string nodePc = node.Attributes["pc"].Value.ToUpper();
                ipList.Add(new SelectListItem
                {
                    Text = nodePc + " (" + node.InnerText + ")",
                    Value = node.InnerText
                });
            }
            else
            {
                ipList.Add(new SelectListItem
                {
                    Text = node.InnerText,
                    Value = node.InnerText
                });
            }
        }

        public void addProjectorIfNameEquals(string pcName, XmlNode node)
        {
            if (node.Attributes["pc"] != null)
            {
                string nodePc = node.Attributes["pc"].Value.ToUpper();
                if (nodePc.Equals(pcName))
                {
                    ipList.Add(new SelectListItem
                    {
                        Text = nodePc + " (" + node.InnerText + ")",
                        Value = node.InnerText
                    });
                }
            }
        }
    }
}