 The software solves a task defined in the [Task.pdf](https://github.com/dudinda/External-Merge-Sorting/blob/master/Task.pdf), which was originally issued as a test assignment.
 # External Merge Sorting

1. [Console interface](#console-interface)
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

## Console Interface
The software provides a console interface with three verbs: ```[g]enerate```, ```[s]ort```, ```[e]valuate``` for every program. To start sort an output file the target directory must be created. By default the path is ```C:\Temp\Files```.

```powershell
.\ExtSort.exe --help
```

<p align="center">
    <img src="https://github.com/user-attachments/assets/532277a0-3324-4a90-8ab3-36b8af2778d4" width="600" height = "400" alt="console interface">
    <p align="center">Fig. 1 - Using the option --help with the interface.</p>
</p>

To generate a file with the size of 1 GB the following command can be executed:
```powershell
.\ExtSort.exe generate output.txt 1048576
```

To start sorting a file with the correct data format the following commands can be executed:
```powershell
.\ExtSort.exe sort output.txt output_sorted.txt IO
```
```powershell
.\ExtSort.exe sort output.txt output_sorted.txt CPU
```
***

## Algorithm
The provided software contains an implementation of the [External Sort](https://en.wikipedia.org/wiki/External_sorting) algorithm with a possible extension to split I/O operations between 2 drives. 
### IO Mode
#### Splitting 
During the splitting phase, a source file is sequentially read and split into ```NumberOfFiles``` chunks. For every chunk, the name is set as ```<counter>.unsorted```. In the end of each iteration the algorithm analyzes whether the current byte is set at the position of a line's end, and if not, continues to write byte-by-byte until the end of the line is met. After a file is persisted, a map of ```filename::number of lines``` is populated.
#### Sorting/Merging
For every page, the program opens the ```SortPageSize``` streams each in  a separate task and starts to populate a buffer with priorities, the size of which was calculated during the Splitting phase. When a buffer is loaded, sorting occurs using the implemented [Multi-Column Comparer](https://github.com/dudinda/External-Merge-Sorting/blob/master/ExtSort/Code/Comparers/MultiColumnComparer.cs) to correspond to the requested template, then a space for a sorted file is allocated and the buffer is written into a file with the ```.sorted``` extension.  Right after a page was sorted a task to merge sorted files is executed. A possible strategy during the phase is to equate ```SortPageSize = SortThenMergePageSize x SortThenMergeChunkSize``` so a page starts merging into ```~sqrt(SortPageSize)``` files when the next page is being sorted.
### CPU Mode
#### Splitting/Sorting 
During the splitting phase, a source file is sequentially read and split into blocks of ```FileSplitSizeKb``` size. For every chunk, a priority is calculated and linked with the original row using a priority queue size of ```BufferCapacityLines```. At the end of each iteration, the algorithm starts to dequeue the priority queue into a file with the ```.sorted.tmp``` extension. When there are ```SortPageSize``` tasks, they are awaited until all of them are completed.
### Merging
The common merging strategy is to try to converge files from bottom to top forming a [B-Tree](https://en.wikipedia.org/wiki/B-tree). A possible chain is ```64 -> 16 -> 4 -> 1```. Every ```MergePageSize``` opens ```MergeChunkSize``` streams, then reads the very first element, binds an index to it and continues to process every stream line-by-line sequentially enqueue a row to the priority queue, where the priority is set as a tuple of ```(<number>, <string>)``` using the [K-Way Merge](https://en.wikipedia.org/wiki/K-way_merge_algorithm). The first dequeued item is written to a target file. In case two drives are correctly set in the ```appsettings.json``` it is possible to merge files from one drive to another: ex: ```C:\\->E:\\->C:\\->```.

***
## Strategy to merge a file 1 GB

Specifying the following settings the algorithm will split a file into 64 chunks ~16MB each and start processing 4 pages of 16 files.
The general files merging strategy: ```64 -> 16``` (during the Sorting/Merging Phase) ``` -> 1``` (during the Merging Phase). All operations will be performed within the two drives C:\\ and E:\\. 

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

Specifying the following settings the algorithm will split a file into 512 chunks ~20MB each and start processing 32 pages of 16 files.
The general merging strategy: ```512 -> 64``` (during the Sorting/Merging Phase) ```-> 8 -> 1``` (during the Merging Phase). All operations will be performed within the single drive C:\\.

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

Specifying the following settings the algorithm will split a file into 4096 chunks ~25MB each and start processing 128 pages of 32 files.
The general merging strategy: ```4096 -> 512``` (during the Sorting/Merging Phase) ```-> 64 -> 8 -> 1``` (during the Merging Phase). All operations will be performed within the single drive C:\\.

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

The result of executing the `dotnet build` command is a whole bunch of files that are tedious to
administer. Therefore, it is convenient to work with a compact view of the program, which can be
obtained by executing the `dotnet publish` command.

1. In project folder `ExtSort` execute command:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=false,PublishTrimmed=true,PublishSingleFile=true
```

2. Find needed files in subfolder:

`ExtSort\bin\Release\net6.0\win-x64\publish\`

3. Run with the command as in the example:

```powershell
$Time = [System.Diagnostics.Stopwatch]::StartNew()
& ".\ExtSort.exe" "sort" "input.txt" "output.txt" "IO"
write-host ('Completed in {0} seconds.' -f $Time.Elapsed.TotalSeconds)
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
```
