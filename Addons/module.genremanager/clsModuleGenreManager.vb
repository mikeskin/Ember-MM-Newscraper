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
Imports System.Drawing
Imports System.Windows.Forms

Public Class clsModuleGenreManager
    Implements Interfaces.Base

#Region "Delegates"

    Public Delegate Sub Delegate_SetToolsStripItem(value As ToolStripItem)
    Public Delegate Sub Delegate_RemoveToolsStripItem(value As ToolStripItem)
    Public Delegate Sub Delegate_AddToolsStripItem(tsi As ToolStripMenuItem, value As ToolStripMenuItem)

#End Region 'Delegates

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String

    Private WithEvents cmnuTrayToolsGenreManager As New ToolStripMenuItem
    Private WithEvents mnuMainToolsGenereManager As New ToolStripMenuItem

#End Region 'Fields

#Region "Events"

    Public Event ModuleNeedsRestart() Implements Interfaces.Base.ModuleNeedsRestart
    Public Event ModuleSettingsChanged() Implements Interfaces.Base.ModuleSettingsChanged
    Public Event ModuleStateChanged(ByVal strAssemblyName As String, ByVal tPanelType As Enums.SettingsPanelType, ByVal bIsEnabled As Boolean, ByVal intDifforder As Integer) Implements Interfaces.Base.ModuleStateChanged

#End Region 'Events

#Region "Properties"

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

    Sub Init(ByVal strAssemblyName As String) Implements Interfaces.Base.Init
        _AssemblyName = strAssemblyName
        Enable()
    End Sub

    Function InjectSettingsPanels() As List(Of Containers.SettingsPanel) Implements Interfaces.Base.InjectSettingsPanels
        Return New List(Of Containers.SettingsPanel)
    End Function

    Public Sub ScraperOrderChanged_Movie(ByVal tPanelType As Enums.SettingsPanelType) Implements Interfaces.Base.ModuleOrderChanged
        Return
    End Sub

    Sub SaveSettingsPanel(ByVal DoDispose As Boolean) Implements Interfaces.Base.SaveSettingsPanel
        Return
    End Sub

    Sub Enable()
        Dim tsi As New ToolStripMenuItem

        'mnuMainTools menu
        mnuMainToolsGenereManager.Image = New Bitmap(My.Resources.icon)
        mnuMainToolsGenereManager.Text = Master.eLang.GetString(782, "Genre Manager")
        mnuMainToolsGenereManager.Tag = New Structures.ModulesMenus With {.ForMovies = True, .IfTabMovies = True, .ForTVShows = True, .IfTabTVShows = True}
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.MainMenu.Items("mnuMainTools"), ToolStripMenuItem)
        AddToolsStripItem(tsi, mnuMainToolsGenereManager)

        'cmnuTrayTools
        cmnuTrayToolsGenreManager.Image = New Bitmap(My.Resources.icon)
        cmnuTrayToolsGenreManager.Text = Master.eLang.GetString(782, "Genre Manager")
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayTools"), ToolStripMenuItem)
        AddToolsStripItem(tsi, cmnuTrayToolsGenreManager)
    End Sub

    Public Sub AddToolsStripItem(control As ToolStripMenuItem, value As ToolStripItem)
        If control.Owner.InvokeRequired Then
            control.Owner.Invoke(New Delegate_AddToolsStripItem(AddressOf AddToolsStripItem), New Object() {control, value})
        Else
            control.DropDownItems.Add(value)
        End If
    End Sub

    Private Sub mnuGenreManager_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuMainToolsGenereManager.Click, cmnuTrayToolsGenreManager.Click
        'RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", False}))
        Using dGenreManager As New dlgGenreManager
            dGenreManager.ShowDialog()
        End Using
        'RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", True}))
        'RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"filllist", True, True, True}))
    End Sub

#End Region 'Methods

#Region "Nested Types"

#End Region  'Nested Types

End Class
