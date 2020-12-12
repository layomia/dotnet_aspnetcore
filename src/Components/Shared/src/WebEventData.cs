// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using JsonCodeGeneration;

[assembly: JsonSerializable(typeof(Microsoft.AspNetCore.Components.ChangeEventArgs))]
[assembly: JsonSerializable(typeof(ClipboardEventArgs))]
[assembly: JsonSerializable(typeof(DragEventArgs))]
[assembly: JsonSerializable(typeof(ErrorEventArgs))]
[assembly: JsonSerializable(typeof(FocusEventArgs))]
[assembly: JsonSerializable(typeof(KeyboardEventArgs))]
[assembly: JsonSerializable(typeof(MouseEventArgs))]
[assembly: JsonSerializable(typeof(PointerEventArgs))]
[assembly: JsonSerializable(typeof(ProgressEventArgs))]
[assembly: JsonSerializable(typeof(TouchEventArgs))]
[assembly: JsonSerializable(typeof(WheelEventArgs))]
[assembly: JsonSerializable(typeof(EventArgs))]

namespace Microsoft.AspNetCore.Components.Web
{
    internal class WebEventData
    {
        // This class represents the second half of parsing incoming event data,
        // once the type of the eventArgs becomes known.
        public static WebEventData Parse(string eventDescriptorJson, string eventArgsJson)
        {
            WebEventDescriptor eventDescriptor;
            try
            {
                eventDescriptor = Deserialize<WebEventDescriptor>(eventDescriptorJson);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error parsing the event descriptor", e);
            }

            return Parse(
                eventDescriptor,
                eventArgsJson);
        }

        public static WebEventData Parse(WebEventDescriptor eventDescriptor, string eventArgsJson)
        {
            return new WebEventData(
                eventDescriptor.BrowserRendererId,
                eventDescriptor.EventHandlerId,
                InterpretEventFieldInfo(eventDescriptor.EventFieldInfo),
                ParseEventArgsJson(eventDescriptor.EventHandlerId, eventDescriptor.EventArgsType, eventArgsJson));
        }

        private WebEventData(int browserRendererId, ulong eventHandlerId, EventFieldInfo eventFieldInfo, EventArgs eventArgs)
        {
            BrowserRendererId = browserRendererId;
            EventHandlerId = eventHandlerId;
            EventFieldInfo = eventFieldInfo;
            EventArgs = eventArgs;
        }

        public int BrowserRendererId { get; }

        public ulong EventHandlerId { get; }

        public EventFieldInfo EventFieldInfo { get; }

        public EventArgs EventArgs { get; }

        private static EventArgs ParseEventArgsJson(ulong eventHandlerId, string eventArgsType, string eventArgsJson)
        {
            try
            {
                return eventArgsType switch
                {
                    "change" => DeserializeChangeEventArgs(eventArgsJson),
                    "clipboard" => JsonSerializer.Deserialize<ClipboardEventArgs>(eventArgsJson, SerializerContext.ClipboardEventArgs),
                    "drag" => JsonSerializer.Deserialize<DragEventArgs>(eventArgsJson, SerializerContext.DragEventArgs),
                    "error" => JsonSerializer.Deserialize<ErrorEventArgs>(eventArgsJson, SerializerContext.ErrorEventArgs),
                    "focus" => JsonSerializer.Deserialize<FocusEventArgs>(eventArgsJson, SerializerContext.FocusEventArgs),
                    "keyboard" => JsonSerializer.Deserialize<KeyboardEventArgs>(eventArgsJson, SerializerContext.KeyboardEventArgs),
                    "mouse" => JsonSerializer.Deserialize<MouseEventArgs>(eventArgsJson, SerializerContext.MouseEventArgs),
                    "pointer" => JsonSerializer.Deserialize<PointerEventArgs>(eventArgsJson, SerializerContext.PointerEventArgs),
                    "progress" => JsonSerializer.Deserialize<ProgressEventArgs>(eventArgsJson, SerializerContext.ProgressEventArgs),
                    "touch" => JsonSerializer.Deserialize<TouchEventArgs>(eventArgsJson, SerializerContext.TouchEventArgs),
                    "unknown" => EventArgs.Empty,
                    "wheel" => JsonSerializer.Deserialize<WheelEventArgs>(eventArgsJson, SerializerContext.WheelEventArgs),
                    "toggle" => JsonSerializer.Deserialize<EventArgs>(eventArgsJson, SerializerContext.EventArgs),
                    _ => throw new InvalidOperationException($"Unsupported event type '{eventArgsType}'. EventId: '{eventHandlerId}'."),
                };
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"There was an error parsing the event arguments. EventId: '{eventHandlerId}'.", e);
            }
        }

        private static JsonContext SerializerContext = new(JsonSerializerOptionsProvider.Options);

        private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, jsonSerializerContext: null);

        private static EventFieldInfo InterpretEventFieldInfo(EventFieldInfo fieldInfo)
        {
            // The incoming field value can be either a bool or a string, but since the .NET property
            // type is 'object', it will deserialize initially as a JsonElement
            if (fieldInfo?.FieldValue is JsonElement attributeValueJsonElement)
            {
                switch (attributeValueJsonElement.ValueKind)
                {
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return new EventFieldInfo
                        {
                            ComponentId = fieldInfo.ComponentId,
                            FieldValue = attributeValueJsonElement.GetBoolean()
                        };
                    default:
                        return new EventFieldInfo
                        {
                            ComponentId = fieldInfo.ComponentId,
                            FieldValue = attributeValueJsonElement.GetString()
                        };
                }
            }

            return null;
        }

        private static ChangeEventArgs DeserializeChangeEventArgs(string eventArgsJson)
        {
            var changeArgs = Deserialize<ChangeEventArgs>(eventArgsJson);
            var jsonElement = (JsonElement)changeArgs.Value;
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                    changeArgs.Value = null;
                    break;
                case JsonValueKind.String:
                    changeArgs.Value = jsonElement.GetString();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    changeArgs.Value = jsonElement.GetBoolean();
                    break;
                default:
                    throw new ArgumentException($"Unsupported {nameof(ChangeEventArgs)} value {jsonElement}.");
            }
            return changeArgs;
        }
    }
}
