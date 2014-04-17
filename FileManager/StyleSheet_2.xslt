<xsl:stylesheet version="1.0"
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="xml" indent="yes"/>

  <xsl:strip-space elements="*" />

  <xsl:key match="File[@MediaYear != 0]" name="MediaYears" use="@MediaYear"/>

  <!-- the following key has been designed on the assumption that MediaYear never contains a space-->
  <xsl:key match="File[@MediaArtists != '']" name="MediaArtists" use="concat(@MediaYear, ' ', @MediaArtists)"/>

  <!-- the following key has been designed on the assumption that MediaYear never contains a space AND MediaArtists never contains a %-->
  <xsl:key match="File[@MediaAlbum != '']" name="MediaAlbum" use="concat(@MediaYear, ' ', @MediaArtists, '%', @MediaAlbum)"/>

  <xsl:template match="/">

    <xsl:for-each select="//File[generate-id(.)= generate-id(key('MediaYears', @MediaYear)[1])]">

      <xsl:sort select="@MediaYear"/>

      <MediaYear Year="{@MediaYear}">

        <xsl:variable name="MediaYear" select="@MediaYear"/>

        <xsl:for-each select="//File[@MediaYear=$MediaYear and generate-id(.)= generate-id(key('MediaArtists', concat(@MediaYear, ' ', @MediaArtists))[1])]">

          <xsl:sort select="@MediaArtists"/>

          <MediaArtists Artist="{@MediaArtists}">

            <xsl:variable name="MediaArtists" select="@MediaArtists"/>

            <xsl:for-each select="//File[@MediaYear=$MediaYear and @MediaArtists=$MediaArtists and generate-id(.)= generate-id(key('MediaAlbum', concat(@MediaYear, ' ', @MediaArtists, '%', @MediaAlbum))[1])]">

              <xsl:sort select="@MediaAlbum"/>

              <MediaAlbum Album="{@MediaAlbum}">

                <xsl:variable name="MediaAlbum" select="@MediaAlbum"/>

                <xsl:for-each select="//File[@MediaYear=$MediaYear and @MediaArtists=$MediaArtists and @MediaAlbum=$MediaAlbum]">

                  <xsl:copy>

                    <xsl:copy-of select="node() | @* | node()"/>

                  </xsl:copy>

                </xsl:for-each>

              </MediaAlbum>

            </xsl:for-each>

          </MediaArtists>

        </xsl:for-each>

      </MediaYear>

    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>