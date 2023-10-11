#!/bin/bash

sudo cp static_libs/libOpenCvSharpExtern.so /lib/x86_64-linux-gnu/libOpenCvSharpExtern

sudo cp static_libs/libOpenCvSharpExtern.so /lib/x86_64-linux-gnu/libOpenCvSharpExtern.so

sudo apt-get update
sudo apt-get install -y \
libopenblas-dev \
liblapack-dev \
libx11-6 \
libdlib19