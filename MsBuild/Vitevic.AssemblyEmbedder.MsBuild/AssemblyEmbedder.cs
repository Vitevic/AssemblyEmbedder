using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using Mono.Cecil.Pdb;
using System.Reflection;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
 //private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args) {
 //       var executingAssembly = Assembly.GetExecutingAssembly();
 //       var assemblyName = new AssemblyName(args.Name);
 
 //       var path = assemblyName.Name + ".dll";
 //       if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false) {
 //           path = String.Format(@"{0}\Vitevic.EmbeddedAssembly.{1}", assemblyName.CultureInfo, path);
 //       }
 
 //       using (var stream = executingAssembly.GetManifestResourceStream(path)) {
 //           if (stream == null)
 //               return null;
 
 //           var assemblyRawBytes = new byte[stream.Length];
 //           stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
 //           return Assembly.Load(assemblyRawBytes);
 //       }
 //   }


    class AssemblyEmbedder
    {
        TaskLoggingHelper _log;
        ITaskItem[] _referenses;
        ITaskItem[] _targetsPath;
        ITaskItem[] _locals;
        List<ITaskItem> _removedLocals;

        internal AssemblyEmbedder(TaskLoggingHelper Log, ITaskItem[] targetsPath, ITaskItem[] referenses, ITaskItem[] locals)
        {
            _log = Log;
            _targetsPath = targetsPath;
            _referenses = referenses;
            _locals = locals;
            _removedLocals = new List<ITaskItem>();
        }

        internal ITaskItem[] Process()
        {
            var itemsToEmbed = PrepareItemsToEmbed();
            if (itemsToEmbed.Count > 0)
            {
                if (_targetsPath == null || _targetsPath.Length == 0)
                {
                    _log.LogError("No target specified");
                }
                else
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
                    EmbedAssemblies(itemsToEmbed);
                    AppDomain.CurrentDomain.AssemblyResolve -= OnResolveAssembly;
                }
            }

            return _removedLocals.ToArray();
        }

        Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name.ToLower();
            switch (name)
            {
                case "mono.cecil":
                    return LoadEmbeddedAssembly("Vitevic.AssemblyEmbedder.MsBuild.Dlls.Mono.Cecil.dll");

                case "mono.cecil.pdb":
                    return LoadEmbeddedAssembly("Vitevic.AssemblyEmbedder.MsBuild.Dlls.Mono.Cecil.Pdb.dll");
            }

            return null;
        }

        private static Assembly LoadEmbeddedAssembly(string assemblyName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
            {
                if (stream != null)
                {
                    var data = new Byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    return Assembly.Load(data);
                }
            }

            return null;
        }

        private void EmbedAssemblies(ICollection<EmbeddedItemInfo> itemsToEmbed)
        {
            var targetAssemblyPath = _targetsPath[0].GetFullPath();
            WriterParameters writerParameters = null;
            var assembly = ReadAssembly(targetAssemblyPath, out writerParameters);

            foreach (var item in itemsToEmbed)
            {
                var message = String.Format("Embedding \"{0}\"", item.Path);
                _log.LogMessageFromText(message, MessageImportance.Normal);

                var data = File.ReadAllBytes(item.Path);
                var resource = new EmbeddedResource(item.Name, ManifestResourceAttributes.Private, data);
                assembly.MainModule.Resources.Add(resource);
            }

            assembly.Write(targetAssemblyPath, writerParameters);
        }

        private AssemblyDefinition ReadAssembly(String targetAssemblyPath, out WriterParameters writerParameters)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(targetAssemblyPath));
            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            writerParameters = new WriterParameters();
            var pdbPath = Path.ChangeExtension(targetAssemblyPath, "pdb");
            if (File.Exists(pdbPath))
            {
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();
                readerParameters.ReadSymbols = true;
                writerParameters.WriteSymbols = true;
            }

            return AssemblyDefinition.ReadAssembly(targetAssemblyPath, readerParameters);
        }

        private ICollection<EmbeddedItemInfo> PrepareItemsToEmbed()
        {
            List<EmbeddedItemInfo> itemsToEmbed = new List<EmbeddedItemInfo>();
            foreach (var reference in _referenses)
            {
                var embedAssembly = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyName));
                if (embedAssembly)
                {
                    var compress = Attributes.IsTrue(reference.GetMetadata(Attributes.CompressEmbededDataName));
                    var embedInfo = new EmbeddedItemInfo(reference, compress);
                    itemsToEmbed.Add(embedInfo);
                    _removedLocals.Add(reference);
                    var possibleDocPath = Path.ChangeExtension(embedInfo.Path, "xml");
                    RemoveFromLocals(possibleDocPath);

                    PreparePdb(itemsToEmbed, reference, compress, embedInfo.Path);
                    var embedAssemblyDependensies = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyDependensiesName));
                }
            }

            return itemsToEmbed;
        }

        private void PreparePdb(List<EmbeddedItemInfo> itemsToEmbed, ITaskItem reference, bool compress, string assemblyPath)
        {
            var embedAssemblyPdb = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyPdbName));
            if (embedAssemblyPdb)
            {
                var possiblePdbPath = Path.ChangeExtension(assemblyPath, "pdb");
                if (!String.IsNullOrEmpty(possiblePdbPath) && File.Exists(possiblePdbPath))
                {
                    var pdbItemInfo = new EmbeddedItemInfo(possiblePdbPath, compress);
                    itemsToEmbed.Add(pdbItemInfo);
                    RemoveFromLocals(pdbItemInfo.Path);
                }
            }
        }

        private void RemoveFromLocals(string path)
        {
            foreach (var local in _locals)
            {
                var localPath = local.GetFullPath();
                if (0 == String.Compare(localPath, path, true))
                {
                    _removedLocals.Add(local);
                }
            }
        }
    }
}
