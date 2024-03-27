# MDR_Harvester
Takes downloaded source files and loads their data into the MDR source or staging databases.

The program uses the XML files already downloaded as a data source located in a source folder (one is designated for
each source). The XML files, or a subset as controlled by the parameters - see below - are converted into data in the
'sd' schema (= session data) tables within each source database. Note that the SD tables are dropped and created anew
on each run and thus only ever contain the data from the most recent harvest. The tables present will vary in different
databases, though if a table is present, it will have a consistent structure in every database. Therefore, converting
SD data represents the second and final stage of converting the source data into a consistent ECRIN schema. For that
reason, the detailed code for different sources can vary widely.

The program represents the second stage in the 5 stage MDR extraction process:

>Download => **Harvest** => Import => Coding => Aggregation

For a more detailed explanation of the extraction process and the MDR system, please see the project wiki (landing
page at https://ecrin-mdr.online/index.php/Project_Overview).
In particular, for the harvesting process, please see:

- http://ecrin-mdr.online/index.php/Harvesting_Data
- http://ecrin-mdr.online/index.php/Contextual_Data

and linked pages.

## Parameters and Usage
The table below shows the allowed parameters.

| Parameter | Description   |
|:----------| :-------------|
| -s        | Followed by a comma-delimited list of source IDs. The data is harvested for each specified host.
| -t        | Followed by an integer. Indicates the type of harvest to be carried out. (1 = full, i.e. all available files, 2 = only files downloaded since last import, 3 = test data only).
| -E        | As a flag, it establishes expected test data. If present, it only creates and fills tables for the 'expected' data, which can be compared with processed test data.
| -F        | As a flag, it harvests all test data. If present, it only creates and fills tables for the designated test data for comparison with expected test data.
| -G        | Is a flag that can be applied to prevent a normal harvest from occurring so that the SD tables are not recreated and reloaded. Instead, they are updated using revised contextual data so that - for example - organisation IDs and topic data codes can be re-applied. The option provides a relatively efficient way of updating data, though it works better if preceded with a type 1 complete harvest of all data. Because the data is revised, the various composite hash values summarising data content must also be re-created (see [Missing PIDs and Hashing](https://ecrin-mdr.online/index.php/Missing_PIDs_and_Hashing))
## Dependencies

The program is written in .Net 7.0.
It uses the following `Nuget` packages:

- `CommandLineParser 2.9.1` - to carry out the initial processing of the CLI arguments

- `Npgsql 7.0.0`, `Dapper 2.0.123` and `Dapper.contrib 2.0.78` to handle database connectivity

- `PostgreSQLCopyHelper 2.8.0` to support fast bulk inserts into Postgres

- `Microsoft.Extensions.Configuration 7.0.0`, and `.Configuration.Json 7.0.0` to read the json settings file.

- `Microsoft.Extensions.DependencyInjection 7.0.0` and `.Hosting 7.0.0` to support the initial setup of the application.



## Provenance
* Author: Steve Canham
* Contributor: Michele Scarlato
* Organisation: ECRIN (https://ecrin.org)
* System: Clinical Research Metadata Repository (MDR)
* Project: EOSC Life
* Funding: EU H2020 programme, grant 824087
