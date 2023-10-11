#!/bin/bash

user=$1
image=$2
image_name=$3

if [ "$user" == "" ] ||  [ "$image" == "" ]  ||  [ "$image_name" == "" ] ; then
    echo "usage: $0 <username> <photo_directory> <name_photo.ext>";
    exit 
fi

user_path=models/data/images/known/"$user"
if [ ! -d "$user_path" ]; then
    mkdir "$user_path";
fi

cp "$image" models/data/images/known/"$user"/"$image_name"