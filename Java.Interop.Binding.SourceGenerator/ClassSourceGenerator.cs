using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Java.Interop.Binding.SourceGenerators;

[Generator]
public class ClassSourceGenerator : IIncrementalGenerator
{
	public void Initialize (IncrementalGeneratorInitializationContext context)
	{
		var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName (
		     fullyQualifiedMetadataName: "Java.Interop.Binding.SourceGenerator.JavaBindingTypeAttribute",
		     predicate: static (syntaxNode, cancellationToken) => syntaxNode is BaseTypeDeclarationSyntax,
		     transform: CreateModel);

		context.RegisterSourceOutput (pipeline, AddSource);
	}

	private static Model CreateModel (GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		var ns = context.TargetSymbol.GetNamespace ();
		var java_name = context.Attributes.Single (a => a.AttributeClass?.Name == "JavaBindingTypeAttribute").ConstructorArguments [0].Value?.ToString ();

		return new Model (ns, context.TargetSymbol.Name, java_name!);
	}

	private static void AddSource (SourceProductionContext context, Model model)
	{
		// static readonly JniPeerMembers _members = new XAPeerMembers ("android/app/Activity", typeof (Activity));
		var sb = new StringBuilder ();

		/*
		 // Metadata.xml XPath constructor reference: path="/api/package[@name='com.example']/class[@name='Mathinator']/constructor[@name='Mathinator' and count(parameter)=0]"
		[Register (".ctor", "()V", "")]
		public unsafe Mathinator () : base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
		{
			const string __id = "()V";

			if (((global::Java.Lang.Object) this).Handle != IntPtr.Zero)
				return;

			try {
				var __r = _members.InstanceMethods.StartCreateInstance (__id, ((object) this).GetType (), null);
				SetHandle (__r.Handle, JniHandleOwnership.TransferLocalRef);
				_members.InstanceMethods.FinishCreateInstance (__id, this, null);
			} finally {
			}
		}
	*/

		sb.AppendLine ("using System;");
		sb.AppendLine ("using Android.Runtime;");
		sb.AppendLine ("using Java.Interop;");
		sb.AppendLine ();
		sb.AppendLine ($"namespace {model.Namespace};");
		sb.AppendLine ();
		sb.AppendLine ($"[global::Android.Runtime.Register (\"{model.GetRegisterName ()}\", DoNotGenerateAcw=true)]");
		sb.AppendLine ($"partial class {model.ClassName}");
		sb.AppendLine ("{");
		sb.AppendLine ($"\tstatic readonly JniPeerMembers _members = new XAPeerMembers (\"{model.JavaType}\", typeof ({model.ClassName}));");
		sb.AppendLine ();
		sb.AppendLine ("\t[Register (\".ctor\", \"()V\", \"\")]");
		sb.AppendLine ($"\tpublic unsafe {model.ClassName} () : base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)");
		sb.AppendLine ("\t{");
		sb.AppendLine ("\t\tconst string __id = \"()V\";");
		sb.AppendLine ();
		sb.AppendLine ("\t\tif (((global::Java.Lang.Object) this).Handle != IntPtr.Zero)");
		sb.AppendLine ("\t\t\treturn;");
		sb.AppendLine ();
		sb.AppendLine ("\t\ttry {");
		sb.AppendLine ("\t\t\tvar __r = _members.InstanceMethods.StartCreateInstance (__id, ((object) this).GetType (), null);");
		sb.AppendLine ("\t\t\tSetHandle (__r.Handle, JniHandleOwnership.TransferLocalRef);");
		sb.AppendLine ("\t\t\t_members.InstanceMethods.FinishCreateInstance (__id, this, null);");
		sb.AppendLine ("\t\t} finally {");
		sb.AppendLine ("\t\t}");
		sb.AppendLine ("\t}");

		sb.AppendLine ("}");

		var sourceText = SourceText.From (sb.ToString (), Encoding.UTF8);

		context.AddSource ($"{model.GetFileName ()}", sourceText);
	}

	private class Model (string ns, string className, string javaName)
	{
		public string Namespace { get; } = ns;
		public string ClassName { get; } = className;
		public string JavaName { get; } = javaName;

		public string JavaType => JavaName.Replace ('.', '/');

		public string GetFileName ()
		{
			return $"{GetFullName ().Replace ('.', '_')}.g.cs";
		}

		public string GetFullName ()
		{
			if (string.IsNullOrEmpty (Namespace))
				return ClassName;

			return $"{Namespace}.{ClassName}";
		}

		public string GetRegisterName ()
		{
			if (string.IsNullOrEmpty (Namespace))
				return $"{ClassName.Replace ('.', '$')}";

			return $"{Namespace.Replace ('.', '/')}/{ClassName.Replace ('.', '$')}";
		}
	}
}
