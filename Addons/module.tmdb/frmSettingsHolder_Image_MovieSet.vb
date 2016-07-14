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
Imports EmberAPI
Imports System.Diagnostics

Public Class frmSettingsHolder_Image_MovieSet

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

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.Count - 1 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.ModuleOrder = order + 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order + 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, 1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order > 0 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.ModuleOrder = order - 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order - 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, -1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleStateChanged(chkEnabled.Checked, Enums.SettingsPanelType.MovieSetImage, 0)
    End Sub

    Private Sub chkScrapeFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapeFanart.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkScrapePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapePoster.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Sub orderChanged()
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.Count > 1 Then
        '    btnDown.Enabled = (order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_MovieSet.Count - 1)
        '    btnUp.Enabled = (order > 0)
        'Else
        '    btnDown.Enabled = False
        '    btnUp.Enabled = False
        'End If
    End Sub

    Sub SetUp()
        chkEnabled.Text = Master.eLang.GetString(774, "Enabled")
        chkScrapeFanart.Text = Master.eLang.GetString(149, "Fanart")
        chkScrapePoster.Text = Master.eLang.GetString(148, "Poster")
        gbScraperImagesOpts.Text = Master.eLang.GetString(268, "Images - Scraper specific")
        gbScraperOpts.Text = Master.eLang.GetString(1186, "Scraper Options")
        lblAPIKey.Text = String.Concat(Master.eLang.GetString(870, "TMDB API Key"), ":")
        lblInfoBottom.Text = String.Format(Master.eLang.GetString(790, "These settings are specific to this module.{0}Please refer to the global settings for more options."), Environment.NewLine)
        lblScraperOrder.Text = Master.eLang.GetString(168, "Scrape Order")
        txtApiKey.WatermarkText = Master.eLang.GetString(1189, "Ember Media Manager API key")
    End Sub

    Private Sub txtApiKey_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtApiKey.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub pbApiKeyInfo_Click(sender As System.Object, e As System.EventArgs) Handles pbApiKeyInfo.Click
        Functions.Launch(My.Resources.urlAPIKey)
    End Sub

#End Region 'Methods

End Class