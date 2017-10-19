﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pGina.Plugin.TopicusKeyHub.Settings.Model;

namespace pGina.Plugin.TopicusKeyHub.Settings.ImportExport
{
    internal class TopicusKeyHubSettingsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            var objGetConnectionSettings = jo["GetConnectionSettings"];
            var connectionSettings = objGetConnectionSettings.ToObject<ConnectionSettings>();

            var objGetGroupSettings = jo["GetGroupSettings"];
            var groupSettings = objGetGroupSettings.ToObject<GroupSettings>();

            var objGetGatewaySettings = jo["GetGatewaySettings"];
            var gatewaySettings = objGetGatewaySettings.ToObject<GatewaySettings>();

            var settingsProvider = SettingsProvider.GetInstance(TopicusKeyHubPlugin.TopicusKeyHubUuid);
            var settings = settingsProvider.GetSettings();
            settings.SetConnectionSettings(connectionSettings);
            settings.SetGroupsSettings(groupSettings);
            settings.SetGatewaySettings(gatewaySettings);

            return settings;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TopicusKeyHubSettings);
        }
    }
}
