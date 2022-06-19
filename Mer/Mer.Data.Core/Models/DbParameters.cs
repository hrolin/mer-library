using System;
using System.Collections.Generic;
using System.Text;

namespace Mer.Data.Core.Models
{
    public class DbParameters
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }
        public ParameterDirections ParameterDirection { get; set; }
        public ParameterDataTypes ParameterDataType { get; set; }
    }

    public enum ParameterDirections
    {
        In = 1,
        Out = 2,
        InOut = 3,
        ReturnValue = 6
    }

    public enum ParameterDataTypes
    {
        Varchar2 = 126,
        Number = 108,
        Date = 106,
        Bool = 134,
        Blob = 102
    }
}
