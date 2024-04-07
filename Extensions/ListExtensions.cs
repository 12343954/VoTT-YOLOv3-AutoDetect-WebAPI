using CoolooAI.Robot.Model;
using System.Diagnostics;

namespace CoolooAI.Robot.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// 去重
        /// </summary>
        /// <param name="detections"></param>
        /// <returns></returns>
        public static List<obj_box> UniqueNamedBox(this List<obj_box> detections)
        {
            // 小于2个物体，直接返回
            if (detections == null || detections.Count < 2) return detections;

            // 排列规则（抓取优先序）
            // 1.靠右优先
            // 2.靠上优先
            detections = detections.OrderByDescending(p => p.center.X).ThenBy(p => p.center.Y).ToList();

            #region // C# => np.diff(list) 计算相邻物体的中心点距离，如果距离过近，则为同一物体
            // https://stackoverflow.com/questions/28902728/c-sharp-method-for-element-by-element-difference-of-an-array-derivative-approxi
            // Double.Hypot 勾股定理
            #endregion
            //var distances = detections.Zip(detections.Skip(1), (a, b) => Math.Sqrt(Math.Pow(b.center.X - a.center.X, 2) + Math.Pow(b.center.Y - a.center.Y, 2))).ToList();
            var distances = detections.Zip(detections.Skip(1), (a, b) => Double.Hypot(b.center.X - a.center.X, b.center.Y - a.center.Y)).ToList();

            //Debug.WriteLine($"UniqueNamedBox:distances={string.Join(", ", distances)}");

            for (var i = 0; i < distances.Count; i++)
            {
                //1. 距离大于40，不是同一方框，不是同一物体，继续循环
                if (distances[i] > 40) continue;

                //2. 否则，视为同一物体，重复识别，则需要去重，把下一个识别信息，合并到上一个信息中(id, name, prob)
                //   Union 去重，然后prob倒序，最高的在最前面
                detections[i].Obj_IDs = detections[i].Obj_IDs.Union(detections[i + 1].Obj_IDs).OrderByDescending(p => p.prob).ToList();
                var first = detections[i].Obj_IDs.FirstOrDefault();
                detections[i].obj_id = first.name_id;
                detections[i].obj_name = first.name;
                detections[i].prob = first.prob;
                //3. 把下一个信息置空
                detections[i + 1].Obj_IDs.Clear();

            }

            //4. 最后过滤掉空值，即为去重结果
            return detections.Where(p => p.Obj_IDs.Count > 0).ToList();
        }
    }
}
