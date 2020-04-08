using System;
using MiNET.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.Utils
{
    public class FormConverter : JsonCreationConverter<Form>
    {
        protected override Form Create(Type objectType, JObject jObject)
        {
            string type = jObject["type"].Value<string>();

            switch (type)
            {
                case "modal":
                    return new ModalForm();
                case "form":
                    return new SimpleForm();
                case "custom_form":
                    return new CustomForm();
                default:
                    throw new NotSupportedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Form).IsAssignableFrom(objectType);
        }
    }
    
    public class CustomElementConverter : JsonCreationConverter<CustomElement>
    {
        protected override CustomElement Create(Type objectType, JObject jObject)
        {
            string type = jObject["type"].Value<string>();

            switch (type)
            {
                case "label":
                    return new Label();
                case "input":
                    return new Input();
                case "toggle":
                    return new Toggle();
                case "slider":
                    return new Slider();
                case "step_slider":
                    return new StepSlider();
                case "dropdown":
                    return new Dropdown();
                default:
                    throw new NotSupportedException($"The type {type} is not supported!");
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(CustomElement).IsAssignableFrom(objectType);
        }
    }
    
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
    
}