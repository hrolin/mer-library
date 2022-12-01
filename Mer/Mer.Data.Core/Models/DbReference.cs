using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mer.Data.Core.Models
{
	public class DbReference : Attribute
	{
		public string DbReferencedTab { get; set; }
		public string DbReferencedCol { get; set; }
		public string DbDisplayCol { get; set; }
	}
}
