﻿using System;
using System.Collections.Generic;
using antlr;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using System.IO;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        public BooSource(Service service, Microsoft.VisualStudio.TextManager.Interop.IVsTextLines buffer, Colorizer colorizer)
            : base(service, buffer, colorizer)
        { }

        private CompilerContext compileResult;

        public CompilerContext CompileResult
        {
            get
            {
                lock (this)
                {
                    return compileResult;
                }
            }
        }

        private readonly List<Node> nodes = new List<Node>();

        private class AstWalker : DepthFirstTransformer
        {
            public AstWalker(BooSource source)
            {
                this.source = source;
            }

            private readonly BooSource source;

            protected override void OnNode(Node node)
            {
                if (node.EndSourceLocation != null)
                    source.nodes.Add(node);
                base.OnNode(node);
            }

            protected override void OnError(Node node, Exception error)
            {
                // Do Nothing
            }
        }

        //internal IEnumerable<Node> GetNodes(int line, int pos)
        //{
        //    foreach (var node in nodes)
        //    {
        //        if (node.LexicalInfo == null) continue;
        //        if (node.LexicalInfo.Line > line) continue;
        //        if (node.LexicalInfo.Line == line && node.LexicalInfo.Column > pos) continue;
        //        if (node.EndSourceLocation.Line < line) continue;
        //        if (node.EndSourceLocation.Line == line && node.EndSourceLocation.Column < pos) continue;
        //        yield return node;
        //    }
        //}

        internal bool IsBlockComment(int line, IToken token)
        {
            lock (this)
            {
                if (compileResult == null)
                    return false; // do not know yet
                //foreach (var t in tokens)
                //    if (t.getColumn() == token.getColumn()
                //        && t.getLine() == line
                //        && t.getText() == token.getText()
                //        )
                        return false;
                //return true;
            }
        }

        private readonly List<IToken> tokens = new List<IToken>();

        internal void Compile(BooCompiler compiler, ParseRequest req)
        {
            lock (this)
            {
                tokens.Clear();
                IToken token;
                var lexer = BooParser.CreateBooLexer(1, "code stream", new StringReader(req.Text));
                while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                    tokens.Add(token);

                compileResult = compiler.Run(BooParser.ParseString("code", req.Text));

                nodes.Clear();
                new AstWalker(this).Visit(compileResult.CompileUnit);
            }
            Recolorize(0, this.GetLineCount()-1);
        }
    }
}
