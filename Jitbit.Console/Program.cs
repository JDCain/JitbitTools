using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JitbitTools;
using JitbitTools.Models;
using Microsoft.Extensions.Configuration;
using static System.Console;

namespace JitBit.Console
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        private static string _baseUrl;
        private static Jitbit _jitBit;
        public static Regex Sepid => new Regex(@"(?i)(?<![0-9])[0-9]{5,6}\b(?-i)");
        public static Regex SepidBeginsWithSpace => new Regex(@"(?<=\s)(?<![0-9])[0-9]{5,6}\b");
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Program>();
            Configuration = builder.Build();
            _baseUrl = Configuration["Jitbit_Host"];

            _jitBit = new Jitbit(Encoding.ASCII.GetBytes(Configuration["Jitbit_Cred"]), _baseUrl);
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var attVmTickets = await _jitBit.GetTickets(327428);
            await MergeOpenVoicemails(attVmTickets);
            //var mergeTicketsByUserFunc = new Func<TicketSummary, Task>(async (x) => await MergeTicketsByUser(x));
            //var updateCustomFieldFunc = new Func<Ticket, Task>(async (x) => await UpdateCustomField(x));
            foreach (Catagories catagory in Enum.GetValues(typeof(Catagories)))
            {
                var tickets = await _jitBit.GetTickets(Convert.ToInt32(catagory));
                await CloseShortVoiceMail(tickets, 10);
                tickets = await _jitBit.GetTickets(Convert.ToInt32(catagory));
                await MergeOpenVoicemails(tickets);
            }
            //await LoopTicketsTask(hdTickets, mergeTicketsByUserFunc);
        }

        private static async Task UpdateId(TicketSummary x)
        {
            var field = x.CustomFields.FirstOrDefault(y => y.FieldID == 22860);
            if (string.IsNullOrWhiteSpace(field?.Value))
            {
                //if (x.Subject)
                await _jitBit.WriteCustomField(x, field);
            }
        }

        private static async Task LoopTicketsTask(IEnumerable<TicketSummary> tickets, Func<TicketSummary, Task> func)
        {
            foreach (var ticket in tickets)
            {
                WriteLine($"trying: {ticket.IssueID}");
                await func.Invoke(ticket);
            }
        }

        private static async Task MergeTicketsByUser(TicketSummary ticket)
        {
            var parameters2 = new Dictionary<string, string>
            {
                {"categoryid", ticket.CategoryID.ToString()},
                {"fromUserid", ticket.UserID.ToString()}
            };
            var userTickets = await _jitBit.GetTicketsByParameters(parameters2);
            var filteredTickets = userTickets?.Where(x => x.StatusID < 3).ToList();
            if (filteredTickets?.Count > 1)
            {
                var newList = filteredTickets.OrderBy(x => x.IssueDate).ToList();
                var firsTicket = newList.FirstOrDefault();
                var builtComment = $"Closed the following newer tickets and linked them to current ({firsTicket?.IssueID}).{Environment.NewLine}";

                foreach (var userTicket in newList.Skip(1))
                {
                    builtComment += $"{_baseUrl}/Ticket/{userTicket.IssueID}{Environment.NewLine}";
                    WriteLine($"Comment: {ticket.IssueID}");
                    await _jitBit.Comment(userTicket,
                        $"Closing and linking to oldest open ticket {_baseUrl}/Ticket/{firsTicket?.IssueID}",
                        techOnly: true);
                    WriteLine($"Closing: {ticket.IssueID}");
                    await _jitBit.CloseTicket(ticket);
                }
                WriteLine($"Comment: {ticket.IssueID}");
                await _jitBit.Comment(firsTicket, builtComment, techOnly:true);
            }
        }

        private static async Task CloseShortVoiceMail(IEnumerable<TicketSummary> tickets, int lessThanSeconds)
        {
            foreach (var ticket in tickets)
            {
                var matches = Regex.Matches(ticket.Subject,
                    @"(((\d+):(\d+))|((\d+)\s*m[a-z]*)|(((\d+)\s*h[a-z]*)([^\d]*(\d+)\s*m[a-z]*)?)|(\d+))");
                var caculate = Helpers.MatchCollectonToSeconds(matches);
                if (caculate < lessThanSeconds)
                {
                    WriteLine($"Closing: {ticket.IssueID}");
                    await _jitBit.CloseTicket(ticket);
                }
            }
        }

        private static async Task MergeOpenVoicemails(IEnumerable<TicketSummary> tickets)
        {
            //var phoneUsers = tickets.Where(x => Regex.IsMatch(x.UserName, @"^[0-9]{10}"));
            //var phonesGroupBy = phoneUsers.GroupBy(s => s.UserName.Substring(0, 9)).Where(x => x.Count() > 1);
            var detailedTickets = new List<TicketDetails>();
            foreach (var ticket in tickets)
            {
                detailedTickets.Add(await _jitBit.GetTicket(ticket));
            }
            var phoneBody = detailedTickets.Where(x =>
                Regex.IsMatch(x.Body, "(?<=You received a voice mail from<a href=\\\"tel:)[0-9]{10}"));
            var phoneBodyGroupBy = phoneBody.GroupBy(x => Regex.Match(x.Body, "(?<=You received a voice mail from<a href=\\\"tel:)[0-9]{10}").Value).Where(x => x.Count() > 1);        

            foreach (var set in phoneBodyGroupBy)
            {
                var newList = set.OrderBy(x => x.IssueDate).ToList();
                var firsTicket = newList.FirstOrDefault();
                var comment = $"Closed the following newer tickets and linked them to current ticket ({firsTicket?.TicketId}).{Environment.NewLine}";

                foreach (var userTicket in newList.Skip(1))
                {
                    comment += $"{_baseUrl}/Ticket/{userTicket.TicketId}{Environment.NewLine}";

                    WriteLine($"Comment: {userTicket.TicketId}");
                    await _jitBit.Comment(userTicket.TicketId,
                        $"Closing as duplicate and linking to oldest open ticket {_baseUrl}/Ticket/{firsTicket?.TicketId}",
                        techOnly: true);
                    WriteLine($"Closing: {userTicket.TicketId}");
                    await _jitBit.CloseTicket(userTicket);
                }
                WriteLine($"Comment: {firsTicket?.TicketId}");
                await _jitBit.Comment(firsTicket, comment, techOnly: true);
            }
        }
    }
}
