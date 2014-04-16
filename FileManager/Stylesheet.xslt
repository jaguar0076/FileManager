<xsl:stylesheet version="1.0"
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:ext="http://exslt.org/common" exclude-result-prefixes="ext">

  <xsl:output method="xml" indent="yes"/>

  <xsl:strip-space elements="*" />

  <xsl:key match="File[@MediaYear != 0]" name="MediaYears" use="@MediaYear"/>

  <xsl:key match="//File[@MediaArtists != '']" name="AlbumArtists" use="@MediaArtists"/>

  <xsl:template match="/">

    <xsl:variable name="VPass1">

      <xsl:call-template name="YearProcess"/>
      <!--Works alone-->

    </xsl:variable>

    <!--<xsl:apply-templates mode="Artist_Display" select="ext:node-set($VPass1)"/>-->

  </xsl:template>

  <xsl:template match="/" name="YearProcess" mode="Year_Display">

    <xsl:for-each select="//File[generate-id(.)= generate-id(key('MediaYears', @MediaYear)[1])]">

      <xsl:sort select="@MediaYear"/>

      <MediaYear>

        <xsl:attribute name="Year">

          <xsl:value-of select="@MediaYear"/>

        </xsl:attribute>

        <xsl:for-each select="key('MediaYears', @MediaYear)">

          <xsl:copy>

            <xsl:copy-of select="@*"/>

          </xsl:copy>

        </xsl:for-each>

      </MediaYear>

    </xsl:for-each>

  </xsl:template>
  <!--
  <xsl:template match="/" mode="Artist_Display">

    <xsl:for-each select="//File[generate-id(.)= generate-id(key('AlbumArtists', @MediaArtists)[1])]">

      <xsl:sort select="ancestor::MediaYear[1]/@Year"/>

      <xsl:sort select="@MediaArtists"/>

      <MediaArtists>

        <xsl:attribute name="AlbumArtists">

          <xsl:value-of select="@MediaArtists"/>

        </xsl:attribute>

        <xsl:attribute name="AlbumYear">

          <xsl:value-of select="ancestor::MediaYear[1]/@Year"/>

        </xsl:attribute>

        <xsl:for-each select="key('AlbumArtists', @MediaArtists)">

          <xsl:copy>-->

            <!--<xsl:copy-of select="ancestor::MediaYear[1]"/>-->

            <!--<xsl:copy-of select="node() | @* | node()"/>

          </xsl:copy>

        </xsl:for-each>

      </MediaArtists>
    
      
    </xsl:for-each>

  </xsl:template>-->

</xsl:stylesheet>