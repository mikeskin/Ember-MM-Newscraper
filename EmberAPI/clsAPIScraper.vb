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

Public Class Scraper

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

#End Region 'Fields

#Region "Events"

#End Region 'Events

#Region "Methods"

    Public Function DoScrape(ByRef tDBElement As Database.DBElement, ByVal tScrapeModifiers As Structures.ScrapeModifiers, ByVal tScrapeType As Enums.ScrapeType, ByVal tScrapeOptions As Structures.ScrapeOptions, ByVal bShowMessage As Boolean) As Boolean
        Select Case tDBElement.ContentType
            Case Enums.ContentType.Movie
                logger.Trace(String.Format("[Scraper] [DoScrape] [Movie] [Start] {0}", tDBElement.Filename))
                If tDBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(tDBElement, bShowMessage) Then

                    'clean DBMovie if the movie is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
                    If tScrapeModifiers.DoSearch Then
                        tDBElement.ImagesContainer = New MediaContainers.ImagesContainer
                        tDBElement.Movie = New MediaContainers.Movie

                        tDBElement.Movie.Title = StringUtils.FilterTitleFromPath_Movie(tDBElement.Filename, tDBElement.IsSingle, tDBElement.Source.UseFolderName)
                        tDBElement.Movie.Year = StringUtils.FilterYearFromPath_Movie(tDBElement.Filename, tDBElement.IsSingle, tDBElement.Source.UseFolderName)
                    End If

                    'create a clone of DBMovie
                    Dim oDBElement As Database.DBElement = CType(tDBElement.CloneDeep, Database.DBElement)

                    If Not tDBElement.Movie.AnyUniqueIDSpecified Then
                        If Not DoSearch(oDBElement, tScrapeType) Then
                            Return False
                        End If
                    End If

                    Dim ret = ModulesManager.Instance.RunScraper(oDBElement, tScrapeModifiers, tScrapeType, tScrapeOptions)



                    'Merge scraperresults considering global datascraper settings
                    tDBElement = NFO.MergeDataScraperResults_Movie(tDBElement, tScrapedData, tScrapeType, tScrapeOptions)

                    'create cache paths for Actor Thumbs
                    tDBElement.Movie.CreateCachePaths_ActorsThumbs()
                End If

                If tScrapedData.Count > 0 Then
                    logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Done] {0}", tDBElement.Filename))
                Else
                    logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Done] [No Scraper Results] {0}", tDBElement.Filename))
                    Return True 'TODO: need a new trigger
                End If
                Return ret.bCancelled
                Else
                logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Abort] [Offline] {0}", tDBElement.Filename))
                Return False
                End If
        End Select

        Return False
    End Function


    Public Function DoSearch(ByRef DBElement As Database.DBElement, ByVal ScrapeType As Enums.ScrapeType) As Boolean
        Dim tSearchResults As New MediaContainers.SearchResultsContainer

        Select Case DBElement.ContentType
            Case Enums.ContentType.Movie
                logger.Trace(String.Format("[Scraper] [DoSearch] [Movie] [Start] {0}", DBElement.Filename))
                tSearchResults = ModulesManager.Instance.RunSearch(DBElement.Movie.Title, DBElement.Movie.Year, DBElement.ContentType)
                If tSearchResults.Movies.Count > 0 Then
                    DBElement.Movie = tSearchResults.Movies.Item(0)
                    Return True
                End If
        End Select

        Return False
    End Function

#End Region 'Methods

End Class
