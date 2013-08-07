<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:umb="urn:umbraco.library"
  exclude-result-prefixes="umb"
>

  <xsl:output method="xml" omit-xml-declaration="yes"/>
  <xsl:param name="currentPage"/>
  
  <xsl:template match="/">

  <xsl:variable name="searchString" select="umb:RequestQueryString('s')" />
    <xsl:variable name="searchValue">
      <xsl:choose>
        <xsl:when test="string(umb:RequestForm('s')) != ''">
          <xsl:value-of select="umb:RequestForm('s')" />
        </xsl:when>
        <xsl:when test="$searchString !=''">
          <xsl:value-of select="$searchString" />
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    
		<!-- Find the search page, and post search requests there -->
    <form method="get" action="{umb:NiceUrl($currentPage/ancestor-or-self::*/descendant::Search)}" class="search">
      <label>
        <span>Search </span>
        <input name="s" placeholder="Search" value="{$searchValue}"/>
      </label>    
      <input class="search submit" type="submit" value="Search" />
    </form>
   
  </xsl:template>
  
</xsl:stylesheet>