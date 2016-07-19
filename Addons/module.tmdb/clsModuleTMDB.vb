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

Public Class clsModuleTMDB
    Implements Interfaces.Base
    Implements Interfaces.ScraperEngine
    Implements Interfaces.SearchEngine

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeOptions_Movie As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_MovieSet As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_TV As New Structures.ScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_MovieSet As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_TV As New Structures.ScrapeModifiers

    Private _strPrivateAPIKey As String = String.Empty
    Private _SpecialSettings_Data_Movie As New SpecialSettings
    Private _SpecialSettings_Data_MovieSet As New SpecialSettings
    Private _SpecialSettings_Data_TV As New SpecialSettings
    Private _SpecialSettings_Trailer_Movie As New SpecialSettings

    'Scraper
    Private _ScraperEnabled_Data_Movie As Boolean = False
    Private _ScraperEnabled_Data_MovieSet As Boolean = False
    Private _ScraperEnabled_Data_TV As Boolean = False
    Private _ScraperEnabled_Image_Movie As Boolean = False
    Private _ScraperEnabled_Image_MovieSet As Boolean = False
    Private _ScraperEnabled_Image_TV As Boolean = False
    Private _ScraperEnabled_Search_Movie As Boolean = False
    Private _ScraperEnabled_Trailer_Movie As Boolean = False
    Private _sPanel_Data_Movie As frmSettingsPanel_Data_Movie
    Private _sPanel_Data_MovieSet As frmSettingsPanel_Data_MovieSet
    Private _sPanel_Data_TV As frmSettingsPanel_Data_TV
    Private _sPanel_Image_Movie As frmSettingsPanel_Image_Movie
    Private _sPanel_Image_MovieSet As frmSettingsPanel_Image_MovieSet
    Private _sPanel_Image_TV As frmSettingsPanel_Image_TV
    Private _sPanel_Search_Movie As frmSettingsPanel_Search_Movie
    Private _sPanel_Trailer_Movie As frmSettingsPanel_Trailer_Movie

    'SearchEngine
    Private _SearchEngineEnabled_Movie As Boolean = False
    Private _SearchEngineEnabled_MovieSet As Boolean = False
    Private _SearchEngineEnabled_TV As Boolean = False

#End Region 'Fields

#Region "Events"

    Public Event ModuleNeedsRestart() Implements Interfaces.Base.ModuleNeedsRestart
    Public Event ModuleSettingsChanged() Implements Interfaces.Base.ModuleSettingsChanged
    Public Event ModuleStateChanged(ByVal strAssemblyName As String, ByVal tPanelType As Enums.SettingsPanelType, ByVal bIsEnabled As Boolean, ByVal intDifforder As Integer) Implements Interfaces.Base.ModuleStateChanged

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleEnabled(ByVal tType As Enums.SettingsPanelType) As Boolean Implements Interfaces.ScraperEngine.ModuleEnabled, Interfaces.SearchEngine.ModuleEnabled
        Get
            Select Case tType
                Case Enums.SettingsPanelType.MovieData
                    Return _ScraperEnabled_Data_Movie
                Case Enums.SettingsPanelType.MovieSetData
                    Return _ScraperEnabled_Data_MovieSet
                Case Enums.SettingsPanelType.TVData
                    Return _ScraperEnabled_Data_TV
                Case Enums.SettingsPanelType.MovieImage
                    Return _ScraperEnabled_Image_Movie
                Case Enums.SettingsPanelType.MovieSetImage
                    Return _ScraperEnabled_Image_MovieSet
                Case Enums.SettingsPanelType.TVImage
                    Return _ScraperEnabled_Image_TV
                Case Enums.SettingsPanelType.MovieSearch
                    Return _SearchEngineEnabled_Movie
                Case Enums.SettingsPanelType.MovieSetSearch
                    Return _SearchEngineEnabled_MovieSet
                Case Enums.SettingsPanelType.TVSearch
                    Return _SearchEngineEnabled_TV
                Case Enums.SettingsPanelType.MovieTrailer
                    Return _ScraperEnabled_Trailer_Movie
                Case Else
                    Return False
            End Select
        End Get
    End Property

    ReadOnly Property ModuleName() As String Implements Interfaces.Base.ModuleName
        Get
            Return _AssemblyName
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.Base.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Private Sub Handle_ModuleNeedsRestart()
        RaiseEvent ModuleNeedsRestart()
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Handle_ModuleStateChanged(ByVal bIsEnabled As Boolean, ByVal tPanelType As Enums.SettingsPanelType, ByVal intDifforder As Integer)
        _ScraperEnabled_Data_Movie = bIsEnabled
        RaiseEvent ModuleStateChanged(_AssemblyName, tPanelType, bIsEnabled, intDifforder)
    End Sub

    Sub Init(ByVal strAssemblyName As String) Implements Interfaces.Base.Init
        _AssemblyName = strAssemblyName
        LoadSettings()
    End Sub

    Public Sub ScraperOrderChanged_Movie(ByVal tPanelType As Enums.SettingsPanelType) Implements Interfaces.Base.ModuleOrderChanged
        Select Case tPanelType
            Case Enums.SettingsPanelType.MovieData
                _sPanel_Data_Movie.orderChanged()
            Case Enums.SettingsPanelType.MovieSetData
                _sPanel_Data_MovieSet.orderChanged()
            Case Enums.SettingsPanelType.TVData
                _sPanel_Data_TV.orderChanged()
            Case Enums.SettingsPanelType.MovieImage
                _sPanel_Image_Movie.orderChanged()
            Case Enums.SettingsPanelType.MovieSetImage
                _sPanel_Image_MovieSet.orderChanged()
            Case Enums.SettingsPanelType.TVImage
                _sPanel_Image_TV.orderChanged()
            Case Enums.SettingsPanelType.MovieSearch
                '_sPanel_Search_Movie.orderChanged()
            Case Enums.SettingsPanelType.MovieSetSearch
                '_sPanel_Search_MovieSet.orderChanged()
            Case Enums.SettingsPanelType.TVSearch
                '_sPanel_Search_TV.orderChanged()
            Case Enums.SettingsPanelType.MovieTrailer
                _sPanel_Trailer_Movie.orderChanged()
        End Select
    End Sub

    Function QueryCapabilities_Scraper(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean Implements Interfaces.ScraperEngine.QueryCapabilities
        Select Case tContentType
            Case Enums.ContentType.Movie
                Select Case tModifierType
                    Case Enums.ModifierType.MainFanart
                        Return _ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return _ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainPoster
                    Case Enums.ModifierType.MainTrailer
                        Return _ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainTrailer
                End Select
            Case Enums.ContentType.MovieSet
                Select Case tModifierType
                    Case Enums.ModifierType.MainFanart
                        Return _ScraperEnabled_Image_MovieSet AndAlso ConfigScrapeModifier_MovieSet.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return _ScraperEnabled_Image_MovieSet AndAlso ConfigScrapeModifier_MovieSet.MainPoster
                End Select
            Case Enums.ContentType.TV, Enums.ContentType.TVEpisode, Enums.ContentType.TVSeason, Enums.ContentType.TVShow
                Select Case tModifierType
                    Case Enums.ModifierType.EpisodePoster
                        Return _ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.EpisodePoster
                    Case Enums.ModifierType.MainFanart
                        Return _ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return _ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.MainPoster
                    Case Enums.ModifierType.SeasonPoster
                        Return _ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.SeasonPoster
                End Select
        End Select
        Return False
    End Function

    Function QueryCapabilities_SearchEngine(ByVal tContentType As Enums.ContentType) As Boolean Implements Interfaces.SearchEngine.QueryCapabilities
        Select Case tContentType
            Case Enums.ContentType.Movie
                Return _SearchEngineEnabled_Movie
            Case Enums.ContentType.MovieSet
                Return _SearchEngineEnabled_MovieSet
            Case Enums.ContentType.TV, Enums.ContentType.TVEpisode, Enums.ContentType.TVSeason, Enums.ContentType.TVShow
                Return _SearchEngineEnabled_TV
        End Select
        Return False
    End Function

    Function RunScraper(ByRef DBElement As Database.DBElement) As Interfaces.ScrapeResults Implements Interfaces.ScraperEngine.RunScraper
        Dim tScraperResults As New Interfaces.ScrapeResults

        Select Case DBElement.ContentType
            Case Enums.ContentType.Movie
                logger.Trace("[TMDB] [RunScraper] [Movie] [Start]")

                LoadSettings()
                _SpecialSettings_Data_Movie.PrefLanguage = DBElement.Language

                Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Data_Movie)

                Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(DBElement.ScrapeOptions, ConfigScrapeOptions_Movie)
                Dim FilteredModifiers As Structures.ScrapeModifiers = Functions.ScrapeModifiersAndAlso(DBElement.ScrapeModifiers, ConfigScrapeModifier_Movie)

                If DBElement.Movie.TMDBSpecified Then
                    'TMDB-ID already available -> scrape and save data into an empty movie container (nMovie)
                    tScraperResults.tResult = _scraper.GetInfo_Movie(DBElement.Movie.TMDB, FilteredModifiers, FilteredOptions)
                ElseIf DBElement.Movie.IMDBSpecified Then
                    'IMDB-ID already available -> scrape and save data into an empty movie container (nMovie)
                    tScraperResults.tResult = _scraper.GetInfo_Movie(DBElement.Movie.IMDB, FilteredModifiers, FilteredOptions)
                Else
                    logger.Trace("[TMDB] [RunScraper] [Movie] [Abort] No TMDB/IMDB ID")
                    Return New Interfaces.ScrapeResults
                End If

                logger.Trace("[TMDB] [RunScraper] [Movie] [Done]")
                Return tScraperResults
        End Select

        Return tScraperResults
    End Function

    Function RunSearch(ByVal strTitle As String, ByVal intYear As Integer, ByVal strLanguage As String, ByVal tContentType As Enums.ContentType) As Interfaces.SearchResults Implements Interfaces.SearchEngine.RunSearch
        Dim nSearchResults As New Interfaces.SearchResults

        LoadSettings()
        _SpecialSettings_Data_Movie.PrefLanguage = strLanguage

        Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Trailer_Movie)

        Select Case tContentType
            Case Enums.ContentType.Movie
                nSearchResults.tResult = _scraper.Search_Movie(strTitle, intYear)
            Case Enums.ContentType.MovieSet
                nSearchResults.tResult.MovieSets = _scraper.Search_MovieSet(strTitle)
            Case Enums.ContentType.TVShow
                nSearchResults.tResult.TVShows = _scraper.Search_TVShow(strTitle)
        End Select

        Return nSearchResults
    End Function

    Function InjectSettingsPanels() As List(Of Containers.SettingsPanel) Implements Interfaces.Base.InjectSettingsPanels
        LoadSettings()
        Dim sPanelList As New List(Of Containers.SettingsPanel)
        sPanelList.Add(InjectSettingsPanel_Data_Movie)
        sPanelList.Add(InjectSettingsPanel_Data_MovieSet)
        sPanelList.Add(InjectSettingsPanel_Data_TV)
        sPanelList.Add(InjectSettingsPanel_Image_Movie)
        sPanelList.Add(InjectSettingsPanel_Image_MovieSet)
        sPanelList.Add(InjectSettingsPanel_Image_TV)
        sPanelList.Add(InjectSettingsPanel_Search_Movie)
        sPanelList.Add(InjectSettingsPanel_Trailer_Movie)

        Return sPanelList
    End Function

    Function InjectSettingsPanel_Data_Movie() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieData)
        _sPanel_Data_Movie = New frmSettingsPanel_Data_Movie
        _sPanel_Data_Movie.chkEnabled.Checked = _ScraperEnabled_Data_Movie
        _sPanel_Data_Movie.chkActors.Checked = ConfigScrapeOptions_Movie.bMainActors
        _sPanel_Data_Movie.chkCollectionID.Checked = ConfigScrapeOptions_Movie.bMainCollectionID
        _sPanel_Data_Movie.chkCountries.Checked = ConfigScrapeOptions_Movie.bMainCountries
        _sPanel_Data_Movie.chkDirectors.Checked = ConfigScrapeOptions_Movie.bMainDirectors
        _sPanel_Data_Movie.chkFallBackEng.Checked = _SpecialSettings_Data_Movie.FallBackEng
        _sPanel_Data_Movie.chkGenres.Checked = ConfigScrapeOptions_Movie.bMainGenres
        _sPanel_Data_Movie.chkGetAdultItems.Checked = _SpecialSettings_Data_Movie.GetAdultItems
        _sPanel_Data_Movie.chkCertifications.Checked = ConfigScrapeOptions_Movie.bMainMPAA
        _sPanel_Data_Movie.chkOriginalTitle.Checked = ConfigScrapeOptions_Movie.bMainOriginalTitle
        _sPanel_Data_Movie.chkPlot.Checked = ConfigScrapeOptions_Movie.bMainPlot
        _sPanel_Data_Movie.chkRating.Checked = ConfigScrapeOptions_Movie.bMainRating
        _sPanel_Data_Movie.chkRelease.Checked = ConfigScrapeOptions_Movie.bMainRelease
        _sPanel_Data_Movie.chkRuntime.Checked = ConfigScrapeOptions_Movie.bMainRuntime
        _sPanel_Data_Movie.chkSearchDeviant.Checked = _SpecialSettings_Data_Movie.SearchDeviant
        _sPanel_Data_Movie.chkStudios.Checked = ConfigScrapeOptions_Movie.bMainStudios
        _sPanel_Data_Movie.chkTagline.Checked = ConfigScrapeOptions_Movie.bMainTagline
        _sPanel_Data_Movie.chkTitle.Checked = ConfigScrapeOptions_Movie.bMainTitle
        _sPanel_Data_Movie.chkTrailer.Checked = ConfigScrapeOptions_Movie.bMainTrailer
        _sPanel_Data_Movie.chkWriters.Checked = ConfigScrapeOptions_Movie.bMainWriters
        _sPanel_Data_Movie.chkYear.Checked = ConfigScrapeOptions_Movie.bMainYear
        _sPanel_Data_Movie.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Data_Movie.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieInfo_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Data_Movie, 9, 10)
        sPanel.Panel = _sPanel_Data_Movie.pnlSettings

        AddHandler _sPanel_Data_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Data_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Data_MovieSet() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieSetData)
        _sPanel_Data_MovieSet = New frmSettingsPanel_Data_MovieSet
        _sPanel_Data_MovieSet.chkEnabled.Checked = _ScraperEnabled_Data_MovieSet
        _sPanel_Data_MovieSet.chkFallBackEng.Checked = _SpecialSettings_Data_MovieSet.FallBackEng
        _sPanel_Data_MovieSet.chkGetAdultItems.Checked = _SpecialSettings_Data_MovieSet.GetAdultItems
        _sPanel_Data_MovieSet.chkPlot.Checked = ConfigScrapeOptions_MovieSet.bMainPlot
        _sPanel_Data_MovieSet.chkTitle.Checked = ConfigScrapeOptions_MovieSet.bMainTitle
        _sPanel_Data_MovieSet.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Data_MovieSet.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieSetInfo_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Data_MovieSet, 9, 10)
        sPanel.Panel = _sPanel_Data_MovieSet.pnlSettings

        AddHandler _sPanel_Data_MovieSet.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Data_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_MovieSet.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Data_TV() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.TVData)
        _sPanel_Data_TV = New frmSettingsPanel_Data_TV
        _sPanel_Data_TV.chkEnabled.Checked = _ScraperEnabled_Data_TV
        _sPanel_Data_TV.chkFallBackEng.Checked = _SpecialSettings_Data_TV.FallBackEng
        _sPanel_Data_TV.chkGetAdultItems.Checked = _SpecialSettings_Data_TV.GetAdultItems
        _sPanel_Data_TV.chkScraperEpisodeActors.Checked = ConfigScrapeOptions_TV.bEpisodeActors
        _sPanel_Data_TV.chkScraperEpisodeAired.Checked = ConfigScrapeOptions_TV.bEpisodeAired
        _sPanel_Data_TV.chkScraperEpisodeCredits.Checked = ConfigScrapeOptions_TV.bEpisodeCredits
        _sPanel_Data_TV.chkScraperEpisodeDirectors.Checked = ConfigScrapeOptions_TV.bEpisodeDirectors
        _sPanel_Data_TV.chkScraperEpisodeGuestStars.Checked = ConfigScrapeOptions_TV.bEpisodeGuestStars
        _sPanel_Data_TV.chkScraperEpisodePlot.Checked = ConfigScrapeOptions_TV.bEpisodePlot
        _sPanel_Data_TV.chkScraperEpisodeRating.Checked = ConfigScrapeOptions_TV.bEpisodeRating
        _sPanel_Data_TV.chkScraperEpisodeTitle.Checked = ConfigScrapeOptions_TV.bEpisodeTitle
        _sPanel_Data_TV.chkScraperSeasonAired.Checked = ConfigScrapeOptions_TV.bSeasonAired
        _sPanel_Data_TV.chkScraperSeasonPlot.Checked = ConfigScrapeOptions_TV.bSeasonPlot
        _sPanel_Data_TV.chkScraperSeasonTitle.Checked = ConfigScrapeOptions_TV.bSeasonTitle
        _sPanel_Data_TV.chkScraperShowActors.Checked = ConfigScrapeOptions_TV.bMainActors
        _sPanel_Data_TV.chkScraperShowCertifications.Checked = ConfigScrapeOptions_TV.bMainCertifications
        _sPanel_Data_TV.chkScraperShowCountries.Checked = ConfigScrapeOptions_TV.bMainCountries
        _sPanel_Data_TV.chkScraperShowCreators.Checked = ConfigScrapeOptions_TV.bMainCreators
        _sPanel_Data_TV.chkScraperShowGenres.Checked = ConfigScrapeOptions_TV.bMainGenres
        _sPanel_Data_TV.chkScraperShowOriginalTitle.Checked = ConfigScrapeOptions_TV.bMainOriginalTitle
        _sPanel_Data_TV.chkScraperShowPlot.Checked = ConfigScrapeOptions_TV.bMainPlot
        _sPanel_Data_TV.chkScraperShowPremiered.Checked = ConfigScrapeOptions_TV.bMainPremiered
        _sPanel_Data_TV.chkScraperShowRating.Checked = ConfigScrapeOptions_TV.bMainRating
        _sPanel_Data_TV.chkScraperShowRuntime.Checked = ConfigScrapeOptions_TV.bMainRuntime
        _sPanel_Data_TV.chkScraperShowStatus.Checked = ConfigScrapeOptions_TV.bMainStatus
        _sPanel_Data_TV.chkScraperShowStudios.Checked = ConfigScrapeOptions_TV.bMainStudios
        _sPanel_Data_TV.chkScraperShowTitle.Checked = ConfigScrapeOptions_TV.bMainTitle
        _sPanel_Data_TV.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Data_TV.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTVInfo_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Data_TV, 9, 10)
        sPanel.Panel = _sPanel_Data_TV.pnlSettings

        AddHandler _sPanel_Data_TV.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Data_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_TV.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Image_Movie() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieImage)
        _sPanel_Image_Movie = New frmSettingsPanel_Image_Movie
        _sPanel_Image_Movie.chkEnabled.Checked = _ScraperEnabled_Image_Movie
        _sPanel_Image_Movie.chkScrapeFanart.Checked = ConfigScrapeModifier_Movie.MainFanart
        _sPanel_Image_Movie.chkScrapePoster.Checked = ConfigScrapeModifier_Movie.MainPoster
        _sPanel_Image_Movie.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Image_Movie.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieMedia_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Image_Movie, 9, 10)
        sPanel.Panel = _sPanel_Image_Movie.pnlSettings

        AddHandler _sPanel_Image_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Image_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Image_MovieSet() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieSetImage)
        _sPanel_Image_MovieSet = New frmSettingsPanel_Image_MovieSet
        _sPanel_Image_MovieSet.chkEnabled.Checked = _ScraperEnabled_Image_MovieSet
        _sPanel_Image_MovieSet.chkScrapeFanart.Checked = ConfigScrapeModifier_MovieSet.MainFanart
        _sPanel_Image_MovieSet.chkScrapePoster.Checked = ConfigScrapeModifier_MovieSet.MainPoster
        _sPanel_Image_MovieSet.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Image_MovieSet.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieSetMedia_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Image_MovieSet, 9, 10)
        sPanel.Panel = _sPanel_Image_MovieSet.pnlSettings

        AddHandler _sPanel_Image_MovieSet.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Image_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_MovieSet.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Image_TV() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.TVImage)
        _sPanel_Image_TV = New frmSettingsPanel_Image_TV
        _sPanel_Image_TV.chkEnabled.Checked = _ScraperEnabled_Image_TV
        _sPanel_Image_TV.chkScrapeEpisodePoster.Checked = ConfigScrapeModifier_TV.EpisodePoster
        _sPanel_Image_TV.chkScrapeSeasonPoster.Checked = ConfigScrapeModifier_TV.SeasonPoster
        _sPanel_Image_TV.chkScrapeShowFanart.Checked = ConfigScrapeModifier_TV.MainFanart
        _sPanel_Image_TV.chkScrapeShowPoster.Checked = ConfigScrapeModifier_TV.MainPoster
        _sPanel_Image_TV.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Image_TV.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTVMedia_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Image_TV, 9, 10)
        sPanel.Panel = _sPanel_Image_TV.pnlSettings

        AddHandler _sPanel_Image_TV.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Image_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_TV.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Search_Movie() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieSearch)
        _sPanel_Search_Movie = New frmSettingsPanel_Search_Movie
        _sPanel_Search_Movie.chkEnabled.Checked = _ScraperEnabled_Data_Movie
        _sPanel_Search_Movie.chkFallBackEng.Checked = _SpecialSettings_Data_Movie.FallBackEng
        _sPanel_Search_Movie.chkGetAdultItems.Checked = _SpecialSettings_Data_Movie.GetAdultItems
        _sPanel_Search_Movie.chkSearchDeviant.Checked = _SpecialSettings_Data_Movie.SearchDeviant
        _sPanel_Search_Movie.txtApiKey.Text = _strPrivateAPIKey

        _sPanel_Search_Movie.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieInfo_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Search_Movie, 9, 10)
        sPanel.Panel = _sPanel_Search_Movie.pnlSettings

        AddHandler _sPanel_Search_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Search_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Search_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Function InjectSettingsPanel_Trailer_Movie() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieTrailer)
        _sPanel_Trailer_Movie = New frmSettingsPanel_Trailer_Movie
        _sPanel_Trailer_Movie.chkEnabled.Checked = _ScraperEnabled_Trailer_Movie
        _sPanel_Trailer_Movie.txtApiKey.Text = _strPrivateAPIKey
        _sPanel_Trailer_Movie.chkFallBackEng.Checked = _SpecialSettings_Trailer_Movie.FallBackEng

        _sPanel_Trailer_Movie.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTrailer_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Trailer_Movie, 9, 10)
        sPanel.Panel = _sPanel_Trailer_Movie.pnlSettings

        AddHandler _sPanel_Trailer_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Trailer_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Sub LoadSettings()
        'Data Movie
        ConfigScrapeOptions_Movie.bMainActors = AdvancedSettings.GetBooleanSetting("DoCast", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainCertifications = AdvancedSettings.GetBooleanSetting("DoCert", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainCollectionID = AdvancedSettings.GetBooleanSetting("DoCollectionID", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainCountries = AdvancedSettings.GetBooleanSetting("DoCountry", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainDirectors = AdvancedSettings.GetBooleanSetting("DoDirector", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainGenres = AdvancedSettings.GetBooleanSetting("DoGenres", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainMPAA = AdvancedSettings.GetBooleanSetting("DoMPAA", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainOriginalTitle = AdvancedSettings.GetBooleanSetting("DoOriginalTitle", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainOutline = AdvancedSettings.GetBooleanSetting("DoOutline", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainRating = AdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainRelease = AdvancedSettings.GetBooleanSetting("DoRelease", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainRuntime = AdvancedSettings.GetBooleanSetting("DoRuntime", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainStudios = AdvancedSettings.GetBooleanSetting("DoStudio", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainTagline = AdvancedSettings.GetBooleanSetting("DoTagline", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainTop250 = AdvancedSettings.GetBooleanSetting("DoTop250", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainTrailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainWriters = AdvancedSettings.GetBooleanSetting("DoWriters", True, , Enums.ContentType.Movie)
        ConfigScrapeOptions_Movie.bMainYear = AdvancedSettings.GetBooleanSetting("DoYear", True, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.SearchDeviant = AdvancedSettings.GetBooleanSetting("SearchDeviant", False, , Enums.ContentType.Movie)

        'Data MovieSet
        ConfigScrapeOptions_MovieSet.bMainPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.MovieSet)
        ConfigScrapeOptions_MovieSet.bMainTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.MovieSet)

        'Data TV
        ConfigScrapeOptions_TV.bEpisodeActors = AdvancedSettings.GetBooleanSetting("DoActors", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeAired = AdvancedSettings.GetBooleanSetting("DoAired", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeCredits = AdvancedSettings.GetBooleanSetting("DoCredits", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeDirectors = AdvancedSettings.GetBooleanSetting("DoDirector", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeGuestStars = AdvancedSettings.GetBooleanSetting("DoGuestStars", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodePlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeRating = AdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bSeasonAired = AdvancedSettings.GetBooleanSetting("DoAired", True, , Enums.ContentType.TVSeason)
        ConfigScrapeOptions_TV.bSeasonPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.TVSeason)
        ConfigScrapeOptions_TV.bSeasonTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.TVSeason)
        ConfigScrapeOptions_TV.bMainActors = AdvancedSettings.GetBooleanSetting("DoActors", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainCertifications = AdvancedSettings.GetBooleanSetting("DoCert", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainCountries = AdvancedSettings.GetBooleanSetting("DoCountry", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainCreators = AdvancedSettings.GetBooleanSetting("DoCreator", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainEpisodeGuide = AdvancedSettings.GetBooleanSetting("DoEpisodeGuide", False, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainGenres = AdvancedSettings.GetBooleanSetting("DoGenre", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainOriginalTitle = AdvancedSettings.GetBooleanSetting("DoOriginalTitle", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainPremiered = AdvancedSettings.GetBooleanSetting("DoPremiered", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainRating = AdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainRuntime = AdvancedSettings.GetBooleanSetting("DoRuntime", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainStatus = AdvancedSettings.GetBooleanSetting("DoStatus", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainStudios = AdvancedSettings.GetBooleanSetting("DoStudio", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.TVShow)
        _SpecialSettings_Data_TV.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.TV)
        _SpecialSettings_Data_TV.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.TV)

        'Images Movie
        ConfigScrapeModifier_Movie.MainPoster = AdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.ContentType.Movie)
        ConfigScrapeModifier_Movie.MainFanart = AdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.ContentType.Movie)
        ConfigScrapeModifier_Movie.MainExtrafanarts = ConfigScrapeModifier_Movie.MainFanart
        ConfigScrapeModifier_Movie.MainExtrathumbs = ConfigScrapeModifier_Movie.MainFanart

        'Images MovieSet
        ConfigScrapeModifier_MovieSet.MainPoster = AdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.ContentType.MovieSet)
        ConfigScrapeModifier_MovieSet.MainFanart = AdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.ContentType.MovieSet)
        ConfigScrapeModifier_MovieSet.MainExtrafanarts = ConfigScrapeModifier_MovieSet.MainFanart
        ConfigScrapeModifier_MovieSet.MainExtrathumbs = ConfigScrapeModifier_MovieSet.MainFanart

        'Images TV
        ConfigScrapeModifier_TV.EpisodePoster = AdvancedSettings.GetBooleanSetting("DoEpisodePoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.SeasonPoster = AdvancedSettings.GetBooleanSetting("DoSeasonPoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainFanart = AdvancedSettings.GetBooleanSetting("DoShowFanart", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainPoster = AdvancedSettings.GetBooleanSetting("DoShowPoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainExtrafanarts = ConfigScrapeModifier_TV.MainFanart

        'Trailer Movie
        ConfigScrapeModifier_Movie.MainTrailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True)
        _SpecialSettings_Trailer_Movie.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False)

        'Global
        _strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", String.Empty)
        _SpecialSettings_Data_Movie.APIKey = If(String.IsNullOrEmpty(_strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", _strPrivateAPIKey)
        _SpecialSettings_Data_MovieSet.APIKey = If(String.IsNullOrEmpty(_strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", _strPrivateAPIKey)
        _SpecialSettings_Data_TV.APIKey = If(String.IsNullOrEmpty(_strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", _strPrivateAPIKey)
        _SpecialSettings_Trailer_Movie.APIKey = If(String.IsNullOrEmpty(_strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", _strPrivateAPIKey)
    End Sub

    Sub SaveSettings()
        Using settings = New AdvancedSettings()
            'Data Movie
            settings.SetBooleanSetting("DoCast", ConfigScrapeOptions_Movie.bMainActors, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoCert", ConfigScrapeOptions_Movie.bMainCertifications, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoCollectionID", ConfigScrapeOptions_Movie.bMainCollectionID, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoCountry", ConfigScrapeOptions_Movie.bMainCountries, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoDirector", ConfigScrapeOptions_Movie.bMainDirectors, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_Movie.MainFanart, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoGenres", ConfigScrapeOptions_Movie.bMainGenres, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoMPAA", ConfigScrapeOptions_Movie.bMainMPAA, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoOriginalTitle", ConfigScrapeOptions_Movie.bMainOriginalTitle, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoOutline", ConfigScrapeOptions_Movie.bMainOutline, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_Movie.bMainPlot, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_Movie.MainPoster, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_Movie.bMainRating, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoRelease", ConfigScrapeOptions_Movie.bMainRelease, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoRuntime", ConfigScrapeOptions_Movie.bMainRuntime, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoStudio", ConfigScrapeOptions_Movie.bMainStudios, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoTagline", ConfigScrapeOptions_Movie.bMainTagline, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_Movie.bMainTitle, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoTop250", ConfigScrapeOptions_Movie.bMainTop250, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoTrailer", ConfigScrapeOptions_Movie.bMainTrailer, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoWriters", ConfigScrapeOptions_Movie.bMainWriters, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoYear", ConfigScrapeOptions_Movie.bMainYear, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("FallBackEn", _SpecialSettings_Data_Movie.FallBackEng, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("GetAdultItems", _SpecialSettings_Data_Movie.GetAdultItems, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("SearchDeviant", _SpecialSettings_Data_Movie.SearchDeviant, , , Enums.ContentType.Movie)

            'Data MovieSet
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_MovieSet.bMainPlot, , , Enums.ContentType.MovieSet)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_MovieSet.bMainTitle, , , Enums.ContentType.MovieSet)
            settings.SetBooleanSetting("GetAdultItems", _SpecialSettings_Data_MovieSet.GetAdultItems, , , Enums.ContentType.MovieSet)

            'Data TV
            settings.SetBooleanSetting("DoActors", ConfigScrapeOptions_TV.bEpisodeActors, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoAired", ConfigScrapeOptions_TV.bEpisodeAired, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoCredits", ConfigScrapeOptions_TV.bEpisodeCredits, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoDirector", ConfigScrapeOptions_TV.bEpisodeDirectors, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoGuestStars", ConfigScrapeOptions_TV.bEpisodeGuestStars, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_TV.bEpisodePlot, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_TV.bEpisodeRating, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_TV.bEpisodeTitle, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoAired", ConfigScrapeOptions_TV.bSeasonAired, , , Enums.ContentType.TVSeason)
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_TV.bSeasonPlot, , , Enums.ContentType.TVSeason)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_TV.bSeasonTitle, , , Enums.ContentType.TVSeason)
            settings.SetBooleanSetting("DoActors", ConfigScrapeOptions_TV.bMainActors, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoCert", ConfigScrapeOptions_TV.bMainCertifications, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoCountry", ConfigScrapeOptions_TV.bMainCountries, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoCreator", ConfigScrapeOptions_TV.bMainCreators, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoEpisodeGuide", ConfigScrapeOptions_TV.bMainEpisodeGuide, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoGenre", ConfigScrapeOptions_TV.bMainGenres, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoOriginalTitle", ConfigScrapeOptions_TV.bMainOriginalTitle, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_TV.bMainPlot, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoPremiered", ConfigScrapeOptions_TV.bMainPremiered, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_TV.bMainRating, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoStatus", ConfigScrapeOptions_TV.bMainStatus, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoStudio", ConfigScrapeOptions_TV.bMainStudios, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_TV.bMainTitle, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("FallBackEn", _SpecialSettings_Data_TV.FallBackEng, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("GetAdultItems", _SpecialSettings_Data_TV.GetAdultItems, , , Enums.ContentType.TV)

            'Image Movie
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_Movie.MainPoster, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_Movie.MainFanart, , , Enums.ContentType.Movie)

            'Image MovieSet
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_MovieSet.MainPoster, , , Enums.ContentType.MovieSet)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_MovieSet.MainFanart, , , Enums.ContentType.MovieSet)

            'Image TV
            settings.SetBooleanSetting("DoEpisodePoster", ConfigScrapeModifier_TV.EpisodePoster, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoSeasonPoster", ConfigScrapeModifier_TV.SeasonPoster, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoShowFanart", ConfigScrapeModifier_TV.MainFanart, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoShowPoster", ConfigScrapeModifier_TV.MainPoster, , , Enums.ContentType.TV)

            'Trailer Movie
            settings.SetBooleanSetting("DoTrailer", ConfigScrapeModifier_Movie.MainTrailer, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("FallBackEn", _SpecialSettings_Data_Movie.FallBackEng, , , Enums.ContentType.Movie)

            'Global
            settings.SetSetting("APIKey", _strPrivateAPIKey)
        End Using
    End Sub

    Sub SaveSettingsPanel(ByVal DoDispose As Boolean) Implements Interfaces.Base.SaveSettingsPanel
        'Data Movie
        ConfigScrapeOptions_Movie.bMainActors = _sPanel_Data_Movie.chkActors.Checked
        ConfigScrapeOptions_Movie.bMainCertifications = _sPanel_Data_Movie.chkCertifications.Checked
        ConfigScrapeOptions_Movie.bMainCollectionID = _sPanel_Data_Movie.chkCollectionID.Checked
        ConfigScrapeOptions_Movie.bMainCountries = _sPanel_Data_Movie.chkCountries.Checked
        ConfigScrapeOptions_Movie.bMainDirectors = _sPanel_Data_Movie.chkDirectors.Checked
        ConfigScrapeOptions_Movie.bMainGenres = _sPanel_Data_Movie.chkGenres.Checked
        ConfigScrapeOptions_Movie.bMainMPAA = _sPanel_Data_Movie.chkCertifications.Checked
        ConfigScrapeOptions_Movie.bMainOriginalTitle = _sPanel_Data_Movie.chkOriginalTitle.Checked
        ConfigScrapeOptions_Movie.bMainOutline = _sPanel_Data_Movie.chkPlot.Checked
        ConfigScrapeOptions_Movie.bMainPlot = _sPanel_Data_Movie.chkPlot.Checked
        ConfigScrapeOptions_Movie.bMainRating = _sPanel_Data_Movie.chkRating.Checked
        ConfigScrapeOptions_Movie.bMainRelease = _sPanel_Data_Movie.chkRelease.Checked
        ConfigScrapeOptions_Movie.bMainRuntime = _sPanel_Data_Movie.chkRuntime.Checked
        ConfigScrapeOptions_Movie.bMainStudios = _sPanel_Data_Movie.chkStudios.Checked
        ConfigScrapeOptions_Movie.bMainTagline = _sPanel_Data_Movie.chkTagline.Checked
        ConfigScrapeOptions_Movie.bMainTitle = _sPanel_Data_Movie.chkTitle.Checked
        ConfigScrapeOptions_Movie.bMainTop250 = False
        ConfigScrapeOptions_Movie.bMainTrailer = _sPanel_Data_Movie.chkTrailer.Checked
        ConfigScrapeOptions_Movie.bMainWriters = _sPanel_Data_Movie.chkWriters.Checked
        ConfigScrapeOptions_Movie.bMainYear = _sPanel_Data_Movie.chkYear.Checked
        _SpecialSettings_Data_Movie.FallBackEng = _sPanel_Data_Movie.chkFallBackEng.Checked
        _SpecialSettings_Data_Movie.GetAdultItems = _sPanel_Data_Movie.chkGetAdultItems.Checked
        _SpecialSettings_Data_Movie.SearchDeviant = _sPanel_Data_Movie.chkSearchDeviant.Checked

        'Data Movieset
        ConfigScrapeOptions_MovieSet.bMainPlot = _sPanel_Data_MovieSet.chkPlot.Checked
        ConfigScrapeOptions_MovieSet.bMainTitle = _sPanel_Data_MovieSet.chkTitle.Checked
        _SpecialSettings_Data_MovieSet.FallBackEng = _sPanel_Data_MovieSet.chkFallBackEng.Checked
        _SpecialSettings_Data_MovieSet.GetAdultItems = _sPanel_Data_MovieSet.chkGetAdultItems.Checked

        'Data TV
        ConfigScrapeOptions_TV.bEpisodeActors = _sPanel_Data_TV.chkScraperEpisodeActors.Checked
        ConfigScrapeOptions_TV.bEpisodeAired = _sPanel_Data_TV.chkScraperEpisodeAired.Checked
        ConfigScrapeOptions_TV.bEpisodeCredits = _sPanel_Data_TV.chkScraperEpisodeCredits.Checked
        ConfigScrapeOptions_TV.bEpisodeDirectors = _sPanel_Data_TV.chkScraperEpisodeDirectors.Checked
        ConfigScrapeOptions_TV.bEpisodeGuestStars = _sPanel_Data_TV.chkScraperEpisodeGuestStars.Checked
        ConfigScrapeOptions_TV.bEpisodePlot = _sPanel_Data_TV.chkScraperEpisodePlot.Checked
        ConfigScrapeOptions_TV.bEpisodeRating = _sPanel_Data_TV.chkScraperEpisodeRating.Checked
        ConfigScrapeOptions_TV.bEpisodeTitle = _sPanel_Data_TV.chkScraperEpisodeTitle.Checked
        ConfigScrapeOptions_TV.bMainActors = _sPanel_Data_TV.chkScraperShowActors.Checked
        ConfigScrapeOptions_TV.bMainCertifications = _sPanel_Data_TV.chkScraperShowCertifications.Checked
        ConfigScrapeOptions_TV.bMainCreators = _sPanel_Data_TV.chkScraperShowCreators.Checked
        ConfigScrapeOptions_TV.bMainGenres = _sPanel_Data_TV.chkScraperShowGenres.Checked
        ConfigScrapeOptions_TV.bMainOriginalTitle = _sPanel_Data_TV.chkScraperShowOriginalTitle.Checked
        ConfigScrapeOptions_TV.bMainPlot = _sPanel_Data_TV.chkScraperShowPlot.Checked
        ConfigScrapeOptions_TV.bMainPremiered = _sPanel_Data_TV.chkScraperShowPremiered.Checked
        ConfigScrapeOptions_TV.bMainRating = _sPanel_Data_TV.chkScraperShowRating.Checked
        ConfigScrapeOptions_TV.bMainRuntime = _sPanel_Data_TV.chkScraperShowRuntime.Checked
        ConfigScrapeOptions_TV.bMainStatus = _sPanel_Data_TV.chkScraperShowStatus.Checked
        ConfigScrapeOptions_TV.bMainStudios = _sPanel_Data_TV.chkScraperShowStudios.Checked
        ConfigScrapeOptions_TV.bMainTitle = _sPanel_Data_TV.chkScraperShowTitle.Checked
        ConfigScrapeOptions_TV.bSeasonAired = _sPanel_Data_TV.chkScraperSeasonAired.Checked
        ConfigScrapeOptions_TV.bSeasonPlot = _sPanel_Data_TV.chkScraperSeasonPlot.Checked
        ConfigScrapeOptions_TV.bSeasonTitle = _sPanel_Data_TV.chkScraperSeasonTitle.Checked
        _SpecialSettings_Data_TV.FallBackEng = _sPanel_Data_TV.chkFallBackEng.Checked
        _SpecialSettings_Data_TV.GetAdultItems = _sPanel_Data_TV.chkGetAdultItems.Checked

        'Image Movie
        ConfigScrapeModifier_Movie.MainPoster = _sPanel_Image_Movie.chkScrapePoster.Checked
        ConfigScrapeModifier_Movie.MainFanart = _sPanel_Image_Movie.chkScrapeFanart.Checked

        'Image MovieSet
        ConfigScrapeModifier_MovieSet.MainPoster = _sPanel_Image_MovieSet.chkScrapePoster.Checked
        ConfigScrapeModifier_MovieSet.MainFanart = _sPanel_Image_MovieSet.chkScrapeFanart.Checked

        'Image TV
        ConfigScrapeModifier_TV.EpisodePoster = _sPanel_Image_TV.chkScrapeEpisodePoster.Checked
        ConfigScrapeModifier_TV.SeasonPoster = _sPanel_Image_TV.chkScrapeSeasonPoster.Checked
        ConfigScrapeModifier_TV.MainFanart = _sPanel_Image_TV.chkScrapeShowFanart.Checked
        ConfigScrapeModifier_TV.MainPoster = _sPanel_Image_TV.chkScrapeShowPoster.Checked

        'Trailer Movie
        _SpecialSettings_Data_Movie.FallBackEng = _sPanel_Trailer_Movie.chkFallBackEng.Checked

        'Global
        _strPrivateAPIKey = _sPanel_Data_Movie.txtApiKey.Text

        SaveSettings()

        If DoDispose Then
            'Data Movie
            RemoveHandler _sPanel_Data_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Data_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Data_Movie.Dispose()

            'Data MovieSet
            RemoveHandler _sPanel_Data_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Data_MovieSet.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Data_MovieSet.Dispose()

            'Data TV
            RemoveHandler _sPanel_Data_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Data_TV.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Data_TV.Dispose()

            'Image Movie
            RemoveHandler _sPanel_Image_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
            RemoveHandler _sPanel_Image_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Image_Movie.Dispose()

            'Image MovieSet
            RemoveHandler _sPanel_Image_MovieSet.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
            RemoveHandler _sPanel_Image_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_MovieSet.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Image_MovieSet.Dispose()

            'Image TV
            RemoveHandler _sPanel_Image_TV.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
            RemoveHandler _sPanel_Image_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_TV.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Image_TV.Dispose()

            'Trailer Movie
            RemoveHandler _sPanel_Trailer_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
            RemoveHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Trailer_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Trailer_Movie.Dispose()
        End If
    End Sub

    'Function Scraper_MovieSet(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_MovieSet
    '    logger.Trace("[TMDB_Data] [Scraper_MovieSet] [Start]")

    '    LoadSettings_Data_MovieSet()
    '    _SpecialSettings_Data_MovieSet.PrefLanguage = oDBElement.Language

    '    Dim nMovieSet As MediaContainers.MovieSet = Nothing
    '    Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Data_MovieSet)

    '    Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_MovieSet)

    '    If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
    '        If oDBElement.MovieSet.TMDBSpecified Then
    '            'TMDB-ID already available -> scrape and save data into an empty movieset container (nMovieSet)
    '            nMovieSet = _scraper.GetMovieSetInfo(oDBElement.MovieSet.TMDB, FilteredOptions, False)
    '        ElseIf Not ScrapeType = Enums.ScrapeType.SingleScrape Then
    '            'no ITMDB-ID for movieset --> search first and try to get ID!
    '            If oDBElement.MovieSet.TitleSpecified Then
    '                nMovieSet = _scraper.GetSearchMovieSetInfo(oDBElement.MovieSet.Title, oDBElement, ScrapeType, FilteredOptions)
    '            End If
    '            'if still no search result -> exit
    '            If nMovieSet Is Nothing Then
    '                logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Abort] No search result found"))
    '                Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = Nothing}
    '            End If
    '        End If
    '    End If

    '    If nMovieSet Is Nothing Then
    '        Select Case ScrapeType
    '            Case Enums.ScrapeType.AllAuto, Enums.ScrapeType.FilterAuto, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.SelectedAuto
    '                logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Abort] No search result found"))
    '                Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = Nothing}
    '        End Select
    '    Else
    '        Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = nMovieSet}
    '    End If

    '    If ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto Then
    '        If Not oDBElement.MovieSet.TMDBSpecified Then
    '            'Using dlgSearch As New dlgTMDBSearchResults_MovieSet(_SpecialSettings_MovieSet, _scraper)
    '            '    If dlgSearch.ShowDialog(oDBElement.MovieSet.Title, FilteredOptions) = DialogResult.OK Then
    '            '        nMovieSet = _scraper.GetMovieSetInfo(dlgSearch.Result.TMDB, FilteredOptions, False)
    '            '        'if a movieset is found, set DoSearch back to "false" for following scrapers
    '            '        ScrapeModifiers.DoSearch = False
    '            '    Else
    '            '        logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Cancelled] Cancelled by user"))
    '            '        Return New Interfaces.ModuleResult_Data_MovieSet With {.Cancelled = True, .Result = Nothing}
    '            '    End If
    '            'End Using
    '        End If
    '    End If

    '    logger.Trace("[TMDB_Data] [Scraper_MovieSet] [Done]")
    '    Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = nMovieSet}
    'End Function
    ''' <summary>
    '''  Scrape MovieDetails from TMDB
    ''' </summary>
    ''' <param name="oDBTV">TV Show to be scraped. DBTV as ByRef to use existing data for identifing tv show and to fill with IMDB/TMDB/TVDB ID for next scraper</param>
    ''' <param name="Options">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>Database.DBElement Object (nMovie) which contains the scraped data</returns>
    ''' <remarks></remarks>
    'Function Scraper_TV(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVShow
    '    logger.Trace("[TMDB_Data] [Scraper_TV] [Start]")

    '    LoadSettings_Data_TV()
    '    _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

    '    Dim nTVShow As MediaContainers.TVShow = Nothing
    '    Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Data_TV)

    '    Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

    '    If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
    '        If oDBElement.TVShow.TMDBSpecified Then
    '            'TMDB-ID already available -> scrape and save data into an empty tv show container (nShow)
    '            nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
    '        ElseIf oDBElement.TVShow.TVDBSpecified Then
    '            oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
    '            If Not oDBElement.TVShow.TMDBSpecified Then Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
    '            nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
    '        ElseIf oDBElement.TVShow.IMDBSpecified Then
    '            oDBElement.TVShow.TMDB = _scraper.GetTMDBbyIMDB(oDBElement.TVShow.IMDB)
    '            If Not oDBElement.TVShow.TMDBSpecified Then Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
    '            nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
    '        ElseIf Not ScrapeType = Enums.ScrapeType.SingleScrape Then
    '            'no TVDB-ID for tv show --> search first and try to get ID!
    '            If oDBElement.TVShow.TitleSpecified Then
    '                nTVShow = _scraper.GetSearchTVShowInfo(oDBElement.TVShow.Title, oDBElement, ScrapeType, ScrapeModifiers, FilteredOptions)
    '            End If
    '            'if still no search result -> exit
    '            If nTVShow Is Nothing Then
    '                logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Abort] No search result found"))
    '                Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
    '            End If
    '        End If
    '    End If

    '    If nTVShow Is Nothing Then
    '        Select Case ScrapeType
    '            Case Enums.ScrapeType.AllAuto, Enums.ScrapeType.FilterAuto, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.SelectedAuto
    '                logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Abort] No search result found"))
    '                Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
    '        End Select
    '    Else
    '        Return New Interfaces.ModuleResult_Data_TVShow With {.Result = nTVShow}
    '    End If

    '    If ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto Then
    '        If Not oDBElement.TVShow.TMDBSpecified Then
    '            'Using dlgSearch As New dlgTMDBSearchResults_TV(_SpecialSettings_TV, _scraper)
    '            '    If dlgSearch.ShowDialog(oDBElement.TVShow.Title, oDBElement.ShowPath, FilteredOptions) = DialogResult.OK Then
    '            '        nTVShow = _scraper.GetTVShowInfo(dlgSearch.Result.TMDB, ScrapeModifiers, FilteredOptions, False)
    '            '        'if a tvshow is found, set DoSearch back to "false" for following scrapers
    '            '        ScrapeModifiers.DoSearch = False
    '            '    Else
    '            '        logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Cancelled] Cancelled by user"))
    '            '        Return New Interfaces.ModuleResult_Data_TVShow With {.Cancelled = True, .Result = Nothing}
    '            '    End If
    '            'End Using
    '        End If
    '    End If

    '    logger.Trace("[TMDB_Data] [Scraper_TV] [Done]")
    '    Return New Interfaces.ModuleResult_Data_TVShow With {.Result = nTVShow}
    'End Function

    'Public Function Scraper_TVEpisode(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVEpisode
    '    logger.Trace("[TMDB_Data] [Scraper_TVEpisode] [Start]")

    '    LoadSettings_Data_TV()
    '    _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

    '    Dim nTVEpisode As New MediaContainers.EpisodeDetails
    '    Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Data_TV)

    '    Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

    '    If Not oDBElement.TVShow.TMDBSpecified AndAlso oDBElement.TVShow.TVDBSpecified Then
    '        oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
    '    End If

    '    If oDBElement.TVShow.TMDBSpecified Then
    '        If Not oDBElement.TVEpisode.Episode = -1 AndAlso Not oDBElement.TVEpisode.Season = -1 Then
    '            nTVEpisode = _scraper.GetTVEpisodeInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVEpisode.Season, oDBElement.TVEpisode.Episode, FilteredOptions)
    '        ElseIf oDBElement.TVEpisode.AiredSpecified Then
    '            nTVEpisode = _scraper.GetTVEpisodeInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVEpisode.Aired, FilteredOptions)
    '        Else
    '            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No search result found"))
    '            Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
    '        End If
    '        'if still no search result -> exit
    '        If nTVEpisode Is Nothing Then
    '            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No search result found"))
    '            Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
    '        End If
    '    Else
    '        logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No TV Show TMDB ID available"))
    '        Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
    '    End If

    '    logger.Trace("[TMDB_Data] [Scraper_TVEpisode] [Done]")
    '    Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = nTVEpisode}
    'End Function

    'Public Function Scraper_TVSeason(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVSeason
    '    logger.Trace("[TMDB_Data] [Scraper_TVSeason] [Start]")

    '    LoadSettings_Data_TV()
    '    _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

    '    Dim nTVSeason As New MediaContainers.SeasonDetails
    '    Dim _scraper As New TMDB.clsScraperTMDB(_SpecialSettings_Data_TV)

    '    Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

    '    If Not oDBElement.TVShow.TMDBSpecified AndAlso oDBElement.TVShow.TVDBSpecified Then
    '        oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
    '    End If

    '    If oDBElement.TVShow.TMDBSpecified Then
    '        If oDBElement.TVSeason.SeasonSpecified Then
    '            nTVSeason = _scraper.GetTVSeasonInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVSeason.Season, FilteredOptions)
    '        Else
    '            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] Season is not specified"))
    '            Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
    '        End If
    '        'if still no search result -> exit
    '        If nTVSeason Is Nothing Then
    '            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] No search result found"))
    '            Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
    '        End If
    '    Else
    '        logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] No TV Show TMDB ID available"))
    '        Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
    '    End If

    '    logger.Trace("[TMDB_Data] [Scraper_TVSeason] [Done]")
    '    Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = nTVSeason}
    'End Function

#End Region 'Methods

#Region "Nested Types"

    Structure SpecialSettings

#Region "Fields"

        Dim APIKey As String
        Dim FallBackEng As Boolean
        Dim GetAdultItems As Boolean
        Dim PrefLanguage As String
        Dim SearchDeviant As Boolean

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class