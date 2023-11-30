using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Magnus.Editor.TypeGenerator;
using Rhinox.Perceptor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UnityGenericImplementationGenerator
{
    public static ICollection<string> CreateFactoryClass(string className, Type typeDefinition, ICollection<Type> typeGeneratorOptions)
    {
        if (typeDefinition == null || !typeDefinition.IsGenericTypeDefinition ||
            typeDefinition.GetGenericArguments().Length != 1)
        {
            PLog.Error<MagnusLogger>($"Factory class creation failed...");
            return Array.Empty<string>();
        }
        
        var codeGen = new CodeGenerationHelper();
        var genericTypeGen = new UnityGenericGenerator(typeDefinition);
        
        codeGen.CreateNamespace(typeDefinition.Namespace);
        foreach (var innerType in typeGeneratorOptions)
        {
            if (innerType == null || innerType.IsGenericTypeDefinition)
                continue;
            string typeImplDefinition = genericTypeGen.GetTypeImplementation(innerType);
            codeGen.AddRaw(typeImplDefinition);
        }
        
        codeGen.AddNewLine();
        codeGen.OpenClass(className, false, typeof(BaseUnitySafeTypeFactory));

        MethodInfo mi = typeof(BaseUnitySafeTypeFactory).GetMethod(nameof(BaseUnitySafeTypeFactory.BuildGenericType));
        var methodBuilder = codeGen.OpenMethod(mi);
        
        var parameters = mi.GetParameters();
        methodBuilder.AddRaw($"var type = genericTypeDefinition.MakeGenericType({parameters[0].Name});");
        foreach (var option in genericTypeGen.GetInnerTypeOptions())
        {
            var stringOption = genericTypeGen.GetTypeName(option);
            var bodyBuilder = methodBuilder.CreateIfStatement($"type.IsAssignableFrom(typeof({stringOption}))", true);
            string body = $"return new {stringOption}();";
            bodyBuilder.AddRaw(body);
        }
        methodBuilder.AddRaw("return null;");

        var lines = codeGen.ToLines();
        return lines;
    }

    public static ICollection<Type> GetDefaultTypeOptions()
    {
        return new[]
        {
            typeof(bool),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(Transform),
            typeof(GameObject),
            typeof(string),
            typeof(Button),
            typeof(long),
            typeof(byte),
            typeof(short),
            typeof(char),
            typeof(Collider),
            typeof(Audio),
            typeof(Bounds),
            typeof(Camera),
        };
    }
}
