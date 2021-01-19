using System;
using System.Linq;
using Mono.Cecil;
using System.IO;
using Mono.Cecil.Cil;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
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
//    private static Assembly OnResolveAssembly(object A_0, ResolveEventArgs A_1)
//	{
//		string text = "Vitevic.EmbeddedAssembly." + new AssemblyName(A_1.Name).Name + ".dll";
//		if (<Module>.assemblies.ContainsKey(text))
//		{
//			return <Module>.assemblies[text];
//		}
//		using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text))
//		{
//			if (manifestResourceStream != null)
//			{
//				byte[] array = new byte[manifestResourceStream.Length];
//				manifestResourceStream.Read(array, 0, array.Length);
//				Assembly assembly = Assembly.Load(array);
//				<Module>.assemblies[text] = assembly;
//				return assembly;
//			}
//		}
//		return null;
//	}

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
           var hiddenAssembly = new VariableDefinition(ImportType<System.Reflection.Assembly>());
           method.Body.Variables.Add(hiddenAssembly);
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

           //////////////////////////////////////////////// try
           var startTry = il.Create(OpCodes.Ldloc_1);
           var il_0085 = il.Create(OpCodes.Ldnull);
           var il_0079 = il.Create(OpCodes.Leave_S, il_0085);

           il.Append(startTry);
           il.Emit(OpCodes.Brfalse_S, il_0079);
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
           il.Emit(OpCodes.Ldsfld, field);
           il.Emit(OpCodes.Ldloc_0);
           il.Emit(OpCodes.Ldloc_3);
           il.Emit(OpCodes.Callvirt, ImportMethod(fieldType, "set_Item"));
           il.Emit(OpCodes.Ldloc_3);
           il.Emit(OpCodes.Stloc_S, hiddenAssembly);

           var il_0087 = il.Create(OpCodes.Ldloc_S, hiddenAssembly);
           il.Emit(OpCodes.Leave_S, il_0087);
           il.Append(il_0079);

           // finally
           var endTry = il.Create(OpCodes.Ldloc_1);
           il.Append(endTry);
           var il_0084_endFinally = il.Create(OpCodes.Endfinally);
           il.Emit(OpCodes.Brfalse_S, il_0084_endFinally);

           il.Emit(OpCodes.Ldloc_1);
           il.Emit(OpCodes.Callvirt, ImportMethod<Stream>("Dispose"));
           il.Append(il_0084_endFinally);

           il.Append(il_0085);
           il.Emit(OpCodes.Ret);
           il.Append(il_0087);
           il.Emit(OpCodes.Ret);

           var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
           {
               TryStart = startTry,
               TryEnd = endTry,
               HandlerStart = endTry,
               HandlerEnd = il_0085,
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
