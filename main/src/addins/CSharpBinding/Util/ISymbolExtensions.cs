﻿//
// ISymbolExtensions.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;

namespace ICSharpCode.NRefactory6.CSharp
{
	public static class ISymbolExtensions
	{
		

		public static bool IsDefinedInMetadata(this ISymbol symbol)
		{
			return symbol.Locations.Any (loc => loc.IsInMetadata);
		}

		public static bool IsDefinedInSource(this ISymbol symbol)
		{
			return symbol.Locations.All (loc => loc.IsInSource);
		}

		public static DeclarationModifiers GetSymbolModifiers(this ISymbol symbol)
		{
			// ported from roslyn source - why they didn't use DeclarationModifiers.From (symbol) ?
			return DeclarationModifiers.None
				.WithIsStatic (symbol.IsStatic)
				.WithIsAbstract (symbol.IsAbstract)
				.WithIsUnsafe (symbol.IsUnsafe ())
				.WithIsVirtual (symbol.IsVirtual)
				.WithIsOverride (symbol.IsOverride)
				.WithIsSealed (symbol.IsSealed);
		}

		public static IEnumerable<SyntaxReference> GetDeclarations(this ISymbol symbol)
		{
			return symbol != null
				? symbol.DeclaringSyntaxReferences.AsEnumerable()
					: SpecializedCollections.EmptyEnumerable<SyntaxReference>();
		}

	}
}

