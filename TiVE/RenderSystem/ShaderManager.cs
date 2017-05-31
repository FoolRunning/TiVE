using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal sealed class ShaderManager : IDisposable
    {
        private readonly Dictionary<string, ShaderProgram> shaderPrograms = new Dictionary<string, ShaderProgram>();

        public void Dispose()
        {
            foreach (ShaderProgram program in shaderPrograms.Values)
                program.Dispose();
            shaderPrograms.Clear();
        }

        public bool Initialize()
        {
            Messages.Print("Loading shaders...");
            List<string> warnings = new List<string>();
            try
            {
                ParseShaderDefinitionFile(TiVEController.Backend.GetShaderDefinitionFileResourcePath());
            }
            catch (ShaderDefinitionException e)
            {
                warnings.Add(e.Message);
            }

            if (shaderPrograms.Count != 0)
                Messages.AddDoneText();
            else
            {
                Messages.AddFailText();
                Messages.AddWarning("No shader definitions were given by the backend");
            }

            foreach (string warning in warnings)
                Messages.AddWarning(warning);

            return shaderPrograms.Count > 0;
        }

        /// <summary>
        /// Gets the shader program with the specified name. If the shader has not been initialize, it will be initialized before being returned.
        /// </summary>
        /// <exception cref="ArgumentException">If a shader with the specified name could not be found.</exception>
        public ShaderProgram GetShaderProgram(string name)
        {
            ShaderProgram program;
            if (!shaderPrograms.TryGetValue(name, out program))
                throw new ArgumentException("Shader with the name '" + name + "' was not found");

            if (!program.IsInitialized)
                program.Initialize();

            return program;
        }

        private void ParseShaderDefinitionFile(string resourcePath)
        {
            using (StreamReader shaderDefinitionFile = GetFileResourceStream(resourcePath))
            {
                string line;
                ShaderProgram currentProgram = null;

                while ((line = shaderDefinitionFile.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // Start of a new shader program
                        string programName = line.Substring(1, line.Length - 2);
                        if (string.IsNullOrEmpty(programName))
                            throw new ShaderDefinitionException("Program name can not be empty");

                        currentProgram = TiVEController.Backend.CreateShaderProgram();
                        AddProgram(programName, currentProgram);
                        continue;
                    }

                    int colonIndex = line.IndexOf(':');
                    if (colonIndex == -1)
                        throw new ShaderDefinitionException("Unknown line: " + line);

                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    if (currentProgram == null)
                        throw new ShaderDefinitionException("Key ('" + key + "') without program start ('[]')");

                    if (value == "")
                        throw new ShaderDefinitionException("Value for key ('" + key + "') was empty");

                    switch (key)
                    {
                        case "geom": currentProgram.AddShader(GetShaderSource(value), ShaderType.Geometry); break;
                        case "vert": currentProgram.AddShader(GetShaderSource(value), ShaderType.Vertex); break;
                        case "frag": currentProgram.AddShader(GetShaderSource(value), ShaderType.Fragment); break;
                        case "attrib": currentProgram.AddAttribute(value); break;
                        case "uniform": currentProgram.AddKnownUniform(value); break;
                        case "lightUniform":
                            for (int i = 0; i < SceneLightData.MaxLightsPerChunk; i++)
                            {
                                currentProgram.AddKnownUniform(value + "[" + i + "].location");
                                currentProgram.AddKnownUniform(value + "[" + i + "].color");
                                currentProgram.AddKnownUniform(value + "[" + i + "].cachedValue");
                            }
                            break;
                        default: throw new ShaderDefinitionException("Unknown line: " + line);
                    }
                }
            }
        }

        private void AddProgram(string name, ShaderProgram program)
        {
            try
            {
                shaderPrograms.Add(name, program);
            }
            catch (ArgumentException)
            {
                throw new ShaderDefinitionException("Shader with the name '" + name + "' has already been added");
            }
        }

        private static string GetShaderSource(string fullShaderResourcePath)
        {
            using (StreamReader reader = GetFileResourceStream(fullShaderResourcePath))
                return reader.ReadToEnd();
        }

        private static StreamReader GetFileResourceStream(string fullResourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream;
            try
            {
                stream = assembly.GetManifestResourceStream(fullResourcePath);
            }
            catch
            {
                stream = null;
            }

            if (stream == null)
                throw new ShaderDefinitionException("Unable to load resource " + fullResourcePath);

            return new StreamReader(stream);
        }

        private sealed class ShaderDefinitionException : Exception
        {
            public ShaderDefinitionException(string message) : base(message)
            {
            }
        }
    }
}
