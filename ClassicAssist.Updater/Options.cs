using CommandLine;
using System;

namespace ClassicAssist.Updater
{
    public enum UpdaterStage
    {
        Initial,
        Install
    }

    public class Options
    {
        [Option("version", Required = false)]
        public Version CurrentVersion { get; set; }

        [Option("force", Required = false, Default = false)]
        public bool Force { get; set; }

        [Option("path", Required = false, Default = "")]
        public string Path { get; set; }

        [Option("pid", Required = false, Default = 0)]
        public int PID { get; set; }

        [Option("stage", Default = UpdaterStage.Initial, Required = false)]
        public UpdaterStage Stage { get; set; }

        [Option("updatepath", Required = false, Default = "")]
        public string UpdatePath { get; set; }
    }
}