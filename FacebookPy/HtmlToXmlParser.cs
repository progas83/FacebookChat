using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FacebookPy
{
    public static class HtmlToXmlParser
    {
        private const string _prePattern = "<{0}.*?\\/>";

        private const string _revisionPattern = @"""revision"":([^,]*),";

        private static int GetRevisionValue(string source)
        {
            string reecceewwe1 = Regex.Match(source, _revisionPattern).Groups[1].Value;

            return 0;
        }

        private static XDocument GetXDocWithElements(string source, string elementName, Func<XElement, bool> condition = null)
        {
            if (condition == null)
            {
                condition = (e) => true;
            }
            XDocument xdoc = new XDocument();
            XElement rootElement = new XElement(string.Format("Root_{0}", elementName));
            string pattern = string.Format(_prePattern, elementName);
            MatchCollection matches = Regex.Matches(source, pattern);
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    try
                    {
                        XElement element = XElement.Parse(m.Value);
                        if (condition(element))
                        {
                            rootElement.Add(element);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            xdoc.Add(rootElement);

            return xdoc;
        }

        internal static NameValueCollection GetParametersForLogin(string textResult)
        {
            XDocument xDoc = GetXDocWithElements(textResult, "input", ElementHasNameAndValueAttributes);
            NameValueCollection resultCollection = ExtractParametersForLogin(xDoc);
            return resultCollection;
        }

        private static NameValueCollection ExtractParametersForLogin(XDocument xDoc)
        {
            NameValueCollection nvc = new NameValueCollection();
            if (xDoc.Root == null)
                return nvc;

            foreach (var elem in xDoc.Descendants("input"))
            {
                var name = elem.Attribute("name").Value;

                string val = string.Empty;

                if (string.IsNullOrEmpty(val))
                {
                    try
                    {
                        val = elem.Attribute("value").Value;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                nvc[name] = val;
            }

            nvc["login"] = "Log In";

            return nvc;
        }

        private static bool ElementHasNameAndValueAttributes(XElement element)
        {
            bool condition = false;

            if (element.HasAttributes)
            {
                condition = element.Attribute("name") != null;
            }

            return condition;
        }

        private static bool ElementWithSpecialAttribute(XElement element)
        {
            bool condition = false;

            if (element.HasAttributes)
            {
                condition = element.Attribute("name").Value == "fb_dtsg";
            }

            return condition;
        }

        internal static NameValueCollection GetFbLogoutValues(string html)
        {
            NameValueCollection nvc = new NameValueCollection();
            XDocument xDoc = GetXDocWithElements(html, "input");

            foreach (XElement element in xDoc.Descendants("input"))
            {
                switch (element.Attribute("name").Value)
                {
                    case "fb_dtsg":
                        nvc["fb_dtsg"] = element.Attribute("value").Value;
                        break;

                    case "ref":
                        nvc["ref"] = element.Attribute("value").Value;
                        break;

                    case "h":
                        nvc["h"] = element.Attribute("value").Value;
                        break;

                    default:
                        break;
                }
            }
            return nvc;
        }

        internal static string GetFbDtsgValue(string readText)
        {
            XDocument xDoc = GetXDocWithElements(readText, "input", ElementWithSpecialAttribute);
            string result = xDoc.Descendants("input").FirstOrDefault().Attribute("value").Value;
            return result;
        }
    }
}