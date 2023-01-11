using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithread_JPEG_Codec;

internal static class ConfigurationHandler
{
    public static Dictionary<string, string> ConfigureArgs(this Dictionary<string, string> keyValuePairs, string[] args)
    {
        if(args.Length % 2 != 0) throw new ArgumentException("argument aren't in pairs"); 
        Dictionary<string, string> result = new Dictionary<string, string>();
        
        
        for(int i = 0; i < args.Length; i += 2)
        {
            string key = args[i];
            string value = args[i + 1];
            if (!key.StartsWith("--")) throw new ArgumentException("keys of values always starts with '--' prefix");
            key = key.TrimStart('-');
            result.Add(key, value);
        }
        return result;
    }
}
