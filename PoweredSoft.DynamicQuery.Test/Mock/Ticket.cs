using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.Test.Mock
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string TicketType { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public bool IsHtml { get; set; }
        public string TagList { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string Owner { get; set; }
        public string AssignedTo { get; set; }
        public int TicketStatus { get; set; }
        public DateTimeOffset CurrentStatusDate { get; set; }
        public string CurrentStatusSetBy { get; set; }
        public string LastUpdateBy { get; set; }
        public DateTimeOffset LastUpdateDate { get; set; }
        public string Priority { get; set; }
        public bool AffectedCustomer { get; set; }
        public string Version { get; set; }
        public int ProjectId { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public decimal EstimatedDuration { get; set; }
        public decimal ActualDuration { get; set; }
        public DateTimeOffset TargetDate { get; set; }
        public DateTimeOffset ResolutionDate { get; set; }
        public int Type { get; set; }
        public int ParentId { get; set; }
        public string PreferredLanguage { get; set; }
    }
}
