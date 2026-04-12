using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Blur/Blur (Fastest)")]
    public class BlurFastest : PostEffectsBase
    {

        [Range(0, 4)]
        public int downsample = 1;

        public enum BlurType
        {
            StandardGauss = 0,
            SgxGauss = 1,
        }

        [Range(0.0f, 10.0f)]
        public float blurSize = 3.0f;

        [Range(1, 4)]
        public int blurIterations = 2;
        [Range(0,128)]
        public int blurFrames = 32;

        public BlurType blurType = BlurType.StandardGauss;

        public Shader blurShader = null;
        private Material blurMaterial = null;

        private RenderTexture _LastRenderTexutre;
        private int _RenderedFrame = 0;


        public override bool CheckResources()
        {
            CheckSupport(false);

            blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        public void OnDisable()
        {
            if (blurMaterial)
                DestroyImmediate(blurMaterial);
            if (_LastRenderTexutre != null)
            {
                RenderTexture.ReleaseTemporary(_LastRenderTexutre);
                _LastRenderTexutre = null;
            }
            _RenderedFrame = 0;
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (_RenderedFrame >= blurFrames * 2)
            {
                if (_LastRenderTexutre != null)
                {
                    Graphics.Blit(_LastRenderTexutre, destination);
                }
                else
                {
                    Graphics.Blit(source,destination);
                }
               
                return;
            }

            _RenderedFrame ++;

            float widthMod = 1.0f / (1.0f * (1 << downsample));

            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
            source.filterMode = FilterMode.Bilinear;

            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;

            // downsample
            RenderTexture rt = null;
            if(_LastRenderTexutre == null)
            {
                rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            }
            else
            {
                rt = _LastRenderTexutre;
            }

            rt.filterMode = FilterMode.Bilinear;
            if (_LastRenderTexutre == null)
            {
                Graphics.Blit(source, rt, blurMaterial, 0);
            }

            var passOffs = blurType == BlurType.StandardGauss ? 0 : 2;

            float iterationOffs = 0;
            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            if (_RenderedFrame%2 == 0)
            {
                // vertical blur
                Graphics.Blit(rt, rt2, blurMaterial, 1 + passOffs);
            }
            else
            {
                // horizontal blur
                Graphics.Blit(rt, rt2, blurMaterial, 2 + passOffs);
            }
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            Graphics.Blit(rt, destination);

            _LastRenderTexutre = rt;
        }
    }
}

