﻿using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProjectEye.Core.Models.Statistic
{
    [XmlRootAttribute("StatisticList")]
    public class StatisticListModel
    {
        public List<StatisticModel> Data { get; set; }
    }
}
