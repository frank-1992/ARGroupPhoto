using System;
using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Data Processors/Mesh Textured Renderer")]
    public class MvxMeshTexturedRenderer : MvxMeshRenderer
    {
		#region data

		public bool projectorEnabled = true;

		protected override void CreateMaterialInstances()
        {
            base.CreateMaterialInstances();
            if (materialInstances == null || materialInstances.Length == 0)
                return;

            foreach (Material materialInstance in materialInstances)
                materialInstance.SetTexture(TEXTURE_SHADER_PROPERTY_NAME, null);
        }

        #endregion

        #region process frame

        public static bool SupportsStreamRendering(Mvx2API.SourceInfo sourceInfo)
        {
            bool streamSupported =
                sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_POSITIONS_DATA_LAYER)
                && sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_INDICES_DATA_LAYER)
                && sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_UVS_DATA_LAYER)
                && SourceInfoContainsTexture(sourceInfo);
            return streamSupported;
        }

		//OneHamsa::Addition
		public static bool IsVertexColorMesh(Mvx2API.SourceInfo sourceInfo) {
			return sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_COLORS_DATA_LAYER)
				&& !SourceInfoContainsTexture(sourceInfo);
		}

		private static bool SourceInfoContainsTexture(Mvx2API.SourceInfo sourceInfo)
        {
            return sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.ASTC_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.DXT1_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.ETC2_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.NVX_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.NV12_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.NV21_TEXTURE_DATA_LAYER)
                || sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.RGB_TEXTURE_DATA_LAYER);
        }

        protected override bool CanProcessStream(Mvx2API.SourceInfo sourceInfo)
        {
            bool streamSupported = SupportsStreamRendering(sourceInfo);
            Debug.LogFormat("Mvx2: MeshTextured renderer {0} rendering of the new mvx stream", streamSupported ? "supports" : "does not support");
            return streamSupported;
        }

        protected override void ProcessNextFrame(MVCommon.SharedRef<Mvx2API.Frame> frame)
        {
            base.ProcessNextFrame(frame);
            CollectTextureDataFromFrame(frame.sharedObj);
        }

        protected override bool IgnoreColors()
        {
            return true;
        }

        #endregion

        #region texture

        private static readonly string TEXTURE_SHADER_PROPERTY_NAME = "_MainTex";
        private static readonly string TEXTURE_TYPE_SHADER_PROPERTY_NAME = "_TextureType";
        private static readonly string TEXTURE_WIDTH_PROPERTY_NAME = "_TextureWidth";
        private static readonly string TEXTURE_HEIGHT_PROPERTY_NAME = "_TextureHeight";
		//OneHamsa::Additions
		private static readonly string RANDOMIZER_SHADER_PROPERTY_NAME = "_Randomizer";
		private static readonly string PROJECTOR_ENABLED_SHADER_PROPERTY_NAME = "_ProjectorEnabled";

		private enum TextureTypeCodes
        {
            TTC_ASTC = 4,
            TTC_DXT1 = 3,
            TTC_ETC2 = 2,
            TTC_NVX = 0,
            TTC_RGB = 1,
            TTC_NV12 = 5,
            TTC_NV21 = 6
        };
		

        // an array of textures - they are switched between updates to improve performance -> textures double-buffering
        private Texture2D[] m_textures = new Texture2D[2];
        private int m_activeTextureIndex = -1;

        private void CollectTextureDataFromFrame(Mvx2API.Frame frame)
        {
            if (materialInstances == null || materialInstances.Length == 0)
                return;

            int textureType;
            TextureFormat textureFormat;
            FilterMode filterMode;
            Mvx2API.FrameTextureExtractor.TextureType mvxTextureType;

            if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.ASTC_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_ASTC;
                textureFormat = TextureFormat.ASTC_RGB_8x8;
                filterMode = FilterMode.Bilinear;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_ASTC;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.DXT1_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_DXT1;
                textureFormat = TextureFormat.DXT1;
                filterMode = FilterMode.Bilinear;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_DXT1;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.ETC2_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_ETC2;
                textureFormat = TextureFormat.ETC2_RGB;
                filterMode = FilterMode.Bilinear;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_ETC2;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.NVX_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_NVX;
                textureFormat = TextureFormat.Alpha8;
                filterMode = FilterMode.Bilinear;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_NVX;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.NV12_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_NV12;
                textureFormat = TextureFormat.Alpha8;
                filterMode = FilterMode.Point;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_NV12;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.NV21_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_NV21;
                textureFormat = TextureFormat.Alpha8;
                filterMode = FilterMode.Point;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_NV21;
            }
            else if (frame.StreamContainsDataLayer(Mvx2API.BasicDataLayersGuids.RGB_TEXTURE_DATA_LAYER))
            {
                textureType = (int) TextureTypeCodes.TTC_RGB;
                textureFormat = TextureFormat.RGB24;
                filterMode = FilterMode.Bilinear;
                mvxTextureType = Mvx2API.FrameTextureExtractor.TextureType.TT_RGB;
            }
            else
            {
                foreach (Material materialInstance in materialInstances)
                    materialInstance.SetTexture(TEXTURE_SHADER_PROPERTY_NAME, null);
                return;
            }

            ushort textureWidth, textureHeight;
            Mvx2API.FrameTextureExtractor.GetTextureResolution(frame, mvxTextureType, out textureWidth, out textureHeight);
            UInt32 textureSizeInBytes = Mvx2API.FrameTextureExtractor.GetTextureDataSizeInBytes(frame, mvxTextureType);
            IntPtr textureData = Mvx2API.FrameTextureExtractor.GetTextureData(frame, mvxTextureType);

            m_activeTextureIndex = (m_activeTextureIndex + 1) % m_textures.Length;
            Texture2D newActiveTexture = m_textures[m_activeTextureIndex];
            EnsureTextureProperties(ref newActiveTexture, textureFormat, filterMode, textureWidth, textureHeight);
            m_textures[m_activeTextureIndex] = newActiveTexture;

            newActiveTexture.LoadRawTextureData(textureData, (Int32)textureSizeInBytes);
            newActiveTexture.Apply(false, false);

            foreach (Material materialInstance in materialInstances)
            {
                materialInstance.SetInt(TEXTURE_TYPE_SHADER_PROPERTY_NAME, textureType);
                materialInstance.SetTexture(TEXTURE_SHADER_PROPERTY_NAME, newActiveTexture);
                materialInstance.SetInt(TEXTURE_WIDTH_PROPERTY_NAME, textureWidth);
                materialInstance.SetInt(TEXTURE_HEIGHT_PROPERTY_NAME, textureHeight);
				if (textureType == (int)TextureTypeCodes.TTC_NVX || textureType == (int)TextureTypeCodes.TTC_NV12)
					materialInstance.EnableKeyword("YUV");
				else
					materialInstance.DisableKeyword("YUV");
			}
        }

        private void EnsureTextureProperties(ref Texture2D texture, TextureFormat targetFormat, FilterMode filterMode, ushort targetWidth, ushort targetHeight)
        {
            if (texture == null
                || texture.format != targetFormat || texture.filterMode != filterMode
                || texture.width != targetWidth || texture.height != targetHeight)
            {
                texture = new Texture2D(targetWidth, targetHeight, targetFormat, false);
                texture.filterMode = filterMode;
            }
        }

		#endregion

		#region MonoBehaviour

		//OneHamsa::Additions
		public override void Update() {
			base.Update();
			if (materialInstances != null) {
				foreach (Material materialInstance in materialInstances) {
					if (materialInstance.HasProperty(PROJECTOR_ENABLED_SHADER_PROPERTY_NAME))
						materialInstance.SetFloat(PROJECTOR_ENABLED_SHADER_PROPERTY_NAME, projectorEnabled ? 1f : 0f);
						materialInstance.SetFloat(RANDOMIZER_SHADER_PROPERTY_NAME, UnityEngine.Random.Range(0.7f, 0.95f));
				}
			}
		}

		public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            Material defaultMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Plugins/Mvx2/Materials/MeshTextured.mat");
            if (defaultMaterial != null)
                materialTemplates = new Material[] { defaultMaterial };
#endif
        }

        #endregion
    }
}