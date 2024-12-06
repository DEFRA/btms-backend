# Btms Test Data Generator

This test generator allows us to manage sets of test data for different uses, and store them, either in blob storage or
locally.

The solution is organised as scenarios, which create related messages that together result in a single testable outcome.

A dataset then brings together the scenarios, and a time period and number of records per day, to generate the given
number of each scenario across the time period.

NB. The standard Azure App Registration we currently use doesn't have write access, and using our own creds in the app
isn't quite working, so I've been generating locally and then syncing to blob storage.

az account set --subscription 7d775166-9d6c-4ac2-91a5-61904bae5caa

az storage blob directory delete --container-name dmp-data-1001 --directory-path GENERATED-LOADTEST --account-name
snddmpinfdl1001 --recursive
az storage blob directory delete --container-name dmp-data-1001 --directory-path GENERATED-LOADTEST-BASIC --account-name
snddmpinfdl1001 --recursive

az storage blob directory upload -c dmp-data-1001 -d --account-name snddmpinfdl1001 -s
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST --recursive

az storage blob upload-batch -d dmp-data-1001 --account-name snddmpinfdl1001 -s
<<<<<<< HEAD
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST --destination-path GENERATED-LOADTEST
=======
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST-90Dx10k --destination-path GENERATED-LOADTEST-90Dx10k
>>>>>>> c84d4f8 (fixed up test data generator warnings)
az storage blob upload-batch -d dmp-data-1001 --account-name snddmpinfdl1001 -s
TestDataGenerator/.test-data-generator/GENERATED-LOADTEST-BASIC --destination-path GENERATED-LOADTEST-BASIC