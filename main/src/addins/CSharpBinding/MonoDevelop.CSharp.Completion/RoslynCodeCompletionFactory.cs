// 
// CSharpCompletionTextEditorExtension.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2011 Xamarin <http://xamarin.com>
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
using Microsoft.CodeAnalysis;
using MonoDevelop.Core;
using MonoDevelop.Ide.CodeCompletion;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.CSharp.Completion
{
	class RoslynCodeCompletionFactory : ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory
	{
		readonly CSharpCompletionTextEditorExtension ext;

		public RoslynCodeCompletionFactory (CSharpCompletionTextEditorExtension ext)
		{
			if (ext == null)
				throw new ArgumentNullException ("ext");
			this.ext = ext;
		}

		#region ICompletionDataFactory implementation

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateGenericData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, string data, ICSharpCode.NRefactory6.CSharp.Completion.GenericDataType genericDataType)
		{
			return new RoslynCompletionData (keyHandler) {
				CompletionText = data,
				DisplayText = data,
				Icon = "md-keyword"
			};
		}
		
		ICSharpCode.NRefactory6.CSharp.Completion.ISymbolCompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateEnumMemberCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, IFieldSymbol field)
		{
			var model = ext.ParsedDocument.GetAst<SemanticModel> ();
			return new RoslynSymbolCompletionData (keyHandler, ext, field, field.Type.ToMinimalDisplayString (model, ext.Editor.CaretOffset, SymbolDisplayFormat.CSharpErrorMessageFormat) + "." + field.Name);
		}
		
		class FormatItemCompletionData : RoslynCompletionData
		{
			string format;
			string description;
			object example;

			public FormatItemCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, string format, string description, object example) : base (keyHandler)
			{
				this.format = format;
				this.description = description;
				this.example = example;
			}

			public override string DisplayText {
				get {
					return format;
				}
			}

			public override string GetDisplayDescription (bool isSelected)
			{
				return "- <span foreground=\"darkgray\" size='small'>" + description + "</span>";
			}

			string rightSideDescription = null;
			public override string GetRightSideDescription (bool isSelected)
			{
				if (rightSideDescription == null) {
					try {
						rightSideDescription = "<span size='small'>" + string.Format ("{0:" +format +"}", example) +"</span>";
					} catch (Exception e) {
						rightSideDescription = "";
						LoggingService.LogError ("Format error.", e);
					}
				}
				return rightSideDescription;
			}

			public override string CompletionText {
				get {
					return format;
				}
			}

			public override int CompareTo (object obj)
			{
				return 0;
			}
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateFormatItemCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, string format, string description, object example)
		{
			return new FormatItemCompletionData (keyHandler, format, description, example);
		}

		class XmlDocCompletionData : RoslynCompletionData
		{
			//readonly CSharpCompletionTextEditorExtension ext;
			readonly string title;
			/*
			#region IListData implementation

			CSharpCompletionDataList list;
			public CSharpCompletionDataList List {
				get {
					return list;
				}
				set {
					list = value;
				}
			}

			#endregion*/

			public XmlDocCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, CSharpCompletionTextEditorExtension ext, string title, string description, string insertText) : base (keyHandler, title, "md-keyword", description, insertText ?? title)
			{
				// this.ext = ext;
				this.title = title;
			}
//			public override TooltipInformation CreateTooltipInformation (bool smartWrap)
//			{
//				var sig = new SignatureMarkupCreator (ext.Editor, ext.DocumentContext, ext.Editor.CaretOffset);
//				sig.BreakLineAfterReturnType = smartWrap;
//				return sig.GetKeywordTooltip (title, null);
//			}

			public override void InsertCompletionText (CompletionListWindow window, ref KeyActions ka, MonoDevelop.Ide.Editor.Extension.KeyDescriptor descriptor)
			{
				var currentWord = GetCurrentWord (window);
				var text = CompletionText;
				if (descriptor.KeyChar != '>')
					text += ">";
				window.CompletionWidget.SetCompletionText (window.CodeCompletionContext, currentWord, text);
			}
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateXmlDocCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, string title, string description, string insertText)
		{
			return new XmlDocCompletionData (keyHandler, ext, title, description, insertText);
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ISymbolCompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateSymbolCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, ISymbol symbol)
		{
			return new RoslynSymbolCompletionData (keyHandler, ext, symbol);
		}
		
		ICSharpCode.NRefactory6.CSharp.Completion.ISymbolCompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateSymbolCompletionData (ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, ISymbol symbol, string text)
		{
			return new RoslynSymbolCompletionData (keyHandler, ext, symbol, text);
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateNewOverrideCompletionData(ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, int declarationBegin, ITypeSymbol currentType, ISymbol m)
		{
			return new CreateOverrideCompletionData (keyHandler, ext, declarationBegin, currentType, m);
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreatePartialCompletionData(ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, int declarationBegin, ITypeSymbol currentType, IMethodSymbol method)
		{
			return new CreatePartialCompletionData (keyHandler, ext, declarationBegin, currentType, method);
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateAnonymousMethod(ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, string displayText, string description, string textBeforeCaret, string textAfterCaret)
		{
			return new AnonymousMethodCompletionData (keyHandler) {
				CompletionText = textBeforeCaret + "|" + textAfterCaret,
				DisplayText = displayText,
				Description = description
			};
		}

		ICSharpCode.NRefactory6.CSharp.Completion.ICompletionData ICSharpCode.NRefactory6.CSharp.Completion.ICompletionDataFactory.CreateNewMethodDelegate(ICSharpCode.NRefactory6.CSharp.Completion.ICompletionKeyHandler keyHandler, ITypeSymbol delegateType, string varName, INamedTypeSymbol curType)
		{
			return new EventCreationCompletionData (keyHandler, ext, delegateType, varName, curType);
		}
		#endregion
	}
}