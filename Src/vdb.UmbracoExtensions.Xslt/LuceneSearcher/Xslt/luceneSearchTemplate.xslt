<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:umb="urn:umbraco.library"
  exclude-result-prefixes="umb"
>

  <xsl:output method="xml" omit-xml-declaration="yes"/>
      
  <xsl:template name="header">
    <xsl:param name="numberOfRecords" />
    <xsl:param name="friendlySearchFields" />
    
    <p class="search-info">
      <xsl:choose>
        <xsl:when test="$friendlySearchFields !=''">
          <xsl:value-of select="$labels[@key = 'SearchYourSearchFor']" />
          <mark><xsl:value-of select="$friendlySearchFields" /></mark>
          <!-- Magic -->
          <xsl:choose>
            <xsl:when test="$numberOfRecords = 0">
              <xsl:value-of select="$labels[@key = 'SearchReturnedNoResults']" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:text> </xsl:text>
              <xsl:value-of select="$labels[@key = 'SearchMatched']" />
              <xsl:text> </xsl:text>
              <xsl:value-of select="$numberOfRecords" />
              <xsl:text> </xsl:text>
              <!-- Plurisation is very very important -->
              <xsl:choose>
                <xsl:when test="$numberOfRecords &gt; 1"><xsl:value-of select="$labels[@key = 'SearchPages']" /></xsl:when>
                <xsl:otherwise><xsl:value-of select="$labels[@key = 'SearchPage']" /></xsl:otherwise>
              </xsl:choose>
              <xsl:text>.</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$labels[@key = 'SearchCallToAction']" />  
        </xsl:otherwise>
      </xsl:choose>
    </p>
  </xsl:template>

  <xsl:template name="result">
    <xsl:param name="title" />
    <xsl:param name="teaser" />
    <xsl:param name="previewChars" />
    <xsl:param name="recordsPerPage" />
    <xsl:param name="numberOfRecords" />
    <xsl:param name="pageNumber" />
    <!-- Let's create a search result entry  -->
    <xsl:if test="position() &gt; $recordsPerPage * number($pageNumber) and position() &lt;= number($recordsPerPage * number($pageNumber) + $recordsPerPage )">
      <!-- Normal Template -->
      <article>
        <h1>
          <a href="{umb:NiceUrl(@id)}">
            <xsl:value-of select="$title" disable-output-escaping="yes" />
          </a>
        </h1>
        <p>
          <xsl:value-of select="umb:TruncateString($teaser, $previewChars, '...')" disable-output-escaping="yes" />
        </p>
        <a class="button" href="{umb:NiceUrl(@id)}"><xsl:value-of select="$labels[@key = 'GeneralReadMore']" />...</a>
      </article>
    </xsl:if>
  </xsl:template>      

</xsl:stylesheet>