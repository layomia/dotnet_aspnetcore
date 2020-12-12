// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Components
{
    internal static class JsonSerializerOptionsProvider
    {
        public static JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }
}
