using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public sealed class FloatShaderProperty : ShaderProperty
    {
        public string RName;
        //[Range(0, 1)]
        public float RValue;

        public string GName;
        //[Range(0, 1)]
        public float GValue;

        public string BName;
        //[Range(0, 1)]
        public float BValue;

        public string AName;
        //[Range(0, 1)]
        public float AValue;

        public FloatShaderProperty(string name, string propertyName, Vector4? value = null, string[] channelNames = null) : base(name, propertyName)
        {
            Vector4 val = Vector4.zero;

            if (value != null)
                val = (Vector4)value;
            
            RValue = val.x;
            GValue = val.y;
            BValue = val.z;
            AValue = val.w;

            if (channelNames == null)
            {
                channelNames = new string[4];
                for (int i = 0; i < channelNames.Length; i++)
                    channelNames[i] = "";
            }

            RName = channelNames[0];
            GName = channelNames[1];
            BName = channelNames[2];
            AName = channelNames[3];
        }
    }
}