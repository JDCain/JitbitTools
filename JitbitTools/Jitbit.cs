using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JitbitTools.Models;
using Newtonsoft.Json;

namespace JitbitTools
{
    public class Jitbit
    {
        private readonly byte[] _credentials;
        private readonly string _baseUrl;
        public Jitbit(byte[] credentials, string baseUrl)
        {
            _credentials = credentials;
            _baseUrl = baseUrl;
        }

        public async Task<IEnumerable<TicketSummary>> GetTicketsByParameters(Dictionary<string, string> parameters)
        {

            var url = $"{_baseUrl}/api/Tickets";
            var response = await Post(parameters, url);
            var jsonString = await ProcessResponse(response);
            try
            {
                return JsonConvert.DeserializeObject<List<TicketSummary>>(jsonString);

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CustomField>> GetCustomFields(TicketSummary ticket)
        {
            return await GetCustomFields(ticket.IssueID);
        }

        public async Task<IEnumerable<CustomField>> GetCustomFields(TicketDetails ticket)
        {
            return await GetCustomFields(ticket.IssueId);
        }

        public async Task<IEnumerable<CustomField>> GetCustomFields(int id)
        {
            var url = $"{_baseUrl}/api/TicketCustomFields?id={id}";
            var response = await Get(url);
            var jsonString = await ProcessResponse(response);
            try
            {
                return JsonConvert.DeserializeObject<List<CustomField>>(jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TicketDetails> GetTicketDetails(TicketSummary ticket)
        {
            return await GetTicketDetails(ticket.IssueID);
        }

        public async Task<TicketDetails> GetTicketDetails(int id)
        {
            var url = $"{_baseUrl}/api/Ticket?id={id}";
            var response = await Get(url);
            var jsonString = await ProcessResponse(response);
            try
            {
                return JsonConvert.DeserializeObject<TicketDetails>(jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TicketDetails>> GetDetailedTickets(IEnumerable<TicketSummary> tickets, bool getCustomFields = false)
        {
            var detailedTickets = new List<TicketDetails>();
            foreach (var ticket in tickets)
            {
                var ticketDetails = await GetTicketDetails(ticket);
                if (getCustomFields)
                {
                    ticketDetails.CustomFields = await GetCustomFields(ticketDetails);
                 
                }
                detailedTickets.Add(ticketDetails);
            }
            return detailedTickets;
        }

        public async Task<IEnumerable<TicketSummary>> GetSummaryTickets(int catagory, bool getCustomFields = false, string mode = "unclosed")
        {
            var offset = 0;
            var allTickets = new List<TicketSummary>();
            List<TicketSummary> ticketSet;
            do
            {
                ticketSet = await GetTicketsByParameters(new Dictionary<string, string>
                {
                    {"categoryId", catagory.ToString()},
                    {"count", "100"},
                    {"offset", $"{offset}"},
                    {"mode", mode}
                }) as List<TicketSummary>;
                allTickets.AddRange(ticketSet?.ToList());

                offset += 100;
            } while (ticketSet?.Count == 100);

            if (getCustomFields)
            {
                foreach (var ticket in allTickets)
                {                   
                    ticket.CustomFields = await GetCustomFields(ticket);
                }
            }
            return allTickets;
        }

        public async Task CloseTicket(TicketSummary ticket)
        {
            await CloseTicket(ticket.IssueID);
        }

        public async Task CloseTicket(TicketDetails ticket)
        {
            await CloseTicket(ticket.TicketId);
        }

        public async Task CloseTicket(int? id)
        {
            var url = $"{_baseUrl}/API/UpdateTicket";
            var parameters = new Dictionary<string, string> {{"id", id.ToString()}, {"statusID", 3.ToString()}};
            var response = await Post(parameters, url);
        }

        public async Task Comment(TicketSummary ticket, string comment, bool techOnly = true)
        {
            await Comment(ticket.IssueID, comment, techOnly);
        }

        public async Task Comment(TicketDetails ticket, string comment, bool techOnly = true)
        {
            await Comment(ticket.TicketId, comment, techOnly);
        }

        public async Task Comment(int? id, string comment, bool techOnly = true)
        {
            var parameters = new Dictionary<string, string>
            {
                {"id", id.ToString()},
                {"body", comment},
                {"forTechsOnly", techOnly.ToString()}
            };
            var url = $"{_baseUrl}/API/comment";
            var response = await Post(parameters, url);
        }

        public async Task WriteCustomField(int ticketId, CustomField field)
        {
            var url = $"{_baseUrl}/API/SetCustomField";
            var parameters = new Dictionary<string, string>
            {
                {"ticketid", ticketId.ToString()},
                {"fieldId", field.FieldID.ToString()},
                {"value", field.Value}
            };
            await Post(parameters, url);
        }

        private async Task<string> ProcessResponse(HttpResponseMessage responseMessage)
        {
            string result = null;
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                result = await responseMessage.Content.ReadAsStringAsync();
            }
            return result;
        }

        private async Task<HttpResponseMessage> Post(Dictionary<string, string> parameters, string url)
        {
            HttpResponseMessage responseMessage;
            using (var httpClient = new HttpClient())
            {
                var encodedContent = new FormUrlEncodedContent(parameters);
                SetCreds(httpClient);
                responseMessage = await httpClient.PostAsync(url, encodedContent).ConfigureAwait(false);
                if (responseMessage.StatusCode == (HttpStatusCode)429)
                {
                    await Task.Delay(60000);
                    responseMessage = await Post(parameters, url);
                }
            }
            return responseMessage;
        }

        private async Task<HttpResponseMessage> Get(string url)
        {
            HttpResponseMessage responseMessage;
            using (var httpClient = new HttpClient())
            {
                SetCreds(httpClient);
                responseMessage = await httpClient.GetAsync(url).ConfigureAwait(false);
                if (responseMessage.StatusCode == (HttpStatusCode)429)
                {
                    await Task.Delay(60000);
                    responseMessage = await Get(url);
                }
            }
            return responseMessage;
        }

        private void SetCreds(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_credentials));
        }
    }
}
