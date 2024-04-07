using CoolooAI.Robot.Model;
using Darknet;
using OpenCvSharp;
using System.Diagnostics;

namespace CoolooAI.Robot.Business
{
    public class YOLOv3
    {
        #region //public vars
        /// <summary>
        /// 0: back color
        /// 1: font color
        /// usage: COLORS[obj_id, 0], COLORS[obj_id, 1]
        /// </summary>
        public string[,] COLORS = new string[93, 2] {
            {"#DC143C", "#FFFFFF"},{"#0000FF", "#FFFFFF"},{"#8A2BE2", "#FFFFFF"},{"#7FFF00", "#1F2D3D"},{"#A52A2A", "#FFFFFF"},{"#2F4F4F", "#FFFFFF"},{"#FF1493", "#FFFFFF"},{"#D2691E", "#FFFFFF"},{"#FF7F50", "#1F2D3D"},{"#228B22", "#FFFFFF"},{"#00008B", "#FFFFFF"},{"#6495ED", "#FFFFFF"},{"#B8860B", "#FFFFFF"},{"#006400", "#FFFFFF"},{"#8B008B", "#FFFFFF"},{"#556B2F", "#FFFFFF"},{"#FF8C00", "#1F2D3D"},{"#E9967A", "#1F2D3D"},{"#483D8B", "#FFFFFF"},{"#00CED1", "#FFFFFF"},{"#9400D3", "#FFFFFF"},{"#00BFFF", "#FFFFFF"},{"#1E90FF", "#FFFFFF"},{"#B22222", "#FFFFFF"},{"#FF00FF", "#FFFFFF"},{"#FFD700", "#1F2D3D"},{"#DAA520", "#1F2D3D"},{"#808080", "#FFFFFF"},{"#008000", "#FFFFFF"},{"#ADFF2F", "#1F2D3D"},{"#FF69B4", "#1F2D3D"},{"#CD5C5C", "#FFFFFF"},{"#4B0082", "#FFFFFF"},{"#7CFC00", "#1F2D3D"},{"#ADD8E6", "#1F2D3D"},{"#F08080", "#1F2D3D"},{"#FFB6C1", "#1F2D3D"},{"#FFA07A", "#1F2D3D"},{"#20B2AA", "#FFFFFF"},{"#DEB887", "#1F2D3D"},{"#87CEFA", "#1F2D3D"},{"#778899", "#FFFFFF"},{"#778899", "#FFFFFF"},{"#32CD32", "#FFFFFF"},{"#FF00FF", "#FFFFFF"},{"#800000", "#FFFFFF"},{"#66CDAA", "#1F2D3D"},{"#0000CD", "#FFFFFF"},{"#BA55D3", "#FFFFFF"},{"#9370DB", "#FFFFFF"},{"#3CB371", "#FFFFFF"},{"#7B68EE", "#FFFFFF"},{"#00FA9A", "#1F2D3D"},{"#48D1CC", "#1F2D3D"},{"#C71585", "#FFFFFF"},{"#7FFFD4", "#1F2D3D"},{"#000080", "#FFFFFF"},{"#808000", "#FFFFFF"},{"#6B8E23", "#FFFFFF"},{"#FFA500", "#1F2D3D"},{"#FF4500", "#FFFFFF"},{"#DA70D6", "#1F2D3D"},{"#DB7093", "#FFFFFF"},{"#FFDAB9", "#1F2D3D"},{"#CD853F", "#FFFFFF"},{"#FFC0CB", "#1F2D3D"},{"#DDA0DD", "#1F2D3D"},{"#800080", "#FFFFFF"},{"#663399", "#FFFFFF"},{"#FF0000", "#FFFFFF"},{"#BC8F8F", "#1F2D3D"},{"#4169E1", "#FFFFFF"},{"#8B4513", "#FFFFFF"},{"#FA8072", "#1F2D3D"},{"#00FFFF", "#1F2D3D"},{"#F4A460", "#1F2D3D"},{"#2E8B57", "#FFFFFF"},{"#A0522D", "#FFFFFF"},{"#87CEEB", "#1F2D3D"},{"#6A5ACD", "#FFFFFF"},{"#708090", "#FFFFFF"},{"#00FF7F", "#1F2D3D"},{"#4682B4", "#FFFFFF"},{"#D2B48C", "#1F2D3D"},{"#008080", "#FFFFFF"},{"#D8BFD8", "#1F2D3D"},{"#FF6347", "#FFFFFF"},{"#40E0D0", "#1F2D3D"},{"#EE82EE", "#1F2D3D"},{"#F5DEB3", "#1F2D3D"},{"#FFFF00", "#1F2D3D"},{"#9ACD32", "#1F2D3D"},{"#000000", "#FFFFFF"}
        };
        public List<string> Names;
        public string ModelName;
        #endregion

        #region // private vars
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        YoloWrapper YOLO;
        Stopwatch stopwatch;
        #endregion

        #region //property
        public bool Need_ETA { get; set; } = false;
        public bool Initialized { get; set; } = false;
        public long ETA { get; set; } = 0;
        #endregion
        public YOLOv3(bool need_eta = false, string modelName = "new-coolooai-v3")
        {
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentNullException("`YOLO Model Name` must be specified, for example: `new-coolooai-v3`");

            ModelName = modelName;
            Need_ETA = need_eta;
            Initialized = false;
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// call this InitNamesAsync() before InitYOLOv3Async()
        /// </summary>
        /// <returns></returns>
        public async Task<(bool initialized, string msg)> InitNamesAsync()
        {
            return await Task.Run(() =>
            {
                var files = Directory.GetFiles(Path.Combine(basePath, "YOLO-Model", ModelName));
                var NamesFile = files.Where(p => p.EndsWith(".names")).FirstOrDefault();
                if (string.IsNullOrEmpty(NamesFile) || !File.Exists(NamesFile))
                    return (false, "Missing YOLOv3 Model Names \"eg: coco.names\"！");

                Names = File.ReadAllLines(NamesFile).ToList();
                return (true, $"YOLOv3, Object Names loaded!");
            });
        }

        /// <summary>
        /// Initialize YOLOv3
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public async Task<(bool initialized, string msg)> InitYOLOv3Async()
        {
            Initialized = false;
            return await Task.Run(async () =>
            {
                // pre-training model
                var files = Directory.GetFiles(Path.Combine(basePath, "YOLO-Model", ModelName));

                var ConfigFile = files.Where(p => p.EndsWith(".cfg")).FirstOrDefault();
                var WeightsFile = files.Where(p => p.EndsWith(".weights")).FirstOrDefault();
                if (Names == null || Names.Count() == 0)
                    await InitNamesAsync();

                if (string.IsNullOrEmpty(ConfigFile) || string.IsNullOrEmpty(WeightsFile) || Names?.Count() == 0)
                    return (false, "Missing Model files *.cfg, *.weight, *.names, all are indispensable ！");

                var fileinfo = new FileInfo(WeightsFile);

                // use GPU 0
                var GpuIndex = 0;

                YOLO = new YoloWrapper(ConfigFile, WeightsFile, GpuIndex);
                Initialized = true;
                return (true, $"YOLOv3, RTX 2080 Ti, CUDA 10.2, Custom Dataset, {fileinfo.LastWriteTime.ToString("yyyy/MM/dd")}");
            });

        }

        /// <summary>
        /// Detect objects async with opencv mat data
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<(List<obj_box> list, long ETA)> DetectAsync(Mat image)
        {
            if (YOLO == null) return (new List<obj_box>(), 0);
            return await Task.Run(() =>
            {
                if (Need_ETA)
                {
                    ETA = 0;
                    stopwatch.Restart();
                }

                var bboxes = YOLO.Detect(image.ToBytes())?.Where(p => p.w > 0 && p.h > 0 && p.prob > 0.15);
                if (Need_ETA)
                {
                    stopwatch.Stop();
                    ETA = stopwatch.ElapsedMilliseconds;
                }

                var list = bboxes?.Select((p, i) => new obj_box
                {
                    id = i + 1,
                    Obj_IDs = new List<obj_id> {
                        new obj_id {
                            name_id = (int)p.obj_id,
                            name = Names[(int)p.obj_id],
                            prob = p.prob }
                    },
                    obj_id = (int)p.obj_id,
                    obj_name = Names[(int)p.obj_id],
                    x = (int)p.x,
                    y = (int)p.y,
                    w = (int)p.w,
                    h = (int)p.h,
                    prob = p.prob,
                    center = new System.Drawing.Point((int)(p.x + p.w / 2), (int)(p.y + p.h / 2)),
                    track_id = (int)p.track_id,
                    frames_counter = (int)p.frames_counter,
                    x_3d = p.x_3d,
                    y_3d = p.y_3d,
                    z_3d = p.z_3d,
                }).ToList();

                return (list, ETA);
            });
        }

        /// <summary>
        /// Detect objects async with a local image file path
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<(List<obj_box> list, long ETA)> DetectAsync(string image)
        {
            if (YOLO == null) return (new List<obj_box>(), 0);
            return await Task.Run(() =>
            {
                if (Need_ETA)
                {
                    ETA = 0;
                    stopwatch.Restart();
                }

                var bboxes = YOLO.Detect(image)?.Where(p => p.w > 0 && p.h > 0 && p.prob > 0.15);
                if (Need_ETA)
                {
                    stopwatch.Stop();
                    ETA = stopwatch.ElapsedMilliseconds;
                }

                var list = bboxes?.Select((p, i) => new obj_box
                {
                    id = i + 1,
                    Obj_IDs = new List<obj_id> {
                        new obj_id {
                            name_id = (int)p.obj_id,
                            name = Names[(int)p.obj_id],
                            prob = p.prob }
                    },
                    obj_id = (int)p.obj_id,
                    obj_name = Names[(int)p.obj_id],
                    x = (int)p.x,
                    y = (int)p.y,
                    w = (int)p.w,
                    h = (int)p.h,
                    prob = p.prob,
                    center = new System.Drawing.Point((int)(p.x + p.w / 2), (int)(p.y + p.h / 2)),
                    track_id = (int)p.track_id,
                    frames_counter = (int)p.frames_counter,
                    x_3d = p.x_3d,
                    y_3d = p.y_3d,
                    z_3d = p.z_3d,
                }).ToList();

                return (list, ETA);
            });
        }
    }
}
