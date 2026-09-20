$code = @'
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zlib;

public class CustomBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (typeName.Contains("Assembly-CSharp"))
        {
            typeName = typeName.Replace("Assembly-CSharp", typeof(DataSet.ResearchMatrixSet).Assembly.FullName);
            return Type.GetType(typeName);
        }
        if (assemblyName.Contains("Assembly-CSharp"))
        {
            return typeof(DataSet.ResearchMatrixSet).Assembly.GetType(typeName);
        }
        return null;
    }
}

public static class Reader
{
    public static void Read()
    {
        try
        {
            using (var fs = File.OpenRead(@"E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\StreamingAssets\data\ResearchMatrixSet.bytes"))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                var bf = new BinaryFormatter();
                bf.Binder = new CustomBinder();
                var list = (System.Collections.IList)bf.Deserialize(gz);
                Console.WriteLine("Total: " + list.Count);
                foreach (var item in list)
                {
                    var t = item.GetType();
                    int page = (int)t.GetProperty("page").GetValue(item, null);
                    string id = (string)t.GetProperty("id").GetValue(item, null);
                    string title = (string)t.GetProperty("title").GetValue(item, null);
                    bool open = (bool)t.GetProperty("open").GetValue(item, null);
                    int[] opendata = (int[])t.GetProperty("opendata").GetValue(item, null);
                    Console.WriteLine(string.Format("ID={0}, Page={1}, Open={2}, Title={3}, OpenData=[{4}]", id, page, open, title, string.Join(",", opendata)));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Caught: " + ex.Message);
        }
    }
}
'@

Add-Type -TypeDefinition $code -ReferencedAssemblies @(
    'System.Runtime.Serialization',
    (Resolve-Path '.\bin\Release\Ionic.Zlib.dll').Path,
    'E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed\Assembly-CSharp.dll'
)

[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($s, $e)
    if ($e.Name -like 'Ionic.Zlib*') {
        return [System.Reflection.Assembly]::LoadFrom((Resolve-Path '.\bin\Release\Ionic.Zlib.dll').Path)
    }
    return $null
})

[Reader]::Read()
