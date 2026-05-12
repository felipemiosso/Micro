#!/bin/bash

# Run tests with coverage
# Exclude system, Microsoft namespaces and any generated files
dotnet test --collect:"XPlat Code Coverage" -- \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[System.*]*,[Microsoft.*]*" \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/*.generated.cs"

# Generate report
# Filter out system assemblies and generated files to avoid "file not exist" errors
reportgenerator \
-reports:"tests/Micro.Tests/TestResults/*/coverage.cobertura.xml" \
-targetdir:"tests/Micro.Tests/TestResults/CoverageReport" \
-reporttypes:Html \
-assemblyfilters:"-System*;-Microsoft*" \
-filefilters:"-*.generated.cs"

echo "Coverage report generated at: tests/Micro.Tests/TestResults/CoverageReport/index.html"
