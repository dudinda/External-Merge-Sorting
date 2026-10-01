 The software solves the task described in [Task.pdf](https://github.com/dudinda/External-Merge-Sorting/blob/master/Task.pdf), which was originally issued as a test assignment.
 # External Merge Sorting

1. [Overview](#overview)
    - [Console Interface](#console-interface)
2. [Algorithm](#algorithm)
   - [IO Mode](#io-mode)
      - [Splitting Phase](#splitting)
      - [Sorting/Merging Phase](#sortingmerging)
   - [CPU Mode](#cpu-mode)
      - [Splitting/Sorting Phase](#splittingsorting) 
   - [Merging Phase](#merging)
3. [Strategy to merge a file 1 GB](#strategy-to-merge-a-file-1-gb)
4. [Strategy to merge a file 10 GB](#strategy-to-merge-a-file-10-gb)
5. [Strategy to merge a file 100 GB](#strategy-to-merge-a-file-100-gb)
6. [Created with](#created-with)
7. [How to run the program](#how-to-run-the-program)
***

## Overview
### Console Interface
The software provides a console interface with three verbs for each operation: ```[g]enerate```, ```[s]ort```, ```[e]valuate``` . To start the sorting process, the output directory must be created. By default, the directory is  ```C:\Temp\Files```.

```powershell
.\ExtSort.exe --help
```

<p align="center">
    <img src="https://github.com/user-attachments/assets/e0ba419b-b842-4908-9e6b-7f4fc6775ead" width="600" height = "400" alt="console interface">
    <p align="center">Fig. 1 - Using the option --help with the interface.</p>
</p>

To generate a 1 GB  file, the following command can be executed:
```powershell
.\ExtSort.exe generate output.txt 1048576
```

To start sorting a file with the correct data format, the following commands can be executed:
```powershell
.\ExtSort.exe sort output.txt output_sorted.txt IO
```
```powershell
.\ExtSort.exe sort output.txt output_sorted.txt CPU
```
***

## Algorithm
The provided software contains an implementation of the [External Sort](https://en.wikipedia.org/wiki/External_sorting) algorithm with an optional extension to split I/O operations between 2 drives. 
### IO Mode
#### Splitting 
During the splitting phase, the source file is read sequentially and split into ```NumberOfFiles``` chunks. For each chunk, the name is set as ```<counter>.unsorted```. At the end of each iteration, the algorithm checks whether the current byte is at the end of a line. If not, it continues writing byte-by-byte until the end of the line is reached. After a file is persisted, a map of ```filename::number of lines``` is populated.
#### Sorting/Merging
For each page, the program opens the ```SortPageSize``` streams, each in a separate task, and starts populating a buffer with priorities, the size of which was calculated during the splitting phase. Once the buffer is loaded, sorting is performed using the implemented [Multi-Column Comparer](https://github.com/dudinda/External-Merge-Sorting/blob/master/ExtSort/Code/Comparers/MultiColumnComparer.cs) to match the requested template. A space for the sorted file is then allocated, and the buffer is written to a file with the ```.sorted``` extension.

Right after a page is sorted, a task to merge the sorted files is executed. One possible strategy during the phase is to set ```SortPageSize = SortThenMergePageSize x SortThenMergeChunkSize```, so that a page starts merging into ```~sqrt(SortPageSize)``` files while the next page is being sorted.

### CPU Mode
#### Splitting/Sorting 
During the splitting phase, the source file is read sequentially and split into blocks of ```FileSplitSizeKb``` size. For each chunk, a priority is calculated and associated with the original row using a priority queue with a capacity of ```BufferCapacityLines```. At the end of each iteration, the algorithm starts dequeuing items from the priority queue into a file with the ```.sorted.tmp``` extension. When there are ```SortPageSize``` tasks, they are awaited until all of them are completed.
### Merging
The common merging strategy is to merge files from the bottom up, forming a [B-Tree](https://en.wikipedia.org/wiki/B-tree). A possible chain is ```64 -> 16 -> 4 -> 1```. For every ```MergePageSize```, ```MergeChunkSize``` streams are opened. The first element is then read from each stream, associated with an index, and the streams are processed sequentially line-by-line, with each row enqueued into the priority queue. The priority is set as a tuple of ```(<number>, <string>)``` using the [K-Way Merge](https://en.wikipedia.org/wiki/K-way_merge_algorithm). The first dequeued item is written to a target file. 

If two drives are configured correctly in ```appsettings.json```, files can be merged from one drive to another, for example: ```C:\\->E:\\->C:\\->```.

***
## Strategy to merge a file 1 GB

With the following settings, the algorithm will split a file into 64 chunks of approximately 16 MB each and start processing 4 pages of 16 files. The general file-merging strategy is ```64 -> 16``` (during the Sorting/Merging Phase) ``` -> 1``` (during the Merging Phase). All operations will be performed across  two drives `C:\` and `E:\`. 

```json
"SorterSettings": {
  "NumberOfFiles": 64,
  "SortPageSize": 16,
  "SortOutputBufferSize": 2097152,
  "MergePageSize": 4,
  "MergeChunkSize": 16,
  "MergeOutputBufferSize": 16777216,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "E:\\Temp\\Files",
    "MergeStartPath": "E:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
},
"SorterCPUSettings": {
  "BufferCapacityLines": 720000
},
"SorterIOSettings": {
  "SortThenMergePageSize": 4,
  "SortThenMergeChunkSize": 4
}
```
***

## Strategy to merge a file 10 GB

With the following settings, the algorithm will split a file into 512 chunks of approximately 20 MB each and start processing 32 pages of 16 files. The general merging strategy is ```512 -> 64``` (during the Sorting/Merging Phase) ```-> 8 -> 1``` (during the Merging Phase). All operations will be performed within on single drive `C:\`.

```json
"SorterSetting": {
  "NumberOfFiles": 512,
  "SortPageSize": 16,
  "SortOutputBufferSize": 2097152,
  "MergePageSize": 8,
  "MergeChunkSize": 8,
  "MergeOutputBufferSize": 16777216,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "C:\\Temp\\Files",
    "MergeStartPath": "C:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
},
"SorterCPUSettings": {
  "BufferCapacityLines": 1000000
},
"SorterIOSettings": {
  "SortThenMergePageSize": 2,
  "SortThenMergeChunkSize": 8
}
```
***

## Strategy to merge a file 100 GB

With the following settings, the algorithm will split a file into 4096 chunks of approximately 25 MB each and start processing 128 pages of 32 files. The general merging strategy is ```4096 -> 512``` (during the Sorting/Merging Phase) ```-> 64 -> 8 -> 1``` (during the Merging Phase). All operations will be performed on the single drive `C:\`.

```json
"SorterSetting": {
  "NumberOfFiles": 4096,
  "SortPageSize": 32,
  "SortOutputBufferSize": 2097152,
  "MergePageSize": 8,
  "MergeChunkSize": 8,
  "MergeOutputBufferSize": 16777216,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "C:\\Temp\\Files",
    "MergeStartPath": "C:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
},
"SorterCPUSettings": {
  "BufferCapacityLines": 1300000
},
"SorterIOSettings": {
  "SortThenMergePageSize": 4,
  "SortThenMergeChunkSize": 8
}
```
***

## Created With
[.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

[System.CommandLine](https://www.nuget.org/packages/System.CommandLine)

[Visual Studio Unit Tests](https://www.nuget.org/packages/Microsoft.NET.Test.SDK)

[Microsoft.Extensions.Configuration](https://www.nuget.org/packages/microsoft.extensions.configuration/)

****

## How To Run The Program

The result of executing the `dotnet build` command is a large number of files that can be tedious to manage. Therefore, it is more convenient to work with a compact version of the application, which can be obtained by executing the `dotnet publish` command.

1. In the `ExtSort`  project folder, execute the following command:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=false,PublishTrimmed=true,PublishSingleFile=true
```

2. Find the required files in the following subfolder:

`ExtSort\bin\Release\net6.0\win-x64\publish\`

3. Run the application using the command shown in the example:

```powershell
$Time = [System.Diagnostics.Stopwatch]::StartNew()
& ".\ExtSort.exe" "sort" "input.txt" "output.txt" "IO"
write-host ('Completed in {0} seconds.' -f $Time.Elapsed.TotalSeconds)
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
```
