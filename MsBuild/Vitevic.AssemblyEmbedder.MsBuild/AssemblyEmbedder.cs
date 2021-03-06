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
using Mono.Cecil.Cil;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
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
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            var itemsToEmbed = PrepareItemsToEmbed();
            if (itemsToEmbed.Count > 0)
            {
                if (_targetsPath == null || _targetsPath.Length == 0)
                {
                    _log.LogError("No target specified");
                }
                else
                {
                    EmbedAssemblies(itemsToEmbed);
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= OnResolveAssembly;
            return _removedLocals.ToArray();
        }

        System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new System.Reflection.AssemblyName(args.Name).Name.ToLower();
            switch (name)
            {
                case "mono.cecil":
                    return LoadEmbeddedAssembly("Vitevic.AssemblyEmbedder.MsBuild.Dlls.Mono.Cecil.dll");

                case "mono.cecil.pdb":
                    return LoadEmbeddedAssembly("Vitevic.AssemblyEmbedder.MsBuild.Dlls.Mono.Cecil.Pdb.dll");
            }

            return null;
        }

        private static System.Reflection.Assembly LoadEmbeddedAssembly(string assemblyName)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
            {
                if (stream != null)
                {
                    var data = new Byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    return System.Reflection.Assembly.Load(data);
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

            var injector = new CodeInjector(assembly);
            injector.Inject();

            //if (!String.IsNullOrEmpty(_keyFile) && File.Exists(_keyFile))
            //{
            //    writerParameters.StrongNameKeyPair = new System.Reflection.StrongNameKeyPair(_keyFile);
            //    var pb = writerParameters.StrongNameKeyPair.PublicKey;                
            //}

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

                    //PreparePdb(itemsToEmbed, reference, compress, embedInfo.Path);
                    var embedAssemblyDependensies = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyDependenciesName));
                    if( embedAssemblyDependensies )
                    {
                        var projectPath = reference.GetMSBuildSourceProjectFile();
                        AddProjectDependencies(itemsToEmbed, compress, projectPath);
                    }                    
                }
            }

            return itemsToEmbed;
        }

        private void AddProjectDependencies(IList<EmbeddedItemInfo> itemsToEmbed, bool compress, string projectPath)
        {
            if (!String.IsNullOrEmpty(projectPath))
            {
                var projectDir = System.IO.Path.GetDirectoryName(projectPath);
                var loadedProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectPath).FirstOrDefault();
                if (loadedProject == null)
                    return;

                var dependencyProjects = from x in loadedProject.AllEvaluatedItems
                                         where x.ItemType == "ProjectReference"
                                         select x;
                foreach (var project in dependencyProjects)
                {
                    var depPath = Path.Combine(projectDir, project.EvaluatedInclude);
                    depPath = Path.GetFullPath(depPath); // to remove "/../../" in depPath
                    var depLoadedProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(depPath).FirstOrDefault();
                    if (depLoadedProject == null)
                        continue;

                    var dependencyDllPath = depLoadedProject.GetPropertyValue("TargetPath");
                    var depEmbedInfo = new EmbeddedItemInfo(dependencyDllPath, compress);
                    itemsToEmbed.Add(depEmbedInfo);
                    RemoveFromLocals(dependencyDllPath, true);
                    AddProjectDependencies(itemsToEmbed, compress, depPath);
                }
            }
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

        private void RemoveFromLocals(string path, bool testFileName = false)
        {
            foreach (var local in _locals)
            {
                var localPath = local.GetFullPath();
                if (0 == String.Compare(localPath, path, true))
                {
                    _removedLocals.Add(local);
                }
                else if (testFileName)
                {
                    var fileName = Path.GetFileName(path);
                    var localFileName = Path.GetFileName(localPath);
                    if (0 == String.Compare(fileName, localFileName, true))
                    {
                        _removedLocals.Add(local);
                    }
                }
            }
        }

    }
}
