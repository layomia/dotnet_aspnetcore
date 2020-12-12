// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonCodeGeneration;

namespace Microsoft.AspNetCore.Components
{
    internal static class WebAssemblyComponentSerializationSettings
    {
        public static readonly JsonContext SerializerContext = new(GetOptions());

        private static JsonSerializerOptions GetOptions()
        {
            JsonSerializerOptions options = JsonSerializerOptions.CreateForCodeGen(JsonSerializerDefaults.General);
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            return options;
        }
    }
}
