//MIT License

//Copyright (c) [2019] [Perspectiva di Formentini Filippo]

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A simple class that generates CRUD sql for DAPPER from POCO
/// Database fields may have the same name as the POCO Objects or they can be mapped using the columnMappings property
/// </summary>
public static class DapperSqlGenerator
{
    /// <summary>
    /// Generates INSERT STATEMENT
    /// </summary>
    /// <param name="objType">Domain Type</param>
    /// <param name="table">The name of the table in the database</param>
    /// <param name="idProperty">The name of the Primary Key</param>
    /// <param name="columnMappings">Key(Property Name)-Value(Field Name) pairs of the database fields that have a different property name.</param>
    /// <returns></returns>
    public static string Insert(Type objType, string table, string idProperty, Dictionary<string, string> columnMappings)
    {
        var dictColumns = DictColumns(objType, columnMappings);
        var sqlCols = string.Join(",", dictColumns.Keys.Where(f => f != idProperty));
         var sqlValues = "@" + string.Join(",@", dictColumns.Where(f => f.Key != idProperty).Select(f=>f.Value));
        return string.Format("insert into {0} ({1}) values ({2});SELECT CAST(SCOPE_IDENTITY() as int)", table, sqlCols, sqlValues);
    }
    /// <summary>
    /// Generates UPDATE STATEMENT
    /// </summary>
    /// <param name="objType">Domain Type</param>
    /// <param name="table">The name of the table in the database</param>
    /// <param name="idProperty">The name of the Primary Key</param>
    /// <param name="columnMappings">Key(Property Name)-Value(Field Name) pairs of the database fields that have a different property name.</param>
    /// <returns></returns>
    public static string Update(Type objType, string table, string idProperty, Dictionary<string, string> columnMappings)
    {
        var dictColumns = DictColumns(objType, columnMappings);
        var sqlCols = new List<string>();
        var where = "";
        foreach (var col in dictColumns)
            if (col.Key == idProperty)
                where = col.Key + "=@" + col.Value;
            else
                sqlCols.Add(col.Key + "=@" + col.Value);
        return string.Format("update {0} set {1} where {2}", table, string.Join(",", sqlCols), where);
    }

    /// <summary>
    /// DELETE STATEMENT
    /// </summary>
    /// <param name="table">The name of the table in the database</param>
    /// <param name="idProperty">The name of the Primary Key</param>
    /// <returns></returns>
    public static string Delete(string table, string idProperty)
    {
        return string.Format("delete from {0} where {1} = @{1}", table, idProperty);
    }

    /// <summary>
    /// SELECT single stamente
    /// </summary>
    /// <param name="id">Primary ID of the entity</param>
    /// <param name="objType">Domain Type</param>
    /// <param name="table">The name of the table in the database</param>
    /// <param name="idProperty">The name of the Primary Key</param>
    /// <param name="columnMappings">Key(Property Name)-Value(Field Name) pairs of the database fields that have a different property name.</param>
    /// <returns></returns>
    public static string SelectById(int id, Type objType, string table, string idProperty, Dictionary<string, string> columnMappings)
    {
        var dictColumns = DictColumns(objType, columnMappings);
        var sqlCols = string.Join(",", dictColumns.Keys);
        //aggiungo i valori del mapping
        foreach (var columnMapping in columnMappings)
        {
            sqlCols += "," + columnMapping.Value + " as " + columnMapping.Key;
        }
        return string.Format("select {0} from {1} where {2} = {3}", sqlCols, table, idProperty, id.ToString());
    }

    /// <summary>
    /// This function generates a dictionary that maps the database fields the the POCO properties 
    /// </summary>
    /// <param name="objType">Domain Type</param>
    /// <param name="columnMappings">Key(Property Name)-Value(Field Name) pairs of the database fields that have a different property name.</param>
    /// <returns></returns>
    private static Dictionary<string, string> DictColumns(Type objType, Dictionary<string, string> columnMappings)
    {
        var cols = (from propertyInfo in objType.GetProperties()
                    where propertyInfo.CanRead &&
                            (propertyInfo.PropertyType == typeof(string) ||
                            !typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                    select propertyInfo.Name).ToList();
        var dictColumns = new Dictionary<string, string>();
        foreach (var col in cols)
            if (columnMappings.Keys.Contains(col))
            {
                var mapping = columnMappings[col];
                dictColumns.Add(mapping, col);
            }
            else
                dictColumns.Add(col, col);

        return dictColumns;
    }
}
