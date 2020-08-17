using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public sealed class FloatShaderProperty : ShaderProperty
    {
        public string RChannel;
        //[Range(0, 1)]
        public float R;

        public string GChannel;
        //[Range(0, 1)]
        public float G;

        public string BChannel;
        //[Range(0, 1)]
        public float B;

        public string AChannel;
        //[Range(0, 1)]
        public float A;

        public FloatShaderProperty(string propertyName, Vector4? value = null, string[] channelNames = null) : base(propertyName)
        {
            Vector4 val = Vector4.zero;

            if (value != null)
                val = (Vector4)value;
            
            R = val.x;
            G = val.y;
            B = val.z;
            A = val.w;

            if (channelNames == null)
            {
                channelNames = new string[4];
                for (int i = 0; i < channelNames.Length; i++)
                    channelNames[i] = "";
            }

            RChannel = channelNames[0];
            GChannel = channelNames[1];
            BChannel = channelNames[2];
            AChannel = channelNames[3];
        }
    }
}