using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mer.Data.Core.Db
{
    public class Helper
    {
        public static List<DbParameters> CreateParameterFromClass<T>(T className, ParameterDirections  direction)
        {
            List<DbParameters> parameters = new List<DbParameters>();
            
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                DbParameters parameter = new DbParameters();

                DbAttribute attribute = property.GetCustomAttribute<DbAttribute>();
                if (attribute != null && !string.IsNullOrEmpty(attribute.DbColumnName))
                {
                    parameter.ParameterName = attribute.DbColumnName;
                }
                else
                {
                    parameter.ParameterName = property.Name;
                }
                parameter.ParameterDirection = direction;
                parameter.ParameterValue = property.GetValue(className, null);
                if (property.PropertyType == typeof(string))
                {
                    parameter.ParameterDataType = ParameterDataTypes.Varchar2;
                }
                else if(property.PropertyType == typeof(int) || property.PropertyType == typeof(double) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(float))
                {
                    parameter.ParameterDataType = ParameterDataTypes.Number;
                }
                else if(property.PropertyType == typeof(bool))
                {
                    parameter.ParameterDataType = ParameterDataTypes.Bool;
                }
                else if(property.PropertyType == typeof(DateTime))
                {
                    parameter.ParameterDataType = ParameterDataTypes.Date;
                }

                parameters.Add(parameter);
            }
            return parameters;
        }

        public static List<DbParameters> CreateConditionFromClass<T>(T className)
        {
            List<DbParameters> parameters = new List<DbParameters>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                DbParameters parameter = new DbParameters();

                DbAttribute attribute = property.GetCustomAttribute<DbAttribute>();
                if (attribute != null)
                {
                    if (attribute.DbUpdateParameter)
                    {
                        if (!string.IsNullOrEmpty(attribute.DbColumnName))
                        {
                            parameter.ParameterName = attribute.DbColumnName;
                        }
                        else
                        {
                            parameter.ParameterName = property.Name;
                        }

                        parameter.ParameterDirection = ParameterDirections.In;
                        parameter.ParameterValue = property.GetValue(className, null);
                        
                        if (property.PropertyType == typeof(string))
                        {
                            parameter.ParameterDataType = ParameterDataTypes.Varchar2;
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(float))
                        {
                            parameter.ParameterDataType = ParameterDataTypes.Number;
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            parameter.ParameterDataType = ParameterDataTypes.Bool;
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            parameter.ParameterDataType = ParameterDataTypes.Date;
                        }

                        parameters.Add(parameter);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return parameters;
        }

        public static DbParameters CreateParameterFromProperty<T>(T className, PropertyInfo property, ParameterDirections direction)
        {
            DbParameters parameter = new DbParameters();
            DbAttribute attribute = property.GetCustomAttribute<DbAttribute>();
            if (attribute != null && !string.IsNullOrEmpty(attribute.DbColumnName))
            {
                parameter.ParameterName = attribute.DbColumnName;
            }
            else
            {
                parameter.ParameterValue = property.Name;
            }
            parameter.ParameterDirection = direction;
            parameter.ParameterValue = property.GetValue(className, null);
            if (property.PropertyType == typeof(string))
            {
                parameter.ParameterDataType = ParameterDataTypes.Varchar2;
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(float))
            {
                parameter.ParameterDataType = ParameterDataTypes.Number;
            }
            else if (property.PropertyType == typeof(bool))
            {
                parameter.ParameterDataType = ParameterDataTypes.Bool;
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                parameter.ParameterDataType = ParameterDataTypes.Date;
            }

            return parameter;
        }
    }
}
