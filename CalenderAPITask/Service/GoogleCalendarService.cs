using CalenderAPITask.DTO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using static Google.Rpc.Context.AttributeContext.Types;

namespace CalenderAPITask.Service
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IConfiguration _config;

        public GoogleCalendarService(FirestoreDb firestoreDb, IConfiguration config)
        {
            _firestoreDb = firestoreDb;
            _config = config;
        }
        public async Task<CalendarService> GetCalendarService(string jwtToken) 
        {
            string[] scopes = { CalendarService.Scope.Calendar };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;
            var claimTypeForGmail = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            var userGmail = token?.Claims.FirstOrDefault(claim => claim.Type == claimTypeForGmail).Value;

            var userCollection = _firestoreDb.Collection("users");
            var userQuery = userCollection.WhereEqualTo("gmail", userGmail);
            var userSnapshot = await userQuery.GetSnapshotAsync();

            var userDocument = userSnapshot.Documents.FirstOrDefault();

            var clientId = _config.GetValue<string>("CLIENT_ID");
            var clientSecret = _config.GetValue<string>("CLIENT_SECRET");
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret }, scopes, userGmail, CancellationToken.None, new FileDataStore("CalenderAPI.Token.json")).Result;

            var expiryDate = userDocument.GetValue<int>("expiryDate");
            if (expiryDate == 0)
            {
                var updatedData = new Dictionary<string, object>
                {
                    {"refreshToken", credential.Token.RefreshToken },
                    {"expiryDate",credential.Token.ExpiresInSeconds },
                    {"issuedAt",credential.Token.IssuedUtc }
                };
                await userDocument.Reference.UpdateAsync(updatedData);
            }
            else
            {
                var expiresAt = userDocument.GetValue<DateTime>("issuedAt").AddSeconds(expiryDate);
                if (expiresAt < DateTime.Now)
                {
                    if (credential.RefreshTokenAsync(CancellationToken.None).Result)
                    {
                        var updatedData = new Dictionary<string, object>
                        {
                            {"expiryDate", credential.Token.ExpiresInSeconds },
                            {"issuedAt", credential.Token.IssuedUtc }
                        };
                        await userDocument.Reference.UpdateAsync(updatedData);
                    }
                    else
                    {
                        throw new Exception("Unable to refresh access token.");
                    }
                }
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "remoteCalenderHandler"
            });

            return service;
        }
        public async Task<string>AddEvent(GoogleCalendarReqDTO googleCalendarReqDTO,string jwtToken)
        {

            var service = await GetCalendarService(jwtToken);
            bool validEventTime = googleCalendarReqDTO.StartTime < googleCalendarReqDTO.EndTime && googleCalendarReqDTO.StartTime > DateTime.Now;
            bool validEventDates = googleCalendarReqDTO.StartTime.DayOfWeek != DayOfWeek.Friday && googleCalendarReqDTO.StartTime.DayOfWeek != DayOfWeek.Saturday;

            if (validEventTime && validEventDates)
            {
                var newEvent = new Event()
                {
                    Summary = googleCalendarReqDTO.Summary,
                    Description = googleCalendarReqDTO.Description,
                    Start = new EventDateTime() { DateTime = googleCalendarReqDTO.StartTime },
                    End = new EventDateTime() { DateTime = googleCalendarReqDTO.EndTime },
                };
                try
                {
                    var createdEvent = service.Events.Insert(newEvent, "primary").Execute();
                    return createdEvent.Id;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return "";
            }
        }

        public async Task<string> DeleteEvent(string calenderEventId, string jwtToken)
        {

            var service = await GetCalendarService(jwtToken);
            try
            {
                var deletedEvent = service.Events.Delete("primary",calenderEventId).Execute();
                return deletedEvent;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        public async Task<GetResponseObjDTO> GetEvents(string jwtToken,DateTime? startDate, DateTime? endDate, string nextPageToken, int maxResultSize =10 ,string q = "")
        {
            var calendarId = "primary";
            var timeMin = startDate ?? DateTime.UtcNow;
            string formattedTimeMin = timeMin.ToString("yyyy-MM-ddTHH:mm:ssK");
            var timeMax = endDate ?? DateTime.UtcNow.AddDays(7);
            string formattedTimeMax = timeMax.ToString("yyyy-MM-ddTHH:mm:ssK");

            var service = await GetCalendarService(jwtToken);
            try
            {
                var listOfEventsRequest = service.Events.List(calendarId);
                listOfEventsRequest.TimeMin = DateTime.ParseExact(formattedTimeMin, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                listOfEventsRequest.TimeMax = DateTime.ParseExact(formattedTimeMax, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                listOfEventsRequest.MaxResults = maxResultSize;
                listOfEventsRequest.Q = q;
                if (nextPageToken != null)
                {
                    listOfEventsRequest.PageToken = nextPageToken;
                }

                List<EventDTO> listOfEvents = new List<EventDTO>();

                var response = await listOfEventsRequest.ExecuteAsync();

                if (response.Items != null)
                {
                    foreach (var item in response.Items)
                    {
                        EventDTO mappedEvent = new EventDTO()
                        {
                            EventSummary = item.Summary,
                            EventDescription = item.Description,
                        };
                        listOfEvents.Add(mappedEvent);
                    }
                }

                listOfEventsRequest.PageToken = response.NextPageToken;

                var responseObj = new GetResponseObjDTO() {EventList =  listOfEvents,NextPageToken = listOfEventsRequest.PageToken};

                return responseObj;
            }
            catch (Exception ex )
            {
                Console.WriteLine($"Error === {ex.Message} ===");
                throw new Exception("Failed to retrieve events from Google Calendar API", ex);
            }
        }
    }
}
