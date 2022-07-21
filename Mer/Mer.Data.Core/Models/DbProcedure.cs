using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mer.Data.Core.Models
{
    public class DbProcedure
    {
        public string ProcedureName { get; set; }
        public List<DbParameters> Parameters { get; set; }
    }
}
