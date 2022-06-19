using System;
using System.Collections.Generic;
using System.Text;

namespace Mer.Data.Core.Models
{
    public class ProcessResult
    {
        public object Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
