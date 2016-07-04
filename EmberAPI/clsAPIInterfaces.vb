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

Public Class Interfaces

#Region "Nested Interfaces"

    ' Interfaces for external Modules
    Public Interface GenericModule

#Region "Events"

        Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        Event ModuleSettingsChanged()
        Event ModuleSetupChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer)
        Event SetupNeedsRestart()

#End Region 'Events

#Region "Properties"

        Property Enabled() As Boolean
        ReadOnly Property IsBusy() As Boolean
        ReadOnly Property ModuleName() As String
        ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType)
        ReadOnly Property ModuleVersion() As String

#End Region 'Properties

#Region "Methods"

        Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String)
        Function InjectSetup() As Containers.SettingsPanel
        Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), ByRef _singleobjekt As Object, ByRef _dbelement As Database.DBElement) As ModuleResult
        Sub SaveSetup(ByVal DoDispose As Boolean)

#End Region 'Methods

    End Interface

    Public Interface ScraperModule

#Region "Events"

        Event ModuleSettingsChanged()
        Event SetupNeedsRestart()

#End Region 'Events

#Region "Properties"

        ReadOnly Property AnyScraperEnabled() As Boolean
        ReadOnly Property ModuleName() As String
        ReadOnly Property ModuleVersion() As String

#End Region 'Properties

#Region "Methods"

        Sub Init(ByVal sAssemblyName As String)
        Sub SaveSettingsPanel(ByVal DoDispose As Boolean)

        Function QueryModifierCapabilities(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean
        Function RunScraper(ByRef DBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As ScrapeResults
        Function RunSearch(ByVal strTitle As String, ByVal intYear As Integer, ByVal tContentType As Enums.ContentType) As SearchResults

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Data_Movie

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Data_MovieSet

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Data_TV

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Image_Movie

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Image_MovieSet

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"
        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Image_TV

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSettingsPanel() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Theme_Movie

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSetupScraper() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Theme_TV

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSetupScraper() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

    Public Interface ScraperModuleSettingsPanel_Trailer_Movie

#Region "Events"

        Event ScraperSetupChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Properties"

        Property ScraperEnabled() As Boolean

#End Region 'Properties

#Region "Methods"

        Function InjectSetupScraper() As Containers.SettingsPanel
        Sub ScraperOrderChanged()

#End Region 'Methods

    End Interface

#End Region 'Nested Interfaces

#Region "Nested Types"
    ''' <summary>
    ''' This structure is returned by most scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

#End Region 'Fields

    End Structure
    ''' <summary>
    ''' This structure is returned by movie data scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult_Data_Movie

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

        Public Result As MediaContainers.Movie

#End Region 'Fields

    End Structure
    ''' <summary>
    ''' This structure is returned by movieset data scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult_Data_MovieSet

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

        Public Result As MediaContainers.MovieSet

#End Region 'Fields

    End Structure
    ''' <summary>
    ''' This structure is returned by tv episode data scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult_Data_TVEpisode

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

        Public Result As MediaContainers.EpisodeDetails

#End Region 'Fields

    End Structure
    ''' <summary>
    ''' This structure is returned by tv season data scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult_Data_TVSeason

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

        Public Result As MediaContainers.SeasonDetails

#End Region 'Fields

    End Structure
    ''' <summary>
    ''' This structure is returned by tv show data scraper interfaces to represent the
    ''' status of the operation that was requested
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ModuleResult_Data_TVShow

#Region "Fields"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Public breakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Cancelled As Boolean

        Public Result As MediaContainers.TVShow

#End Region 'Fields

    End Structure

    Public Structure ScrapeResults

#Region "Fields"

        Public bBreakChain As Boolean
        Public bCancelled As Boolean
        Public tScraperResult As MediaContainers.ScrapeResultsContainer

#End Region 'Fields

    End Structure

    Public Structure SearchResults

#Region "Fields"

        Public bBreakChain As Boolean
        Public bCancelled As Boolean
        Public tScraperResult As MediaContainers.SearchResultsContainer

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class