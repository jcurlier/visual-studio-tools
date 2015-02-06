using Microsoft.VisualStudio.ConnectedServices;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// Provides helper methods for reading and writing configuration settings in the user's project.
    /// </summary>
    internal static class ConfigHelperExtensions
    {
        private const string WebServerSectionName = "system.webServer";

        public static bool IsHandlerNameUsed(this XmlConfigHelper configHelper, string handlerName)
        {
            ConfigurationSection webServerSection = configHelper.Configuration.GetSection(ConfigHelperExtensions.WebServerSectionName);
            if (webServerSection == null)
            {
                return false;
            }

            string webServerSectionRawXml = webServerSection.SectionInformation.GetRawXml();
            if (webServerSectionRawXml == null)
            {
                return false;
            }

            XElement webServerElement = XElement.Parse(webServerSectionRawXml);

            return webServerElement.Elements("handlers")
                .SelectMany(handlersElement => handlersElement.Elements("add"))
                .Any(addElement => addElement.Attribute("name").Value == handlerName);
        }

        public static void RegisterRedirectHandler(this EditableXmlConfigHelper configHelper, string name, string redirectHandlerPath, string fullTypeName)
        {
            // Because the "system.webServer" ConfigurationSection is an IgnoreSection, it must be modified
            // using the raw XML.

            ConfigurationSection webServerSection = configHelper.Configuration.GetSection(ConfigHelperExtensions.WebServerSectionName);
            if (webServerSection == null)
            {
                webServerSection = new IgnoreSection();
                configHelper.Configuration.Sections.Add(ConfigHelperExtensions.WebServerSectionName, webServerSection);
            }

            XElement webServerElement;
            string webServerSectionRawXml = webServerSection.SectionInformation.GetRawXml();
            if (webServerSectionRawXml == null)
            {
                webServerElement = new XElement(ConfigHelperExtensions.WebServerSectionName);
            }
            else
            {
                webServerElement = XElement.Parse(webServerSectionRawXml);
            }

            XElement handlersElement = webServerElement.Element("handlers");

            if (handlersElement == null)
            {
                handlersElement = new XElement("handlers");
                webServerElement.Add(handlersElement);
            }

            XElement addElement = handlersElement.Elements("add")
                .FirstOrDefault(e =>
                {
                    XAttribute attr = e.Attribute("type");
                    return attr != null && attr.Value == fullTypeName;
                });

            if (addElement == null)
            {
                addElement = new XElement("add",
                   new XAttribute("name", name),
                   new XAttribute("verb", "GET"),
                   new XAttribute("path", redirectHandlerPath),
                   new XAttribute("type", fullTypeName));
                handlersElement.Add(addElement);
            }

            webServerSection.SectionInformation.SetRawXml(webServerElement.ToString());
        }
    }
}
