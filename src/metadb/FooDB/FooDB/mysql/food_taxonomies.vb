﻿#Region "Microsoft.VisualBasic::a00f24a5841efd2e06860159fa04dfbd, mzkit\src\metadb\FooDB\FooDB\mysql\food_taxonomies.vb"

    ' Author:
    ' 
    '       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
    ' 
    ' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
    ' 
    ' 
    ' MIT License
    ' 
    ' 
    ' Permission is hereby granted, free of charge, to any person obtaining a copy
    ' of this software and associated documentation files (the "Software"), to deal
    ' in the Software without restriction, including without limitation the rights
    ' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ' copies of the Software, and to permit persons to whom the Software is
    ' furnished to do so, subject to the following conditions:
    ' 
    ' The above copyright notice and this permission notice shall be included in all
    ' copies or substantial portions of the Software.
    ' 
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    ' SOFTWARE.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 166
    '    Code Lines: 84
    ' Comment Lines: 60
    '   Blank Lines: 22
    '     File Size: 8.78 KB


    ' Class food_taxonomies
    ' 
    '     Properties: classification_name, classification_order, created_at, food_id, id
    '                 ncbi_taxonomy_id, updated_at
    ' 
    '     Function: Clone, GetDeleteSQL, GetDumpInsertValue, (+2 Overloads) GetInsertSQL, (+2 Overloads) GetReplaceSQL
    '               GetUpdateSQL
    ' 
    ' 
    ' /********************************************************************************/

#End Region

REM  Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic.CodeGenerator
REM  MYSQL Schema Mapper
REM      for Microsoft VisualBasic.NET 2.1.0.2569

REM  Dump @2018/5/23 11:01:41


Imports System.Data.Linq.Mapping
Imports System.Xml.Serialization
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports MySqlScript = Oracle.LinuxCompatibility.MySQL.Scripting.Extensions

Namespace TMIC.FooDB.mysql

''' <summary>
''' ```SQL
''' 
''' --
''' 
''' DROP TABLE IF EXISTS `food_taxonomies`;
''' /*!40101 SET @saved_cs_client     = @@character_set_client */;
''' /*!40101 SET character_set_client = utf8 */;
''' CREATE TABLE `food_taxonomies` (
'''   `id` int(11) NOT NULL AUTO_INCREMENT,
'''   `food_id` int(11) DEFAULT NULL,
'''   `ncbi_taxonomy_id` int(11) DEFAULT NULL,
'''   `classification_name` varchar(255) DEFAULT NULL,
'''   `classification_order` int(11) DEFAULT NULL,
'''   `created_at` datetime NOT NULL,
'''   `updated_at` datetime NOT NULL,
'''   PRIMARY KEY (`id`)
''' ) ENGINE=InnoDB AUTO_INCREMENT=920 DEFAULT CHARSET=utf8;
''' /*!40101 SET character_set_client = @saved_cs_client */;
''' 
''' --
''' ```
''' </summary>
''' <remarks></remarks>
<Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes.TableName("food_taxonomies", Database:="foodb", SchemaSQL:="
CREATE TABLE `food_taxonomies` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `food_id` int(11) DEFAULT NULL,
  `ncbi_taxonomy_id` int(11) DEFAULT NULL,
  `classification_name` varchar(255) DEFAULT NULL,
  `classification_order` int(11) DEFAULT NULL,
  `created_at` datetime NOT NULL,
  `updated_at` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=920 DEFAULT CHARSET=utf8;")>
Public Class food_taxonomies: Inherits Oracle.LinuxCompatibility.MySQL.MySQLTable
#Region "Public Property Mapping To Database Fields"
    <DatabaseField("id"), PrimaryKey, AutoIncrement, NotNull, DataType(MySqlDbType.Int64, "11"), Column(Name:="id"), XmlAttribute> Public Property id As Long
    <DatabaseField("food_id"), DataType(MySqlDbType.Int64, "11"), Column(Name:="food_id")> Public Property food_id As Long
    <DatabaseField("ncbi_taxonomy_id"), DataType(MySqlDbType.Int64, "11"), Column(Name:="ncbi_taxonomy_id")> Public Property ncbi_taxonomy_id As Long
    <DatabaseField("classification_name"), DataType(MySqlDbType.VarChar, "255"), Column(Name:="classification_name")> Public Property classification_name As String
    <DatabaseField("classification_order"), DataType(MySqlDbType.Int64, "11"), Column(Name:="classification_order")> Public Property classification_order As Long
    <DatabaseField("created_at"), NotNull, DataType(MySqlDbType.DateTime), Column(Name:="created_at")> Public Property created_at As Date
    <DatabaseField("updated_at"), NotNull, DataType(MySqlDbType.DateTime), Column(Name:="updated_at")> Public Property updated_at As Date
#End Region
#Region "Public SQL Interface"
#Region "Interface SQL"
    Friend Shared ReadOnly INSERT_SQL$ = 
        <SQL>INSERT INTO `food_taxonomies` (`food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');</SQL>

    Friend Shared ReadOnly INSERT_AI_SQL$ = 
        <SQL>INSERT INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly REPLACE_SQL$ = 
        <SQL>REPLACE INTO `food_taxonomies` (`food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');</SQL>

    Friend Shared ReadOnly REPLACE_AI_SQL$ = 
        <SQL>REPLACE INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly DELETE_SQL$ =
        <SQL>DELETE FROM `food_taxonomies` WHERE `id` = '{0}';</SQL>

    Friend Shared ReadOnly UPDATE_SQL$ = 
        <SQL>UPDATE `food_taxonomies` SET `id`='{0}', `food_id`='{1}', `ncbi_taxonomy_id`='{2}', `classification_name`='{3}', `classification_order`='{4}', `created_at`='{5}', `updated_at`='{6}' WHERE `id` = '{7}';</SQL>

#End Region

''' <summary>
''' ```SQL
''' DELETE FROM `food_taxonomies` WHERE `id` = '{0}';
''' ```
''' </summary>
    Public Overrides Function GetDeleteSQL() As String
        Return String.Format(DELETE_SQL, id)
    End Function

''' <summary>
''' ```SQL
''' INSERT INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetInsertSQL() As String
        Return String.Format(INSERT_SQL, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
    End Function

''' <summary>
''' ```SQL
''' INSERT INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetInsertSQL(AI As Boolean) As String
        If AI Then
        Return String.Format(INSERT_AI_SQL, id, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
        Else
        Return String.Format(INSERT_SQL, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
        End If
    End Function

''' <summary>
''' <see cref="GetInsertSQL"/>
''' </summary>
    Public Overrides Function GetDumpInsertValue(AI As Boolean) As String
        If AI Then
            Return $"('{id}', '{food_id}', '{ncbi_taxonomy_id}', '{classification_name}', '{classification_order}', '{created_at}', '{updated_at}')"
        Else
            Return $"('{food_id}', '{ncbi_taxonomy_id}', '{classification_name}', '{classification_order}', '{created_at}', '{updated_at}')"
        End If
    End Function


''' <summary>
''' ```SQL
''' REPLACE INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetReplaceSQL() As String
        Return String.Format(REPLACE_SQL, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
    End Function

''' <summary>
''' ```SQL
''' REPLACE INTO `food_taxonomies` (`id`, `food_id`, `ncbi_taxonomy_id`, `classification_name`, `classification_order`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetReplaceSQL(AI As Boolean) As String
        If AI Then
        Return String.Format(REPLACE_AI_SQL, id, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
        Else
        Return String.Format(REPLACE_SQL, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
        End If
    End Function

''' <summary>
''' ```SQL
''' UPDATE `food_taxonomies` SET `id`='{0}', `food_id`='{1}', `ncbi_taxonomy_id`='{2}', `classification_name`='{3}', `classification_order`='{4}', `created_at`='{5}', `updated_at`='{6}' WHERE `id` = '{7}';
''' ```
''' </summary>
    Public Overrides Function GetUpdateSQL() As String
        Return String.Format(UPDATE_SQL, id, food_id, ncbi_taxonomy_id, classification_name, classification_order, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at), id)
    End Function
#End Region

''' <summary>
                     ''' Memberwise clone of current table Object.
                     ''' </summary>
                     Public Function Clone() As food_taxonomies
                         Return DirectCast(MyClass.MemberwiseClone, food_taxonomies)
                     End Function
End Class


End Namespace
