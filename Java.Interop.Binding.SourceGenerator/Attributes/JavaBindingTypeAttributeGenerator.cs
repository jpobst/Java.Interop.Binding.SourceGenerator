using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Java.Interop.Binding.SourceGenerator;

[Generator]
public     class JavaBindingTypeAttributeGenerator : IIncrementalGenerator
    {
	public void Initialize (IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput (static postInitializationContext => {
			postInitializationContext.AddSource ("JavaBindingTypeAttribute.g.cs", SourceText.From (
"""
using System;

namespace Java.Interop.Binding.SourceGenerator;

[System.AttributeUsage (System.AttributeTargets.Class)]
internal sealed class JavaBindingTypeAttribute : Attribute
{
	public string JavaType { get; }

	public JavaBindingTypeAttribute (string javaType)
	{
		JavaType = javaType;
	}
}
"""
			, Encoding.UTF8));
		});
	}

}
