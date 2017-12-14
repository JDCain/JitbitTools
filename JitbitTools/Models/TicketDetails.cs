
using System;
using System.Collections.Generic;

namespace JitbitTools.Models
{
    public class TicketDetails : ICustomFields
    {
        public Attachment[] Attachments { get; set; }
        public Submitteruserinfo SubmitterUserInfo { get; set; }
        public string CategoryName { get; set; }
        public Assigneeuserinfo AssigneeUserInfo { get; set; }
        public string Url { get; set; }
        public string[] ViewingTechNames { get; set; }
        public object[] Tags { get; set; }
        public string OnBehalfUserName { get; set; }
        public Integrations Integrations { get; set; }
        public int TicketId { get; set; }
        public int IssueId => TicketId;
        public int? UserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? IssueDate { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int? Priority { get; set; }
        public int? StatusId { get; set; }
        public int? CategoryId { get; set; }
        public object DueDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public int? TimeSpentInSeconds { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool UpdatedByUser { get; set; }
        public bool UpdatedByPerformer { get; set; }
        public bool UpdatedForTechView { get; set; }
        public bool IsCurrentUserTechInThisCategory { get; set; }
        public bool SubmittedByCurrentUser { get; set; }
        public string Status { get; set; }
        public bool StatusStopTimeSpent { get; set; }        
        public IEnumerable<CustomField> CustomFields { get; set; }
    }

    public class Submitteruserinfo
    {
        public int? UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool Disabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Notes { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public object CompanyName { get; set; }
        public object DepartmentName { get; set; }
        public object IPAddress { get; set; }
        public object HostName { get; set; }
        public string Lang { get; set; }
        public object UserAgent { get; set; }
        public object AvatarURL { get; set; }
        public object Signature { get; set; }
        public object Greeting { get; set; }
        public object CompanyId { get; set; }
        public object DepartmentID { get; set; }
        public object CompanyNotes { get; set; }
        public bool SendEmail { get; set; }
        public bool IsTech { get; set; }
        public object LastSeen { get; set; }
        public object RecentTickets { get; set; }
        public bool IsManager { get; set; }
        public bool TwoFactorAuthEnabled { get; set; }
        public string FullNameAndLogin { get; set; }
        public string FullName { get; set; }
    }

    public class Assigneeuserinfo
    {
        public int? UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool Disabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Notes { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public object DepartmentName { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Lang { get; set; }
        public string UserAgent { get; set; }
        public object AvatarURL { get; set; }
        public string Signature { get; set; }
        public string Greeting { get; set; }
        public int? CompanyId { get; set; }
        public object DepartmentID { get; set; }
        public object CompanyNotes { get; set; }
        public bool SendEmail { get; set; }
        public bool IsTech { get; set; }
        public DateTime? LastSeen { get; set; }
        public object RecentTickets { get; set; }
        public bool IsManager { get; set; }
        public bool TwoFactorAuthEnabled { get; set; }
        public string FullNameAndLogin { get; set; }
        public string FullName { get; set; }
    }

    public class Integrations
    {
    }

    public class Attachment
    {
        public string FileName { get; set; }
        public int? FileID { get; set; }
        public int? CommentID { get; set; }
        public DateTime? CommentDate { get; set; }
        public object FileHash { get; set; }
        public int? FileSize { get; set; }
        public bool HiddenFromKB { get; set; }
        public bool HiddenFromTickets { get; set; }
        public int? IssueID { get; set; }
        public int? InstanceID { get; set; }
        public object AssetID { get; set; }
        public int? UserID { get; set; }
        public object GoogleDriveUrl { get; set; }
        public object DropboxUrl { get; set; }
        public bool ForTechsOnly { get; set; }
        public string Url { get; set; }
        public string UrlRelative { get; set; }
    }

}
