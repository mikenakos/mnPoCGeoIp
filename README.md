# PoC assignment project \[mnPoCGeoIp\]

This is a Proof-of-Concept project that performs IP Geolocation lookups
by Mike Nakos

It consists of 3 endpoints

1.  Stores a look-up batch with a single IP Address, performs the
    look-up and updates the DB with the results

2.  Stores a batch with 1 or more IP Addresses, returns the batch id and
    the status endpoint URL and starts the background processing of each
    IP Address

3.  This is the batch status endpoint that informs about the completion
    progress and the ETA of the batch completion

## Installation instructions

1.  Run PowerShell

2.  Change directory to the project folder `mnPoCGeoIp`, where the
    mnPoCGeoIp.csproj lives

3.  Run the PowerShell batch `.\\setup-db.ps1` -- That will create the
    database and the schema

4.  In the appsettings.json file set the property
    "provider_geoip_access_token" with your own ipbase.com access
    token/api key

## Notes

### Note 1

>The 2nd case `/api/IPAddress/SaveIpBatch` normally, in production
environments should have pushed each batch to a message broker
(RabbitMQ, etc.) and this message to be consumed by a worker process,
out of this process. The worker would do the IP lookup job and process
each batch in the background.

>Following that approach, we avoid having a monolithic service and the
most important, don't stress the API with background processing.

### Note 2

>To feed the background processing thread I have used
System.Threading.Channels as a pipeline, to simulate the publish to the
message broker

### Note 3

>AI is used in terms of asking targeted questions to double check/confirm
the alignment with best practices and not to produce an entire function
or class, since that would slow-down the implementation to review any
produced code by the AI
