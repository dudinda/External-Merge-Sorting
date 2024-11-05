# Altium Test Task by Dudin D. A., Nov 2024

1. [Console interface](#console-interface)
2. [Algorithm](#algorithm)
   - [Splitting Phase](#splitting)
   - [Sorting/Merging Phase](#sortingmerging)
   - [Merging Phase](#merging)
3. [Strategy to merge a file 1 GB / Time in a local environment](#strategy-to-merge-a-file-1-gb)
4. [Strategy to merge a file 10 GB / Time in a local environment](#strategy-to-merge-a-file-10-gb)
5. [Created with](#created-with)
***
## Console Interface
The program provides a console interface with the two verbs [g]enerate and [s]ort to generate a file size of N (kb) and to sort 

```powershell
.\TestTask.exe --help
```

<p align="center">
    <img src="https://github.com/user-attachments/assets/03e3c6eb-a988-41f8-8fe6-eef479311f52" width="600" height = "400" alt="original underexposed image">
    <p align="center">Fig. 1 - Using the option --help with the interface.</p>
</p>

To generate a file with the size of 1 GB the following command can be executed:
```powershell
.\TestTask.exe generate output.txt 1000000
```

To start sorting a file with the correct data format the following command can be executed:
```powershell
.\TestTask.exe sort output.txt output_sorted.txt 
```

***
## Algorithm
The provided software contains an implementation of the [External Sort](https://en.wikipedia.org/wiki/External_sorting) algorithm with a possible extension to split I/O operations between 2 drives. 
### Splitting 
During the splitting phase a source file is being sequantially read and split into blocks of ```FileSplitSizeKb``` size. For every chunk the name is set as ```<counter>.unsorted``` . In the end of each iteration the algorithm analyze whether the current byte is set at the position of a line's end, and if not, continues to write byte-by-byte until the end of line is met. After a file is persisted, a map of ```filename::number of lines``` is populated.
### Sorting/Merging
### Merging
The common merging strategy is to try to converge files from bottom to top forming a [B-Tree](https://en.wikipedia.org/wiki/B-tree). The possible chain is ```256 -> 64 -> 16 -> 4 -> 1```. Every ```MergePageSize``` opens ```MergeChunkSize``` streams, then reads the very first element, binds an index to it and continues to process every stream line-by-line sequentially enqueue a row to the priority queue, where the priority is set as a tuple of ```(<number>, <string>)```. The first dequeued item is written to a target file. In case two drives correctly set in the ```appsettings.json``` it is possible to merge files from one drive to another: ex: ```C:\\->E:\\->C:\\```.
***
## Strategy to merge a file 1 GB

Specifing the following settings the algorithm will split a file into 64 chunks ~16MB each and start process 4 pages of 16 files.
The general files merging strategy: ```64 -> 16``` (during the Sorting/Merging Phase) ```-> 4 -> 1``` (during the Merging Phase). All operations will be performed within the single drive C:\\.

```json
"SorterSetting": {
  "FileSplitSizeKb": 16384,
  "SortPageSize": 16,
  "SortInputBufferSize": 4096,
  "SortOutputBufferSize": 81920,
  "SortThenMergePageSize": 4,
  "SortThenMergeChunkSize": 4,
  "MergePageSize": 4,
  "MergeChunkSize": 4,
  "MergeInputBufferSize": 4096,
  "MergeOutputBufferSize": 81920,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "C:\\Temp\\Files",
    "MergeStartPath": "C:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
}
```

***
## Strategy to merge a file 10 GB

Specifing the following settings the algorithm will split a file into 256 chunks ~41MB each and start process 8 pages of 64 files.
The general merging strategy: ```256 -> 64``` (during the Sorting/Merging Phase) ```-> 16 -> 4 -> 1``` (during the Merging Phase). All operations will be performed within the single drive C:\\.

```json
"SorterSetting": {
  "FileSplitSizeKb": 40960,
  "SortPageSize": 64,
  "SortInputBufferSize": 4096,
  "SortOutputBufferSize": 81920,
  "SortThenMergePageSize": 8,
  "SortThenMergeChunkSize": 8,
  "MergePageSize": 4,
  "MergeChunkSize": 4,
  "MergeInputBufferSize": 4096,
  "MergeOutputBufferSize": 81920,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "C:\\Temp\\Files",
    "MergeStartPath": "C:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
}
```

***

## Created With

[System.CommandLine](https://www.nuget.org/packages/ImageProcessing.Microkernel.DIAdapter](https://www.nuget.org/packages/System.CommandLine)/)

[Visual Studio Unit Tests](https://www.nuget.org/packages/Microsoft.NET.Test.SDK)

[Microsoft.Extensions.Configuration](https://www.nuget.org/packages/microsoft.extensions.configuration/)


