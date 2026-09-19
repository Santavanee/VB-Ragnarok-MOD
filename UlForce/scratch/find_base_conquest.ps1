$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

Write-Output "=== Classes related to Base / Occupa / Domina / City / Conquer / Win / Result ==="
foreach ($t in $asmCS.GetTypes()) {
    if ($t.Name -match "Base|Occupa|Domina|Conquer|Win|Result|Capture|Invasion|Point") {
        Write-Output $t.FullName
    }
}

