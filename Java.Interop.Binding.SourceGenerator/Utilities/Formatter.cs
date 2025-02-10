using Microsoft.CodeAnalysis;

namespace Java.Interop.Binding.SourceGenerators;

static class Formatter
{
	public static string FormatCSharpType (string type)
	{
		return type switch {
			"System.Byte" => "byte",
			"System.Boolean" => "bool",
			"System.Char" => "char",
			"System.Double" => "double",
			"System.Single" => "float",
			"System.Int32" => "int",
			"System.Int64" => "long",
			"System.Int16" => "short",
			"System.Void" => "void",
			_ => throw new InvalidOperationException ($"Type '{type}' is not supported yet, only primitive types."),
		};
	}

	public static string FormatJniType (string type)
	{
		return type switch {
			"System.Byte" => "B",
			"System.Boolean" => "Z",
			"System.Char" => "C",
			"System.Double" => "D",
			"System.Single" => "F",
			"System.Int32" => "I",
			"System.Int64" => "J",
			"System.Int16" => "S",
			"System.Void" => "V",
			_ => throw new InvalidOperationException ($"Type '{type}' is not supported yet, only primitive types."),
		};
	}

	public static string FormatInvokeType (string type)
	{
		return type switch {
			"System.Byte" => "SByte",
			"System.Boolean" => "Boolean",
			"System.Char" => "Char",
			"System.Double" => "Double",
			"System.Single" => "Single",
			"System.Int32" => "Int32",
			"System.Int64" => "Int64",
			"System.Int16" => "Int16",
			"System.Void" => "Void",
			_ => throw new InvalidOperationException ($"Type '{type}' is not supported yet, only primitive types."),
		};
	}

	public static string GetFullName (this ISymbol symbol)
	{
		var declaring_type = symbol.ContainingType;

		if (declaring_type is null) {
			var ns = symbol.ContainingNamespace.ToDisplayString ();

			if (string.IsNullOrEmpty (ns))
				return symbol.Name;

			return $"{ns}.{symbol.Name}";
		}

		return $"{GetFullName (declaring_type)}.{symbol.Name}";
	}

	public static string GetNamespace (this ISymbol symbol)
	{
		var declaring_type = symbol.ContainingType;

		if (declaring_type is null)
			return symbol.ContainingNamespace.ToDisplayString ();

		return GetNamespace (declaring_type);
	}
}
