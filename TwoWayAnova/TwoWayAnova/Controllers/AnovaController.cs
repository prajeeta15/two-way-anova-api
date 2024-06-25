using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace TwoWayAnovaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnovaController : ControllerBase
    {
        // GET api/values
        [HttpPost("upload")]
        public IActionResult FilterOutLiers(IFormFile jsonfile)
        {
            try
            {

                if (jsonfile == null || jsonfile.Length == 0)          //error handling if file not found
                {

                    return BadRequest("Invalid found.");
                }

                using var stream = new StreamReader(jsonfile.OpenReadStream());
                string json = stream.ReadToEnd(); // read the JSON file

                // deserialize JSON data into a list of the model objects 
                var data = JsonConvert.DeserializeObject<List<AnovaData>>(json);

                if (data == null || !data.Any())
                {
                    return BadRequest("No data available in the JSON file.");
                }

                // calculate means and standard deviations
                var meanA1 = data.Average(item => item.Attribute1);
                var meanA2 = data.Average(item => item.Attribute2);
                var meanA3 = data.Average(item => item.Attribute3);

                var stdDevA1 = CalculateStandardDeviation(data.Select(item => item.Attribute1).ToList());
                var stdDevA2 = CalculateStandardDeviation(data.Select(item => item.Attribute2).ToList());
                var stdDevA3 = CalculateStandardDeviation(data.Select(item => item.Attribute3).ToList());

                // defining range for outliers
                var lowerLimitA1 = meanA1 - 2 * stdDevA1;
                var upperLimitA1 = meanA1 + 2 * stdDevA1;

                var lowerLimitA2 = meanA2 - 2 * stdDevA2;
                var upperLimitA2 = meanA2 + 2 * stdDevA2;

                var lowerLimitA3 = meanA3 - 2 * stdDevA3;
                var upperLimitA3 = meanA3 + 2 * stdDevA3;

                // filter outliers
                var outliers = data.Where(item =>
                    item.Attribute1 < lowerLimitA1 || item.Attribute1 > upperLimitA1 ||
                    item.Attribute2 < lowerLimitA2 || item.Attribute2 > upperLimitA2 ||
                    item.Attribute3 < lowerLimitA3 || item.Attribute3 > upperLimitA3
                ).ToList();

                // data without outliers
                var dataWithoutOutliers = data.Except(outliers).ToList();

                // convert the list to a datatable for easier manipulation
                var dataTable = ToDataTable(dataWithoutOutliers);

                // calculate sum, mean, and variance for rows and columns
                var sumOfRows = dataTable.AsEnumerable().Select(row => row.ItemArray.Sum(item => Convert.ToDouble(item))).ToList();
                var sumOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col => dataTable.AsEnumerable().Sum(row => Convert.ToDouble(row[col]))).ToList();

                var meanOfRows = dataTable.AsEnumerable().Select(row => row.ItemArray.Average(item => Convert.ToDouble(item))).ToList();
                var meanOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col => dataTable.AsEnumerable().Average(row => Convert.ToDouble(row[col]))).ToList();

                var varianceOfRows = dataTable.AsEnumerable().Select(row =>
                {
                    var values = row.ItemArray.Select(item => Convert.ToDouble(item)).ToList();
                    return CalculateVariance(values);
                }).ToList();

                var varianceOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col =>
                {
                    var values = dataTable.AsEnumerable().Select(row => Convert.ToDouble(row[col])).ToList();
                    return CalculateVariance(values);
                }).ToList();

                // ANOVA Calculations
                var grandMean = dataTable.AsEnumerable().SelectMany(row => row.ItemArray.Select(item => Convert.ToDouble(item))).Average();

                var SSB = dataTable.Columns.Count * meanOfRows.Sum(mean => Math.Pow(mean - grandMean, 2));

                var deviations = dataTable.AsEnumerable().Select(row => row.ItemArray.Select((item, index) => Convert.ToDouble(item) - row.ItemArray.Average(cell => Convert.ToDouble(cell))).ToArray()).ToList(); ;
                double SSW = deviations.Select(row => row.Select(deviation => Math.Pow(deviation, 2)).Sum()).Sum();

                var dfRows = dataTable.Rows.Count - 1;
                var dCol = dataTable.Columns.Count - 1;
                var dfWithinRows = dfRows * dCol;

                var MSB = SSB / dfRows;
                var MSW = SSW / dfWithinRows;

                var SSC = dataTable.Rows.Count * meanOfColumns.Sum(mean => Math.Pow(mean - grandMean, 2));
                var SSE = SSW - SSC;
                var MSE = SSE / dfWithinRows;
                var MSC = SSC / dCol;

                var FRows = MSB / MSE;
                var FColumns = MSC / MSE;

                var pValueRows = 1 - FDistribution(FRows, dfRows, dfWithinRows);
                var pValueColumns = 1 - FDistribution(FColumns, dCol, dfWithinRows);

                var probability = 0.05;
                var degree_freedom_row = dfRows;
                var degree_freedom_column = dCol;
                var degree_freedom_within = dfWithinRows;
                var dof = (dfWithinRows + dCol);


                var fCritRows = FDistributionCriticalValue(1 - probability, dfRows, dfWithinRows);
                var fCritColumns = FDistributionCriticalValue(1 - probability, dataTable.Columns.Count - 1, dfWithinRows);

                var fCritat95 = FisherSnedecor.InvCDF(dfRows, dof, 1 - probability);
                var F_value = MSB / MSE;
                var homogeneityTest = (F_value > fCritat95) ? "FAIL HOMOGENEITY TEST" : "PASS HOMOGENEITY TEST";


                //variance & mean table
                var resultsTable = new
                {
                    SumOfRows = sumOfRows,
                    SumOfColumns = sumOfColumns,
                    MeanOfRows = meanOfRows,
                    MeanOfColumns = meanOfColumns,
                    VarianceOfRows = varianceOfRows,
                    VarianceOfColumns = varianceOfColumns
                };

                // standard deviation table
                var result = new
                {
                    StandardDeviationTable = new
                    {
                        StandardDeviationA1 = stdDevA1,
                        StandardDeviationA2 = stdDevA2,
                        StandardDeviationA3 = stdDevA3,
                        LowerLimitA1 = lowerLimitA1,
                        UpperLimitA1 = upperLimitA1,
                        LowerLimitA2 = lowerLimitA2,
                        UpperLimitA2 = upperLimitA2,
                        LowerLimitA3 = lowerLimitA3,
                        UpperLimitA3 = upperLimitA3
                    },


                    Outliers = outliers, //printing the outliers
                    DataWithoutOutliers = dataWithoutOutliers, //printing the data without the outliers
                    ResultsTable = resultsTable,   //printing the variance table

                    //anova table
                    ANOVA = new
                    {
                        deviations,
                        SSB,
                        SSC,
                        SSW,
                        SSE,
                        degree_freedom_row,
                        degree_freedom_column,
                        degree_freedom_within,
                        MSB,
                        MSC,
                        MSW,
                        MSE,
                        FRows,
                        FColumns,
                        pValueRows,
                        pValueColumns,
                        fCritRows,
                        fCritColumns,
                        Fvalue = F_value,
                        FcriticalAt95 = fCritat95,
                        HomogeneityTest = homogeneityTest
                    }
                };
                return Ok(result); //printing everything 
            }
            catch (FileNotFoundException ex)  //error handling if file has issues
            {
                return NotFound(ex.Message);
            }
            
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            var mean = values.Average();
            var variance = values.Average(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(variance);
        }

        private double CalculateVariance(List<double> values)
        {
            var mean = values.Average();
            return values.Average(v => Math.Pow(v - mean, 2));
        }

        private DataTable ToDataTable(List<AnovaData> data)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Attribute1", typeof(double));
            dataTable.Columns.Add("Attribute2", typeof(double));
            dataTable.Columns.Add("Attribute3", typeof(double));

            foreach (var item in data)
            {
                dataTable.Rows.Add(item.Attribute1, item.Attribute2, item.Attribute3);
            }

            return dataTable;
        }

        private double FDistribution(double value, int dfn, int dfd)
        {
            return new FisherSnedecor(dfn, dfd).CumulativeDistribution(value);
        }

        private double FDistributionCriticalValue(double probability, int dfn, int dfd)
        {
            return new FisherSnedecor(dfn, dfd).InverseCumulativeDistribution(probability);
        }

    }
}
