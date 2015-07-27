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
    class CodeInjector
    {
       AssemblyDefinition assembly;
    
       public CodeInjector(AssemblyDefinition assembly)
       {
           this.assembly = assembly;
       }
    
       private MethodDefinition DefineModuleCtor(Type fieldType, FieldDefinition field, MethodDefinition assemblyResolveMethod)
       {
           var ctor = new MethodDefinition(".cctor",
                       MethodAttributes.Static |
                       MethodAttributes.SpecialName |
                       MethodAttributes.RTSpecialName,
                       assembly.MainModule.Import(typeof(void)));
    
           var il = ctor.Body.GetILProcessor();
           il.Emit(OpCodes.Newobj, assembly.MainModule.Import(fieldType.GetConstructor(new Type[0])));
           il.Emit(OpCodes.Stsfld, field);
           il.Emit(OpCodes.Call, ImportMethod<AppDomain>("get_CurrentDomain"));
           il.Emit(OpCodes.Ldnull);
           il.Emit(OpCodes.Ldftn, assemblyResolveMethod);
           il.Emit(OpCodes.Newobj, ImportCtor<ResolveEventHandler>(typeof(object), typeof(IntPtr)));
           il.Emit(OpCodes.Callvirt, ImportMethod<AppDomain>("add_AssemblyResolve"));
           il.Emit(OpCodes.Ret);
    
           return ctor;
       }

       private MethodDefinition DefineOnAssemblyResolveMethod(Type fieldType, FieldDefinition field)
       {
           //static System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
           //{
           //    var assemblyName = "Vitevic.Embedded." + new System.Reflection.AssemblyName(args.Name).Name + ".dll";
           //    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
           //    {
           //        if (stream != null)
           //        {
           //            var data = new Byte[stream.Length];
           //            stream.Read(data, 0, data.Length);
           //            return System.Reflection.Assembly.Load(data);
           //        }
           //    }

           //    return null;
           //}

           var method = new MethodDefinition("OnResolveAssembly",
           MethodAttributes.Private |
           MethodAttributes.HideBySig |
           MethodAttributes.Static,
           ImportType<System.Reflection.Assembly>());
           method.Parameters.Add(new ParameterDefinition(ImportType<object>()));
           method.Parameters.Add(new ParameterDefinition(ImportType<ResolveEventArgs>()));

           method.Body.Variables.Add(new VariableDefinition(ImportType<string>())); // assemblyName
           method.Body.Variables.Add(new VariableDefinition(ImportType<Stream>())); // stream
           method.Body.Variables.Add(new VariableDefinition(ImportType<byte[]>())); // data
           method.Body.Variables.Add(new VariableDefinition(ImportType<System.Reflection.Assembly>())); // result
           var hiddenBool = new VariableDefinition(ImportType<bool>());
           method.Body.Variables.Add(hiddenBool); // hiddenBool
           method.Body.InitLocals = true;

           var il = method.Body.GetILProcessor();
           il.Emit(OpCodes.Ldstr, EmbeddedItemInfo.ResourcePrefix);
           il.Emit(OpCodes.Ldarg_1);
           il.Emit(OpCodes.Callvirt, ImportMethod<System.ResolveEventArgs>("get_Name"));
           il.Emit(OpCodes.Newobj, ImportCtor<System.Reflection.AssemblyName>(typeof(string)));
           il.Emit(OpCodes.Call, ImportMethod<System.Reflection.AssemblyName>("get_Name"));
           il.Emit(OpCodes.Ldstr, ".dll");
           il.Emit(OpCodes.Call, ImportMethod<String>("Concat", typeof(String), typeof(String), typeof(String)));
           il.Emit(OpCodes.Stloc_0);
           il.Emit(OpCodes.Ldsfld, field);
           il.Emit(OpCodes.Ldloc_0);
           il.Emit(OpCodes.Callvirt, ImportMethod(fieldType, "ContainsKey"));
           var il_0039_getExecutingAssembly = il.Create(OpCodes.Call, ImportMethod<System.Reflection.Assembly>("GetExecutingAssembly"));
           il.Emit(OpCodes.Brfalse_S, il_0039_getExecutingAssembly);

           il.Emit(OpCodes.Ldsfld, field);
           il.Emit(OpCodes.Ldloc_0);
           il.Emit(OpCodes.Callvirt, ImportMethod(fieldType, "get_Item"));
           il.Emit(OpCodes.Ret);

           il.Append(il_0039_getExecutingAssembly);
           il.Emit(OpCodes.Ldloc_0);
           il.Emit(OpCodes.Callvirt, ImportMethod<System.Reflection.Assembly>("GetManifestResourceStream", typeof(string)));
           il.Emit(OpCodes.Stloc_1);

           var startTry = il.Create(OpCodes.Nop);

           il.Append(startTry);
           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Ldnull);
           il.Emit(OpCodes.Ceq);
           il.Emit(OpCodes.Stloc_S, hiddenBool);
           il.Emit(OpCodes.Ldloc_S, hiddenBool);

           var streamNullInTry = il.Create(OpCodes.Nop);
           il.Emit(OpCodes.Brtrue_S, streamNullInTry);

           il.Emit(OpCodes.Nop);
           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Callvirt, ImportMethod<Stream>("get_Length"));
           il.Emit(OpCodes.Conv_Ovf_I);
           il.Emit(OpCodes.Newarr, ImportType<byte>());
           il.Emit(OpCodes.Stloc_2);
           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Ldloc_2);
           il.Emit(OpCodes.Ldc_I4_0);
           il.Emit(OpCodes.Ldloc_2);
           il.Emit(OpCodes.Ldlen);
           il.Emit(OpCodes.Conv_I4);
           il.Emit(OpCodes.Callvirt, ImportMethod<Stream>("Read", typeof(byte[]), typeof(int), typeof(int)));
           il.Emit(OpCodes.Pop);
           il.Emit(OpCodes.Ldloc_2);
           il.Emit(OpCodes.Call, ImportMethod<System.Reflection.Assembly>("Load", typeof(byte[])));
           il.Emit(OpCodes.Stloc_3);

           var beforeFinalRet = il.Create(OpCodes.Nop);
           il.Emit(OpCodes.Leave_S, beforeFinalRet);

           il.Append(streamNullInTry);
           il.Emit(OpCodes.Ldnull);
           il.Emit(OpCodes.Stloc_3);
           il.Emit(OpCodes.Leave_S, beforeFinalRet);

           var endTry = il.Create(OpCodes.Nop);
           il.Append(endTry);

           // finally handler
           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Ldnull);
           il.Emit(OpCodes.Ceq);
           il.Emit(OpCodes.Stloc_S, hiddenBool);
           il.Emit(OpCodes.Ldloc_S, hiddenBool);
           var endFinally = il.Create(OpCodes.Endfinally);
           il.Emit(OpCodes.Brtrue_S, endFinally);
           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Callvirt, ImportMethod<Stream>("Dispose"));
           il.Append(endFinally);

           // return result block
           il.Append(beforeFinalRet);
           il.Emit(OpCodes.Ldloc_3);
           il.Emit(OpCodes.Ret);

           var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
           {
               TryStart = startTry,
               TryEnd = endTry,
               HandlerStart = endTry,
               HandlerEnd = beforeFinalRet,
           };

           method.Body.ExceptionHandlers.Add(handler);

           return method;
       }
    
       private TypeReference ImportType<T>()
       {
           return assembly.MainModule.Import(typeof(T));
       }
       private MethodReference ImportMethod(Type type, string methodName)
       {
           return assembly.MainModule.Import(type.GetMethod(methodName));
       }
       private MethodReference ImportMethod<T>(string methodName)
       {
           return assembly.MainModule.Import(typeof(T).GetMethod(methodName));
       }
       private MethodReference ImportMethod<T>(string methodName, params Type[] types)
       {
           return assembly.MainModule.Import(typeof(T).GetMethod(methodName, types));
       }
       private MethodReference ImportCtor<T>(params Type[] types)
       {
           return assembly.MainModule.Import(typeof(T).GetConstructor(types));
       }
    
       internal void Inject()
       {
           var fieldType = typeof(System.Collections.Generic.Dictionary<string, System.Reflection.Assembly>);
           var field = new FieldDefinition("assemblies", FieldAttributes.Private | FieldAttributes.Static, assembly.MainModule.Import(fieldType) );

           var assemblyResolve = DefineOnAssemblyResolveMethod(fieldType, field);
           var ctor = DefineModuleCtor(fieldType, field, assemblyResolve);
    
           var moduleType = assembly.MainModule.Types.Single(x => x.Name == "<Module>");
           moduleType.Methods.Add(assemblyResolve);
           moduleType.Methods.Add(ctor);
           moduleType.Fields.Add(field);
       }
    }
}
