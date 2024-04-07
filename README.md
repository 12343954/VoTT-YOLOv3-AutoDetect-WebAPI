# YOLOv3 auto-detect webapi for [VoTT 2.2.2](https://github.com/12343954/VoTT)

- feat: Automatically annotate images via YOLOv3 auto-detect webapi (with [AlexeyAB](https://github.com/AlexeyAB/darknet)'s YOLOv3 `yolo_cpp_dll.dll` which written in 2019, the fastest YOLO on RTX20 series graphics cards, 30+ fps)

    [https://www.youtube.com/watch?v=ajZxjAxAqNk](https://www.youtube.com/watch?v=ajZxjAxAqNk)

    shortcut key⌨: Q

    - webapi : http://localhost:50505/api/YOLOv3/detect/${image_path} , only accept local image path

        ```
        nvidia-smi
        Driver Version: 456.71  CUDA Version: 11.1

        nvcc -V
        nvcc: NVIDIA (R) Cuda compiler driver
        Copyright (c) 2005-2019 NVIDIA Corporation
        Built on Wed_Oct_23_19:32:27_Pacific_Daylight_Time_2019
        Cuda compilation tools, release 10.2, V10.2.89

        cuDnn 10.2
        ```

    - json result:
        ```
        {
            eta: 28, // ms
            diff: 0, // means no coincident recognition（green toast）
                  n, // means duplicate detection (yellow toast),
            detect: [{
                id, // detection index
                x,y,w,h,
                obj_id, obj_name, prob, // max prob
                obj_IDs: [ // the same region, order by prob desc
                    {obj_id, obj_name, prob},
                    ...
                    ]
                },
                ...
            ]
        }
        ```
- feat: Highlight the region on mouse enter each row in yolo detection dialog
- fix: Remove the last toast immediately, and show the next toast with result smoothly, avoid occluding the main UI

![https://www.youtube.com/watch?v=ajZxjAxAqNk](https://github.com/12343954/VoTT/assets/1804003/1144b9a5-490b-4939-a0f9-d5e21a43856f)


<hr />