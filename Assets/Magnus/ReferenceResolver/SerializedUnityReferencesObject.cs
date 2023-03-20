using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
#if ODIN_INSPECTOR
using Sirenix.Serialization;
using Sirenix.Utilities;
#endif
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    public static class SerializedUnityReferencesObjectManager
    {
        public struct ResolverInfo
        {
            public ReferenceResolverAttribute Attribute;
            public MethodInfo Method;

            public Type DeclaringType => Method.DeclaringType;
            public int Order => Attribute.Order;

            public ResolverInfo(MethodInfo mi)
            {
                Attribute = mi.GetCustomAttribute<ReferenceResolverAttribute>();
                Method = mi;
            }

            public static bool IsValid(MethodInfo mi)
            {
                var attr = mi.GetCustomAttribute<ReferenceResolverAttribute>();
                if (attr == null) return false;
                
                if (mi.ReturnType != typeof(bool))
                {
                    PLog.Error<MagnusLogger>($"'{mi.DeclaringType}::{mi.Name}' has an invalid usage of ReferenceResolver. It must return a boolean.");
                    return false;
                }
                
                if (!mi.HasParameters(MethodTypes))
                {
                    PLog.Error<MagnusLogger>($"'{mi.DeclaringType}::{mi.Name}' has an invalid usage of ReferenceResolver. " +
                                             $"It must have the following parameters: UnityEngine.Object, out IObjectReferenceResolver");
                    return false;
                }

                return true;
            }
        }
        
        private static ResolverInfo[] Encoders;

        private static Type[] MethodTypes = new[] { typeof(UnityEngine.Object), typeof(IObjectReferenceResolver).MakeByRefType() };
        
        public static IObjectReferenceResolver TryEncode(UnityEngine.Object o, bool allowEditorResolvers = false)
        {
            BuildCacheIfNeeded();

            if (o == null) return null;

            foreach (var encoder in Encoders)
            {
                if (!allowEditorResolvers && encoder.Attribute.IsEditorOnly)
                    continue;
                
                var parameters = new object[] {
                    o,
                    null
                };
                
                bool result = (bool) encoder.Method.Invoke(null, parameters);
                
                if (result)
                    return (IObjectReferenceResolver) parameters[1];
            }

            return null;
        }

        private static void BuildCacheIfNeeded()
        {
            if (Encoders != null) return;
            
            Encoders = ReflectionUtility.GetTypesInheritingFrom(typeof(IObjectReferenceResolver))
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                .Where(x => ResolverInfo.IsValid(x))
                .Select(x => new ResolverInfo(x))
                .OrderBy(x => x.Order)
                .ToArray();
        }
    }
    
#if ODIN_INSPECTOR
    public static class SerializedUnityReferencesObject
    {
        private static readonly Encoding DEFAULT_ENCODING = Encoding.UTF8;
        
        private const string HEADER_START_JSON =   "$$RHIN_TXT_HEADER$$"; // NOTE: header should be of fixed length (for both)
        private const string HEADER_START_BINARY = "$$RHIN_BIN_HEADER$$"; // NOTE: header should be of fixed length (for both)
        private static int HEADER_START_LENGTH => DEFAULT_ENCODING.GetBytes(HEADER_START_BINARY).Length;
        private const string HEADER_END = "$$END_RHINOX_HEADER$$";
        private static int HEADER_END_LENGTH => DEFAULT_ENCODING.GetBytes(HEADER_END).Length;
#if UNITY_EDITOR
        
        public static byte[] Pack<T>(T container, DataFormat format = DataFormat.JSON)
        {
            if (format == DataFormat.Nodes)
                throw new InvalidOperationException("DataFormat of type Nodes not supported.");
            
            var objects = new List<Object>();
            var bytes = SerializationUtility.SerializeValue(container, format, out objects);

            var externalReferences = new List<IObjectReferenceResolver>();
            foreach (var o in objects)
            {
                var resolver = SerializedUnityReferencesObjectManager.TryEncode(o);
                externalReferences.Add(resolver);
            }
            
            byte[] headerBytes = SerializationUtility.SerializeValue(externalReferences, format);

            byte[] headerStart = DEFAULT_ENCODING.GetBytes(format == DataFormat.JSON ? HEADER_START_JSON : HEADER_START_BINARY);
            byte[] headerEnd = DEFAULT_ENCODING.GetBytes(HEADER_END);
            bytes = ByteHelper.Merge(headerStart, headerBytes, headerEnd, bytes);
        
            return bytes;
        }
#endif

        public static T Unpack<T>(byte[] bytes)
        {
            if (!TryUnpack(bytes, out T unpackedData, out string error))
                throw new InvalidOperationException($"[SerializedUnityReferencesObject] {error}");
            return unpackedData;
        }

        public static bool TryUnpack<T>(byte[] bytes, out T unpackedData, out string error)
        {
            unpackedData = default;
            
            if (CollectionExtensions.IsNullOrEmpty(bytes))
            {
                error = "ByteStream is empty";
                return false;
            }
            
            if (!CheckHeader(bytes, out DataFormat format))
            {
                error = "ByteStream does not contain a header";
                return false;
            }

            unpackedData = default(T);
            if (!FindHeaderEnd(bytes, out int endMarkerStartAddress))
            {
                error = "ByteStream does not contain end header";
                return false;
            }

            int headerLength = endMarkerStartAddress - HEADER_START_LENGTH;
            byte[] header = new byte[headerLength];
            Array.Copy(bytes, HEADER_START_LENGTH, header, 0, headerLength);

            int dataStartIndex = endMarkerStartAddress + HEADER_END_LENGTH;
            int dataLength = bytes.Length - dataStartIndex;

            if (dataLength < 0)
            {
                error = "ByteStream had no data";
                return false;
            }
            
            byte[] data = new byte[dataLength];
            Array.Copy(bytes, dataStartIndex, data, 0, dataLength);
            
            SceneHierarchyTree.Freeze(); // (to prevent multiple iterations)
            var sw = Stopwatch.StartNew();
            var externalReferences = SerializationUtility.DeserializeValue<List<IObjectReferenceResolver>>(header, format);
            var list = new List<Object>();
            foreach (var resolver in externalReferences)
            {
                var obj = resolver?.Resolve();
                list.Add(obj);
            }
            sw.Stop();
            PLog.Info<MagnusLogger>($"Resolving references took: {sw.ElapsedMilliseconds}ms");
            SceneHierarchyTree.UnFreeze();
            
            unpackedData = SerializationUtility.DeserializeValue<T>(data, format, list);
            error = null;
            return true;
        }
        
        private static bool CheckHeader(byte[] bytes, out DataFormat format)
        {
            if (bytes == null || bytes.Length < HEADER_START_LENGTH + 5)
            {
                format = DataFormat.Nodes;
                return false;
            }
            
            byte[] jsonHeader = DEFAULT_ENCODING.GetBytes(HEADER_START_JSON);
            byte[] binaryHeader = DEFAULT_ENCODING.GetBytes(HEADER_START_BINARY);
            int jsonCount = 0, binaryCount = 0;
            for (int i = 0; i < HEADER_START_LENGTH; ++i)
            {
                if (bytes[i] == jsonHeader[i])
                    ++jsonCount;

                if (bytes[i] == binaryHeader[i])
                    ++binaryCount;
            }

            if ((jsonCount < HEADER_START_LENGTH && binaryCount < HEADER_START_LENGTH) || jsonCount == binaryCount)
            {
                format = DataFormat.Nodes;
                return false;
            }

            format = jsonCount == HEADER_START_LENGTH ? DataFormat.JSON : DataFormat.Binary;
            return true;
        }

        private static bool FindHeaderEnd(byte[] bytes, out int headerEndStart)
        {
            byte[] endMarker = DEFAULT_ENCODING.GetBytes(HEADER_END);
            int endMarkerStartAddress = -1;
            bool isEndMarker = false;
            for (int i = HEADER_START_LENGTH; i < bytes.Length; ++i)
            {
                if (i + endMarker.Length > bytes.Length - 1)
                    break;
                isEndMarker = true;
                for (int j = 0; j < endMarker.Length; ++j)
                {
                    if (endMarker[j] != bytes[i + j])
                    {
                        isEndMarker = false;
                        break;
                    }
                }

                if (isEndMarker)
                {
                    endMarkerStartAddress = i;
                    break;
                }
            }

            headerEndStart = endMarkerStartAddress;
            return headerEndStart != -1;
        }
    }
#endif
}