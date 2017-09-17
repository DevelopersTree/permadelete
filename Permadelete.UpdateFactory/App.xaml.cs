using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Permadelete.UpdateFactory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                var argsList = e.Args.Select(a => a.ToLower()).ToList();

                if (argsList.Contains("--help") || argsList.Contains("-help") || argsList.Contains("/?"))
                {
                    Console.WriteLine(@"
Permadelete UpdateFactory
Used to make update config files for Permadelete.

ufactory -source <source path> [-output <output path>] [-link <change list link>] [-version <version>] [-type <update type>] [-target <target name>]

Paramters:
-source     The full path of the folder that contains the binaries.

-output     the full path of the folder to put the publish output in.
            If not specified, then the output will be <source>\Publish

-link       The link to a webpage that contains the full changelist of the current version
            Default value is: https://github.com/encrypt0r/permadelete/releases

-version    The version of the update. Default value is 1.0.0.0

-type       The type of the update. Valid types: normal, critical

-target     The name of the main application. It's used to get the version in it's not supplied.

Example:
ufactory -source D:\permadelete\Permadelete.FrontEnd\bin\Release -version 0.5.1
ufactory -source D:\permadelete\Permadelete.FrontEnd\bin\Release -output D:\permadelete\publish -target Permadelete.exe
");
                    Current.Shutdown();
                    return;
                }

                var sourceIndex = argsList.IndexOf("-source");
                var linkIndex = argsList.IndexOf("-link");
                var versionIndex = argsList.IndexOf("-version");
                var targetIndex = argsList.IndexOf("-target");
                var outputIndex = argsList.IndexOf("-output");
                var typeIndex = argsList.IndexOf("-type");

                var source = GetIfExists(argsList, sourceIndex, null);
                if (source == null)
                {
                    Console.WriteLine("You need to specify source.");
                    Current.Shutdown();
                    return;
                }

                var link = GetIfExists(argsList, linkIndex, "https://github.com/encrypt0r/permadelete/releases");
                var version = GetIfExists(argsList, versionIndex, "1.0.0.0");
                var output = GetIfExists(argsList, outputIndex, UpdateConfigManger.GetUniformPath(source, @"\Publish"));
                var type = GetIfExists(argsList, typeIndex, "normal");
                var targetName = GetIfExists(argsList, targetIndex, null);

                if (targetName != null && version == "1.0.0.0")
                {
                    version = FileVersionInfo.GetVersionInfo(UpdateConfigManger.GetUniformPath(source, targetName)).FileVersion;
                }

                var viewModel = UpdateConfigManger.Load(source);
                viewModel.Version = version;
                viewModel.Path = "data";
                viewModel.Indented = false;
                viewModel.Link = link;
                viewModel.Type = type == "normal" ? Updater.UpdateType.Normal : Updater.UpdateType.Critical;

                var updateInfo = UpdateConfigManger.GetUpdateInfo(viewModel.Path, viewModel.Type, viewModel.Link, viewModel.Version);
                UpdateConfigManger.Write(output, viewModel.Files, viewModel.Indented ? Formatting.Indented : Formatting.None, updateInfo);

                Console.WriteLine("Published successfuly to: " + output);
                Current.Shutdown();
                return;
            }
            else
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        }

        private static string GetIfExists(List<string> args, int index, string fallback)
        {
            if (index == -1 || args.Count == index + 1)
                return fallback;
            else
                return args[index + 1];
        }
    }
}
