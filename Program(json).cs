using System;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Net;
using System.Text;

namespace VAPIConsoleReader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string xml = Utils.GetAPIGenericURL(EDIName.Customers);
                XMLPostManager manager = new XMLPostManager();
                XmlDocument doc = manager.GetXMLFromURL(xml);
                string json = JsonConvert.SerializeXmlNode(doc);
                Console.WriteLine("XML -> JSON: {0}", json);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            Console.ReadLine();
        }
    }

    public enum EDIName
    {
        [Description(@"Generic\Orders")]
        Orders,
        [Description(@"Generic\Products")]
        Products,
        [Description(@"Generic\Customers")]
        Customers
    }

    public class Utils
    {
        public static readonly string VOLUSION_BASE_API = ConfigurationManager.AppSettings["VOLUSION_BASE_API"];
        public static readonly string VOLUSION_USER = ConfigurationManager.AppSettings["VOLUSION_USER"];
        public static readonly string VOLUSION_PASSWORD = ConfigurationManager.AppSettings["VOLUSION_PASSWORD"];

        /// <summary>
        /// Gets the API URL For generic exports
        /// </summary>
        /// <param name="eDIName"></param>
        /// <returns></returns>
        public static string GetAPIGenericURL(EDIName eDIName)
        {
            return string.Format("{0}?Login={1}&EncryptedPassword={2}&EDI_Name={3}&SELECT_Columns=*", VOLUSION_BASE_API, VOLUSION_USER, VOLUSION_PASSWORD, GetEnumDescription(eDIName));
        }

        /// <summary>
        /// Gets the Description tag value from an enum
        /// </summary>
        /// <param name="value">The enum</param>
        /// <returns>The Description dag value as a string</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }
    public class XMLPostManager
    {
        /// <summary>
        /// Gets back an XML Document from a URL
        /// </summary>
        /// <param name="APIURL">The URL</param>
        /// <returns>The XML Document</returns>
        public XmlDocument GetXMLFromURL(string APIURL)
        {
            XmlDocument APIDoc = null;
            try
            {
                XmlTextReader reader = new XmlTextReader(APIURL);
                APIDoc = new XmlDocument();
                APIDoc.Load(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error reading API URL", e);
            }
            return APIDoc;
        }
    }
}

 


	 

 




 
