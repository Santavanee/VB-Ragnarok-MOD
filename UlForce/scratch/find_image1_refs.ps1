$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

Write-Output "=== Methods referencing image1 ==="
foreach ($t in $asmCS.GetTypes()) {
    foreach ($m in $t.GetMethods([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static")) {
        try {
            $body = $m.GetMethodBody()
            if ($body) {
                # We check instructions using IL
            }
        } catch {}
    }
}

