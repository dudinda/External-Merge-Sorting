# Altium Test Task by Dudin D. A., Nov 2024

1. [Console interface](#console-interface)
2. [Algorithm](#algorithm)
   - [Splitting Phase](#)
   - [Sorting/Merging Phase](#)
   - [Merging Phase](#)
3. [Strategy to merge a file 1 GB / Time in a local environment](#strategy-to-merge-a-file-1-gb)
4. [Strategy to merge a file 10 GB / Time in a local environment](#strategy-to-merge-a-file-10-gb)
5. [Created with](#created-with)
***
## Console Interface
The program provides a console interface with the two verbs [g]enerate and [s]ort to generate a file size of N (kb) and to sort 

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
***
## Strategy to merge a file 1 GB

Specifing the following settings the algorithm will split a file into 64 chunks ~15MB each and start sorting 4 pages of 16 chunks each.
The general merging strategy: 64 -> 16 (during the Sorting/Merging Phase) -> 4 -> 1 (during the Merging Phase). All operations will be performed within the single drive C:\\.

```json
"SorterSetting": {
  "FileSplitSizeKb": 15625,
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
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
}
```

***
## Strategy to merge a file 10 GB

Specifing the following settings the algorithm will split a file into 256 chunks ~39MB each and start sorting 8 pages of 64 chunks each.
The general merging strategy: 256 -> 64 (during the Sorting/Merging Phase) -> 16 -> 4 -> 1 (during the Merging Phase). All operations will be performed within the single drive C:\\.

```json
"SorterSetting": {
  "FileSplitSizeKb": 39070,
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
    "MergeStartTargetPath": "C:\\Temp\\Files"
  }
}
```

***

## Created With

[System.CommandLine](https://www.nuget.org/packages/ImageProcessing.Microkernel.DIAdapter](https://www.nuget.org/packages/System.CommandLine)/)

[Visual Studio Unit Tests](https://www.nuget.org/packages/Microsoft.NET.Test.SDK)

[Microsoft.Extensions.Configuration](https://www.nuget.org/packages/microsoft.extensions.configuration/)

[Benchmark.Net](https://www.nuget.org/packages/BenchmarkDotNet)

