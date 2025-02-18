# BTMS Test Data Generator

This test generator allows us to manage sets of test data for different uses, and store them, either in blob storage or
locally.

The solution is organised as scenarios, which create related messages that together result in a single testable outcome.

A dataset then brings together the scenarios, and a time period and number of records per day, to generate the given
number of each scenario across the time period.

NB. The standard Azure App Registration we currently use doesn't have write access, and using our own creds in the app
isn't quite working, so I've been generating locally and then syncing to blob storage.

## Generating a scenario sample folder given a resource ID

We can use files from the datalake datasets to generate scenario folders that have the messages for a given MRN &/or CHED number, to allow us to use those resources in unit tests. This allows us to easily reproduce sequences seen in prod (from the redacted dataset) or other environments, as a test, and should allow us reproduce issues that we find and validate fixes for them.

See the implementations of SpecificFilesScenarioGenerator for examples. The files can be used as-is, in a single unit test, or can be modified using the same builder pattern we use for other scenarios before being built. It's even possible to use them in load tests, by customising them to ensure they're unique using the builders (by changing CHED numbers, MRNs etc).

The CLI has a method that will do this for us:

`download-scenario-data --environment Local --clearance-request 24GBD2UOWTWYM5LAR8 --rootFolder PRODREDACTED-202412 --outputFolder ~/src/defra/cdms/btms-test-data/Samples/Mrn-24GBD2UOWTWYM5LAR8`

## Merging datasets

As we download datasets each month, we want to also have a single 'all' data set. This is called PRODREDACTED-ALL in the SND data.

Something like the below should work, however I haven't been able to work out the auth...

azcopy login --tenant-id c9d74090-b4e6-4b04-981d-e6757a

azcopy list https://snddmpinfdl1001.blob.core.windows.net/dmp-1001/PRODREDACTED-202411/ALVS/2024/11/01

azcopy sync 'https://snddmpinfdl1001.blob.core.windows.net/dmp-1001/PRODREDACTED-202411' 'https://snddmpinfdl1001.blob.core.windows.net/dmp-1001/PRODREDACTED-ALL' --recursive

Instead, i've merged locally and then synched up to blob storage

## Interacting with blob storage to push generated datasets

az account set --subscription 7d775166-9d6c-4ac2-91a5-61904bae5caa

az storage blob directory delete --container-name dmp-data-1001 --directory-path GENERATED-LOADTEST --account-name snddmpinfdl1001 --recursive
az storage blob directory delete --container-name dmp-data-1001 --directory-path GENERATED-LOADTEST-BASIC --account-name snddmpinfdl1001 --recursive
az storage blob directory delete --container-name dmp-data-1001 --directory-path PRODREDACTED-20241204 --account-name snddmpinfdl1001 --recursive

az storage blob directory upload -c dmp-data-1001 -d --account-name snddmpinfdl1001 -s
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST --recursive

az storage blob upload-batch -d dmp-data-1001 --account-name snddmpinfdl1001 -s
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST-90Dx10k --destination-path GENERATED-LOADTEST-90Dx10k

az storage blob upload-batch -d dmp-data-1001 --account-name snddmpinfdl1001 -s TestDataGenerator/.test-data-generator/GENERATED-LOADTEST-BASIC --destination-path GENERATED-LOADTEST-BASIC
az storage blob upload-batch -d dmp-data-1001 --account-name snddmpinfdl1001 -s TestDataGenerator/.test-data-generator/PRODREDACTED-20241204 --destination-path PRODREDACTED-20241204


az storage blob directory list -c dmp-data-1001 -d  --account-name snddmpinfdl1001

az storage fs directory list -f dmp-data-1001 -d --account-name snddmpinfdl1001


az storage fs directory download -f dmp-data-1001 -s "PRODREDACTED-20241204" -d "TestDataGenerator/.test-data-generator" --recursive --account-name snddmpinfdl1001
