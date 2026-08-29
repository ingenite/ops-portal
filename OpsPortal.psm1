#region InitialSetup
function New-EnvFile {
    param([switch]$Overwrite=$false)
    $targetPath = Join-Path $PSScriptRoot "infra\.env"

    if ((Test-Path $targetPath) -and ! $Overwrite) {
        Write-Output "File exists. Use -Overwrite param to create a new one."
        return
    } elseif ((Test-Path $targetPath) -and $Overwrite) {
        Move-Item $targetPath "$targetPath.bak" -Force -Confirm:$false -Verbose
    }

    New-Item -Path $targetPath -ItemType File -Force
    $saPassword = Read-Host -AsSecureString
    Set-Content -Path $targetPath -Value "MSSQL_SA_PASSWORD=$($saPassword | ConvertFrom-SecureString -AsPlainText)"
}

function Set-dotNetUserSecret {
    param(
        [string]$Server = "localhost",
        [String]$Database = "OpsPortal",
        [string]$User = "sa"
    )
    $saPassword = Read-Host -AsSecureString
    $connStr = "Server=$Server;Database=$Database;User Id=$User;Password=$(ConvertFrom-SecureString -Value $saPassword -AsPlainText);TrustServerCertificate=True"
    & dotnet user-secrets set "ConnectionStrings:OpsPortalDatabase" $connStr --project .\src\OpsPortal.Api
}

function Start-SqlContainer {
    $envFile = ".\infra\.env"
    $composeYaml = ".\infra\compose.yaml"
    & docker compose --env-file $envFile -f $composeYaml up -d
    & docker compose --env-file $envFile -f $composeYaml ps
}

function Invoke-EFDatabaseUpdate {
    & dotnet ef database update --project .\src\OpsPortal.Api --startup-project .\src\OpsPortal.Api
}
#endregion

#region ApiTests

#endregion

#region dotNetBuildTest

#endregion