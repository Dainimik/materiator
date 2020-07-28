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

            return PipelineType.BuiltIn;
        }
    }
}