using System;

namespace JitbitTools.Models
{
    public interface ISharedFields
    {
        int IssueID { get; }
        int TicketId { get; }
        DateTime? IssueDate { get; }
        string Subject { get; }
    }
}