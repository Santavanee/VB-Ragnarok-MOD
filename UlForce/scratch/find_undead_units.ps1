$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

function Read-BytesFile($fileName) {
    $path = Join-Path "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\StreamingAssets\data" $fileName
    $fs = [System.IO.File]::OpenRead($path)
    $gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Decompress)
    $bf = New-Object System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
    $data = $bf.Deserialize($gz)
    $gz.Dispose(); $fs.Dispose()
    return ,$data
}

$units = Read-BytesFile "UnitDataSet.bytes"
$races = @{
    [char]0x7537 = "Man";
    [char]0x5973 = "Woman";
    [char]0x4EBA = "Human";
    [char]0x9B54 = "Demon";
    [char]0x795E = "Divine";
    [char]0x7363 = "Beast";
    [char]0x6A39 = "Nature";
    [char]0x6D77 = "Aqua";
    [char]0x7ADC = "Dragon";
    [char]0x5668 = "Mechanical";
    [char]0x6B7B = "Undead";
    [char]0x87F2 = "Insect";
    [char]0x708E = "Fire";
    [char]0x6C37 = "Ice";
    [char]0x96F7 = "Lightning";
    [char]0x6BD2 = "Poison";
    [char]0x98DB = "Flying";
    [char]0x9A0E = "Knight";
    [char]0x591C = "Night";
    [char]0x8D85 = "Supreme";
    [char]0x5168 = "All";
}

function Decode-Str($s) {
    $out = ""
    if (-not $s) { return "" }
    foreach ($c in $s.ToCharArray()) {
        if ($races.ContainsKey($c)) {
            $out += $races[$c] + " "
        } else {
            $out += ("[" + [int]$c + "] ")
        }
    }
    return $out.Trim()
}

Write-Output "--- Units with Woman, Undead, Divine or Night ---"
foreach ($u in $units) {
    $dec = Decode-Str $u.tribe
    if ($dec -match "Woman" -and $dec -match "Undead" -and ($dec -match "Divine" -or $dec -match "Night")) {
        Write-Output ($u.name + " (" + $u.id + ") tribe: " + $dec + " special: " + (Decode-Str $u.special))
    }
}

