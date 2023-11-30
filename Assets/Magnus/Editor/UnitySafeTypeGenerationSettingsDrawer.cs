using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    [CustomPropertyDrawer(typeof(UnitySafeTypeGenerationSettings))]
    public class UnitySafeTypeGenerationSettingsDrawer : BasePropertyDrawer<UnitySafeTypeGenerationSettings>
    {
        private Regex _classNameValidator;
        
        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            var nextRect = CallInnerDrawer(position, label);

            if (GUI.Button(nextRect, "Generate Factory Class"))
                TryGenerateTypeFactoryClass();
            
            if (GUI.Button(nextRect.MoveDownLine(), "Auto-populate Factories"))
                SmartValue.PopulateFromCode();
        }
        
        private void TryGenerateTypeFactoryClass()
        {
            EditorInputDialog.Create("Metaprogramming Helper", "Generate a factory class for an open generic type...")
                .TextField("Class Name", out var classNameField, validation: ValidateClassName)
                .Dropdown("Open Generic Type", GetGenericTypeOptions(), out var genericTypeField)
                .OpenFolderField("Class Save Location", out var saveLocation)
                .OnAccept(() =>
                {
                    var genericTypeDefinition = genericTypeField.Value.Value as Type;
                    if (genericTypeDefinition == null || !genericTypeDefinition.IsGenericTypeDefinition)
                    {
                        PLog.Error<MagnusLogger>($"Something went wrong, type '{genericTypeDefinition?.GetCSharpName()}' was not an open generic type...");
                        return;
                    }
                    
                    var typeOptions = UnityGenericImplementationGenerator.GetDefaultTypeOptions().ToList();
                    DuplicateForCollections(ref typeOptions);
                    if (SmartValue.AdditionalTypes != null)
                        typeOptions.AddRange(SmartValue.AdditionalTypes.Select(x => x.Type).Where(x => x != null));
                    typeOptions = typeOptions.Distinct().ToList();
                    
                    var lines = UnityGenericImplementationGenerator.CreateFactoryClass(classNameField.Value, genericTypeDefinition, typeOptions);
                    if (lines.IsNullOrEmpty())
                    {
                        PLog.Warn<MagnusLogger>($"No class was generated, aborting...");
                        return;
                    }
                    
                    var outPath = Path.Combine(saveLocation.Value, $"{classNameField.Value}.cs");
                    File.WriteAllLines(outPath, lines);
        
                    PLog.Info<MagnusLogger>($"Code created @ {Path.GetFullPath(outPath)}");
                })
                .Show();
        }

        private void DuplicateForCollections(ref List<Type> typeOptions)
        {
            var singleTypes = typeOptions.Where(x => !x.IsArray && !x.ImplementsOpenGenericClass(typeof(List<>))).ToArray();
            foreach (var singleType in singleTypes)
            {
                var arrayType = singleType.MakeArrayType();
                if (!typeOptions.Contains(arrayType))
                    typeOptions.Add(arrayType);

                var listType = typeof(List<>).MakeGenericType(singleType);
                if (!typeOptions.Contains(listType))
                    typeOptions.Add(listType);
            }
        }

        private bool ValidateClassName(string potentialClassName)
        {
            if (potentialClassName == null)
                return false;

            if (_classNameValidator == null)
                _classNameValidator = new Regex("^[a-zA-Z_]{1}[a-zA-Z_0-9]*$");

            return _classNameValidator.IsMatch(potentialClassName);
        }

        private ICollection<ValueDropdownItem> GetGenericTypeOptions()
        {
            return TypeCache.GetTypesWithAttribute<GenericTypeGenerationAttribute>()
                .Select(x => new ValueDropdownItem(x.GetCSharpName(), x))
                .ToArray();
        }
    }
}