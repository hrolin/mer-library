using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mer.Data.Core.Models
{
    public class DbAttribute : Attribute
    {
        public string DbTableName { get; set; }
        public string DbColumnName { get; set; }
        public bool DbUpdateParameter { get; set; }
    }
}
