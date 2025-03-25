using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;

namespace MyCaseManagementDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read Rules from file (=CaseManagement.json)
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "CaseManagement.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");
            var fileData = File.ReadAllText(files[0]);

            // Workflow initialization with rules (=CaseManagement.json)
            var workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData);

            // Generate test data as a JSON
            var testData = "{\"country\": \"Jupiter\", \"loyaltyFactor\": 2,  \"caseType\": \"x\", \"activationDate\": \"2025-01-01\", \"currentDate\": \"2025-01-08\", \"sumA\": 600, \"sumB\": 400}";
            using JsonDocument jsonDoc = JsonDocument.Parse(testData);
            var input = new ExpandoObject() as IDictionary<string, object>;
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {

                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        input[property.Name] = property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        input[property.Name] = property.Value.GetInt32();
                        break;
                    default:
                        input[property.Name] = property.Value.ToString();
                        break;
                }
            }

            // Pretty-print test data before rule processing
            var options = new JsonSerializerOptions { WriteIndented = true };
            string prettyInputs = JsonSerializer.Serialize(input, options);
            Console.WriteLine("\n");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("| Test Data in JSON                                         |");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine(prettyInputs);
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("\n");

            // Execute rules
            var bre = new RulesEngine.RulesEngine(workflow.ToArray());
            List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync("CaseManagement", input).Result;

            // Pretty-print rule results in table format
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("| Rule Name                                  | Result      |");
            Console.WriteLine("------------------------------------------------------------");
            foreach (var result in resultList)
            {
                Console.WriteLine($"| {result.Rule.RuleName,-42} | {result.IsSuccess,-11} |");
            }
            Console.WriteLine("------------------------------------------------------------");

        }
    }
}