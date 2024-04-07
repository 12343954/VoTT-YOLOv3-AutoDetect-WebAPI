using CoolooAI.Robot.Extensions;
using CoolooAI.Robot.Model;
using Microsoft.AspNetCore.Mvc;

namespace CoolooAI.YOLOv3.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YOLOv3Controller : ControllerBase
    {
        // GET api/<YOLOv3Controller>
        [HttpGet(Name = "GetYOLOv3")]
        public async Task<string> Get()
        {
            if (GG.YOLO.Initialized)
            {
                return $"YOLO v3 {(GG.YOLO.Initialized ? "Initialized OK" : "Initializing")}";
            }
            else
            {
                await GG.YOLO.InitYOLOv3Async();
                return $"YOLO v3 {(GG.YOLO.Initialized ? "Initialized OK" : "Initializing")}";
            }
        }

        [HttpGet("Detect/{image_path}")]
        public async Task<Model.JsonResult> DetectImageAsync(string image_path)
        {
            if (image_path.Length < 10 || image_path.Length > 256)
            {
                return new Model.JsonResult
                {
                    return_code = 0,
                    message = "Access deny! Image Path is availd!",
                };
            }

            image_path = image_path.Replace("\"", "").Replace("/", "\\").Replace("%2F", "\\");
            if (!System.IO.File.Exists(image_path))
            {
                return new Model.JsonResult
                {
                    return_code = 0,
                    message = "Access deny! File not exist!",
                };
            }

            if (!GG.ImageFileExtentions.Contains(Path.GetExtension(image_path).ToLower()))
            {
                return new Model.JsonResult
                {
                    return_code = 0,
                    message = $"Access deny! File type \"{Path.GetExtension(image_path)}\" not allowed!",
                };
            }

            if (!GG.YOLO.Initialized)
            {
                await GG.YOLO.InitYOLOv3Async();
            }

            GG.YOLO.Need_ETA = true;
            var (detect, eta) = await GG.YOLO.DetectAsync(image_path);
            var diff = detect.Count;
            detect = detect.UniqueNamedBox();
            diff = detect.Count - diff;

            _ = DebugPrintAsync(image_path, detect, diff, eta);

            return new Model.JsonResult
            {
                message = diff == 0
                            ? "OK"
                            : $"Yeap, Repeated {diff} !",
                return_code = 1,
                data = new
                {
                    eta,
                    diff,
                    detect,
                }
            };
        }

        private Task DebugPrintAsync(string image_path, List<obj_box> boxes, int diff, long eta)
        {
            return Task.Run(() =>
            {
                Console.WriteLine(string.Join("\n",
                    new List<string> {
                        $"\n-------------{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}--------------",
                        $"Image: {image_path}" ,
                        $"Detect {boxes.Count} obj(s), {(diff > 0 ? $"repeated {diff} obj(s)," : "")} ETA: {eta}ms",
                        string.Join("\n",  boxes.OrderBy(p=>p.id).Select(p=> $"{p.id, -6} {string.Join(",",  p.Obj_IDs.Select(pp=>pp.name)), -20} {p.prob.ToString("P2")}"))
                    }));
            });
        }
    }
}
