﻿using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MuleSoft.RAML.Tools.Properties;
using NuGet.VisualStudio;
using Raml.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MuleSoft.RAML.Tools
{
    public class ReverseEngineeringService
    {
        private readonly string nugetPackagesSource = Settings.Default.NugetPackagesSource;
        private readonly string ramlWebApiExplorerPackageId = Settings.Default.RAMLWebApiExplorerPackageId;
        private readonly string ramlWebApiExplorerPackageVersion = Settings.Default.RAMLWebApiExplorerPackageVersion;
        private readonly string ramlParserPackageId = Settings.Default.RAMLParserPackageId;
        private readonly string ramlParserPackageVersion = Settings.Default.RAMLParserPackageVersion;
        private readonly string ramlApiCorePackageId = Settings.Default.RAMLApiCorePackageId;
        private readonly string ramlApiCorePackageVersion = Settings.Default.RAMLApiCorePackageVersion;
        private readonly string newtonsoftJsonPackageId = Settings.Default.NewtonsoftJsonPackageId;
        private readonly string newtonsoftJsonPackageVersion = Settings.Default.NewtonsoftJsonPackageVersion;
        private readonly string edgePackageId = Settings.Default.EdgePackageId;
        private readonly string edgePackageVersion = Settings.Default.EdgePackageVersion;

        private readonly IServiceProvider serviceProvider;
        

        public ReverseEngineeringService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }


        public void AddReverseEngineering()
        {
            try
            {
                ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "Enable RAML metadata output process started");
                var dte = serviceProvider.GetService(typeof(SDTE)) as DTE;
                var proj = VisualStudioAutomationHelper.GetActiveProject(dte);

                InstallNugetAndDependencies(proj);
                ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "Nuget packages and dependencies installed");

                if (!VisualStudioAutomationHelper.IsAVisualStudio2015Project(proj))
                {
                    AddXmlCommentsDocumentation(proj);
                    ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                        "XML comments documentation added");
                }
                else
                {
                    ConfigureNetCoreStartUp(proj);
                    ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                        "StatUp configuration added");

                    AddCoreContentFiles(Path.GetDirectoryName(proj.FullName));
                    ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                       "Content files added");
                }
            }
            catch (Exception ex)
            {
                ActivityLog.LogError(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                    VisualStudioAutomationHelper.GetExceptionInfo(ex));
                MessageBox.Show("Error when trying to enable RAML metadata output. " + ex.Message);
                throw;
            }
        }

        private void AddCoreContentFiles(string destinationPath)
        {
            var extensionPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var sourcePath = Path.Combine(extensionPath, "MetadataPackage" + Path.DirectorySeparatorChar + "Content");
            AddRamlController(sourcePath, destinationPath);
            AddViews(sourcePath, destinationPath);
            AddWebContent(sourcePath, destinationPath);
        }

        private void AddWebContent(string sourcePath, string destinationPath)
        {
            var webRoot = "wwwroot";
            CopyFilesRecursively(sourcePath, destinationPath, webRoot);
        }

        private void AddViews(string sourcePath, string destinationPath)
        {
            var subfolder = "Views" + Path.DirectorySeparatorChar + "Raml";
            CopyFilesRecursively(sourcePath, destinationPath, subfolder);
        }

        private static void CopyFilesRecursively(string sourcePath, string destinationPath, string subfolder)
        {
            var viewsSourcePath = Path.Combine(sourcePath, subfolder);
            var viewDestinationPath = Path.Combine(destinationPath, subfolder);

            CopyFilesRecusively(viewsSourcePath, viewDestinationPath);
        }

        private static void CopyFilesRecusively(string sourcePath, string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            var sourceFilePaths = Directory.GetFiles(sourcePath);
            foreach (var sourceFilePath in sourceFilePaths)
            {
                var destFileName = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));
                if (!File.Exists(destFileName))
                    File.Copy(sourceFilePath, destFileName, false);
            }

            // Copy sub folders
            var sourceSubFolders = Directory.GetDirectories(sourcePath);
            foreach (var sourceSubFolder in sourceSubFolders)
            {
                var lastDirectory = GetLastDirectory(sourceSubFolder);
                CopyFilesRecusively(sourceSubFolder, Path.Combine(destinationPath, lastDirectory));
            }
        }

        private static string GetLastDirectory(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            var index = path.LastIndexOf(Path.DirectorySeparatorChar);
            return path.Substring(index + 1);
        }

        private static void AddRamlController(string sourcePath, string destinationPath)
        {
            var controllersFolder = "Controllers";

            var controllersDestPath = Path.Combine(destinationPath, controllersFolder + Path.DirectorySeparatorChar);
            if (!Directory.Exists(controllersDestPath))
                Directory.CreateDirectory(controllersDestPath);

            var controllersPath = Path.Combine(sourcePath, controllersFolder);
            var ramlControllerDest = Path.Combine(controllersDestPath, "RamlController.cs");
            File.Copy(Path.Combine(controllersPath, "RamlController.class"), ramlControllerDest);
        }

        private void ConfigureNetCoreStartUp(Project proj)
        {
            var startUpPath = Path.Combine(Path.GetDirectoryName(proj.FullName), "Startup.cs");
            if (!File.Exists(startUpPath)) return;

            var lines = File.ReadAllLines(startUpPath).ToList();

            ConfigureNetCoreMvcServices(lines);

            ConfigureNetCoreMvc(lines);

            File.WriteAllText(startUpPath, string.Join(Environment.NewLine, lines));            
        }

        private void AddXmlCommentsDocumentation(Project proj)
        {
            ConfigureXmlDocumentationFileInProject(proj);
            AddIncludeXmlCommentsInWebApiConfig(proj);
        }

        private static void AddIncludeXmlCommentsInWebApiConfig(Project proj)
        {
            var appStart = proj.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name == "App_Start");
            if (appStart == null) return;

            var webApiConfig = appStart.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name == "WebApiConfig.cs");
            if (webApiConfig == null) return;

            var path = webApiConfig.FileNames[0];
            var lines = File.ReadAllLines(path).ToList();

            if (lines.Any(l => l.Contains("DocumentationProviderConfig.IncludeXmlComments")))
                return;

            InsertLine(lines);

            File.WriteAllText(path, string.Join(Environment.NewLine, lines));
        }

        private void ConfigureNetCoreMvc(List<string> lines)
        {
            var appUsestaticfiles = "            app.UseStaticFiles();";

            if (lines.Any(l => l.Contains("app.UseStaticFiles();")))
                return;

            var line = FindLineWith(lines, "public void Configure(IApplicationBuilder app");
            if (line > 0)
                lines.Insert(line + 2, appUsestaticfiles);
        }

        private void ConfigureNetCoreMvcServices(List<string> lines)
        {
            var addService = "            services.AddScoped<RAML.WebApiExplorer.ApiExplorerDataFilter>();";

            int line;
            if (!lines.Any(l => l.Contains("services.AddMvc")))
            {
                line = FindLineWith(lines, "public void ConfigureServices");
                lines.Insert(line + 2, addService);
                lines.Insert(line + 2, AddMvcWithOptions());
                return;
            }

            line = FindLineWith(lines, "services.AddMvc()");
            if (line > 0)
            {
                lines.Insert(line -1, addService);
                lines.RemoveAt(line);
                lines.Insert(line, AddMvcWithOptions());
                return;
            }

            line = FindLineWith(lines, "services.AddMvc(options =>");
            if (line > 0 && lines[line + 1] == "{")
            {
                lines.Insert(line + 1, addService);
                lines.Insert(line + 2, AddOptions());
            }
        }

        private static string AddMvcWithOptions()
        {
            return "            services.AddMvc(options =>" + Environment.NewLine
                   + "                {" + Environment.NewLine
                   + AddOptions()
                   + "                });";
        }

        private static string AddOptions()
        {
            return "                    options.Filters.AddService(typeof(RAML.WebApiExplorer.ApiExplorerDataFilter));" + Environment.NewLine
                   + "                    options.Conventions.Add(new RAML.WebApiExplorer.ApiExplorerVisibilityEnabledConvention());" + Environment.NewLine
                   + "                    options.Conventions.Add(new RAML.WebApiExplorer.ApiExplorerVisibilityDisabledConvention(typeof(RAML.WebApiExplorer.RamlController)));" + Environment.NewLine;
        }

        private static void InsertLine(List<string> lines)
        {
            var line = FindLineWith(lines, "Register(HttpConfiguration config)");
            var inserted = false;
            if (line != -1)
            {
                if (lines[line + 1].Contains("{"))
                {
                    lines.Insert(line + 2, "\t\t\tRAML.WebApiExplorer.DocumentationProviderConfig.IncludeXmlComments();");
                    inserted = true;
                }
            }

            if (!inserted)
            {
                line = FindLineWith(lines, ".MapHttpAttributeRoutes();");
                if (line != -1)
                    lines.Insert(line + 1, "\t\t\tRAML.WebApiExplorer.DocumentationProviderConfig.IncludeXmlComments();");
            }
        }

        private static int FindLineWith(IReadOnlyList<string> lines, string find)
        {
            var line = -1;
            for (var i = 0; i < lines.Count(); i++)
            {
                if (lines[i].Contains(find))
                    line = i;
            }
            return line;
        }

        private static void ConfigureXmlDocumentationFileInProject(Project proj)
        {
            var config = proj.ConfigurationManager.ActiveConfiguration;
            var configProps = config.Properties;
            var prop = configProps.Item("DocumentationFile");
            prop.Value = string.Format("bin\\{0}.XML", proj.Name);
        }



        private void InstallNugetAndDependencies(Project proj)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            var installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            var installer = componentModel.GetService<IVsPackageInstaller>();

            var packs = installerServices.GetInstalledPackages(proj).ToArray();

            if (VisualStudioAutomationHelper.IsAVisualStudio2015Project(proj))
            {
                InstallNetCoreDependencies(proj, packs, installer, installerServices);
            }
            else
            {
                InstallWebApiDependencies(proj, packs, installer, installerServices);
            }
        }

        private void InstallNetCoreDependencies(Project proj, IVsPackageMetadata[] packs, IVsPackageInstaller installer, IVsPackageInstallerServices installerServices)
        {
            NugetInstallerHelper.InstallPackageIfNeeded(proj, packs, installer, "Microsoft.AspNetCore.StaticFiles",
                            "1.0.0", Settings.Default.NugetExternalPackagesSource);

            // RAML.Parser
            if (!installerServices.IsPackageInstalled(proj, "RAML.Parser.Expressions"))
            {
                installer.InstallPackage(nugetPackagesSource, proj, "RAML.Parser.Expressions", "1.0.0", false);
            }

            // RAML.NetCoreApiExplorer
            if (!installerServices.IsPackageInstalled(proj, "RAML.NetCoreApiExplorer"))
            {
                installer.InstallPackage(nugetPackagesSource, proj, "RAML.NetCoreApiExplorer", "1.0.0", false);
            }
        }

        private void InstallWebApiDependencies(Project proj, IVsPackageMetadata[] packs, IVsPackageInstaller installer,
            IVsPackageInstallerServices installerServices)
        {
            NugetInstallerHelper.InstallPackageIfNeeded(proj, packs, installer, newtonsoftJsonPackageId,
                newtonsoftJsonPackageVersion, Settings.Default.NugetExternalPackagesSource);
            NugetInstallerHelper.InstallPackageIfNeeded(proj, packs, installer, edgePackageId, edgePackageVersion,
                Settings.Default.NugetExternalPackagesSource);
            NugetInstallerHelper.InstallPackageIfNeeded(proj, packs, installer, "System.ComponentModel.Annotations",
                "4.0.0", Settings.Default.NugetExternalPackagesSource);

            // RAML.Parser
            if (!installerServices.IsPackageInstalled(proj, ramlParserPackageId))
            {
                installer.InstallPackage(nugetPackagesSource, proj, ramlParserPackageId, ramlParserPackageVersion,
                    false);
            }

            // RAML.Api.Core
            if (!installerServices.IsPackageInstalled(proj, ramlApiCorePackageId))
            {
                //installer.InstallPackage(nugetPackagesSource, proj, ramlApiCorePackageId, ramlApiCorePackageVersion, false);
                installer.InstallPackage(nugetPackagesSource, proj, ramlApiCorePackageId, ramlApiCorePackageVersion,
                    false);
            }

            // RAML.WebApiExplorer
            if (!installerServices.IsPackageInstalled(proj, ramlWebApiExplorerPackageId))
            {
                installer.InstallPackage(nugetPackagesSource, proj, ramlWebApiExplorerPackageId,
                    ramlWebApiExplorerPackageVersion, false);
            }
        }

        public void ExtractRAML()
        {
            throw new NotImplementedException();
        }

        public void RemoveReverseEngineering()
        {
            try
            {
                ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "Disable RAML metadata output process started");
                var dte = serviceProvider.GetService(typeof(SDTE)) as DTE;
                var proj = VisualStudioAutomationHelper.GetActiveProject(dte);

                UninstallNugetAndDependencies(proj);
                ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "Nuget package uninstalled");

                if (VisualStudioAutomationHelper.IsAVisualStudio2015Project(proj))
                {
                    RemoveNetCoreStartUpConfiguration(proj);
                }
                else
                {
                    RemovXmlCommentsDocumentation(proj);
                    ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                        "XML comments documentation removed");
                }
            }
            catch (Exception ex)
            {
                ActivityLog.LogError(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource,
                    VisualStudioAutomationHelper.GetExceptionInfo(ex));
                MessageBox.Show("Error when trying to disable RAML metadata output. " + ex.Message);
                throw;
            }
        }

        private void UninstallNugetAndDependencies(Project proj)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            var installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            var installer = componentModel.GetService<IVsPackageUninstaller>();

            // Uninstall RAML.WebApiExplorer
            if (installerServices.IsPackageInstalled(proj, ramlWebApiExplorerPackageId))
            {
                installer.UninstallPackage(proj, ramlWebApiExplorerPackageId, false);
            }

            // RAML.NetCoreApiExplorer
            if (installerServices.IsPackageInstalled(proj, "RAML.NetCoreApiExplorer"))
            {
                installer.UninstallPackage(proj, "RAML.NetCoreApiExplorer", false);
            }
        }

        private void RemovXmlCommentsDocumentation(Project proj)
        {
            RemoveXmlCommentsInWebApiConfig(proj);
            RemoveXmlDocumentationFileInProject(proj);
        }

        private static void RemoveXmlCommentsInWebApiConfig(Project proj)
        {
            var appStart = proj.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name == "App_Start");
            if (appStart == null) return;

            var webApiConfig = appStart.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name == "WebApiConfig.cs");
            if (webApiConfig == null) return;

            var path = webApiConfig.FileNames[0];
            var content = File.ReadAllText(path);

            if (!content.Contains("DocumentationProviderConfig.IncludeXmlComments"))
                return;

            content = content.Replace("RAML.WebApiExplorer.DocumentationProviderConfig.IncludeXmlComments();", string.Empty);

            File.WriteAllText(path, content);
        }

        private static void RemoveXmlDocumentationFileInProject(Project proj)
        {
            var config = proj.ConfigurationManager.ActiveConfiguration;
            var configProps = config.Properties;
            var prop = configProps.Item("DocumentationFile");
            prop.Value = string.Empty;
        }

        private void RemoveNetCoreStartUpConfiguration(Project proj)
        {
            var startUpPath = Path.Combine(Path.GetDirectoryName(proj.FullName), "Startup.cs");
            if (!File.Exists(startUpPath)) return;

            var lines = File.ReadAllLines(startUpPath).ToList();

            var addService = "            services.AddScoped<RAML.WebApiExplorer.ApiExplorerDataFilter>();";
            RemoveLine(lines, addService);

            var appUsestaticfiles = "            app.UseStaticFiles();";
            RemoveLine(lines, appUsestaticfiles);

            var option1 = "                    options.Filters.AddService(typeof(RAML.WebApiExplorer.ApiExplorerDataFilter));";
            RemoveLine(lines, option1);

            var option2 = "                    options.Conventions.Add(new RAML.WebApiExplorer.ApiExplorerVisibilityEnabledConvention());";
            RemoveLine(lines, option2);

            var option3 = "                    options.Conventions.Add(new RAML.WebApiExplorer.ApiExplorerVisibilityDisabledConvention(typeof(RAML.WebApiExplorer.RamlController)));";
            RemoveLine(lines, option3);

            File.WriteAllText(startUpPath, string.Join(Environment.NewLine, lines));
        }

        private static void RemoveLine(List<string> lines, string content)
        {
            var line = FindLineWith(lines, content);
            if (line > 0)
                lines.RemoveAt(line);
        }
    }
}