# Duplicate File Finder (Windows App)

A fast and efficient C# Windows Forms application to find and remove duplicate files from your computer.

![Main Screenshot](https://via.placeholder.com/800x450.png?text=Duplicate+File+Finder+Screenshot)

## 🚀 Features

- **Smart Hashing**: Groups files by size first, then performs hashing on potential duplicates to maximize speed.
- **Large File Optimization**: Uses partial hashing (first + last 10MB) for files larger than 50MB to significantly reduce scan time.
- **Preview Panel**: Instantly preview images or view system icons for other duplicate files in the results list.
- **Flexible Filters**: Scan for specific types:
  - Images (`.jpg`, `.png`, `.gif`, etc.)
  - Videos (`.mp4`, `.mkv`, etc.)
  - PDFs
  - Documents & Audio
- **Safety First**: Deletes duplicates to the **Recycle Bin**, so you can always restore them if needed.
- **One-Click Cleanup**: "Select All Duplicates" auto-selects all but the first instance in every group.
- **Explorer Integration**: Double-click any result to open its location in Windows Explorer.

## 🛠️ Requirements

- Windows OS
- [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)

## 📥 Installation & Running

### Option 1: Using the Command Line
1. Clone the repository:
   ```bash
   git clone https://github.com/learn-with-santosh/duplicate-file-finder.git
   ```
2. Navigate to the project folder:
   ```bash
   cd DuplicateFileFinder
   ```
3. Run the app:
   ```bash
   dotnet run
   ```

### Option 2: Using Visual Studio
1. Open `DuplicateFileFinder.sln` in Visual Studio 2022 or newer.
2. Press **F5** to build and run.

## ⚙️ How it Works

1. **Path Selection**: Choose any drive (C:\, D:\) or folder.
2. **File Selection**: Pick the extensions you want to check.
3. **Scan**: The app collects all files, sorts them by size, and then calculates MD5 hashes for files that share the same size.
4. **Identify**: Exact duplicates are grouped together.
5. **Manage**: Review the results using the **Preview Panel** and choose which ones to delete.

## 📄 License

This project is open-source and available under the MIT License.
