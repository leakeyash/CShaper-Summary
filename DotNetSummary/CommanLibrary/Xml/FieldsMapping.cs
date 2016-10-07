using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CommanLibrary.Xml
{   
    public class FieldsMapping
    {
        readonly XmlDocument _xml=new XmlDocument();
        private readonly bool _loaded = false;
        public FieldsMapping(string xmlPath)
        {
            if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
            {
                _xml.Load(xmlPath);
                _loaded = true;
            }
        }

        private Dictionary<string, string> GetSpecificNodeDictionary(string xPath)
        {
            Dictionary<string,string> selectedValues = new Dictionary<string, string>();
            if (!_loaded || string.IsNullOrEmpty(xPath)) return selectedValues;
            XmlNodeList nodeList = _xml.SelectNodes(xPath);
            if (nodeList == null) return null;
            foreach (XmlNode node in nodeList)
            {
                if (node.Attributes != null)
                {
                    string name = string.Empty;
                    string value = string.Empty;
                    foreach (XmlAttribute xmlAttribute in node.Attributes)
                    {
                        if (xmlAttribute.Name == "name")
                        {
                            name = xmlAttribute.Value;
                        }
                        if (xmlAttribute.Name == "value")
                        {
                            value = xmlAttribute.Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(name) && !selectedValues.ContainsKey(name))
                    {
                        selectedValues.Add(name, value);
                    }
                }
            }
            return selectedValues;
        }  
        public string ReplaceSpecificNodes(string text, string xPath,string symbolStart,string symbolEnd,out Dictionary<string,string> selectedValues,out List<string> unMappingList)
        {          
            unMappingList=new List<string>();
            string replacedString = text;

            selectedValues = GetSpecificNodeDictionary(xPath);

            replacedString = ReplacedString(text, symbolStart, symbolEnd, selectedValues, unMappingList, replacedString);

            return replacedString;
        }

        private static string ReplacedString(string text, string symbolStart, string symbolEnd, Dictionary<string, string> selectedValues,
            List<string> unMappingList, string replacedString)
        {
            int tempStart = text.IndexOf(symbolStart, StringComparison.OrdinalIgnoreCase);
            while (tempStart > 0)
            {
                int tempEnd = text.IndexOf(symbolEnd, tempStart + symbolStart.Length, StringComparison.OrdinalIgnoreCase);
                if (tempEnd > 0)
                {
                    string mapString = text.Substring(tempStart + symbolStart.Length,
                        tempEnd + symbolEnd.Length - tempStart - symbolStart.Length - 1);
                    string wholeString = text.Substring(tempStart, tempEnd + symbolEnd.Length - tempStart);
                    if (selectedValues.ContainsKey(mapString))
                    {
                        replacedString = replacedString.Replace(wholeString, selectedValues[mapString]);
                    }
                    else
                    {
                        unMappingList.Add(wholeString);
                    }
                    tempStart = replacedString.IndexOf(symbolStart, tempEnd + symbolEnd.Length,
                        StringComparison.OrdinalIgnoreCase);
                }
                tempStart = replacedString.IndexOf(symbolStart, tempStart + symbolStart.Length,
                    StringComparison.OrdinalIgnoreCase);
            }
            return replacedString;
        }
    }
}
