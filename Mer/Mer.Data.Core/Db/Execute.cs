
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Mer.Data.Core.Db
{
    public static class Execute
    {

        public static ProcessResult LoginTest(OracleConnectionStringBuilder connectionString)
        {
            OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);           
            ProcessResult result = new ProcessResult();
            try
            {
                oracleConnection.Open();
                result.Success = true;
                result.Message = "Connection Successfull";
                oracleConnection.Close();
            }
            catch (OracleException ex)
            {
                result.Success=false;
                result.Message =ex.Message;
            }

            return result;

        }

        public static IEnumerable<T> Select<T>(string query, string where, string orderBy, OracleConnectionStringBuilder connectionString)
        {
            bool hasOrderBy = string.IsNullOrEmpty(orderBy) ? false : true;
            bool hasWhere = string.IsNullOrEmpty(where) ? false : true;
            string selectQuery = String.Format("{0} {1} {2}", query, (hasWhere ? "WHERE " + where : "") , (hasOrderBy ? "ORDER BY " + orderBy : ""));
            OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
            oracleConnection.Open();
            IEnumerable<T> result = oracleConnection.Query<T>(selectQuery);
            oracleConnection.Close();

            return result;
        }

        public static ProcessResult Procedure(DbProcedure dbProcedure, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            try
            {
                
                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = dbProcedure.ProcedureName;
                oracleCommand.CommandType = CommandType.StoredProcedure;

                oracleCommand.Parameters.Clear();
                OracleParameter[] oracleParameters = new OracleParameter[dbProcedure.Parameters.Count];
                int counter = 0;
                foreach (DbParameters parameter in dbProcedure.Parameters)
                {
                    oracleParameters[counter] = new OracleParameter(parameter.ParameterName,(OracleDbType)(int)parameter.ParameterDataType,parameter.ParameterValue, (ParameterDirection)(int)parameter.ParameterDirection );
                    counter++;
                }
                oracleCommand.Parameters.AddRange(oracleParameters);
                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true};


            }
            catch (OracleException ex)
            {
                result = new ProcessResult { Success = false, Message = ex.Message };
            }

            return result;
        }

        public static ProcessResult Procedure(List<DbProcedure> dbProcedureList, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result;
            OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
            oracleConnection.Open();
            using (OracleTransaction transaction = oracleConnection.BeginTransaction())
            {
                try
                {


                    foreach (DbProcedure dbProcedure in dbProcedureList)
                    {
                        OracleCommand oracleCommand = new OracleCommand();
                        oracleCommand.Connection = oracleConnection;
                        oracleCommand.CommandText = dbProcedure.ProcedureName;
                        oracleCommand.CommandType = CommandType.StoredProcedure;

                        oracleCommand.Parameters.Clear();
                        OracleParameter[] oracleParameters = new OracleParameter[dbProcedure.Parameters.Count];
                        int counter = 0;
                        foreach (DbParameters parameter in dbProcedure.Parameters)
                        {
                            oracleParameters[counter] = new OracleParameter(parameter.ParameterName, (OracleDbType)(int)parameter.ParameterDataType, parameter.ParameterValue, (ParameterDirection)(int)parameter.ParameterDirection);
                            counter++;
                        }
                        oracleCommand.Parameters.AddRange(oracleParameters);
                        oracleCommand.ExecuteNonQuery();
                        oracleConnection.Dispose();
                    }
                    transaction.Commit(); 
                    
                    result = new ProcessResult { Success = true };

                }
                catch (OracleException ex)
                {
                    transaction.Rollback();
                    result = new ProcessResult { Success = false, Message = ex.Message };
                }
                oracleConnection.Close();

            }


            return result;
        }

        public static ProcessResult Insert(string tableName, List<DbParameters> parameters, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterNames = "";
            string parameterValues = "";
            string insertQry = "";
            try
            {
                foreach (DbParameters parameter in parameters)
                {
                    parameterNames += parameter.ParameterName + ",";
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }
                }

                if (parameterNames.EndsWith(','))
                {
                    parameterNames = parameterNames.Substring(0, parameterNames.Length - 1);
                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                insertQry = String.Format(@"INSERT INTO {0} ( {1} ) VALUES( {2} )", tableName, parameterNames, parameterValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;


                
                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };


            }
            catch (OracleException ex)
            {
                
                result = new ProcessResult { Success = false, Message = ex.Message, Query = insertQry };
            }

            return result;
        }

        public static ProcessResult Insert<T>(T className, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterNames = "";
            string parameterValues = "";
            string insertQry = "";
            try
            {
                string tableName = "";
                List<DbParameters> parameters = Helper.CreateParameterFromClass(className, ParameterDirections.In);
                DbAttribute tableAttr = className.GetType().GetCustomAttribute<DbAttribute>();
                if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.DbTableName))
                {
                    tableName = tableAttr.DbTableName;
                }
                foreach (DbParameters parameter in parameters)
                {
                    parameterNames += parameter.ParameterName + ",";
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }
                }

                if (parameterNames.EndsWith(','))
                {
                    parameterNames = parameterNames.Substring(0, parameterNames.Length - 1);
                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                insertQry = String.Format(@"INSERT INTO {0} ( {1} ) VALUES( {2} )", tableName, parameterNames, parameterValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;



                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };


            }
            catch (OracleException ex)
            {

                result = new ProcessResult { Success = false, Message = ex.Message, Query = insertQry };
            }

            return result;
        }

        public static ProcessResult Update(string tableName, List<DbParameters> parameters, List<DbParameters> conditions, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterValues = "";
            string conditionValues = "";
            string updateQry = "";
            try
            {

                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterName + " = " + parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += parameter.ParameterName + " = " + "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }

                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                if (conditions != null && conditions.Count > 0)
                {
                    conditionValues = " WHERE ";
                }

                foreach (DbParameters condition in conditions)
                {
                    if (condition.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Number)
                    {
                        conditionValues += condition.ParameterName + " = " + condition.ParameterValue + " AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue.ToString().ToUpper() + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Date)
                    {
                        conditionValues += condition.ParameterName + " = " + "to_date('" + condition.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS') AND ";
                    }

                }

                if (conditionValues.EndsWith("AND "))
                {
                    conditionValues = conditionValues.Substring(0, conditionValues.Length - 4);
                }

                updateQry = String.Format(@"UPDATE {0} SET {1} {2}", tableName, parameterValues, conditionValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = updateQry;
                oracleCommand.CommandType = CommandType.Text;



                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };
            }
            catch (OracleException ex)
            {

                result = new ProcessResult { Success = false, Message = ex.Message, Query = updateQry };
            }

            return result;
        }



        public static ProcessResult Update<T>(T className, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterValues = "";
            string conditionValues = "";
            string updateQry = "";
            try
            {

                string tableName = "";
                List<DbParameters> parameters = Helper.CreateParameterFromClass(className, ParameterDirections.In);
                List<DbParameters> conditions = Helper.CreateConditionFromClass(className);
                DbAttribute tableAttr = className.GetType().GetCustomAttribute<DbAttribute>();
                if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.DbTableName))
                {
                    tableName = tableAttr.DbTableName;
                }
                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterName + " = " + parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += parameter.ParameterName + " = " + "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }

                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                if (conditions != null && conditions.Count > 0)
                {
                    conditionValues = " WHERE ";
                }

                foreach (DbParameters condition in conditions)
                {
                    if (condition.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Number)
                    {
                        conditionValues += condition.ParameterName + " = " + condition.ParameterValue + " AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue.ToString().ToUpper() + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Date)
                    {
                        conditionValues += condition.ParameterName + " = " + "to_date('" + condition.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS') AND ";
                    }

                }

                if (conditionValues.EndsWith("AND "))
                {
                    conditionValues = conditionValues.Substring(0, conditionValues.Length - 4);
                }

                updateQry = String.Format(@"UPDATE {0} SET {1} {2}", tableName, parameterValues, conditionValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = updateQry;
                oracleCommand.CommandType = CommandType.Text;



                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };
            }
            catch (OracleException ex)
            {

                result = new ProcessResult { Success = false, Message = ex.Message, Query = updateQry };
            }

            return result;
        }

        public static ProcessResult UpdateNotNull<T>(T className, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterValues = "";
            string conditionValues = "";
            string updateQry = "";
            try
            {

                string tableName = "";
                List<DbParameters> parameters = Helper.CreateParameterFromClassNotNull(className, ParameterDirections.In);
                List<DbParameters> conditions = Helper.CreateConditionFromClass(className);
                DbAttribute tableAttr = className.GetType().GetCustomAttribute<DbAttribute>();
                if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.DbTableName))
                {
                    tableName = tableAttr.DbTableName;
                }
                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterName + " = " + parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += parameter.ParameterName + " = " + "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += parameter.ParameterName + " = " + "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }

                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                if (conditions != null && conditions.Count > 0)
                {
                    conditionValues = " WHERE ";
                }

                foreach (DbParameters condition in conditions)
                {
                    if (condition.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Number)
                    {
                        conditionValues += condition.ParameterName + " = " + condition.ParameterValue + " AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        conditionValues += condition.ParameterName + " = " + "'" + condition.ParameterValue.ToString().ToUpper() + "' AND ";
                    }
                    else if (condition.ParameterDataType == ParameterDataTypes.Date)
                    {
                        conditionValues += condition.ParameterName + " = " + "to_date('" + condition.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS') AND ";
                    }

                }

                if (conditionValues.EndsWith("AND "))
                {
                    conditionValues = conditionValues.Substring(0, conditionValues.Length - 4);
                }

                updateQry = String.Format(@"UPDATE {0} SET {1} {2}", tableName, parameterValues, conditionValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = updateQry;
                oracleCommand.CommandType = CommandType.Text;



                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };
            }
            catch (OracleException ex)
            {

                result = new ProcessResult { Success = false, Message = ex.Message, Query = updateQry };
            }

            return result;
        }

        public static ProcessResult Delete(string tableName, List<DbParameters> parameters, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterValues = "";
            try
            {
                
                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }

                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                string insertQry = string.Format(@"DELETE FROM {0} WHERE {1}", tableName, parameterValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;

                oracleCommand.Parameters.Clear();
                oracleCommand.Parameters.Add("TABLE_NAME", tableName);
                oracleCommand.Parameters.Add("PARAMETER_VALUES", parameterValues);

                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };


            }
            catch (OracleException ex)
            {
                result = new ProcessResult { Success = false, Message = ex.Message };
            }

            return result;
        }

        public static ProcessResult Delete<T>(T className, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterValues = "";
            try
            {

                string tableName = "";
                List<DbParameters> parameters = Helper.CreateConditionFromClass(className);
                DbAttribute tableAttr = className.GetType().GetCustomAttribute<DbAttribute>();
                if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.DbTableName))
                {
                    tableName = tableAttr.DbTableName;
                }
                
                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ",";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Bool)
                    {
                        parameterValues += "'" + parameter.ParameterValue.ToString().ToUpper() + "',";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "to_date('" + parameter.ParameterValue + "', 'dd/mm/yyyy HH24:MI:SS'),";
                    }

                }

                if (parameterValues.EndsWith(','))
                {
                    parameterValues = parameterValues.Substring(0, parameterValues.Length - 1);
                }

                string insertQry = string.Format(@"DELETE FROM {0} WHERE {1}", tableName, parameterValues);

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;

                oracleCommand.Parameters.Clear();
                oracleCommand.Parameters.Add("TABLE_NAME", tableName);
                oracleCommand.Parameters.Add("PARAMETER_VALUES", parameterValues);

                oracleConnection.Open();
                oracleCommand.ExecuteNonQuery();
                oracleConnection.Close();
                oracleConnection.Dispose();

                result = new ProcessResult { Success = true };


            }
            catch (OracleException ex)
            {
                result = new ProcessResult { Success = false, Message = ex.Message };
            }

            return result;
        }

    }
}
