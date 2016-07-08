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

Public Class frmSettingsHolder_Image_Movie

#Region "Events"

    Public Event ModuleSettingsChanged()

    Public Event ModuleStateChanged(ByVal state As Boolean, ByVal difforder As Integer)

    Public Event SetupNeedsRestart()

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
        Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        If order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.Count - 1 Then
            ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.ModuleOrder = order + 1).ModuleOrder = order
            ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order + 1
            RaiseEvent ModuleStateChanged(chkEnabled.Checked, 1)
            orderChanged()
        End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        If order > 0 Then
            ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.ModuleOrder = order - 1).ModuleOrder = order
            ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order - 1
            RaiseEvent ModuleStateChanged(chkEnabled.Checked, -1)
            orderChanged()
        End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleStateChanged(chkEnabled.Checked, 0)
    End Sub

    Private Sub chkScrapeFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapeFanart.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkScrapePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapePoster.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Sub orderChanged()
        Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        If ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.Count > 1 Then
            btnDown.Enabled = (order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Image_Movie.Count - 1)
            btnUp.Enabled = (order > 0)
        Else
            btnDown.Enabled = False
            btnUp.Enabled = False
        End If
    End Sub

    Sub SetUp()
        btnUnlockAPI.Text = Master.eLang.GetString(1188, "Use my own API key")
        chkEnabled.Text = Master.eLang.GetString(774, "Enabled")
        chkScrapeFanart.Text = Master.eLang.GetString(149, "Fanart")
        chkScrapePoster.Text = Master.eLang.GetString(148, "Poster")
        gbScraperImagesOpts.Text = Master.eLang.GetString(268, "Images - Scraper specific")
        gbScraperOpts.Text = Master.eLang.GetString(1186, "Scraper Options")
        lblAPIKey.Text = String.Concat(Master.eLang.GetString(870, "TMDB API Key"), ":")
        lblEMMAPI.Text = Master.eLang.GetString(1189, "Ember Media Manager API key")
        lblInfoBottom.Text = String.Format(Master.eLang.GetString(790, "These settings are specific to this module.{0}Please refer to the global settings for more options."), Environment.NewLine)
        lblScraperOrder.Text = Master.eLang.GetString(168, "Scrape Order")
    End Sub

    Private Sub btnUnlockAPI_Click(sender As Object, e As EventArgs) Handles btnUnlockAPI.Click
        If btnUnlockAPI.Text = Master.eLang.GetString(1188, "Use my own API key") Then
            btnUnlockAPI.Text = Master.eLang.GetString(443, "Use embedded API Key")
            lblEMMAPI.Visible = False
            txtApiKey.Enabled = True
        Else
            btnUnlockAPI.Text = Master.eLang.GetString(1188, "Use my own API key")
            lblEMMAPI.Visible = True
            txtApiKey.Enabled = False
            txtApiKey.Text = String.Empty
        End If
    End Sub

    Private Sub txtApiKey_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtApiKey.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub pbApiKeyInfo_Click(sender As System.Object, e As System.EventArgs) Handles pbApiKeyInfo.Click
        Functions.Launch(My.Resources.urlAPIKey)
    End Sub

#End Region 'Methods

End Class