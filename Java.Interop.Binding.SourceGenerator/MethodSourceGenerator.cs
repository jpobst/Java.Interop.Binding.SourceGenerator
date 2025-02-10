using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Java.Interop.Binding.SourceGenerators;

[Generator]
public class MethodSourceGenerator : IIncrementalGenerator
{
	public void Initialize (IncrementalGeneratorInitializationContext context)
	{
		var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName (
		     fullyQualifiedMetadataName: "Java.Interop.Binding.SourceGenerator.JavaBindingMethodAttribute",
		     predicate: static (syntaxNode, cancellationToken) => syntaxNode is BaseMethodDeclarationSyntax,
		     transform: CreateModel);

		context.RegisterSourceOutput (pipeline, AddSource);
	}

	private static Model CreateModel (GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		var method = (IMethodSymbol) context.TargetSymbol;
		var return_type = method.ReturnType.GetFullName ();
		var declaring_type = method.ContainingType;

		var ns = declaring_type.GetNamespace ();
		var java_name = context.Attributes.SingleOrDefault (a => a.AttributeClass?.Name == "JavaBindingMethodAttribute")?.ConstructorArguments [0].Value?.ToString ();

		//Debugger.Launch ();
		// Note: this is a simplified example. You will also need to handle the case where the type is in a global namespace, nested, etc.
		var model = new Model (ns, declaring_type.Name, method.Name, java_name!, return_type);

		foreach (var parameter in method.Parameters)
			model.Parameters.Add (new ParameterModel (parameter.Name, parameter.Type.GetFullName ()));

		return model;
	}

	private static void AddSource (SourceProductionContext context, Model model)
	{
		var sb = new StringBuilder ();

		sb.AppendLine ("using System;");
		sb.AppendLine ("using Java.Interop;");
		sb.AppendLine ();
		sb.AppendLine ($"namespace {model.Namespace};");
		sb.AppendLine ();
		sb.AppendLine ($"partial class {model.ClassName}");
		sb.AppendLine ("{");
		sb.AppendLine ($"\tpublic unsafe partial {Formatter.FormatCSharpType (model.ReturnType)} {model.MethodName} ({model.FormatMethodParameters ()})");
		sb.AppendLine ("\t{");
		sb.AppendLine ($"\t\tconst string __id = \"{model.JavaMethodName}.{model.FormatJniSignature ()}\";");
		sb.AppendLine ("\t\ttry {");
		sb.AppendLine ($"\t\t\tJniArgumentValue* __args = stackalloc JniArgumentValue [{model.Parameters.Count}];");

		for (var i = 0; i < model.Parameters.Count; i++)
			sb.AppendLine ($"\t\t\t__args [{i}] = new JniArgumentValue ({model.Parameters [i].Name});");

		var ret = model.ReturnType == "System.Void" ? "" : "return ";

		sb.AppendLine ($"\t\t\t{ret}_members.InstanceMethods.InvokeNonvirtual{model.GetInvokeType ()}Method (__id, this, __args);");
		sb.AppendLine ("\t\t} finally {");
		sb.AppendLine ("\t\t}");
		sb.AppendLine ("\t}");
		sb.AppendLine ("}");

		var sourceText = SourceText.From (sb.ToString (), Encoding.UTF8);

		context.AddSource (model.GetFileName (), sourceText);
	}

	private class Model (string ns, string className, string methodName, string javaMethodName, string returnType)
	{
		public string Namespace { get; } = ns;
		public string ClassName { get; } = className;
		public string MethodName { get; } = methodName;
		public string JavaMethodName { get; } = javaMethodName;
		public string ReturnType { get; set; } = returnType;
		public List<ParameterModel> Parameters { get; } = [];

		public string FormatMethodParameters () => string.Join (", ", Parameters.Select (p => p.ToString ()));
		public string FormatJniSignature () => $"({string.Concat (Parameters.Select (p => Formatter.FormatJniType (p.Type)))}){Formatter.FormatJniType (ReturnType)}";
		public string GetInvokeType () => Formatter.FormatInvokeType (ReturnType);

		public string GetFileName ()
		{
			var sig = FormatJniSignature ().TrimStart ('(').Replace (')', '_');
			return $"{GetFullName ().Replace ('.', '_')}_{MethodName}_{sig}.g.cs";
		}

		public string GetFullName ()
		{
			if (string.IsNullOrEmpty (Namespace))
				return ClassName;

			return $"{Namespace}.{ClassName}";
		}
	}

	private class ParameterModel (string name, string type)
	{
		public string Name { get; } = name;
		public string Type { get; } = type;

		public override string ToString () => $"{Formatter.FormatCSharpType (Type)} {Name}";
	}
}
