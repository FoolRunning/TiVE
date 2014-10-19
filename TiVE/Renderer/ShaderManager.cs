using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class ShaderManager : IDisposable
    {
        private readonly Dictionary<string, IShaderProgram> shaderPrograms = new Dictionary<string, IShaderProgram>();

        public void Dispose()
        {
            foreach (IShaderProgram program in shaderPrograms.Values)
                program.Dispose();
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
        public IShaderProgram GetShaderProgram(string name)
        {
            IShaderProgram program;
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
                IShaderProgram currentProgram = null;

                while ((line = shaderDefinitionFile.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // Start of a new shader program
                        string programName = line.Substring(1, line.Length - 2);
                        if (string.IsNullOrEmpty(programName))
                            throw new ShaderDefinitionException("Program name can not be empty");

                        currentProgram = TiVEController.Backend.CreateShaderProgram();
                        AddProgram(programName, currentProgram);
                    }
                    else if (line.StartsWith("vert:"))
                    {
                        // Definition of the vertex shader for the current program
                        if (currentProgram == null)
                            throw new ShaderDefinitionException("Vertex shader definition ('vert') without program start ('[]')");
                        
                        string shaderPath = line.Substring(5).Trim();
                        if (string.IsNullOrEmpty(shaderPath))
                            throw new ShaderDefinitionException("Vertex shader definition path was empty");

                        currentProgram.AddShader(GetShaderSource(shaderPath), ShaderType.Vertex);
                    }
                    else if (line.StartsWith("frag:"))
                    {
                        // Definition of the fragment shader for the current program
                        if (currentProgram == null)
                            throw new ShaderDefinitionException("Fragment shader definition ('frag') without program start ('[]')");

                        string shaderPath = line.Substring(5).Trim();
                        if (string.IsNullOrEmpty(shaderPath))
                            throw new ShaderDefinitionException("Fragment shader definition path was empty");

                        currentProgram.AddShader(GetShaderSource(shaderPath), ShaderType.Fragment);
                    }
                    else if (line.StartsWith("attrib:"))
                    {
                        // Definition of a attribute for the current program
                        if (currentProgram == null)
                            throw new ShaderDefinitionException("Attribute definition without program start");

                        string attribName = line.Substring(7).Trim();
                        if (string.IsNullOrEmpty(attribName))
                            throw new ShaderDefinitionException("Attribute definition name was empty");

                        currentProgram.AddAttribute(attribName);
                    }
                    else if (line.StartsWith("uniform:"))
                    {
                        // Definition of a uniform variable for the current program
                        if (currentProgram == null)
                            throw new ShaderDefinitionException("Uniform definition without program start");

                        string uniformName = line.Substring(8).Trim();
                        if (string.IsNullOrEmpty(uniformName))
                            throw new ShaderDefinitionException("Uniform definition name was empty");

                        currentProgram.AddKnownUniform(uniformName);
                    }
                }
            }
        }

        private void AddProgram(string name, IShaderProgram program)
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
            {
                return reader.ReadToEnd();
            }
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

        private class ShaderDefinitionException : Exception
        {
            public ShaderDefinitionException(string message) : base(message)
            {
            }
        }
    }
}
