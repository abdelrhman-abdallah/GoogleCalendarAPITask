using CalenderAPITask.DTO;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace CalenderAPITask.Service
{
    public interface IGoogleCalendarService
    {
        Task <CalendarService> GetCalendarService(string jwtToken);
        Task<string> AddEvent(GoogleCalendarReqDTO googleCalendarReqDTO, string jwtToken);
        Task<string> DeleteEvent(string calenderEventId, string jwtToken);
        
        Task<GetResponseObjDTO> GetEvents(string jwtToken, DateTime? startDate, DateTime? endDate, string nextPageToken, int maxResultSize = 10, string q = "");
    }
}
