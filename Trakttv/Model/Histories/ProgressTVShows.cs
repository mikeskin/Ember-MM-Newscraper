namespace Trakttv.TraktAPI.Model.TVShows
{
    public class WatchedTVShowProgress
    {
        public TVShow Show { get; set; }
        public int EpisodesAired { get; set; }
        public int EpisodesWatched { get; set; }
        public int EpisodePlaycount { get; set; }
        public string LastWatchedEpisode { get; set; }      
    }
}
