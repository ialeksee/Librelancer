﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibreLancer.Shaders
{
    using System;
    
    public class Particle
    {
        static ShaderVariables[] variants;
        private static bool iscompiled = false;
        public static ShaderVariables Get(LibreLancer.Graphics.RenderContext device, ShaderFeatures features)
        {
            AllShaders.Compile(device);
            return variants[0];
        }
        public static ShaderVariables Get(LibreLancer.Graphics.RenderContext device)
        {
            AllShaders.Compile(device);
            return variants[0];
        }
        internal static void Compile(LibreLancer.Graphics.RenderContext device, string sourceBundle)
        {
            if (iscompiled)
            {
                return;
            }
            iscompiled = true;
            ShaderVariables.Log("Compiling Particle");
            variants = new ShaderVariables[1];
            // No GL4 variants detected
            variants[0] = ShaderVariables.Compile(device, sourceBundle.Substring(333191, 708), sourceBundle.Substring(244636, 236), sourceBundle.Substring(333899, 2127));
        }
    }
}
