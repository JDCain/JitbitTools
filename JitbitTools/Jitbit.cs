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

        public async Task<IEnumerable<Ticket>> GetTicketsByParameters(Dictionary<string, string> parameters)
        {

            var url = $"{_baseUrl}/api/Tickets";
            var response = await Post(parameters, url);
            var jsonString = await ProcessResponse(response);
            try
            {
                return JsonConvert.DeserializeObject<List<Ticket>>(jsonString);

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CustomField>> GetCustomFields(Ticket ticket)
        {
            var url = $"{_baseUrl}/api/TicketCustomFields?id={ticket.IssueID}";
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

        public async Task<IEnumerable<Ticket>> GetTickets(int catagory, bool getCustomFields = false, string mode = "unclosed")
        {
            var offset = 0;
            var allTickets = new List<Ticket>();
            List<Ticket> ticketSet;
            do
            {
                ticketSet = await GetTicketsByParameters(new Dictionary<string, string>
                {
                    {"categoryId", catagory.ToString()},
                    {"count", "100"},
                    {"offset", $"{offset}"},
                    {"mode", mode}
                }) as List<Ticket>;
                allTickets.AddRange(ticketSet?.ToList());

                offset += 100;
            } while (ticketSet?.Count == 100);

            if (getCustomFields)
            {
                foreach (var ticket in allTickets)
                {
                    Console.WriteLine($"Getting Custom fields for {ticket.IssueID}");
                    ticket.CustomFields = await GetCustomFields(ticket);
                }
            }
            return allTickets;
        }

        public async Task CloseTicket(Ticket ticket)
        {
            var url = $"{_baseUrl}API/UpdateTicket";
            var parameters = new Dictionary<string, string> { { "id", ticket.IssueID.ToString() }, { "statusID", 3.ToString() } };
            var response = await Post(parameters, url);
        }

        public async Task Comment(Ticket ticket, string comment, bool techOnly = true)
        {
            var parameters = new Dictionary<string, string>
            {
                {"id", ticket.IssueID.ToString()},
                {"body", comment},
                {"forTechsOnly", techOnly.ToString()}
            };
            var url = $"{_baseUrl}/API/comment";
            var response = await Post(parameters, url);
        }

        public async Task WriteCustomField(Ticket ticket, CustomField field)
        {
            var url = $"{_baseUrl}/API/SetCustomField";
            var parameters = new Dictionary<string, string>
            {
                {"ticketid", ticket.IssueID.ToString()},
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
