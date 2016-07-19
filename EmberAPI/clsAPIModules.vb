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

Imports System.IO
Imports System.Xml.Serialization
Imports System.Windows.Forms
Imports System.Drawing
Imports NLog

Public Class ModulesManager

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared AssemblyList As New List(Of AssemblyListItem)
    Public Shared VersionList As New List(Of VersionItem)

    Public externalGenericModules As New List(Of _externalGenericModuleClass)
    Public externalModules As New List(Of _externalModuleClass)
    Public RuntimeObjects As New EmberRuntimeObjects

    'Singleton Instace for module manager .. allways use this one
    Private Shared Singleton As ModulesManager = Nothing

    Private moduleLocation As String = Path.Combine(Functions.AppPath, "Modules")

    Friend WithEvents bwLoadModules As New System.ComponentModel.BackgroundWorker

#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))

#End Region 'Events

#Region "Properties"

    Public Shared ReadOnly Property Instance() As ModulesManager
        Get
            If (Singleton Is Nothing) Then
                Singleton = New ModulesManager()
            End If
            Return Singleton
        End Get
    End Property

    Public ReadOnly Property ModulesLoaded() As Boolean
        Get
            Return Not bwLoadModules.IsBusy
        End Get
    End Property
#End Region 'Properties

#Region "Methods"

    Private Sub BuildVersionList()
        VersionList.Clear()
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPP", .Name = "Ember Application", .Version = My.Application.Info.Version.ToString()})
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPI", .Name = "Ember API", .Version = Functions.EmberAPIVersion()})
        For Each _externalScraperModule As _externalModuleClass In externalModules
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.Base.ModuleName,
              .AssemblyFileName = _externalScraperModule.AssemblyFilename,
              .Version = _externalScraperModule.Base.ModuleVersion})
        Next
        For Each _externalModule As _externalGenericModuleClass In externalGenericModules
            VersionList.Add(New VersionItem With {.Name = _externalModule.ProcessorModule.ModuleName,
              .AssemblyFileName = _externalModule.AssemblyFileName,
              .Version = _externalModule.ProcessorModule.ModuleVersion})
        Next
    End Sub

    Private Sub bwLoadModules_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadModules.DoWork
        LoadModules()
    End Sub

    Private Sub bwLoadModules_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadModules.RunWorkerCompleted
        BuildVersionList()
    End Sub

    Public Sub GetVersions()
        Dim dlgVersions As New dlgVersions
        Dim li As ListViewItem
        While Not ModulesLoaded
            Application.DoEvents()
        End While
        For Each v As VersionItem In VersionList
            li = dlgVersions.lstVersions.Items.Add(v.Name)
            li.SubItems.Add(v.Version)
        Next
        dlgVersions.ShowDialog()
    End Sub

    Public Sub LoadAllModules()
        bwLoadModules.RunWorkerAsync()
    End Sub

    Public Sub LoadModules()
        Dim DataScraperAnyEnabled_Movie As Boolean = False
        Dim DataScraperAnyEnabled_MovieSet As Boolean = False
        Dim DataScraperAnyEnabled_TV As Boolean = False
        Dim DataScraperFound_Movie As Boolean = False
        Dim DataScraperFound_MovieSet As Boolean = False
        Dim DataScraperFound_TV As Boolean = False
        Dim ImageScraperAnyEnabled_Movie As Boolean = False
        Dim ImageScraperAnyEnabled_MovieSet As Boolean = False
        Dim ImageScraperAnyEnabled_TV As Boolean = False
        Dim ImageScraperFound_Movie As Boolean = False
        Dim ImageScraperFound_MovieSet As Boolean = False
        Dim ImageScraperFound_TV As Boolean = False
        Dim ThemeScraperAnyEnabled_Movie As Boolean = False
        Dim ThemeScraperAnyEnabled_TV As Boolean = False
        Dim ThemeScraperFound_Movie As Boolean = False
        Dim ThemeScraperFound_TV As Boolean = False
        Dim TrailerScraperAnyEnabled_Movie As Boolean = False
        Dim TrailerScraperFound_Movie As Boolean = False

        logger.Trace("[ModulesManager] [LoadModules] [Start]")

        If Directory.Exists(moduleLocation) Then
            'add each .dll file to AssemblyList
            For Each file As String In Directory.GetFiles(moduleLocation, "*.dll")
                Dim nAssembly As Reflection.Assembly = Reflection.Assembly.LoadFile(file)
                AssemblyList.Add(New ModulesManager.AssemblyListItem With {.Assembly = nAssembly, .AssemblyName = nAssembly.GetName.Name})
            Next

            For Each tAssemblyItem As AssemblyListItem In AssemblyList
                'Loop through each of the assemeblies type
                Dim test = tAssemblyItem.Assembly.GetTypes
                For Each fileType As Type In tAssemblyItem.Assembly.GetTypes

                    Dim fType As Type = fileType.GetInterface("Base")
                    If fType IsNot Nothing Then
                        Dim Base As Interfaces.Base
                        Base = CType(Activator.CreateInstance(fileType), Interfaces.Base)

                        Dim tExternalModule As New _externalModuleClass
                        tExternalModule.Base = Base
                        tExternalModule.AssemblyName = tAssemblyItem.AssemblyName
                        tExternalModule.AssemblyFilename = tAssemblyItem.Assembly.ManifestModule.Name

                        fType = fileType.GetInterface("GenericEngine")
                        If fType IsNot Nothing Then
                            Dim GenericEngine As Interfaces.GenericEngine
                            GenericEngine = CType(Activator.CreateInstance(fileType), Interfaces.GenericEngine)
                            tExternalModule.GenericEngine = GenericEngine
                        End If

                        fType = fileType.GetInterface("ScraperEngine")
                        If fType IsNot Nothing Then
                            Dim ScraperEngine As Interfaces.ScraperEngine
                            ScraperEngine = CType(Activator.CreateInstance(fileType), Interfaces.ScraperEngine)
                            tExternalModule.ScraperEngine = ScraperEngine
                        End If

                        fType = fileType.GetInterface("SearchEngine")
                        If fType IsNot Nothing Then
                            Dim SearchEngine As Interfaces.SearchEngine
                            SearchEngine = CType(Activator.CreateInstance(fileType), Interfaces.SearchEngine)
                            tExternalModule.SearchEngine = SearchEngine
                        End If

                        externalModules.Add(tExternalModule)

                        tExternalModule.Base.Init(tExternalModule.AssemblyName)
                        logger.Trace(String.Concat("[ModulesManager] [LoadModules] Loaded: ", tExternalModule.AssemblyName))
                    End If

                    'fType = fileType.GetInterface("GenericModule")
                    'If Not fType Is Nothing Then
                    '    Dim ProcessorModule As Interfaces.GenericEngine 'Object
                    '    ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.GenericEngine)

                    '    Dim GenericModule As New _externalGenericModuleClass
                    '    GenericModule.ProcessorModule = ProcessorModule
                    '    GenericModule.AssemblyName = tAssemblyItem.AssemblyName
                    '    GenericModule.AssemblyFileName = tAssemblyItem.Assembly.ManifestModule.Name
                    '    GenericModule.Type = ProcessorModule.ModuleType
                    '    externalGenericModules.Add(GenericModule)

                    '    GenericModule.ProcessorModule.Init(GenericModule.AssemblyName, GenericModule.AssemblyFileName)

                    '    Dim bFound As Boolean = False
                    '    For Each i In Master.eSettings.EmberModules
                    '        If i.AssemblyName = GenericModule.AssemblyName Then
                    '            GenericModule.ProcessorModule.ModuleEnabled = i.ModuleEnabled
                    '            bFound = True
                    '        End If
                    '    Next

                    '    'Enable all Core Modules by default if no setting was found
                    '    If Not bFound AndAlso GenericModule.AssemblyFileName.Contains("generic.EmberCore") Then
                    '        GenericModule.ProcessorModule.ModuleEnabled = True
                    '    End If
                    '    AddHandler ProcessorModule.GenericEvent, AddressOf GenericRunCallBack
                    'End If

                    'fType = fileType.GetInterface("ScraperModuleSettingsPanel_Data_Movie")
                    'If Not fType Is Nothing Then
                    '    Dim ProcessorModule As Interfaces.ScraperModuleSettingsPanel_Data_Movie
                    '    ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModuleSettingsPanel_Data_Movie)

                    '    Dim SettingsPanel As New _externalScraperModuleSettingsPanelClass_Data_Movie
                    '    SettingsPanel.SettingsPanel = ProcessorModule
                    '    SettingsPanel.AssemblyName = tAssemblyItem.AssemblyName
                    '    SettingsPanel.AssemblyFileName = tAssemblyItem.Assembly.ManifestModule.Name
                    '    externalScraperModulesSettingsPanels_Data_Movie.Add(SettingsPanel)

                    '    logger.Trace(String.Concat("[ModulesManager] [LoadModules] Scraper Added: ", SettingsPanel.AssemblyName, "_", SettingsPanel.ContentType))

                    '    'ScraperModule.ProcessorModule.Init(ScraperModule.AssemblyName)

                    '    For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(f) f.AssemblyName = SettingsPanel.AssemblyName AndAlso
                    '                                                                                      f.ContentType = Enums.ContentType.Movie)
                    '        SettingsPanel.SettingsPanel.ScraperEnabled = i.ModuleEnabled
                    '        DataScraperAnyEnabled_Movie = DataScraperAnyEnabled_Movie OrElse i.ModuleEnabled
                    '        SettingsPanel.ModuleOrder = i.ModuleOrder
                    '        DataScraperFound_Movie = True
                    '    Next
                    '    If Not DataScraperFound_Movie Then
                    '        SettingsPanel.ModuleOrder = 999
                    '    End If
                    'End If
                Next
            Next

            'Modules ordering
            Dim c As Integer = 0
            For Each ext As _externalGenericModuleClass In externalGenericModules.OrderBy(Function(f) f.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            'For Each ext As _externalScraperModuleClass In externalScraperModules.OrderBy(Function(f) f.ModuleOrder)
            '    ext.ModuleOrder = c
            '    c += 1
            'Next

            'Enable default Modules
            If Not DataScraperAnyEnabled_Movie AndAlso Not DataScraperFound_Movie Then
                SetScraperEnable("scraper.Data.TMDB", Enums.SettingsPanelType.MovieData, True)
            End If
            If Not ImageScraperAnyEnabled_Movie AndAlso Not ImageScraperFound_Movie Then
                SetScraperEnable_Image_Movie("scraper.Image.FanartTV", True)
                SetScraperEnable_Image_Movie("scraper.Image.TMDB", True)
            End If
            If Not ThemeScraperAnyEnabled_Movie AndAlso Not ThemeScraperFound_Movie Then
                SetScraperEnable_Theme_Movie("scraper.Theme.TelevisionTunes", True)
            End If
            If Not TrailerScraperAnyEnabled_Movie AndAlso Not TrailerScraperFound_Movie Then
                SetScraperEnable_Trailer_Movie("scraper.Trailer.TMDB", True)
            End If
            If Not DataScraperAnyEnabled_MovieSet AndAlso Not DataScraperFound_MovieSet Then
                SetScraperEnable_Data_MovieSet("scraper.Data.TMDB", True)
            End If
            If Not ImageScraperAnyEnabled_MovieSet AndAlso Not ImageScraperFound_MovieSet Then
                SetScraperEnable_Image_MovieSet("scraper.Image.FanartTV", True)
                SetScraperEnable_Image_MovieSet("scraper.Image.TMDB", True)
            End If
            If Not DataScraperAnyEnabled_TV AndAlso Not DataScraperFound_TV Then
                SetScraperEnable_Data_TV("scraper.Data.TVDB", True)
            End If
            If Not ImageScraperAnyEnabled_TV AndAlso Not ImageScraperFound_TV Then
                SetScraperEnable_Image_TV("scraper.Image.FanartTV", True)
                SetScraperEnable_Image_TV("scraper.Image.TMDB", True)
                SetScraperEnable_Image_TV("scraper.Image.TVDB", True)
            End If
            If Not ThemeScraperAnyEnabled_TV AndAlso Not ThemeScraperFound_TV Then
                SetScraperEnable_Theme_TV("scraper.TelevisionTunes.Theme", True)
            End If
        End If

        logger.Trace("[ModulesManager] [LoadModules] [Done]")
    End Sub

    Function QueryAnyGenericIsBusy() As Boolean
        While Not ModulesLoaded
            Application.DoEvents()
        End While

        Dim modules As IEnumerable(Of _externalGenericModuleClass) = externalGenericModules.Where(Function(e) e.ProcessorModule.IsBusy)
        If modules.Count() > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Function QueryScraperCapabilities(ByVal externalScraperModule As _externalModuleClass, ByVal tScrapeModifiers As Structures.ScrapeModifiers, ByVal tContentType As Enums.ContentType) As Boolean
        While Not ModulesLoaded
            Application.DoEvents()
        End While

        If tScrapeModifiers.EpisodeFanart AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.EpisodeFanart, tContentType) Then Return True
        If tScrapeModifiers.EpisodePoster AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.EpisodePoster, tContentType) Then Return True
        If tScrapeModifiers.MainBanner AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainBanner, tContentType) Then Return True
        If tScrapeModifiers.MainCharacterArt AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainCharacterArt, tContentType) Then Return True
        If tScrapeModifiers.MainClearArt AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainClearArt, tContentType) Then Return True
        If tScrapeModifiers.MainClearLogo AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainClearLogo, tContentType) Then Return True
        If tScrapeModifiers.MainDiscArt AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainDiscArt, tContentType) Then Return True
        If tScrapeModifiers.MainExtrafanarts AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) Then Return True
        If tScrapeModifiers.MainExtrathumbs AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) Then Return True
        If tScrapeModifiers.MainFanart AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) Then Return True
        If tScrapeModifiers.MainLandscape AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainLandscape, tContentType) Then Return True
        If tScrapeModifiers.MainPoster AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainPoster, tContentType) Then Return True
        If tScrapeModifiers.MainSubtitles AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainSubtitle, tContentType) Then Return True
        If tScrapeModifiers.MainTheme AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainTheme, tContentType) Then Return True
        If tScrapeModifiers.MainTrailer AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainTrailer, tContentType) Then Return True
        If tScrapeModifiers.SeasonBanner AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonBanner, tContentType) Then Return True
        If tScrapeModifiers.SeasonFanart AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonFanart, tContentType) Then Return True
        If tScrapeModifiers.SeasonLandscape AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonLandscape, tContentType) Then Return True
        If tScrapeModifiers.SeasonPoster AndAlso externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonPoster, tContentType) Then Return True

        Return False
    End Function

    Function QueryScraperCapabilities(ByVal externalScraperModule As _externalModuleClass, ByVal tImageType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean
        While Not ModulesLoaded
            Application.DoEvents()
        End While

        Select Case tImageType
            Case Enums.ModifierType.AllSeasonsBanner
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainBanner, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonBanner, tContentType) Then Return True
            Case Enums.ModifierType.AllSeasonsFanart
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonFanart, tContentType) Then Return True
            Case Enums.ModifierType.AllSeasonsLandscape
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainLandscape, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonLandscape, tContentType) Then Return True
            Case Enums.ModifierType.AllSeasonsPoster
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainPoster, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonPoster, tContentType) Then Return True
            Case Enums.ModifierType.EpisodeFanart
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.EpisodeFanart, tContentType) Then Return True
            Case Enums.ModifierType.MainExtrafanarts
                Return externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType)
            Case Enums.ModifierType.MainExtrathumbs
                Return externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType)
            Case Enums.ModifierType.SeasonFanart
                If externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.MainFanart, tContentType) OrElse
                    externalScraperModule.ScraperEngine.QueryCapabilities(Enums.ModifierType.SeasonFanart, tContentType) Then Return True
            Case Else
                Return externalScraperModule.ScraperEngine.QueryCapabilities(tImageType, tContentType)
        End Select

        Return False
    End Function

    Function QueryScraperCapabilities_AnyEnabled(ByVal tImageType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean
        Dim ret As Boolean = False
        While Not ModulesLoaded
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalModuleClass In externalModules '.Where(Function(e) e.ScraperModule.ModuleEnabled)
            ret = QueryScraperCapabilities(_externalScraperModule, tImageType, tContentType)
            If ret Then Exit For
        Next
        Return ret
    End Function
    ''' <summary>
    ''' Calls all the generic modules of the supplied type (if one is defined), passing the supplied _params.
    ''' The module will do its task and return any expected results in the _refparams.
    ''' </summary>
    ''' <param name="mType">The <c>Enums.ModuleEventType</c> of module to execute.</param>
    ''' <param name="_params">Parameters to pass to the module</param>
    ''' <param name="_singleobjekt"><c>Object</c> representing the module's result (if relevant)</param>
    ''' <param name="RunOnlyOne">If <c>True</c>, allow only one module to perform the required task.</param>
    ''' <returns></returns>
    ''' <remarks>Note that if any module returns a result of breakChain, no further modules are processed</remarks>
    Public Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), Optional ByVal _singleobjekt As Object = Nothing, Optional ByVal RunOnlyOne As Boolean = False, Optional ByRef DBElement As Database.DBElement = Nothing) As Boolean
        logger.Trace(String.Format("[ModulesManager] [RunGeneric] [Start] <{0}>", mType.ToString))
        Dim ret As Interfaces.ModuleResult

        While Not ModulesLoaded
            Application.DoEvents()
        End While

        Try
            Dim modules As IEnumerable(Of _externalGenericModuleClass) = externalGenericModules.Where(Function(e) e.ProcessorModule.ModuleType.Contains(mType) AndAlso e.ProcessorModule.ModuleEnabled)
            If (modules.Count() <= 0) Then
                logger.Warn("[ModulesManager] [RunGeneric] No generic modules defined <{0}>", mType.ToString)
            Else
                For Each _externalGenericModule As _externalGenericModuleClass In modules
                    Try
                        logger.Trace("[ModulesManager] [RunGeneric] Run generic module <{0}>", _externalGenericModule.ProcessorModule.ModuleName)
                        ret = _externalGenericModule.ProcessorModule.RunGeneric(mType, _params, _singleobjekt, DBElement)
                    Catch ex As Exception
                        logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Error scraping movies images using <" & _externalGenericModule.ProcessorModule.ModuleName & ">")
                    End Try
                    If ret.bBreakChain OrElse RunOnlyOne Then Exit For
                Next
            End If
        Catch ex As Exception
            logger.Error(ex, New StackFrame().GetMethod().Name)
        End Try

        Return ret.bCancelled
    End Function

    Public Function RunSearch(ByVal strTitle As String, ByVal intYear As Integer, ByVal strLanguage As String, ByVal tContentType As Enums.ContentType) As MediaContainers.SearchResultsContainer
        Dim tSearchResults As New MediaContainers.SearchResultsContainer
        Dim ret As Interfaces.SearchResults

        Dim modules As IEnumerable(Of _externalModuleClass) = externalModules.Where(Function(e) e.SearchEngine IsNot Nothing)

        For Each _externalScraperModule As _externalModuleClass In modules
            ret = _externalScraperModule.SearchEngine.RunSearch(strTitle, intYear, strLanguage, tContentType)
            If ret.tResult IsNot Nothing Then
                tSearchResults.Movies.AddRange(ret.tResult.Movies)
                tSearchResults.MovieSets.AddRange(ret.tResult.MovieSets)
                tSearchResults.TVEpisodes.AddRange(ret.tResult.TVEpisodes)
                tSearchResults.TVSeasons.AddRange(ret.tResult.TVSeasons)
                tSearchResults.TVShows.AddRange(ret.tResult.TVShows)
            End If
        Next

        Return tSearchResults
    End Function

    Public Function RunScraper(ByVal tDBElement As Database.DBElement, ByVal tScrapeType As Enums.ScrapeType) As MediaContainers.ScrapeResultsContainer
        Select Case tDBElement.ContentType
            Case Enums.ContentType.Movie
                logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Start] {0}", tDBElement.Filename))

                Dim modules As IEnumerable(Of _externalModuleClass) = externalModules.Where(Function(e) e.ScraperEngine IsNot Nothing) '.OrderBy(Function(e) e.ModuleOrder)
                Dim ret As Interfaces.ScrapeResults
                Dim tScrapedData As New List(Of MediaContainers.Movie)
                Dim tScrapedImages As New List(Of MediaContainers.ImageResultsContainer)
                Dim tScrapedThemes As New List(Of MediaContainers.Theme)
                Dim tScrapedTrailers As New List(Of MediaContainers.Trailer)

                While Not ModulesLoaded
                    Application.DoEvents()
                End While

                If (modules.Count() <= 0) Then
                    logger.Warn("[ModulesManager] [Scrape] [Movie] [Abort] No scrapers enabled")
                Else
                    For Each _externalScraperModule As _externalModuleClass In modules
                        logger.Trace(String.Format("[ModulesManager] [Scrape] [Using] {0}", _externalScraperModule.Base.ModuleName))
                        'AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie

                        ret = _externalScraperModule.ScraperEngine.RunScraper(tDBElement)

                        If Not ret.bCancelled Then
                            tScrapedData.Add(ret.tResult.Movie)

                            ''set new informations for following scrapers
                            'If ret.tScraperResult.Movie.IDSpecified Then
                            '    oDBMovie.Movie.ID = ret.tScraperResult.Movie.ID
                            'End If
                            'If ret.tScraperResult.Movie.IMDBIDSpecified Then
                            '    oDBMovie.Movie.IMDBID = ret.tScraperResult.Movie.IMDBID
                            'End If
                            'If ret.tScraperResult.Movie.OriginalTitleSpecified Then
                            '    oDBMovie.Movie.OriginalTitle = ret.tScraperResult.Movie.OriginalTitle
                            'End If
                            'If ret.tScraperResult.Movie.TitleSpecified Then
                            '    oDBMovie.Movie.Title = ret.tScraperResult.Movie.Title
                            'End If
                            'If ret.tScraperResult.Movie.TMDBIDSpecified Then
                            '    oDBMovie.Movie.TMDBID = ret.tScraperResult.Movie.TMDBID
                            'End If
                            'If ret.tScraperResult.Movie.YearSpecified Then
                            '    oDBMovie.Movie.Year = ret.tScraperResult.Movie.Year
                            'End If
                        End If

                        If ret.tResult.Images IsNot Nothing Then
                            tScrapedImages.Add(ret.tResult.Images)
                        End If
                        'RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                        If ret.bBreakChain Then Exit For
                    Next

                    'Merge scraperresults considering global datascraper settings
                    tDBElement = NFO.MergeDataScraperResults_Movie(tDBElement, tScrapedData, tScrapeType)

                    'create cache paths for Actor Thumbs
                    tDBElement.Movie.CreateCachePaths_ActorsThumbs()
                End If

                If tScrapedData.Count > 0 Then
                    logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Done] {0}", tDBElement.Filename))
                Else
                    logger.Trace(String.Format("[ModulesManager] [Scrape] [Movie] [Done] [No Scraper Results] {0}", tDBElement.Filename))
                    'Return True 'TODO: need a new trigger
                End If
                'Return ret.bCancelled
        End Select
        Return New MediaContainers.ScrapeResultsContainer
    End Function

    '''' <summary>
    '''' Request that enabled movie scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Movie to be scraped</param>
    '''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    '''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    'Public Function ScrapeData_Movie(ByRef DBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
    '    logger.Trace(String.Format("[ModulesManager] [ScrapeData_Movie] [Start] {0}", DBElement.Filename))
    '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(DBElement, showMessage) Then
    '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_Movie) = externalScrapersModules_Data_Movie.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
    '        Dim ret As Interfaces.ModuleResult_Data_Movie
    '        Dim ScrapedList As New List(Of MediaContainers.Movie)

    '        While Not ModulesLoaded
    '            Application.DoEvents()
    '        End While

    '        'clean DBMovie if the movie is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
    '        If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifiers.DoSearch Then
    '            DBElement.ImagesContainer = New MediaContainers.ImagesContainer
    '            DBElement.Movie = New MediaContainers.Movie

    '            DBElement.Movie.Title = StringUtils.FilterTitleFromPath_Movie(DBElement.Filename, DBElement.IsSingle, DBElement.Source.UseFolderName)
    '            DBElement.Movie.Year = StringUtils.FilterYearFromPath_Movie(DBElement.Filename, DBElement.IsSingle, DBElement.Source.UseFolderName)
    '        End If

    '        'create a clone of DBMovie
    '        Dim oDBMovie As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

    '        If (modules.Count() <= 0) Then
    '            logger.Warn("[ModulesManager] [ScrapeData_Movie] [Abort] No scrapers enabled")
    '        Else
    '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_Movie In modules
    '                'logger.Trace(String.Format("[ModulesManager] [ScrapeData_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))

    '                ret = _externalScraperModule.SettingsPanel.Scraper_Movie(oDBMovie, ScrapeModifiers, ScrapeType, ScrapeOptions)

    '                If ret.Cancelled Then Return ret.Cancelled

    '                If ret.Result IsNot Nothing Then
    '                    ScrapedList.Add(ret.Result)

    '                    'set new informations for following scrapers
    '                    If ret.Result.IDSpecified Then
    '                        oDBMovie.Movie.ID = ret.Result.ID
    '                    End If
    '                    If ret.Result.IMDBIDSpecified Then
    '                        oDBMovie.Movie.IMDBID = ret.Result.IMDBID
    '                    End If
    '                    If ret.Result.OriginalTitleSpecified Then
    '                        oDBMovie.Movie.OriginalTitle = ret.Result.OriginalTitle
    '                    End If
    '                    If ret.Result.TitleSpecified Then
    '                        oDBMovie.Movie.Title = ret.Result.Title
    '                    End If
    '                    If ret.Result.TMDBIDSpecified Then
    '                        oDBMovie.Movie.TMDBID = ret.Result.TMDBID
    '                    End If
    '                    If ret.Result.YearSpecified Then
    '                        oDBMovie.Movie.Year = ret.Result.Year
    '                    End If
    '                End If
    '                If ret.breakChain Then Exit For
    '            Next

    '            'Merge scraperresults considering global datascraper settings
    '            DBElement = NFO.MergeDataScraperResults_Movie(DBElement, ScrapedList, ScrapeType, ScrapeOptions)

    '            'create cache paths for Actor Thumbs
    '            DBElement.Movie.CreateCachePaths_ActorsThumbs()
    '        End If

    '        If ScrapedList.Count > 0 Then
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_Movie] [Done] {0}", DBElement.Filename))
    '        Else
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_Movie] [Done] [No Scraper Results] {0}", DBElement.Filename))
    '            Return True 'TODO: need a new trigger
    '        End If
    '        Return ret.Cancelled
    '    Else
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_Movie] [Abort] [Offline] {0}", DBElement.Filename))
    '        Return True 'Cancelled
    '    End If
    'End Function
    '''' <summary>
    '''' Request that enabled movie scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">MovieSet to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    '''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie set scrapers are enabled, a silent warning is generated.</remarks>
    'Public Function ScrapeData_MovieSet(ByRef DBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
    '    logger.Trace(String.Format("[ModulesManager] [ScrapeData_MovieSet] [Start] {0}", DBElement.MovieSet.Title))
    '    'If DBMovieSet.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_MovieSet(DBMovieSet, showMessage) Then
    '    Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_MovieSet) = externalScrapersModules_Data_MovieSet.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
    '    Dim ret As Interfaces.ModuleResult_Data_MovieSet
    '    Dim ScrapedList As New List(Of MediaContainers.MovieSet)

    '    While Not ModulesLoaded
    '        Application.DoEvents()
    '    End While

    '    'clean DBMovie if the movie is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
    '    If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifiers.DoSearch Then
    '        Dim tmpTitle As String = DBElement.MovieSet.Title

    '        DBElement.ImagesContainer = New MediaContainers.ImagesContainer
    '        DBElement.MovieSet = New MediaContainers.MovieSet

    '        DBElement.MovieSet.Title = tmpTitle
    '    End If

    '    'create a clone of DBMovieSet
    '    Dim oDBMovieSet As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

    '    If (modules.Count() <= 0) Then
    '        logger.Warn("[ModulesManager] [ScrapeData_MovieSet] [Abort] No scrapers enabled")
    '    Else
    '        For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_MovieSet In modules
    '            'logger.Trace(String.Format("[ModulesManager] [ScrapeData_MovieSet] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))

    '            ret = _externalScraperModule.SettingsPanel.Scraper(oDBMovieSet, ScrapeModifiers, ScrapeType, ScrapeOptions)

    '            If ret.Cancelled Then
    '                logger.Trace(String.Format("[ModulesManager] [ScrapeData_MovieSet] [Cancelled] [No Scraper Results] {0}", DBElement.MovieSet.Title))
    '                Return ret.Cancelled
    '            End If

    '            If ret.Result IsNot Nothing Then
    '                ScrapedList.Add(ret.Result)

    '                'set new informations for following scrapers
    '                If ret.Result.TitleSpecified Then
    '                    oDBMovieSet.MovieSet.Title = ret.Result.Title
    '                End If
    '                If ret.Result.TMDBSpecified Then
    '                    oDBMovieSet.MovieSet.TMDB = ret.Result.TMDB
    '                End If
    '            End If
    '            If ret.breakChain Then Exit For
    '        Next

    '        'Merge scraperresults considering global datascraper settings
    '        DBElement = NFO.MergeDataScraperResults_MovieSet(DBElement, ScrapedList, ScrapeType, ScrapeOptions)
    '    End If

    '    If ScrapedList.Count > 0 Then
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_MovieSet] [Done] {0}", DBElement.MovieSet.Title))
    '    Else
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_MovieSet] [Done] [No Scraper Results] {0}", DBElement.MovieSet.Title))
    '        Return True 'TODO: need a new trigger
    '    End If
    '    Return ret.Cancelled
    '    'Else
    '    'Return True 'Cancelled
    '    'End If
    'End Function

    'Public Function ScrapeData_TVEpisode(ByRef DBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
    '    logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVEpisode] [Start] {0}", DBElement.Filename))
    '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
    '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
    '        Dim ret As Interfaces.ModuleResult_Data_TVEpisode
    '        Dim ScrapedList As New List(Of MediaContainers.EpisodeDetails)

    '        While Not ModulesLoaded
    '            Application.DoEvents()
    '        End While

    '        'create a clone of DBTV
    '        Dim oEpisode As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

    '        If (modules.Count() <= 0) Then
    '            logger.Warn("[ModulesManager] [ScrapeData_TVEpisode] [Abort] No scrapers enabled")
    '        Else
    '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_TV In modules
    '                'logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVEpisode] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))

    '                ret = _externalScraperModule.SettingsPanel.Scraper_TVEpisode(oEpisode, ScrapeOptions)

    '                If ret.Cancelled Then Return ret.Cancelled

    '                If ret.Result IsNot Nothing Then
    '                    ScrapedList.Add(ret.Result)

    '                    'set new informations for following scrapers
    '                    If ret.Result.AiredSpecified Then
    '                        oEpisode.TVEpisode.Aired = ret.Result.Aired
    '                    End If
    '                    If ret.Result.EpisodeSpecified Then
    '                        oEpisode.TVEpisode.Episode = ret.Result.Episode
    '                    End If
    '                    If ret.Result.IMDBSpecified Then
    '                        oEpisode.TVEpisode.IMDB = ret.Result.IMDB
    '                    End If
    '                    If ret.Result.SeasonSpecified Then
    '                        oEpisode.TVEpisode.Season = ret.Result.Season
    '                    End If
    '                    If ret.Result.TitleSpecified Then
    '                        oEpisode.TVEpisode.Title = ret.Result.Title
    '                    End If
    '                    If ret.Result.TMDBSpecified Then
    '                        oEpisode.TVEpisode.TMDB = ret.Result.TMDB
    '                    End If
    '                    If ret.Result.TVDBSpecified Then
    '                        oEpisode.TVEpisode.TVDB = ret.Result.TVDB
    '                    End If
    '                End If
    '                If ret.breakChain Then Exit For
    '            Next

    '            'Merge scraperresults considering global datascraper settings
    '            DBElement = NFO.MergeDataScraperResults_TVEpisode_Single(DBElement, ScrapedList, ScrapeOptions)

    '            'create cache paths for Actor Thumbs
    '            DBElement.TVEpisode.CreateCachePaths_ActorsThumbs()
    '        End If

    '        If ScrapedList.Count > 0 Then
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVEpisode] [Done] {0}", DBElement.Filename))
    '        Else
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVEpisode] [Done] [No Scraper Results] {0}", DBElement.Filename))
    '            Return True 'TODO: need a new trigger
    '        End If
    '        Return ret.Cancelled
    '    Else
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVEpisode] [Abort] [Offline] {0}", DBElement.Filename))
    '        Return True 'Cancelled
    '    End If
    'End Function

    'Public Function ScrapeData_TVSeason(ByRef DBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
    '    logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVSeason] [Start] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
    '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
    '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
    '        Dim ret As Interfaces.ModuleResult_Data_TVSeason
    '        Dim ScrapedList As New List(Of MediaContainers.SeasonDetails)

    '        While Not ModulesLoaded
    '            Application.DoEvents()
    '        End While

    '        'create a clone of DBTV
    '        Dim oSeason As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

    '        If (modules.Count() <= 0) Then
    '            logger.Warn("[ModulesManager] [ScrapeData_TVSeason] [Abort] No scrapers enabled")
    '        Else
    '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_TV In modules
    '                'logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVSeason] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))

    '                ret = _externalScraperModule.SettingsPanel.Scraper_TVSeason(oSeason, ScrapeOptions)

    '                If ret.Cancelled Then Return ret.Cancelled

    '                If ret.Result IsNot Nothing Then
    '                    ScrapedList.Add(ret.Result)

    '                    'set new informations for following scrapers
    '                    If ret.Result.TMDBSpecified Then
    '                        oSeason.TVSeason.TMDB = ret.Result.TMDB
    '                    End If
    '                    If ret.Result.TVDBSpecified Then
    '                        oSeason.TVSeason.TVDB = ret.Result.TVDB
    '                    End If
    '                End If
    '                If ret.breakChain Then Exit For
    '            Next

    '            'Merge scraperresults considering global datascraper settings
    '            DBElement = NFO.MergeDataScraperResults_TVSeason(DBElement, ScrapedList, ScrapeOptions)
    '        End If

    '        If ScrapedList.Count > 0 Then
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVSeason] [Done] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
    '        Else
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVSeason] [Done] [No Scraper Results] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
    '            Return True 'TODO: need a new trigger
    '        End If
    '        Return ret.Cancelled
    '    Else
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVSeason] [Abort] [Offline] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
    '        Return True 'Cancelled
    '    End If
    'End Function
    '''' <summary>
    '''' Request that enabled movie scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Show to be scraped</param>
    '''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    '''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    'Public Function ScrapeData_TVShow(ByRef DBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
    '    logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVShow] [Start] {0}", DBElement.TVShow.Title))
    '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
    '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
    '        Dim ret As Interfaces.ModuleResult_Data_TVShow
    '        Dim ScrapedList As New List(Of MediaContainers.TVShow)

    '        While Not ModulesLoaded
    '            Application.DoEvents()
    '        End While

    '        'clean DBTV if the tv show is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
    '        If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifiers.DoSearch Then
    '            DBElement.ExtrafanartsPath = String.Empty
    '            DBElement.ImagesContainer = New MediaContainers.ImagesContainer
    '            DBElement.NfoPath = String.Empty
    '            DBElement.Seasons.Clear()
    '            DBElement.ThemePath = String.Empty
    '            DBElement.TVShow = New MediaContainers.TVShow

    '            DBElement.TVShow.Title = StringUtils.FilterTitleFromPath_TVShow(DBElement.ShowPath)

    '            For Each sEpisode As Database.DBElement In DBElement.Episodes
    '                Dim iEpisode As Integer = sEpisode.TVEpisode.Episode
    '                Dim iSeason As Integer = sEpisode.TVEpisode.Season
    '                Dim strAired As String = sEpisode.TVEpisode.Aired
    '                sEpisode.ImagesContainer = New MediaContainers.ImagesContainer
    '                sEpisode.NfoPath = String.Empty
    '                sEpisode.TVEpisode = New MediaContainers.EpisodeDetails With {.Aired = strAired, .Episode = iEpisode, .Season = iSeason}
    '            Next
    '        End If

    '        'create a clone of DBTV
    '        Dim oShow As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

    '        If (modules.Count() <= 0) Then
    '            logger.Warn("[ModulesManager] [ScrapeData_TVShow] [Abort] No scrapers enabled")
    '        Else
    '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_TV In modules
    '                'logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVShow] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))

    '                ret = _externalScraperModule.SettingsPanel.Scraper_TVShow(oShow, ScrapeModifiers, ScrapeType, ScrapeOptions)

    '                If ret.Cancelled Then Return ret.Cancelled

    '                If ret.Result IsNot Nothing Then
    '                    ScrapedList.Add(ret.Result)

    '                    'set new informations for following scrapers
    '                    If ret.Result.IMDBSpecified Then
    '                        oShow.TVShow.IMDB = ret.Result.IMDB
    '                    End If
    '                    If ret.Result.OriginalTitleSpecified Then
    '                        oShow.TVShow.OriginalTitle = ret.Result.OriginalTitle
    '                    End If
    '                    If ret.Result.TitleSpecified Then
    '                        oShow.TVShow.Title = ret.Result.Title
    '                    End If
    '                    If ret.Result.TMDBSpecified Then
    '                        oShow.TVShow.TMDB = ret.Result.TMDB
    '                    End If
    '                    If ret.Result.TVDBSpecified Then
    '                        oShow.TVShow.TVDB = ret.Result.TVDB
    '                    End If
    '                End If
    '                If ret.breakChain Then Exit For
    '            Next

    '            'Merge scraperresults considering global datascraper settings
    '            DBElement = NFO.MergeDataScraperResults_TV(DBElement, ScrapedList, ScrapeType, ScrapeOptions, ScrapeModifiers.withEpisodes)

    '            'create cache paths for Actor Thumbs
    '            DBElement.TVShow.CreateCachePaths_ActorsThumbs()
    '            If ScrapeModifiers.withEpisodes Then
    '                For Each tEpisode As Database.DBElement In DBElement.Episodes
    '                    tEpisode.TVEpisode.CreateCachePaths_ActorsThumbs()
    '                Next
    '            End If
    '        End If

    '        If ScrapedList.Count > 0 Then
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVShow] [Done] {0}", DBElement.TVShow.Title))
    '        Else
    '            logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVShow] [Done] [No Scraper Results] {0}", DBElement.TVShow.Title))
    '            Return True 'TODO: need a new trigger
    '        End If
    '        Return ret.Cancelled
    '    Else
    '        logger.Trace(String.Format("[ModulesManager] [ScrapeData_TVShow] [Abort] [Offline] {0}", DBElement.TVShow.Title))
    '        Return True 'Cancelled
    '    End If
    'End Function
    '''' <summary>
    '''' Request that enabled movie image scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_Movie(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.ImageResultsContainer, ByVal ScrapeModifiers As Structures.ScrapeModifiers, ByVal showMessage As Boolean) As Boolean
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeImage_Movie] [Start] {0}", DBElement.Filename))
        '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(DBElement, showMessage) Then
        '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_Movie) = externalScrapersModules_Image_Movie.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '        Dim ret As Interfaces.ModuleResult

        '        While Not ModulesLoaded
        '            Application.DoEvents()
        '        End While

        '        If (modules.Count() <= 0) Then
        '            logger.Warn("[ModulesManager] [ScrapeImage_Movie] [Abort] No scrapers enabled")
        '        Else
        '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_Movie In modules
        '                'logger.Trace(String.Format("[ModulesManager] [ScrapeImage_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
        '                If QueryScraperCapabilities(_externalScraperModule, ScrapeModifiers, DBElement.ContentType) Then
        '                    Dim aContainer As New MediaContainers.ImageResultsContainer
        '                    ret = _externalScraperModule.SettingsPanel.Scraper(DBElement, aContainer, ScrapeModifiers)
        '                    If aContainer IsNot Nothing Then
        '                        ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
        '                        ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
        '                        ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
        '                        ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
        '                        ImagesContainer.MainDiscArts.AddRange(aContainer.MainDiscArts)
        '                        ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
        '                        ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
        '                        ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
        '                    End If
        '                    If ret.breakChain Then Exit For
        '                End If
        '            Next

        '            'sorting
        '            ImagesContainer.SortAndFilter(DBElement)

        '            'create cache paths
        '            ImagesContainer.CreateCachePaths(DBElement)
        '        End If

        '        logger.Trace(String.Format("[ModulesManager] [ScrapeImage_Movie] [Done] {0}", DBElement.Filename))
        '        Return ret.Cancelled
        '    Else
        '        logger.Trace(String.Format("[ModulesManager] [ScrapeImage_Movie] [Abort] [Offline] {0}", DBElement.Filename))
        '        Return True 'Cancelled
        '    End If
        Return True
    End Function
    '''' <summary>
    '''' Request that enabled movieset image scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Movieset to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_MovieSet(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.ImageResultsContainer, ByVal ScrapeModifiers As Structures.ScrapeModifiers) As Boolean
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeImage_MovieSet] [Start] {0}", DBElement.MovieSet.Title))
        '    Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_MovieSet) = externalScrapersModules_Image_MovieSet.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '    Dim ret As Interfaces.ModuleResult

        '    While Not ModulesLoaded
        '        Application.DoEvents()
        '    End While

        '    If (modules.Count() <= 0) Then
        '        logger.Warn("[ModulesManager] [ScrapeImage_MovieSet] [Abort] No scrapers enabled")
        '    Else
        '        For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_MovieSet In modules
        '            'logger.Trace(String.Format("[ModulesManager] [ScrapeImage_MovieSet] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
        '            If QueryScraperCapabilities_Image_MovieSet(_externalScraperModule, ScrapeModifiers) Then
        '                Dim aContainer As New MediaContainers.ImageResultsContainer
        '                ret = _externalScraperModule.SettingsPanel.Scraper(DBElement, aContainer, ScrapeModifiers)
        '                If aContainer IsNot Nothing Then
        '                    ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
        '                    ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
        '                    ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
        '                    ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
        '                    ImagesContainer.MainDiscArts.AddRange(aContainer.MainDiscArts)
        '                    ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
        '                    ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
        '                    ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
        '                End If
        '                If ret.breakChain Then Exit For
        '            End If
        '        Next

        '        'sorting
        '        ImagesContainer.SortAndFilter(DBElement)

        '        'create cache paths
        '        ImagesContainer.CreateCachePaths(DBElement)
        '    End If

        '    logger.Trace(String.Format("[ModulesManager] [ScrapeImage_MovieSet] [Done] {0}", DBElement.MovieSet.Title))
        '    Return ret.Cancelled
        Return True
    End Function
    '''' <summary>
    '''' Request that enabled tv image scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">TV Show to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="ScrapeModifiers">What kind of image is being scraped (poster, fanart, etc)</param>
    '''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_TV(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.ImageResultsContainer, ByVal ScrapeModifiers As Structures.ScrapeModifiers, ByVal showMessage As Boolean) As Boolean
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeImage_TV] [Start] {0}", DBElement.TVShow.Title))
        '    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
        '        Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_TV) = externalScrapersModules_Image_TV.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '        Dim ret As Interfaces.ModuleResult

        '        While Not ModulesLoaded
        '            Application.DoEvents()
        '        End While

        '        'workaround to get MainFanarts for AllSeasonsFanarts, EpisodeFanarts and SeasonFanarts,
        '        'also get MainBanners, MainLandscapes and MainPosters for AllSeasonsBanners, AllSeasonsLandscapes and AllSeasonsPosters
        '        If ScrapeModifiers.AllSeasonsBanner Then
        '            ScrapeModifiers.MainBanner = True
        '            ScrapeModifiers.SeasonBanner = True
        '        End If
        '        If ScrapeModifiers.AllSeasonsFanart Then
        '            ScrapeModifiers.MainFanart = True
        '            ScrapeModifiers.SeasonFanart = True
        '        End If
        '        If ScrapeModifiers.AllSeasonsLandscape Then
        '            ScrapeModifiers.MainLandscape = True
        '            ScrapeModifiers.SeasonLandscape = True
        '        End If
        '        If ScrapeModifiers.AllSeasonsPoster Then
        '            ScrapeModifiers.MainPoster = True
        '            ScrapeModifiers.SeasonPoster = True
        '        End If
        '        If ScrapeModifiers.EpisodeFanart Then
        '            ScrapeModifiers.MainFanart = True
        '        End If
        '        If ScrapeModifiers.MainExtrafanarts Then
        '            ScrapeModifiers.MainFanart = True
        '        End If
        '        If ScrapeModifiers.MainExtrathumbs Then
        '            ScrapeModifiers.MainFanart = True
        '        End If
        '        If ScrapeModifiers.SeasonFanart Then
        '            ScrapeModifiers.MainFanart = True
        '        End If

        '        If (modules.Count() <= 0) Then
        '            logger.Warn("[ModulesManager] [ScrapeImage_TV] [Abort] No scrapers enabled")
        '        Else
        '            For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_TV In modules
        '                'logger.Trace(String.Format("[ModulesManager] [ScrapeImage_TV] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
        '                If QueryScraperCapabilities_Image_TV(_externalScraperModule, ScrapeModifiers) Then
        '                    Dim aContainer As New MediaContainers.ImageResultsContainer
        '                    ret = _externalScraperModule.SettingsPanel.Scraper(DBElement, aContainer, ScrapeModifiers)
        '                    If aContainer IsNot Nothing Then
        '                        ImagesContainer.EpisodeFanarts.AddRange(aContainer.EpisodeFanarts)
        '                        ImagesContainer.EpisodePosters.AddRange(aContainer.EpisodePosters)
        '                        ImagesContainer.SeasonBanners.AddRange(aContainer.SeasonBanners)
        '                        ImagesContainer.SeasonFanarts.AddRange(aContainer.SeasonFanarts)
        '                        ImagesContainer.SeasonLandscapes.AddRange(aContainer.SeasonLandscapes)
        '                        ImagesContainer.SeasonPosters.AddRange(aContainer.SeasonPosters)
        '                        ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
        '                        ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
        '                        ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
        '                        ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
        '                        ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
        '                        ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
        '                        ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
        '                    End If
        '                    If ret.breakChain Then Exit For
        '                End If
        '            Next

        '            'sorting
        '            ImagesContainer.SortAndFilter(DBElement)

        '            'create cache paths
        '            ImagesContainer.CreateCachePaths(DBElement)
        '        End If

        '        logger.Trace(String.Format("[ModulesManager] [ScrapeImage_TV] [Done] {0}", DBElement.TVShow.Title))
        '        Return ret.Cancelled
        '    Else
        '        logger.Trace(String.Format("[ModulesManager] [ScrapeImage_Movie] [Abort] [Offline] {0}", DBElement.Filename))
        '        Return True 'Cancelled
        '    End If
        Return True
    End Function
    '''' <summary>
    '''' Request that enabled movie theme scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="URLList">List of Themes objects that the scraper will append to. Note that only the URL is returned, 
    '''' not the full content of the trailer</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks></remarks>
    Public Function ScrapeTheme_Movie(ByRef DBElement As Database.DBElement, ByRef URLList As List(Of Themes)) As Boolean
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeTheme_Movie] [Start] {0}", DBElement.Filename))
        '    Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Theme_Movie) = externalScrapersModules_Theme_Movie.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '    Dim ret As Interfaces.ModuleResult
        '    Dim aList As List(Of Themes)

        '    While Not ModulesLoaded
        '        Application.DoEvents()
        '    End While

        '    If (modules.Count() <= 0) Then
        '        logger.Warn("[ModulesManager] [ScrapeTheme_Movie] [Abort] No scrapers enabled")
        '    Else
        '        For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Theme_Movie In modules
        '            'logger.Trace(String.Format("[ModulesManager] [ScrapeTheme_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
        '            aList = New List(Of Themes)
        '            ret = _externalScraperModule.SettingsPanel.Scraper(DBElement, aList)
        '            If aList IsNot Nothing AndAlso aList.Count > 0 Then
        '                For Each aIm In aList
        '                    URLList.Add(aIm)
        '                Next
        '            End If
        '            If ret.breakChain Then Exit For
        '        Next
        '    End If
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeTheme_Movie] [Done] {0}", DBElement.Filename))
        '    Return ret.Cancelled
        Return True
    End Function
    '''' <summary>
    '''' Request that enabled movie trailer scrapers perform their functions on the supplied movie
    '''' </summary>
    '''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    '''' <param name="Type">NOT ACTUALLY USED!</param>
    '''' <param name="TrailerList">List of Trailer objects that the scraper will append to. Note that only the URL is returned, 
    '''' not the full content of the trailer</param>
    '''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    '''' <remarks></remarks>
    Public Function ScrapeTrailer_Movie(ByRef DBElement As Database.DBElement, ByVal Type As Enums.ModifierType, ByRef TrailerList As List(Of MediaContainers.Trailer)) As Boolean
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeTrailer_Movie] [Start] {0}", DBElement.Filename))
        '    Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Trailer_Movie) = externalScrapersModules_Trailer_Movie.Where(Function(e) e.SettingsPanel.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '    Dim ret As Interfaces.ModuleResult

        '    While Not ModulesLoaded
        '        Application.DoEvents()
        '    End While

        '    If (modules.Count() <= 0) Then
        '        logger.Warn("[ModulesManager] [ScrapeTrailer_Movie] [Abort] No scrapers enabled")
        '    Else
        '        For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Trailer_Movie In modules
        '            'logger.Trace(String.Format("[ModulesManager] [ScrapeTrailer_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
        '            Dim aList As New List(Of MediaContainers.Trailer)
        '            ret = _externalScraperModule.SettingsPanel.Scraper(DBElement, Type, aList)
        '            If aList IsNot Nothing Then
        '                TrailerList.AddRange(aList)
        '            End If
        '            If ret.breakChain Then Exit For
        '        Next
        '    End If
        '    logger.Trace(String.Format("[ModulesManager] [ScrapeTrailer_Movie] [Done] {0}", DBElement.Filename))
        '    Return ret.Cancelled
        Return True
    End Function

    Public Sub SaveSettings()
        Dim tmpForXML As New List(Of _XMLEmberModuleClass)

        While Not ModulesLoaded
            Application.DoEvents()
        End While

        For Each _externalProcessorModule As _externalGenericModuleClass In externalGenericModules
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalProcessorModule.AssemblyName
            t.AssemblyFileName = _externalProcessorModule.AssemblyFileName
            t.ModuleEnabled = _externalProcessorModule.ProcessorModule.ModuleEnabled
            t.ContentType = _externalProcessorModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalModuleClass In externalModules
            For Each nSettingsPanel As Containers.SettingsPanel In _externalScraperModule.Base.InjectSettingsPanels
                Dim t As New _XMLEmberModuleClass
                t.AssemblyName = _externalScraperModule.AssemblyName
                t.AssemblyFileName = _externalScraperModule.AssemblyFilename
                t.ModuleEnabled = nSettingsPanel.Enabled
                t.ModuleOrder = nSettingsPanel.Order
                't.ContentType = nSettingsPanel.ContentType
                tmpForXML.Add(t)
            Next
        Next
        Master.eSettings.EmberModules = tmpForXML
        Master.eSettings.Save()
    End Sub

    Public Sub SetModuleEnable_Generic(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("[ModulesManager] [SetModuleEnable_Generic] Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalGenericModuleClass) = externalGenericModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("[ModulesManager] [SetModuleEnable_Generic] No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalProcessorModule As _externalGenericModuleClass In modules
                Try
                    _externalProcessorModule.ProcessorModule.ModuleEnabled = value
                Catch ex As Exception
                    logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable(ByVal strModuleAssembly As String, ByVal tType As Enums.SettingsPanelType, ByVal bValue As Boolean)
        If (String.IsNullOrEmpty(strModuleAssembly)) Then
            logger.Error(String.Format("[ModulesManager] [SetScraperEnable] Invalid ModuleAssembly: <{0}>", strModuleAssembly))
            Return
        End If

        Dim modules As IEnumerable(Of _externalModuleClass) = externalModules.Where(Function(p) p.AssemblyName = strModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn(String.Format("[ModulesManager] [SetScraperEnable_Data_Movie]  modules of type <{0}> were found", strModuleAssembly))
        Else
            For Each _externalScraperModule As _externalModuleClass In modules
                Try
                    '_externalScraperModule.Base.SettingsPanel.ScraperEnabled = bValue
                Catch ex As Exception
                    logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & strModuleAssembly & "> to enabled status <" & bValue & ">")
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Data_MovieSet(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Data_MovieSet] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_MovieSet) = externalScraperModulesSettingsPanels_Data_MovieSet.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Data_MovieSet] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_MovieSet In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Data_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Data_TV] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Data_TV) = externalScraperModulesSettingsPanels_Data_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Data_TV] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Data_TV In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Image_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Image_Movie] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_Movie) = externalScraperModulesSettingsPanels_Image_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Image_Movie] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_Movie In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Image_MovieSet(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Image_MovieSet] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_MovieSet) = externalScraperModulesSettingsPanels_Image_MovieSet.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Image_MovieSet] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_MovieSet In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Image_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Image_TV] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Image_TV) = externalScraperModulesSettingsPanels_Image_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Image_TV] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Image_TV In externalScraperModulesSettingsPanels_Image_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name)
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Theme_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Theme_Movie] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Theme_Movie) = externalScraperModulesSettingsPanels_Theme_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Theme_Movie] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Theme_Movie In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Theme_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Theme_TV] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Theme_TV) = externalScraperModulesSettingsPanels_Theme_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Theme_TV] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Theme_TV In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Public Sub SetScraperEnable_Trailer_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        'If (String.IsNullOrEmpty(ModuleAssembly)) Then
        '    logger.Error("[ModulesManager] [SetScraperEnable_Trailer_Movie] Invalid ModuleAssembly")
        '    Return
        'End If

        'Dim modules As IEnumerable(Of _externalScraperModuleSettingsPanelClass_Trailer_Movie) = externalScraperModulesSettingsPanels_Trailer_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        'If (modules.Count < 0) Then
        '    logger.Warn("[ModulesManager] [SetScraperEnable_Trailer_Movie] No modules of type <{0}> were found", ModuleAssembly)
        'Else
        '    For Each _externalScraperModule As _externalScraperModuleSettingsPanelClass_Trailer_Movie In modules
        '        Try
        '            _externalScraperModule.SettingsPanel.ScraperEnabled = value
        '        Catch ex As Exception
        '            logger.Error(ex, New StackFrame().GetMethod().Name & Convert.ToChar(Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">")
        '        End Try
        '    Next
        'End If
    End Sub

    Private Sub GenericRunCallBack(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        RaiseEvent GenericEvent(mType, _params)
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Structure AssemblyListItem

#Region "Fields"

        Public Assembly As Reflection.Assembly
        Public AssemblyName As String

#End Region 'Fields

    End Structure

    Structure VersionItem

#Region "Fields"

        Public AssemblyFileName As String
        Public Name As String
        Public NeedUpdate As Boolean
        Public Version As String

#End Region 'Fields

    End Structure

    Class EmberRuntimeObjects

#Region "Fields"

        Private _ContextMenuMovieList As ContextMenuStrip
        Private _ContextMenuMovieSetList As ContextMenuStrip
        Private _ContextMenuTVEpisodeList As ContextMenuStrip
        Private _ContextMenuTVSeasonList As ContextMenuStrip
        Private _ContextMenuTVShowList As ContextMenuStrip
        Private _FilterMovies As String
        Private _FilterMoviesSearch As String
        Private _FilterMoviesType As String
        Private _FilterTVShows As String
        Private _FilterTVShowsSearch As String
        Private _FilterTVShowsType As String
        Private _ListMovieSets As String
        Private _ListMovies As String
        Private _ListTVShows As String
        Private _LoadMedia As LoadMedia
        Private _MainMenu As MenuStrip
        Private _MainTabControl As TabControl
        Private _MainToolStrip As ToolStrip
        Private _MediaListMovieSets As DataGridView
        Private _MediaListMovies As DataGridView
        Private _MediaListTVEpisodes As DataGridView
        Private _MediaListTVSeasons As DataGridView
        Private _MediaListTVShows As DataGridView
        Private _MediaTabSelected As Structures.MainTabType
        Private _OpenImageViewer As OpenImageViewer
        Private _TrayMenu As ContextMenuStrip


#End Region 'Fields

#Region "Delegates"

        Delegate Sub LoadMedia(ByVal Scan As Structures.ScanOrClean, ByVal SourceID As Long)

        'all runtime object including Function (delegate) that need to be exposed to Modules
        Delegate Sub OpenImageViewer(ByVal _Image As Image)

#End Region 'Delegates

#Region "Properties"

        Public Property ListMovies() As String
            Get
                Return If(_ListMovies IsNot Nothing, _ListMovies, "movielist")
            End Get
            Set(ByVal value As String)
                _ListMovies = value
            End Set
        End Property

        Public Property ListMovieSets() As String
            Get
                Return If(_ListMovieSets IsNot Nothing, _ListMovieSets, "setslist")
            End Get
            Set(ByVal value As String)
                _ListMovieSets = value
            End Set
        End Property

        Public Property ListTVShows() As String
            Get
                Return If(_ListTVShows IsNot Nothing, _ListTVShows, "tvshowlist")
            End Get
            Set(ByVal value As String)
                _ListTVShows = value
            End Set
        End Property

        Public Property FilterMovies() As String
            Get
                Return _FilterMovies
            End Get
            Set(ByVal value As String)
                _FilterMovies = value
            End Set
        End Property

        Public Property FilterMoviesSearch() As String
            Get
                Return _FilterMoviesSearch
            End Get
            Set(ByVal value As String)
                _FilterMoviesSearch = value
            End Set
        End Property

        Public Property FilterMoviesType() As String
            Get
                Return _FilterMoviesType
            End Get
            Set(ByVal value As String)
                _FilterMoviesType = value
            End Set
        End Property
        Public Property FilterTVShows() As String
            Get
                Return _FilterTVShows
            End Get
            Set(ByVal value As String)
                _FilterTVShows = value
            End Set
        End Property

        Public Property FilterTVShowsSearch() As String
            Get
                Return _FilterTVShowsSearch
            End Get
            Set(ByVal value As String)
                _FilterTVShowsSearch = value
            End Set
        End Property

        Public Property FilterTVShowsType() As String
            Get
                Return _FilterTVShowsType
            End Get
            Set(ByVal value As String)
                _FilterTVShowsType = value
            End Set
        End Property

        Public Property MediaTabSelected() As Structures.MainTabType
            Get
                Return _MediaTabSelected
            End Get
            Set(ByVal value As Structures.MainTabType)
                _MediaTabSelected = value
            End Set
        End Property

        Public Property MainToolStrip() As ToolStrip
            Get
                Return _MainToolStrip
            End Get
            Set(ByVal value As ToolStrip)
                _MainToolStrip = value
            End Set
        End Property

        Public Property MediaListMovies() As DataGridView
            Get
                Return _MediaListMovies
            End Get
            Set(ByVal value As DataGridView)
                _MediaListMovies = value
            End Set
        End Property

        Public Property MediaListMovieSets() As DataGridView
            Get
                Return _MediaListMovieSets
            End Get
            Set(ByVal value As DataGridView)
                _MediaListMovieSets = value
            End Set
        End Property

        Public Property MediaListTVEpisodes() As DataGridView
            Get
                Return _MediaListTVEpisodes
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVEpisodes = value
            End Set
        End Property

        Public Property MediaListTVSeasons() As DataGridView
            Get
                Return _MediaListTVSeasons
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVSeasons = value
            End Set
        End Property

        Public Property MediaListTVShows() As DataGridView
            Get
                Return _MediaListTVShows
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVShows = value
            End Set
        End Property

        Public Property ContextMenuMovieList() As ContextMenuStrip
            Get
                Return _ContextMenuMovieList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuMovieList = value
            End Set
        End Property

        Public Property ContextMenuMovieSetList() As ContextMenuStrip
            Get
                Return _ContextMenuMovieSetList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuMovieSetList = value
            End Set
        End Property

        Public Property ContextMenuTVEpisodeList() As ContextMenuStrip
            Get
                Return _ContextMenuTVEpisodeList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVEpisodeList = value
            End Set
        End Property

        Public Property ContextMenuTVSeasonList() As ContextMenuStrip
            Get
                Return _ContextMenuTVSeasonList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVSeasonList = value
            End Set
        End Property

        Public Property ContextMenuTVShowList() As ContextMenuStrip
            Get
                Return _ContextMenuTVShowList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVShowList = value
            End Set
        End Property

        Public Property MainMenu() As MenuStrip
            Get
                Return _MainMenu
            End Get
            Set(ByVal value As MenuStrip)
                _MainMenu = value
            End Set
        End Property

        Public Property TrayMenu() As ContextMenuStrip
            Get
                Return _TrayMenu
            End Get
            Set(ByVal value As ContextMenuStrip)
                _TrayMenu = value
            End Set
        End Property

        Public Property MainTabControl() As TabControl
            Get
                Return _MainTabControl
            End Get
            Set(ByVal value As TabControl)
                _MainTabControl = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub DelegateLoadMedia(ByRef lm As LoadMedia)
            'Setup from EmberAPP
            _LoadMedia = lm
        End Sub

        Public Sub DelegateOpenImageViewer(ByRef IV As OpenImageViewer)
            _OpenImageViewer = IV
        End Sub

        Public Sub InvokeLoadMedia(ByVal Scan As Structures.ScanOrClean, Optional ByVal SourceID As Long = -1)
            'Invoked from Modules
            _LoadMedia.Invoke(Scan, SourceID)
        End Sub

        Public Sub InvokeOpenImageViewer(ByRef _image As Image)
            _OpenImageViewer.Invoke(_image)
        End Sub

#End Region 'Methods

    End Class

    Class _externalGenericModuleClass

#Region "Fields"

        Public AssemblyFileName As String

        'Public Enabled As Boolean
        Public AssemblyName As String
        Public ModuleOrder As Integer 'TODO: not important at this point.. for 1.5
        Public ProcessorModule As Interfaces.GenericEngine 'Object
        Public Type As List(Of Enums.ModuleEventType)
        Public ContentType As Enums.ContentType = Enums.ContentType.Generic

#End Region 'Fields

    End Class

    Class _externalModuleClass

#Region "Fields"

        Public AssemblyFilename As String
        Public AssemblyName As String
        Public Base As Interfaces.Base
        Public GenericEngine As Interfaces.GenericEngine
        Public ScraperEngine As Interfaces.ScraperEngine
        Public SearchEngine As Interfaces.SearchEngine
        'Public SettingsPanels As List(Of Interfaces.ScraperModuleSettingsPanel_Data_Movie)

#End Region 'Fields

    End Class

    <XmlRoot("EmberModule")>
    Class _XMLEmberModuleClass

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ContentType As Enums.ContentType
        Public ModuleEnabled As Boolean
        Public ModuleOrder As Integer

#End Region 'Fields

    End Class

#End Region 'Nested Types

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class