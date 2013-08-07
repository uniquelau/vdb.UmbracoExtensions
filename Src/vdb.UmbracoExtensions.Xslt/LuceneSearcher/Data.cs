using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using umbraco.BusinessLogic;

namespace vdb.UmbracoExtensions.Xslt
{
  public class Data
  {
    /// <summary>
    /// Generates an XPath navigatable dataset based on sql
    /// </summary>
    /// <param name="sql">The SQL to execute</param>
    /// <returns>A <result></result> element with the results, or an <error></error> element if an exception occured</returns>
    public static XPathNodeIterator GetDataSet(string sql)
    {
      return GetDataSet(sql, "result");
    }

    /// <summary>
    /// Generates an XPath navigatable dataset based on sql
    /// </summary>
    /// <param name="sql">The SQL to execute</param>
    /// <param name="setName">Name of the returned set</param>
    /// <returns>A <setName></setName> element with the results, or an <error></error> element if an exception occured</returns>
    public static XPathNodeIterator GetDataSet(string sql, string setName)
    {
      try
      {
        DataSet ds = GetDataSetFromSql(sql, setName);
        var dataDoc = new XmlDataDocument(ds);
        return dataDoc.CreateNavigator().Select(".");
      }
      catch (Exception e)
      {
        // If there's an exception we'll output an error element instead
        var errorDoc = new XmlDocument();
        errorDoc.LoadXml(String.Format("<error>{0}</error>", HttpUtility.HtmlEncode(e.ToString())));
        return errorDoc.CreateNavigator().Select(".");
      }
    }

    /// <summary>
    /// Gets the dataset from SQL using ADO.NET.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="tableName">Name of the set.</param>
    /// <returns></returns>
    private static DataSet GetDataSetFromSql(string sql, string tableName)
    {
      DataSet result;
      using (var connection = new SqlConnection(Application.SqlHelper.ConnectionString))
      using (var command = new SqlCommand(sql, connection))
      using (var adapter = new SqlDataAdapter(command))
      {
        result = new DataSet(tableName);
        adapter.Fill(result, tableName);
      }
      return result;
    }
  }
}