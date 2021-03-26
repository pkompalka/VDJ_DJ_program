namespace VDJServer.Models
{
    public class RequestRich : Request
    {
        public string Title { get; set; }

        public string LeadAuthor { get; set; }

        public RequestRich(Request request)
        {
            Id = request.Id;
            SongId = request.SongId;
            Nick = request.Nick;
            Dedication = request.Dedication;
            Title = string.Empty;
            LeadAuthor = string.Empty;
        }

        public RequestRich()
        {
            Id = 0;
            SongId = 0;
            Nick = string.Empty;
            Dedication = string.Empty;
            Title = string.Empty;
            LeadAuthor = string.Empty;
        }
    }
}

