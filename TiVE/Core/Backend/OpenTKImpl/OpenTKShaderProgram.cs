﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal sealed class OpenTKShaderProgram : ShaderProgram
    {

        private static readonly string[] lightLocationUniformNames = new string[SceneLightData.MaxLightsPerChunk];
        private static readonly string[] lightColorUniformNames = new string[SceneLightData.MaxLightsPerChunk];
        private static readonly string[] lightCachedValueUniformNames = new string[SceneLightData.MaxLightsPerChunk];
        private readonly List<Shader> shaders = new List<Shader>();
        private readonly Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
        private readonly List<string> attributes = new List<string>();

        private int programId;

        static OpenTKShaderProgram()
        {
            for (int i = 0; i < SceneLightData.MaxLightsPerChunk; i++)
            {
                lightLocationUniformNames[i] = "lights[" + i + "].location";
                lightColorUniformNames[i] = "lights[" + i + "].color";
                lightCachedValueUniformNames[i] = "lights[" + i + "].cachedValue";
            }
        }

        ~OpenTKShaderProgram()
        {
            Messages.Assert(programId == 0, "Shader program was not properly deleted");
        }

        public override void Dispose()
        {
            // Shaders are deleted after attempting to compile

            if (programId != 0)
                GL.DeleteProgram(programId);
            programId = 0;

            GC.SuppressFinalize(this);
        }

        public override bool IsInitialized
        {
            get { return programId != 0; }
        }

        public override void AddShader(string shaderSource, ShaderType shaderType)
        {
            shaders.Add(new Shader(shaderSource, shaderType));
        }

        public override void AddAttribute(string name)
        {
            attributes.Add(name);
        }

        public override void AddKnownUniform(string name)
        {
            uniformLocations.Add(name, -1);
        }

        public override void Bind()
        {
            GL.UseProgram(programId);
        }

        public override void Unbind()
        {
            GL.UseProgram(0);
        }

        public override bool Initialize()
        {
            programId = GL.CreateProgram();

            bool success = true;
            shaders.ForEach(s => success &= s.Compile());

            if (success)
            {
                shaders.ForEach(s => GL.AttachShader(programId, s.ShaderId));

                for (int i = 0; i < attributes.Count; i++)
                    GL.BindAttribLocation(programId, i, attributes[i]);

                GL.LinkProgram(programId);

                string info;
                GL.GetProgramInfoLog(programId, out info);
                if (!string.IsNullOrEmpty(info))
                    Console.WriteLine(info);

                int linkResult;
                GL.GetProgram(programId, ProgramParameter.LinkStatus, out linkResult);
                success = (linkResult == 1);
                GlUtils.CheckGLErrors();
            }

            if (success)
            {
                foreach (string uniform in uniformLocations.Keys.ToList()) // Make copy of the keys so we can change the dictionary
                {
                    int location = GL.GetUniformLocation(programId, uniform);
                    if (location < 0)
                        Messages.AddWarning("Unable to find uniform " + uniform + " in program");
                    uniformLocations[uniform] = location;
                }
                GlUtils.CheckGLErrors();
            }

            shaders.ForEach(s => s.Dispose());
            return success;
        }

        public override void SetUniform(string name, ref Matrix4f value)
        {
            unsafe
            {
                fixed (float* ptr = &value.Row0X)
                {
                    GL.UniformMatrix4(uniformLocations[name], 1, false, ptr);
                }
            }
            GlUtils.CheckGLErrors();
        }

        public override void SetUniform(string name, ref Vector3f value)
        {
            GL.Uniform3(uniformLocations[name], value.X, value.Y, value.Z);
            GlUtils.CheckGLErrors();
        }

        public override void SetUniform(string name, ref Color3f value)
        {
            GL.Uniform3(uniformLocations[name], value.R, value.B, value.G);
            GlUtils.CheckGLErrors();
        }

        public override void SetUniform(string name, ref Color4f value)
        {
            GL.Uniform4(uniformLocations[name], value.R, value.B, value.G, value.A);
            GlUtils.CheckGLErrors();
        }

        public override void SetUniform(string name, RenderedLight[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                GL.Uniform3(uniformLocations[lightLocationUniformNames[i]], value[i].Location.X, value[i].Location.Y, value[i].Location.Z);
                GL.Uniform3(uniformLocations[lightColorUniformNames[i]], value[i].Color.R, value[i].Color.G, value[i].Color.B);
                GL.Uniform1(uniformLocations[lightCachedValueUniformNames[i]], value[i].CachedValue);
            }
            GlUtils.CheckGLErrors();
        }

        public override void SetUniform(string name, int value)
        {
            GL.Uniform1(uniformLocations[name], value);
            GlUtils.CheckGLErrors();
        }
    }
}
