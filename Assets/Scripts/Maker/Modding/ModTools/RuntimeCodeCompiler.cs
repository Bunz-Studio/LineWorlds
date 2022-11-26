using System;
using System.Text;
using System.Reflection;
using System.Security.Policy;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;
using ExternMaker;

public class AssemblyInAppDomain : IDisposable
{
    public AppDomain domain;
    public Proxy proxy;

    public Action<Assembly> onAssemblyCompiled;
    public Action<Exception> onAssemblyError;

    public AssemblyInAppDomain() { }
    public AssemblyInAppDomain(string[] codes, string[] references, string location = null)
    {
        Compile(codes, references, location);
    }

    public void CreateDomain()
    {
        AppDomainSetup domaininfo = new AppDomainSetup();
        domaininfo.ApplicationBase = Environment.CurrentDirectory;
        Evidence adevidence = AppDomain.CurrentDomain.Evidence;
        domain = AppDomain.CreateDomain("MyDomain", adevidence, domaininfo);
    }

    public void CreateProxyInstance()
    {
        // if (domain == null) CreateDomain();

        Type type = typeof(Proxy);
        proxy = new Proxy();
    }

    public Proxy.AssemblyResult Compile(string[] codes, string[] references, string location = null)
    {
        if (proxy == null) CreateProxyInstance();
        return proxy.CompileCode(codes, references, location);
    }

    public void Unload()
    {
        if(proxy != null) proxy.Dispose();
        if(domain != null) AppDomain.Unload(domain);
    }

    public void Dispose()
    {
        Unload();
    }
}

public class Proxy : MarshalByRefObject, IDisposable
{
    private static volatile Dictionary<string, Assembly> cache = new Dictionary<string, Assembly>();
    private static object syncRoot = new object();
    static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

    public Proxy()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
        {
            assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly;
        };
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            Assembly assembly = null;
            assemblies.TryGetValue(e.Name, out assembly);
            return assembly;
        };
    }

    public AssemblyResult CompileCode(string[] codes, string[] references, string location = null)
    {
        bool external = !string.IsNullOrWhiteSpace(location);
        CSharpCodeProvider provider = new CSharpCodeProvider();
        CompilerParameters compilerparams = new CompilerParameters();
        compilerparams.GenerateExecutable = false;
        compilerparams.GenerateInMemory = false;

        if(external) compilerparams.OutputAssembly = location;
        references = null;
        if (references != null && references.Length > 0)
        {
            foreach(var reference in references)
            {
                compilerparams.ReferencedAssemblies.Add(reference);
            }
        }
        else
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.FullName.Contains("YamlDotNet") || assembly.FullName.Contains("Newtonsoft")) continue;
                    string asmLocation = assembly.Location;
                    if (!string.IsNullOrEmpty(asmLocation))
                    {
                        compilerparams.ReferencedAssemblies.Add(asmLocation);
                    }
                }
                catch (NotSupportedException)
                {
                    // this happens for dynamic assemblies, so just ignore it. 
                }
            }
        }
        
        CompilerResults results = provider.CompileAssemblyFromSource(compilerparams, codes);
        // compiler.CompileAssemblyFromSource(compilerparams, code);

        if (results.Errors.HasErrors)
        {
            StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
            foreach (CompilerError error in results.Errors)
            {
                errors.AppendFormat("Line {0},{1}\t: {2}\n",
                       error.Line, error.Column, error.ErrorText);
            }
            return new AssemblyResult(new Exception(errors.ToString()));
        }
        else
        {
            AppDomain.CurrentDomain.Load(results.CompiledAssembly.GetName());
            return new AssemblyResult(results.CompiledAssembly);
        }
    }

    public Assembly CompileCodeOrGetFromCache(string[] codes, string key)
    {
        bool exists = cache.ContainsKey(key);

        if (!exists)
        {
            lock (syncRoot)
            {
                exists = cache.ContainsKey(key);

                if (!exists)
                {
                    cache.Add(key, CompileCode(codes, null).Assembly);
                }
            }
        }

        return cache[key];
    }

    public class AssemblyResult : EventArgs
    {
        public bool IsError { get; private set; }
        public Assembly Assembly { get; private set; }
        public Exception Exception { get; private set; }

        public AssemblyResult() { }

        public AssemblyResult(Exception ex)
        {
            Exception = ex;
            IsError = true;
        }

        public AssemblyResult(Assembly asm)
        {
            Assembly = asm;
            IsError = false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
