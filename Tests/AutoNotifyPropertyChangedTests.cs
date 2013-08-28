using System;
using System.ComponentModel;
using Kekekeks.NotifyHelper;
using Xunit;

namespace Tests
{
	public class AutoNotifyPropertyChangedTests
	{
		interface INamed : INotifyPropertyChanged
		{
			string Name { get; set; }
		}

		public abstract class MyModelWithAbstractProperty : AutoNotifyPropertyChanged, INamed
		{
			public abstract string Name { get; set; }
		}

		public abstract class MyModelWithNamedProperty : AutoNotifyPropertyChanged, INamed
		{
			[PropertyName("NotName")]
			public abstract string Name { get; set; }
		}

		public abstract class MyModelWithVirtualProperty : AutoNotifyPropertyChanged, INamed
		{
			private string _name;
			// ReSharper disable ConvertToAutoProperty
			public virtual string Name
			// ReSharper restore ConvertToAutoProperty
			{
				get { return _name; }
				set { _name = value; }
			}
			public string GetName()
			{
				return _name;
			}
		}

		public abstract class MyModelWithInvalidProperty : AutoNotifyPropertyChanged
		{
			public string Name { get; set; }
		}

// ReSharper disable UnusedParameter.Local
		void TestNamed(INamed instance, string name = "Name")
// ReSharper restore UnusedParameter.Local
		{
			string propName = null;
			instance.PropertyChanged += (_, n) => propName = n.PropertyName;

			instance.Name = "1";
			Assert.Equal("1", instance.Name);
			Assert.Equal(name, propName);
		}

		[Fact]
		public void TestNotifyAbstract()
		{
			TestNamed(AutoNotifyPropertyChanged.CreateInstance<MyModelWithAbstractProperty>());
		}

		[Fact]
		public void TestNotifyVirtual()
		{
			var instance = AutoNotifyPropertyChanged.CreateInstance<MyModelWithVirtualProperty>();
			TestNamed(instance);
			instance.Name = "2";
			Assert.Equal("2", instance.GetName());
		}

		[Fact]
		public void TestNotifyRenamedAbstract()
		{
			TestNamed(AutoNotifyPropertyChanged.CreateInstance<MyModelWithNamedProperty>(), "NotName");
		}

		[Fact]
		public void TestRenamedReflection()
		{
			var instance = AutoNotifyPropertyChanged.CreateInstance<MyModelWithNamedProperty>();
			var prop = instance.GetType().GetProperty("NotName");
			prop.SetValue(instance, "1", null);
			Assert.Equal("1", instance.Name);
		}

		[Fact]
		public void TestInvalid()
		{
			Assert.Throws<InvalidProgramException>(() => AutoNotifyPropertyChanged.CreateInstance<MyModelWithInvalidProperty>());
		}


		public abstract class Ignorance : AutoNotifyPropertyChanged
		{
			public string ReadOnly { get { return "123"; } }

			[SuppressNotify]
			public string Ignored { get; set; }
		}

		[Fact]
		public void TestIgnorance()
		{
			//Check for correct compilation
			AutoNotifyPropertyChanged.CreateInstance<Ignorance>();
		}

	}
}
