using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Highlight;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;


namespace vdb.UmbracoExtensions.Xslt
{
		public class LuceneSearcher : IDisposable
		{
				private static readonly Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_29;
				readonly FSDirectory directory;
				readonly IndexReader reader;
				readonly IndexSearcher searcher;
				private readonly string[] searchFields;
			 
				public LuceneSearcher(string searchIndex)
				{
						DirectoryInfo indexDirectory = GetIndexPath(searchIndex);
						directory = FSDirectory.Open(indexDirectory);
						reader = IndexReader.Open(directory, true);
						searcher = new IndexSearcher(reader);	
						searchFields = GetSearchFields(reader);
				}

				public IEnumerable<SearchResult> Search(string luceneQuery, int maxResults = 500, string highlightOpenTag = null, string highlightCloseTag = null, params string[] fieldsToHighlight)
				{
						var results = new List<SearchResult>();
						if (String.IsNullOrWhiteSpace(luceneQuery)) return results;
						 
						var parser = new MultiFieldQueryParser(LuceneVersion, searchFields, new StandardAnalyzer(LuceneVersion));
						Query query = parser.Parse(luceneQuery);
						TopDocs topDocs = searcher.Search(query, maxResults);

						foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
						{
								Document document = reader.Document(scoreDoc.doc);
								var result = new SearchResult(document, scoreDoc.score);
								results.Add(result);
						}

						if(!String.IsNullOrEmpty(highlightOpenTag) && !String.IsNullOrEmpty(highlightCloseTag) && fieldsToHighlight.Length > 0)
						{
								var scorer = new QueryScorer(query);
								var formatter = new SimpleHTMLFormatter(highlightOpenTag, highlightCloseTag);
								var highlighter = new Highlighter(formatter, scorer);
								highlighter.SetTextFragmenter(new SimpleFragmenter());
								foreach (SearchResult result in results)
								{
										foreach (string highlightField in fieldsToHighlight)
										{
												if(!result.Fields.ContainsKey(highlightField)) continue;
												string fieldValue = result[highlightField];
												TokenStream stream = new StandardAnalyzer(LuceneVersion).TokenStream(highlightField, new StringReader(fieldValue));
												string highlightedFieldValue = highlighter.GetBestFragments(stream, fieldValue, 500, "...");
												if (!String.IsNullOrWhiteSpace(highlightedFieldValue))
												{
														result.Fields[highlightField] = highlightedFieldValue;
												}
										}
								}
						}
						return results;
				}

				/// <summary>
				/// Returns a list of fields to search on
				/// </summary>
				/// <returns></returns>
				protected static string[] GetSearchFields(IndexReader reader)
				{
						//exclude the special index fields
						return reader.GetFieldNames(IndexReader.FieldOption.ALL)
								.Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
								.ToArray();
				}

				private static DirectoryInfo GetIndexPath(string indexToQuery)
				{

						string configFilePath = HttpContext.Current.Server.MapPath("/config/ExamineIndex.Config");
						indexToQuery = indexToQuery.Replace("Indexer", "IndexSet");
						XElement configElement = XElement.Load(configFilePath)
								.Elements("IndexSet")
								.Where(c => c.Attribute("SetName").Value == indexToQuery)
								.SingleOrDefault();
						
						if(configElement == null) throw new ApplicationException("Unknown index set " + indexToQuery);

						string indexPath = HttpContext.Current.Server.MapPath(configElement.Attribute("IndexPath").Value +"Index");
						return new DirectoryInfo(indexPath);
				}
				 
				#region IDisposable Members

		public void Dispose()
				{
						if(directory != null)
						{
								directory.Close();
						}
						if(reader != null)
						{
								reader.Close();
						}
						if(searcher != null)
						{
								searcher.Close();
						}
				}

				#endregion
		}
}