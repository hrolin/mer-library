using Dapper;
using Mer.Data.Core.Models;
using Oracle.ManagedDataAccess.Client;
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
            string selectQuery = String.Format("{0} {1} {2}", query, (hasWhere ? where : "") , (hasOrderBy ? orderBy : ""));
            OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
            oracleConnection.Open();
            IEnumerable<T> result = oracleConnection.Query<T>(selectQuery);
            oracleConnection.Close();

            return result;
        }

        public static ProcessResult Procedure(string procedure, List<DbParameters> parameters, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            try
            {
                
                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = procedure;
                oracleCommand.CommandType = CommandType.StoredProcedure;

                oracleCommand.Parameters.Clear();
                OracleParameter[] oracleParameters = new OracleParameter[parameters.Count];
                int counter = 0;
                foreach (DbParameters parameter in parameters)
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

        public static ProcessResult Insert(string tableName, List<DbParameters> parameters, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterNames = "";
            string parameterValues = "";
            try
            {
                foreach (DbParameters parameter in parameters)
                {
                    parameterNames += parameter.ParameterName + ";";
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2 || parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "';";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ";";
                    }
                    
                }
                string insertQry = @"INSERT INTO :TABLE_NAME(:PARAMETER_NAMES) VALUES(:PARAMETER_VALUES);";

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;

                oracleCommand.Parameters.Clear();
                oracleCommand.Parameters.Add(":TABLE_NAME", tableName);
                oracleCommand.Parameters.Add(":PARAMETER_NAMES", parameterNames);
                oracleCommand.Parameters.Add(":PARAMETER_VALUES",parameterValues);

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

        public static ProcessResult Delete(string tableName, List<DbParameters> parameters, OracleConnectionStringBuilder connectionString)
        {
            ProcessResult result = new ProcessResult();
            string parameterNames = "";
            string parameterValues = "";
            try
            {
                if (parameters != null && parameters.Count > 0)
                {
                    parameterValues = "WHERE ";
                }
                foreach (DbParameters parameter in parameters)
                {
                    if (parameter.ParameterDataType == ParameterDataTypes.Varchar2 || parameter.ParameterDataType == ParameterDataTypes.Date)
                    {
                        parameterValues += "'" + parameter.ParameterValue + "';";
                    }
                    else if (parameter.ParameterDataType == ParameterDataTypes.Number)
                    {
                        parameterValues += parameter.ParameterValue + ";";
                    }

                }
                string insertQry = @"DELETE FROM :TABLE_NAME WHERE :PARAMETER_VALUES ;";

                OracleConnection oracleConnection = new OracleConnection(connectionString.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oracleConnection;
                oracleCommand.CommandText = insertQry;
                oracleCommand.CommandType = CommandType.Text;

                oracleCommand.Parameters.Clear();
                oracleCommand.Parameters.Add(":TABLE_NAME", tableName);
                oracleCommand.Parameters.Add(":PARAMETER_NAMES", parameterNames);
                oracleCommand.Parameters.Add(":PARAMETER_VALUES", parameterValues);

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
