﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Kekekeks.NotifyHelper
{
	public abstract class AutoNotifyPropertyChanged : INotifyPropertyChanged
	{
		protected class PropertyNameAttribute : Attribute
		{
			public string Name { get; private set; }

			public PropertyNameAttribute(string name)
			{
				Name = name;
			}
		}

		protected class SuppressNotifyAttribute : Attribute
		{
			 
		}

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		public Dictionary<string, PropertyInfo> GetAllNamedProperties()
		{
			var t = GetType();
			var rslt = new Dictionary<string, PropertyInfo>();
			foreach (var p in t.GetProperties())
			{
				var attr = p.GetCustomAttributes(typeof(PropertyNameAttribute), false).OfType<PropertyNameAttribute>().FirstOrDefault();
				rslt[attr.Name] = p;
			}
			return rslt;
		}

		//To survive obfuscation we have to find RaisePropertyChanged by attribute
		class RaisePropertyChangedAttribute : Attribute { }

		[RaisePropertyChanged]
		protected virtual void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		static class GeneratedContainer<T>
		{
			// ReSharper disable StaticFieldInGenericType
			static volatile Type _generated;
			static readonly object Lock = new object();
			// ReSharper restore StaticFieldInGenericType
			public static Type GetGenerated(Action<TypeBuilder> cb)
			{
				if (_generated == null)
				{
					lock (Lock)
					{
						if (_generated == null)
							_generated = CodeGen.CreateType(typeof(T).Name, cb);
					}
				}
				return _generated;
			}

		}

		static readonly MethodInfo RaisePropertyChangedInfo = typeof(AutoNotifyPropertyChanged).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).First(m => m.GetCustomAttributes(typeof(RaisePropertyChangedAttribute), true).Any());

		const MethodAttributes PropertyAttrs = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

		public static T CreateInstance<T>() where T : AutoNotifyPropertyChanged
		{
			return (T)Activator.CreateInstance(GeneratedContainer<T>.GetGenerated(typeBuilder =>
			{
				typeBuilder.SetParent(typeof(T));
				foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					if (propertyInfo.GetCustomAttributes(true).OfType<SuppressNotifyAttribute>().Any())
						continue;
					var name =
							propertyInfo.GetCustomAttributes(true).OfType<PropertyNameAttribute>().Select(a => a.Name).FirstOrDefault()
							?? propertyInfo.Name;

					var setterInfo = propertyInfo.GetSetMethod();
					if (setterInfo == null)
						continue;
					if (!setterInfo.IsAbstract && !setterInfo.IsVirtual)
						throw new InvalidProgramException("Property " + propertyInfo.Name + " of type " + typeof(T).Name + " should be virtual or abstract");

					FieldInfo field = null;
					if (setterInfo.IsAbstract)
						field = typeBuilder.DefineField("k_____" + name, propertyInfo.PropertyType, FieldAttributes.Private);

					var setter = typeBuilder.DefineMethod("set_" + name, PropertyAttrs, typeof(void), new[] { propertyInfo.PropertyType });
					var setterIl = setter.GetILGenerator();
					setterIl.Emit(OpCodes.Ldarg_0);
					setterIl.Emit(OpCodes.Ldarg_1);
					if (field != null)
						setterIl.Emit(OpCodes.Stfld, field);
					else
						setterIl.Emit(OpCodes.Call, setterInfo);


					setterIl.Emit(OpCodes.Ldarg_0);
					setterIl.Emit(OpCodes.Ldstr, name);
					setterIl.Emit(OpCodes.Call, RaisePropertyChangedInfo);
					setterIl.Emit(OpCodes.Ret);

					typeBuilder.DefineMethodOverride(setter, setterInfo);


					var getterInfo = propertyInfo.GetGetMethod(true);
					var getter = typeBuilder.DefineMethod("get_" + name, PropertyAttrs, propertyInfo.PropertyType, Type.EmptyTypes);
					var getterIl = getter.GetILGenerator();
					getterIl.Emit(OpCodes.Ldarg_0);
					if (field != null)
						getterIl.Emit(OpCodes.Ldfld, field);
					else
						getterIl.Emit(OpCodes.Call, getterInfo);
					getterIl.Emit(OpCodes.Ret);

					typeBuilder.DefineMethodOverride(getter, getterInfo);

					var newProp = typeBuilder.DefineProperty(name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
					newProp.SetGetMethod(getter);
					newProp.SetSetMethod(setter);
				}
			}));
		}
	}
}