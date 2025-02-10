# Java.Interop.Binding.SourceGenerator

This is a super quick prototype of using Roslyn source generators to create managed
"slim" Android for .NET bindings.  This relies on the user creating the desired C#
type binding and annotating it with attributes that control how the needed infrastructure
will be generated.

Additional context is available on the [GitHub proposal](https://github.com/dotnet/java-interop/issues/1300).

>[!NOTE]
> As this is a quick prototype, there are many, many limitations:
> 
> - Classes only, no interfaces
> - Instance classes only, no static classes
> - Instance methods only, no static methods
> - Final methods only, no virtual methods
> - No support for Java callbacks
> - Only primitive method parameter and return types (byte, bool, char, double, float, int, long, short)
> - No support for arrays
> 
> Future work on this prototype, if it happens, would be to work on supporting the above cases.

## Sample

We are going to use this prototype to bind [this simple Java file](https://github.com/jpobst/Java.Interop.Binding.SourceGenerator/blob/main/java/Mathinator.java).

Start with a basic .NET for Android application:

```cli
dotnet new android
```

Add the prototype source generators NuGet package:

```cli
dotnet add package XamPrototype.Android.Binding.SourceGenerator --version 0.0.1
```

Download the [compiled Java .jar](https://github.com/jpobst/Java.Interop.Binding.SourceGenerator/raw/refs/heads/main/lib/test.jar) we are going to bind and place it in the project root direction.

By default, .NET for Android will automatically bind this library. We want to do it with the source
generators instead, so we need to disable the default binding in the `.csproj`:

```xml
<ItemGroup>
  <AndroidLibrary Update="test.jar" Bind="false" />
</ItemGroup>
```

Additionally, we need to allow `unsafe` code for the binding:

```xml
<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
```

Create a new class called `Mathinator` that uses the source generator attributes to define the
API we wish to bind:

```java
using Android.Runtime;
using Java.Interop.Binding.SourceGenerator;

namespace sample;

[JavaBindingType ("com.example.Mathinator")]
public partial class Mathinator : Java.Lang.Object
{
	[JavaBindingMethod ("add")]
	public unsafe partial int Add (int a, int b);

	[JavaBindingMethod ("setVersion")]
	public unsafe partial void SetVersion (int version);

	[JavaBindingMethod ("getVersion")]
	public unsafe partial int GetVersion ();
}
```

When the project is compiled, infrastructure for this class and its 3 methods will be generated in
the background.  These methods can now be called in `MainActivity.cs`:

```csharp
protected override void OnCreate (Bundle? savedInstanceState)
{
	base.OnCreate (savedInstanceState);

	// Set our view from the "main" layout resource
	SetContentView (Resource.Layout.activity_main);

	var mathinator = new Mathinator ();
	Toast.MakeText (this, $"5 + 15 = {mathinator.Add (5, 15)}", ToastLength.Long).Show ();
}
```

Running the app should now show a toast with the Java library being called:

```cli
dotnet run
```

https://github.com/user-attachments/assets/94284180-4ab7-4eb8-98f6-8c90771d4f07
