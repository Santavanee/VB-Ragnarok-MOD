# UlForce — Catatan Modding Venus Blood Ragnarok International

Ringkasan proses & temuan teknis dari sesi modding ini, buat dipakai lanjut ke depannya.

## Tentang proyek

- **Target game**: VenusBlood RAGNAROK International (US), Unity 5.6.7, Mono lawas, dijalankan via **BepInEx 5.4.23.4**.
- **Proyek**: `UlForce.csproj` — class library .NET Framework 4.7.2, di-build jadi `UlForce.dll` dan di-drop ke folder `BepInEx/plugins/` game.
- Path game: `E:\GameH\VenusBlood RAGNAROK International (US)\`
- Path proyek: `c:\Users\cs_yo\source\repos\UlForce\UlForce\`

## File-file mod (kondisi saat ini)

- **[Class1.cs](Class1.cs)** — plugin `VBR Force Lock` (namespace `VBRForceLock`, GUID `vbr.force.lock`). Berisi Harmony patch: `MapForcePatch` (kunci force division ke 500 lewat toggle F1) dan `ItemLimitPatch` (naikkan limit item min 100).
- **[CustomUnit.cs](CustomUnit.cs)** — plugin `VBR Custom Unit` (GUID `vbr.force.customunit`). Nambahin **Faceless Reaper Lulu** (karakter crossover dari game lain di seri VBR) langsung aktif ke roster (barrack=0) begitu save aktif kedetect. Lengkap dengan stat, 9 skill nyata (lihat tabel di bawah), dan portrait custom di **SEMUA 5 titik render** yang ditemukan (lihat poin 7 — riwayat panjang trial-and-error-nya, tapi hasil akhirnya semua kebenerin).
- **[Assets/lulu.png](Assets/lulu.png)** — crop portrait Lulu (juga ada salinannya sebagai `UlForce_lulu.png` di folder `BepInEx/plugins/` game — **wajib ikut di-copy** tiap deploy, sama kayak DLL). Di-load runtime jadi `Sprite` custom lewat `LoadLuluSprite()`.

**Riwayat**: sempat ada juga unit goblin sederhana ("Custom Unit", trigger F2) sebagai unit custom pertama/percobaan. **Udah dihapus** (28/29 Aug) karena bikin bingung pas battle (unit lain yang emang sengaja goblin ketuker sama Lulu yang portraitnya belum kebenerin waktu itu) dan gak kepake lagi setelah Lulu selesai. Kalau mau nambah unit custom lain lagi, `AddCustomUnit`/`CreateCustomTemplate` di riwayat git/percakapan bisa dijadiin contoh pola-nya (clone `masterList[0]`, timpa `id/name/rank/cost/pay/basic`, `new UnitData(template)`, `barrack=0`).

## Cara build & deploy (wajib diulang tiap ubah kode)

### Title skill search (13 Sep 2026)

- `TitleSkillSearch.cs`: plugin `vbr.force.titlesearch`, search bar native Unity UI di kanan bawah panel medallion Title Settings. Ketik sebagian nama skill (case-insensitive), medallion yang tidak cocok diredupkan dan daftar title kiri/kanan difilter. Tombol X menghapus pencarian; pencarian direset ketika menu dibuka.
- Pencarian memakai `TitleDataSet.attach.id` → `ExValue.GetPassiveDataSet(id).name`, sama dengan `SingleTitleBlockHandler.DataDaraw`. `status` adalah empat modifier stat, bukan skill tambahan.
- Hasil dihitung dari medallion yang sudah unlocked (`forceData.IsMedals`). Jumlah stok/rank unit dan biaya tetap diperiksa oleh game; hasil pencarian bukan jaminan title bisa dipasang pada unit tersebut.
- Patch `MedallionControl.OnOpen` memasang UI sekali per instance. Prefix `TitleListBlockHandler.DrawData` memfilter salinan list tanpa mengubah database atau index title. Hover/pemilihan medallion tetap memakai alur asli game.
- `Class1.cs` sekarang patch hanya `MapForcePatch` dan `ItemLimitPatch`, masing-masing dengan try/catch, agar tidak ikut memasang patch plugin lain dua kali.
- Build Debug berhasil. Posisi/ukuran search bar dan interaksi input masih perlu diverifikasi langsung di game; belum ada verifikasi visual runtime.

```powershell
# build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" `
  "c:\Users\cs_yo\source\repos\UlForce\UlForce\UlForce.csproj" /p:Configuration=Debug /nologo /v:minimal

# copy DLL ke game (WAJIB tiap kali, gak auto — ini penyebab bug "gak ada perubahan" beberapa kali)
cp "c:/Users/cs_yo/source/repos/UlForce/UlForce/bin/Debug/UlForce.dll" `
   "E:/GameH/VenusBlood RAGNAROK International (US)/BepInEx/plugins/UlForce.dll"
```

Cek hasil jalan lewat log:
```
E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\LogOutput.log
```
Klaude bisa baca file ini langsung — gak perlu selalu minta user paste log manual.

## Cara decompile game (kalau perlu investigasi struktur data lagi)

```bash
dotnet tool install -g ilspycmd
ASM="E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/Managed/Assembly-CSharp.dll"
ilspycmd -p -o /path/to/output "$ASM"   # decompile penuh jadi project .cs
ilspycmd -t "Namespace.TypeName" "$ASM" # decompile 1 type doang
```
Reflection cepat (tanpa decompile) bisa juga pakai `powershell.exe` (bukan `pwsh`) karena `ReflectionOnlyLoadFrom` cuma jalan di .NET Framework klasik.

## Temuan teknis penting (gotcha dari eksperimen "custom unit")

1. **`GameDatas.Instance.userData`** adalah pintu masuk ke semua data save aktif: `.UnitData.player` (roster unit), `.UnitDataSet` (master template unit), `.ResearchMatrixData` (data Research), `.forceData`, dll. Objeknya **bisa berubah reference** (title screen → continue/load save) — jangan pakai flag `bool` sekali-jalan buat nge-gate inisialisasi, tapi bandingkan **reference object**-nya.

2. **Bikin unit custom** = clone `UnitDataSet` yang udah ada (`(UnitDataSet)template.Clone()`), timpa `id/name/rank/cost/pay/basic`, baru `new UnitData(clonedTemplate)`. `UnitDataSet.Clone()` itu **deep copy yang benar** (list-nya di-clone beneran).

3. **`ResearchMatrixSet.Clone()` itu SHALLOW (`MemberwiseClone`)** — array field-nya (`opendata`, `openimage`, `medalions`, `medalioncounts`, `function`, `infotext`) **TETAP nunjuk ke array yang sama** kayak sumber clone-nya. Kalau isi array itu diubah tanpa di-deep-copy dulu, bakal ikut ngerusak node vanilla yang jadi basis clone. **Ini yang bikin Research blank/rusak** waktu sempat dicoba nyelipin unit custom biar bisa direkrut lewat tab Research. Kesimpulan: **jangan coba nyelipin node custom ke Research** kecuali siap pakai reflection buat ganti reference field array-nya (`Traverse.Field("_opendata").SetValue(newArray)` dst) — dan bahkan itu pun integrasi Research auto-recruit akhirnya ditinggalkan karena terlalu berisiko/rumit. Kalau mau unit custom langsung kepake, taruh langsung ke roster (`barrack=0`), jangan lewat Research.

4. **Formula HP**: `hp.max = ((level-1)*0.25+1) * unitDatas.basic.hp` (dihitung ulang tiap akses selama `battleHP == -1`). Buat target HP tertentu di level tertentu, hitung mundur `basic.hp` dari situ.

5. **Stat POW/DEF/SPD/WIS** (`unit.status.pow/def/spd/wis`) itu di-generate lewat sistem skill (`SetStatusAll` → `skills.skills.StatusExecCalc`) yang formulanya gak straightforward ditiru. Override manual (`unit.status.pow = X`) sempat kelihatan nempel di beberapa layar, tapi di layar battle/detail lain angkanya balik lagi disertai modifier aneh (`ATK[14 (-27)]` dst) — jadi kemungkinan besar ADA proses lain yang recalculate status ini pas render (belum sempat ditelusuri sampai tuntas, keburu fitur unit custom-nya dicabut). Kalau nanti mau coba lagi, ini PR yang belum selesai.

6. **PENTING — nama skill yang ditampilkan BUKAN dari `SkillData.name`.** Tiap skill diimplementasi sebagai class `Skill_XXXX` (ratusan, di namespace `SkillConfig`) dengan `id` unik & efek battle hardcoded. Tapi **nama yang ditampilin di UI juga BUKAN dari `Skill_XXXX.setInfo()`** (itu isinya bahasa Jepang mentah) — UI (`UnitInfoHandler.GetDrawSkill`) manggil `ExValue.GetPassiveDataSet(sk.id).name`, yang baca dari **`PassiveDataSet`, database terpisah** berisi id + nama-yang-udah-dilokalisasi (Inggris di build ini). Ini sumber kebenaran buat nama skill, bukan grep teks ke file game (sempat salah — lihat di bawah) dan bukan field `.name` di `SkillData` (rename manual situ SIA-SIA, gak kebaca UI).
   - **Cara cepat cari id skill by nama** (gak perlu screenshot-screenshot unit): panggil `BinaryLoad.ReadDataSet<PassiveDataSet>()` (public, aman dipanggil langsung — beda dari cerita generic-method-patch yang bahaya di poin 7) dari diagnostic log sekali jalan, dump semua `id`+`name`-nya (~300an entri), terus grep log-nya sendiri buat nama yang dicari.
   - Buat `power`/level skill: construct `SkillData` baru (`id = "L014"; name = HDDataSetDef.NULL; power = X;`) — `name` WAJIB di-set ke `HDDataSetDef.NULL` biar resolve lewat id, bukan string kosong C# (`""`) yang beda makna.
   - **Jangan percaya hasil grep teks mentah** ke file game buat mastiin sebuah skill "gak ada" — nama skill ke-compress di asset bundle (`.unity3d`), sempat grep kasih hasil nihil padahal skillnya ADA (baru ketauan pas user nemu 2 unit di roster-nya yang punya skill itu). Selalu verifikasi lewat `PassiveDataSet` runtime, bukan grep file mentah.
   - Kapasitas slot skill di template yang dipakai (basis clone unit "goblin"): **`skillBase` 8 slot**, `leader` 2 slot, `trick` 1 slot — konfirmasi dari observasi UI (bukan dari baca source langsung), jadi kalau ganti basis clone ke unit lain, jumlah slotnya bisa beda.

7. **Portrait/sprite unit dipanggil dari 5 JALUR BERBEDA tergantung layar** — semuanya akhirnya kebenerin, tapi butuh nemuin & patch tiap jalur satu-satu. Dugaan awal "animasi-driven" (Animator/AnimationClip) di sesi sebelumnya **TERBUKTI SALAH** — semua ternyata cuma soal nemuin CONSUMER METHOD yang bener buat tiap layar. `UnitBlockHandler` vs `SingleUnitBlockHandler` itu **DUA CLASS BEDA** (nama mirip, gampang ketuker) yang render 2 UI berbeda.

   | # | Layar | image1 index | Method pemanggil | Cara patch |
   |---|---|---|---|---|
   | 1 | Grid Research & sebagian icon battle | `[4]` | `BinaryLoad.LoadSpriteMultiple("ics", key)` | Prefix LANGSUNG di method ini (non-generic, aman) — cek `fn == key`, isi `__result`, `return false`. |
   | 2 | Portrait besar kartu detail ("Assigned Units") | `[2]` (tapi kita gak isi field ini sama sekali) | `UnitInfoHandler.DrawData(UnitData,UnitData[])` manggil `BinaryLoad.Load<Sprite>` (**generik**) | **JANGAN patch `Load<T>` langsung** (lihat poin di bawah). Postfix di `DrawData`, timpa field `_unitImage` (`Image`) pakai `Traverse`. |
   | 3 | Icon kecil grid army/roster biasa | `[2]` juga (tapi lagi-lagi gak perlu diisi) | `SingleUnitBlockHandler.DataDaraw()` manggil `Load<T>` | Postfix di `DataDaraw`, timpa field `_Image`. |
   | 4 | Kotak Division Info (yang ada tombol eject/X) | `[1]` | **`UnitBlockHandler.DataDaraw()`** (BEDA class dari #3!) manggil `Load<T>` pakai `data.image1[1]` | Postfix di `DataDaraw`-nya `UnitBlockHandler`, timpa field `_Background` (nama field-nya "Background" tapi ini portrait karakter, bukan BG). **JANGAN isi `image1[1]` dengan key custom** — lihat poin battle HUD di bawah, field ini dipakai bareng sama sistem lain yang bakal rusak. |
   | 5 | Kotak status HUD pas battle | `[1]` (baca lewat `unitDatas.image1[1]`, bukan dari instance) | `DivisionData.TagMake()` encode ke string tag (`BattleSystem.MakeCode`), didecode & dirender di `StatusControl.SetStatus()` lewat `Resources.Load<Sprite>(make.image)` (Unity generik native, BUKAN `BinaryLoad`) | Postfix di `StatusControl.SetStatus`, timpa `GameObject image` (public field) → `.GetComponent<Image>()`. |

   **Kenapa jalur #4 dan #5 SALING KONFLIK soal `image1[1]`**: #4 (`UnitBlockHandler`) baca lewat `data.image1[1]` LANGSUNG sebagai lookup key di `Load<T>`. #5 nge-generate STRING TAG dari `unitDatas.image1[1]` (`makeCode.image = unitDatas.image1[1];` di `DivisionData.TagMake()`), dan tag itu di-`Resources.Load<Sprite>` di tempat lain TANPA lewat method yang bisa gampang di-patch. **Kalau `image1[1]` diisi key custom yang gak match aset asli apa pun, jalur #5 gagal load dan HASILNYA BLANK PUTIH** (bukan fallback ke gambar lama). Solusi: **jangan pernah isi `image1[1]`** — biarkan tetap nilai asli (goblin/basis clone), dan andalkan PATCH METHOD KONSUMEN (poin #4 di tabel) buat benerin tampilannya, yang gak bergantung ke isi `image1[1]` sama sekali.

   **Bug ganas: `Image.overrideSprite` bikin SEMUA unit ketimpa jadi Lulu.** Awalnya tiap fix nimpa `img.sprite` **DAN** `img.overrideSprite` (dikira perlu karena `overrideSprite` prioritas render-nya di atas `sprite`). Ternyata itu nyebabin bug: begitu mouse diarahin ke icon Lulu (hover), SEMUA portrait unit lain ikut ketimpa jadi Lulu juga. Dugaan kuat `overrideSprite` dipakai INTERNAL sama sistem hover/selection game ini (biasa dipakai buat efek highlight sementara), dan nilai yang kita paksa masuk situ "bocor"/ke-cache-salah ke elemen lain pas sistem hover jalan. **Fix: cukup timpa `.sprite` doang, JANGAN sentuh `overrideSprite` sama sekali.** Semua 5 titik di atas tetap kebenerin cuma pakai `.sprite`.

   **`BinaryLoad.Load<Sprite>` itu method GENERIK — JANGAN di-patch langsung.** Udah kejadian 2x nyoba: Harmony patch ke method generik (`Load<T>`, ataupun native `Resources.Load<T>`) bikin `Awake()` gagal total (exception di `PatchAll`), dan itu ngediemin SEMUA fitur mod (Update() gak pernah jalan lagi) TANPA error yang jelas sampai game di-restart. **Selalu patch method KONSUMEN-nya** (method yang MEMANGGIL `Load<T>`, misal `DrawData`/`DataDaraw`/`SetStatus`), pakai Postfix yang nimpa field `Image` targetnya langsung — bukan intercept si `Load<T>` itu sendiri. Metode non-generic (`LoadSpriteMultiple`, `LoadSprite`) AMAN dipatch langsung.

   **`SetNativeSize()`/ukuran kotak bisa ke-resize ngikutin ukuran PNG kita.** Portrait besar (jalur #2) sempat keliatan ngerecil jadi kotak kecil di pojok — dugaan ada kode yang manggil semacam auto-resize berdasar ukuran native sprite abis di-assign. Fix: di helper `ForceApplySprite`, SIMPEN `img.rectTransform.sizeDelta` SEBELUM ganti sprite, lalu PAKSA BALIKIN lagi abis assign — biar ukuran kotak konsisten walau PNG kita (183×195px) jauh lebih kecil dari portrait asli.

   **Sistem tag battle (`BattleSystem.MakeCode`) MOTONG nama unit di SPASI.** `MakeCode.Encode()`/`Decode()` nge-encode/decode field-field (termasuk `name`) jadi satu string tag yang di-split, dan proses tokenizing-nya PECAH di karakter spasi. Unit dengan nama 1 kata (`"Jack-o-Lantern"`) aman, tapi `"Faceless Reaper Lulu"` kepotong jadi cuma **`"Faceless"`** doang pas nyampe di `StatusControl.SetStatus()`. Kalau mau identifikasi unit lewat `make.name` di jalur #5, **bandingin ke KATA PERTAMA doang**, bukan nama lengkap. (Berlaku juga kemungkinan buat sistem tag lain yang sejenis di codebase ini — AVG cutin tags dll kemungkinan punya masalah sama.)

   **Cara diagnostik yang kepake buat nemuin semua ini**: tambahin Postfix logging SEDERHANA (bukan yang di-gate "cuma log sekali" — itu jebakan, bikin gak keliatan panggilan berikutnya yang justru relevan) di method yang dicurigai, log `unit.unique`/`make.name`/dll buat SETIAP panggilan, minta user buka layar yang bermasalah, terus baca `LogOutput.log` buat lihat pola manggil-nya. Diagnostik `LogImageContext` (path hierarki GameObject + `activeInHierarchy`) juga kepake buat mastiin Image yang dipatch itu beneran instance yang aktif/kelihatan.

8. **`new Harmony(...).PatchAll()` tanpa argumen scan SELURUH assembly** — kalau ada banyak `[HarmonyPatch]` class, satu class gagal (misal karena target generic di poin 7) bisa bikin exception nyebar ke SEMUA fitur lain di plugin yang sama. **Selalu**: (a) bungkus tiap `PatchAll` dengan try/catch sendiri-sendiri, (b) panggil dengan target type spesifik (`PatchAll(typeof(SpecificPatchClass))`), jangan overload kosong yang scan semua.

9. **`System.Linq` berbahaya buat dipakai di mod ini.** Proyek reference `System.Core` tanpa `HintPath`, jadi resolve ke `System.Core.dll` dari .NET SDK dev machine (bukan yang dibundle Mono runtime game). Method LINQ apa pun (`.Max()`, dll) bikin `ReflectionTypeLoadException` pas Harmony `PatchAll()` scan assembly, dan itu bisa bikin game hang/blank. **Solusi**: jangan pakai LINQ sama sekali, ganti loop manual. `List<T>.Find(Predicate<T>)` AMAN dipakai (itu method `mscorlib`, bukan LINQ/System.Core).

10. **Reflection buat private-set property**: banyak kelas data (`ResearchMatrixSet`, dll) punya property `{ get; private set; }`. Set dari luar assembly pakai `HarmonyLib.Traverse`: `Traverse.Create(obj).Property("name").SetValue(val)`. Buat FIELD private murni (bukan property, kayak `_opendata` yang expose lewat `=> _opendata` read-only), pakai `Traverse.Create(obj).Field("_namaField").SetValue(val)`.

11. **Race (`tribe`) & Slay (`special`) unit** — 2 field string terpisah di `UnitDataSet`, tiap KARAKTER dalam string-nya jadi 1 ikon:
    - `UnitDataSet.tribe` → baris **"Race"** di kartu detail unit. Dibaca `UnitInfoHandler` (field `_tribes`, ~baris 340): tiap `tribe[i]` (1 char) dijadiin key `BinaryLoad.LoadSpriteMultiple("race_icon_48x5_" + TextResource.mode, tribe[i].ToString())`.
    - `UnitDataSet.special` → baris **"Slay"** di kartu detail unit (race yang DIUNTUNGKAN diserang unit ini). Mekanisme identik, field `_specials` (~baris 354), atlas sprite sama (`race_icon_48x5_*`).
    - Kedua field divalidasi lewat `NTCheckStr(HDDataSetDef.STRTRIBE)` — jadi karakternya HARUS salah satu dari 21 karakter di bawah (bukan bebas teks apa pun).
    - **Cara nemuin karakter yang bener**: jangan coba-coba nebak dari `STRTRIBE`/`STRTRIBEALL` di source — nama constant-nya Jepang dan ke-garbled kalau di-print ke console non-UTF8 (encoding trap yang sama kayak di poin 6). Cara yang kepake & reliabel: **decode `UnitDataSet.bytes` via `BinaryFormatter` (pola sama kayak `PassiveDataSet` di poin 6/lampiran)**, cari unit ASLI yang race-nya udah dikenal dari screenshot in-game (mis. "Luna Fairy" `m0573`), baca `.tribe`/`.special`-nya, salin PERSIS karakternya (bukan tebak index).
    - **Tabel lengkap 21 karakter** (urutan = urutan `K001`–`K021` "Slay X" di lampiran skill, dikonfirmasi lewat scan seluruh 880 unit di `UnitDataSet.bytes` buat mastiin gak ada karakter ke-13 yang kelewat):

      | Race | Char | Code | Race | Char | Code |
      |---|---|---|---|---|---|
      | Man | 男 | U+7537 | Mechanical | 器 | U+5668 |
      | Woman | 女 | U+5973 | Undead | 死 | U+6B7B |
      | Human | 人 | U+4EBA | Insect | 虫 | U+87F2 |
      | Demon | 魔 | U+9B54 | Fire | 炎 | U+708E |
      | Divine | 神 | U+795E | Ice | 氷 | U+6C37 |
      | Beast | 獣 | U+7363 | Lightning | 雷 | U+96F7 |
      | Nature | 樹 | U+6A39 | Poison | 毒 | U+6BD2 |
      | Aqua | 海 | U+6D77 | Flying | 飛 | U+98DB |
      | Dragon | 竜 | U+7ADC | Knight | 騎 | U+9A0E |
      |  |  |  | Night | 夜 | U+591C |
      |  |  |  | Supreme | 超 | U+8D85 |
      |  |  |  | All | 全 | U+5168 |

      Contoh pakai: `custom.tribe = "女魔飛樹夜氷";` (Female, Demon, Fly, Nature, Night, Ice).
    - **Encoding source file**: `CustomUnit.cs` gak punya BOM tapi tetap ke-compile bener sebagai UTF-8 (diverifikasi: build sukses + decompile ulang DLL hasil build nunjukin karakternya utuh, gak mangled). Jadi karakter Kanji literal AMAN ditulis langsung di `.cs`, gak perlu `\uXXXX` escape.

## Status Lulu sekarang (semua fitur AKTIF & konfirmasi jalan)

- ✅ Otomatis masuk roster (barrack=0) begitu save aktif kedetect. Stat: Lv.106, POW 199, DEF 93, SPD 172, WIS 42, HP dikalibrasi ~2861 lewat `basic.hp=109`. Rank 17, cost 12.
- ✅ **9/9 skill nyata terpasang & bener** (tabel di bawah) — id dari `PassiveDataSet`, power disamain ke kartu aslinya.
- ✅ **Portrait custom di SEMUA 5 titik render** (grid Research, kartu detail "Assigned Units", icon grid army, kotak Division Info, kotak status battle HUD) — lihat poin 7 di atas buat detail tiap titik & cara patch-nya.
- ✅ **Race & Slay disamain ke "Luna Fairy" (`m0573`)** — lihat poin 11. Race: Female, Demon, Fly, Nature, Night, Ice (`tribe`). Slay: Fire, Ice, Lightning (`special`).
- ❌ Equipment (Demonic Blade of Stars, Grisani Dress) — sengaja di-skip atas permintaan user. Skill turunannya (Sap Speed `N033`, Strat Hinder `O001`) udah ketemu id-nya tapi belum dipasang (skillBase/leader/trick semua udah penuh, 11 slot terisi).
- ❌ Recruit lewat tab Research — dicoba, gagal (lihat poin 3), ditinggalkan demi stabilitas. Lulu masuk langsung ke roster tanpa perlu di-research.

Tabel skill lengkap:

| Slot | Nama | id | power |
|---|---|---|---|
| skillBase[0] | Lightning Boost | `L014` | 40 |
| skillBase[1] | Lightning Field | `B011` | 20 |
| skillBase[2] | Flank Attack | `I005` | 20 |
| skillBase[3] | Cross Attack | `I003` | 0 |
| skillBase[4] | Dimension Slash | `I010` | 50 |
| skillBase[5] | Lethal Critical | `I011` | 50 |
| skillBase[6] | Max-Power Attack (nama di game ini: "Full Power Attack") | `I015` | 50 |
| skillBase[7] | Range Null | `J002` | 0 |
| leader[0] | Dimension Slash (tier leader) | `I010` | 25 |
| leader[1] | Squad Boost (tier leader) | `L030` | 50 |
| trick[0] | Squad Boost (tier trick) | `L030` | 15 |

**Catatan**: save yang sempat kepasang mod versi LAMA (waktu masih ada `zzz_custom_001` si goblin) — unit goblin itu KEMUNGKINAN masih nyangkut di save sebagai entri roster biasa (data-nya legit, cuma gak ada dukungan kode lagi karena `AddCustomUnit`/`CreateCustomTemplate` udah dihapus) — harmless, gak perlu dibersihin kecuali user minta.

## Lampiran: daftar LENGKAP semua skill di game (id → nama Inggris)

309 entri, sumbernya `PassiveDataSet` (database nama skill resmi, lihat poin 6 di atas). Ini SEMUA skill yang ada di game, bukan cuma punya Lulu — kepake buat referensi cepat kalau nanti butuh nyari id skill lain (buat unit custom, atau modding lain yang nyangkut skill).

**Cara generate ulang** (gak perlu jalanin game sama sekali — baca file data-nya langsung pakai PowerShell .NET Framework, `BinaryFormatter` + `GZipStream`):

```powershell
$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmUE = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "UnityEngine.dll"))
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))

# BinaryFormatter perlu resolve assembly by-name pas deserialize — LoadFrom
# gak otomatis kedaftar buat itu, jadi di-hook manual lewat AssemblyResolve.
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    if ($name -eq "UnityEngine") { return $asmUE }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

# TextResource._mode default-nya "jp" (index bahasa Jepang) kalau game gak
# dijalanin — paksa ke "us" biar .name balikin teks Inggris.
$trType = $asmCS.GetType("CoreSystem.TextResource")
$modeField = $trType.GetField("_mode", [System.Reflection.BindingFlags]"NonPublic,Static")
$modeField.SetValue($null, "us")

$path = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\StreamingAssets\data\PassiveDataSet.bytes"
$fs = [System.IO.File]::OpenRead($path)
$gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Decompress)
$bf = New-Object System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
$list = $bf.Deserialize($gz)
$gz.Dispose(); $fs.Dispose()

foreach ($item in $list) { Write-Output ($item.id + "|" + $item.name) }
```

Jalankan lewat `powershell.exe` (bukan `pwsh`) buat konsistensi, walau `Assembly.LoadFrom` biasa (bukan `ReflectionOnlyLoadFrom`) kemungkinan jalan juga di `pwsh`. Pola yang sama (baca `.bytes` file langsung tanpa jalanin game) reusable buat `.bytes` data lain di `StreamingAssets/data/` (`ItemDataSet.bytes`, `UnitDataSet.bytes`, dll) — tinggal ganti nama file & tipe generic-nya.

**Daftar lengkap** (format `id` — nama):

```
A001 Flame Cannonball      A002 Water Cannonball      A003 Ice Cannonball
A004 Lightning Cannonball  A005 Poison Cannonball     A006 Divine Cannonball
A007 Evil Cannonball       A008 Acid Cannonball       B001 Fire Blast
B002 Water Blast           B003 Ice Blast             B004 Lightning Blast
B005 Poison Blast          B006 Light Blast           B007 Dark Blast
B008 Fire Field            B009 Water Field           B010 Ice Field
B011 Lightning Field       B012 Poison Field          B013 Light Field
B014 Dark Field            B015 Acid Blast            B016 Acid Field
C001 Amplification         D001 Wall Breaker          D002 Wall Builder
D003 Barrier               D004 Pump Up               E001 Spell Wall
E002 Spell Barrier         E003 Spell Reflect         E004 Spell Absorb
E005 Bombardment Wall      E006 Bombardment Barrier   F001 Self Heal
F002 Target Heal           F003 Division Heal         F004 Equitable Heal
F005 Demon Medic           F006 Day Regen             F007 Night Regen
G001 Poison Cure           G002 Curse Cure            G003 Stun Cure
G004 Debuff Cure           G005 Absolute Cure         G006 Resist Ailments
H001 Fool's Lie            H002 Rainbow Venom         H003 Poison Attack
H004 Stun Attack           H005 Cursed Strike         H006 Charm Attack
H007 Seal Attack           H008 Cancelling Attack     H009 Attack Debuff
H010 Defense Debuff        H011 Speed Debuff          H012 Wisdom Debuff
H013 Vampiric Attack       H014 Multi-Ailment         H015 Counter Ailment
H016 Forced Ailment        I001 Pierce Attack         I002 Wide Attack
I003 Cross Attack          I004 All Attack            I019 Group Attack
I005 Flank Attack          I006 Range Attack          I007 Added Attack
I008 Critical Boost        I009 Counter Amp           I010 Dimension Slash
I011 Lethal Critical       I012 Helmet Split          I013 Self-Destruct
I014 Mental Attack         I015 Full Power Attack     I017 Multi-Attack
I018 Heart Pierce          I016 Counter Resist        J001 Flank Null
J002 Range Null            J003 Pierce Null           J004 Wide Null
J005 Cross Null            J006 All Null              J007 Surround Null
J008 Slayer Defense        J009 Parry                 J010 Evade
J011 Defense Only          J012 Forward Guard         J013 Target Miss
J014 Hardy Physique        J015 Godly Physique        J016 Tiny Physique
J017 Dragon Scales         J018 Recovery              J019 Troop Carrier
J020 Terrain Null          J021 Deadly Resist         J022 Fatal Resist
J023 S-Destruct Wall       J024 Self-D Barrier        J025 Ethereal Body
J026 Mental Block          K001 Slay Man              K002 Slay Woman
K003 Slay Human            K004 Slay Demon            K005 Slay Divine
K006 Slay Beast            K007 Slay Nature           K008 Slay Aqua
K009 Slay Dragon           K010 Slay Mechanical       K011 Slay Undead
K012 Slay Insect           K013 Slay Fire             K014 Slay Ice
K015 Slay Lightning        K016 Slay Poison           K017 Slay Flying
K018 Slay Knight           K019 Slay Night            K020 Slay Supreme
K021 Slay All              L001 Man Boost             L002 Woman Boost
L003 Human Boost           L004 Demon Boost           L005 Divine Boost
L006 Beast Boost           L007 Nature Boost          L008 Aqua Boost
L009 Dragon Boost          L010 Mechanical Boost      L011 Undead Boost
L012 Insect Boost          L013 Fire Boost            L014 Lightning Boost
L015 Ice Boost             L016 Poison Boost          L017 Flying Boost
L018 Knight Boost          L019 Night Boost           L020 Supreme Boost
L030 Squad Boost           L031 Attack Formation      L032 Defense Formation
L033 Speed Formation       L034 Wisdom Formation      L040 Bolster Fire
L041 Bolster Water         L042 Bolster Wind          L043 Bolster Earth
L044 Bolster Light         L045 Bolster Dark          M001 Command Man
M002 Command Woman         M003 Command Human         M004 Command Demon
M005 Command Divine        M006 Command Beast         M007 Command Nature
M008 Command Aqua          M009 Command Dragon        M010 Command Mech
M011 Command Undead        M012 Command Insect        M013 Command Fire
M014 Command Lightning     M015 Command Ice           M016 Command Poison
M017 Command Flying        M018 Command Knight        M019 Command Night
M020 Command Supreme       M030 Command Division      M031 Attack Command
M032 Defense Command       M033 Speed Command         M034 Wisdom Command
M040 Fire Command          M041 Water Command         M042 Wind Command
M043 Earth Command         M044 Light Command         M045 Dark Command
M050 We're Cornered!       M051 Revenge Fang          M052 Wild Fang
M060 Sun Worship           M061 Nocturnal             M062 Night-Attuned
M063 Day-Attuned           N001 Sap Man               N002 Sap Woman
N003 Sap Human             N004 Sap Demon             N005 Sap Divine
N006 Sap Beast             N007 Sap Nature            N008 Sap Aqua
N009 Sap Dragon            N010 Sap Mechanical        N011 Sap Undead
N012 Sap Insect            N013 Sap Fire              N014 Sap Lightning
N015 Sap Ice               N016 Sap Poison            N017 Sap Flying
N018 Sap Knight            N019 Sap Night             N020 Sap Supreme
N030 Sap Squad             N031 Sap Attack            N032 Sap Defense
N033 Sap Speed             N034 Sap Wisdom            N040 Weaken Fire
N041 Weaken Water          N042 Weaken Wind           N043 Weaken Earth
N044 Weaken Light          N045 Weaken Dark           O001 Strat Hinder
O002 Strat Support         O003 Strat Wall            O004 Strat Barrier
P001 Action Boost          P002 Action Block          P010 Taunt
Q001 Ambush Tactics        Q002 Ambush Alert          R001 Elite
R002 Slacker               R003 Treasure Hunt         R004 Bounty Hunter
R005 Replenish Res.        R006 Dauntless             R007 Attack Point
R008 Defense Point         R009 Speed Point           R010 Wisdom Point
T001 OpnSalvo(S)           T002 OpMiasma(S)           T003 SrcBlast(E)
T004 PsnBlast(E)           T005 SrcField(E)           T006 PsnField(E)
T007 BombBar(D)            T008 SpellBar(D)           T009 DivHeal(H)
T010 DemonMed(H)           T011 EqHeal(H)             T012 SlayDef(C)
T013 Parry(C)              T014 Evade(C)              T015 Hardy(C)
T016 DrgScale(C)           T017 Recovery(C)           T018 TrpCarry(C)
T019 TrnNull(C)            T020 DeadlyRs(C)           T021 FatalRs(C)
T022 SDstWall(C)           T023 SquadBst(B)           T024 AtkForm(B)
T025 DefForm(B)            T026 SpdForm(B)            T027 WisForm(B)
T028 CmndDiv(B)            T029 AtkCmnd(B)            T030 DefCmnd(B)
T031 SpdCmnd(B)            T032 WisCmnd(B)            T033 SapSquad(J)
T034 SapAtk(J)             T035 SapDef(J)             T036 SapSpd(J)
T037 SapWis(J)             T038 TreaHunt(R)           T039 Bounty(R)
T040 WallBrk(F)            T041 WallBld(F)            T042 Barrier(F)
T043 PumpUp(F)             T044 PsnCure(P)            T045 CursCure(P)
T046 StunCure(P)           T047 DebfCure(P)           T048 AbsoCure(P)
T049 FlankAtk(A)           T050 RangeAtk(A)           T051 AddedAtk(A)
T052 CritUp(A)             T053 DmnSlash(A)           T054 LthlCrit(A)
T055 HelmSplt(A)           T056 Self-Dst(A)           T057 F-PwrAtk(A)
T058 CountRes(A)           T059 HeartPrc(A)           B999 x
```

Seri `T001`–`T059` itu skill dengan nama SINGKATAN/kode (mis. `T024 AtkForm(B)`, `T053 DmnSlash(A)`, huruf dalam kurung nunjuk kategori aslinya) — kemungkinan varian "compact label" buat tampilan UI yang kepepet ruang (battle log dll), bukan skill efek terpisah. `B999 x` kelihatannya entri placeholder/gak kepake.
