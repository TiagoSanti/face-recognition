# Face recognition with Dotnet working with Linux and MacOS using Docker

This project use Docker so if you haven't you can install it [Debian]

```bash
    sudo apt install docker.io
```

Clone the project from GitHub: 

```bash
    git clone https://github.com/andreock/face-recognition.git
```

To start it [you have to change the path with your name]:

```bash
    docker build --tag YourName/face_recognition - < Dockerfile
    docker run -it -d -v /tmp/.X11-unix:/tmp/.X11-unix -v /dev/shm:/dev/shm  --device /dev/dri --device=dev/video0:/dev/video0 -e DISPLAY=:0 -p 2222:22 -v /dev/video0:/dev/video0 YourName/face_recognition /bin/bash
```

Now we have to add docker to group to running it without root:

```bash
    sudo usermod -aG docker $USER
```

Then you can open an ssh session at localhost:2222 and start the program at /home/dev/face-recognition/FaceRecognition [the password for ssh is qwe123]: 

```bash
    ssh root@localhost -p 2222
```
