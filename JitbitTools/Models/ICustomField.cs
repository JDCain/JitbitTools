using System.Collections.Generic;
using System.Text;

namespace JitbitTools.Models
{
    public interface ICustomFields
    {
        IEnumerable<CustomField> CustomFields { get; set; }
    }
}
