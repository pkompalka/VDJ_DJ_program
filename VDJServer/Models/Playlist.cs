namespace VDJServer.Models
{
    public class Playlist
    {
        public int Id { get; set; }

        public int SongId { get; set; }

        public bool WasPlayed { get; set; }

        public Playlist(int id, int songId, bool wasPlayed)
        {
            Id = id;
            SongId = songId;
            WasPlayed = wasPlayed;
        }
    }
}
