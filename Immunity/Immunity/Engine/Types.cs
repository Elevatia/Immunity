using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Immunity.Engine
{
    public class Types
    {
        public static string local_player = "engine.localplayer";
        public static string system_settings = "systemsettings";
        public static string engine_engine = "engine.engine";
        public static string renderer_settings = "engine.renderersettings";
        public static string engine_network_settings = "engine.networksettings";
        public static string script = "/script/";

        private static Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "engine.localplayer", local_player },
            { "systemsettings", system_settings },
            { "engine.engine", engine_engine },
            { "engine.renderersettings", renderer_settings },
            { "engine.networksettings", engine_network_settings },
            { "script", script }
        };

        public static string get(string value, bool script)
        {
            string configuration = "";

            if (script == true)
            {
                configuration = data["script"];
            }
            configuration += data[value];

            return (configuration);
        }
    }
}
