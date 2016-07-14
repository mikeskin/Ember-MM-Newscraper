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

    Public Interface Base

#Region "Events"

        Event ModuleNeedsRestart()
        Event ModuleSettingsChanged()
        Event ModuleStateChanged(ByVal strName As String, ByVal tPanelType As Enums.SettingsPanelType, ByVal bIsEnabled As Boolean, ByVal intDiffOrder As Integer)

#End Region 'Events

#Region "Properties"

        'ReadOnly Property ModuleEnabled(ByVal tType As Enums.SettingsPanelType) As Boolean
        ReadOnly Property ModuleName() As String
        ReadOnly Property ModuleVersion() As String

#End Region 'Properties

#Region "Methods"

        Sub Init(ByVal sAssemblyName As String)
        Sub SaveSettingsPanel(ByVal DoDispose As Boolean)
        Sub ModuleOrderChanged(ByVal tPanelType As Enums.SettingsPanelType)

        Function InjectSettingsPanels() As List(Of Containers.SettingsPanel)
        'Function QueryCapabilities(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean

#End Region 'Methods

    End Interface

    Public Interface GenericEngine

#Region "Events"

        Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        Event ModuleNeedsRestart()
        Event ModuleSettingsChanged()
        Event ModuleStateChanged(ByVal strName As String, ByVal bState As Boolean, ByVal intDiffOrder As Integer)

#End Region 'Events

#Region "Properties"

        Property ModuleEnabled() As Boolean
        ReadOnly Property IsBusy() As Boolean
        ReadOnly Property ModuleName() As String
        ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType)
        ReadOnly Property ModuleVersion() As String

#End Region 'Properties

#Region "Methods"

        Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String)
        Function InjectSettingsPanel() As Containers.SettingsPanel
        Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), ByRef _singleobjekt As Object, ByRef _dbelement As Database.DBElement) As ModuleResult
        Sub SaveSettings(ByVal DoDispose As Boolean)

#End Region 'Methods

    End Interface

    Public Interface ScraperEngine

#Region "Events"

#End Region 'Events

#Region "Properties"

        ReadOnly Property ModuleEnabled(ByVal tType As Enums.SettingsPanelType) As Boolean

#End Region 'Properties

#Region "Methods"

        Function QueryCapabilities(ByVal tModifierType As Enums.ModifierType, ByVal tContentType As Enums.ContentType) As Boolean
        Function RunScraper(ByRef DBElement As Database.DBElement) As ScrapeResults

#End Region 'Methods

    End Interface

    Public Interface SearchEngine

#Region "Events"

#End Region 'Events

#Region "Properties"

        ReadOnly Property ModuleEnabled(ByVal tType As Enums.SettingsPanelType) As Boolean

#End Region 'Properties

#Region "Methods"

        Function QueryCapabilities(ByVal tContentType As Enums.ContentType) As Boolean
        Function RunSearch(ByVal strTitle As String, ByVal intYear As Integer, ByVal strLanguage As String, ByVal tContentType As Enums.ContentType) As SearchResults

#End Region 'Methods

    End Interface

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
        Public bBreakChain As Boolean
        ''' <summary>
        ''' An error has occurred in the module, and its operation has been cancelled. 
        ''' </summary>
        ''' <remarks></remarks>
        Public bCancelled As Boolean

#End Region 'Fields

    End Structure

    Public Structure ScrapeResults

#Region "Fields"

        Public bBreakChain As Boolean
        Public bCancelled As Boolean
        Public tResult As MediaContainers.ScrapeResultsContainer

#End Region 'Fields

    End Structure

    Public Structure SearchResults

#Region "Fields"

        Public bBreakChain As Boolean
        Public bCancelled As Boolean
        Public tResult As MediaContainers.SearchResultsContainer

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class