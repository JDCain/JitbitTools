using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JitbitTools;
using JitbitTools.Models;

namespace JitBit.Console
{
    public class Program
    {
        private static readonly string _baseUrl = "***********";
        private static Jitbit _jitBit;
        public static Regex Sepid => new Regex(@"(?i)(?<![0-9])[0-9]{5,6}\b(?-i)");
        static void Main(string[] args)
        {
            _jitBit = new Jitbit(Encoding.ASCII.GetBytes("*******"), _baseUrl);
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var mergeTicketsByUserFunc = new Func<Ticket, Task>(async (x) => await MergeTicketsByUser(x));
            var hdTickets = await _jitBit.GetTickets(Convert.ToInt32(Catagories.HdVoicemail));
            await LoopTicketsTask(hdTickets, mergeTicketsByUserFunc);
        }

        private static async Task LoopTicketsTask(IEnumerable<Ticket> tickets, Func<Ticket, Task> test)
        {
            foreach (var ticket in tickets)
            {
                System.Console.WriteLine($"trying: {ticket.IssueID}");
                await test.Invoke(ticket);
            }
        }

        private static async Task MergeTicketsByUser(Ticket ticket)
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
                var builtComment = $"Closed the following tickets and linked them to this one:{Environment.NewLine}";

                foreach (var userTicket in newList.Skip(1))
                {
                    builtComment += $"{_baseUrl}/Ticket/{userTicket.IssueID}{Environment.NewLine}";
                    await _jitBit.Comment(userTicket,
                        $"Closing and linking to oldest open ticket {_baseUrl}/Ticket/{firsTicket?.IssueID}",
                        techOnly: true);
                    await _jitBit.CloseTicket(ticket);
                }
                await _jitBit.Comment(firsTicket, builtComment, techOnly:true);
            }
        }

        private static async Task CloseShortVoiceMail(IEnumerable<Ticket> tickets)
        {
            foreach (var ticket in tickets)
            {
                var matches = Regex.Matches(ticket.Subject,
                    @"(((\d+):(\d+))|((\d+)\s*m[a-z]*)|(((\d+)\s*h[a-z]*)([^\d]*(\d+)\s*m[a-z]*)?)|(\d+))");
                var caculate = Helpers.MatchCollectonToSeconds(matches);
                if (caculate < 12)
                {
                    await _jitBit.CloseTicket(ticket);
                }
            }
        }

        private static async Task MergeOpenVoicemails(IEnumerable<Ticket> tickets)
        {
            var phoneUsers = tickets.Where(x => Regex.IsMatch(x.UserName, @"^[0-9]{10}"));
            var phonesGroupBy = phoneUsers.GroupBy(s => s.UserName.Substring(0, 9)).Where(x => x.Count() > 1);

            foreach (var set in phonesGroupBy)
            {
                var newList = set.OrderBy(x => x.IssueDate).ToList();
                var firsTicket = newList.FirstOrDefault();
                var comment = $"Closed the following tickets and linked them to this one:{Environment.NewLine}";

                foreach (var userTicket in newList.Skip(1))
                {
                    comment += $"{_baseUrl}/Ticket/{userTicket.IssueID}{Environment.NewLine}";

                    await _jitBit.Comment(userTicket,
                        $"Closing and linking to oldest open ticket {_baseUrl}/Ticket/{firsTicket?.IssueID}",
                        techOnly: true);
                    await _jitBit.CloseTicket(userTicket);
                }

                await _jitBit.Comment(firsTicket, comment, techOnly: true);
            }
        }
    }
}
