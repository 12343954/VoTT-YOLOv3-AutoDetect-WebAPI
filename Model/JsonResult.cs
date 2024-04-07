namespace CoolooAI.YOLOv3.WebAPI.Model
{
    public class JsonResult
    {
        public JsonResult() { }

        public int return_code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
