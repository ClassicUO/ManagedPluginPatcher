using dnlib.DotNet;
using dnlib.DotNet.Writer;


foreach (var s in args)
{
    PatchDLL(s);
}


static void PatchDLL(string dllPath)
{
    Console.WriteLine("reading: " + dllPath);

    ModuleDef module = ModuleDefMD.Load(dllPath);

    var entry = module?.Find("Assistant.Engine", true)?.FindMethod("Install");

    if (entry != null && entry.IsStatic && entry.IsPublic)
    {
        entry.ExportInfo = new MethodExportInfo();
        entry.IsUnmanagedExport = true;

        ModuleWriterOptions opts = new ModuleWriterOptions(module);
        opts.PEHeadersOptions.Machine = dnlib.PE.Machine.AMD64;
        opts.Cor20HeaderOptions.Flags = 0;

        if ((opts.PEHeadersOptions.Characteristics & dnlib.PE.Characteristics.Dll) == 0)
        {
            Console.WriteLine("file converted to DLL");

            opts.PEHeadersOptions.Characteristics |= dnlib.PE.Characteristics.Dll;
        }

        Console.WriteLine("writing asm...");

        string path = Path.GetDirectoryName(dllPath);
        string name = Path.GetFileName(dllPath).Replace(".dll", ".FIXED.dll");

        module.Write(Path.Combine(path, name), opts);
        
        Console.WriteLine("done");
    }
    else
    {
        Console.WriteLine("impossible to find the 'Assitant.Engine.Install' method");
    }
}