using AlarmWorkflow.Shared.Core;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AlarmWorkflow.TestOperation
{
    /// <summary>
    /// Serialize/Deserialize an operation with json
    /// </summary>
    public static class OperationSerializer
    {
        private static readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        });

        /// <summary>
        /// Serialize a operation to a file
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="file"></param>
        public static void Serialize(Operation operation, string file)
        {
            using (var stream = File.CreateText(file))
            {
                serializer.Serialize(stream, operation);
            }
        }

        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Operation Deserialize(string file)
        {
            using (StreamReader stream = File.OpenText(file))
            {
                try
                {
                    return (Operation)serializer.Deserialize(stream, typeof(Operation));

                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
    }
}
