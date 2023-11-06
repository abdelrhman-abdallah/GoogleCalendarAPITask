using Google.Apis.Calendar.v3.Data;

namespace CalenderAPITask.DTO
{
    public class GetResponseObjDTO
    {
        public List<EventDTO> EventList { get; set; }
        public string NextPageToken { get; set; }
    }
}
