################################################################################
##  File:  Install-Wix.ps1
##  Team:  Automated Testing
##  Desc:  Install Latest Wix
################################################################################

Import-Module $PSScriptRoot\ImageHelpers\InstallHelpers.ps1

$temp_install_dir = 'C:\Windows\Installer'
New-Item -Path $temp_install_dir -ItemType Directory -Force

Install-MSI -MsiUrl "https://wixtoolset.org/downloads/v3.14.0.3910/wix314.exe" -MsiName "wix314.msi"
