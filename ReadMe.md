<h2>BookieBreaker Season Participants Service Stack</h2>

<h3>BookieBreaker Ecosystem Information</h3>

<p>
	The BookieBreaker software stack is broken into two categories;

	<ul>
		<li>Data Extraction - series of scalable micro services that will scrape data from WhoScored.coms data feeds</li>
		<li>Data Utilisation - series of applications that make use of the acquired data via REST API's</li>
	</ul>

	WhoScored.com has a wealth of information available for seasons, clubs, players, fixtures & match statistics.
	All data extraction services will communicate via an Azure Service Bus event pipeline.
	Note that all data extration services have request throttling to ensure that the endpoints don't spam the server.
</p>

<h3>BookieBreaker Season Participants Service Information</h3>

<p>The BookieBreaker Season Participants Service is a single stand alone micro service which is part of the larger 'BookieBreaker' micro service ecosystem.</p>

<p>The service has a sole responsibility for exracting club and club season participation data and passing this off to the season participants API to be stored in a data repo.</p>

<p>The service is triggered by the creation of new seasons by way of a BookieBreaker Service Bus - this can be simulated in the stage environment via PostMan with the following request to the API.</p>

<pre>
	POST /api/seasons HTTP/1.1
	Host: seasonparticipantsvcapi.azurewebsites.net
	Content-Type: application/json
	cache-control: no-cache
	Postman-Token: 2f34bbdc-c50e-43b7-a2d3-db7cab0de365
	{
	  "key": "string",
	  "name": "2017-18",
	  "startDate": "2017-06-01",
	  "endDate": "2018-5-31",
	  "tournamentKey": "EPL",
	  "countryKey": "gb-eng",
	  "regionCode": "252",
	  "tournamentCode": "2",
	  "seasonCode": "6335",
	  "stageCode": "13786"
	}------WebKitFormBoundary7MA4YWxkTrZu0gW--
</pre>

<h3>BookieBreaker Season Participants Service Development & Deployment Notes</h3>

<p>
	Master branch is configured to trigger CI/CD for both service components (API & Svc) for build and deployment to staging environment on Azure. 
	Any development should be done in a local feature branch (sourced from develop branch) and pull requests should be raised for review and merge to develop.
	Project owner will be responsible for merging develop branch into master and management of CI/CD pipeline.
	Currently no CI/CD tasks exists for creation of the Azure Service Bus, App Service Instances or SQL database - these exist from manual setup in stage environment currently and the CI/CD is just building and deploying the codebase.
</p>

<p>
	The Service consists of the following components:
	
	<ul>
		<li>Season Participants API - responsible for managing season participant data interactions with the underlying data container</li>
		<li>Season Participants Extration Svc - responsible for parsing and extracting season participant data</li>
	</ul>
</p>

<p><b>Requires Asp.Net Core 2.2</b></p>