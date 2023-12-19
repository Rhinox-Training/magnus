using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.Editor.TypeGenerator
{
    public class UnityGenericGenerator
    {
        private Dictionary<Type, string> _typeNameCache;
        private Dictionary<Type, string> _typeImplementation;
        private Type _openGenericType;

        public ICollection<Type> GetInnerTypeOptions()
        {
            return _typeNameCache != null ? (ICollection<Type>) _typeNameCache.Keys : Array.Empty<Type>();
        }

        public UnityGenericGenerator(Type openGenericType)
        {
            if (openGenericType == null || !openGenericType.IsGenericType || !openGenericType.IsGenericTypeDefinition || openGenericType.GetGenericArguments().Length != 1)
                throw new ArgumentException(nameof(openGenericType));

            _openGenericType = openGenericType;
            _typeNameCache = new Dictionary<Type, string>();
            _typeImplementation = new Dictionary<Type, string>();
        }

        public string GetTypeName(Type innerType)
        {
            if (_typeNameCache.ContainsKey(innerType))
                return _typeNameCache[innerType];

            GenerateDataForType(innerType);
            return _typeNameCache[innerType];
        }

        public string GetTypeImplementation(Type innerType)
        {
            if (_typeImplementation.ContainsKey(innerType))
                return _typeImplementation[innerType];

            GenerateDataForType(innerType);
            return _typeImplementation[innerType];
        }

        private void GenerateDataForType(Type innerType)
        {
            string typeName = $"{SanitizeTypeName(innerType)}_{SanitizeTypeName(_openGenericType)}_Generated";
            _typeNameCache.Add(innerType, typeName);
            string typeImplDefinition =
                $"public {(_openGenericType.IsClass ? "class" : "struct")} {typeName} : {_openGenericType.GetCSharpName().RemoveLast("<>")}<{innerType.GetCSharpName()}> {{ }}";
            _typeImplementation.Add(innerType, typeImplDefinition);
        }

        private static string SanitizeTypeName(Type innerType)
        {
            return innerType.GetCSharpName()
                .Replace(".", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("[]", "_ARRAY_");
        }

        public bool GenericConstraintMatchesType(Type innerType)
        {
            var genericArg = _openGenericType.GetGenericArguments().FirstOrDefault();
            if (genericArg == null)
                return true;
            return innerType.InheritsFrom(genericArg.BaseType);
        }
    }
}