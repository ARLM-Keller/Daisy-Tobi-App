﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AudioLib;
using DtdSharp;
using urakawa.core;

namespace Tobi.Plugin.Validator.ContentDocument
{
    /// <summary>
    /// Transform a DtdSharp DTD object into a table of regular expressions
    /// Also provides functions to store and retrive a cached version of the table
    /// </summary>
    public class DtdSharpToRegex
    {
        public Hashtable DtdRegexTable { get; private set; }

        public Regex GetRegex(StringBuilder strBuilder, TreeNode node)
        {
            if (DtdRegexTable == null)
            {
                return null;
            }

            strBuilder.Clear();
            buildPrefixedQualifiedName(strBuilder, node);

            object regexObj = DtdRegexTable[strBuilder.ToString()];

            if (regexObj == null)
            {
                return null;
            }

            return (Regex)regexObj;
        }

        public void Reset()
        {
            DtdRegexTable = null;
        }

        //take a DtdSharp data structure and create a hashmap where 
        //key: element name
        //value: regex representing the allowed children
        public void ParseDtdIntoHashtable(DTD dtd)
        {
            if (DtdRegexTable == null)
            {
                DtdRegexTable = new Hashtable();
            }
            foreach (DictionaryEntry entry in dtd.Elements)
            {
                DTDElement dtdElement = (DTDElement)entry.Value;
                string regexStr = GenerateRegexForAllowedChildren(dtdElement.Content);
                Regex regex = new Regex(regexStr);
                DtdRegexTable.Add(dtdElement.Name, regex);
            }
        }

        /// <summary>
        ///  Read from cache into a hashtable
        /// 
        /// Cache file format:
        /// line 1: Element name
        /// line 2: Regex string of element's allowed children
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public void ReadFromCache(StreamReader reader)
        {
            if (DtdRegexTable == null)
            {
                DtdRegexTable = new Hashtable();
            }
            try
            {
                string name = reader.ReadLine();
                string regExpStr = reader.ReadLine();

                while (name != null && regExpStr != null)
                {
                    Regex regEx = new Regex(regExpStr);
                    DtdRegexTable[name] = regEx;
                    name = reader.ReadLine();
                    regExpStr = reader.ReadLine();
                }
            }
            catch
            {
                //DebugFix.Assert(false);
#if DEBUG
                Debugger.Break();
#endif // DEBUG
                DtdRegexTable = null;
            }
        }

        /// <summary>
        /// Write hashtable data to cache
        /// 
        /// The hashtable is formatted like:
        /// key: element name
        /// value: regex (object) of element's allowed children
        /// 
        /// Cache file format:
        /// line 1: Element name
        /// line 2: Regex string of element's allowed children
        /// </summary>
        /// <param name="writer"></param>
        public void WriteToCache(StreamWriter writer)
        {
            if (DtdRegexTable == null)
            {
                return;
            }

            foreach (DictionaryEntry entry in DtdRegexTable)
            {
                string name = (string)entry.Key;
                string regExpStr = ((Regex)entry.Value).ToString();
                writer.WriteLine(name);
                writer.WriteLine(regExpStr);
            }
        }

        private static void buildPrefixedQualifiedName(StringBuilder strBuilder, TreeNode n)
        {
            if (n.NeedsXmlNamespacePrefix())
            {
                string nsUri = n.GetXmlNamespaceUri();
                string prefix = n.GetXmlNamespacePrefix(nsUri);

                strBuilder.Append(prefix);
                strBuilder.Append(':');
            }

            strBuilder.Append(n.GetXmlElementLocalName());
        }

        //return a string list of the child node names
        //so that they are compatible with the regular expression 
        //created from the DTD
        public static string GenerateChildNameList(StringBuilder strBuilder, TreeNode node)
        {
            strBuilder.Clear();

            if (node.GetTextMedia() != null)
            {
                strBuilder.Append("#PCDATA");
            }
            foreach (TreeNode child in node.Children.ContentsAs_Enumerable)
            {
                if (child.HasXmlProperty)
                {
                    buildPrefixedQualifiedName(strBuilder, child);
                    strBuilder.Append("#");
                }
            }

            return strBuilder.ToString();
        }

        private static string GenerateRegexForAllowedChildren(DTDItem dtdItem)
        {
            string regexStr = "";

            if (dtdItem is DTDAny)
            {
                regexStr += "";// "Any";
            }
            else if (dtdItem is DTDEmpty)
            {
                regexStr += "";
            }
            else if (dtdItem is DTDName)
            {
                regexStr += "(?:" + ((DTDName)dtdItem).Value + "#)";
            }
            else if (dtdItem is DTDChoice)
            {
                List<DTDItem> items = ((DTDChoice)dtdItem).Items;
                if (items.Count > 1) regexStr += "(?:";

                bool isFirst = true;
                foreach (DTDItem item in items)
                {
                    if (!isFirst) regexStr += "|";
                    isFirst = false;
                    regexStr += GenerateRegexForAllowedChildren(item);
                }
                if (items.Count > 1) regexStr += ")";
            }
            else if (dtdItem is DTDSequence)
            {
                List<DTDItem> items = ((DTDSequence)dtdItem).Items;
                if (items.Count > 1) regexStr += "(?:";

                bool isFirst = true;
                foreach (DTDItem item in items)
                {
                    if (!isFirst) regexStr += "";
                    regexStr += GenerateRegexForAllowedChildren(item);
                    isFirst = false;
                }
                if (items.Count > 1) regexStr += ")";
            }
            else if (dtdItem is DTDMixed)
            {
                List<DTDItem> items = ((DTDMixed)dtdItem).Items;
                if (items.Count > 1) regexStr += "(?:";

                bool isFirst = true;
                foreach (DTDItem item in items)
                {
                    if (!isFirst) regexStr += "|";
                    regexStr += GenerateRegexForAllowedChildren(item);
                    isFirst = false;
                }
                if (items.Count > 1) regexStr += ")";
            }
            else if (dtdItem is DTDPCData)
            {
                regexStr += "#PCDATA";
            }
            else
            {
                regexStr += "**UNKNOWN**";
            }

            if (dtdItem.Cardinal == DTDCardinal.ZEROONE)
            {
                regexStr += "?";
            }
            else if (dtdItem.Cardinal == DTDCardinal.ZEROMANY)
            {
                regexStr += "*";
            }
            else if (dtdItem.Cardinal == DTDCardinal.ONEMANY)
            {
                regexStr += "+";
            }

            return regexStr;
        }
    }
}
