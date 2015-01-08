$regex = New-Object System.Text.RegularExpressions.Regex('<MajorVersion>(\d\.\d)</MajorVersion>')
$m = $regex.Match((Get-Content Salesforce.props))
$majorMinor = $m.Groups[1].Value
$version = $majorMinor + '.' + $env:BuildNumber

Add-AppveyorMessage -Message "Updating version to $version" -Category Information

Update-AppveyorBuild -Version $version
