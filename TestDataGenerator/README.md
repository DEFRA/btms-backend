# BTMS Test Data Generator

This test generator allows us to manage sets of test data for different uses, and store them, either in blob storage or
locally.

The solution is organised as scenarios, which create related messages that together result in a single testable outcome.

A dataset then brings together the scenarios, and a time period and number of records per day, to generate the given
number of each scenario across the time period.

NB. The standard Azure App Registration we currently use doesn't have write access, and using our own creds in the app
isn't quite working, so I've been generating locally and then syncing to blob storage.

## Generating a scenario given a resource ID:

Copy all Movement files matching the ID into

find .test-data-generator/PRODREDACTED-202411/ALVS .test-data-generator/PRODREDACTED-202411/DECISIONS -type f -print0 | xargs -0 -P 4 -n 40 grep -l 24GBC4EB0D97OK4AR4 | xargs -I '{}' rsync -R '{}' ./Scenarios/SpecificFiles/DuplicateMovementItems-CDMS-211/

Copy all Movement files matching the ID into

find .test-data-generator/PRODREDACTED-202411/IPAFFS -type f -print0 | xargs -0 -P 4 -n 40 grep -l 5071194 | xargs -I '{}' rsync -R '{}' ./Scenarios/Samples/DuplicateMovementItems-CDMS-211/

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
