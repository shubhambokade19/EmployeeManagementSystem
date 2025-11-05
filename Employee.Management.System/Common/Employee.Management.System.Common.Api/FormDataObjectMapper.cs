using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Employee.Management.System.Common.Api
{
    public class FormDataObjectMapper
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        });

        public static T? CreateObject<T>(object model) where T : class, new()
        {
            // Convert model to JObject and remove UploadedFileList
            var jObject = JObject.FromObject(model, JsonSerializer);
            jObject.Remove("UploadedFileList");
            jObject.Remove("UploadedImage");
            // Deserialize JObject to target type
            var entity = jObject.ToObject<T>(JsonSerializer);

            // Copy UploadedFileList property if it exists in both model and target
            var uploadedFileList = model.GetType().GetProperty("UploadedFileList")?.GetValue(model);
            typeof(T).GetProperty("UploadedFileList")?.SetValue(entity, uploadedFileList);
            var uploadedImage = model.GetType().GetProperty("UploadedImage")?.GetValue(model);
            typeof(T).GetProperty("UploadedImage")?.SetValue(entity, uploadedImage);
            return entity;
        }
    }
}
