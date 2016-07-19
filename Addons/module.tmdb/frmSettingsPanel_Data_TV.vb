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

Public Class frmSettingsPanel_Data_TV

#Region "Events"

    Public Event ModuleNeedsRestart()
    Public Event ModuleSettingsChanged()
    Public Event ModuleStateChanged(ByVal state As Boolean, ByVal tPanelType As Enums.SettingsPanelType, ByVal difforder As Integer)

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
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.Count - 1 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.ModuleOrder = order + 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order + 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, 1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUp.Click
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If order > 0 Then
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.ModuleOrder = order - 1).ModuleOrder = order
        '    ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder = order - 1
        '    RaiseEvent ModuleStateChanged(chkEnabled.Checked, -1)
        '    orderChanged()
        'End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleStateChanged(chkEnabled.Checked, Enums.SettingsPanelType.TVData, 0)
    End Sub

    Private Sub SettingsChanged(ByVal sender As Object, ByVal e As EventArgs) Handles _
        chkFallBackEng.CheckedChanged,
        chkGetAdultItems.CheckedChanged,
        chkScraperEpisodeActors.CheckedChanged,
        chkScraperEpisodeAired.CheckedChanged,
        chkScraperEpisodeCredits.CheckedChanged,
        chkScraperEpisodeDirectors.CheckedChanged,
        chkScraperEpisodeGuestStars.CheckedChanged,
        chkScraperEpisodePlot.CheckedChanged,
        chkScraperEpisodeRating.CheckedChanged,
        chkScraperEpisodeTitle.CheckedChanged,
        chkScraperSeasonAired.CheckedChanged,
        chkScraperSeasonPlot.CheckedChanged,
        chkScraperSeasonTitle.CheckedChanged,
        chkScraperShowActors.CheckedChanged,
        chkScraperShowCertifications.CheckedChanged,
        chkScraperShowCountries.CheckedChanged,
        chkScraperShowCreators.CheckedChanged,
        chkScraperShowGenres.CheckedChanged,
        chkScraperShowOriginalTitle.CheckedChanged,
        chkScraperShowPlot.CheckedChanged,
        chkScraperShowPremiered.CheckedChanged,
        chkScraperShowRating.CheckedChanged,
        chkScraperShowRuntime.CheckedChanged,
        chkScraperShowStatus.CheckedChanged,
        chkScraperShowStudios.CheckedChanged,
        chkScraperShowTitle.CheckedChanged

        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub txtApiKey_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtApiKey.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Sub orderChanged()
        'Dim order As Integer = ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.FirstOrDefault(Function(p) p.AssemblyName = clsModuleTMDB._AssemblyName).ModuleOrder
        'If ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.Count > 1 Then
        '    btnDown.Enabled = (order < ModulesManager.Instance.externalScraperModulesSettingsPanels_Data_TV.Count - 1)
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
        chkScraperEpisodeActors.Text = Master.eLang.GetString(231, "Actors")
        chkScraperEpisodeAired.Text = Master.eLang.GetString(728, "Aired")
        chkScraperEpisodeCredits.Text = Master.eLang.GetString(394, "Credits (Writers)")
        chkScraperEpisodeDirectors.Text = Master.eLang.GetString(940, "Directors")
        chkScraperEpisodeGuestStars.Text = Master.eLang.GetString(508, "Guest Stars")
        chkScraperEpisodePlot.Text = Master.eLang.GetString(65, "Plot")
        chkScraperEpisodeRating.Text = Master.eLang.GetString(400, "Rating")
        chkScraperEpisodeTitle.Text = Master.eLang.GetString(21, "Title")
        chkScraperSeasonAired.Text = Master.eLang.GetString(728, "Aired")
        chkScraperSeasonPlot.Text = Master.eLang.GetString(65, "Plot")
        chkScraperSeasonTitle.Text = Master.eLang.GetString(21, "Title")
        chkScraperShowActors.Text = Master.eLang.GetString(231, "Actors")
        chkScraperShowCertifications.Text = Master.eLang.GetString(56, "Certifications")
        chkScraperShowCountries.Text = Master.eLang.GetString(237, "Countries")
        chkScraperShowCreators.Text = Master.eLang.GetString(744, "Creators")
        chkScraperShowGenres.Text = Master.eLang.GetString(725, "Genres")
        chkScraperShowOriginalTitle.Text = Master.eLang.GetString(302, "Original Title")
        chkScraperShowPlot.Text = Master.eLang.GetString(65, "Plot")
        chkScraperShowPremiered.Text = Master.eLang.GetString(724, "Premiered")
        chkScraperShowRating.Text = Master.eLang.GetString(400, "Rating")
        chkScraperShowRuntime.Text = Master.eLang.GetString(396, "Runtime")
        chkScraperShowStatus.Text = Master.eLang.GetString(215, "Status")
        chkScraperShowStudios.Text = Master.eLang.GetString(226, "Studios")
        chkScraperShowTitle.Text = Master.eLang.GetString(21, "Title")
        gbScraperFieldsOpts.Text = Master.eLang.GetString(791, "Scraper Fields - Scraper specific")
        gbScraperOpts.Text = Master.eLang.GetString(1186, "Scraper Options")
        lblApiKey.Text = String.Concat(Master.eLang.GetString(870, "TMDB API Key"), ":")
        lblInfoBottom.Text = String.Format(Master.eLang.GetString(790, "These settings are specific to this module.{0}Please refer to the global settings for more options."), Environment.NewLine)
        lblScraperOrder.Text = Master.eLang.GetString(168, "Scrape Order")
        txtApiKey.WatermarkText = Master.eLang.GetString(1189, "Ember Media Manager Embedded API Key")
    End Sub

#End Region 'Methods

End Class