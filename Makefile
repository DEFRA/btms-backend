dependencies:
	dotnet tool restore

generate-openapi-spec: dependencies
	dotnet build -c Release --no-restore
	dotnet swagger tofile --output openapi.json ./Btms.Backend/bin/Release/net8.0/Btms.Backend.dll public-v0.1
