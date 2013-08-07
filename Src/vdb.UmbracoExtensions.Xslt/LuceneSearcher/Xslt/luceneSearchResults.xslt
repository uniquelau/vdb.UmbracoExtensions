<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:umb="urn:umbraco.library" xmlns:exslt.regx="urn:Exslt.ExsltRegularExpressions" xmlns:vdb.search="urn:vdb.search"
  exclude-result-prefixes="umb exslt.regx vdb.search"
>
  <xsl:import href="_lingual.xslt" />
  <xsl:output method="xml" omit-xml-declaration="yes"/>
  
  <xsl:param name="currentPage"/>
  
  <!-- TODO: Refactor, to use Macro/Parameters --> 
  <xsl:variable name="recordsPerPage" select="$currentPage/settingsSearchRecordsPerPage" />
  <xsl:variable name="previewChars" select="$currentPage/settingsSearchPreviewChars" />
  <xsl:variable name="highlightTerms" select="$currentPage/settingsSearchHighlightTerms" />

  <xsl:template match="/">
  
    <!-- Get Search String -->
    <xsl:variable name="searchString">
      <xsl:choose>
        <!-- form field value, if present -->
        <xsl:when test="umb:RequestForm('s') != ''">
          <xsl:value-of select="umb:RequestForm('s')" />
        </xsl:when>
        <!-- querystring value, if present -->
        <xsl:when test="umb:RequestQueryString('s') != ''">
          <xsl:value-of select="umb:RequestQueryString('s')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <!-- Clean String -->
    <xsl:variable name="searchStringClean" select="exslt.regx:replace($searchString, '[^\w\.@-]', '@', '/')"/>
  
    <!-- Filter on Property -->
    <!-- This is very helpful for multilingual / domain sites -->
    <xsl:variable name="filterProperty" select="'homepageNodeId'" />
    <xsl:variable name="filterPropertyValue" select="$currentLanguage" />
    <xsl:variable name="filter" select="'true'" />
  
    <!-- Put Together 'Search String' and 'Filter'  -->
    <xsl:variable name="searchStringAndFilter">
      <xsl:value-of select="$searchString" />
      <xsl:if test="$filter !=''">
        <xsl:text> AND </xsl:text>
        <xsl:value-of select="$filterProperty" />
        <xsl:text>: </xsl:text>
        <xsl:value-of select="$filterPropertyValue" />
      </xsl:if>
    </xsl:variable>
  
    <!-- Render Search Page -->
    <xsl:call-template name="search">
      <xsl:with-param name="searchString" select="$searchString" />
      <xsl:with-param name="searchStringAndFilter" select="$searchStringAndFilter" />
    </xsl:call-template>
  
  </xsl:template>

  <xsl:template name="search">
    <xsl:param name="searchString" />
    <xsl:param name="searchStringAndFilter" />
  
    <!-- Perform Search -->
    <xsl:variable name="results" select="vdb.search:LuceneSearchFull('SiteIndexer', $searchStringAndFilter, '&lt;mark&gt;', '&lt;/mark&gt;', 'contentBody,contentSummary,contentHeader')" />
    <xsl:variable name="numberOfRecords" select="count($results/*)" />
  
    <!-- Header -->
    <xsl:call-template name="header">
      <xsl:with-param name="numberOfRecords" select="$numberOfRecords" />
      <xsl:with-param name="friendlySearchFields" select="$searchString" />
    </xsl:call-template>
  
    <section class="search-results">
    <!-- Process the Results -->
    <xsl:for-each select="$results/* [@isDoc]">
      <!-- Make sure your Index only includes published items, or add test here -->
      <xsl:call-template name="result">
        <xsl:with-param name="recordsPerPage" select="$recordsPerPage" />
        <xsl:with-param name="numberOfRecords" select="$numberOfRecords" />
        <xsl:with-param name="pageNumber" select="$pageNumber" />
        <xsl:with-param name="title">
          <xsl:value-of select="(contentHeader | @nodeName[not(normalize-space(../contentHeader))])[1]" />
        </xsl:with-param>
        <xsl:with-param name="teaser">
          <xsl:value-of select="(contentSummary | contentBody[not(normalize-space(../contentSummary))])[1]" />
        </xsl:with-param>
        <xsl:with-param name="previewChars" select="$previewChars" />
      </xsl:call-template>
    </xsl:for-each>
    </section>
  
    <!-- Footer -->
    <xsl:call-template name="pagination">
      <xsl:with-param name="currentPage" select="$currentPage" />
      <xsl:with-param name="recordsPerPage" select="$recordsPerPage" />
      <xsl:with-param name="numberOfRecords" select="$numberOfRecords" />
      <xsl:with-param name="pageNumber" select="$pageNumber" />
      <xsl:with-param name="searchString" select="$searchString" />
      <xsl:with-param name="textNext" select="$labels[@key = 'PaginationNextPage']" />
      <xsl:with-param name="textPrev" select="$labels[@key = 'PaginationPreviousPage']" />
      <xsl:with-param name="injectQuery">
        <xsl:text>&amp;s=</xsl:text>
        <xsl:value-of select="$searchString"/>
      </xsl:with-param>
    </xsl:call-template>
    
  </xsl:template>

  <!-- Include Template -->
  <xsl:include href="helpers/_Pagination.xslt" />
  <xsl:include href="luceneSearchTemplate.xslt" />

</xsl:stylesheet>