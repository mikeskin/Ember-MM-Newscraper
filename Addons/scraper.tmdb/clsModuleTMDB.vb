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
    Implements Interfaces.ScraperModule
    Implements Interfaces.ScraperModuleSettingsPanel_Data_Movie
    Implements Interfaces.ScraperModuleSettingsPanel_Data_MovieSet
    Implements Interfaces.ScraperModuleSettingsPanel_Data_TV
    Implements Interfaces.ScraperModuleSettingsPanel_Image_Movie
    Implements Interfaces.ScraperModuleSettingsPanel_Image_MovieSet
    Implements Interfaces.ScraperModuleSettingsPanel_Image_TV
    Implements Interfaces.ScraperModuleSettingsPanel_Trailer_Movie


#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeOptions_Movie As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_MovieSet As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_TV As New Structures.ScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_MovieSet As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_TV As New Structures.ScrapeModifiers

    Private strPrivateAPIKey As String = String.Empty
    Private _SpecialSettings_Data_Movie As New SpecialSettings
    Private _SpecialSettings_Data_MovieSet As New SpecialSettings
    Private _SpecialSettings_Data_TV As New SpecialSettings
    Private _SpecialSettings_Trailer_Movie As New SpecialSettings
    Private _Name As String = "TMDB"
    Private _ScraperEnabled_Data_Movie As Boolean = False
    Private _ScraperEnabled_Data_MovieSet As Boolean = False
    Private _ScraperEnabled_Data_TV As Boolean = False
    Private _ScraperEnabled_Image_Movie As Boolean = False
    Private _ScraperEnabled_Image_MovieSet As Boolean = False
    Private _ScraperEnabled_Image_TV As Boolean = False
    Private _ScraperEnabled_Trailer_Movie As Boolean = False
    Private _sPanel_Data_Movie As frmSettingsHolder_Data_Movie
    Private _sPanel_Data_MovieSet As frmSettingsHolder_Data_MovieSet
    Private _sPanel_Data_TV As frmSettingsHolder_Data_TV
    Private _sPanel_Image_Movie As frmSettingsHolder_Image_Movie
    Private _sPanel_Image_MovieSet As frmSettingsHolder_Image_MovieSet
    Private _sPanel_Image_TV As frmSettingsHolder_Image_TV
    Private _sPanel_Trailer_Movie As frmSettingsHolder_Trailer_Movie

#End Region 'Fields

#Region "Events"

    'Global
    Public Event ModuleSettingsChanged() Implements Interfaces.ScraperModule.ModuleSettingsChanged
    Public Event SetupNeedsRestart() Implements Interfaces.ScraperModule.SetupNeedsRestart

    'Data Movie
    Public Event ScraperSetupChanged_Data_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Data_Movie.ScraperSetupChanged

    'Data MovieSet
    Public Event ScraperSetupChanged_Data_MovieSet(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Data_MovieSet.ScraperSetupChanged

    'Data TV
    Public Event ScraperSetupChanged_Data_TV(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Data_TV.ScraperSetupChanged

    'Image Movie
    Public Event ScraperSetupChanged_Image_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Image_Movie.ScraperSetupChanged

    'Image MovieSet
    Public Event ScraperSetupChanged_Image_MovieSet(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Image_MovieSet.ScraperSetupChanged

    'Image TV
    Public Event ScraperSetupChanged_Image_TV(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Image_TV.ScraperSetupChanged

    'Trailer Movie
    Public Event ScraperSetupChanged_Trailer_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModuleSettingsPanel_Trailer_Movie.ScraperSetupChanged

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleName() As String Implements Interfaces.ScraperModule.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.ScraperModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    ReadOnly Property AnyScraperEnabled() As Boolean Implements Interfaces.ScraperModule.AnyScraperEnabled
        Get
            Return _ScraperEnabled_Data_Movie OrElse
                _ScraperEnabled_Data_MovieSet OrElse
                _ScraperEnabled_Data_TV OrElse
                _ScraperEnabled_Image_Movie OrElse
                _ScraperEnabled_Image_MovieSet OrElse
                _ScraperEnabled_Image_TV OrElse
                _ScraperEnabled_Trailer_Movie
        End Get
    End Property

    Property ScraperEnabled_Data_Movie() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Data_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled_Data_Movie
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Data_Movie = value
        End Set
    End Property

    Property ScraperEnabled_Data_MovieSet() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Data_MovieSet.ScraperEnabled
        Get
            Return _ScraperEnabled_Data_MovieSet
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Data_MovieSet = value
        End Set
    End Property

    Property ScraperEnabled_Data_TV() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Data_TV.ScraperEnabled
        Get
            Return _ScraperEnabled_Data_TV
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Data_TV = value
        End Set
    End Property

    Property ScraperEnabled_Image_Movie() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Image_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled_Image_Movie
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Image_Movie = value
        End Set
    End Property

    Property ScraperEnabled_Image_MovieSet() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Image_MovieSet.ScraperEnabled
        Get
            Return _ScraperEnabled_Image_MovieSet
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Image_MovieSet = value
        End Set
    End Property

    Property ScraperEnabled_Image_TV() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Image_TV.ScraperEnabled
        Get
            Return _ScraperEnabled_Image_TV
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Image_TV = value
        End Set
    End Property

    Property ScraperEnabled_Trailer_Movie() As Boolean Implements Interfaces.ScraperModuleSettingsPanel_Trailer_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled_Trailer_Movie
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Trailer_Movie = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Handle_SetupNeedsRestart()
        RaiseEvent SetupNeedsRestart()
    End Sub

    Private Sub Handle_SetupScraperChanged_Data_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Data_Movie = state
        RaiseEvent ScraperSetupChanged_Data_Movie(String.Concat(_Name, "_Movie"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Data_MovieSet(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Data_MovieSet = state
        RaiseEvent ScraperSetupChanged_Data_MovieSet(String.Concat(_Name, "_MovieSet"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Data_TV(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Data_TV = state
        RaiseEvent ScraperSetupChanged_Data_TV(String.Concat(_Name, "_TV"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Image_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Image_Movie = state
        RaiseEvent ScraperSetupChanged_Image_Movie(String.Concat(_Name, "_Movie"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Image_MovieSet(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Image_MovieSet = state
        RaiseEvent ScraperSetupChanged_Image_MovieSet(String.Concat(_Name, "_MovieSet"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Image_TV(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Image_TV = state
        RaiseEvent ScraperSetupChanged_Image_TV(String.Concat(_Name, "_TV"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_Trailer_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Trailer_Movie = state
        RaiseEvent ScraperSetupChanged_Trailer_Movie(String.Concat(_Name, "_Movie"), state, difforder)
    End Sub

    Sub Init(ByVal sAssemblyName As String) Implements Interfaces.ScraperModule.Init
        _AssemblyName = sAssemblyName
        LoadSettings_Data_Movie()
    End Sub

    Function QueryModifierCapabilities(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean Implements Interfaces.ScraperModule.QueryModifierCapabilities
        Select Case tContentType
            Case Enums.ContentType.Movie
                Select Case tModifierType
                    Case Enums.ModifierType.MainFanart
                        Return ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainPoster
                    Case Enums.ModifierType.MainTrailer
                        Return ScraperEnabled_Image_Movie AndAlso ConfigScrapeModifier_Movie.MainTrailer
                End Select
            Case Enums.ContentType.MovieSet
                Select Case tModifierType
                    Case Enums.ModifierType.MainFanart
                        Return ScraperEnabled_Image_MovieSet AndAlso ConfigScrapeModifier_MovieSet.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return ScraperEnabled_Image_MovieSet AndAlso ConfigScrapeModifier_MovieSet.MainPoster
                End Select
            Case Enums.ContentType.TV, Enums.ContentType.TVEpisode, Enums.ContentType.TVSeason, Enums.ContentType.TVShow
                Select Case tModifierType
                    Case Enums.ModifierType.EpisodePoster
                        Return ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.EpisodePoster
                    Case Enums.ModifierType.MainFanart
                        Return ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.MainFanart
                    Case Enums.ModifierType.MainPoster
                        Return ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.MainPoster
                    Case Enums.ModifierType.SeasonPoster
                        Return ScraperEnabled_Image_TV AndAlso ConfigScrapeModifier_TV.SeasonPoster
                End Select
        End Select
        Return False
    End Function

    Function RunScraper(ByRef DBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ScrapeResults Implements Interfaces.ScraperModule.RunScraper
        Dim tScraperResults As New Interfaces.ScrapeResults

        Return tScraperResults
    End Function

    Function RunSearch(ByVal strTitle As String, ByVal intYear As Integer, ByVal tContentType As Enums.ContentType) As Interfaces.SearchResults Implements Interfaces.ScraperModule.RunSearch
        Dim tSearchResults As New Interfaces.SearchResults

        Return tSearchResults
    End Function

    Function InjectSetupScraper_Data_Movie() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Data_Movie.InjectSettingsPanel
        LoadSettings_Data_Movie()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Data_Movie = New frmSettingsHolder_Data_Movie
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
        _sPanel_Data_Movie.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Data_Movie.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Data_Movie.lblEMMAPI.Visible = False
            _sPanel_Data_Movie.txtApiKey.Enabled = True
        End If

        _sPanel_Data_Movie.orderChanged()

        sPanel.Name = String.Concat(_Name, "_Movie")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieInfo_"
        sPanel.Order = 110
        sPanel.Parent = "pnlMovieData"
        sPanel.Type = Master.eLang.GetString(36, "Movies")
        sPanel.ImageIndex = If(_ScraperEnabled_Data_Movie, 9, 10)
        sPanel.Panel = _sPanel_Data_Movie.pnlSettings

        AddHandler _sPanel_Data_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_Movie
        AddHandler _sPanel_Data_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Data_MovieSet() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Data_MovieSet.InjectSettingsPanel
        LoadSettings_Data_MovieSet()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Data_MovieSet = New frmSettingsHolder_Data_MovieSet
        _sPanel_Data_MovieSet.chkEnabled.Checked = _ScraperEnabled_Data_MovieSet
        _sPanel_Data_MovieSet.chkFallBackEng.Checked = _SpecialSettings_Data_MovieSet.FallBackEng
        _sPanel_Data_MovieSet.chkGetAdultItems.Checked = _SpecialSettings_Data_MovieSet.GetAdultItems
        _sPanel_Data_MovieSet.chkPlot.Checked = ConfigScrapeOptions_MovieSet.bMainPlot
        _sPanel_Data_MovieSet.chkTitle.Checked = ConfigScrapeOptions_MovieSet.bMainTitle
        _sPanel_Data_MovieSet.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Data_MovieSet.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Data_MovieSet.lblEMMAPI.Visible = False
            _sPanel_Data_MovieSet.txtApiKey.Enabled = True
        End If

        _sPanel_Data_MovieSet.orderChanged()

        sPanel.Name = String.Concat(_Name, "_MovieSet")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieSetInfo_"
        sPanel.Order = 110
        sPanel.Parent = "pnlMovieSetData"
        sPanel.Type = Master.eLang.GetString(1203, "MovieSets")
        sPanel.ImageIndex = If(_ScraperEnabled_Data_MovieSet, 9, 10)
        sPanel.Panel = _sPanel_Data_MovieSet.pnlSettings

        AddHandler _sPanel_Data_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_MovieSet
        AddHandler _sPanel_Data_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_MovieSet.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Data_TV() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Data_TV.InjectSettingsPanel
        LoadSettings_Data_TV()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Data_TV = New frmSettingsHolder_Data_TV
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
        _sPanel_Data_TV.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Data_TV.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Data_TV.lblEMMAPI.Visible = False
            _sPanel_Data_TV.txtApiKey.Enabled = True
        End If

        _sPanel_Data_TV.orderChanged()

        sPanel.Name = String.Concat(_Name, "_TV")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTVInfo_"
        sPanel.Order = 110
        sPanel.Parent = "pnlTVData"
        sPanel.Type = Master.eLang.GetString(653, "TV Shows")
        sPanel.ImageIndex = If(_ScraperEnabled_Data_TV, 9, 10)
        sPanel.Panel = _sPanel_Data_TV.pnlSettings

        AddHandler _sPanel_Data_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_TV
        AddHandler _sPanel_Data_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Data_TV.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Image_Movie() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Image_Movie.InjectSettingsPanel
        LoadSettings_Image_Movie()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Image_Movie = New frmSettingsHolder_Image_Movie
        _sPanel_Image_Movie.chkEnabled.Checked = _ScraperEnabled_Image_Movie
        _sPanel_Image_Movie.chkScrapeFanart.Checked = ConfigScrapeModifier_Movie.MainFanart
        _sPanel_Image_Movie.chkScrapePoster.Checked = ConfigScrapeModifier_Movie.MainPoster
        _sPanel_Image_Movie.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Image_Movie.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Image_Movie.lblEMMAPI.Visible = False
            _sPanel_Image_Movie.txtApiKey.Enabled = True
        End If

        _sPanel_Image_Movie.orderChanged()

        sPanel.Name = String.Concat(_Name, "_Movie")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieMedia_"
        sPanel.Order = 110
        sPanel.Parent = "pnlMovieMedia"
        sPanel.Type = Master.eLang.GetString(36, "Movies")
        sPanel.ImageIndex = If(_ScraperEnabled_Image_Movie, 9, 10)
        sPanel.Panel = _sPanel_Image_Movie.pnlSettings

        AddHandler _sPanel_Image_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Image_Movie
        AddHandler _sPanel_Image_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Image_MovieSet() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Image_MovieSet.InjectSettingsPanel
        LoadSettings_Image_MovieSet()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Image_MovieSet = New frmSettingsHolder_Image_MovieSet
        _sPanel_Image_MovieSet.chkEnabled.Checked = _ScraperEnabled_Image_MovieSet
        _sPanel_Image_MovieSet.chkScrapeFanart.Checked = ConfigScrapeModifier_MovieSet.MainFanart
        _sPanel_Image_MovieSet.chkScrapePoster.Checked = ConfigScrapeModifier_MovieSet.MainPoster
        _sPanel_Image_MovieSet.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Image_MovieSet.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Image_MovieSet.lblEMMAPI.Visible = False
            _sPanel_Image_MovieSet.txtApiKey.Enabled = True
        End If

        _sPanel_Image_MovieSet.orderChanged()

        sPanel.Name = String.Concat(_Name, "_MovieSet")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBMovieSetMedia_"
        sPanel.Order = 110
        sPanel.Parent = "pnlMovieSetMedia"
        sPanel.Type = Master.eLang.GetString(1203, "MovieSets")
        sPanel.ImageIndex = If(_ScraperEnabled_Image_MovieSet, 9, 10)
        sPanel.Panel = _sPanel_Image_MovieSet.pnlSettings

        AddHandler _sPanel_Image_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Image_MovieSet
        AddHandler _sPanel_Image_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_MovieSet.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Image_TV() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Image_TV.InjectSettingsPanel
        LoadSettings_Image_TV()
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Image_TV = New frmSettingsHolder_Image_TV
        _sPanel_Image_TV.chkEnabled.Checked = _ScraperEnabled_Image_TV
        _sPanel_Image_TV.chkScrapeEpisodePoster.Checked = ConfigScrapeModifier_TV.EpisodePoster
        _sPanel_Image_TV.chkScrapeSeasonPoster.Checked = ConfigScrapeModifier_TV.SeasonPoster
        _sPanel_Image_TV.chkScrapeShowFanart.Checked = ConfigScrapeModifier_TV.MainFanart
        _sPanel_Image_TV.chkScrapeShowPoster.Checked = ConfigScrapeModifier_TV.MainPoster
        _sPanel_Image_TV.txtApiKey.Text = strPrivateAPIKey

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Image_TV.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Image_TV.lblEMMAPI.Visible = False
            _sPanel_Image_TV.txtApiKey.Enabled = True
        End If

        _sPanel_Image_TV.orderChanged()

        sPanel.Name = String.Concat(_Name, "_TV")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTVMedia_"
        sPanel.Order = 110
        sPanel.Parent = "pnlTVMedia"
        sPanel.Type = Master.eLang.GetString(653, "TV Shows")
        sPanel.ImageIndex = If(_ScraperEnabled_Image_TV, 9, 10)
        sPanel.Panel = _sPanel_Image_TV.pnlSettings

        AddHandler _sPanel_Image_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Image_TV
        AddHandler _sPanel_Image_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Image_TV.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Function InjectSetupScraper_Trailer_Movie() As Containers.SettingsPanel Implements Interfaces.ScraperModuleSettingsPanel_Trailer_Movie.InjectSetupScraper
        Dim sPanel As New Containers.SettingsPanel
        _sPanel_Trailer_Movie = New frmSettingsHolder_Trailer_Movie
        LoadSettings_Trailer_Movie()
        _sPanel_Trailer_Movie.chkEnabled.Checked = _ScraperEnabled_Trailer_Movie
        _sPanel_Trailer_Movie.txtApiKey.Text = strPrivateAPIKey
        _sPanel_Trailer_Movie.chkFallBackEng.Checked = _SpecialSettings_Trailer_Movie.FallBackEng

        If Not String.IsNullOrEmpty(strPrivateAPIKey) Then
            _sPanel_Trailer_Movie.btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            _sPanel_Trailer_Movie.lblEMMAPI.Visible = False
            _sPanel_Trailer_Movie.txtApiKey.Enabled = True
        End If

        _sPanel_Trailer_Movie.orderChanged()

        sPanel.Name = String.Concat(_Name, "_Movie")
        sPanel.Text = "TMDB"
        sPanel.Prefix = "TMDBTrailer_"
        sPanel.Order = 110
        sPanel.Parent = "pnlMovieTrailer"
        sPanel.Type = Master.eLang.GetString(36, "Movies")
        sPanel.ImageIndex = If(_ScraperEnabled_Trailer_Movie, 9, 10)
        sPanel.Panel = _sPanel_Trailer_Movie.pnlSettings

        AddHandler _sPanel_Trailer_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Trailer_Movie
        AddHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Trailer_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
        Return sPanel
    End Function

    Sub LoadSettings_Data_Movie()
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

        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", String.Empty, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
        _SpecialSettings_Data_Movie.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.SearchDeviant = AdvancedSettings.GetBooleanSetting("SearchDeviant", False, , Enums.ContentType.Movie)
    End Sub

    Sub LoadSettings_Data_MovieSet()
        ConfigScrapeOptions_MovieSet.bMainPlot = AdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.MovieSet)
        ConfigScrapeOptions_MovieSet.bMainTitle = AdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.MovieSet)

        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", String.Empty, , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
    End Sub

    Sub LoadSettings_Data_TV()
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

        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", String.Empty, , Enums.ContentType.TV)
        _SpecialSettings_Data_TV.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.ContentType.TV)
        _SpecialSettings_Data_TV.GetAdultItems = AdvancedSettings.GetBooleanSetting("GetAdultItems", False, , Enums.ContentType.TV)
        _SpecialSettings_Data_TV.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
    End Sub

    Sub LoadSettings_Image_Movie()
        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", "", , Enums.ContentType.Movie)
        _SpecialSettings_Data_Movie.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)

        ConfigScrapeModifier_Movie.MainPoster = AdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.ContentType.Movie)
        ConfigScrapeModifier_Movie.MainFanart = AdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.ContentType.Movie)
        ConfigScrapeModifier_Movie.MainExtrafanarts = ConfigScrapeModifier_Movie.MainFanart
        ConfigScrapeModifier_Movie.MainExtrathumbs = ConfigScrapeModifier_Movie.MainFanart
    End Sub

    Sub LoadSettings_Image_MovieSet()
        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", "", , Enums.ContentType.MovieSet)
        _SpecialSettings_Data_MovieSet.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)

        ConfigScrapeModifier_MovieSet.MainPoster = AdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.ContentType.MovieSet)
        ConfigScrapeModifier_MovieSet.MainFanart = AdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.ContentType.MovieSet)
        ConfigScrapeModifier_MovieSet.MainExtrafanarts = ConfigScrapeModifier_MovieSet.MainFanart
        ConfigScrapeModifier_MovieSet.MainExtrathumbs = ConfigScrapeModifier_MovieSet.MainFanart
    End Sub

    Sub LoadSettings_Image_TV()
        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", "", , Enums.ContentType.TV)
        _SpecialSettings_Data_TV.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)

        ConfigScrapeModifier_TV.EpisodePoster = AdvancedSettings.GetBooleanSetting("DoEpisodePoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.SeasonPoster = AdvancedSettings.GetBooleanSetting("DoSeasonPoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainFanart = AdvancedSettings.GetBooleanSetting("DoShowFanart", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainPoster = AdvancedSettings.GetBooleanSetting("DoShowPoster", True, , Enums.ContentType.TV)
        ConfigScrapeModifier_TV.MainExtrafanarts = ConfigScrapeModifier_TV.MainFanart
    End Sub

    Sub LoadSettings_Trailer_Movie()
        strPrivateAPIKey = AdvancedSettings.GetSetting("APIKey", "",, Enums.ContentType.Movie)
        _SpecialSettings_Data_TV.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)

        ConfigScrapeModifier_Movie.MainTrailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True)
        _SpecialSettings_Trailer_Movie.APIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
        _SpecialSettings_Trailer_Movie.FallBackEng = AdvancedSettings.GetBooleanSetting("FallBackEn", False)
    End Sub

    Sub SaveSettings_Data_Movie()
        Using settings = New AdvancedSettings()
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
            settings.SetSetting("APIKey", _sPanel_Data_Movie.txtApiKey.Text, , , Enums.ContentType.Movie)
        End Using
    End Sub

    Sub SaveSettings_Data_MovieSet()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoPlot", ConfigScrapeOptions_MovieSet.bMainPlot, , , Enums.ContentType.MovieSet)
            settings.SetBooleanSetting("DoTitle", ConfigScrapeOptions_MovieSet.bMainTitle, , , Enums.ContentType.MovieSet)

            settings.SetBooleanSetting("GetAdultItems", _SpecialSettings_Data_MovieSet.GetAdultItems, , , Enums.ContentType.MovieSet)
            settings.SetSetting("APIKey", _sPanel_Data_MovieSet.txtApiKey.Text, , , Enums.ContentType.MovieSet)
        End Using
    End Sub

    Sub SaveSettings_Data_TV()
        Using settings = New AdvancedSettings()
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
            settings.SetSetting("APIKey", _sPanel_Data_TV.txtApiKey.Text, , , Enums.ContentType.TV)
        End Using
    End Sub

    Sub SaveSettings_Image_Movie()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_Movie.MainPoster, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_Movie.MainFanart, , , Enums.ContentType.Movie)

            settings.SetSetting("APIKey", _sPanel_Image_Movie.txtApiKey.Text, , , Enums.ContentType.Movie)
        End Using
    End Sub

    Sub SaveSettings_Image_MovieSet()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_MovieSet.MainPoster, , , Enums.ContentType.MovieSet)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_MovieSet.MainFanart, , , Enums.ContentType.MovieSet)

            settings.SetSetting("APIKey", _sPanel_Image_MovieSet.txtApiKey.Text, , , Enums.ContentType.MovieSet)
        End Using
    End Sub

    Sub SaveSettings_Image_TV()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoEpisodePoster", ConfigScrapeModifier_TV.EpisodePoster, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoSeasonPoster", ConfigScrapeModifier_TV.SeasonPoster, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoShowFanart", ConfigScrapeModifier_TV.MainFanart, , , Enums.ContentType.TV)
            settings.SetBooleanSetting("DoShowPoster", ConfigScrapeModifier_TV.MainPoster, , , Enums.ContentType.TV)

            settings.SetSetting("ApiKey", _sPanel_Image_TV.txtApiKey.Text, , , Enums.ContentType.TV)
        End Using
    End Sub

    Sub SaveSettings_Trailer_Movie()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoTrailer", ConfigScrapeModifier_Movie.MainTrailer, , , Enums.ContentType.Movie)

            settings.SetBooleanSetting("FallBackEn", _SpecialSettings_Data_Movie.FallBackEng, , , Enums.ContentType.Movie)
            settings.SetSetting("APIKey", _sPanel_Trailer_Movie.txtApiKey.Text, , , Enums.ContentType.Movie)
        End Using
    End Sub

    Sub SaveSettingsPanel(ByVal DoDispose As Boolean) Implements Interfaces.ScraperModule.SaveSettingsPanel
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
        SaveSettings_Data_Movie()

        'Data Movieset
        ConfigScrapeOptions_MovieSet.bMainPlot = _sPanel_Data_MovieSet.chkPlot.Checked
        ConfigScrapeOptions_MovieSet.bMainTitle = _sPanel_Data_MovieSet.chkTitle.Checked
        _SpecialSettings_Data_MovieSet.FallBackEng = _sPanel_Data_MovieSet.chkFallBackEng.Checked
        _SpecialSettings_Data_MovieSet.GetAdultItems = _sPanel_Data_MovieSet.chkGetAdultItems.Checked
        SaveSettings_Data_MovieSet()

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
        SaveSettings_Data_TV()

        'Image Movie
        ConfigScrapeModifier_Movie.MainPoster = _sPanel_Image_Movie.chkScrapePoster.Checked
        ConfigScrapeModifier_Movie.MainFanart = _sPanel_Image_Movie.chkScrapeFanart.Checked
        SaveSettings_Image_Movie()

        'Image MovieSet
        ConfigScrapeModifier_MovieSet.MainPoster = _sPanel_Image_MovieSet.chkScrapePoster.Checked
        ConfigScrapeModifier_MovieSet.MainFanart = _sPanel_Image_MovieSet.chkScrapeFanart.Checked
        SaveSettings_Image_MovieSet()

        'Image TV
        ConfigScrapeModifier_TV.EpisodePoster = _sPanel_Image_TV.chkScrapeEpisodePoster.Checked
        ConfigScrapeModifier_TV.SeasonPoster = _sPanel_Image_TV.chkScrapeSeasonPoster.Checked
        ConfigScrapeModifier_TV.MainFanart = _sPanel_Image_TV.chkScrapeShowFanart.Checked
        ConfigScrapeModifier_TV.MainPoster = _sPanel_Image_TV.chkScrapeShowPoster.Checked
        SaveSettings_Image_TV()

        'Trailer Movie
        _SpecialSettings_Data_Movie.FallBackEng = _sPanel_Trailer_Movie.chkFallBackEng.Checked
        SaveSettings_Trailer_Movie()

        If DoDispose Then
            'Data movie
            RemoveHandler _sPanel_Data_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_Movie
            RemoveHandler _sPanel_Data_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            _sPanel_Data_Movie.Dispose()

            'Data MovieSet
            RemoveHandler _sPanel_Data_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_MovieSet
            RemoveHandler _sPanel_Data_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            _sPanel_Data_MovieSet.Dispose()

            'Data TV
            RemoveHandler _sPanel_Data_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_TV
            RemoveHandler _sPanel_Data_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            _sPanel_Data_TV.Dispose()

            'Image Movie
            RemoveHandler _sPanel_Image_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_Movie
            RemoveHandler _sPanel_Image_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
            _sPanel_Image_Movie.Dispose()

            'Image MovieSet
            RemoveHandler _sPanel_Image_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_MovieSet
            RemoveHandler _sPanel_Image_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_MovieSet.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
            _sPanel_Image_MovieSet.Dispose()

            'Image TV
            RemoveHandler _sPanel_Image_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_TV
            RemoveHandler _sPanel_Image_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Image_TV.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
            _sPanel_Image_TV.Dispose()

            'Trailer Movie
            RemoveHandler _sPanel_Trailer_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Data_Movie
            RemoveHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Trailer_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart
            _sPanel_Trailer_Movie.Dispose()
        End If
    End Sub
    ''' <summary>
    '''  Scrape MovieDetails from TMDB
    ''' </summary>
    ''' <param name="DBMovie">Movie to be scraped. DBMovie as ByRef to use existing data for identifing movie and to fill with IMDB/TMDB ID for next scraper</param>
    ''' <param name="Options">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>Database.DBElement Object (nMovie) which contains the scraped data</returns>
    ''' <remarks></remarks>
    Function Scraper_Movie(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_Movie
        logger.Trace("[TMDB_Data] [Scraper_Movie] [Start]")

        LoadSettings_Data_Movie()
        _SpecialSettings_Data_Movie.PrefLanguage = oDBElement.Language

        Dim nMovie As MediaContainers.Movie = Nothing
        Dim _scraper As New TMDB.Scraper(_SpecialSettings_Data_Movie)

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_Movie)

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
            If oDBElement.Movie.TMDBIDSpecified Then
                'TMDB-ID already available -> scrape and save data into an empty movie container (nMovie)
                nMovie = _scraper.GetMovieInfo(oDBElement.Movie.TMDBID, FilteredOptions, False)
            ElseIf oDBElement.Movie.IDSpecified Then
                'IMDB-ID already available -> scrape and save data into an empty movie container (nMovie)
                nMovie = _scraper.GetMovieInfo(oDBElement.Movie.ID, FilteredOptions, False)
            ElseIf Not ScrapeType = Enums.ScrapeType.SingleScrape Then
                'no IMDB-ID or TMDB-ID for movie --> search first and try to get ID!
                If oDBElement.Movie.TitleSpecified Then
                    nMovie = _scraper.GetSearchMovieInfo(oDBElement.Movie.Title, oDBElement, ScrapeType, FilteredOptions)
                End If
                'if still no search result -> exit
                If nMovie Is Nothing Then
                    logger.Trace("[TMDB_Data] [Scraper_Movie] [Abort] No search result found")
                    Return New Interfaces.ModuleResult_Data_Movie With {.Result = Nothing}
                End If
            End If
        End If

        If nMovie Is Nothing Then
            Select Case ScrapeType
                Case Enums.ScrapeType.AllAuto, Enums.ScrapeType.FilterAuto, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.SelectedAuto
                    logger.Trace("[TMDB_Data] [Scraper_Movie] [Abort] No search result found")
                    Return New Interfaces.ModuleResult_Data_Movie With {.Result = Nothing}
            End Select
        Else
            Return New Interfaces.ModuleResult_Data_Movie With {.Result = nMovie}
        End If

        If ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto Then
            If Not oDBElement.Movie.TMDBIDSpecified Then
                'Using dlgSearch As New dlgTMDBSearchResults_Movie(_SpecialSettings_Movie, _scraper)
                '    If dlgSearch.ShowDialog(oDBElement.Movie.Title, oDBElement.Filename, FilteredOptions, oDBElement.Movie.Year) = DialogResult.OK Then
                '        nMovie = _scraper.GetMovieInfo(dlgSearch.Result.TMDBID, FilteredOptions, False)
                '        'if a movie is found, set DoSearch back to "false" for following scrapers
                '        ScrapeModifiers.DoSearch = False
                '    Else
                '        logger.Trace(String.Format("[TMDB_Data] [Scraper_Movie] [Cancelled] Cancelled by user"))
                '        Return New Interfaces.ModuleResult_Data_Movie With {.Cancelled = True, .Result = Nothing}
                '    End If
                'End Using
            End If
        End If

        logger.Trace("[TMDB_Data] [Scraper_Movie] [Done]")
        Return New Interfaces.ModuleResult_Data_Movie With {.Result = nMovie}
    End Function

    Function Scraper_MovieSet(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_MovieSet
        logger.Trace("[TMDB_Data] [Scraper_MovieSet] [Start]")

        LoadSettings_Data_MovieSet()
        _SpecialSettings_Data_MovieSet.PrefLanguage = oDBElement.Language

        Dim nMovieSet As MediaContainers.MovieSet = Nothing
        Dim _scraper As New TMDB.Scraper(_SpecialSettings_Data_MovieSet)

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_MovieSet)

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
            If oDBElement.MovieSet.TMDBSpecified Then
                'TMDB-ID already available -> scrape and save data into an empty movieset container (nMovieSet)
                nMovieSet = _scraper.GetMovieSetInfo(oDBElement.MovieSet.TMDB, FilteredOptions, False)
            ElseIf Not ScrapeType = Enums.ScrapeType.SingleScrape Then
                'no ITMDB-ID for movieset --> search first and try to get ID!
                If oDBElement.MovieSet.TitleSpecified Then
                    nMovieSet = _scraper.GetSearchMovieSetInfo(oDBElement.MovieSet.Title, oDBElement, ScrapeType, FilteredOptions)
                End If
                'if still no search result -> exit
                If nMovieSet Is Nothing Then
                    logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Abort] No search result found"))
                    Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = Nothing}
                End If
            End If
        End If

        If nMovieSet Is Nothing Then
            Select Case ScrapeType
                Case Enums.ScrapeType.AllAuto, Enums.ScrapeType.FilterAuto, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.SelectedAuto
                    logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Abort] No search result found"))
                    Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = Nothing}
            End Select
        Else
            Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = nMovieSet}
        End If

        If ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto Then
            If Not oDBElement.MovieSet.TMDBSpecified Then
                'Using dlgSearch As New dlgTMDBSearchResults_MovieSet(_SpecialSettings_MovieSet, _scraper)
                '    If dlgSearch.ShowDialog(oDBElement.MovieSet.Title, FilteredOptions) = DialogResult.OK Then
                '        nMovieSet = _scraper.GetMovieSetInfo(dlgSearch.Result.TMDB, FilteredOptions, False)
                '        'if a movieset is found, set DoSearch back to "false" for following scrapers
                '        ScrapeModifiers.DoSearch = False
                '    Else
                '        logger.Trace(String.Format("[TMDB_Data] [Scraper_MovieSet] [Cancelled] Cancelled by user"))
                '        Return New Interfaces.ModuleResult_Data_MovieSet With {.Cancelled = True, .Result = Nothing}
                '    End If
                'End Using
            End If
        End If

        logger.Trace("[TMDB_Data] [Scraper_MovieSet] [Done]")
        Return New Interfaces.ModuleResult_Data_MovieSet With {.Result = nMovieSet}
    End Function
    ''' <summary>
    '''  Scrape MovieDetails from TMDB
    ''' </summary>
    ''' <param name="oDBTV">TV Show to be scraped. DBTV as ByRef to use existing data for identifing tv show and to fill with IMDB/TMDB/TVDB ID for next scraper</param>
    ''' <param name="Options">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>Database.DBElement Object (nMovie) which contains the scraped data</returns>
    ''' <remarks></remarks>
    Function Scraper_TV(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVShow
        logger.Trace("[TMDB_Data] [Scraper_TV] [Start]")

        LoadSettings_Data_TV()
        _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

        Dim nTVShow As MediaContainers.TVShow = Nothing
        Dim _scraper As New TMDB.Scraper(_SpecialSettings_Data_TV)

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
            If oDBElement.TVShow.TMDBSpecified Then
                'TMDB-ID already available -> scrape and save data into an empty tv show container (nShow)
                nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
            ElseIf oDBElement.TVShow.TVDBSpecified Then
                oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
                If Not oDBElement.TVShow.TMDBSpecified Then Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
                nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
            ElseIf oDBElement.TVShow.IMDBSpecified Then
                oDBElement.TVShow.TMDB = _scraper.GetTMDBbyIMDB(oDBElement.TVShow.IMDB)
                If Not oDBElement.TVShow.TMDBSpecified Then Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
                nTVShow = _scraper.GetTVShowInfo(oDBElement.TVShow.TMDB, ScrapeModifiers, FilteredOptions, False)
            ElseIf Not ScrapeType = Enums.ScrapeType.SingleScrape Then
                'no TVDB-ID for tv show --> search first and try to get ID!
                If oDBElement.TVShow.TitleSpecified Then
                    nTVShow = _scraper.GetSearchTVShowInfo(oDBElement.TVShow.Title, oDBElement, ScrapeType, ScrapeModifiers, FilteredOptions)
                End If
                'if still no search result -> exit
                If nTVShow Is Nothing Then
                    logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Abort] No search result found"))
                    Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
                End If
            End If
        End If

        If nTVShow Is Nothing Then
            Select Case ScrapeType
                Case Enums.ScrapeType.AllAuto, Enums.ScrapeType.FilterAuto, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.NewAuto, Enums.ScrapeType.SelectedAuto
                    logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Abort] No search result found"))
                    Return New Interfaces.ModuleResult_Data_TVShow With {.Result = Nothing}
            End Select
        Else
            Return New Interfaces.ModuleResult_Data_TVShow With {.Result = nTVShow}
        End If

        If ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto Then
            If Not oDBElement.TVShow.TMDBSpecified Then
                'Using dlgSearch As New dlgTMDBSearchResults_TV(_SpecialSettings_TV, _scraper)
                '    If dlgSearch.ShowDialog(oDBElement.TVShow.Title, oDBElement.ShowPath, FilteredOptions) = DialogResult.OK Then
                '        nTVShow = _scraper.GetTVShowInfo(dlgSearch.Result.TMDB, ScrapeModifiers, FilteredOptions, False)
                '        'if a tvshow is found, set DoSearch back to "false" for following scrapers
                '        ScrapeModifiers.DoSearch = False
                '    Else
                '        logger.Trace(String.Format("[TMDB_Data] [Scraper_TV] [Cancelled] Cancelled by user"))
                '        Return New Interfaces.ModuleResult_Data_TVShow With {.Cancelled = True, .Result = Nothing}
                '    End If
                'End Using
            End If
        End If

        logger.Trace("[TMDB_Data] [Scraper_TV] [Done]")
        Return New Interfaces.ModuleResult_Data_TVShow With {.Result = nTVShow}
    End Function

    Public Function Scraper_TVEpisode(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVEpisode
        logger.Trace("[TMDB_Data] [Scraper_TVEpisode] [Start]")

        LoadSettings_Data_TV()
        _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

        Dim nTVEpisode As New MediaContainers.EpisodeDetails
        Dim _scraper As New TMDB.Scraper(_SpecialSettings_Data_TV)

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

        If Not oDBElement.TVShow.TMDBSpecified AndAlso oDBElement.TVShow.TVDBSpecified Then
            oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
        End If

        If oDBElement.TVShow.TMDBSpecified Then
            If Not oDBElement.TVEpisode.Episode = -1 AndAlso Not oDBElement.TVEpisode.Season = -1 Then
                nTVEpisode = _scraper.GetTVEpisodeInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVEpisode.Season, oDBElement.TVEpisode.Episode, FilteredOptions)
            ElseIf oDBElement.TVEpisode.AiredSpecified Then
                nTVEpisode = _scraper.GetTVEpisodeInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVEpisode.Aired, FilteredOptions)
            Else
                logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No search result found"))
                Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
            End If
            'if still no search result -> exit
            If nTVEpisode Is Nothing Then
                logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No search result found"))
                Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
            End If
        Else
            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVEpisode] [Abort] No TV Show TMDB ID available"))
            Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = Nothing}
        End If

        logger.Trace("[TMDB_Data] [Scraper_TVEpisode] [Done]")
        Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = nTVEpisode}
    End Function

    Public Function Scraper_TVSeason(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVSeason
        logger.Trace("[TMDB_Data] [Scraper_TVSeason] [Start]")

        LoadSettings_Data_TV()
        _SpecialSettings_Data_TV.PrefLanguage = oDBElement.Language

        Dim nTVSeason As New MediaContainers.SeasonDetails
        Dim _scraper As New TMDB.Scraper(_SpecialSettings_Data_TV)

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)

        If Not oDBElement.TVShow.TMDBSpecified AndAlso oDBElement.TVShow.TVDBSpecified Then
            oDBElement.TVShow.TMDB = _scraper.GetTMDBbyTVDB(oDBElement.TVShow.TVDB)
        End If

        If oDBElement.TVShow.TMDBSpecified Then
            If oDBElement.TVSeason.SeasonSpecified Then
                nTVSeason = _scraper.GetTVSeasonInfo(CInt(oDBElement.TVShow.TMDB), oDBElement.TVSeason.Season, FilteredOptions)
            Else
                logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] Season is not specified"))
                Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
            End If
            'if still no search result -> exit
            If nTVSeason Is Nothing Then
                logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] No search result found"))
                Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
            End If
        Else
            logger.Trace(String.Format("[TMDB_Data] [Scraper_TVSeason] [Abort] No TV Show TMDB ID available"))
            Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
        End If

        logger.Trace("[TMDB_Data] [Scraper_TVSeason] [Done]")
        Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = nTVSeason}
    End Function

    Public Sub ScraperOrderChanged_Data_Movie() Implements Interfaces.ScraperModuleSettingsPanel_Data_Movie.ScraperOrderChanged
        _sPanel_Data_Movie.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Data_MovieSet() Implements Interfaces.ScraperModuleSettingsPanel_Data_MovieSet.ScraperOrderChanged
        _sPanel_Data_MovieSet.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Data_TV() Implements Interfaces.ScraperModuleSettingsPanel_Data_TV.ScraperOrderChanged
        _sPanel_Data_TV.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Image_Movie() Implements Interfaces.ScraperModuleSettingsPanel_Image_Movie.ScraperOrderChanged
        _sPanel_Image_Movie.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Image_MovieSet() Implements Interfaces.ScraperModuleSettingsPanel_Image_MovieSet.ScraperOrderChanged
        _sPanel_Image_MovieSet.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Image_TV() Implements Interfaces.ScraperModuleSettingsPanel_Image_TV.ScraperOrderChanged
        _sPanel_Image_TV.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_Trailer_Movie() Implements Interfaces.ScraperModuleSettingsPanel_Trailer_Movie.ScraperOrderChanged
        _sPanel_Trailer_Movie.orderChanged()
    End Sub

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