$pub = ".\bin\Release\net8.0-windows\win-x64\publish"
$ver = "1.0"
$zip = "havCurCtr-$ver-win-x64.zip"

# Remove .pdb and .xml files in the publish folder
Get-ChildItem "$pub\*.pdb","$pub\*.xml" -File | Remove-Item -Force -ErrorAction SilentlyContinue

# Copy README.txt
Copy-Item .\README.txt "$pub\README.txt" -Force

# Copy LICENSE
Copy-Item .\LICENSE "$pub\LICENSE.txt" -Force

Compress-Archive -Path "$pub\*" -DestinationPath $zip -Force
Write-Host "Done! Created $zip."