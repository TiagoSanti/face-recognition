#!/bin/bash

git clone https://github.com/davisking/dlib-models.git
mv dlib-models/* models/
bzip2 -d models/*
rm -rf dlib-models
mkdir -p models/data/images/known/
mkdir -p models/data/encodings/known/