BoardTracker
===

![](http://i.imgur.com/FI9VEZp.jpg)

BoardTracker helps you to track specific profiles of a board and saves their posts into a database.
You can use this data to create i.e. a website which shows the most recent posts of a companies' development team.
An example site would be [BlueTracker](http://www.wowhead.com/bluetracker) for WoW's team (BlueTracker was not created with this program of course, it's just an example).

###Features
---

* Database Types
  * Microsoft SQL Server (with LinqToSql)
  * MySQL
* Parallelized tracking of multiple websites
* Select specific parts of a post with jQuery-selectors
* Request-configuration
  * Number of requests till sleep
  * Sleep timespan in milliseconds
  * Delay in milliseconds between each request
* Disable/enable the storage of the post's content, if you don't need it
* Advanced logging through NLog

###Usage
---

1. Replace the connection string in your App.config file.
2. Create your dabatase with the sql scripts in the "DatabaseCreation"-folder (MySQL and MSSQL). Note for MSSQL: If you want to extend the VARCHAR-size of some columns, you have to update the LinqToSql models, too.
3. Update the tracking profiles to the websites that you want to track. Ensure that the jquery-selectors match to the website's html design!
4. Change the NLog-configuration file if you need additional logging targets/rules.
5. Start the program..

Any website where the post-history is openly accessible (without a login) and where a pagination-system exists can be tracked with the univeral data-provider.
Something like Twitter's virtual scrolling system is not supported. Feel free to implement your own data-provider to use specific website APIs though. :)

I have tested the tracking of profiles on the following sites:
* http://eu.battle.net/wow/en
* https://www.reddit.com/
* http://forum.blackdesertonline.com/

###Development Stage
---

There's only a repository for Microsoft SQL server and MySQL at the moment, you can create your own repository with the implementation of the interface "ITrackingRepository" though.

###Configuration
---

Tracking profiles are specified in the App.Config file.
A profile looks like this:

```xml
<!-- Blizzard Battle.Net board -->
<trackingConfiguration>
  <!-- The name of the website that you want to track -->
  <add key="website" value="WoW" />
  <!-- Its URL -->
  <add key="websiteUrl" value="http://eu.battle.net/wow/en"/>

  <!-- Define the data provider type which is used to download the data from the website, parse its html code and create the post-models, here.
  You can implement your own tracker for specific websites (i.e. Twitter with their API) and change the tracker type for every tracking profile seperately.
  The definition is optional, the standard provider is "Universal" -->
  <add key="dataProviderType" value="universal" />

  <!--A boolean to specify whether the content of a post should be stored
  The definition is optional, the standard value is "true"-->
  <add key="savePostContent" value="false"/>

  <!-- If a profile is tracked for the first time, the tracker will save the whole post-history.
  In the next tracking-rotation, only new posts will be tracked - the program doesn't search for updated old posts if you don't use this option!
  If the value is set to 30, all posts of the last thirty days will be checked for updates. -->
  <add key="checkLastXDaysForUpdates" value="30" />

  <!--There are two different ways how the tracker paginates through the profile history.
  The first one "templateUrl" uses the template url and replaces the {page}-tag with the current page,
  the second one "buttonLink" uses a link to get the url of the next page-->
  <add key="paginationType" value="templateUrl"/>

  <!-- The URL template of the profile history-->
  <add key="postHistoryTemplate" value="http://eu.battle.net/wow/en/search?f=post&amp;sort=time&amp;a={profile}&amp;page={page}"/>

  <!-- After the tracker completed the whole profile, it will sleep for a specific number of minutes-->
  <add key="requestRateInMinutes" value="10"/>
  <add key="requestsTillSleep" value="20"/>
  <add key="requestSleepInMilliseconds" value="1000"/>
  <!-- Sleep a little bit after every request -->
  <add key="requestDelayInMilliseconds" value="100" />

  <!-- Specify the jQuery selectors for the different parts of a post here-->
  <contentSelectors>
	<selector target="PostElementPostingDateTime" jqSelector=".meta" dataType="Html" regexPattern=".*(\d{2}/\d{2}/\d{2} \d{2}:\d{2})\s*$" regexReplace="$1" dateTimeFormat="dd/MM/yy HH:mm" />
	<selector target="PostElementContent" jqSelector=".content" dataType="Html" />
	<selector target="PostElementForum" jqSelector=".meta .sublink" dataType="Text" />
	<selector target="PostElementForumLink" jqSelector=".meta .sublink" dataType="Attribute" attributeName="href" regexPattern="^.*$" regexReplace="http://eu.battle.net$0" />
	<selector target="PostElementPostLink" jqSelector=".subheader-3 a" dataType="Attribute" attributeName="href" regexPattern="^.*$" regexReplace="http://eu.battle.net$0"  />
	<selector target="PostElementThreadTitle" jqSelector=".subheader-3 a" dataType="Text" />

	<!--The pagination element should select either the link of the next page or the last page number (depending on the pagination type)-->
	<selector target="PaginationElement" jqSelector=".ui-pagination li:nth-last-child(2) a" dataType="Attribute" attributeName="data-pagenum" />
	<!-- The post list returns a list of posts in the current page, typically a list of li-elements -->
	<selector target="PostList" jqSelector=".result" dataType="Html" />
  </contentSelectors>

  <!-- The profiles that you want to track..-->
  <trackedProfiles>
    <!-- Name = Name of the profile, templateKey = The key which is used for the {profile} tag in the template url -->
	<profile name="Watcher" templateKey="Watcher" />
  </trackedProfiles>
</trackingConfiguration>
```



