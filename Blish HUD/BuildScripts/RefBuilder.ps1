param ([string]$project, [string]$output)

Write-Output "RefBuilder will be using the following directories:"
Write-Output "$($project)ref\*"
Write-Output "$($project)obj\ref.zip"
Write-Output "$($project)$($output)ref.dat"

Write-Output "Building ref.dat..."

Compress-Archive -Path "$($project)ref\*" -DestinationPath "$($project)obj\ref.zip" -Update
Copy-Item "$($project)obj\ref.zip" "$($project)$($output)ref.dat" -Force

Write-Output "$($output)ref.dat was built!"