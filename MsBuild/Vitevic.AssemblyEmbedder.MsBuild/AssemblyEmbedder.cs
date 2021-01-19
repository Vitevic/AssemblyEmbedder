using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using Mono.Cecil.Pdb;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    internal class AssemblyEmbedder
    {
        private TaskLoggingHelper _log;
        private ITaskItem[] _referenses;
        private ITaskItem[] _targetsPath;
        private ITaskItem[] _locals;
        private List<ITaskItem> _removedLocals;

        internal AssemblyEmbedder(TaskLoggingHelper Log, ITaskItem[] targetsPath, ITaskItem[] referenses, ITaskItem[] locals)
        {
            this._log = Log;
            this._targetsPath = targetsPath;
            this._referenses = referenses;
            this._locals = locals;
            this._removedLocals = new List<ITaskItem>();
        }

        internal ITaskItem[] Process()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            var itemsToEmbed = PrepareItemsToEmbed();
            if (itemsToEmbed.Count > 0)
            {
                if (this._targetsPath == null || this._targetsPath.Length == 0)
                {
                    this._log.LogError("No target specified");
                }
                else
                {
                    EmbedAssemblies(itemsToEmbed);
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= OnResolveAssembly;
            return this._removedLocals.ToArray();
        }

        private System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            string name = new System.Reflection.AssemblyName(args.Name).Name.ToLower();
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
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    return System.Reflection.Assembly.Load(data);
                }
            }

            return null;
        }

        private void EmbedAssemblies(ICollection<EmbeddedItemInfo> itemsToEmbed)
        {
            string targetAssemblyPath = this._targetsPath[0].GetFullPath();
            WriterParameters writerParameters;
            var assembly = ReadAssembly(targetAssemblyPath, out writerParameters);

            foreach (var item in itemsToEmbed)
            {
                string message = $"Embedding \"{item.Path}\"";
                this._log.LogMessageFromText(message, MessageImportance.Normal);

                byte[] data = File.ReadAllBytes(item.Path);
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

        private static AssemblyDefinition ReadAssembly(string targetAssemblyPath, out WriterParameters writerParameters)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(targetAssemblyPath));
            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            writerParameters = new WriterParameters();
            string pdbPath = Path.ChangeExtension(targetAssemblyPath, "pdb");
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
            foreach (var reference in this._referenses)
            {
                bool embedAssembly = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyName));
                if (embedAssembly)
                {
                    bool compress = Attributes.IsTrue(reference.GetMetadata(Attributes.CompressEmbededDataName));
                    var embedInfo = new EmbeddedItemInfo(reference, compress);
                    itemsToEmbed.Add(embedInfo);
                    this._removedLocals.Add(reference);
                    string possibleDocPath = Path.ChangeExtension(embedInfo.Path, "xml");
                    RemoveFromLocals(possibleDocPath);

                    //PreparePdb(itemsToEmbed, reference, compress, embedInfo.Path);
                    bool embedAssemblyDependensies = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyDependenciesName));
                    if( embedAssemblyDependensies )
                    {
                        string projectPath = reference.GetMSBuildSourceProjectFile();
                        AddProjectDependencies(itemsToEmbed, compress, projectPath);
                    }                    
                }
            }

            return itemsToEmbed;
        }

        private void AddProjectDependencies(IList<EmbeddedItemInfo> itemsToEmbed, bool compress, string projectPath)
        {
            if (!string.IsNullOrEmpty(projectPath))
            {
                string projectDir = Path.GetDirectoryName(projectPath);
                var loadedProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectPath).FirstOrDefault();
                if (loadedProject == null)
                    return;

                var dependencyProjects = from x in loadedProject.AllEvaluatedItems
                                         where x.ItemType == "ProjectReference"
                                         select x;
                foreach (var project in dependencyProjects)
                {
                    string depPath = Path.Combine(projectDir, project.EvaluatedInclude);
                    depPath = Path.GetFullPath(depPath); // to remove "/../../" in depPath
                    var depLoadedProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(depPath).FirstOrDefault();
                    if (depLoadedProject == null)
                        continue;

                    string dependencyDllPath = depLoadedProject.GetPropertyValue("TargetPath");
                    var depEmbedInfo = new EmbeddedItemInfo(dependencyDllPath, compress);
                    itemsToEmbed.Add(depEmbedInfo);
                    RemoveFromLocals(dependencyDllPath, true);
                    AddProjectDependencies(itemsToEmbed, compress, depPath);
                }
            }
        }

        private void PreparePdb(List<EmbeddedItemInfo> itemsToEmbed, ITaskItem reference, bool compress, string assemblyPath)
        {
            bool embedAssemblyPdb = Attributes.IsTrue(reference.GetMetadata(Attributes.EmbedAssemblyPdbName));
            if (embedAssemblyPdb)
            {
                string possiblePdbPath = Path.ChangeExtension(assemblyPath, "pdb");
                if (!string.IsNullOrEmpty(possiblePdbPath) && File.Exists(possiblePdbPath))
                {
                    var pdbItemInfo = new EmbeddedItemInfo(possiblePdbPath, compress);
                    itemsToEmbed.Add(pdbItemInfo);
                    RemoveFromLocals(pdbItemInfo.Path);
                }
            }
        }

        private void RemoveFromLocals(string path, bool testFileName = false)
        {
            foreach (var local in this._locals)
            {
                string localPath = local.GetFullPath();
                if (0 == string.Compare(localPath, path, true))
                {
                    this._removedLocals.Add(local);
                }
                else if (testFileName)
                {
                    string fileName = Path.GetFileName(path);
                    string localFileName = Path.GetFileName(localPath);
                    if (0 == string.Compare(fileName, localFileName, true))
                    {
                        this._removedLocals.Add(local);
                    }
                }
            }
        }

    }
}
