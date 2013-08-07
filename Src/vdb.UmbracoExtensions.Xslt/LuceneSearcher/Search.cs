using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Lucene.Net.QueryParsers;
using umbraco.BusinessLogic;

namespace vdb.UmbracoExtensions.Xslt
{
    public class Search
    {
        /// <summary>
        /// Bypasses the Examine stuff to execute a lucene query directly on the given index.
        /// </summary>
        /// <param name="indexer">The name of the indexer to search on</param>
        /// <param name="luceneQuery">normal lucene query</param>
        /// <param name="maxResults">max results to return</param>
        /// <param name="highlightOpenTag">Opening tag for highlighted search terms in the result</param>
        /// <param name="highlightCloseTag">Closing tag for highlighted search terms in the result</param>
        /// <param name="fieldsToHighlightCsv">Comma-separated names of fields to apply highlighting to</param>
        /// <returns>search results on success or an error element on error</returns>
        private static XPathNodeIterator DoLuceneSearch(string indexer, string luceneQuery, int maxResults, string highlightOpenTag, string highlightCloseTag, string fieldsToHighlightCsv)
        {
            using(var searcher = new LuceneSearcher(indexer))
            {
                try
                {
                    string[] fieldsToHighlight = String.IsNullOrEmpty(fieldsToHighlightCsv)
                                                     ? new string[0]
                                                     : fieldsToHighlightCsv.Split(new[] {','},
                                                                                  StringSplitOptions.RemoveEmptyEntries);
                    IEnumerable<SearchResult> results = searcher.Search(luceneQuery, maxResults, highlightOpenTag, highlightCloseTag, fieldsToHighlight);
                    return GetResultsAsXml(results);
                }
                catch (ParseException)
                {
                    return new XDocument(new XElement("error", "Invalid lucene query")).CreateNavigator().Select("/*");
                }
            }
        }

        #region LuceneSearch overloads 
        //Can't have default or params parameters for xslt extensions, every overload needs to have a different number of parameters regardless of parameter type

        public static XPathNodeIterator LuceneSearchFull(string indexer, string luceneQuery)
        {
            return LuceneSearch(indexer, luceneQuery, 999);
        }
        
        public static XPathNodeIterator LuceneSearch(string indexer, string luceneQuery, int maxResults)
        {
            return DoLuceneSearch(indexer, luceneQuery, maxResults, null, null, null);
        }

        public static XPathNodeIterator LuceneSearchFull(string indexer, string luceneQuery, string highlightOpenTag, string highlightCloseTag, string fieldsToHighlightCsv)
        {
            return DoLuceneSearch(indexer, luceneQuery, 999, highlightOpenTag, highlightCloseTag, fieldsToHighlightCsv);
        }

        public static XPathNodeIterator LuceneSearch(string indexer, string luceneQuery, int maxResults, string highlightOpenTag, string highlightCloseTag, string fieldsToHighlightCsv)
        {
            return DoLuceneSearch(indexer, luceneQuery, maxResults, highlightOpenTag, highlightCloseTag, fieldsToHighlightCsv);
        }
        #endregion

        /// <summary>
        /// Gets the results as XML.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns>
        /// Returns an XML structure of the search results.
        /// </returns>
        private static XPathNodeIterator GetResultsAsXml(IEnumerable<SearchResult> results)
        {
            var attributes = new List<string> { "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias", "parentID", "loginName", "email" };

            XDocument doc = new XDocument();
            if (results.Count() > 0)
            {
                XElement nodes = new XElement("results");
                foreach (SearchResult result in results)
                {
                    XElement node = new XElement(result.Fields["nodeTypeAlias"]);
                    node.Add(new object[] { new XAttribute("score", result.Score) });

                    foreach (KeyValuePair<string, string> item in result.Fields)
                    {
                        // if the field key starts with '__' (double-underscore) - then skip.
                        if (item.Key.StartsWith("__"))
                        {
                            continue;
                        }
                        // if not legacy schema and 'nodeTypeAlias' - add the @isDoc attribute
                        if (item.Key == "nodeTypeAlias")
                        {
                            node.Add(new object[] { new XAttribute("isDoc", string.Empty) });
                        }
                        // check if the field is an attribute or a data value
                        else if (attributes.Contains(item.Key))
                        {
                            // attribute field
                            node.Add(new object[] { new XAttribute(item.Key, item.Value) });
                        }
                        else
                        {
                            // data field
                            XElement data = new XElement(item.Key);

                            // CDATA the value - because we don't know what it is!
                            data.Add(new object[] { new XCData(item.Value) });

                            // add the data field to the node.
                            node.Add(data);
                        }
                    }

                    // add the node to the collection.
                    nodes.Add(node);
                }

                doc.Add(nodes);
            }
            else
            {
                doc.Add(new XElement("error", "There were no search results."));
            }

            return doc.CreateNavigator().Select("/*");
        }
    }
}