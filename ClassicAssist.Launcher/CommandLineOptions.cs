using CommandLine;

namespace ClassicAssist.Launcher
{
    public class CommandLineOptions
    {
        [Option( "shard", Required = false )]
        public string Shard { get; set; }
    }
}