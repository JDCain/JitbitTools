using System;
using System.Collections.Generic;

namespace JitbitTools.Models
{
    public interface ISharedFields
    {
        int IssueID { get; }
        int TicketId { get; }
        DateTime? IssueDate { get; }
        string Subject { get; }
        IEnumerable<CustomField> CustomFields { get; set; }
    }
}