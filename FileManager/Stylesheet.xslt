<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:output method="xml" indent="yes"/>

  <xsl:strip-space elements="*" />

  <xsl:key match="File[@MediaYear != 0]" name="MediaYears" use="@MediaYear"/>

  <xsl:key match="File" name="AlbumArtists" use="@MediaArtists"/>

  <xsl:template match="/">

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
  <xsl:template match="/File">

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

  </xsl:template>-->

</xsl:stylesheet>