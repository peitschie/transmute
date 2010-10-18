using System;
using System.Reflection.Emit;
using System.Reflection;

namespace EmitMapper
{
	/// <summary>
	/// Class which maintains an assembly for created object Mappers
	/// </summary>
	public class DynamicAssemblyManager
	{
		/// <summary>
		/// Saves assembly with created Mappers to file. This method is useful for debugging purpose.
		/// </summary>
		public static void SaveAssembly()
		{
#if !SILVERLIGHT
			lock (typeof(DynamicAssemblyManager))
			{
				assemblyBuilder.Save(assemblyName.Name + ".dll");
			}
#else
          throw new NotImplementedException("DynamicAssemblyManager.SaveAssembly");
#endif
		}

		#region Non-public members

		private static AssemblyName assemblyName;
		private static AssemblyBuilder assemblyBuilder;
		private static ModuleBuilder moduleBuilder;

		static DynamicAssemblyManager()
		{
#if !SILVERLIGHT
            assemblyName = new AssemblyName("Transmute.EmitHelper.Dynamic");
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				assemblyName,
				AssemblyBuilderAccess.RunAndSave
				);

			moduleBuilder = assemblyBuilder.DefineDynamicModule(
				assemblyName.Name,
				assemblyName.Name + ".dll",
				true);
#else
            assemblyName = new AssemblyName("Transmute.EmitHelper.Dynamic.SL");
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                  assemblyName,
                  AssemblyBuilderAccess.Run
                  );
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, true);
#endif
        }

		private static string CorrectTypeName(string typeName)
		{
			if (typeName.Length >= 1042)
			{
				typeName = "type_" + typeName.Substring(0, 900) + Guid.NewGuid().ToString().Replace("-", "");
			}
			return typeName;
		}

		public static TypeBuilder DefineMapperType(string typeName)
		{
			lock (typeof(DynamicAssemblyManager))
			{
				return moduleBuilder.DefineType(
					CorrectTypeName(typeName + Guid.NewGuid().ToString().Replace("-", "")),
					TypeAttributes.Public | TypeAttributes.Sealed,
					null
					);
			}
		}
		#endregion
	}
}