# syntax=docker/dockerfile:1

# Use Ubuntu 20.04 since OpenCVSharp need it
FROM ubuntu:20.04

RUN apt update

# Install dependencies
RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get install -y \
    git \
    bzip2 \
    libopenblas-dev \
    liblapack-dev \
    libx11-6 \
    libdlib19 \
    libtesseract-dev \
    libgtk2.0-0 \
    libdc1394-22 \
    libswscale5 \
    libopenexr24 && \
    apt install libavformat58 --reinstall -y && \
    apt install libavcodec58 --reinstall -y && \
    apt install libavresample4 --reinstall -y && \
    apt install libavutil56 --reinstall

# Enable SSH
RUN apt install -y sudo openssh-server && service ssh enable

# Create dev user with qwe123 passwod
RUN adduser dev
RUN echo dev:qwe123 | chpasswd
RUN adduser dev sudo
RUN echo '%sudo ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers

# Change to dev
USER dev

# Install dotnet
RUN export DEBIAN_FRONTEND=noninteractive && \
    cd /home/dev && \
    sudo apt install -y wget && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --channel 6.0

# Add dotnet to path
ENV PATH "$PATH:/home/dev/.dotnet"

# Bootstrap the project
RUN cd /home/dev && git clone https://github.com/Andrea055/face-recognition.git && \
    cd face-recognition/FaceRecognition && \
    bash init_dlib_models.sh && \
    sudo cp static_libs/libOpenCvSharpExtern.so /lib/x86_64-linux-gnu/libOpenCvSharpExtern && \
    sudo cp static_libs/libOpenCvSharpExtern.so /lib/x86_64-linux-gnu/libOpenCvSharpExtern.so && \
    wget https://bellard.org/bpg/2.png > camera_frame.png

ENTRYPOINT service ssh start && bash