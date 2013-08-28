using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Kekekeks.NotifyHelper
{
	public static class CodeGen
	{
		static readonly AssemblyBuilder AssemblyBuilder;
		static readonly ModuleBuilder ModuleBuilder;

		static readonly ComponentResourceManager Res = new ComponentResourceManager(typeof(CodeGen));

		static CodeGen()
		{
			AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Kekekeks.NotifyHelper.Generated"), AssemblyBuilderAccess.Run);
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Module");
		}


		public static Type CreateType(string name, Action<TypeBuilder> cb)
		{
			lock (AssemblyBuilder)
			{
				var builder = ModuleBuilder.DefineType(Guid.NewGuid().ToString().Replace("-", "") + "." + name);
				cb(builder);
				return builder.CreateType();
			}
		}
	}
}