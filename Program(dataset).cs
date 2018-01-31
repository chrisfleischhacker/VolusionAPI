using System;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Net;
using System.Text;


namespace VAPI2DatasetConsoleReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string customersURL = Utils.GetAPIGenericURL(EDIName.Customers);
            try
            {
                XMLPostManager manager = new XMLPostManager();
                XmlDocument doc = manager.GetXMLFromURL(customersURL);
                MemoryStream ms = new MemoryStream();
                doc.Save(ms);
                ms.Seek(0, 0);
                DataSet Ds = new DataSet();
                Ds.ReadXml(ms, XmlReadMode.Auto);
                if (Ds.Tables.Count > 0)
                {
                    PrintDataSet(Ds);
                }
                else
                {
                    Console.WriteLine("No new data to display");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            Console.ReadLine();
        }

        static void PrintDataSet(DataSet ds)
        {
            Console.WriteLine("Tables in '{0}' DataSet.\n", ds.DataSetName);
            foreach (DataTable dt in ds.Tables)
            {
                Console.WriteLine("{0} Table.\n", dt.TableName);
                for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                {
                    Console.Write(dt.Columns[curCol].ColumnName.Trim() + "|");
                }
                Console.ReadLine();
                for (int curRow = 0; curRow < dt.Rows.Count; curRow++)
                {
                    for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                    {
                        Console.Write(dt.Rows[curRow][curCol].ToString().Trim() + "|");
                    }
                    Console.WriteLine();
                }
            }
        }
    }



    public enum ImportMode
    {
        [Description("Insert")]
        Insert,
        [Description("Insert-Update")]
        InsertUpdate,
        [Description("Insert-Truncate")]
        InsertTruncate,
        [Description("Update")]
        Update,
        [Description("Delete")]
        Delete
    }

    /// <summary>
    /// Export options
    /// </summary>
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
        /// Gets the API URL to post to
        /// </summary>
        /// <param name="importMode">The impost Mode option</param>
        /// <returns>The API URL as string</returns>
        public static string GetAPIPostURL(ImportMode importMode)
        {
            return string.Format("{0}?Login={1}&EncryptedPassword={2}&Import={3}", VOLUSION_BASE_API, VOLUSION_USER, VOLUSION_PASSWORD, GetEnumDescription(importMode));
        }

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

        /// <summary>
        /// Posts a string to a URL
        /// </summary>
        /// <param name="APIURL">The URL</param>
        /// <returns>True on success or false on error</returns>
        public string SendXMLToURL(string APIURL, string postData)
        {
            string response = string.Empty;
            try
            {
                HttpWebRequest APIRequest = (HttpWebRequest)WebRequest.Create(APIURL);
                APIRequest.Method = "POST";
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(postData);
                APIRequest.ContentType = "application/x-www-form-urlencoded";
                APIRequest.ContentLength = data.Length;

                using (Stream APIStream = APIRequest.GetRequestStream())
                {
                    APIStream.Write(data, 0, data.Length);
                }
                //Read the response
                HttpWebResponse APIResponse = (HttpWebResponse)APIRequest.GetResponse();
                Stream receiveStream = APIResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);
                Char[] read = new Char[256];

                int count = readStream.Read(read, 0, 256);
                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    String str = new String(read, 0, count);
                    response += str;
                    count = readStream.Read(read, 0, 256);
                }
                APIResponse.Close();
                readStream.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error posting to API URL", e);
            }

            return response;
        }
    }

}
