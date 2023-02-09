
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Ordering
{
    public class OrderingService : IOrderingService
    {
        private string XMLfilePath = "ordering23.xml";
        private string txtFilePath = "skus.txt";

        private IList<OrderModel> listOrders = new List<OrderModel>();
        private IDictionary<Regex, double> PrimaryAuthValueDictionary = new Dictionary<Regex, double>();

        public void Process()
        {
            Console.WriteLine("Processing files............\n");
            /// <summary>
            /// Read from text file
            /// </summary>
            string[] skus = File.ReadAllLines(txtFilePath);

            skus = skus.Where(sku => !sku.StartsWith("SHIP")).ToArray();

            /// <summary>
            /// Read from XML file
            /// </summary>
            Stream stream = new MemoryStream();
            FileStream sourceStream = new FileStream(XMLfilePath, FileMode.Open, FileAccess.Read);
            sourceStream.CopyTo(stream);
            stream.Position = 0;

            var ordersFile = XElement.Load(stream);

            //checking for xml elements with primary attributes
            var categoriesWithPrimary = ordersFile.Descendants("category").Where(x => x.Attribute("primary") != null);

            foreach (var category in categoriesWithPrimary)
            {

                var id = category.Attribute("id");
                var name = category.Attribute("name");
                var primary = category.Attribute("primary");

                double authorityValue = GetAuthorityAttributeValue(category);

                if(name != null && name.Value.StartsWith("* "))
                {
                    authorityValue -= 2.5;
                }

                string[] subs = primary.Value.Split(',');

                foreach (var sub in subs)
                {

                    //adjusting authority value for all primary category element in a list
                    Regex rejexSKUObj = new Regex(sub);

                    if (PrimaryAuthValueDictionary.ContainsKey(rejexSKUObj))
                    {
                        if(PrimaryAuthValueDictionary.TryGetValue(rejexSKUObj, out double ExistingAuthValue))
                        {
                            if(authorityValue > ExistingAuthValue)
                            {
                                PrimaryAuthValueDictionary[rejexSKUObj] = authorityValue;
                            }
                        }
                    }
                    else
                    {
                        PrimaryAuthValueDictionary.Add(rejexSKUObj, authorityValue);
                    }


                    listOrders.Add(new OrderModel { authorityValue = authorityValue, Id = Int32.Parse(id.Value), regex = rejexSKUObj });
                }
            }



            //Pattern matching using Regular expressions
            foreach(var sku in skus)
            {
                for (int i = 0; i < listOrders.Count; i++)
                {
                    string pattern = "^" + listOrders[i].regex;

                    Regex rejexObj = new Regex(pattern);

                    if (rejexObj.IsMatch(sku))
                    {
                        listOrders[i].sku = sku;
                    }
                }
            }

            //Load only SKU and ID to CSV model
            List<CSVModel> SKUList = listOrders.Where(o => o.sku != null).Select(o => new CSVModel 
                    { 
                        SKU = o.sku, ID = o.Id

                    }).ToList();


            ExportCSV(SKUList);

        }

        /// <summary>
        /// Export CSV file
        /// </summary>
        private void ExportCSV(List<CSVModel> skuList)
        {
            using (StreamWriter sw = new StreamWriter("export.csv"))
            {
                Console.WriteLine("Exporting CSV...............\n");
                CreateHeader(sw);
                CreateRows(skuList, sw);
            }
        }

        /// <summary>
        /// Get attribute value of XML element 
        /// </summary>
        private double GetAuthorityAttributeValue(XElement element)
        {
            if (element.Attribute("authority") == null)
            {
                return 5.0;
            }
            var authValue = element.Attribute("authority").Value;
            return Convert.ToDouble(authValue);

        }

        /// <summary>
        /// Create CSV header
        /// </summary>
        private static void CreateHeader(StreamWriter sw)
        {
            PropertyInfo[] properties = typeof(CSVModel).GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                sw.Write(properties[i].Name + ",");
            }
            var lastProp = properties[properties.Length - 1].Name;
            sw.Write(lastProp + sw.NewLine);
        }

        /// <summary>
        /// Create CSV rows
        /// </summary>
        private static void CreateRows(List<CSVModel> list, StreamWriter sw)
        {
            foreach (var item in list)
            {
                PropertyInfo[] properties = typeof(CSVModel).GetProperties();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var prop = properties[i];
                    sw.Write(prop.GetValue(item) + ",");
                }
                var lastProp = properties[properties.Length - 1];
                sw.Write(lastProp.GetValue(item) + sw.NewLine);
            }
        }
    }
}
