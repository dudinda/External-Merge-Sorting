# External Merge Sorting

1. [Console interface](#console-interface)
2. [Algorithm](#algorithm)
   - [Splitting Phase](#splitting)
   - [Sorting/Merging Phase](#sortingmerging)
   - [Merging Phase](#merging)
3. [Strategy to merge a file 1 GB](#strategy-to-merge-a-file-1-gb)
4. [Strategy to merge a file 10 GB](#strategy-to-merge-a-file-10-gb)
5. [Created with](#created-with)
***

## Console Interface
The software provides a console interface with two verbs: [g]enerate, [s]ort for every program. To start sort an output file the target directory must be created. By default the path is ```C:\Temp\Files```.

```powershell
.\ExtSort.exe --help
```

<p align="center">
    <img src="https://github.com/user-attachments/assets/f948e35f-595e-42eb-a677-cb6431297b9e" width="600" height = "400" alt="original underexposed image">
    <p align="center">Fig. 1 - Using the option --help with the interface.</p>
</p>

To generate a file with the size of 1 GB the following command can be executed:
```powershell
.\ExtSort.exe generate output.txt 1048576
```

To start sorting a file with the correct data format the following command can be executed:
```powershell
.\ExtSort.exe sort output.txt output_sorted.txt 
```

***

## Algorithm
The provided software contains an implementation of the [External Sort](https://en.wikipedia.org/wiki/External_sorting) algorithm with a possible extension to split I/O operations between 2 drives. 
### Splitting 
During the splitting phase, a source file is sequentially read and split into blocks of ```FileSplitSizeKb``` size. For every chunk, the name is set as ```<counter>.unsorted```. In the end of each iteration the algorithm analyzes whether the current byte is set at the position of a line's end, and if not, continues to write byte-by-byte until the end of the line is met. After a file is persisted, a map of ```filename::number of lines``` is populated.
### Sorting/Merging
For every page, the program opens the ```SortPageSize``` streams each in  a separate task and starts to populate a buffer with priorities, the size of which was calculated during the Splitting phase. When a buffer is loaded, sorting occurs using the implemented [Test Task Comparer](https://github.com/dudinda/TestTask/blob/master/TestTask/Code/Comparers/TaskTemplateComparer.cs) to correspond to the requested template, then a space for a sorted file is allocated and the buffer is written into a file with the ```.sorted``` extension.  Right after a page was sorted a task to merge sorted files is executed. A possible strategy during the phase is to equate ```SortPageSize = SortThenMergePageSize x SortThenMergeChunkSize``` so a page starts merging into ```~sqrt(SortPageSize)``` files when the next page is being sorted.
### Merging
The common merging strategy is to try to converge files from bottom to top forming a [B-Tree](https://en.wikipedia.org/wiki/B-tree). A possible chain is ```64 -> 16 -> 4 -> 1```. Every ```MergePageSize``` opens ```MergeChunkSize``` streams, then reads the very first element, binds an index to it and continues to process every stream line-by-line sequentially enqueue a row to the priority queue, where the priority is set as a tuple of ```(<number>, <string>)``` using the [K-Way Merge](https://en.wikipedia.org/wiki/K-way_merge_algorithm). The first dequeued item is written to a target file. In case two drives are correctly set in the ```appsettings.json``` it is possible to merge files from one drive to another: ex: ```C:\\->E:\\->C:\\->```.

***
## Strategy to merge a file 1 GB

Specifying the following settings the algorithm will split a file into 64 chunks ~16MB each and start processing 4 pages of 16 files.
The general files merging strategy: ```64 -> 16``` (during the Sorting/Merging Phase) ```-> 4 -> 1``` (during the Merging Phase). All operations will be performed within the two drives C:\\ and E:\\. 

```json
"SorterSetting": {
  "FileSplitSizeKb": 16384,
  "SortPageSize": 16,
  "SortOutputBufferSize": 81920,
  "SortThenMergePageSize": 4,
  "SortThenMergeChunkSize": 4,
  "MergePageSize": 4,
  "MergeChunkSize": 4,
  "MergeOutputBufferSize": 16777216,
  "IOPath": {
    "SortReadPath": "C:\\Temp\\Files",
    "SortWritePath": "E:\\Temp\\Files",
    "MergeStartPath": "E:\\Temp\\Files",
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
}
```

***

## Strategy to merge a file 10 GB

Specifying the following settings the algorithm will split a file into 256 chunks ~41MB each and start processing 8 pages of 32 files.
The general merging strategy: ```256 -> 32``` (during the Sorting/Merging Phase) ```-> 4 -> 1``` (during the Merging Phase). All operations will be performed within the single drive C:\\.

```json
"SorterSetting": {
  "FileSplitSizeKb": 40960,
  "SortPageSize": 64,
  "SortOutputBufferSize": 81920,
  "SortThenMergePageSize": 8,
  "SortThenMergeChunkSize": 8,
  "MergePageSize": 8,
  "MergeChunkSize": 4,
  "MergeOutputBufferSize": 16777216,
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
[.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

[System.CommandLine](https://www.nuget.org/packages/System.CommandLine)

[Visual Studio Unit Tests](https://www.nuget.org/packages/Microsoft.NET.Test.SDK)

[Microsoft.Extensions.Configuration](https://www.nuget.org/packages/microsoft.extensions.configuration/)


