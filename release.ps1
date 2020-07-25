if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$patreonFile = $dir + "\patreon.txt"

function CreateZip ($subfolder)
{
    $name = $subfolder.Name
    $ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($subfolder.GetFiles("*.exe").Fullname).FileVersion.ToString()
    
    $launcherDir = $subfolder.Fullname + "\UserData\LauncherEN"
    New-Item -ItemType Directory -Force -Path ($launcherDir)

    foreach($langDir in Get-ChildItem -Path $subfolder -Directory -Force -Exclude UserData)
    {
        #$langDir.MoveTo($launcherDir + "\" + $langDir.Name)
        Move-Item -Path $langDir -Destination ($launcherDir + "\" + $langDir.Name) -Force
    }

    Copy-Item -Path $patreonFile -Destination $launcherDir -Force -ErrorAction Ignore

    Compress-Archive -Path ($subfolder.FullName + "\*") -Force -CompressionLevel "Optimal" -DestinationPath ($subfolder.Parent.FullName + "\IllusionLaunchers_" + $name + "_" + $ver + ".zip")
}

$subfolders = Get-ChildItem -Path $dir -Directory -Force -Exclude out
foreach ($subfolder in $subfolders) 
{
    try
    {
        CreateZip ($subfolder)
    }
    catch 
    {
        # retry
        CreateZip ($subfolder)
    }
}
