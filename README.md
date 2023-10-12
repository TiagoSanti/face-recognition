# Face recognition with Dotnet working with Linux and MacOS

To start it:

```bash
docker build --tag Andrea055/face_recognition - < Dockerfile
docker run --rm -ti -v /tmp/.X11-unix:/tmp/.X11-unix -v /dev/shm:/dev/shm  --device /dev/dri --device=/dev/video0:/dev/video0 -e DISPLAY=:0 -p 2222:22 -v /dev/video0:/dev/video0 Andrae055/face_recognition
```

Then you can open an ssh session at localhost:2222 and start the program at /home/dev/face-recognition/FaceRecognition