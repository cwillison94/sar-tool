/* Copyright (C) 2017 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace sar.Tools
{
    /// <summary>
    /// Enum representing all the different types of objects codes returned by MSSQL
    /// </summary>
	public enum SqlObjectType
	{
		FN,	// SQL scalar function
		IF,	// SQL inline table-valued function
		TF,	// SQL table-valued-function
		P,	// SQL Stored Procedure
		TR,	// SQL DML trigger
		U,	// Table (user-defined)
		TT,	// Table type
		V	// View
	}
	
    /// <summary>
    /// This class can be used to list  and managed MSSQL (Microsoft SQL Server) objects. 
    /// This module offers the ability to easily create table create as well as table insert scripts. 
    /// 
    /// This module's intended use is for services to be able easily install, create and manage there own embedded SQL procedures
    /// and insert scripts.
    /// </summary>
	public class DatabaseObject
	{
        /// <summary>
        /// Sql Command timeout in seconds. This is equivalent to 10 mins.
        /// </summary>
        private const int COMMAND_TIMEOUT = 600;

		#region static
		
        /// <summary>
        /// Gets all the objects in a given database. Note that the SQL script this method is running is embedded within the project.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <returns>List of Database Objects</returns>
		public static List<DatabaseObject> GetDatabaseObjects(SqlConnection connection)
		{
			var result = new List<DatabaseObject>();

            // Get the SQL from the embedded file in the project.
			var sql = Encoding.ASCII.GetString(EmbeddedResource.Get(@"sar.Databases.MSSQL.GetObjects.sql"));
			using (var command = new SqlCommand(sql, connection))
			{
				command.CommandTimeout = 600; 	// 10 minutes
				
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string name = reader.GetString(0);

                        // convert the database string to an enum
						var type = (SqlObjectType)Enum.Parse(typeof(SqlObjectType), reader.GetString(1));

						result.Add(new DatabaseObject(name, type));
					}
				}
			}
			
			return result;
		}
		
        /// <summary>
        /// Returns a database object given its name. If the object is not found in the particular database, null is returned
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="name">Name of the desired database object</param>
        /// <returns>DatabaseObject</returns>
		public static DatabaseObject GetDatabaseObject(SqlConnection connection, string name)
		{
			var result = new List<DatabaseObject>();
			
			using (var command = new SqlCommand(@"SELECT name, xtype FROM sysobjects WHERE name = " + name.QuoteSingle(), connection))
			{
                command.CommandTimeout = COMMAND_TIMEOUT;
				
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
                        // convert the database string to an enum
						var type = (SqlObjectType)Enum.Parse(typeof(SqlObjectType), reader.GetString(1));
						return new DatabaseObject(reader.GetString(0), type);
					}
					else
					{
						return null;
						//throw new MissingMemberException("object " + name + " not found");
					}
				}
			}
		}
		
        /// <summary>
        /// Returns all the columns in a given SQL table.
        /// </summary>
        /// <param name="connection">Datavase Connection</param>
        /// <param name="table">Name of table</param>
        /// <returns>List of column names</returns>
		public static List<string> GetColumnNames(SqlConnection connection, string table)
		{
			var result = new List<string>();
			
			using (var command = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N" + table.QuoteSingle(), connection))
			{
                command.CommandTimeout = COMMAND_TIMEOUT;
				
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetString(0));
					}
				}
			}
			
			return result;
		}
		
		#endregion
		
		#region members
		
		private string name;
		private SqlObjectType type;
		
		#endregion
		
		#region properties
		
		public string Name
		{
			get { return this.name; }
		}
		
        /// <summary>
        /// Return a readable name of the SQL object based of type code
        /// </summary>
		public string Type
		{
			get
			{
				/*
					FN,	// SQL scalar function
					IF,	// SQL inline table-valued function
					TF,	// SQL table-valued-function
					P,	// SQL Stored Procedure
					TR,	// SQL DML trigger
					U,	// Table (user-defined)
					TT,	// Table type
					V	// View
				 */
				
				
				switch (this.type)
				{
					case SqlObjectType.FN:
					case SqlObjectType.IF:
					case SqlObjectType.TF:
						return "Function";
					case SqlObjectType.P:
						return "StoredProcedure";
					case SqlObjectType.TR:
						return "Trigger";
					case SqlObjectType.U:
						return "Table";
					case SqlObjectType.TT:
						return "UserDefinedTableType";
					case SqlObjectType.V:
						return "View";
				}
				return this.name;
			}
		}

		#endregion

		
		private DatabaseObject(string name, SqlObjectType type)
		{
			this.name = name;
			this.type = type;
		}
		
        /// <summary>
        /// Builds a create table script which has the ability to either create (non-existant) table or alter an
        /// existing table to added the missing information.
        /// </summary>
        /// <param name="connection">Database Connection</param>
        /// <returns>Generate create script</returns>
		public string GetCreateScript(SqlConnection connection)
		{
			string result = "";
			
			switch (this.type)
			{
				case SqlObjectType.TT:
                    // get the embedded sql code
					var createTableTypeScript = Encoding.ASCII.GetString(EmbeddedResource.Get(@"sar.Databases.MSSQL.CreateTableType.sql"));
					createTableTypeScript = createTableTypeScript.Replace(@"%%TableTypeName%%", this.name);
					
					using (var command = new SqlCommand(createTableTypeScript, connection))
					{
                        command.CommandTimeout = COMMAND_TIMEOUT;
						
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								result += reader.GetString(0) + Environment.NewLine;
							}
						}
					}
					
					return result.TrimWhiteSpace();
					
				case SqlObjectType.U:
                    // get the embedded sql code
					var createTableScript = EmbeddedResource.Get(@"sar.Databases.MSSQL.CreateTable.sql");
					var script = Encoding.ASCII.GetString(createTableScript);
					script = script.Replace(@"%%TableName%%", this.name);
					
					using (var command = new SqlCommand(script, connection))
					{
						command.CommandTimeout = COMMAND_TIMEOUT;
						
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								result += reader.GetString(0) + Environment.NewLine;
							}
						}
					}
					
					return result.TrimWhiteSpace();

				default:

                    // if the type of the object is not a user defined table or a table type then use 
                    // sq_helptext to return the definition of the object
                    // msdn: https://msdn.microsoft.com/en-us/library/ms176112.aspx
					using (var command = new SqlCommand("sp_helptext " + this.name.QuoteSingle(), connection))
					{
						command.CommandTimeout = COMMAND_TIMEOUT;
						
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								result += reader.GetString(0);
							}
						}
					}
					
					return result.TrimWhiteSpace();
			}
		}
		
        /// <summary>
        /// Create a SQL script of insert statements which replicates the data of a table object or table type.
        /// The purpose is to regenerate data or populate data automated way
        /// </summary>
        /// <param name="connection">Database Object</param>
        /// <returns>Generate insert script</returns>
		public string GetInsertScript(SqlConnection connection)
		{
			string result = "";
			
			switch (this.type)
			{
				case SqlObjectType.TT:
				case SqlObjectType.U:
					var sproc = Encoding.ASCII.GetString(EmbeddedResource.Get(@"sar.Databases.MSSQL.GenerateInsert.sql"));
					foreach (var sql in DatabaseHelper.SplitByGO(sproc))
					{
						using (var command = new SqlCommand(sql, connection))
						{
                            command.CommandTimeout = COMMAND_TIMEOUT;
							command.ExecuteNonQuery();
						}
					}
					
                    // set the script parameters
                    // @PrintGeneratedCode = 0      => Use PRINT command instead or SELECT
                    // @GenerateProjectInfo = 0     => Don't add the project comments
					var script = "";
					script += "EXECUTE ";
					script += " dbo.GenerateInsert @ObjectName = N'" + this.name + "'";
					script += " ,@PrintGeneratedCode=0";
					script += " ,@GenerateProjectInfo=0";

					
					result = @"IF NOT EXISTS (SELECT TOP 1 * FROM [" + this.name + "])" + Environment.NewLine;
					
					using (var command = new SqlCommand(script, connection))
					{
                        command.CommandTimeout = COMMAND_TIMEOUT;
						
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								result += reader.GetString(0) + Environment.NewLine;
							}
						}
					}
					
					// Drop the stored procedure the database
					using (var command = new SqlCommand(@"DROP PROCEDURE dbo.GenerateInsert;", connection))
					{
						command.CommandTimeout = COMMAND_TIMEOUT;
						command.ExecuteNonQuery();
					}
					
					return result.TrimWhiteSpace();

				default:
					throw new ApplicationException("object is not a table");
			}
		}
	}
}
