' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports EmberAPI
Imports NLog
Imports System.Diagnostics

Namespace TMDB

    Public Class clsScraperTMDB

#Region "Fields"

        Shared logger As Logger = LogManager.GetCurrentClassLogger()

        Private _TMDBApi As TMDbLib.Client.TMDbClient  'preferred language
        Private _TMDBApiE As TMDbLib.Client.TMDbClient 'english language
        Private _SpecialSettings As clsModuleTMDB.SpecialSettings
        Private _sPoster As String

#End Region 'Fields

#Region "Properties"

#End Region 'Properties

#Region "Events"

#End Region 'Events

#Region "Methods"

        Public Sub New(ByVal SpecialSettings As clsModuleTMDB.SpecialSettings)
            Try
                _SpecialSettings = SpecialSettings

                _TMDBApi = New TMDbLib.Client.TMDbClient(_SpecialSettings.APIKey)
                _TMDBApi.GetConfig()
                _TMDBApi.DefaultLanguage = _SpecialSettings.PrefLanguage

                If _SpecialSettings.FallBackEng Then
                    _TMDBApiE = New TMDbLib.Client.TMDbClient(_SpecialSettings.APIKey)
                    _TMDBApiE.GetConfig()
                    _TMDBApiE.DefaultLanguage = "en-US"
                Else
                    _TMDBApiE = _TMDBApi
                End If
            Catch ex As Exception
                logger.Error(ex, New StackFrame().GetMethod().Name)
            End Try
        End Sub

        Public Sub GetMovieID(ByVal DBMovie As Database.DBElement)
            Dim Movie As TMDbLib.Objects.Movies.Movie

            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
            APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(DBMovie.Movie.ID))

            Movie = APIResult.Result
            If Movie Is Nothing OrElse Movie.Id = 0 Then Return

            DBMovie.Movie.TMDBID = CStr(Movie.Id)
        End Sub

        Public Function GetMovieID(ByVal imdbID As String) As String
            Dim Movie As TMDbLib.Objects.Movies.Movie

            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
            APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(imdbID))

            Movie = APIResult.Result
            If Movie Is Nothing OrElse Movie.Id = 0 Then Return String.Empty

            Return CStr(Movie.Id)
        End Function

        Public Function GetMovieCollectionID(ByVal imdbID As String) As String
            Dim Movie As TMDbLib.Objects.Movies.Movie

            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
            APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(imdbID))

            Movie = APIResult.Result
            If Movie Is Nothing Then Return String.Empty

            If Movie.BelongsToCollection IsNot Nothing AndAlso Movie.BelongsToCollection.Id > 0 Then
                Return CStr(Movie.BelongsToCollection.Id)
            Else
                Return String.Empty
            End If
        End Function
        ''' <summary>
        '''  Scrape MovieDetails from TMDB
        ''' </summary>
        ''' <param name="strID">TMDBID or ID (IMDB ID starts with "tt") of movie to be scraped</param>
        ''' <param name="GetPoster">Scrape posters for the movie?</param>
        ''' <returns>True: success, false: no success</returns>
        Public Function GetInfo_Movie(ByVal strID As String, ByVal FilteredModifiers As Structures.ScrapeModifiers, ByVal FilteredOptions As Structures.ScrapeOptions) As MediaContainers.ScrapeResultsContainer
            If String.IsNullOrEmpty(strID) OrElse strID.Length < 2 Then Return Nothing

            Dim nScrapeResults As New MediaContainers.ScrapeResultsContainer

            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
            Dim APIResultE As Task(Of TMDbLib.Objects.Movies.Movie)

            If strID.Substring(0, 2).ToLower = "tt" Then
                'search movie by IMDB ID
                APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(strID, TMDbLib.Objects.Movies.MovieMethods.Credits Or TMDbLib.Objects.Movies.MovieMethods.Images Or TMDbLib.Objects.Movies.MovieMethods.Releases Or TMDbLib.Objects.Movies.MovieMethods.Videos))
                If _SpecialSettings.FallBackEng Then
                    APIResultE = Task.Run(Function() _TMDBApiE.GetMovieAsync(strID, TMDbLib.Objects.Movies.MovieMethods.Credits Or TMDbLib.Objects.Movies.MovieMethods.Images Or TMDbLib.Objects.Movies.MovieMethods.Releases Or TMDbLib.Objects.Movies.MovieMethods.Videos))
                Else
                    APIResultE = APIResult
                End If
            Else
                'search movie by TMDB ID
                APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(CInt(strID), TMDbLib.Objects.Movies.MovieMethods.Credits Or TMDbLib.Objects.Movies.MovieMethods.Images Or TMDbLib.Objects.Movies.MovieMethods.Releases Or TMDbLib.Objects.Movies.MovieMethods.Videos))
                If _SpecialSettings.FallBackEng Then
                    APIResultE = Task.Run(Function() _TMDBApiE.GetMovieAsync(CInt(strID), TMDbLib.Objects.Movies.MovieMethods.Credits Or TMDbLib.Objects.Movies.MovieMethods.Images Or TMDbLib.Objects.Movies.MovieMethods.Releases Or TMDbLib.Objects.Movies.MovieMethods.Videos))
                Else
                    APIResultE = APIResult
                End If
            End If

            Dim Result As TMDbLib.Objects.Movies.Movie = APIResult.Result
            Dim ResultE As TMDbLib.Objects.Movies.Movie = APIResultE.Result

            If (Result Is Nothing AndAlso Not _SpecialSettings.FallBackEng) OrElse (Result Is Nothing AndAlso ResultE Is Nothing) OrElse
                (Not Result.Id > 0 AndAlso Not _SpecialSettings.FallBackEng) OrElse (Not Result.Id > 0 AndAlso Not ResultE.Id > 0) Then
                logger.Error(String.Format("Can't scrape or movie not found: [0]", strID))
                Return Nothing
            End If

            nScrapeResults.Movie.Scrapersource = "TMDB"

            'IDs
            nScrapeResults.Movie.TMDBID = CStr(Result.Id)
            If Result.ImdbId IsNot Nothing Then nScrapeResults.Movie.ID = Result.ImdbId

            'Main NFO
            If FilteredModifiers.MainNFO Then
                'Cast (Actors)
                If FilteredOptions.bMainActors Then
                    If Result.Credits IsNot Nothing AndAlso Result.Credits.Cast IsNot Nothing Then
                        For Each aCast As TMDbLib.Objects.Movies.Cast In Result.Credits.Cast
                            nScrapeResults.Movie.Actors.Add(New MediaContainers.Person With {.Name = aCast.Name,
                                                                               .Role = aCast.Character,
                                                                               .URLOriginal = If(Not String.IsNullOrEmpty(aCast.ProfilePath), String.Concat(_TMDBApi.Config.Images.BaseUrl, "original", aCast.ProfilePath), String.Empty),
                                                                               .TMDB = CStr(aCast.Id)})
                        Next
                    End If
                End If

                'Certifications
                If FilteredOptions.bMainCertifications Then
                    If Result.Releases IsNot Nothing AndAlso Result.Releases.Countries IsNot Nothing AndAlso Result.Releases.Countries.Count > 0 Then
                        For Each cCountry In Result.Releases.Countries
                            If Not String.IsNullOrEmpty(cCountry.Certification) Then
                                Dim CertificationLanguage = APIXML.CertLanguagesXML.Language.FirstOrDefault(Function(l) l.abbreviation = cCountry.Iso_3166_1.ToLower)
                                If CertificationLanguage IsNot Nothing AndAlso CertificationLanguage.name IsNot Nothing AndAlso Not String.IsNullOrEmpty(CertificationLanguage.name) Then
                                    nScrapeResults.Movie.Certifications.Add(String.Concat(CertificationLanguage.name, ":", cCountry.Certification))
                                Else
                                    logger.Warn("Unhandled certification language encountered: {0}", cCountry.Iso_3166_1.ToLower)
                                End If
                            End If
                        Next
                    End If
                End If

                'Collection ID
                If FilteredOptions.bMainCollectionID Then
                    If Result.BelongsToCollection Is Nothing Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.BelongsToCollection IsNot Nothing Then
                            nScrapeResults.Movie.AddSet(New MediaContainers.SetDetails With {
                                          .ID = -1,
                                          .Order = -1,
                                          .Plot = String.Empty,
                                          .Title = ResultE.BelongsToCollection.Name,
                                          .TMDB = CStr(ResultE.BelongsToCollection.Id)})
                            nScrapeResults.Movie.TMDBColID = CStr(ResultE.BelongsToCollection.Id)
                        End If
                    Else
                        nScrapeResults.Movie.AddSet(New MediaContainers.SetDetails With {
                                      .ID = -1,
                                      .Order = -1,
                                      .Plot = String.Empty,
                                      .Title = ResultE.BelongsToCollection.Name,
                                      .TMDB = CStr(ResultE.BelongsToCollection.Id)})
                        nScrapeResults.Movie.TMDBColID = CStr(Result.BelongsToCollection.Id)
                    End If
                End If

                'Countries
                If FilteredOptions.bMainCountries Then
                    If Result.ProductionCountries IsNot Nothing AndAlso Result.ProductionCountries.Count > 0 Then
                        For Each aContry As TMDbLib.Objects.Movies.ProductionCountry In Result.ProductionCountries
                            nScrapeResults.Movie.Countries.Add(aContry.Name)
                        Next
                    End If
                End If

                'Director / Writer
                If FilteredOptions.bMainDirectors OrElse FilteredOptions.bMainWriters Then
                    If Result.Credits IsNot Nothing AndAlso Result.Credits.Crew IsNot Nothing Then
                        For Each aCrew As TMDbLib.Objects.General.Crew In Result.Credits.Crew
                            If FilteredOptions.bMainDirectors AndAlso aCrew.Department = "Directing" AndAlso aCrew.Job = "Director" Then
                                nScrapeResults.Movie.Directors.Add(aCrew.Name)
                            End If
                            If FilteredOptions.bMainWriters AndAlso aCrew.Department = "Writing" AndAlso (aCrew.Job = "Author" OrElse aCrew.Job = "Screenplay" OrElse aCrew.Job = "Writer") Then
                                nScrapeResults.Movie.Credits.Add(aCrew.Name)
                            End If
                        Next
                    End If
                End If

                'Genres
                If FilteredOptions.bMainGenres Then
                    Dim aGenres As List(Of TMDbLib.Objects.General.Genre) = Nothing
                    If Result.Genres Is Nothing OrElse (Result.Genres IsNot Nothing AndAlso Result.Genres.Count = 0) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Genres IsNot Nothing AndAlso ResultE.Genres.Count > 0 Then
                            aGenres = ResultE.Genres
                        End If
                    Else
                        aGenres = Result.Genres
                    End If

                    If aGenres IsNot Nothing Then
                        For Each tGenre As TMDbLib.Objects.General.Genre In aGenres
                            nScrapeResults.Movie.Genres.Add(tGenre.Name)
                        Next
                    End If
                End If

                'OriginalTitle
                If FilteredOptions.bMainOriginalTitle Then
                    If Result.OriginalTitle Is Nothing OrElse (Result.OriginalTitle IsNot Nothing AndAlso String.IsNullOrEmpty(Result.OriginalTitle)) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.OriginalTitle IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.OriginalTitle) Then
                            nScrapeResults.Movie.OriginalTitle = ResultE.OriginalTitle
                        End If
                    Else
                        nScrapeResults.Movie.OriginalTitle = Result.OriginalTitle
                    End If
                End If

                'Plot
                If FilteredOptions.bMainPlot Then
                    If Result.Overview Is Nothing OrElse (Result.Overview IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Overview)) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Overview IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Overview) Then
                            nScrapeResults.Movie.Plot = ResultE.Overview
                        End If
                    Else
                        nScrapeResults.Movie.Plot = Result.Overview
                    End If
                End If

                'Rating
                If FilteredOptions.bMainRating Then
                    nScrapeResults.Movie.Rating = CStr(Result.VoteAverage)
                    nScrapeResults.Movie.Votes = CStr(Result.VoteCount)
                End If

                'ReleaseDate
                If FilteredOptions.bMainRelease Then
                    Dim ScrapedDate As String = String.Empty
                    If Result.ReleaseDate Is Nothing OrElse (Result.ReleaseDate IsNot Nothing AndAlso String.IsNullOrEmpty(CStr(Result.ReleaseDate))) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.ReleaseDate IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(ResultE.ReleaseDate)) Then
                            ScrapedDate = CStr(ResultE.ReleaseDate)
                        End If
                    Else
                        ScrapedDate = CStr(Result.ReleaseDate)
                    End If
                    If Not String.IsNullOrEmpty(ScrapedDate) Then
                        Dim RelDate As Date
                        If Date.TryParse(ScrapedDate, RelDate) Then
                            'always save date in same date format not depending on users language setting!
                            nScrapeResults.Movie.ReleaseDate = RelDate.ToString("yyyy-MM-dd")
                        Else
                            nScrapeResults.Movie.ReleaseDate = ScrapedDate
                        End If
                    End If
                End If

                'Runtime
                If FilteredOptions.bMainRuntime Then
                    If Result.Runtime Is Nothing OrElse Result.Runtime = 0 Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Runtime IsNot Nothing Then
                            nScrapeResults.Movie.Runtime = CStr(ResultE.Runtime)
                        End If
                    Else
                        nScrapeResults.Movie.Runtime = CStr(Result.Runtime)
                    End If
                End If

                'Studios
                If FilteredOptions.bMainStudios Then
                    If Result.ProductionCompanies IsNot Nothing AndAlso Result.ProductionCompanies.Count > 0 Then
                        For Each cStudio In Result.ProductionCompanies
                            nScrapeResults.Movie.Studios.Add(cStudio.Name)
                        Next
                    End If
                End If

                'Tagline
                If FilteredOptions.bMainTagline Then
                    If Result.Tagline Is Nothing OrElse (Result.Tagline IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Tagline)) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Tagline IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Tagline) Then
                            nScrapeResults.Movie.Tagline = ResultE.Tagline
                        End If
                    Else
                        nScrapeResults.Movie.Tagline = Result.Tagline
                    End If
                End If

                'Title
                If FilteredOptions.bMainTitle Then
                    If Result.Title Is Nothing OrElse (Result.Title IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Title)) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Title IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Title) Then
                            nScrapeResults.Movie.Title = ResultE.Title
                        End If
                    Else
                        nScrapeResults.Movie.Title = Result.Title
                    End If
                End If

                'Trailer
                If FilteredOptions.bMainTrailer Then
                    Dim aTrailers As List(Of TMDbLib.Objects.General.Video) = Nothing
                    If Result.Videos Is Nothing OrElse (Result.Videos IsNot Nothing AndAlso Result.Videos.Results.Count = 0) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.Videos IsNot Nothing AndAlso ResultE.Videos.Results.Count > 0 Then
                            aTrailers = ResultE.Videos.Results
                        End If
                    Else
                        aTrailers = Result.Videos.Results
                    End If

                    If aTrailers IsNot Nothing AndAlso aTrailers.Count > 0 Then
                        For Each tTrailer In aTrailers
                            If YouTube.Scraper.IsAvailable("http://www.youtube.com/watch?hd=1&v=" & tTrailer.Key) Then
                                nScrapeResults.Movie.Trailer = "http://www.youtube.com/watch?hd=1&v=" & tTrailer.Key
                                Exit For
                            End If
                        Next
                    End If
                End If

                'Year
                If FilteredOptions.bMainYear Then
                    If Result.ReleaseDate Is Nothing OrElse (Result.ReleaseDate IsNot Nothing AndAlso String.IsNullOrEmpty(CStr(Result.ReleaseDate))) Then
                        If _SpecialSettings.FallBackEng AndAlso ResultE.ReleaseDate IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(ResultE.ReleaseDate)) Then
                            nScrapeResults.Movie.Year = CStr(ResultE.ReleaseDate.Value.Year)
                        End If
                    Else
                        nScrapeResults.Movie.Year = CStr(Result.ReleaseDate.Value.Year)
                    End If
                End If
            End If

            'MainFanart
            If (FilteredModifiers.MainExtrafanarts OrElse FilteredModifiers.MainExtrathumbs OrElse FilteredModifiers.MainFanart) AndAlso Result.Images.Backdrops IsNot Nothing Then
                For Each tImage In Result.Images.Backdrops
                    Dim newImage As New MediaContainers.Image With {
                            .Height = tImage.Height.ToString,
                            .Likes = 0,
                            .LongLang = If(String.IsNullOrEmpty(tImage.Iso_639_1), String.Empty, Localization.ISOGetLangByCode2(tImage.Iso_639_1)),
                            .Scraper = "TMDB",
                            .ShortLang = If(String.IsNullOrEmpty(tImage.Iso_639_1), String.Empty, tImage.Iso_639_1),
                            .URLOriginal = _TMDBApi.Config.Images.BaseUrl & "original" & tImage.FilePath,
                            .URLThumb = _TMDBApi.Config.Images.BaseUrl & "w300" & tImage.FilePath,
                            .VoteAverage = tImage.VoteAverage.ToString,
                            .VoteCount = tImage.VoteCount,
                            .Width = tImage.Width.ToString}

                    nScrapeResults.Images.MainFanarts.Add(newImage)
                Next
            End If

            'MainPoster
            If FilteredModifiers.MainPoster AndAlso Result.Images.Posters IsNot Nothing Then
                For Each tImage In Result.Images.Posters
                    Dim newImage As New MediaContainers.Image With {
                                .Height = tImage.Height.ToString,
                                .Likes = 0,
                                .LongLang = If(String.IsNullOrEmpty(tImage.Iso_639_1), String.Empty, Localization.ISOGetLangByCode2(tImage.Iso_639_1)),
                                .Scraper = "TMDB",
                                .ShortLang = If(String.IsNullOrEmpty(tImage.Iso_639_1), String.Empty, tImage.Iso_639_1),
                                .URLOriginal = _TMDBApi.Config.Images.BaseUrl & "original" & tImage.FilePath,
                                .URLThumb = _TMDBApi.Config.Images.BaseUrl & "w185" & tImage.FilePath,
                                .VoteAverage = tImage.VoteAverage.ToString,
                                .VoteCount = tImage.VoteCount,
                                .Width = tImage.Width.ToString}

                    nScrapeResults.Images.MainPosters.Add(newImage)
                Next
            End If

            'Main Trailer
            If FilteredModifiers.MainTrailer Then
                Dim aTrailers As List(Of TMDbLib.Objects.General.Video) = Nothing
                If Result.Videos Is Nothing OrElse (Result.Videos IsNot Nothing AndAlso Result.Videos.Results.Count = 0) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Videos IsNot Nothing AndAlso ResultE.Videos.Results.Count > 0 Then
                        aTrailers = ResultE.Videos.Results
                    End If
                Else
                    aTrailers = Result.Videos.Results
                End If

                If aTrailers IsNot Nothing AndAlso aTrailers.Count > 0 Then
                    For Each Video As TMDbLib.Objects.General.Video In aTrailers.Where(Function(f) f.Site = "YouTube")
                        Dim tLink As String = String.Format("http://www.youtube.com/watch?v={0}", Video.Key)
                        If YouTube.Scraper.IsAvailable(tLink) Then
                            Dim tName As String = YouTube.Scraper.GetVideoTitle(tLink)
                            nScrapeResults.Trailers.Add(New MediaContainers.Trailer With {
                                               .LongLang = If(String.IsNullOrEmpty(Video.Iso_639_1), String.Empty, Localization.ISOGetLangByCode2(Video.Iso_639_1)),
                                               .Quality = GetVideoQuality(Video.Size),
                                               .Scraper = "TMDB",
                                               .ShortLang = If(String.IsNullOrEmpty(Video.Iso_639_1), String.Empty, Video.Iso_639_1),
                                               .Source = Video.Site,
                                               .Title = tName,
                                               .Type = GetVideoType(Video.Type),
                                               .URLWebsite = tLink})
                        End If
                    Next
                End If
            End If

            Return nScrapeResults
        End Function

        Public Function GetMovieSetInfo(ByVal strID As String, ByVal FilteredOptions As Structures.ScrapeOptions, ByVal GetPoster As Boolean) As MediaContainers.MovieSet
            If String.IsNullOrEmpty(strID) OrElse Not Integer.TryParse(strID, 0) Then Return Nothing

            Dim nMovieSet As New MediaContainers.MovieSet

            Dim APIResult As Task(Of TMDbLib.Objects.Collections.Collection)
            Dim APIResultE As Task(Of TMDbLib.Objects.Collections.Collection)

            APIResult = Task.Run(Function() _TMDBApi.GetCollectionAsync(CInt(strID), _SpecialSettings.PrefLanguage))
            If _SpecialSettings.FallBackEng Then
                APIResultE = Task.Run(Function() _TMDBApiE.GetCollectionAsync(CInt(strID)))
            Else
                APIResultE = APIResult
            End If

            If APIResult Is Nothing OrElse APIResultE Is Nothing Then
                Return Nothing
            End If

            Dim Result As TMDbLib.Objects.Collections.Collection = APIResult.Result
            Dim ResultE As TMDbLib.Objects.Collections.Collection = APIResultE.Result

            If (Result Is Nothing AndAlso Not _SpecialSettings.FallBackEng) OrElse (Result Is Nothing AndAlso ResultE Is Nothing) OrElse
                (Not Result.Id > 0 AndAlso Not _SpecialSettings.FallBackEng) OrElse (Not Result.Id > 0 AndAlso Not ResultE.Id > 0) Then
                logger.Warn(String.Format("[TMDB_Data] [Abort] No API result for TMDB Collection ID [{0}]", strID))
                Return Nothing
            End If

            nMovieSet.TMDB = CStr(Result.Id)

            'Plot
            If FilteredOptions.bMainPlot Then
                If Result.Overview Is Nothing OrElse (Result.Overview IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Overview)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Overview IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Overview) Then
                        'nMovieSet.Plot = MovieSetE.Overview
                        nMovieSet.Plot = ResultE.Overview
                    End If
                Else
                    'nMovieSet.Plot = MovieSet.Overview
                    nMovieSet.Plot = Result.Overview
                End If
            End If

            'Posters (only for SearchResult dialog, auto fallback to "en" by TMDB)
            If GetPoster Then
                If Result.PosterPath IsNot Nothing AndAlso Not String.IsNullOrEmpty(Result.PosterPath) Then
                    _sPoster = String.Concat(_TMDBApi.Config.Images.BaseUrl, "w92", Result.PosterPath)
                Else
                    _sPoster = String.Empty
                End If
            End If

            'Title
            If FilteredOptions.bMainTitle Then
                If Result.Name Is Nothing OrElse (Result.Name IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Name)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Name IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Name) Then
                        'nMovieSet.Title = MovieSetE.Name
                        nMovieSet.Title = ResultE.Name
                    End If
                Else
                    'nMovieSet.Title = MovieSet.Name
                    nMovieSet.Title = Result.Name
                End If
            End If

            Return nMovieSet
        End Function
        ''' <summary>
        '''  Scrape TV Show details from TMDB
        ''' </summary>
        ''' <param name="strID">TMDB ID of tv show to be scraped</param>
        ''' <param name="GetPoster">Scrape posters for the movie?</param>
        ''' <returns>True: success, false: no success</returns>
        Public Function GetTVShowInfo(ByVal strID As String, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef FilteredOptions As Structures.ScrapeOptions, ByVal GetPoster As Boolean) As MediaContainers.TVShow
            If String.IsNullOrEmpty(strID) OrElse strID.Length < 2 Then Return Nothing

            Dim nTVShow As New MediaContainers.TVShow

            Dim APIResult As Task(Of TMDbLib.Objects.TvShows.TvShow)
            Dim APIResultE As Task(Of TMDbLib.Objects.TvShows.TvShow)

            'search movie by TMDB ID
            APIResult = Task.Run(Function() _TMDBApi.GetTvShowAsync(CInt(strID), TMDbLib.Objects.TvShows.TvShowMethods.ContentRatings Or TMDbLib.Objects.TvShows.TvShowMethods.Credits Or TMDbLib.Objects.TvShows.TvShowMethods.ExternalIds))
            If _SpecialSettings.FallBackEng Then
                APIResultE = Task.Run(Function() _TMDBApiE.GetTvShowAsync(CInt(strID), TMDbLib.Objects.TvShows.TvShowMethods.ContentRatings Or TMDbLib.Objects.TvShows.TvShowMethods.Credits Or TMDbLib.Objects.TvShows.TvShowMethods.ExternalIds))
            Else
                APIResultE = APIResult
            End If

            If APIResult Is Nothing OrElse APIResultE Is Nothing Then
                Return Nothing
            End If

            Dim Result As TMDbLib.Objects.TvShows.TvShow = APIResult.Result
            Dim ResultE As TMDbLib.Objects.TvShows.TvShow = APIResultE.Result

            If (Result Is Nothing AndAlso Not _SpecialSettings.FallBackEng) OrElse (Result Is Nothing AndAlso ResultE Is Nothing) OrElse
                (Not Result.Id > 0 AndAlso Not _SpecialSettings.FallBackEng) OrElse (Not Result.Id > 0 AndAlso Not ResultE.Id > 0) Then
                logger.Error(String.Format("Can't scrape or tv show not found: [{0}]", strID))
                Return Nothing
            End If

            nTVShow.Scrapersource = "TMDB"

            'IDs
            nTVShow.TMDB = CStr(Result.Id)
            If Result.ExternalIds.TvdbId IsNot Nothing Then nTVShow.TVDB = CStr(Result.ExternalIds.TvdbId)
            If Result.ExternalIds.ImdbId IsNot Nothing Then nTVShow.IMDB = Result.ExternalIds.ImdbId

            'Actors
            If FilteredOptions.bMainActors Then
                If Result.Credits IsNot Nothing AndAlso Result.Credits.Cast IsNot Nothing Then
                    For Each aCast As TMDbLib.Objects.TvShows.Cast In Result.Credits.Cast
                        nTVShow.Actors.Add(New MediaContainers.Person With {.Name = aCast.Name,
                                                                           .Role = aCast.Character,
                                                                           .URLOriginal = If(Not String.IsNullOrEmpty(aCast.ProfilePath), String.Concat(_TMDBApi.Config.Images.BaseUrl, "original", aCast.ProfilePath), String.Empty),
                                                                           .TMDB = CStr(aCast.Id)})
                    Next
                End If
            End If

            'Certifications
            If FilteredOptions.bMainCertifications Then
                If Result.ContentRatings IsNot Nothing AndAlso Result.ContentRatings.Results IsNot Nothing AndAlso Result.ContentRatings.Results.Count > 0 Then
                    For Each aCountry In Result.ContentRatings.Results
                        If Not String.IsNullOrEmpty(aCountry.Rating) Then
                            Dim CertificationLanguage = APIXML.CertLanguagesXML.Language.FirstOrDefault(Function(l) l.abbreviation = aCountry.Iso_3166_1.ToLower)
                            If CertificationLanguage IsNot Nothing AndAlso CertificationLanguage.name IsNot Nothing AndAlso Not String.IsNullOrEmpty(CertificationLanguage.name) Then
                                nTVShow.Certifications.Add(String.Concat(CertificationLanguage.name, ":", aCountry.Rating))
                            Else
                                logger.Warn("Unhandled certification language encountered: {0}", aCountry.Iso_3166_1.ToLower)
                            End If
                        End If
                    Next
                End If
            End If

            'Countries 'TODO: Change from OriginCountry to ProductionCountries (not yet supported by API)
            'If FilteredOptions.bMainCountry Then
            '    If Show.OriginCountry IsNot Nothing AndAlso Show.OriginCountry.Count > 0 Then
            '        For Each aCountry As String In Show.OriginCountry
            '            nShow.Countries.Add(aCountry)
            '        Next
            '    End If
            'End If

            'Creators
            If FilteredOptions.bMainCreators Then
                If Result.CreatedBy IsNot Nothing Then
                    For Each aCreator As TMDbLib.Objects.People.Person In Result.CreatedBy
                        nTVShow.Creators.Add(aCreator.Name)
                    Next
                End If
            End If

            'Genres
            If FilteredOptions.bMainGenres Then
                Dim aGenres As List(Of TMDbLib.Objects.General.Genre) = Nothing
                If Result.Genres Is Nothing OrElse (Result.Genres IsNot Nothing AndAlso Result.Genres.Count = 0) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Genres IsNot Nothing AndAlso ResultE.Genres.Count > 0 Then
                        aGenres = ResultE.Genres
                    End If
                Else
                    aGenres = Result.Genres
                End If

                If aGenres IsNot Nothing Then
                    For Each tGenre As TMDbLib.Objects.General.Genre In aGenres
                        nTVShow.Genres.Add(tGenre.Name)
                    Next
                End If
            End If

            'OriginalTitle
            If FilteredOptions.bMainOriginalTitle Then
                If Result.OriginalName Is Nothing OrElse (Result.OriginalName IsNot Nothing AndAlso String.IsNullOrEmpty(Result.OriginalName)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.OriginalName IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.OriginalName) Then
                        nTVShow.OriginalTitle = ResultE.OriginalName
                    End If
                Else
                    nTVShow.OriginalTitle = ResultE.OriginalName
                End If
            End If

            'Plot
            If FilteredOptions.bMainPlot Then
                If Result.Overview Is Nothing OrElse (Result.Overview IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Overview)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Overview IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Overview) Then
                        nTVShow.Plot = ResultE.Overview
                    End If
                Else
                    nTVShow.Plot = Result.Overview
                End If
            End If

            'Posters (only for SearchResult dialog, auto fallback to "en" by TMDB)
            If GetPoster Then
                If Result.PosterPath IsNot Nothing AndAlso Not String.IsNullOrEmpty(Result.PosterPath) Then
                    _sPoster = String.Concat(_TMDBApi.Config.Images.BaseUrl, "w92", Result.PosterPath)
                Else
                    _sPoster = String.Empty
                End If
            End If

            'Premiered
            If FilteredOptions.bMainPremiered Then
                Dim ScrapedDate As String = String.Empty
                If Result.FirstAirDate Is Nothing OrElse (Result.FirstAirDate IsNot Nothing AndAlso String.IsNullOrEmpty(CStr(Result.FirstAirDate))) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.FirstAirDate IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(ResultE.FirstAirDate)) Then
                        ScrapedDate = CStr(ResultE.FirstAirDate)
                    End If
                Else
                    ScrapedDate = CStr(Result.FirstAirDate)
                End If
                If Not String.IsNullOrEmpty(ScrapedDate) Then
                    Dim RelDate As Date
                    If Date.TryParse(ScrapedDate, RelDate) Then
                        'always save date in same date format not depending on users language setting!
                        nTVShow.Premiered = RelDate.ToString("yyyy-MM-dd")
                    Else
                        nTVShow.Premiered = ScrapedDate
                    End If
                End If
            End If

            'Rating
            If FilteredOptions.bMainRating Then
                nTVShow.Rating = CStr(Result.VoteAverage)
                nTVShow.Votes = CStr(Result.VoteCount)
            End If

            'Runtime
            If FilteredOptions.bMainRuntime Then
                If Result.EpisodeRunTime Is Nothing OrElse Result.EpisodeRunTime.Count = 0 Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.EpisodeRunTime IsNot Nothing AndAlso ResultE.EpisodeRunTime.Count > 0 Then
                        nTVShow.Runtime = CStr(ResultE.EpisodeRunTime.Item(0))
                    End If
                Else
                    nTVShow.Runtime = CStr(Result.EpisodeRunTime.Item(0))
                End If
            End If

            'Status
            If FilteredOptions.bMainStatus Then
                If Result.Status Is Nothing OrElse (Result.Status IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Status)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Status IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Status) Then
                        nTVShow.Status = ResultE.Status
                    End If
                Else
                    nTVShow.Status = Result.Status
                End If
            End If

            'Studios
            If FilteredOptions.bMainStudios Then
                If Result.Networks IsNot Nothing AndAlso Result.Networks.Count > 0 Then
                    For Each aStudio In Result.Networks
                        nTVShow.Studios.Add(aStudio.Name)
                    Next
                End If
            End If

            'Title
            If FilteredOptions.bMainTitle Then
                If Result.Name Is Nothing OrElse (Result.Name IsNot Nothing AndAlso String.IsNullOrEmpty(Result.Name)) Then
                    If _SpecialSettings.FallBackEng AndAlso ResultE.Name IsNot Nothing AndAlso Not String.IsNullOrEmpty(ResultE.Name) Then
                        nTVShow.Title = ResultE.Name
                    End If
                Else
                    nTVShow.Title = Result.Name
                End If
            End If

            ''Trailer
            'If Options.bTrailer Then
            '    Dim aTrailers As List(Of TMDbLib.Objects.TvShows.Video) = Nothing
            '    If Show.Videos Is Nothing OrElse (Show.Videos IsNot Nothing AndAlso Show.Videos.Results.Count = 0) Then
            '        If _MySettings.FallBackEng AndAlso ShowE.Videos IsNot Nothing AndAlso ShowE.Videos.Results.Count > 0 Then
            '            aTrailers = ShowE.Videos.Results
            '        End If
            '    Else
            '        aTrailers = Show.Videos.Results
            '    End If

            '    If aTrailers IsNot Nothing AndAlso aTrailers IsNot Nothing AndAlso aTrailers.Count > 0 Then
            '        nShow.Trailer = "http://www.youtube.com/watch?hd=1&v=" & aTrailers.Item(0).Key
            '    End If
            'End If

            'Seasons and Episodes
            If ScrapeModifiers.withEpisodes OrElse ScrapeModifiers.withSeasons Then
                For Each aSeason As TMDbLib.Objects.TvShows.TvSeason In Result.Seasons
                    GetTVSeasonInfo(nTVShow, Result.Id, aSeason.SeasonNumber, ScrapeModifiers, FilteredOptions)
                Next
            End If

            Return nTVShow
        End Function

        Public Function GetTVEpisodeInfo(ByVal ShowID As Integer, ByVal Aired As String, ByRef FilteredOptions As Structures.ScrapeOptions) As MediaContainers.EpisodeDetails
            Dim nTVEpisode As New MediaContainers.EpisodeDetails
            Dim ShowInfo As TMDbLib.Objects.TvShows.TvShow

            Dim showAPIResult As Task(Of TMDbLib.Objects.TvShows.TvShow)
            showAPIResult = Task.Run(Function() _TMDBApi.GetTvShowAsync(ShowID))

            ShowInfo = showAPIResult.Result

            For Each aSeason As TMDbLib.Objects.TvShows.TvSeason In ShowInfo.Seasons
                Dim seasonAPIResult As Task(Of TMDbLib.Objects.TvShows.TvSeason)
                seasonAPIResult = Task.Run(Function() _TMDBApi.GetTvSeasonAsync(ShowID, aSeason.SeasonNumber, TMDbLib.Objects.TvShows.TvSeasonMethods.Credits Or TMDbLib.Objects.TvShows.TvSeasonMethods.ExternalIds))

                Dim SeasonInfo As TMDbLib.Objects.TvShows.TvSeason = seasonAPIResult.Result
                Dim EpisodeList As IEnumerable(Of TMDbLib.Objects.TvShows.TvEpisode) = SeasonInfo.Episodes.Where(Function(f) CBool(f.AirDate = CDate(Aired)))
                If EpisodeList IsNot Nothing AndAlso EpisodeList.Count = 1 Then
                    Return GetTVEpisodeInfo(EpisodeList(0), FilteredOptions)
                ElseIf EpisodeList.Count > 0 Then
                    Return Nothing
                End If
            Next

            Return Nothing
        End Function

        Public Function GetTVEpisodeInfo(ByVal tmdbID As Integer, ByVal SeasonNumber As Integer, ByVal EpisodeNumber As Integer, ByRef FilteredOptions As Structures.ScrapeOptions) As MediaContainers.EpisodeDetails
            Dim APIResult As Task(Of TMDbLib.Objects.TvShows.TvEpisode)
            APIResult = Task.Run(Function() _TMDBApi.GetTvEpisodeAsync(tmdbID, SeasonNumber, EpisodeNumber, TMDbLib.Objects.TvShows.TvEpisodeMethods.Credits Or TMDbLib.Objects.TvShows.TvEpisodeMethods.ExternalIds))

            If APIResult IsNot Nothing AndAlso APIResult.Exception Is Nothing AndAlso APIResult.Result IsNot Nothing Then
                Dim EpisodeInfo As TMDbLib.Objects.TvShows.TvEpisode = APIResult.Result

                If EpisodeInfo Is Nothing OrElse EpisodeInfo.Id Is Nothing OrElse Not EpisodeInfo.Id > 0 Then
                    logger.Error(String.Format("Can't scrape or episode not found: tmdbID={0}, Season{1}, Episode{2}", tmdbID, SeasonNumber, EpisodeNumber))
                    Return Nothing
                End If

                Dim nEpisode As MediaContainers.EpisodeDetails = GetTVEpisodeInfo(EpisodeInfo, FilteredOptions)
                Return nEpisode
            Else
                logger.Error(String.Format("Can't scrape or episode not found: tmdbID={0}, Season{1}, Episode{2}", tmdbID, SeasonNumber, EpisodeNumber))
                Return Nothing
            End If
        End Function

        Public Function GetTVEpisodeInfo(ByRef EpisodeInfo As TMDbLib.Objects.TvShows.TvEpisode, ByRef FilteredOptions As Structures.ScrapeOptions) As MediaContainers.EpisodeDetails
            Dim nTVEpisode As New MediaContainers.EpisodeDetails

            nTVEpisode.Scrapersource = "TMDB"

            'IDs
            nTVEpisode.TMDB = CStr(EpisodeInfo.Id)
            If EpisodeInfo.ExternalIds IsNot Nothing AndAlso EpisodeInfo.ExternalIds.TvdbId IsNot Nothing Then nTVEpisode.TVDB = CStr(EpisodeInfo.ExternalIds.TvdbId)
            If EpisodeInfo.ExternalIds IsNot Nothing AndAlso EpisodeInfo.ExternalIds.ImdbId IsNot Nothing Then nTVEpisode.IMDB = EpisodeInfo.ExternalIds.ImdbId

            'Episode # Standard
            If EpisodeInfo.EpisodeNumber >= 0 Then
                nTVEpisode.Episode = EpisodeInfo.EpisodeNumber
            End If

            'Season # Standard
            If EpisodeInfo.SeasonNumber >= 0 Then
                nTVEpisode.Season = CInt(EpisodeInfo.SeasonNumber)
            End If

            'Cast (Actors)
            If FilteredOptions.bEpisodeActors Then
                If EpisodeInfo.Credits IsNot Nothing AndAlso EpisodeInfo.Credits.Cast IsNot Nothing Then
                    For Each aCast As TMDbLib.Objects.TvShows.Cast In EpisodeInfo.Credits.Cast
                        nTVEpisode.Actors.Add(New MediaContainers.Person With {.Name = aCast.Name,
                                                                           .Role = aCast.Character,
                                                                           .URLOriginal = If(Not String.IsNullOrEmpty(aCast.ProfilePath), String.Concat(_TMDBApi.Config.Images.BaseUrl, "original", aCast.ProfilePath), String.Empty),
                                                                           .TMDB = CStr(aCast.Id)})
                    Next
                End If
            End If

            'Aired
            If FilteredOptions.bEpisodeAired Then
                If EpisodeInfo.AirDate IsNot Nothing Then
                    Dim ScrapedDate As String = CStr(EpisodeInfo.AirDate)
                    If Not String.IsNullOrEmpty(ScrapedDate) AndAlso Not ScrapedDate = "00:00:00" Then
                        Dim RelDate As Date
                        If Date.TryParse(ScrapedDate, RelDate) Then
                            'always save date in same date format not depending on users language setting!
                            nTVEpisode.Aired = RelDate.ToString("yyyy-MM-dd")
                        Else
                            nTVEpisode.Aired = ScrapedDate
                        End If
                    End If
                End If
            End If

            'Director / Writer
            If FilteredOptions.bEpisodeCredits OrElse FilteredOptions.bEpisodeDirectors Then
                If EpisodeInfo.Credits IsNot Nothing AndAlso EpisodeInfo.Credits.Crew IsNot Nothing Then
                    For Each aCrew As TMDbLib.Objects.General.Crew In EpisodeInfo.Credits.Crew
                        If FilteredOptions.bEpisodeCredits AndAlso aCrew.Department = "Writing" AndAlso (aCrew.Job = "Author" OrElse aCrew.Job = "Screenplay" OrElse aCrew.Job = "Writer") Then
                            nTVEpisode.Credits.Add(aCrew.Name)
                        End If
                        If FilteredOptions.bEpisodeDirectors AndAlso aCrew.Department = "Directing" AndAlso aCrew.Job = "Director" Then
                            nTVEpisode.Directors.Add(aCrew.Name)
                        End If
                    Next
                End If
            End If

            'Guest Stars
            If FilteredOptions.bEpisodeGuestStars Then
                If EpisodeInfo.GuestStars IsNot Nothing Then
                    For Each aCast As TMDbLib.Objects.TvShows.Cast In EpisodeInfo.GuestStars
                        nTVEpisode.GuestStars.Add(New MediaContainers.Person With {.Name = aCast.Name,
                                                                           .Role = aCast.Character,
                                                                           .URLOriginal = If(Not String.IsNullOrEmpty(aCast.ProfilePath), String.Concat(_TMDBApi.Config.Images.BaseUrl, "original", aCast.ProfilePath), String.Empty),
                                                                           .TMDB = CStr(aCast.Id)})
                    Next
                End If
            End If

            'Plot
            If FilteredOptions.bEpisodePlot Then
                If EpisodeInfo.Overview IsNot Nothing Then
                    nTVEpisode.Plot = EpisodeInfo.Overview
                End If
            End If

            'Rating
            If FilteredOptions.bEpisodeRating Then
                nTVEpisode.Rating = CStr(EpisodeInfo.VoteAverage)
                nTVEpisode.Votes = CStr(EpisodeInfo.VoteCount)
            End If

            'ThumbPoster
            If EpisodeInfo.StillPath IsNot Nothing Then
                nTVEpisode.ThumbPoster.URLOriginal = _TMDBApi.Config.Images.BaseUrl & "original" & EpisodeInfo.StillPath
                nTVEpisode.ThumbPoster.URLThumb = _TMDBApi.Config.Images.BaseUrl & "w185" & EpisodeInfo.StillPath
            End If

            'Title
            If FilteredOptions.bEpisodeTitle Then
                If EpisodeInfo.Name IsNot Nothing Then
                    nTVEpisode.Title = EpisodeInfo.Name
                End If
            End If

            Return nTVEpisode
        End Function

        Public Sub GetTVSeasonInfo(ByRef nTVShow As MediaContainers.TVShow, ByVal ShowID As Integer, ByVal SeasonNumber As Integer, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef FilteredOptions As Structures.ScrapeOptions)
            Dim nSeason As New MediaContainers.SeasonDetails

            Dim APIResult As Task(Of TMDbLib.Objects.TvShows.TvSeason)
            APIResult = Task.Run(Function() _TMDBApi.GetTvSeasonAsync(ShowID, SeasonNumber, TMDbLib.Objects.TvShows.TvSeasonMethods.Credits Or TMDbLib.Objects.TvShows.TvSeasonMethods.ExternalIds))

            If APIResult IsNot Nothing AndAlso APIResult.Exception Is Nothing AndAlso APIResult.Result IsNot Nothing Then
                Dim SeasonInfo As TMDbLib.Objects.TvShows.TvSeason = APIResult.Result

                nSeason.TMDB = CStr(SeasonInfo.Id)
                If SeasonInfo.ExternalIds IsNot Nothing AndAlso SeasonInfo.ExternalIds.TvdbId IsNot Nothing Then nSeason.TVDB = CStr(SeasonInfo.ExternalIds.TvdbId)

                If ScrapeModifiers.withSeasons Then

                    'Aired
                    If FilteredOptions.bSeasonAired Then
                        If SeasonInfo.AirDate IsNot Nothing Then
                            Dim ScrapedDate As String = CStr(SeasonInfo.AirDate)
                            If Not String.IsNullOrEmpty(ScrapedDate) Then
                                Dim RelDate As Date
                                If Date.TryParse(ScrapedDate, RelDate) Then
                                    'always save date in same date format not depending on users language setting!
                                    nSeason.Aired = RelDate.ToString("yyyy-MM-dd")
                                Else
                                    nSeason.Aired = ScrapedDate
                                End If
                            End If
                        End If
                    End If

                    'Plot
                    If FilteredOptions.bSeasonPlot Then
                        If SeasonInfo.Overview IsNot Nothing Then
                            nSeason.Plot = SeasonInfo.Overview
                        End If
                    End If

                    'Season #
                    If SeasonInfo.SeasonNumber >= 0 Then
                        nSeason.Season = SeasonInfo.SeasonNumber
                    End If

                    'Title
                    If SeasonInfo.Name IsNot Nothing Then
                        nSeason.Title = SeasonInfo.Name
                    End If

                    nTVShow.KnownSeasons.Add(nSeason)
                End If

                If ScrapeModifiers.withEpisodes AndAlso SeasonInfo.Episodes IsNot Nothing Then
                    For Each aEpisode As TMDbLib.Objects.TvShows.TvEpisode In SeasonInfo.Episodes
                        nTVShow.KnownEpisodes.Add(GetTVEpisodeInfo(aEpisode, FilteredOptions))
                        'nShowContainer.KnownEpisodes.Add(GetTVEpisodeInfo(ShowID, SeasonNumber, aEpisode.EpisodeNumber, Options))
                    Next
                End If
            Else
                logger.Error(String.Format("Can't scrape or season not found: ShowID={0}, Season={1}", ShowID, SeasonNumber))
            End If
        End Sub

        Public Function GetTVSeasonInfo(ByVal tmdbID As Integer, ByVal SeasonNumber As Integer, ByRef FilteredOptions As Structures.ScrapeOptions) As MediaContainers.SeasonDetails
            Dim APIResult As Task(Of TMDbLib.Objects.TvShows.TvSeason)
            APIResult = Task.Run(Function() _TMDBApi.GetTvSeasonAsync(tmdbID, SeasonNumber, TMDbLib.Objects.TvShows.TvSeasonMethods.Credits Or TMDbLib.Objects.TvShows.TvSeasonMethods.ExternalIds))

            If APIResult IsNot Nothing AndAlso APIResult.Exception Is Nothing AndAlso APIResult.Result IsNot Nothing Then
                Dim SeasonInfo As TMDbLib.Objects.TvShows.TvSeason = APIResult.Result

                If SeasonInfo Is Nothing OrElse SeasonInfo.Id Is Nothing OrElse Not SeasonInfo.Id > 0 Then
                    logger.Error(String.Format("Can't scrape or season not found: tmdbID={0}, Season={1}", tmdbID, SeasonNumber))
                    Return Nothing
                End If

                Dim nTVSeason As MediaContainers.SeasonDetails = GetTVSeasonInfo(SeasonInfo, FilteredOptions)
                Return nTVSeason
            Else
                logger.Error(String.Format("Can't scrape or season not found: tmdbID={0}, Season={1}", tmdbID, SeasonNumber))
                Return Nothing
            End If
        End Function

        Public Function GetTVSeasonInfo(ByRef SeasonInfo As TMDbLib.Objects.TvShows.TvSeason, ByRef FilteredOptions As Structures.ScrapeOptions) As MediaContainers.SeasonDetails
            Dim nTVSeason As New MediaContainers.SeasonDetails

            nTVSeason.Scrapersource = "TMDB"

            'IDs
            nTVSeason.TMDB = CStr(SeasonInfo.Id)
            If SeasonInfo.ExternalIds IsNot Nothing AndAlso SeasonInfo.ExternalIds.TvdbId IsNot Nothing Then nTVSeason.TVDB = CStr(SeasonInfo.ExternalIds.TvdbId)

            'Season #
            If SeasonInfo.SeasonNumber >= 0 Then
                nTVSeason.Season = SeasonInfo.SeasonNumber
            End If

            'Aired
            If FilteredOptions.bSeasonAired Then
                If SeasonInfo.AirDate IsNot Nothing Then
                    Dim ScrapedDate As String = CStr(SeasonInfo.AirDate)
                    If Not String.IsNullOrEmpty(ScrapedDate) Then
                        Dim RelDate As Date
                        If Date.TryParse(ScrapedDate, RelDate) Then
                            'always save date in same date format not depending on users language setting!
                            nTVSeason.Aired = RelDate.ToString("yyyy-MM-dd")
                        Else
                            nTVSeason.Aired = ScrapedDate
                        End If
                    End If
                End If
            End If

            'Plot
            If FilteredOptions.bSeasonPlot Then
                If SeasonInfo.Overview IsNot Nothing Then
                    nTVSeason.Plot = SeasonInfo.Overview
                End If
            End If

            'Title
            If FilteredOptions.bSeasonTitle Then
                If SeasonInfo.Name IsNot Nothing Then
                    nTVSeason.Title = SeasonInfo.Name
                End If
            End If

            Return nTVSeason
        End Function

        Public Function GetTMDBbyIMDB(ByVal imdbID As String) As String
            Dim tmdbID As String = String.Empty

            Try
                Dim APIResult As Task(Of TMDbLib.Objects.Find.FindContainer)
                APIResult = Task.Run(Function() _TMDBApi.FindAsync(TMDbLib.Objects.Find.FindExternalSource.Imdb, imdbID))

                If APIResult IsNot Nothing AndAlso APIResult.Exception Is Nothing AndAlso APIResult.Result IsNot Nothing AndAlso
                    APIResult.Result.TvResults IsNot Nothing AndAlso APIResult.Result.TvResults.Count > 0 Then
                    tmdbID = APIResult.Result.TvResults.Item(0).Id.ToString
                End If

            Catch ex As Exception
                logger.Error(ex, New StackFrame().GetMethod().Name)
            End Try

            Return tmdbID
        End Function

        Public Function GetTMDBbyTVDB(ByVal tvdbID As String) As String
            Dim tmdbID As String = String.Empty

            Try
                Dim APIResult As Task(Of TMDbLib.Objects.Find.FindContainer)
                APIResult = Task.Run(Function() _TMDBApi.FindAsync(TMDbLib.Objects.Find.FindExternalSource.TvDb, tvdbID))

                If APIResult IsNot Nothing AndAlso APIResult.Exception Is Nothing AndAlso APIResult.Result IsNot Nothing AndAlso
                    APIResult.Result.TvResults IsNot Nothing AndAlso APIResult.Result.TvResults.Count > 0 Then
                    tmdbID = APIResult.Result.TvResults.Item(0).Id.ToString
                End If

            Catch ex As Exception
                logger.Error(ex, New StackFrame().GetMethod().Name)
            End Try

            Return tmdbID
        End Function

        Public Function GetMovieStudios(ByVal strID As String) As List(Of String)
            If String.IsNullOrEmpty(strID) OrElse strID.Length > 2 Then Return New List(Of String)

            Dim alStudio As New List(Of String)
            Dim Movie As TMDbLib.Objects.Movies.Movie

            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)

            If strID.Substring(0, 2).ToLower = "tt" Then
                APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(strID))
            Else
                APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(CInt(strID)))
            End If

            Movie = APIResult.Result

            If Movie IsNot Nothing AndAlso Movie.ProductionCompanies IsNot Nothing AndAlso Movie.ProductionCompanies.Count > 0 Then
                For Each cStudio In Movie.ProductionCompanies
                    alStudio.Add(cStudio.Name)
                Next
            End If

            Return alStudio
        End Function

        Private Function GetVideoQuality(ByRef Size As Integer) As Enums.TrailerVideoQuality
            If Size = 0 Then Return Enums.TrailerVideoQuality.Any

            Select Case Size
                Case 1080
                    Return Enums.TrailerVideoQuality.HD1080p
                Case 720
                    Return Enums.TrailerVideoQuality.HD720p
                Case 480
                    Return Enums.TrailerVideoQuality.HQ480p
            End Select

            Return Enums.TrailerVideoQuality.Any
        End Function

        Private Function GetVideoType(ByRef Type As String) As Enums.TrailerType
            If String.IsNullOrEmpty(Type) Then Return Enums.TrailerType.Any

            Select Case Type.ToLower
                Case "clip"
                    Return Enums.TrailerType.Clip
                Case "featurette"
                    Return Enums.TrailerType.Featurette
                Case "teaser"
                    Return Enums.TrailerType.Teaser
                Case "trailer"
                    Return Enums.TrailerType.Trailer
            End Select

            Return Enums.TrailerType.Any
        End Function

        Public Function SearchMovie(ByVal strTitle As String, Optional ByVal iYear As Integer = 0) As List(Of MediaContainers.Movie)
            If String.IsNullOrEmpty(strTitle) Then Return New List(Of MediaContainers.Movie)

            Dim R As New List(Of MediaContainers.Movie)
            Dim Page As Integer = 1
            Dim Movies As TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchMovie)
            Dim TotP As Integer
            Dim aE As Boolean

            Dim APIResult As Task(Of TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchMovie))
            APIResult = Task.Run(Function() _TMDBApi.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear))

            Movies = APIResult.Result

            If Movies.TotalResults = 0 AndAlso _SpecialSettings.FallBackEng Then
                APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear))
                Movies = APIResult.Result
                aE = True
            End If

            'try -1 year if no search result was found
            If Movies.TotalResults = 0 AndAlso iYear > 0 AndAlso _SpecialSettings.SearchDeviant Then
                APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear - 1))
                Movies = APIResult.Result

                If Movies.TotalResults = 0 AndAlso _SpecialSettings.FallBackEng Then
                    APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear - 1))
                    Movies = APIResult.Result
                    aE = True
                End If

                'still no search result, try +1 year
                If Movies.TotalResults = 0 Then
                    APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear + 1))
                    Movies = APIResult.Result

                    If Movies.TotalResults = 0 AndAlso _SpecialSettings.FallBackEng Then
                        APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear + 1))
                        Movies = APIResult.Result
                        aE = True
                    End If
                End If
            End If

            If Movies.TotalResults > 0 Then
                TotP = Movies.TotalPages
                While Page <= TotP AndAlso Page <= 3
                    If Movies.Results IsNot Nothing Then
                        For Each aMovie In Movies.Results
                            Dim tOriginalTitle As String = String.Empty
                            Dim tPlot As String = String.Empty
                            Dim tThumbPoster As New MediaContainers.Image
                            Dim tTitle As String = String.Empty
                            Dim tYear As String = String.Empty

                            If aMovie.OriginalTitle IsNot Nothing Then tOriginalTitle = aMovie.OriginalTitle
                            If aMovie.Overview IsNot Nothing Then tPlot = aMovie.Overview
                            If aMovie.PosterPath IsNot Nothing Then
                                tThumbPoster.URLOriginal = _TMDBApi.Config.Images.BaseUrl & "original" & aMovie.PosterPath
                                tThumbPoster.URLThumb = _TMDBApi.Config.Images.BaseUrl & "w185" & aMovie.PosterPath
                            End If
                            If aMovie.ReleaseDate IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(aMovie.ReleaseDate)) Then tYear = CStr(aMovie.ReleaseDate.Value.Year)
                            If aMovie.Title IsNot Nothing Then tTitle = aMovie.Title

                            Dim lNewMovie As MediaContainers.Movie = New MediaContainers.Movie With {.OriginalTitle = tOriginalTitle,
                                                                                                     .Plot = tPlot,
                                                                                                     .Title = tTitle,
                                                                                                     .ThumbPoster = tThumbPoster,
                                                                                                     .TMDBID = CStr(aMovie.Id),
                                                                                                     .Year = tYear}
                            R.Add(lNewMovie)
                        Next
                    End If
                    Page = Page + 1
                    If aE Then
                        APIResult = Task.Run(Function() _TMDBApiE.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear))
                        Movies = APIResult.Result
                    Else
                        APIResult = Task.Run(Function() _TMDBApi.SearchMovieAsync(strTitle, Page, _SpecialSettings.GetAdultItems, iYear))
                        Movies = APIResult.Result
                    End If
                End While
            End If

            Return R
        End Function

        Public Function SearchMovieSet(ByVal strTitle As String) As List(Of MediaContainers.MovieSet)
            If String.IsNullOrEmpty(strTitle) Then Return New List(Of MediaContainers.MovieSet)

            Dim R As New List(Of MediaContainers.MovieSet)
            Dim Page As Integer = 1
            Dim MovieSets As TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchResultCollection)
            Dim TotP As Integer
            Dim aE As Boolean

            Dim APIResult As Task(Of TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchResultCollection))
            APIResult = Task.Run(Function() _TMDBApi.SearchCollectionAsync(strTitle, Page))

            MovieSets = APIResult.Result

            If MovieSets.TotalResults = 0 AndAlso _SpecialSettings.FallBackEng Then
                APIResult = Task.Run(Function() _TMDBApiE.SearchCollectionAsync(strTitle, Page))
                MovieSets = APIResult.Result
                aE = True
            End If

            If MovieSets.TotalResults > 0 Then
                TotP = MovieSets.TotalPages
                While Page <= TotP AndAlso Page <= 3
                    If MovieSets.Results IsNot Nothing Then
                        For Each aMovieSet In MovieSets.Results
                            Dim tPlot As String = String.Empty
                            Dim tThumbPoster As New MediaContainers.Image
                            Dim tTitle As String = String.Empty

                            If aMovieSet.Name IsNot Nothing Then tTitle = aMovieSet.Name
                            If aMovieSet.PosterPath IsNot Nothing Then
                                tThumbPoster.URLOriginal = _TMDBApi.Config.Images.BaseUrl & "original" & aMovieSet.PosterPath
                                tThumbPoster.URLThumb = _TMDBApi.Config.Images.BaseUrl & "w185" & aMovieSet.PosterPath
                            End If

                            Dim lNewMovieSet As MediaContainers.MovieSet = New MediaContainers.MovieSet With {
                                .Title = tTitle,
                                .ThumbPoster = tThumbPoster,
                                .TMDB = CStr(aMovieSet.Id)}

                            R.Add(lNewMovieSet)
                        Next
                    End If
                    Page = Page + 1
                    If aE Then
                        APIResult = Task.Run(Function() _TMDBApiE.SearchCollectionAsync(strTitle, Page))
                        MovieSets = APIResult.Result
                    Else
                        APIResult = Task.Run(Function() _TMDBApi.SearchCollectionAsync(strTitle, Page))
                        MovieSets = APIResult.Result
                    End If
                End While
            End If

            Return R
        End Function

        Public Function SearchTVShow(ByVal strShow As String) As List(Of MediaContainers.TVShow)
            If String.IsNullOrEmpty(strShow) Then Return New List(Of MediaContainers.TVShow)

            Dim R As New List(Of MediaContainers.TVShow)
            Dim Page As Integer = 1
            Dim Shows As TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchTv)
            Dim TotP As Integer
            Dim aE As Boolean

            Dim APIResult As Task(Of TMDbLib.Objects.General.SearchContainer(Of TMDbLib.Objects.Search.SearchTv))
            APIResult = Task.Run(Function() _TMDBApi.SearchTvShowAsync(strShow, Page))

            Shows = APIResult.Result

            If Shows.TotalResults = 0 AndAlso _SpecialSettings.FallBackEng Then
                APIResult = Task.Run(Function() _TMDBApiE.SearchTvShowAsync(strShow, Page))
                Shows = APIResult.Result
                aE = True
            End If

            If Shows.TotalResults > 0 Then
                Dim t1 As String = String.Empty
                Dim t2 As String = String.Empty
                TotP = Shows.TotalPages
                While Page <= TotP AndAlso Page <= 3
                    If Shows.Results IsNot Nothing Then
                        For Each aShow In Shows.Results
                            If aShow.Name Is Nothing OrElse (aShow.Name IsNot Nothing AndAlso String.IsNullOrEmpty(aShow.Name)) Then
                                If aShow.OriginalName IsNot Nothing AndAlso Not String.IsNullOrEmpty(aShow.OriginalName) Then
                                    t1 = aShow.OriginalName
                                End If
                            Else
                                t1 = aShow.Name
                            End If
                            If aShow.FirstAirDate IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(aShow.FirstAirDate)) Then
                                t2 = CStr(aShow.FirstAirDate.Value.Year)
                            End If
                            Dim lNewShow As MediaContainers.TVShow = New MediaContainers.TVShow(String.Empty, t1, t2)
                            lNewShow.TMDB = CStr(aShow.Id)
                            R.Add(lNewShow)
                        Next
                    End If
                    Page = Page + 1
                    If aE Then
                        APIResult = Task.Run(Function() _TMDBApiE.SearchTvShowAsync(strShow, Page))
                        Shows = APIResult.Result
                    Else
                        APIResult = Task.Run(Function() _TMDBApi.SearchTvShowAsync(strShow, Page))
                        Shows = APIResult.Result
                    End If
                End While
            End If

            Return R
        End Function

#End Region 'Methods

#Region "Nested Types"

#End Region 'Nested Types

    End Class

End Namespace

