using System;

namespace BlazorDatasource.Shared.Domain.Custom
{
    public class Source : BaseEntity
    {
        public string Name { get; set; }
        public string SourceId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
