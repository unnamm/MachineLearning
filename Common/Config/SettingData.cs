using Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Config
{
    [SettingMapper("Setting Name")]
    public class SettingData : YamlBase
    {
        [SettingMember("FolderPath", ConvertType.Text, @"C:\")]
        public string FolderPath { get; set; }

        [SettingMember("ModelName", ConvertType.Text, @".mlnet")]
        public string ModelName { get; set; }

        //[SettingMember("item2", ConvertType.Combo, ["select1", "select2"])]
        //public string? Mode { get; set; } = "select1";

        //[SettingMember("item3", ConvertType.Radio, ["select1", "select2"])]
        //public string? Third { get; set; }
    }
}
