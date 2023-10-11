#!/bin/bash

user=$1
image=$2

if [ "$user" == "" ] || [ "$image" == "" ]; then
    echo "usage: $0 username photo";
    exit 255;
fi

user_path=models/data/images/known/"$user"
if [ ! -d "$user_path" ]; then
    mkdir "$user_path";
fi

cp "$image" models/data/images/known/"$user"/"$image"