using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Trakttv.TraktAPI.Model;
using Trakttv.TraktAPI.Model.General;
using Trakttv.TraktAPI.Model.Histories;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;
using NLog;

namespace Trakttv
{

    /// <summary>
    /// Class for communication with Trakt API
    /// </summary>
    public class TrakttvAPI
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region Communication Eventhandler
        // these events are used for handling data transfer between trakt <-> client
        internal delegate void OnDataSendDelegate(string url, string postData);
        internal delegate void OnDataReceivedDelegate(string response);
        internal delegate void OnDataErrorDelegate(string error);

        internal static event OnDataSendDelegate OnDataSend;
        internal static event OnDataReceivedDelegate OnDataReceived;
        internal static event OnDataErrorDelegate OnDataError;
        #endregion

        #region Communication Client <-> Trakttv Webservice

        /// <summary>
        /// Gets a User Authentication object
        /// </summary>       
        /// <returns>The User Authentication json string</returns>
        private static string GetUserAuthentication()
        {
            return new TraktAuthentication { Username = TraktSettings.Username, Password = TraktSettings.Password }.ToJSON();
        }

        /// <summary>
        /// Login to trakt and request a token for user for all subsequent requests
        /// </summary>
        /// <returns></returns>
        public static TraktToken Login(string loginData = null)
        {
            // clear User Token if set
            TraktSettings.Token = null;

           var response = SENDToTrakt(TraktURIs.Login, loginData ?? GetUserAuthentication(), false);
            return response.FromJSON<TraktToken>();
        }


        // Changes for v2 Trakt.tv API:
       // Since we are working with token, old transmit method could not be used anymore. 2 new methods: READFromTrakt and SENDToTrakt

        /// <summary>
        ///  GET Requests to trakt.tv API
        /// </summary>
        /// <param name="address">The URI to use</param>
        /// <param name="type">The type of request GET or DELETE, default: GET</param>
        /// <param name="oAuth">The token needed or not, default: yes</param>
        static string READFromTrakt(string address, string type = "GET", bool oAuth = true)
        
        {
            logger.Info("[READFromTrakt] Address: " + address);

            // no SSL for now... -> faster
            //address = address.Replace("https://", "http://");
            if (OnDataSend != null)
                OnDataSend(address, null);

            var request = WebRequest.Create(address) as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = type;
            request.ContentLength = 0;
            // set request timeout to 15s
            request.Timeout = 15000;
            request.ContentType = "application/json";
            request.UserAgent = TraktSettings.UserAgent;

            // v2 API, add required headers
            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", TraktSettings.ApiKey);

            // if we want to get all data, we need oAuth
            if (oAuth)
            {
                request.Headers.Add("trakt-user-login", TraktSettings.Username ?? string.Empty);
                //logger.Info("[READFromTrakt] trakt-user-login: " + TraktSettings.Username);
                request.Headers.Add("trakt-user-token", TraktSettings.Token ?? string.Empty);
                //logger.Info("[READFromTrakt] trakt-user-token: " + TraktMethods.MaskSensibleString(TraktSettings.Token));  
            }
            logger.Info("[READFromTrakt] Header: " + request.Headers);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                logger.Info("[READFromTrakt] Waiting for response...");
                if (response == null)
                {
                   logger.Info("[READFromTrakt] Response is null");
                    return null;
                }
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string strResponse = reader.ReadToEnd();
                logger.Info("[READFromTrakt] Response: " + strResponse);
                if (type == "DELETE")
                {
                    strResponse = response.StatusCode.ToString();
                }

                if (OnDataReceived != null)
                    OnDataReceived(strResponse);
  
                stream.Close();
                reader.Close();
                response.Close();

                return strResponse;
            }
            catch (WebException ex)
            {  
                string errorMessage = ex.Message;
                logger.Error("[READFromTrakt] Error during Request! ", ex);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    errorMessage = string.Format("API error! Code = '{0}', Description = '{1}'", (int)response.StatusCode, response.StatusDescription);
                    logger.Error(errorMessage);
                }

                if (OnDataError != null)
                    OnDataError(ex.Message);

                return null;
            }
        }

        /// <summary>
        /// POST Requests to trakt.tv API
        /// </summary>
        /// <param name="address">The URI to use</param>
        /// <param name="uploadstring">The text to post</param>
        /// <param name="oAuth">The token needed or not, default: yes</param>
        /// <param name="method">The type of request POST or PUT, default: POST</param>
        static string SENDToTrakt(string address, string uploadstring, bool oAuth = true, string method = "POST")
        
        {

            // address = address.Replace("https://", "http://");
            logger.Info("[SENDToTrakt] Address: " + address);
            // logger.Info("[SENDToTrakt] Post: " + uploadstring);

          
            if (OnDataSend != null && oAuth)
                OnDataSend(address, uploadstring);

            if (uploadstring == null)
                uploadstring = string.Empty;

            byte[] data = new UTF8Encoding().GetBytes(uploadstring);

            var request = WebRequest.Create(address) as HttpWebRequest;
            request.KeepAlive = true;

            request.Method = method;
            request.ContentLength = data.Length;
            request.Timeout = 15000;
            request.ContentType = "application/json";
            request.UserAgent = TraktSettings.UserAgent;

            // add required headers for authorisation
            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", TraktSettings.ApiKey);

            // if we're logging in, we don't need to add these headers
            if (!string.IsNullOrEmpty(TraktSettings.Token))
            {
                request.Headers.Add("trakt-user-login", TraktSettings.Username);
                request.Headers.Add("trakt-user-token", TraktSettings.Token);
            }

            try
            {
                // post to trakt
                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);
          
                // get the response
                var response = (HttpWebResponse)request.GetResponse();
                logger.Info("[SENDToTrakt] Waiting for response...");
                if (response == null)
                {
                    logger.Info("[SENDToTrakt] Response is null");
                    return null;
                }
                Stream responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                string strResponse = reader.ReadToEnd();
                // logger.Info("[SENDToTrakt] Response: " + strResponse);
                if (OnDataReceived != null)
                    OnDataReceived(strResponse);

                // cleanup
                postStream.Close();
                responseStream.Close();
                reader.Close();
                response.Close();

                return strResponse;
            }
            catch (WebException ex)
            {
                string errorMessage = ex.Message;
                logger.Error("[SENDToTrakt] Error during Request! ", ex);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    errorMessage = string.Format("API error! Code = '{0}', Description = '{1}'", (int)response.StatusCode, response.StatusDescription);
                }

                if (OnDataError != null)
                    OnDataError(ex.Message);

                return null;
            }
        }

        static string UPDATEOnTrakt(string address, string postData)
        {
            return SENDToTrakt(address, postData, true, "PUT");
        }

        static bool REMOVEFromTrakt(string address)
        {
            var response = READFromTrakt(address, "DELETE");
            return response != null;
        }

        //OUTDATED V1 logic, not used anymore and replaced by 2 methods (GET../SEND..) above
        /// <summary>
        ///  WebClient Logic - Communicates to and from Trakt
        /// </summary>
        /// <param name="address">The URI to use</param>
        /// <param name="data">The Data to Send</param>
        /// <returns>The response from Trakt</returns>
        //private static string Transmit(string address, string data)
        //{

        //   // address.Replace("  ", "");
        //    if (OnDataSend != null) OnDataSend(address, data);

        //    try
        //    {
        //        ServicePointManager.Expect100Continue = false;
        //        WebClient client = new WebClient();
        //        client.Encoding = Encoding.UTF8;
        //        client.Headers.Add("user-agent", TrakttvAPI.UserAgent);

        //        // wait for a response from the server
        //        string response = client.UploadString(address, data);

        //        // received data, pass it back
        //        if (OnDataReceived != null) OnDataReceived(response);
        //        return response;
        //    }
        //    catch (WebException e)
        //    {
        //        if (OnDataError != null) OnDataError(e.Message);

        //        if (e.Status == WebExceptionStatus.ProtocolError)
        //        {
        //            var response = ((HttpWebResponse)e.Response);
        //            try
        //            {
        //                using (var stream = response.GetResponseStream())
        //                {
        //                    using (var reader = new StreamReader(stream))
        //                    {
        //                        return reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch { }
        //        }

        //        // create a proper response object
        //        TraktResponse error = new TraktResponse
        //        {
        //            Status = "failure",
        //            Error = e.Message
        //        };
        //        // not using at moment
        //        //  throw new TraktException(error.Message);
        //        return error.ToJSON();

        //    }
        //}


        #endregion

            

        public static TraktResponse AddToHistory(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryAdd, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse AddToHistoryMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return AddToHistory(items);
        }

        public static TraktResponse AddToHistoryEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return AddToHistory(items);
        }

        public static TraktResponse AddToHistoryShow(TVShow show) 
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return AddToHistory(items);
        }

        public static TraktResponse AddRatings(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDRatingsAdd, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse AddRatingToMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return AddRatings(items);
        }

        public static TraktResponse AddRatingToEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return AddRatings(items);
        }

        public static TraktResponse AddRatingToShow(TVShow show)
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return AddRatings(items);
        }

        public static TraktResponse RemoveFromCollection(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDCollectionRemove, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse RemoveFromCollectionMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return RemoveFromCollection(items);
        }

        public static TraktResponse RemoveFromCollectionEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return RemoveFromCollection(items);
        }

        public static TraktResponse RemoveFromCollectionShow(TVShow show)
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return RemoveFromCollection(items);
        }

        public static TraktResponse RemoveFromHistory(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryRemove, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse RemoveFromHistoryMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return RemoveFromHistory(items);
        }

        public static TraktResponse RemoveFromHistoryEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return RemoveFromHistory(items);
        }

        public static TraktResponse RemoveFromHistoryID(TraktId id)
        {
            var items = new TraktSynchronize
            {
                IDs = new List<TraktId>() { id }
            };

            return RemoveFromHistory(items);
        }

        public static TraktResponse RemoveFromHistoryShow(TVShow show)
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return RemoveFromHistory(items);
        }

        public static TraktResponse RemoveFromWatchlist(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryRemove, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse RemoveFromWatchlistMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return RemoveFromWatchlist(items);
        }

        public static TraktResponse RemoveFromWatchlistEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return RemoveFromWatchlist(items);
        }

        public static TraktResponse RemoveFromWatchlistShow(TVShow show)
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return RemoveFromWatchlist(items);
        }

        public static TraktResponse RemoveRatings(TraktSynchronize items)
        {
            var response = SENDToTrakt(TraktURIs.SENDRatingsRemove, items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse RemoveRatingFromMovie(Movie movie)
        {
            var items = new TraktSynchronize
            {
                Movies = new List<Movie>() { movie }
            };

            return RemoveRatings(items);
        }

        public static TraktResponse RemoveRatingFromEpisode(TVEpisode episode)
        {
            var items = new TraktSynchronize
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return RemoveRatings(items);
        }

        public static TraktResponse RemoveRatingFromShow(TVShow show)
        {
            var items = new TraktSynchronize
            {
                Shows = new List<TVShow>() { show }
            };

            return RemoveRatings(items);
        }




        #region GET Collection (Movie/Episodes)

        public static IEnumerable<MovieCollected> GetCollectedMovies()
        {
            var response = READFromTrakt(TraktURIs.GETCollectionMovies);

            if (response == null) return null;
            return response.FromJSONArray<MovieCollected>();
        }

        public static IEnumerable<TVEpisodeCollected> GetCollectedEpisodes()
        {
            var response = READFromTrakt(TraktURIs.GETCollectionEpisodes);

            if (response == null) return null;
            return response.FromJSONArray<TVEpisodeCollected>();
        }

        #endregion

        #region GET Watched Movies/Episodes

        /// <summary>
        /// Returns list of watched movies
        /// </summary>
        public static IEnumerable<WatchedMoviesResponse> GetWatchedMovies()
        {
            var response = READFromTrakt(TraktURIs.GETWatchedMovies);

            if (response == null) return null;
            return response.FromJSONArray<WatchedMoviesResponse>();
        }

        /// <summary>
        /// Returns list of watched episodes
        /// </summary>
        public static IEnumerable<WatchedTVShowsResponse> GetWatchedTVShows()
        {
            var response = READFromTrakt(TraktURIs.GETWatchedTVShows);

            if (response == null) return null;
            return response.FromJSONArray<WatchedTVShowsResponse>();
        }

        /// <summary>
        /// Returns a more detailed by type and limit
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="type">movies / shows / seasons / episodes</param>
        /// <param name="limit">Limit</param>
        /// <returns></returns>
        public static IEnumerable<HistoryResponse> GetUsersMovieWatchedHistory(string username = "", string type = "movies", int limit = 100)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETHistoryByTypeExtended, username, type, limit));

            if (response == null) return null;
            return response.FromJSONArray<HistoryResponse>();
        }

        #endregion

        #region GET List(s)

        /// <summary>
        /// Returns a list of lists created by user
        /// </summary>
        public static IEnumerable<TraktListDetail> GetUserLists(string username)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETUserLists, username));
            return response.FromJSONArray<TraktListDetail>();
        }

        /// <summary>
        /// Returns the contents of a list
        /// </summary>
        public static IEnumerable<TraktListItem> GetUserListItems(string username, string listId, string extendedInfoParams = "min")
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETUserListItems, username, listId, extendedInfoParams));
            return response.FromJSONArray<TraktListItem>();
        }

       #endregion

        #region GET Ratings
        /// <summary>
        /// Returns the users rated movies
        /// </summary>
        public static IEnumerable<TraktMovieRated> GetRatedMovies()
        {
            var response = READFromTrakt(TraktURIs.GETRatedMovies);

            if (response == null) return null;
            return response.FromJSONArray<TraktMovieRated>();
        }

        /// <summary>
        /// Returns the users rated episodes
        /// </summary>
        public static IEnumerable<TraktEpisodeRated> GetRatedEpisodes()
        {
            var response = READFromTrakt(TraktURIs.GETRatedEpisodes);
            return response.FromJSONArray<TraktEpisodeRated>();
        }

        /// <summary>
        /// Returns the users rated shows
        /// </summary>
        public static IEnumerable<TraktShowRated> GetRatedShows()
        {
            var response = READFromTrakt(TraktURIs.GETRatedShows);
            return response.FromJSONArray<TraktShowRated>();
        }

        /// <summary>
        /// Returns the users rated seasons
        /// </summary>
        public static IEnumerable<TraktSeasonRated> GetRatedSeasons()
        {
            var response = READFromTrakt(TraktURIs.GETRatedSeasons);
            return response.FromJSONArray<TraktSeasonRated>();
        }

        #endregion

        #region GET Community Ratings

        /// <summary>
        /// Returns rating of a specific tv show
        /// <param name="MovieID">The ID of the movie</param>
        /// </summary>
        public static TraktRating GetMovieRating(string MovieID)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETMovieRating, MovieID));
            return response.FromJSON<TraktRating>();
        }


        /// <summary>
        /// Returns rating of a specific tv show
        /// <param name="ShowID">The ID of the show</param>
        /// </summary>
        public static TraktRating GetShowRating(string ShowID)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETShowRating, ShowID));
            return response.FromJSON<TraktRating>();
        }

        /// <summary>
        /// Returns rating of a season
        /// <param name="ShowID">The ID of the show</param>
        /// <param name="Seasonnumber">The number of season</param>
        /// </summary>
        public static TraktRating GetSeasonRating(string ShowID, int Seasonnumber)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETSeasonRating, ShowID, Seasonnumber));
            return response.FromJSON<TraktRating>();
        }

        /// <summary>
        /// Returns rating of a specific episode
        /// <param name="ShowID">The ID of the show</param>
        /// <param name="Seasonnumber">The number of season</param>
        /// <param name="Episodenumber">The episode number</param>
        /// </summary>
        public static TraktRating GetEpisodeRating(string ShowID, int Seasonnumber, int Episodenumber)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETEpisodeRating, ShowID, Seasonnumber, Episodenumber));
            return response.FromJSON<TraktRating>();
        }

        #endregion

        #region GET ShowProgress
        /// <summary>
        /// Returns progress of a specific show
        /// <param name="ID">The ID of the show</param>
        /// </summary>
        public static TVShowProgress GetProgressShow(string ID)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETProgressShow, ID));

            if (response == null) return null;
            return response.FromJSON<TVShowProgress>();
        }

        #endregion

        #region GET Watchlists (Movies/TVShows/Episodes)

        /// <summary>
        /// Returns the users watchlists of movies
        /// </summary>
        /// <param name="user">The user to get</param>
        public static IEnumerable<TraktMovieWatchList> GetWatchListMovies(string username, string extendedInfoParams = "min")
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETWatchlistMovies, username, extendedInfoParams));
            return response.FromJSONArray<TraktMovieWatchList>();
        }

        /// <summary>
        /// Returns the users watchlists of shows
        /// </summary>
        /// <param name="user">The user to get</param>
        public static IEnumerable<TraktShowWatchList> GetWatchListShows(string username)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETWatchlistShows, username));
            return response.FromJSONArray<TraktShowWatchList>();
        }

        /// <summary>
        /// Returns the users watchlists of episodes
        /// </summary>
        /// <param name="user">The user to get</param>
        public static IEnumerable<TraktEpisodeWatchList> GetWatchListEpisodes(string username)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETWatchlistEpisodes, username));
            return response.FromJSONArray<TraktEpisodeWatchList>();
        }

        #endregion

        #region GET Friends/Followers
        /// <summary>
        /// Returns a list of Friends for current user
        /// Friends are a two-way relationship ie. both following each other
        /// </summary>
        public static IEnumerable<TraktNetworkFriend> GetNetworkFriends()
        {
            return GetNetworkFriends(TraktSettings.Username);
        }
        public static IEnumerable<TraktNetworkFriend> GetNetworkFriends(string username)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETNetworkFriends, username));
            return response.FromJSONArray<TraktNetworkFriend>();
        }

        /// <summary>
        /// Returns a list of people the current user follows
        /// </summary>
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowing()
        {
            return GetNetworkFollowing(TraktSettings.Username);
        }
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowing(string username)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETNetworkFollowing, username));
            return response.FromJSONArray<TraktNetworkUser>();
        }
        #endregion

        #region GET Comments
        /// <summary>
        /// Get comments of user
        /// </summary>
        /// <param name="username">Username of commentator</param>
        /// <param name="comment_type">Possible values: all, reviews, shouts</param>
        /// <param name="type">Possible values: all, movies, shows, seasons, episodes, lists</param>
        public static IEnumerable<TraktCommentItem> GetComments(string username, string comment_type, string type)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETUserComments, username, comment_type, type));
            return response.FromJSONArray<TraktCommentItem>();
        }

         /// <summary>
        /// Get replies for a users comment
        /// </summary>
        /// <param name="idcomment">ID of the comment</param>
        public static IEnumerable<TraktCommentItem> GetRepliesForComment(string idcomment)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETCommentReplies, idcomment));
            return response.FromJSONArray<TraktCommentItem>();
        }

        /// <summary>
        /// Get comment/reply
        /// </summary>
        /// <param name="idcomment">ID of the comment</param>
        public static IEnumerable<TraktCommentItem> GetCommentOrReply(string idcomment)
        {
            var response = READFromTrakt(string.Format(TraktURIs.GETComment, idcomment));
            return response.FromJSONArray<TraktCommentItem>();
        }

        #endregion

        #region GET SearchResults
        /// <summary>
        /// Lookup an item by using a Trakt.tv ID or other external ID. This is helpful to get an items info including the Trakt.tv ID.
        /// </summary>
        /// <param name="idType">ID that matches with the type. Example: tt0848228.</param>
        /// <param name="id">Type of ID to lookup. Possible values: trakt-movie , trakt-show , trakt-episode , imdb , tmdb , tvdb , tvrage</param>
        public static IEnumerable<TraktSearchResult> SearchById(string idType, string id)
        {
            string response = READFromTrakt(string.Format(TraktURIs.GETSearchById, idType, id));
            return response.FromJSONArray<TraktSearchResult>();
        }

        #endregion




        #region POST User List

        /// <summary>
        /// Create new personal list on trakt
        /// </summary>
        /// <param name="user">The user to get</param>
        public static TraktListDetail AddUserList(TraktList list, string username)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDListAdd, username), list.ToJSON());
            return response.FromJSON<TraktListDetail>();
        }

        /// <summary>
        /// Updates existing list on trakt
        /// </summary>
        /// <param name="user">The user to get</param>
        public static TraktListDetail UpdateCustomList(TraktListDetail list, string username, string id)
        {
            var response = UPDATEOnTrakt(string.Format(TraktURIs.SENDListEdit, username,id), list.ToJSON());
            return response.FromJSON<TraktListDetail>();
        }

        /// <summary>
        /// Add new list entries on trakt
        /// </summary>
        /// <param name="user">The user to get</param>
        public static TraktResponse AddItemsToList(string username, string id, TraktSynchronize items)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDListItemsAdd, username, id), items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        /// <summary>
        /// Delete list items on trakt
        /// </summary>
        /// <param name="user">The user to get</param>
        public static TraktResponse RemoveItemsFromList(string username, string id, TraktSynchronize items)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDListItemsRemove, username, id), items.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        /// <summary>
        /// Delete existing list on trakt
        /// </summary>
        /// <param name="user">The user to get</param>
        public static bool RemoveUserList(string username, string listId)
        {
            return REMOVEFromTrakt(string.Format(TraktURIs.SENDListDelete, username, listId));
        }

        #endregion
        
        #region POST Watchlist

        public static TraktResponse AddMoviesToWatchlist(TraktSyncMovies movies)
        {
            var response = SENDToTrakt(TraktURIs.SENDWatchlistAdd, movies.ToJSON());
            return response.FromJSON<TraktResponse>();
        }
        public static TraktResponse AddMovieToWatchlist(Movie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<Movie>() { movie }
            };

            return AddMoviesToWatchlist(movies);
        }

        public static TraktResponse AddShowsToWatchlist(TraktSyncShows shows)
        {
            var response = SENDToTrakt(TraktURIs.SENDWatchlistAdd, shows.ToJSON());
            return response.FromJSON<TraktResponse>();
        }
        public static TraktResponse AddShowToWatchlist(TVShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TVShow>() { show }
            };

            return AddShowsToWatchlist(shows);
        }
        public static TraktResponse AddEpisodesToWatchlist(TraktSyncEpisodes episodes)
        {
            var response = SENDToTrakt(TraktURIs.SENDWatchlistAdd, episodes.ToJSON());
            return response.FromJSON<TraktResponse>();
        }
        public static TraktResponse AddEpisodeToWatchlist(TVEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TVEpisode>() { episode }
            };

            return AddEpisodesToWatchlist(episodes);
        }

        #endregion

        #region POST Comment/Reply/Like

        /// <summary>
        /// Like a comment
        /// </summary>
        /// <param name="commentID">A specific comment ID</param>
        public static bool AddLikeToComment(int commentID)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDCommentLike, commentID), null);
            return response != null;
        }

        /// <summary>
        /// UnLike a comment
        /// </summary>
        /// <param name="commentID">A specific comment ID</param>
        public static bool RemoveLikeFromComment(int commentID)
        {
            return REMOVEFromTrakt(string.Format(TraktURIs.SENDCommentLike, commentID));
        }

        /// <summary>
        /// Add a new comment to a movie
        /// </summary>
        /// <param name="moviecomment">contains all info necessary for posting a comment for a movie</param>
        public static TraktComment AddCommentForMovie(TraktCommentMovie moviecomment)
        {
            var response = SENDToTrakt(TraktURIs.SENDCommentAdd, moviecomment.ToJSON());
            return response.FromJSON<TraktComment>();
        }

        /// <summary>
        /// Update a single comment created within the last hour
        /// </summary>
        /// <param name="commentID">A specific comment ID</param>
        /// <param name="comment">Base comment object (spoiler info and text)</param>
        public static TraktComment UpdateComment(string commentID, TraktCommentBase comment)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDCommentUpdate, commentID), comment.ToJSON());
            return response.FromJSON<TraktComment>();
        }

        /// <summary>
        /// Delete a single comment/reply created within the last hour. This also effectively removes any replies this comment has
        /// </summary>
        /// <param name="commentID">A specific comment ID</param>
        public static bool RemoveCommentOrReply(int commentID)
        {
            return REMOVEFromTrakt(string.Format(TraktURIs.SENDCommentDelete, commentID));
        }

        /// <summary>
        /// Add a new reply to an existing comment
        /// </summary>
        /// <param name="commentID">A specific comment ID</param>
        /// <param name="comment">Base comment object (spoiler info and text)</param>
        public static TraktComment AddReplyForComment(string commentID, TraktCommentBase comment)
        {
            var response = SENDToTrakt(string.Format(TraktURIs.SENDCommentReply, commentID), comment.ToJSON());
            return response.FromJSON<TraktComment>();
        }

        #endregion

        #region POST Watched History

        public static TraktResponse RemoveHistoryIDFromWatchedHistory(TraktSyncHistoryID HistoryID)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryRemove, HistoryID.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse AddShowsToWatchedHistoryEx(TraktSyncShowsEx shows)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryAdd, shows.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse AddShowsToWatchedHistoryEx(TraktSyncShowsWatchedEx shows)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryAdd, shows.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse RemoveShowsFromWatchedHistoryEx(TraktSyncShowsEx shows)
        {
            var response = SENDToTrakt(TraktURIs.SENDHistoryRemove, shows.ToJSON());
            return response.FromJSON<TraktResponse>();
        }

        public static TraktResponse AddShowToWatchedHistoryEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return AddShowsToWatchedHistoryEx(shows);
        }

        public static TraktResponse RemoveShowFromWatchedHistoryEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return RemoveShowsFromWatchedHistoryEx(shows);
        }

        #endregion

        #region Helper: Retrieve Show Seasons

        /// <summary>
        /// Return a list of seasons for a tv show
        /// </summary>
        /// <param name="title">The show search term, either (title-year seperate spaces with '-'), imdbid, tvdbid</param>

        public static IEnumerable<TVSeasonSummary> GetShowSeasons(string id)
        {
            var response = READFromTrakt(string.Format(TraktURIs.ShowSeasons, id));
            return response.FromJSONArray<TVSeasonSummary>();
        }


        /// <summary>
        /// Return a list of episodes for a tv show season
        /// </summary>
        /// <param name="title">The show search term, either (title-year seperate spaces with '-'), imdbid, tvdbid</param>
        /// <param name="season">The season, 0 for specials</param>
        public static IEnumerable<TVEpisodeSummary> GetSeasonEpisodes(string showId, string seasonId)
        {
            var response = READFromTrakt(string.Format(TraktURIs.SeasonEpisodes, showId, seasonId));
            return response.FromJSONArray<TVEpisodeSummary>();
        }






        public static IEnumerable<WatchedTVShowsResponseExtended> Test()
        {
            var response = READFromTrakt(TraktURIs.GETWatchedTVShowsExtended);

            if (response == null) return null;
            return response.FromJSONArray<WatchedTVShowsResponseExtended>();
        }


        #endregion


        #region TODO Missing API calls (currently not needed)


        #region GET User Data
        //public static TraktUserStatistics GetUserStatistics(string user)
        //{
        //    var response = GetFromTrakt(string.Format(TraktURIs.UserStats, user));
        //    return response.FromJSON<TraktUserStatistics>();
        //}

        //public static TraktUserSummary GetUserProfile(string user)
        //{
        //    var response = GetFromTrakt(string.Format(TraktURIs.UserProfile, user));
        //    return response.FromJSON<TraktUserSummary>();
        //}

        #endregion

        #region Friends / Network
        #endregion

        #region Trending
        #endregion

        #region Recommendation
        #endregion

        #region Summary
        #endregion

        #region Comments
        #endregion

        #region Search
        #endregion

        #region Activity
        #endregion

        #region Related
        #endregion

        #region Syncing
        #endregion

        #endregion


    }
}
