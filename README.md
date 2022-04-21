# Facial detection and recognition using [FaceRecognitionDotNet](https://github.com/takuya-takeuchi/FaceRecognitionDotNet)

Use this project representation below as a reference for directory instructions.
```
FaceRecognition
│   Program.cs
│   Person.cs
│   obj
|   ...
|   bin
|   └─── Debug
│        Release
│        x64
│        └─── Release
│             Debug
│             └─── net6.0
│                  └─── models
│                  |    data
|                  |    └─── encodings
|                  |    |    └─── known
|                  |    |         └─── <target_people_directory>
|                  |    |              └─── <target_person_encodings>
|                  |    └─── images
|                  |         └─── known
|                  |              └─── <target_people_directory>
|                  |                   └─── <target_person_images>
|                  |    ...
|                  └              
└           
```
### Execution requirements
* DLLs *DlibDotNet.dll, DlibDotNet.Native.dll and DlibDotNet.Dnn.dll* ```.\bin\x64\<your solution>\``` </br>
* Execute in x64 solution platform </br>
* [Models](https://github.com/davisking/dlib-models) located in ```.\bin\x64\<your solution>\models``` </br>
* Restore NuGet packages
