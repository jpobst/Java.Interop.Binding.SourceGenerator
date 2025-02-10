using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Java.Interop.Binding.SourceGenerator;

[Generator]
public     class JavaBindingMethodAttributeGenerator : IIncrementalGenerator
    {
	public void Initialize (IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput (static postInitializationContext => {
			postInitializationContext.AddSource ("JavaBindingMethodAttribute.g.cs", SourceText.From (
"""
using System;

namespace Java.Interop.Binding.SourceGenerator;

[System.AttributeUsage (System.AttributeTargets.Method)]
internal sealed class JavaBindingMethodAttribute : Attribute
{
	public string JavaMethod { get; }

	public JavaBindingMethodAttribute (string javaMethod)
	{
		JavaMethod = javaMethod;
	}
}
"""
			, Encoding.UTF8));
		});
	}

}
