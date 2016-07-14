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

Namespace YouTube

    Public Class clsScraperYouTube

#Region "Fields"

        Shared logger As Logger = LogManager.GetCurrentClassLogger()

#End Region 'Fields

#Region "Methods"

        Public Function GetTrailers(ByVal strTitle As String) As MediaContainers.ScrapeResultsContainer
            Dim nScrapeResults As New MediaContainers.ScrapeResultsContainer
            nScrapeResults.Trailers = EmberAPI.YouTube.Scraper.SearchOnYouTube(String.Concat(strTitle, " ", Master.eSettings.MovieTrailerDefaultSearch))
            For Each tTrailer In nScrapeResults.Trailers
                tTrailer.Scraper = "YouTube"
            Next
            Return nScrapeResults
        End Function

#End Region 'Methods

#Region "Nested Types"

#End Region 'Nested Types

    End Class

End Namespace