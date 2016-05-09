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

Imports NLog
Imports System.Threading.Tasks

Public Class UniqueID

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Private Shared _TMDBApi As TMDbLib.Client.TMDbClient

#End Region

#Region "Methods"

    Public Shared Sub GetUniqueIDs(ByRef tDBElement As Database.DBElement)
        Select Case tDBElement.ContentType
            Case Enums.ContentType.Movie
                If tDBElement.Movie.TMDBIDSpecified OrElse tDBElement.Movie.IMDBIDSpecified Then
                    TMDB_CreateAPI()
                    TMDB_GetIDs_Movie(tDBElement)
                End If
            Case Enums.ContentType.TVEpisode
            Case Enums.ContentType.TVSeason
            Case Enums.ContentType.TVShow
                If tDBElement.TVShow.AnyUniqueIDSpecified Then
                    TMDB_CreateAPI()
                    TMDB_GetIDs_TVShow(tDBElement)
                End If
        End Select
    End Sub

    Private Shared Sub TMDB_CreateAPI()
        _TMDBApi = New TMDbLib.Client.TMDbClient("44810eefccd9cb1fa1d57e7b0d67b08d")
        _TMDBApi.GetConfig()
        _TMDBApi.DefaultLanguage = "en"
    End Sub

    Private Shared Sub TMDB_GetIDs_Movie(ByRef tDBElement As Database.DBElement)
        Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
        Dim Movie As TMDbLib.Objects.Movies.Movie = Nothing

        If tDBElement.Movie.TMDBIDSpecified Then
            Dim iTMDBID As Integer = CInt(tDBElement.Movie.TMDBID)
            APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(iTMDBID))
            Movie = APIResult.Result
        ElseIf tDBElement.Movie.IDSpecified Then
            Dim strIMDBID As String = tDBElement.Movie.ID
            APIResult = Task.Run(Function() _TMDBApi.GetMovieAsync(strIMDBID))
            Movie = APIResult.Result
        End If

        If Movie IsNot Nothing Then
            tDBElement.Movie.TMDBID = CStr(Movie.Id)
            If Movie.ImdbId IsNot Nothing AndAlso Not String.IsNullOrEmpty(Movie.ImdbId) Then
                tDBElement.Movie.IMDBID = Movie.ImdbId
            End If
        End If

    End Sub

    Private Shared Sub TMDB_GetIDs_TVShow(ByRef tDBElement As Database.DBElement)
        Dim APIResult As Task(Of TMDbLib.Objects.TvShows.TvShow)
        Dim TVShow As TMDbLib.Objects.TvShows.TvShow = Nothing
        Dim iTMDBID As Integer = -1

        If tDBElement.TVShow.TMDBSpecified Then
            iTMDBID = CInt(tDBElement.TVShow.TMDB)
        Else
            If tDBElement.TVShow.TVDBSpecified Then
                Dim strTVDBID As String = tDBElement.TVShow.TVDB
                Dim APIFind As Task(Of TMDbLib.Objects.Find.FindContainer)
                APIFind = Task.Run(Function() _TMDBApi.FindAsync(TMDbLib.Objects.Find.FindExternalSource.TvDb, strTVDBID))
                If APIFind IsNot Nothing AndAlso APIFind.Result IsNot Nothing AndAlso
                        APIFind.Result.TvResults IsNot Nothing AndAlso APIFind.Result.TvResults.Count > 0 Then
                    iTMDBID = APIFind.Result.TvResults.Item(0).Id
                End If
            ElseIf tDBElement.TVShow.IMDBSpecified Then
                Dim strIMDBID As String = tDBElement.TVShow.IMDB
                Dim APIFind As Task(Of TMDbLib.Objects.Find.FindContainer)
                APIFind = Task.Run(Function() _TMDBApi.FindAsync(TMDbLib.Objects.Find.FindExternalSource.Imdb, strIMDBID))
                If APIFind IsNot Nothing AndAlso APIFind.Result IsNot Nothing AndAlso
                    APIFind.Result.TvResults IsNot Nothing AndAlso APIFind.Result.TvResults.Count > 0 Then
                    iTMDBID = APIFind.Result.TvResults.Item(0).Id
                End If
            End If
        End If

        If Not iTMDBID = -1 Then
            APIResult = Task.Run(Function() _TMDBApi.GetTvShowAsync(iTMDBID, TMDbLib.Objects.TvShows.TvShowMethods.ExternalIds))
            TVShow = APIResult.Result

            If TVShow IsNot Nothing Then
                tDBElement.TVShow.TMDB = CStr(TVShow.Id)
                If TVShow.ExternalIds IsNot Nothing Then
                    If TVShow.ExternalIds.ImdbId IsNot Nothing AndAlso Not String.IsNullOrEmpty(TVShow.ExternalIds.ImdbId) Then
                        tDBElement.TVShow.IMDB = TVShow.ExternalIds.ImdbId
                    End If
                    If TVShow.ExternalIds.TvdbId IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(TVShow.ExternalIds.TvdbId)) Then
                        tDBElement.TVShow.TVDB = CStr(TVShow.ExternalIds.TvdbId)
                    End If
                End If
            End If
        End If

    End Sub

#End Region

End Class
