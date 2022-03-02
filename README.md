# Detecção e reconhecimento facial usando [FaceRecognitionDotNet](https://github.com/takuya-takeuchi/FaceRecognitionDotNet) e [OpenCVSharp](https://github.com/shimat/opencvsharp).

Utilize a representação da estrutura do projeto abaixo como referência para as instruções referentes aos diretórios.
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
### Detalhes de execução
* DLLs *DlibDotNet.dll, DlibDotNet.Native.dll and DlibDotNet.Dnn.dll* ```.\bin\x64\<Sua solução>\``` </br>
* Executar em plataforma de solução x64 </br>
* [Models](https://github.com/davisking/dlib-models) localizados em ```.\bin\x64\<Sua solução>\models``` </br>
* Restaurar pacotes NuGet

<strong>Readme em desenvolvimento</strong> ..
