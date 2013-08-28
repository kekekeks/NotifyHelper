NotifyHelper
============

This repository contains a INotifyPropertyChanged implementation autogenerator. It uses Reflection.Emit runtime code generation and doesn't require any build tasks.



How to use
==========
    abstract class MyModel : AutoNotifyPropertyChanged
    {
         public abstract string MyPropery { get; set; }
    }
Then you need to call AutoNotifyPropertyChanged.CreateInstance&lt;MyModel>();

Your property also can be declared as virtual. You can exclude your property by using `SuppressNotify` attribute. To make your code survive obfuscation you can specify property name with `PropertyName` attribute.

See unit tests, they cover most use cases.
