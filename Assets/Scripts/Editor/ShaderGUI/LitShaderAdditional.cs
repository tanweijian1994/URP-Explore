using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGUI;

namespace Game.Editor.ShaderGUI
{
    internal sealed class LitShaderAdditional : BaseShaderGUI
    {
        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;
        private MaterialProperty[] _customAdditionalProperties;
        private SavedBool m_DetailInputsFoldout;
        private SavedBool _customAdditionalInputFoldout;

        private new static class Styles
        {
            public static readonly GUIContent AdditionalInputs = new GUIContent("Custom Additional Properties", "Custom Additional Properties");
        }

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            base.OnOpenGUI(material, materialEditor);
            m_DetailInputsFoldout = new SavedBool($"{headerStateKey}.DetailInputsFoldout", true);
            _customAdditionalInputFoldout = new SavedBool($"{headerStateKey}.CustomAdditionalInputsFoldout", true);
        }

        public override void DrawAdditionalFoldouts(Material material)
        {
            m_DetailInputsFoldout.value =
                EditorGUILayout.BeginFoldoutHeaderGroup(m_DetailInputsFoldout.value, LitDetailGUI.Styles.detailInputs);
            if (m_DetailInputsFoldout.value)
            {
                LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            _customAdditionalInputFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_customAdditionalInputFoldout.value, Styles.AdditionalInputs);
            if (_customAdditionalInputFoldout.value)
            {
                DrawCustomAdditionalProperties(_customAdditionalProperties);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            litDetailProperties = new LitDetailGUI.LitProperties(properties);
            _customAdditionalProperties = new MaterialProperty[properties.Length - 45];
            for (var i = 0; i < _customAdditionalProperties.Length; i++)
            {
                _customAdditionalProperties[i] = properties[45 + i];  // 默认取后面的作为额外参数
            }
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            if (litProperties.workflowMode != null)
            {
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode,
                    Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            }

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material) obj);
            }

            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
                if (EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }

            material.SetFloat("_Surface", (float) surfaceType);
            material.SetFloat("_Blend", (float) blendMode);

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float) LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float) LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }

            MaterialChanged(material);
        }

        private void DrawCustomAdditionalProperties(IEnumerable<MaterialProperty> properties)
        {
            foreach (var property in properties)
            {
                if ((property.flags & MaterialProperty.PropFlags.HideInInspector) == MaterialProperty.PropFlags.HideInInspector)
                {
                    continue;
                }

                switch (property.type)
                {
                    case MaterialProperty.PropType.Color:
                        materialEditor.ColorProperty(EditorGUILayout.GetControlRect(true, 20), property, property.displayName);
                        break;
                    case MaterialProperty.PropType.Vector:
                        materialEditor.VectorProperty(EditorGUILayout.GetControlRect(true, 50), property, property.displayName);
                        break;
                    case MaterialProperty.PropType.Float:
                        materialEditor.FloatProperty(EditorGUILayout.GetControlRect(true, 20), property, property.displayName);
                        break;
                    case MaterialProperty.PropType.Range:
                        materialEditor.RangeProperty(EditorGUILayout.GetControlRect(true, 20), property, property.displayName);
                        break;
                    case MaterialProperty.PropType.Texture:
                        materialEditor.TexturePropertySingleLine(new GUIContent(property.displayName), property);
                        if (property.textureValue != null && (property.flags & MaterialProperty.PropFlags.NoScaleOffset) != MaterialProperty.PropFlags.NoScaleOffset)
                        {
                            materialEditor.TextureScaleOffsetProperty(property);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}