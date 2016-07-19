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

Public Class frmSettingsPanel_Search_Movie

#Region "Events"

    Public Event ModuleNeedsRestart()
    Public Event ModuleSettingsChanged()
    Public Event ModuleStateChanged(ByVal bIsEnabled As Boolean, ByVal tPanelType As Enums.SettingsPanelType, ByVal intDifforder As Integer)

#End Region 'Events

#Region "Fields"

#End Region 'Fields

#Region "Properties"

#End Region 'Properties

#Region "Methods"

    Public Sub New()
        InitializeComponent()
        SetUp()
    End Sub

    Private Sub pbTMDBApiKeyInfo_Click(sender As Object, e As EventArgs) Handles pbTMDBApiKeyInfo.Click
        Functions.Launch(My.Resources.urlAPIKey)
    End Sub

    Private Sub btnDown_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDown.Click
        'Dim order As Integer = ModulesManager.Instance.externalModules.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.Count - 1 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.ModuleOrder = order + 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order + 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, 1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUp.Click
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order > 0 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.ModuleOrder = order - 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order - 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, -1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleStateChanged(chkEnabled.Checked, Enums.SettingsPanelType.MovieData, 0)
    End Sub

    Private Sub chkGetAdult_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkGetAdultItems.CheckedChanged, chkSearchDeviant.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkSearchDeviant_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkSearchDeviant.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkFallBackEng_CheckedChanged(sender As Object, e As EventArgs) Handles chkFallBackEng.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub txtApiKey_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtApiKey.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Sub orderChanged()
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.Count > 1 Then
        '    btnDown.Enabled = (order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_Movie.Count - 1)
        '    btnUp.Enabled = (order > 0)
        'Else
        '    btnDown.Enabled = False
        '    btnUp.Enabled = False
        'End If
    End Sub

    Private Sub SetUp()
        chkEnabled.Text = Master.eLang.GetString(774, "Enabled")
        chkFallBackEng.Text = Master.eLang.GetString(922, "Fallback to english")
        chkGetAdultItems.Text = Master.eLang.GetString(1046, "Include Adult Items")
        chkSearchDeviant.Text = Master.eLang.GetString(98, "Search -/+ 1 year if no search result was found")
        lblApiKey.Text = String.Concat(Master.eLang.GetString(870, "TMDB API Key"), ":")
        lblScraperOrder.Text = Master.eLang.GetString(168, "Scrape Order")
        txtApiKey.WatermarkText = Master.eLang.GetString(1189, "Ember Media Manager Embedded API Key")
    End Sub

#End Region 'Methods

End Class