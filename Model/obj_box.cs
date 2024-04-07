using OpenCvSharp;

namespace CoolooAI.Robot.Model
{
    public class obj_box
    {
        public int id { get; set; }        //row_index + 1

        // (x,y) - top-left corner
        public int x { get; set; }

        // (x,y) - top-left corner
        public int y { get; set; }

        // (w, h) - width & height of bounded box
        public int w { get; set; }

        // (w, h) - width & height of bounded box
        public int h { get; set; }

        public int angle { get; set; }

        public System.Drawing.Point center { get; set; }

        public List<obj_id> Obj_IDs { get; set; } = new List<obj_id>();

        // class of object - from range [0, classes-1]
        public int obj_id { get; set; } = -1;

        // class of object - from name list[obj_id]
        public string obj_name { get; set; } = "";

        // confidence - probability that the object was found correctly
        public double prob { get; set; }

        // tracking id for video (0 - untracked, 1 - inf - tracked object)
        public int track_id { get; set; }

        public int frames_counter { get; set; }

        public float x_3d { get; set; }
        public float y_3d { get; set; }

        public float z_3d { get; set; }  // 3-D coordinates, if there is used 3D-stereo camera

        public RotatedRect RotatedBox { get; set; }

        public Point2f[] BoxPoints { get; set; }

        /// <summary>
        /// Opencv detect time - timestamp
        /// </summary>
        public long DetectTime { get; set; }

        /// <summary>
        /// YOLO Detect Time - timestamp
        /// </summary>
        public long YoloTime { get; set; }

        /// <summary>
        /// robot pick up time, 0:means no need to pick up
        /// </summary>
        public long PickTime { get; set; }

        /// <summary>
        /// object picked up time
        /// </summary>
        public long PickedTime { get; set; }


    }

    public class obj_id
    {
        public int name_id { get; set; }
        public string name { get; set; } = String.Empty;
        public double prob { get; set; }
    }
}
