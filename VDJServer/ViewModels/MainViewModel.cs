using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using TagLib;
using VDJServer.Models;
using VDJServer.Utilities;

namespace VDJServer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public HttpClient HttpClientServer { get; set; }

        private string serverPassword;

        public string ServerPassword
        {
            get
            {
                return serverPassword;
            }
            set
            {
                serverPassword = value;
                NotifyPropertyChanged(nameof(ServerPassword));
            }
        }

        private string vdjDirectory;

        public string VdjDirectory
        {
            get
            {
                return vdjDirectory;
            }
            set
            {
                vdjDirectory = value;
                NotifyPropertyChanged(nameof(VdjDirectory));
            }
        }

        private string serverStatus;

        public string ServerStatus
        {
            get
            {
                return serverStatus;
            }
            set
            {
                serverStatus = value;
                NotifyPropertyChanged(nameof(ServerStatus));
            }
        }

        private bool isServerStartButtonEnabled;

        public bool IsServerStartButtonEnabled
        {
            get
            {
                return isServerStartButtonEnabled;
            }
            set
            {
                isServerStartButtonEnabled = value;
                NotifyPropertyChanged(nameof(IsServerStartButtonEnabled));
            }
        }

        private bool isServerCloseButtonEnabled;

        public bool IsServerCloseButtonEnabled
        {
            get
            {
                return isServerCloseButtonEnabled;
            }
            set
            {
                isServerCloseButtonEnabled = value;
                NotifyPropertyChanged(nameof(IsServerCloseButtonEnabled));
            }
        }

        private bool isUpdateButtonEnabled;

        public bool IsUpdateButtonEnabled
        {
            get
            {
                return isUpdateButtonEnabled;
            }
            set
            {
                isUpdateButtonEnabled = value;
                NotifyPropertyChanged(nameof(IsUpdateButtonEnabled));
            }
        }

        public List<Song> SongList { get; set; }

        public List<Playlist> PlaylistSongList { get; set; }

        public int SongFirstId { get; set; }

        public int PlaylistLinePosition { get; set; }

        public int SongInPlaylistId { get; set; }

        public bool PlaylistFirstLoad { get; set; }

        private ObservableCollection<RequestRich> richRequestObservable;

        public ObservableCollection<RequestRich> RichRequestObservable
        {
            get
            {
                return richRequestObservable;
            }
            set
            {
                richRequestObservable = value;
                NotifyPropertyChanged(nameof(RichRequestObservable));
            }
        }

        private RequestRich requestSelected;

        public RequestRich RequestSelected
        {
            get
            {
                return requestSelected;
            }
            set
            {
                requestSelected = value;
                NotifyPropertyChanged(nameof(RequestSelected));
            }
        }

        public int LastRequestId { get; set; }

        public int HistoryLinePosition { get; set; }

        public int CurrentSongId { get; set; }

        public string SongHistoryPath { get; set; }

        public ObservableCollection<string> ChatObservable { get; set; }

        private string chatText;

        public string ChatText
        {
            get
            {
                return chatText;
            }
            set
            {
                chatText = value;
                NotifyPropertyChanged(nameof(ChatText));
            }
        }

        private string top5Text;

        public string Top5Text
        {
            get
            {
                return top5Text;
            }
            set
            {
                top5Text = value;
                NotifyPropertyChanged(nameof(Top5Text));
            }
        }

        private string top10Text;

        public string Top10Text
        {
            get
            {
                return top10Text;
            }
            set
            {
                top10Text = value;
                NotifyPropertyChanged(nameof(Top10Text));
            }
        }

        public int ChatLines { get; set; }

        public int LastChatId { get; set; }

        public int PreviousSongToDeleteId { get; set; }

        private bool isCheckBoxChecked;

        public bool IsCheckBoxChecked
        {
            get
            {
                return isCheckBoxChecked;
            }
            set
            {
                isCheckBoxChecked = value;
                NotifyPropertyChanged(nameof(IsCheckBoxChecked));
            }
        }

        public DateTime TodaysDate { get; set; }

        public DateTime PlaylistDate { get; set; }

        public ICommand ButtonStartServer { get; set; }

        public ICommand ButtonCloseServer { get; set; }

        public ICommand ButtonUpdatePlaylist { get; set; }

        public ICommand ShowMessageBoxCommand { get; set; }

        public ICommand DeleteFromGridCommand { get; set; }

        public MainViewModel()
        {
            HttpClientServer = new HttpClient();
            HttpClientServer.BaseAddress = new Uri("https://azure.net/");
            HttpClientServer.DefaultRequestHeaders.Accept.Clear();
            HttpClientServer.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            ButtonStartServer = new RelayCommand(async o => await StartServer());
            ButtonCloseServer = new RelayCommand(async o => await CloseServer());
            ButtonUpdatePlaylist = new RelayCommand(async o => await UpdatePlaylist());
            ShowMessageBoxCommand = new RelayCommand(ShowMessageBox);
            DeleteFromGridCommand = new RelayCommand(DeleteFromGridMethod);
            IsServerStartButtonEnabled = true;
            IsServerCloseButtonEnabled = false;
            IsUpdateButtonEnabled = false;
            ServerPassword = string.Empty;
            VdjDirectory = "";
            ServerStatus = "Server password: ";
            MessageBox.Show("Before starting server prepare everything in VDJ (playlist you will be using must be called myplaylist), when you are done with VDJ setup paste directory and press green button to start server. To stop people from loging to server press red button. If you want program to automatically delete played requests check checkbox", "Important note!");
        }

        public async Task StartServer()
        {
            if(VdjDirectory == "")
            {
                ServerStatus = "Paste directory first!";
            }
            else
            {
                VdjDirectory.Trim();
                SongFirstId = 1;
                PlaylistLinePosition = 0;
                SongInPlaylistId = 1;
                ChatLines = 0;
                LastRequestId = 0;
                LastChatId = 0;
                HistoryLinePosition = 0;
                PreviousSongToDeleteId = -1;
                PlaylistFirstLoad = false;
                ChatText = "Today's chat";
                SongList = new List<Song>();
                PlaylistSongList = new List<Playlist>();
                ChatObservable = new ObservableCollection<string>();
                RichRequestObservable = new ObservableCollection<RequestRich>();
                object lockObj = new object();
                BindingOperations.EnableCollectionSynchronization(RichRequestObservable, lockObj);
                RequestSelected = new RequestRich();

                IsServerStartButtonEnabled = false;
                ServerStatus = "Server is starting!";
                try
                {
                    LoadSongDatabase();
                }
                catch
                {
                    MessageBox.Show("Incorrect VDJ path!");
                }
                await FirstLoadPlaylist();
                await FirstRequestDatabase();
                await FirstCurrentSongsDatabase();
                await FirstChatDatabase();
                await AddSongsToDatabase();
                GetCurrentSong();
                await AddDJToDatabase(ServerPassword);
                GetRequestsandTop5();
                if (ServerStatus == "Server is starting!")
                {
                    ServerStatus = "Server is running!";
                    IsUpdateButtonEnabled = true;
                    IsServerCloseButtonEnabled = true;
                    MessageBox.Show("Server started successfully!");
                }
            }
        }

        public async Task CloseServer()
        {
            IsServerCloseButtonEnabled = false;
            try
            {
                HttpResponseMessage closeServerResponse = await HttpClientServer.PutAsJsonAsync("api/Users/close", "*fQ$|4x#9d_,yP7>a@0");

                if (closeServerResponse.IsSuccessStatusCode)
                {
                    ServerPassword = "Server closed!";
                }
                else
                {
                    ServerPassword = "Server failed to close!";
                    IsServerCloseButtonEnabled = true;
                }
            }
            catch
            {
                IsServerCloseButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public async Task AddDJToDatabase(string password)
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/users");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {
                    HttpResponseMessage setPasswordResponse = await HttpClientServer.PostAsJsonAsync("api/Users/adddj", password);
                    if (setPasswordResponse.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        ServerStatus = "Server fail, try again!";
                        MessageBox.Show("Server fail, try again!");
                    }
                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public void LoadSongDatabase()
        {
            List<Song> SongListTmp = new List<Song>();
            XmlDocument PlaylistXML = new XmlDocument();
            string databasePath = VdjDirectory + "\\database.xml";
            using (FileStream fileStream = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                PlaylistXML.Load(fileStream);
            }
            XmlNodeList AllSongs = PlaylistXML.GetElementsByTagName("Song");
            List<XmlNode> SongNodeList = new List<XmlNode>();
            for (int i = 0; i < AllSongs.Count; i++)
            {
                if (AllSongs[i].InnerXml.StartsWith("<Tags Author") == true)
                {
                    SongNodeList.Add(AllSongs[i]);
                }
            }

            for (int i10 = 0; i10 < SongNodeList.Count; i10++)
            {
                string songKey = SongNodeList[i10].Attributes[0].Value;
                if (songKey.Substring(songKey.Length - 4)[0].Equals('.'))
                {
                    if (songKey.LastIndexOf('\\') > 0)
                    {
                        bool isRepeatTitle = false;
                        bool isFeatureFlag = false;

                        string songPath = songKey;
                        string songInfo = songKey.Remove(0, songKey.LastIndexOf('\\') + 1);
                        songInfo = songInfo.Remove(songInfo.Length - 4);

                        int tmpSeparator = songInfo.IndexOf("-");
                        string leadAuthor = songInfo.Remove(tmpSeparator - 1);
                        string[] leadAuthorArray = leadAuthor.Split(',');

                        if (leadAuthorArray.Length > 1)
                        {
                            for (int i1 = 1; i1 < leadAuthorArray.Length; i1++)
                            {
                                leadAuthorArray[i1] = leadAuthorArray[i1].Remove(0, 1);
                            }
                        }

                        string title = songInfo.Remove(0, tmpSeparator + 2);
                        string featureAuthor = title;

                        if (title.Contains("(feat"))
                        {
                            int tmpFeat = title.IndexOf("(feat");
                            featureAuthor = title.Remove(0, tmpFeat);
                            title = title.Remove(tmpFeat - 1);
                            isFeatureFlag = true;
                        }

                        for (int i2 = 0; i2 < SongListTmp.Count; i2++)
                        {
                            if (SongListTmp[i2].Title == title && SongListTmp[i2].LeadAuthorList[0] == leadAuthorArray[0])
                            {
                                isRepeatTitle = true;
                            }
                        }

                        if (isRepeatTitle == false)
                        {
                            Song song = new Song(title);

                            TagLib.File songFile = new TagLib.Mpeg.AudioFile(songPath);
                            IPicture cover = songFile.Tag.Pictures[0];
                            MemoryStream tmpStream = new MemoryStream(cover.Data.Data);
                            tmpStream.Seek(0, SeekOrigin.Begin);

                            BitmapImage songCoverImage = new BitmapImage();
                            songCoverImage.BeginInit();
                            songCoverImage.StreamSource = tmpStream;
                            songCoverImage.DecodePixelHeight = 200;
                            songCoverImage.DecodePixelWidth = 200;
                            songCoverImage.EndInit();

                            byte[] imageByte;
                            JpegBitmapEncoder tmpEncoder = new JpegBitmapEncoder();
                            tmpEncoder.Frames.Add(BitmapFrame.Create(songCoverImage));
                            using (MemoryStream tmpMS = new MemoryStream())
                            {
                                tmpEncoder.Save(tmpMS);
                                imageByte = tmpMS.ToArray();
                            }

                            song.CoverArt = imageByte;
                            song.Cover = Convert.ToBase64String(imageByte);
                            song.Album = songFile.Tag.Album;

                            for (int i3 = 0; i3 < leadAuthorArray.Length; i3++)
                            {
                                song.LeadAuthorList.Add(leadAuthorArray[i3]);
                                song.LeadAuthor = song.LeadAuthor + leadAuthorArray[i3] + ", ";
                            }

                            song.LeadAuthor = song.LeadAuthor.Remove(song.LeadAuthor.Length - 2);

                            if (isFeatureFlag == true)
                            {
                                featureAuthor = featureAuthor.Remove(0, 7);
                                featureAuthor = featureAuthor.Remove(featureAuthor.Length - 1);
                                song.FeatureAuthor = featureAuthor;
                                string[] featureAuthorArray = featureAuthor.Split(',');

                                if (featureAuthorArray.Length > 1)
                                {
                                    for (int i4 = 1; i4 < featureAuthorArray.Length; i4++)
                                    {
                                        featureAuthorArray[i4] = featureAuthorArray[i4].Remove(0, 1);
                                    }
                                }

                                for (int i5 = 0; i5 < featureAuthorArray.Length; i5++)
                                {
                                    song.FeatureAuthorList.Add(featureAuthorArray[i5]);
                                }
                                song.FullAuthor = song.LeadAuthor + " feat. " + song.FeatureAuthor;
                            }
                            else
                            {
                                song.FullAuthor = song.LeadAuthor;
                            }

                            song.Id = SongFirstId;
                            SongFirstId++;
                            SongListTmp.Add(song);
                        }
                    }
                }
            }
            SongList = SongListTmp;
        }

        public async Task AddSongsToDatabase()
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/songs");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {
                    HttpResponseMessage songDatabaseResponse = await HttpClientServer.PostAsJsonAsync("api/songs/add", SongList);
                    if (songDatabaseResponse.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        ServerStatus = "Server fail, try again!";
                        MessageBox.Show("Server fail, try again!");
                    }
                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }
        
        public async Task FirstLoadPlaylist()
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/playlists");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {
                    await AddSongsToPlaylistDatabase(0);
                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public async Task AddSongsToPlaylistDatabase(int startPosition)
        {
            string[] PlaylistArray = ReadPlaylistFromLine(startPosition);

            for (int i = 0; i < PlaylistArray.Length; i++)
            {
                if (PlaylistArray[i].Substring(PlaylistArray[i].Length - 4)[0].Equals('.'))
                {
                    string songInfo = PlaylistArray[i].Remove(0, PlaylistArray[i].LastIndexOf('\\') + 1);
                    songInfo = songInfo.Remove(songInfo.Length - 4);
                    int tmpSeparator = songInfo.IndexOf("-");
                    string leadAuthor = songInfo.Remove(tmpSeparator - 1);
                    string[] leadAuthorArray = leadAuthor.Split(',');
                    string title = songInfo.Remove(0, tmpSeparator + 2);

                    if (title.Contains("(feat"))
                    {
                        int tmpFeat = title.IndexOf("(feat");
                        title = title.Remove(tmpFeat - 1);
                    }

                    for (int i1 = 0; i1 < SongList.Count; i1++)
                    {
                        if (leadAuthorArray[0] == SongList[i1].LeadAuthorList[0] && title == SongList[i1].Title)
                        {
                            Playlist playlistSong = new Playlist(SongInPlaylistId, SongList[i1].Id, false);
                            PlaylistSongList.Add(playlistSong);
                            SongInPlaylistId++;
                        }
                    }
                }
            }

            try
            {
                HttpResponseMessage playlistDatabaseResponse = await HttpClientServer.PostAsJsonAsync("api/playlists/add", PlaylistSongList);
                if (playlistDatabaseResponse.IsSuccessStatusCode)
                {
                    
                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public string[] ReadPlaylistFromLine(int startPosition)
        {
            List<string> linesList = new List<string>();
            List<string> playlistAllLines = new List<string>();
            string playlistPath = VdjDirectory + "\\Playlists\\myplaylist.m3u";
            PlaylistDate = System.IO.File.GetLastWriteTime(playlistPath);
            using (var fileStream = new FileStream(playlistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream, System.Text.Encoding.Default))
            {
                string tmpLine = null;

                while ((tmpLine = streamReader.ReadLine()) != null)
                {
                    playlistAllLines.Add(tmpLine);                    
                }
            }

            for (int i = startPosition; i < playlistAllLines.Count(); i++)
            {
                linesList.Add(playlistAllLines[i]);
                PlaylistLinePosition++;
            }

            return linesList.ToArray();
        }

        public async Task UpdatePlaylist()
        {
            int counterPlayedSongs = 0;
            List<Playlist> playedSongsList = new List<Playlist>();
            for (int i = 0; i < PlaylistSongList.Count; i++)
            {
                if (PlaylistSongList[i].WasPlayed == false)
                {
                    break;
                }
                playedSongsList.Add(PlaylistSongList[i]);
                counterPlayedSongs++;
            }
            SongInPlaylistId = counterPlayedSongs + 1;
            counterPlayedSongs = counterPlayedSongs * 2;
            PlaylistSongList.Clear();
            PlaylistSongList.AddRange(playedSongsList);
            await AddSongsToPlaylistDatabase(counterPlayedSongs);
        }

        public async Task FirstRequestDatabase()
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/requests");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {

                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public async Task FirstChatDatabase()
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/Chats");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {
                    HttpResponseMessage firstChatResponse = await HttpClientServer.PostAsJsonAsync("api/Chats/first", "Welcome on chat");
                    if (firstChatResponse.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        ServerStatus = "Server fail, try again!";
                        MessageBox.Show("Server fail, try again!");
                    }
                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }

        public void GetRequestsandTop5()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        List<Request> requestListToAdd = new List<Request>();

                        HttpResponseMessage requestResponse = await HttpClientServer.GetAsync($"api/Requests/{LastRequestId}");

                        if (requestResponse.IsSuccessStatusCode)
                        {
                            string jsonRequestResponse = await requestResponse.Content.ReadAsStringAsync();
                            requestListToAdd = JsonConvert.DeserializeObject<List<Request>>(jsonRequestResponse);
                            LastRequestId = LastRequestId + requestListToAdd.Count;
                        }
                        else
                        {
                            ServerStatus = "Server fail to refresh!";
                        }

                        for (int i = 0; i < requestListToAdd.Count; i++)
                        {
                            RequestRich requestRich = new RequestRich(requestListToAdd[i]);
                            Song requestedSong = SongList.Find(x => x.Id == requestListToAdd[i].SongId);
                            requestRich.Title = requestedSong.Title;
                            requestRich.LeadAuthor = requestedSong.LeadAuthor;
                            RichRequestObservable.Add(requestRich);
                        }

                        requestListToAdd.Clear();

                        List<VotedSong> votesSongList = new List<VotedSong>();

                        HttpResponseMessage votesResponse = await HttpClientServer.GetAsync("api/songs/votes");

                        if (votesResponse.IsSuccessStatusCode)
                        {
                            string jsonVotes = await votesResponse.Content.ReadAsStringAsync();
                            votesSongList = JsonConvert.DeserializeObject<List<VotedSong>>(jsonVotes);
                        }
                        else
                        {
                            ServerStatus = "Server fail to refresh!";
                        }

                        Top5Text = "Top songs:";

                        for (int i = 0; i < 5; i++)
                        {
                            int topSongId = votesSongList[i].SongId;
                            Song top5Song = SongList.Find(x => x.Id == topSongId);
                            string songToAdd = "Votes: " + votesSongList[i].Votes + " " + top5Song.Title + " by " + top5Song.LeadAuthor;
                            Top5Text = Top5Text + "\n\n" + songToAdd;
                        }

                        votesSongList.Clear();

                        List<VotedSong> moreVotesSongList = new List<VotedSong>();

                        HttpResponseMessage moreVotesResponse = await HttpClientServer.GetAsync("api/songs/morevotes");

                        if (moreVotesResponse.IsSuccessStatusCode)
                        {
                            string jsonMoreVotes = await moreVotesResponse.Content.ReadAsStringAsync();
                            moreVotesSongList = JsonConvert.DeserializeObject<List<VotedSong>>(jsonMoreVotes);
                        }
                        else
                        {
                            ServerStatus = "Server fail to refresh!";
                        }

                        Top10Text = "Top songs:";

                        for (int i = 0; i < 10; i++)
                        {
                            int top10SongId = moreVotesSongList[i].SongId;
                            Song top10Song = SongList.Find(x => x.Id == top10SongId);
                            string song10ToAdd = "Votes: " + moreVotesSongList[i].Votes + " " + top10Song.Title + " by " + top10Song.LeadAuthor;
                            Top10Text = Top10Text + "\n\n" + song10ToAdd;
                        }

                        moreVotesSongList.Clear();

                        HttpResponseMessage chatResponse = await HttpClientServer.GetAsync($"api/Chats/{LastChatId}");

                        if (chatResponse.IsSuccessStatusCode)
                        {
                            string jsonChatResponse = await chatResponse.Content.ReadAsStringAsync();
                            List<string> chatGot = JsonConvert.DeserializeObject<List<string>>(jsonChatResponse);
                            if (chatGot.Count == 0)
                            {

                            }
                            else
                            {
                                for (int i = 0; i < chatGot.Count; i++)
                                {
                                    ChatText = ChatText + "\n\n" + chatGot[i];
                                }

                                LastChatId = LastChatId + chatGot.Count;
                            }
                        }
                        else
                        {
                            ServerStatus = "Server fail to refresh!";
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error while loading data!");
                    }
                    
                    await Task.Delay(TimeSpan.FromMilliseconds(5000));
                }
            });
        }

        public async Task FirstCurrentSongsDatabase()
        {
            try
            {
                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/CurrentSongs");

                if (deleteHttpResponse.IsSuccessStatusCode)
                {

                }
                else
                {
                    ServerStatus = "Server fail, try again!";
                    MessageBox.Show("Server fail, try again!");
                }

                TodaysDate = DateTime.Now;
                TodaysDate = TodaysDate.AddHours(-8);
                string dateString = TodaysDate.ToString("yyyy-MM-dd");
                string historyPath = VdjDirectory + "\\History\\";
                SongHistoryPath = historyPath + dateString + ".m3u";
                bool checkIfOldSongPlayed = System.IO.File.Exists(SongHistoryPath);
                if (checkIfOldSongPlayed == true)
                {
                    System.IO.File.Delete(SongHistoryPath);
                }
            }
            catch
            {
                IsServerStartButtonEnabled = true;
                ServerStatus = "Server fail, try again!";
                MessageBox.Show("Server fail, try again!");
            }
        }
        
        public void GetCurrentSong()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        string playlistPath = VdjDirectory + "\\Playlists\\myplaylist.m3u";
                        DateTime lastTimeModifiedPlaylist = System.IO.File.GetLastWriteTime(playlistPath);
                        if (lastTimeModifiedPlaylist != PlaylistDate)
                        {
                            PlaylistDate = lastTimeModifiedPlaylist;
                            await UpdatePlaylist();
                        }

                        bool checkIfSongPlayed = System.IO.File.Exists(SongHistoryPath);
                        if (checkIfSongPlayed == true)
                        {
                            DateTime lastTimeModified = System.IO.File.GetLastWriteTime(SongHistoryPath);

                            if (lastTimeModified != TodaysDate)
                            {
                                TodaysDate = lastTimeModified;

                                List<string> historyAllLines = new List<string>();
                                using (var fileStream = new FileStream(SongHistoryPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                using (var streamReader = new StreamReader(fileStream, System.Text.Encoding.Default))
                                {
                                    string tmpLine = null;

                                    while ((tmpLine = streamReader.ReadLine()) != null)
                                    {
                                        if (tmpLine.StartsWith("#EXT"))
                                        {

                                        }
                                        else
                                        {
                                            historyAllLines.Add(tmpLine);
                                        }
                                    }
                                }

                                string currentSongPath = historyAllLines.Last();

                                string songInfo = currentSongPath.Remove(0, currentSongPath.LastIndexOf('\\') + 1);
                                songInfo = songInfo.Remove(songInfo.Length - 4);
                                int tmpSeparator = songInfo.IndexOf("-");
                                string leadAuthor = songInfo.Remove(tmpSeparator - 1);
                                string[] leadAuthorArray = leadAuthor.Split(',');
                                string title = songInfo.Remove(0, tmpSeparator + 2);

                                if (title.Contains("(feat"))
                                {
                                    int tmpFeat = title.IndexOf("(feat");
                                    title = title.Remove(tmpFeat - 1);
                                }

                                for (int i1 = 0; i1 < SongList.Count; i1++)
                                {
                                    if (leadAuthorArray[0] == SongList[i1].LeadAuthorList[0] && title == SongList[i1].Title)
                                    {
                                        CurrentSongId = SongList[i1].Id;
                                        break;
                                    }
                                }

                                HttpResponseMessage deleteHttpResponse = await HttpClientServer.DeleteAsync("api/CurrentSongs");

                                if (deleteHttpResponse.IsSuccessStatusCode)
                                {
                                    HttpResponseMessage addSongResponse = await HttpClientServer.PostAsJsonAsync("api/CurrentSongs/add", CurrentSongId);
                                    if (addSongResponse.IsSuccessStatusCode)
                                    {
                                        if(IsCheckBoxChecked == true)
                                        {
                                            for (int z1 = 0; z1 < RichRequestObservable.Count; z1++)
                                            {
                                                if (PreviousSongToDeleteId == RichRequestObservable[z1].SongId)
                                                {
                                                    RichRequestObservable.RemoveAt(z1);
                                                }
                                            }
                                        }
                                        PreviousSongToDeleteId = CurrentSongId;
                                    }
                                    else
                                    {
                                        ServerStatus = "Server fail, try again!";
                                        MessageBox.Show("Server fail, try again!");
                                    }
                                }
                                else
                                {
                                    ServerStatus = "Server fail, try again!";
                                    MessageBox.Show("Server fail, try again!");
                                }

                                for (int i2 = 0; i2 < PlaylistSongList.Count; i2++)
                                {
                                    if (PlaylistSongList[i2].WasPlayed == false)
                                    {
                                        if (PlaylistSongList[i2].SongId == CurrentSongId)
                                        {
                                            PlaylistSongList[i2].WasPlayed = true;

                                            HttpResponseMessage updatePlaylistResponse = await HttpClientServer.PutAsJsonAsync("api/Playlists/update", i2);
                                            if (updatePlaylistResponse.IsSuccessStatusCode)
                                            {

                                            }
                                            else
                                            {
                                                ServerStatus = "Server fail, try again!";
                                                MessageBox.Show("Server fail, try again!");
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error while loading data!");
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(3000));
                }
            });
        }

        public void ShowMessageBox(object obj)
        {
            if ((obj is RequestRich))
            {
                RequestRich dedicationRequest = obj as RequestRich;
                MessageBox.Show(dedicationRequest.Dedication);
            }
        }

        public void DeleteFromGridMethod(object obj)
        {
            if ((obj is RequestRich))
            {
                RequestRich dedicationRequest = obj as RequestRich;
                for (int z1 = 0; z1 < RichRequestObservable.Count; z1++)
                {
                    if (dedicationRequest.Id == RichRequestObservable[z1].Id)
                    {
                        RichRequestObservable.RemoveAt(z1);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
