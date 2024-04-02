using System;
using System.Collections.Generic;

namespace BlazorDatasource.Server.Models.Common
{
    public class ChartDataMultiple
    {
        public Guid Id { get; set; } = new();   
        public List<string> Labels { get; set; } = new();
        public List<Dictionary<string, object>> Data { get; set; } = new();
    }
}
