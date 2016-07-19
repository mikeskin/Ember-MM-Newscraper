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
' ###############################################################################

Imports EmberAPI
Imports NLog

Public Class clsModuleYouTube
    Implements Interfaces.Base
    Implements Interfaces.ScraperEngine

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers

    Private _ScraperEnabled_Trailer_Movie As Boolean = False
    Private _sPanel_Trailer_Movie As frmSettingsPanel_Trailer_Movie

#End Region 'Fields

#Region "Events"

    Public Event ModuleNeedsRestart() Implements Interfaces.Base.ModuleNeedsRestart
    Public Event ModuleSettingsChanged() Implements Interfaces.Base.ModuleSettingsChanged
    Public Event ModuleStateChanged(ByVal strAssemblyName As String, ByVal tPanelType As Enums.SettingsPanelType, ByVal bIsEnabled As Boolean, ByVal intDifforder As Integer) Implements Interfaces.Base.ModuleStateChanged

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleEnabled(ByVal tType As Enums.SettingsPanelType) As Boolean Implements Interfaces.ScraperEngine.ModuleEnabled
        Get
            Select Case tType
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
        _ScraperEnabled_Trailer_Movie = bIsEnabled
        RaiseEvent ModuleStateChanged(_AssemblyName, tPanelType, bIsEnabled, intDifforder)
    End Sub

    Sub Init(ByVal strAssemblyName As String) Implements Interfaces.Base.Init
        _AssemblyName = strAssemblyName
        LoadSettings()
    End Sub

    Public Sub ScraperOrderChanged_Movie(ByVal tPanelType As Enums.SettingsPanelType) Implements Interfaces.Base.ModuleOrderChanged
        Select Case tPanelType
            Case Enums.SettingsPanelType.MovieTrailer
                _sPanel_Trailer_Movie.orderChanged()
        End Select
    End Sub

    Function QueryCapabilities(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean Implements Interfaces.ScraperEngine.QueryCapabilities
        Select Case tContentType
            Case Enums.ContentType.Movie
                Select Case tModifierType
                    Case Enums.ModifierType.MainTrailer
                        Return _ScraperEnabled_Trailer_Movie AndAlso ConfigScrapeModifier_Movie.MainTrailer
                End Select
        End Select
        Return False
    End Function

    Function RunScraper(ByRef DBElement As Database.DBElement) As Interfaces.ScrapeResults Implements Interfaces.ScraperEngine.RunScraper
        Dim tScraperResults As New Interfaces.ScrapeResults

        Select Case DBElement.ContentType
            Case Enums.ContentType.Movie
                logger.Trace("[YouTube] [RunScraper] [Movie] [Start]")

                LoadSettings()

                If Not String.IsNullOrEmpty(DBElement.Movie.Title) Then
                    Dim _scraper As New YouTube.clsScraperYouTube()
                    tScraperResults.tResult = _scraper.GetTrailers(DBElement.Movie.Title)
                Else
                    logger.Trace("[YouTube] [RunScraper] [Movie] [Abort] No Title to search")
                    Return New Interfaces.ScrapeResults
                End If

                logger.Trace("[YouTube] [RunScraper] [Movie] [Done]")
                Return tScraperResults
        End Select

        Return tScraperResults
    End Function

    Function InjectSettingsPanels() As List(Of Containers.SettingsPanel) Implements Interfaces.Base.InjectSettingsPanels
        LoadSettings()
        Dim sPanelList As New List(Of Containers.SettingsPanel)
        sPanelList.Add(InjectSettingsPanel_Trailer_Movie)

        Return sPanelList
    End Function

    Function InjectSettingsPanel_Trailer_Movie() As Containers.SettingsPanel
        Dim sPanel As New Containers.SettingsPanel(Enums.SettingsPanelType.MovieTrailer)
        _sPanel_Trailer_Movie = New frmSettingsPanel_Trailer_Movie
        _sPanel_Trailer_Movie.chkEnabled.Checked = _ScraperEnabled_Trailer_Movie

        _sPanel_Trailer_Movie.orderChanged()

        sPanel.Name = _AssemblyName
        sPanel.Text = "YouTube"
        sPanel.Prefix = "YouTubeTrailer_"
        sPanel.Order = 110
        sPanel.ImageIndex = If(_ScraperEnabled_Trailer_Movie, 9, 10)
        sPanel.Panel = _sPanel_Trailer_Movie.pnlSettings

        AddHandler _sPanel_Trailer_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
        AddHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        AddHandler _sPanel_Trailer_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged

        Return sPanel
    End Function

    Sub LoadSettings()
        'Trailer Movie
        ConfigScrapeModifier_Movie.MainTrailer = AdvancedSettings.GetBooleanSetting("DoTrailer", True, , Enums.ContentType.Movie)
    End Sub

    Sub SaveSettings()
        Using settings = New AdvancedSettings()
            'Trailer Movie
            settings.SetBooleanSetting("DoTrailer", ConfigScrapeModifier_Movie.MainTrailer, , , Enums.ContentType.Movie)
        End Using
    End Sub

    Sub SaveSettingsPanel(ByVal DoDispose As Boolean) Implements Interfaces.Base.SaveSettingsPanel
        SaveSettings()

        If DoDispose Then
            'Trailer Movie
            RemoveHandler _sPanel_Trailer_Movie.ModuleNeedsRestart, AddressOf Handle_ModuleNeedsRestart
            RemoveHandler _sPanel_Trailer_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            RemoveHandler _sPanel_Trailer_Movie.ModuleStateChanged, AddressOf Handle_ModuleStateChanged
            _sPanel_Trailer_Movie.Dispose()
        End If
    End Sub

#End Region 'Methods

#Region "Nested Types"

#End Region 'Nested Types

End Class