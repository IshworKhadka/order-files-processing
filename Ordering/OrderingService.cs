
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

        private string XMLfilePath = "..\\..\\..\\ordering23.xml";
        private string txtFilePath = "..\\..\\..\\skus.txt";

        private IDictionary<Regex, double> PrimaryAuthValueDictionary = new Dictionary<Regex, double>();

        public void Process()
        {
            //Read from txt File
            string[] skus = File.ReadAllLines(txtFilePath);

            skus = skus.Where(sku => !sku.StartsWith("SHIP")).ToArray();

            //Read from XML File 
            Stream stream = new MemoryStream();
            FileStream sourceStream = new FileStream(XMLfilePath, FileMode.Open, FileAccess.Read);
            sourceStream.CopyTo(stream);
            stream.Position = 0;

            var ordersFile = XElement.Load(stream);

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
                    Regex rejexObj = new Regex(sub);

                    if (PrimaryAuthValueDictionary.ContainsKey(rejexObj))
                    {
                        if(PrimaryAuthValueDictionary.TryGetValue(rejexObj, out double ExistingAuthValue))
                        {
                            if(authorityValue > ExistingAuthValue)
                            {
                                PrimaryAuthValueDictionary[rejexObj] = authorityValue;
                            }

                        }
                    }
                    else
                    {
                        PrimaryAuthValueDictionary.Add(rejexObj, authorityValue);
                    }
                }
            }


            //Matching remaining

            //CSV Exporting remaining

        }

        private double GetAuthorityAttributeValue(XElement element)
        {
            if (element.Attribute("authority") == null)
            {
                return 5.0;
            }
            var authValue = element.Attribute("authority").Value;
            return Convert.ToDouble(authValue);

        }
    }
}
