# Btms.Backend.Cli

The CLI allows us to create complex regularly used tools that can be run by developers on their machine rather than running within a service, in all environments.

## Generate Models

Consumes the IPAFFS, CDS/ALVS and GVMS schemas, and our own metadata about those schema (sensitive fields, naming and date standardisation, etc), to generate for each:

- A "Source" data model that can be used to handle the messages we receive or that are stored in the data lake
- An "Internal" data model that we store in mongo DB & present in our API
- A mapping library to translate between the two

## Download Scenario Data

Can be pointed at an environment, and provided with an MRN. Triggers a call to the API to search the data lake for the MRN and its related CHEDs, and then downloads the resulting zip for use locally.