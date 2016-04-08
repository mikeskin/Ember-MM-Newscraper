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

    Friend WithEvents bwScraper As New ComponentModel.BackgroundWorker

    Private ScraperList As New Queue(Of ScraperTask)
    Private TasksDone As Boolean = True

#End Region 'Fields

#Region "Events"

    Public Event ScraperUpdated(ByVal iType As Integer, ByVal sText As String)
    Public Event ScrapingCompleted()

#End Region 'Events

#Region "Properties"

    ReadOnly Property IsBusy() As Boolean
        Get
            Return Not TasksDone
            'Return bwScraper.IsBusy
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Public Sub RunScraper(ByVal NewTaskList As List(Of ScraperTask))
        AddToTaskList(NewTaskList)
    End Sub

    Private Sub AddToTaskList(ByRef NewTaskList As List(Of ScraperTask))

        For Each nScraperTask As ScraperTask In NewTaskList
            ScraperList.Enqueue(nScraperTask)
        Next

        If TasksDone Then
            RunTasks()
            'Else
            '    ChangeTaskManagerStatus(lblTaskManagerStatus, String.Concat("Pending Tasks: ", (TaskList.Count + 1).ToString))
        End If
    End Sub

    Private Sub RunTasks()
        Dim getError As Boolean = False
        'Dim GenericEventActionAsync As New Action(Of GenericEventCallBackAsync)(AddressOf Handle_GenericEventAsync)
        'Dim GenericEventProgressAsync = New Progress(Of GenericEventCallBackAsync)(GenericEventActionAsync)

        TasksDone = False
        While ScraperList.Count > 0
            'ChangeTaskManagerStatus(lblTaskManagerStatus, String.Concat("Pending Tasks: ", TaskList.Count.ToString))
            'ChangeTaskManagerProgressBar(tspTaskManager, ProgressBarStyle.Marquee)
            Dim tScraperTask As ScraperTask = ScraperList.Dequeue()
            Select Case tScraperTask.mContentType
                Case Enums.ContentType.Movie
                    ScrapeMovie(tScraperTask)
            End Select
        End While
        TasksDone = True
        'ChangeTaskManagerProgressBar(tspTaskManager, ProgressBarStyle.Continuous)
        'ChangeTaskManagerStatus(lblTaskManagerStatus, "No Pending Tasks")
        If Not getError Then
            'ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"info", Nothing, Master.eLang.GetString(1422, "Kodi Interface"), Master.eLang.GetString(251, "All Tasks Done"), New Bitmap(My.Resources.logo)}))
        Else
            'ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"error", 1, Master.eLang.GetString(1422, "Kodi Interface"), String.Format(Master.eLang.GetString(969, "One or more Task(s) failed.{0}Please check log for more informations"), Environment.NewLine), Nothing}))
        End If
    End Sub

    Public Sub ScrapeMovie(ByRef nScraperTask As ScraperTask)
        Dim Cancelled As Boolean = False
        Dim Theme As New MediaContainers.Theme
        Dim tURL As String = String.Empty
        Dim aUrlList As New List(Of MediaContainers.Trailer)
        Dim tUrlList As New List(Of Themes)
        Dim OldListTitle As String = String.Empty
        Dim NewListTitle As String = String.Empty

        Cancelled = False

        Dim DBScrapeMovie As Database.DBElement = Master.DB.Load_Movie(nScraperTask.mID)

        'If bwMovieScraper.CancellationPending Then Exit For
        OldListTitle = DBScrapeMovie.ListTitle
        'bwMovieScraper.ReportProgress(1, OldListTitle)

        logger.Trace(String.Format("[Movie Scraper] [Start] Scraping {0}", OldListTitle))

        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.BeforeEdit_Movie, Nothing, Nothing, False, DBScrapeMovie)

        If nScraperTask.ScrapeModifiers.MainNFO Then
            If ModulesManager.Instance.ScrapeData_Movie(DBScrapeMovie, nScraperTask.ScrapeModifiers, nScraperTask.ScrapeType, nScraperTask.ScrapeOptions, False) Then
                logger.Trace(String.Format("[Movie Scraper] [Cancelled] Scraping {0}", OldListTitle))
                Cancelled = True
                If nScraperTask.ScrapeType = Enums.ScrapeType.SingleAuto OrElse
                    nScraperTask.ScrapeType = Enums.ScrapeType.SingleField OrElse
                    nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                    'bwMovieScraper.CancelAsync()
                End If
            End If
        Else
            ' if we do not have the movie ID we need to retrive it even if is just a Poster/Fanart/Trailer/Actors update
            If String.IsNullOrEmpty(DBScrapeMovie.Movie.ID) AndAlso (nScraperTask.ScrapeModifiers.MainActorthumbs Or nScraperTask.ScrapeModifiers.MainBanner Or nScraperTask.ScrapeModifiers.MainClearArt Or
                                                                     nScraperTask.ScrapeModifiers.MainClearLogo Or nScraperTask.ScrapeModifiers.MainDiscArt Or nScraperTask.ScrapeModifiers.MainExtrafanarts Or
                                                                     nScraperTask.ScrapeModifiers.MainExtrathumbs Or nScraperTask.ScrapeModifiers.MainFanart Or nScraperTask.ScrapeModifiers.MainLandscape Or
                                                                     nScraperTask.ScrapeModifiers.MainPoster Or nScraperTask.ScrapeModifiers.MainTheme Or nScraperTask.ScrapeModifiers.MainTrailer) Then
                Dim tModifiers As New Structures.ScrapeModifiers With {.MainNFO = True}
                Dim tOptions As New Structures.ScrapeOptions 'set all values to false to not override any field. ID's are always determined.
                If ModulesManager.Instance.ScrapeData_Movie(DBScrapeMovie, tModifiers, nScraperTask.ScrapeType, tOptions, False) Then
                    logger.Trace(String.Format("[Movie Scraper] [Cancelled] Scraping {0}", OldListTitle))
                    Cancelled = True
                    If nScraperTask.ScrapeType = Enums.ScrapeType.SingleAuto OrElse nScraperTask.ScrapeType = Enums.ScrapeType.SingleField OrElse nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                        'bwMovieScraper.CancelAsync()
                    End If
                End If
            End If
        End If

        'If bwMovieScraper.CancellationPending Then Exit For

        If Not Cancelled Then
            If Master.eSettings.MovieScraperMetaDataScan AndAlso nScraperTask.ScrapeModifiers.MainMeta Then
                'bwMovieScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(140, "Scanning Meta Data"), ":"))
                MediaInfo.UpdateMediaInfo(DBScrapeMovie)
            End If

            'If bwMovieScraper.CancellationPending Then Exit For

            NewListTitle = DBScrapeMovie.ListTitle

            If Not NewListTitle = OldListTitle Then
                'bwMovieScraper.ReportProgress(0, String.Format(Master.eLang.GetString(812, "Old Title: {0} | New Title: {1}"), OldListTitle, NewListTitle))
            End If

            'get all images 
            If nScraperTask.ScrapeModifiers.MainBanner OrElse
                nScraperTask.ScrapeModifiers.MainClearArt OrElse
                nScraperTask.ScrapeModifiers.MainClearLogo OrElse
                nScraperTask.ScrapeModifiers.MainDiscArt OrElse
                nScraperTask.ScrapeModifiers.MainExtrafanarts OrElse
                nScraperTask.ScrapeModifiers.MainExtrathumbs OrElse
                nScraperTask.ScrapeModifiers.MainFanart OrElse
                nScraperTask.ScrapeModifiers.MainLandscape OrElse
                nScraperTask.ScrapeModifiers.MainPoster Then

                Dim SearchResultsContainer As New MediaContainers.SearchResultsContainer
                'bwMovieScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(254, "Scraping Images"), ":"))
                If Not ModulesManager.Instance.ScrapeImage_Movie(DBScrapeMovie, SearchResultsContainer, nScraperTask.ScrapeModifiers, False) Then
                    If nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape AndAlso Master.eSettings.MovieImagesDisplayImageSelect Then
                        'Using dImgSelect As New dlgImgSelect
                        '    If dImgSelect.ShowDialog(DBScrapeMovie, SearchResultsContainer, nScraperTask.ScrapeModifiers) = DialogResult.OK Then
                        '        Images.SetPreferredImages(DBScrapeMovie, dImgSelect.Result)
                        '    End If
                        'End Using

                        'autoscraping
                    ElseIf Not nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                        Images.SetPreferredImages(DBScrapeMovie, SearchResultsContainer, nScraperTask.ScrapeModifiers, IsAutoScraper:=True)
                    End If
                End If
            End If

            'If bwMovieScraper.CancellationPending Then Exit For

            'Theme
            If nScraperTask.ScrapeModifiers.MainTheme Then
                'bwMovieScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(266, "Scraping Themes"), ":"))
                If Not (nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape) Then
                    tURL = String.Empty
                    If Theme.WebTheme.IsAllowedToDownload(DBScrapeMovie) Then
                        If Not ModulesManager.Instance.ScrapeTheme_Movie(DBScrapeMovie, tUrlList) Then
                            If tUrlList.Count > 0 Then
                                If Not (nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape) Then
                                    Theme.WebTheme.FromWeb(tUrlList.Item(0).URL, tUrlList.Item(0).WebURL)
                                    If Theme.WebTheme IsNot Nothing Then 'TODO: fix check
                                        tURL = Theme.WebTheme.SaveAsMovieTheme(DBScrapeMovie)
                                        If Not String.IsNullOrEmpty(tURL) Then
                                            DBScrapeMovie.ThemePath = tURL
                                        End If
                                    End If
                                    'ElseIf Args.scrapeType = Enums.ScrapeType.SingleScrape OrElse Args.scrapeType = Enums.ScrapeType.FullAsk OrElse Args.scrapeType = Enums.ScrapeType.NewAsk OrElse Args.scrapeType = Enums.ScrapeType.MarkAsk  OrElse Args.scrapeType = Enums.ScrapeType.UpdateAsk Then
                                    '    If Args.scrapeType = Enums.ScrapeType.FullAsk OrElse Args.scrapeType = Enums.ScrapeType.NewAsk OrElse Args.scrapeType = Enums.ScrapeType.MarkAsk  OrElse Args.scrapeType = Enums.ScrapeType.UpdateAsk Then
                                    '        MsgBox(Master.eLang.GetString(930, "Trailer of your preferred size could not be found. Please choose another."), MsgBoxStyle.Information, Master.eLang.GetString(929, "No Preferred Size:"))
                                    '    End If
                                    '    Using dThemeSelect As New dlgThemeSelect()
                                    '        tURL = dThemeSelect.ShowDialog(DBScrapeMovie, tUrlList)
                                    '        If Not String.IsNullOrEmpty(tURL) Then
                                    '            DBScrapeMovie.ThemePath = tURL
                                    '            MovieScraperEvent(Enums.MovieScraperEventType.ThemeItem, DBScrapeMovie.ThemePath )
                                    '        End If
                                    '    End Using
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            'If bwMovieScraper.CancellationPending Then Exit For

            'Trailer
            If nScraperTask.ScrapeModifiers.MainTrailer Then
                'bwMovieScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(574, "Scraping Trailers"), ":"))
                Dim SearchResults As New List(Of MediaContainers.Trailer)
                If Not ModulesManager.Instance.ScrapeTrailer_Movie(DBScrapeMovie, Enums.ModifierType.MainTrailer, SearchResults) Then
                    If nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                        'Using dTrailerSelect As New dlgTrailerSelect
                        '    'note msavazzi why is always False with Player? If dTrailerSelect.ShowDialog(DBScrapeMovie, SearchResults, False, True, False) = DialogResult.OK Then
                        '    'DanCooper: the VLC COM interface is/was not able to call in multithread
                        '    If dTrailerSelect.ShowDialog(DBScrapeMovie, SearchResults, False, True, clsAdvancedSettings.GetBooleanSetting("UseAsVideoPlayer", False, "generic.EmberCore.VLCPlayer")) = DialogResult.OK Then
                        '        DBScrapeMovie.Trailer = dTrailerSelect.Result
                        '    End If
                        'End Using

                        'autoscraping
                    ElseIf Not nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                        Dim newPreferredTrailer As New MediaContainers.Trailer
                        If Trailers.GetPreferredMovieTrailer(SearchResults, newPreferredTrailer) Then
                            DBScrapeMovie.Trailer = newPreferredTrailer
                        End If
                    End If
                End If
            End If

            'If bwMovieScraper.CancellationPending Then Exit For

            If nScraperTask.ScrapeType = Enums.ScrapeType.SingleScrape Then
                ModulesManager.Instance.RuntimeObjects.InvokeDialog_Edit_Movie(DBScrapeMovie)
            Else
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.ScraperMulti_Movie, Nothing, Nothing, False, DBScrapeMovie)
                'bwMovieScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(399, "Downloading and Saving Contents into Database"), ":"))
                Master.DB.Save_Movie(DBScrapeMovie, False, nScraperTask.ScrapeModifiers.MainNFO OrElse nScraperTask.ScrapeModifiers.MainMeta, True)
                'bwMovieScraper.ReportProgress(-2, DBScrapeMovie.ID)
                'bwMovieScraper.ReportProgress(-1, If(Not OldListTitle = NewListTitle, String.Format(Master.eLang.GetString(812, "Old Title: {0} | New Title: {1}"), OldListTitle, NewListTitle), NewListTitle))
            End If
            logger.Trace(String.Format("[Movie Scraper] [Done] Scraping {0}", OldListTitle))
        Else
            logger.Trace(String.Format("[Movie Scraper] [Cancelled] Scraping {0}", OldListTitle))
        End If
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Structure ScraperTask

        Dim mContentType As Enums.ContentType
        Dim mID As Long
        Dim ScrapeModifiers As Structures.ScrapeModifiers
        Dim ScrapeOptions As Structures.ScrapeOptions
        Dim ScrapeType As Enums.ScrapeType

    End Structure

#End Region 'Nested Types

End Class
