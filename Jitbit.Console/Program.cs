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
            var mergeTicketsByUserFunc = new Func<TicketSummary, Task>(async (x) => await MergeTicketsByUser(x));
            //var updateCustomFieldFunc = new Func<Ticket, Task>(async (x) => await UpdateCustomField(x));
            foreach (Catagories catagory in Enum.GetValues(typeof(Catagories)))
            {
                var tickets = await _jitBit.GetTickets(Convert.ToInt32(catagory));
                await CloseShortVoiceMail(tickets);
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
                var builtComment = $"Closed the following tickets and linked them to this one:{Environment.NewLine}";

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

        private static async Task CloseShortVoiceMail(IEnumerable<TicketSummary> tickets)
        {
            foreach (var ticket in tickets)
            {
                var matches = Regex.Matches(ticket.Subject,
                    @"(((\d+):(\d+))|((\d+)\s*m[a-z]*)|(((\d+)\s*h[a-z]*)([^\d]*(\d+)\s*m[a-z]*)?)|(\d+))");
                var caculate = Helpers.MatchCollectonToSeconds(matches);
                if (caculate < 10)
                {
                    WriteLine($"Closing: {ticket.IssueID}");
                    await _jitBit.CloseTicket(ticket);
                }
            }
        }

        private static async Task MergeOpenVoicemails(IEnumerable<TicketSummary> tickets)
        {
            var body =
                "<a href=\"#\" target=\"_blank\" rel=\"nofollow\"><span style=\"color:#1F497D\"> <br/></span></a><span style=\"color:#1F497D\"> <br/></span><span style=\"color:#1F497D\">Stephanie Faust<br/></span><span style=\"color:#1F497D\">Attendance Coordinator<br/></span><span style=\"color:#1F497D\">Electronic Classroom of Tomorrow<br/></span><span style=\"color:#1F497D\">Phone 614-492-8884 x2491<br/></span><span style=\"color:#1F497D\">Fax 614-643-7748<br/></span><span style=\"color:#1F497D\"> <br/></span><span style=\"color:#1F497D\"> <br/></span><b>From:</b> Microsoft Outlook<b> On Behalf Of</b> 4196019287<br/><b>Sent:</b> Friday, December 1, 2017 11:15 AM<br/><b>To:</b> Attendance &lt;Truancy@ecotoh.org&gt;<br/><b>Subject:</b> Voice Mail (39 seconds)<br/> <br/><span style=\"color:black\">Yes is this isn't ramble my dodson's ID number is RSG 5005 24.<br/><br/>And we were just calling because she's having problems logging in today I guess something about a trash words not right -- as you can call me back because it is very important because she he only had some sort so I'm trying to log in or E very beach aright me -- so please call me back.<br/><br/>You can reach me at<a href=\"tel:4196019287\" target=\"_blank\" rel=\"nofollow\"> (419) 601-9287</a> thank you.<br/></span><span style=\"color:#686A6B\">Preview provided by Microsoft Speech Technology.<a href=\"http://go.microsoft.com/fwlink/?LinkId=150048\" target=\"_blank\" rel=\"nofollow\"> Learn More...</a><br/></span><span style=\"color:black\"><hr/></span><b><span style=\"color:#000066\">You received a voice mail from<a href=\"tel:4196019287\" target=\"_blank\" rel=\"nofollow\"> 4196019287</a><br/></span></b><span style=\"color:black\"> <br/></span><table><tr><td style='padding:7px 20px'><span style=\"color:#686A6B\">Caller-Id:<br/></span></td><td style='padding:7px 20px'><span style=\"color:black\"><a href=\"tel:4196019287\" target=\"_blank\" rel=\"nofollow\">4196019287</a><br/></span></td></tr></table>";

            var testMatch = Regex.Match(body, "(?<=You received a voice mail from<a href=\\\"tel:)[0-9]{10}");
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
                var comment = $"Closed the following tickets and linked them to this one:{Environment.NewLine}";

                foreach (var userTicket in newList.Skip(1))
                {
                    comment += $"{_baseUrl}/Ticket/{userTicket.TicketId}{Environment.NewLine}";

                    WriteLine($"Comment: {userTicket.TicketId}");
                    await _jitBit.Comment(userTicket.TicketId,
                        $"Closing and linking to oldest open ticket {_baseUrl}/Ticket/{firsTicket?.TicketId}",
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
