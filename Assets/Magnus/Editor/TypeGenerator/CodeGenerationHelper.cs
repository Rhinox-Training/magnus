using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.Editor.TypeGenerator
{
    public class CodeGenerationHelper
    {
        private List<string> _lines;

        private const string REAL_TAB = "    ";

        private int _curIndent = 0;

        private enum GeneratorContext
        {
            FILE = 0,
            NAMESPACE,
            CLASS,
            METHOD
        }

        private GeneratorContext _context;
        private MethodBodyHelper _currentBodyHelper;

        public CodeGenerationHelper()
        {
            _lines = new List<string>();
            _curIndent = 0;
            _context = GeneratorContext.FILE;
        }
        
        public void CreateNamespace(string namespaceName)
        {
            if (_context > GeneratorContext.FILE)
                CloseNamespace();
            
            _lines.Add($"{GetIndent()}namespace {namespaceName.Trim()}");
            _lines.Add($"{GetIndent()}{{");

            ++_curIndent;
            _context = GeneratorContext.NAMESPACE;
        }

        public void CloseNamespace()
        {
            if (_context > GeneratorContext.NAMESPACE)
                CloseClass();
            
            _lines.Add($"{GetIndent()}}}");
            --_curIndent;
            _context = GeneratorContext.FILE;
        }


        public void OpenClass(string className, bool isStatic)
        {
            OpenClass(className, isStatic, Array.Empty<string>());
        }
        
        public void OpenClass(string className, bool isStatic, params Type[] baseTypes)
        {
            OpenClass(className, isStatic, baseTypes.Select(x => x.FullName).ToArray());
        }

        public void OpenClass(string className, bool isStatic, params string[] baseTypes)
        {
            
            _lines.Add($"{GetIndent()}public {(isStatic ? "static " : "")}class {className.Trim()}{(baseTypes.Length > 0 ? " : " + string.Join(", ", baseTypes) : "")}");
            _lines.Add($"{GetIndent()}{{");
            
            ++_curIndent;
            _context = GeneratorContext.CLASS;
        }
        
        public void CloseClass()
        {
            
            if (_context > GeneratorContext.CLASS)
                CloseMethod();
            
            _lines.Add($"{GetIndent()}}}");
            --_curIndent;
            _context = GeneratorContext.NAMESPACE;
        }

        public IImplementationBodyHelper OpenMethod(string methodName, bool isStatic, bool isPublic, Type[] arguments,
            string[] argNames, Type returnType = null, bool shouldWriteOverride = false)
        {
            if (_context == GeneratorContext.METHOD)
                CloseMethod();

            if (isStatic)
                shouldWriteOverride = false;
            
            string signature = $"{(isPublic ? "public" : "private")} {(isStatic ? "static " : "")}{(shouldWriteOverride ? "override " : "")}{(returnType != null && returnType != typeof(void) ? returnType.GetCSharpName() : "void")} {methodName.Trim()}(";
            for (int i = 0; i < arguments.Length; ++i)
            {
                signature += arguments[i].GetCSharpName() + " " + argNames[i];
                if (i < arguments.Length - 1)
                    signature += ", ";
            }

            signature += ")";
            
            AddRaw(signature);
            AddRaw("{");
            ++_curIndent;
            _context = GeneratorContext.METHOD;

            _currentBodyHelper = new MethodBodyHelper(this);
            return _currentBodyHelper;
        }

        public IImplementationBodyHelper OpenMethod(MethodInfo methodInfo)
        {
            bool shouldWriteOverride = !methodInfo.DeclaringType.IsInterface && (methodInfo.IsAbstract || methodInfo.IsVirtual);
            return OpenMethod(methodInfo.Name, methodInfo.IsStatic, methodInfo.IsPublic, methodInfo.GetParameters(), methodInfo.ReturnType, shouldWriteOverride: shouldWriteOverride);
        }

        public IImplementationBodyHelper OpenMethod(string methodName, bool isStatic, bool isPublic,
            ParameterInfo[] getParameters, Type returnType = null, bool shouldWriteOverride = false)
        {
            return OpenMethod(methodName, isStatic, isPublic, getParameters.Select(x => x.ParameterType).ToArray(),
                getParameters.Select(x => x.Name).ToArray(), returnType, shouldWriteOverride);
        }

        private void CloseMethod()
        {
            if (_context != GeneratorContext.METHOD)
                return;
            
            if (_currentBodyHelper != null)
                _currentBodyHelper.Close();
            _currentBodyHelper = null;
            --_curIndent;
            _context = GeneratorContext.CLASS;
        }

        private string GetIndent()
        {
            if (_curIndent <= 0)
                return String.Empty;
            return string.Join(string.Empty, Enumerable.Repeat(REAL_TAB, _curIndent));
        }

        public void AddRaw(string line)
        {
            _lines.Add($"{GetIndent()}{line}");
        }

        public void AddNewLine()
        {
            AddRaw(string.Empty);
        }

        public ICollection<string> ToLines()
        {
            if (_context >= GeneratorContext.CLASS)
                CloseNamespace();

            return _lines;
        }
        
        public class MethodBodyHelper : IImplementationBodyHelper
        {
            private readonly CodeGenerationHelper _root;
            private readonly MethodBodyHelper _parent;
            private List<MethodBodyHelper> _children;
            private bool _isClosed;

            private enum BodyType
            {
                If,
                // For
            }

            private BodyType _type;

            public MethodBodyHelper(CodeGenerationHelper root)
            {
                _root = root;
                _children = new List<MethodBodyHelper>();
            }

            public MethodBodyHelper(MethodBodyHelper currentBodyHelper)
            {
                _parent = currentBodyHelper;
                if (_parent != null)
                    _parent._children.Add(this);
                _root = currentBodyHelper._root;
            }

            public void AddRaw(string s)
            {                
                TryCloseLastActiveChild();
                _root.AddRaw(s);
            }

            public IImplementationBodyHelper CreateIfStatement(string s, bool stitchInIfElseBlock = false)
            {
                TryCloseLastActiveChild();
                // if (!_isClosed)
                //     Close();

                string ifStatement;
                if (stitchInIfElseBlock && HasPrecedingIf())
                    ifStatement = $"else if ({s})";
                else
                    ifStatement = $"if ({s})";
                AddRaw(ifStatement);
                AddRaw("{");

                ++_root._curIndent;
                _type = BodyType.If;
                
                var childHelper = new MethodBodyHelper(this);
                return childHelper;
            }

            private void TryCloseLastActiveChild()
            {
                if (_children != null)
                {
                    var child = _children.LastOrDefault();
                    if (child != null && !child._isClosed)
                        child.Close();
                }
            }

            private bool HasPrecedingIf()
            {
                if (_children.IsNullOrEmpty())
                    return false;

                var lastChild = _children.LastOrDefault();
                if (lastChild == null)
                    return false;

                return lastChild._type == BodyType.If;
            }

            // public IImplementationBodyHelper CreateForLoop(string indexer, string collection, bool isArray = true)
            // {
            //     throw new NotImplementedException();
            // }

            public void Close()
            {
                if (_isClosed)
                    return;
                
                if (_children != null)
                {
                    foreach (var child in _children)
                        child.Close();
                }

                --_root._curIndent;
                AddRaw("}");
                _isClosed = true;
            }
        }
    }

    public interface IImplementationBodyHelper
    {
        void AddRaw(string s);
        IImplementationBodyHelper CreateIfStatement(string s, bool stitchInIfElseBlock = false);
        // IImplementationBodyHelper CreateForLoop(string indexer, string collection, bool isArray = true);
    }
}