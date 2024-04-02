using BlazorDatasource.Server.Models.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BlazorDatasource.Server.Helpers
{
    public class MainHelper
    {
        private readonly Random _random = new();

        public List<string> GetChartAxisLabels(List<Dictionary<string, object>> inputList, string? datasetName)
        {
            var result = new List<string>();

            foreach (var item in inputList)
            {
                foreach (var key in item.Keys)
                {
                    if (!string.Equals(key, datasetName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!result.Contains(key))
                        {
                            result.Add(key);
                        }
                    }
                }
            }

            return result;
        }

        public List<string> GetDatasetKeys(List<Dictionary<string, object>> inputList)
        {
            var result = new List<string>();

            foreach (var item in inputList)
            {
                foreach (var key in item.Keys)
                {
                    if (!result.Contains(key))
                    {
                        result.Add(key);
                    }
                }
            }

            return result;
        }

        public ChartDataMultiple GetChartDataMultiple(List<Dictionary<string, object>> inputList, string datasetName, string xAxis, string yAxis)
        {
            var result = new ChartDataMultiple();
            var yAxisList = new List<string>();
            var datasets = new List<Dictionary<string, object>>();

            // Create a list of time labels ordered
            foreach (var item in inputList)
            {
                foreach (var key in item.Keys)
                {
                    if (string.Equals(key, xAxis, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!result.Labels.Contains(item[key].ToString()!))
                        {
                            result.Labels.Add(item[key].ToString()!);
                        }
                    }
                    else if (string.Equals(key, yAxis, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!yAxisList.Contains(item[key].ToString()!))
                        {
                            yAxisList.Add(item[key].ToString()!);
                        }
                    }   
                }
            }

            result.Labels.Sort(); 

            foreach (var yAxisItem in yAxisList)
            {
                var dataset = new Dictionary<string, object>
                {
                    { "label", yAxisItem },
                    { "borderColor", GenerateRandomHexColor() }
                };

                var data = new List<decimal?>();

                foreach (var resultLabel in result.Labels)
                {
                    var founDictionary = inputList
                        .Where(item => 
                            item.ContainsKey(xAxis) && item[xAxis].ToString() == (resultLabel))
                        .FirstOrDefault(item => item.ContainsKey(yAxis) && item[yAxis].ToString() == yAxisItem);
                    
                   if (founDictionary != null)
                    {
                        decimal.TryParse(
                            founDictionary[datasetName].ToString(), 
                            NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, 
                            CultureInfo.InvariantCulture, 
                            out var chartDataTupleValue);
                        data.Add(chartDataTupleValue);
                    }
                    else
                    {
                        data.Add(null);
                    }
                }
                dataset.Add("data", data);
                datasets.Add(dataset);
            }

            result.Data = datasets;
            return result;
        }

        public string GenerateRandomHexColor()
        {
            return $"#{_random.Next(0x1000000):X6}"; 
        }
    }
}
