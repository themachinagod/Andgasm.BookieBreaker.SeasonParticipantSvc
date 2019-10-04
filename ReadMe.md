<h2>BookieBreaker Season Participants Service Stack:</h2>

<h3>BookieBreaker Season Participants Service Information</h3>

<p>The BookieBreaker Season Participants Service is a single stand alone micro service which is part of the larger 'BookieBreaker' micro service ecosystem.</p>

<p>The service has a sole responsibility for exracting club and club season participation data and passing this off to the season participants API to be stored in a data repo.</p>

<p>The service is triggered by the creation of new seasons by way of a BookieBreaker Service Bus - this can be simulated via PostMan with the following request.</p>

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


<p>The Service consists of the following components:
	<ul>
		<li>Season Participants API - responsible for managing season participant data interactions with the underlying data container</li>
		<li>Season Participants Extration Svc - responsible for parsing and extracting season participant data</li>
	</ul>
</p>
<p><b>Requires Asp.Net Core 2.1</b></p>