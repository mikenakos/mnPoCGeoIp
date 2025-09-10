param(
  [string]$Server = "(localdb)\MSSQLLocalDB",
  [string]$Db = "mnakosPoCGeoIpDb",
  [string]$SchemaPath = ".\scripts\Schema.sql"
)

# Create DB if missing
sqlcmd -S $Server -E -Q "IF DB_ID('$Db') IS NULL CREATE DATABASE [$Db]"

# Apply schema (runs your file once; re-running will error if objects exist)
sqlcmd -S $Server -E -d $Db -i $SchemaPath

Write-Host "Database '$Db' is ready on '$Server'."
