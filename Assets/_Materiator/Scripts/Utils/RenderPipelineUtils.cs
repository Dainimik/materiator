using UnityEngine.Rendering;

namespace Materiator
{
    public static class RenderPipelineUtils
    {
        public enum PipelineType
        {
            Unsupported,
            BuiltIn,
            Universal,
            HD
        }

        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        public static PipelineType GetActivePipelineType()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return PipelineType.HD;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return PipelineType.Universal;
                }
                else return PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null) {
                return PipelineType.Unsupported;
            }
#endif
            return PipelineType.BuiltIn;
        }
    }
}