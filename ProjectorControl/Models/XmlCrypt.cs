using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using Rijndael256; //http://2toad.com/Project/Rijndael256

namespace ProjectorControl.Models
{
    public class XmlCrypt
    { 
        // Web configuration file
        private static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/Views/web.config");
        private static string key = rootWebConfig.AppSettings.Settings["ReadXML"].Value;

        public XmlCrypt ()
        {

        }

        public static XmlNode EncryptNode(XmlNode node)
        {
            string encryptedData = Rijndael.Encrypt(node.OuterXml, key, KeySize.Aes256);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml("<EncryptedData>" +
                encryptedData + "</EncryptedData>");

            return xmlDoc.DocumentElement;
        }

        public static XmlNode DecryptNode(XmlNode encryptedNode)
        {
            string decryptedData = Rijndael.Decrypt(encryptedNode.InnerText, key, KeySize.Aes256);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(decryptedData);

            return xmlDoc.DocumentElement;
        }

        // Automatically encrypt Web.Config's appSettings
        private void EncryptAppSettings()
        {
            AppSettingsSection configAppSettings = (AppSettingsSection)rootWebConfig.GetSection("appSettings");
            if (!configAppSettings.SectionInformation.IsProtected)
            {
                configAppSettings.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                configAppSettings.SectionInformation.ForceSave = true;
                rootWebConfig.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}