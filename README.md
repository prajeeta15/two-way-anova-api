# two-way-anova-api
two way anova api for BPCL

# two-way-anova-api-documentation
## Table of Contents

1. [Introduction](#introduction)
2. [Overview](#overview)
3. [Installation](#installation)
4. [API Endpoints](#api-endpoints)
    1. [Upload JSON Data for Two-Way ANOVA](#upload-json-data-for-two-way-anova)
5. [Data Model](#data-model)
6. [Detailed Process Explanation](#detailed-process-explanation)
    1. [Data Validation](#data-validation)
    2. [Outlier Detection](#outlier-detection)
    3. [Data Transformation](#data-transformation)
    4. [ANOVA Calculation](#anova-calculation)
7. [Error Handling](#error-handling)
8. [Response Structure](#response-structure)
9. [Code Explanation](#code-explanation)
    1. [Controller](#controller)
    2. [Helper Methods](#helper-methods)
10. [Future Enhancements](#future-enhancements)
11. [Conclusion](#conclusion)

## Introduction

The Two-Way ANOVA API is designed to perform a two-way analysis of variance (ANOVA) on a given dataset. This API can be utilized to identify significant differences between group means in a dataset, while also handling outliers and calculating various statistical measures. The API is built using ASP.NET Core and is structured to handle JSON file inputs.

## Overview

This documentation covers the implementation and usage of the Two-Way ANOVA API. It provides a detailed explanation of the API endpoints, data models, and the overall process involved in performing a two-way ANOVA. Additionally, it explains the internal workings of the code, error handling mechanisms, and expected responses.

## Installation

To set up the Two-Way ANOVA API, follow these steps:

1. *Clone the repository:*
    bash
    git clone https://github.com/your-repo/two-way-anova-api.git
    cd two-way-anova-api
    

2. *Install dependencies:*
    bash
    dotnet restore
    

3. *Build the project:*
    bash
    dotnet build
    

4. *Run the application:*
    bash
    dotnet run
    

The API should now be running on http://localhost:5000.

## API Endpoints

### Upload JSON Data for Two-Way ANOVA

*Endpoint:* POST /api/anova/upload

*Description:* This endpoint accepts a JSON file containing the dataset for which the two-way ANOVA will be performed. It filters outliers, calculates various statistical measures, and returns the ANOVA results along with other relevant data.

*Request:*
- *Header:* Content-Type: multipart/form-data
- *Body:* A file input field with the key jsonfile containing the JSON data file.

*Response:*
- *Status Code:* 200 OK
- *Body:* A JSON object containing the ANOVA results, outliers, data without outliers, and various statistical tables.

*Example:*
bash
curl -X POST http://localhost:5000/api/anova/upload \
-F 'jsonfile=@/path/to/your/data.json'


## Data Model

The API expects the JSON data to be structured as a list of objects, where each object represents a data point with three attributes:

json
[
    {
        "Attribute1": value1,
        "Attribute2": value2,
        "Attribute3": value3
    },
    ...
]


*Example:*
json
[
    {
        "Attribute1": 12.5,
        "Attribute2": 15.3,
        "Attribute3": 11.8
    },
    {
        "Attribute1": 10.4,
        "Attribute2": 14.9,
        "Attribute3": 13.2
    }
]


## Detailed Process Explanation

### Data Validation

The API starts by validating the uploaded JSON file to ensure it is not null or empty. If the file is invalid, it returns a 400 Bad Request status with an appropriate error message.

### Outlier Detection

Outliers are detected based on the calculated means and standard deviations of the attributes. The range for identifying outliers is defined as:
- Lower limit: mean - 2 * standard deviation
- Upper limit: mean + 2 * standard deviation

Data points falling outside these ranges for any attribute are considered outliers.

### Data Transformation

The data without outliers is transformed into a DataTable for easier manipulation. This table is used for subsequent calculations, including sums, means, and variances for rows and columns.

### ANOVA Calculation

The API performs the following steps to calculate the ANOVA:
1. Calculate the grand mean of all data points.
2. Compute the sum of squares between groups (SSB), sum of squares within groups (SSW), and sum of squares for columns (SSC).
3. Determine the mean squares (MSB, MSW, MSC, MSE).
4. Calculate the F-values for rows and columns.
5. Obtain the p-values for rows and columns.
6. Perform the homogeneity test to check the assumption of equal variances.

## Error Handling

The API handles various types of errors, including:
- *FileNotFoundException:* Returned when the file cannot be found.
- *JsonException:* Returned when there is an error in parsing the JSON data.
- *General Exception:* Catches all other exceptions and returns a 500 Internal Server Error status with the error message.

## Response Structure

The response from the API contains several sections, each providing specific information:
- *StandardDeviationTable:* Includes the standard deviations and outlier limits for each attribute.
- *Outliers:* Lists the data points identified as outliers.
- *DataWithoutOutliers:* Contains the dataset after removing outliers.
- *ResultsTable:* Provides the sums, means, and variances for rows and columns.
- *ANOVA:* Details the ANOVA calculations, including SSB, SSC, SSW, SSE, degrees of freedom, mean squares, F-values, p-values, critical F-values, and the homogeneity test result.

*Example Response:*
json
{
    "StandardDeviationTable": {
        "StandardDeviationA1": 2.3,
        "StandardDeviationA2": 3.1,
        "StandardDeviationA3": 1.8,
        "LowerLimitA1": 8.4,
        "UpperLimitA1": 16.2,
        "LowerLimitA2": 9.1,
        "UpperLimitA2": 17.3,
        "LowerLimitA3": 7.5,
        "UpperLimitA3": 14.3
    },
    "Outliers": [
        {
            "Attribute1": 18.5,
            "Attribute2": 20.3,
            "Attribute3": 19.8
        }
    ],
    "DataWithoutOutliers": [
        {
            "Attribute1": 12.5,
            "Attribute2": 15.3,
            "Attribute3": 11.8
        },
        {
            "Attribute1": 10.4,
            "Attribute2": 14.9,
            "Attribute3": 13.2
        }
    ],
    "ResultsTable": {
        "SumOfRows": [39.6, 38.5],
        "SumOfColumns": [22.9, 30.2, 25.0],
        "MeanOfRows": [13.2, 12.8],
        "MeanOfColumns": [11.45, 15.1, 12.5],
        "VarianceOfRows": [2.2, 1.9],
        "VarianceOfColumns": [1.85, 2.3, 1.7]
    },
    "ANOVA": {
        "deviations": [
            [1.3, -0.4, 1.0],
            [-1.5, 0.3, -1.2]
        ],
        "SSB": 12.5,
        "SSC": 15.2,
        "SSW": 8.9,
        "SSE": 2.3,
        "degree_freedom_row": 1,
        "degree_freedom_column": 2,
        "degree_freedom_within": 1,
        "MSB": 12.5,
        "MSC": 7.6,
        "MSW": 8.9,
        "MSE": 2.3,
        "FRows": 5.4,
        "FColumns": 3.3,
        "pValueRows": 0.03,
        "pValueColumns": 0.05,
        "fCritRows": 4.3,
        "fCritColumns": 3.8,
        "HomogeneityTest": "PASS HOMOGENEITY TEST"
    }
}


## Code Explanation

### Controller

The AnovaController handles the main logic for processing the uploaded JSON data and performing the two-way ANOVA. The controller includes the following key sections:

#### Upload Endpoint

The FilterOutLiers method is the primary endpoint for uploading and processing the JSON file. It handles file validation, JSON deserialization, outlier detection, data transformation, and ANOVA calculation. The results are then compiled into a JSON response.

csharp
[HttpPost("upload")]
public IActionResult FilterOutLiers(IFormFile jsonfile)
{
    try
    {
        if (jsonfile == null

 || jsonfile.Length == 0)
        {
            return BadRequest("Invalid file.");
        }

        using var stream = new StreamReader(jsonfile.OpenReadStream());
        string json = stream.ReadToEnd();
        var data = JsonConvert.DeserializeObject<List<AnovaData>>(json);

        if (data == null || !data.Any())
        {
            return BadRequest("No data available in the JSON file.");
        }

        var meanA1 = data.Average(item => item.Attribute1);
        var meanA2 = data.Average(item => item.Attribute2);
        var meanA3 = data.Average(item => item.Attribute3);

        var stdDevA1 = CalculateStandardDeviation(data.Select(item => item.Attribute1).ToList());
        var stdDevA2 = CalculateStandardDeviation(data.Select(item => item.Attribute2).ToList());
        var stdDevA3 = CalculateStandardDeviation(data.Select(item => item.Attribute3).ToList());

        var lowerLimitA1 = meanA1 - 2 * stdDevA1;
        var upperLimitA1 = meanA1 + 2 * stdDevA1;
        var lowerLimitA2 = meanA2 - 2 * stdDevA2;
        var upperLimitA2 = meanA2 + 2 * stdDevA2;
        var lowerLimitA3 = meanA3 - 2 * stdDevA3;
        var upperLimitA3 = meanA3 + 2 * stdDevA3;

        var outliers = data.Where(item =>
            item.Attribute1 < lowerLimitA1 || item.Attribute1 > upperLimitA1 ||
            item.Attribute2 < lowerLimitA2 || item.Attribute2 > upperLimitA2 ||
            item.Attribute3 < lowerLimitA3 || item.Attribute3 > upperLimitA3
        ).ToList();

        var dataWithoutOutliers = data.Except(outliers).ToList();
        var dataTable = ToDataTable(dataWithoutOutliers);

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

        var grandMean = dataTable.AsEnumerable().SelectMany(row => row.ItemArray.Select(item => Convert.ToDouble(item))).Average();
        var SSB = dataTable.Columns.Count * meanOfRows.Sum(mean => Math.Pow(mean - grandMean, 2));

        var deviations = dataTable.AsEnumerable().Select(row => row.ItemArray.Select((item, index) => Convert.ToDouble(item) - row.ItemArray.Average(cell => Convert.ToDouble(cell))).ToArray()).ToList();
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

        var fCritAt95 = 2.39281411;
        var fCritRows = FDistributionCriticalValue(1 - probability, dfRows, dfWithinRows);
        var fCritColumns = FDistributionCriticalValue(1 - probability, dataTable.Columns.Count - 1, dfWithinRows);

        var homogeneityTest = (FRows > fCritAt95) && (pValueRows < 0.05) ? "FAIL HOMOGENEITY TEST" : "PASS HOMOGENEITY TEST";

        var resultsTable = new
        {
            SumOfRows = sumOfRows,
            SumOfColumns = sumOfColumns,
            MeanOfRows = meanOfRows,
            MeanOfColumns = meanOfColumns,
            VarianceOfRows = varianceOfRows,
            VarianceOfColumns = varianceOfColumns
        };

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

            Outliers = outliers,
            DataWithoutOutliers = dataWithoutOutliers,
            ResultsTable = resultsTable,

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
                HomogeneityTest = homogeneityTest
            }
        };
        return Ok(result);
    }
    catch (FileNotFoundException ex)
    {
        return NotFound(ex.Message);
    }
    catch (JsonException ex)
    {
        return BadRequest("Error parsing JSON: " + ex.Message);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Internal server error: " + ex.Message);
    }
}


### Helper Methods

Several helper methods are used to perform calculations and data transformations:

#### CalculateStandardDeviation

Calculates the standard deviation of a list of values.

csharp
private double CalculateStandardDeviation(List<double> values)
{
    var mean = values.Average();
    var variance = values.Average(v => Math.Pow(v - mean, 2));
    return Math.Sqrt(variance);
}


#### CalculateVariance

Calculates the variance of a list of values.

csharp
private double CalculateVariance(List<double> values)
{
    var mean = values.Average();
    return values.Average(v => Math.Pow(v - mean, 2));
}


#### ToDataTable

Converts a list of AnovaData objects into a DataTable.

csharp
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


#### FDistribution

Calculates the cumulative distribution function for the F-distribution.

csharp
private double FDistribution(double value, int dfn, int dfd)
{
    return new FisherSnedecor(dfn, dfd).CumulativeDistribution(value);
}


#### FDistributionCriticalValue

Calculates the critical value for the F-distribution.

csharp
private double FDistributionCriticalValue(double probability, int dfn, int dfd)
{
    return new FisherSnedecor(dfn, dfd).InverseCumulativeDistribution(probability);
}


## Future Enhancements

Potential future enhancements for the Two-Way ANOVA API include:
- Adding support for additional data formats (e.g., CSV, Excel).
- Enhancing the outlier detection mechanism with more robust statistical methods.
- Providing more detailed statistical analysis and visualizations in the response.
- Implementing authentication and authorization for secure access to the API.

## Conclusion

The Two-Way ANOVA API provides a comprehensive solution for performing two-way ANOVA on a given dataset, while also handling outliers and calculating various statistical measures. This documentation covers the installation, usage, and detailed explanation of the API, making it easy for users to integrate and utilize this powerful tool in their applications.

##LICENSE
