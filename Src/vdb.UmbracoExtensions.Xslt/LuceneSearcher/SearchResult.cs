using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;

namespace vdb.UmbracoExtensions.Xslt
{
    public class SearchResult
    {
        public SearchResult(Document doc, float score)
        {
            Fields = new Dictionary<string, string>();
            string id = doc.Get("id");
            if (string.IsNullOrEmpty(id))
            {
                id = doc.Get("__NodeId");
            }
            Id = int.Parse(id);
            Score = score;
           
            //we can use lucene to find out the fields which have been stored for this particular document
            //I'm not sure if it'll return fields that have null values though
            var fields = doc.GetFields();

            //ignore our internal fields though
            foreach (Field field in fields.Cast<Field>())
            {
                string fieldName = field.Name();
                Fields.Add(fieldName, doc.Get(fieldName));
                //Examine returns some fields as e.g. __FieldName rather than fieldName
                if (fieldName.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                {
                    int offset = LuceneIndexer.SpecialFieldPrefix.Length;
                    string tidiedFieldName = Char.ToLower(fieldName[offset]) + fieldName.Substring(offset + 1);
                    Fields.Add(tidiedFieldName, doc.Get(fieldName));
                }
            }
        }

        public int Id { get; set; }
        public float Score { get; set; }
        public IDictionary<string, string> Fields { get; protected set; }

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] 
        {
            get
            {
                return Fields.ToArray()[resultIndex];
            }
        }

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] 
        {
            get
            {
                return Fields[key];
            }
        }
        
        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var result = (SearchResult)obj;

            return Id.Equals(result.Id);
        }

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
