using Microsoft.VisualBasic.FileIO;
using Simple_Far_Manager.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace Simple_Far_Manager;

public partial class MainWindow : Window
{
	private DirectoryInfo _currentDirectoryLeft;
	private DirectoryInfo _currentDirectoryRight;

	public MainWindow()
	{
		InitializeComponent();

		_currentDirectoryLeft = new DirectoryInfo(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
		_currentDirectoryRight = new DirectoryInfo(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
	}

	private async Task LoadFilesLeft()
	{
		lvFiles.Items.Clear();

		DirectoryInfo parentDirectory = _currentDirectoryLeft.Parent!;

		if (parentDirectory != null)
			lvFiles.Items.Add(new FileItem { Name = "..", IsFolder = true });

		DirectoryInfo[] dirs = null!;
		FileInfo[] files = null!;

		await Task.Run(() =>
		{
			dirs = _currentDirectoryLeft.GetDirectories();
			files = _currentDirectoryLeft.GetFiles();
		});

		foreach (DirectoryInfo d in dirs)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				lvFiles.Items.Add(new FileItem { Name = d.Name, Size = "", DateModified = d.LastWriteTime.ToString(), IsFolder = true });
			});
		}

		foreach (FileInfo f in files)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				lvFiles.Items.Add(new FileItem { Name = f.Name, Size = f.Length.ToString(), DateModified = f.LastWriteTime.ToString(), IsFolder = false });
			});
		}
	}

	private async Task LoadFilesRight()
	{
		lvFilesRight.Items.Clear();

		DirectoryInfo parentDirectory = _currentDirectoryRight.Parent!;

		if (parentDirectory != null)
			lvFilesRight.Items.Add(new FileItem { Name = "..", IsFolder = true });

		DirectoryInfo[] dirs = null!;
		FileInfo[] files = null!;

		await Task.Run(() =>
		{
			dirs = _currentDirectoryRight.GetDirectories();
			files = _currentDirectoryRight.GetFiles();
		});


		foreach (DirectoryInfo d in dirs)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				lvFilesRight.Items.Add(new FileItem { Name = d.Name, Size = "", DateModified = d.LastWriteTime.ToString(), IsFolder = true });
			});
		}

		foreach (FileInfo f in files)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				lvFilesRight.Items.Add(new FileItem { Name = f.Name, Size = f.Length.ToString(), DateModified = f.LastWriteTime.ToString(), IsFolder = false });
			});
		}
	}

	private async void lvFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		=> await OpenLeft();

	private async void lvFilesRight_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		=> await OpenRight();

	private async void OpenLeft_Click(object sender, RoutedEventArgs e)
		=> await OpenLeft();

	private async void OpenRight_Click(object sender, RoutedEventArgs e)
		=> await OpenRight();

	private async Task OpenLeft()
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFiles.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFiles.SelectedItem;

					if (selectedItem.IsFolder)
					{
						if (selectedItem.Name == "..")
							_currentDirectoryLeft = _currentDirectoryLeft.Parent!;
						else
							_currentDirectoryLeft = new DirectoryInfo(Path.Combine(_currentDirectoryLeft.FullName, selectedItem.Name));

						await LoadFilesLeft();
					}
					else
					{
						var path = Path.Combine(_currentDirectoryLeft.FullName, selectedItem.Name);

						using Process fileopener = new Process();

						fileopener.StartInfo.FileName = "explorer";
						fileopener.StartInfo.Arguments = "\"" + path + "\"";
						fileopener.Start();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async Task OpenRight()
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFilesRight.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFilesRight.SelectedItem;

					if (selectedItem.IsFolder)
					{
						if (selectedItem.Name == "..")
							_currentDirectoryRight = _currentDirectoryRight.Parent!;
						else
							_currentDirectoryRight = new DirectoryInfo(Path.Combine(_currentDirectoryRight.FullName, selectedItem.Name));

						await LoadFilesRight();
					}
					else
					{
						var path = Path.Combine(_currentDirectoryRight.FullName, selectedItem.Name);

						using Process fileopener = new Process();

						fileopener.StartInfo.FileName = "explorer";
						fileopener.StartInfo.Arguments = "\"" + path + "\"";
						fileopener.Start();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void DeleteLeft_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFiles.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFiles.SelectedItem;
					var path = (Path.Combine(_currentDirectoryLeft.FullName, (lvFiles.SelectedItem as FileItem)!.Name));

					if (selectedItem.IsFolder)
						FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
					else
						FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void DeleteRight_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFilesRight.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFilesRight.SelectedItem;

					var path = (Path.Combine(_currentDirectoryRight.FullName, (lvFilesRight.SelectedItem as FileItem)!.Name));

					if (selectedItem.IsFolder)
						FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
					else
						FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void MoveLeft_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFiles.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFiles.SelectedItem;
					var path = (Path.Combine(_currentDirectoryLeft.FullName, (lvFiles.SelectedItem as FileItem)!.Name));

					if (selectedItem.IsFolder)
					{
						if (!Directory.Exists(_currentDirectoryRight.FullName))
							Directory.CreateDirectory(_currentDirectoryRight.FullName);

						Directory.Move(path, Path.Combine(_currentDirectoryRight.FullName, Path.GetFileName(path)));
					}
					else
					{
						if (!Directory.Exists(_currentDirectoryRight.FullName))
							Directory.CreateDirectory(_currentDirectoryRight.FullName);

						// Get the file name and destination file path
						string fileName = Path.GetFileName(path);
						string destFilePath = Path.Combine(_currentDirectoryRight.FullName, fileName);

						File.Move(path, destFilePath);
					}

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void MoveRight_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFilesRight.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFilesRight.SelectedItem;
					var path = (Path.Combine(_currentDirectoryRight.FullName, (lvFilesRight.SelectedItem as FileItem)!.Name));

					if (selectedItem.IsFolder)
					{
						if (!Directory.Exists(_currentDirectoryLeft.FullName))
							Directory.CreateDirectory(_currentDirectoryLeft.FullName);

						Directory.Move(path, Path.Combine(_currentDirectoryLeft.FullName, Path.GetFileName(path)));
					}
					else
					{
						if (!Directory.Exists(_currentDirectoryLeft.FullName))
							Directory.CreateDirectory(_currentDirectoryLeft.FullName);

						// Get the file name and destination file path
						string fileName = Path.GetFileName(path);
						string destFilePath = Path.Combine(_currentDirectoryLeft.FullName, fileName);

						File.Move(path, destFilePath);

					}

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void CopyLeft_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFiles.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFiles.SelectedItem;
					var sourcePath = (Path.Combine(_currentDirectoryLeft.FullName, (lvFiles.SelectedItem as FileItem)!.Name));
					var destPath = _currentDirectoryRight.FullName;

					if (selectedItem.IsFolder)
					{
						if (!Directory.Exists(destPath))
							Directory.CreateDirectory(destPath);

						foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", System.IO.SearchOption.AllDirectories))
							Directory.CreateDirectory(Path.Combine(destPath, dirPath.Substring(sourcePath.Length + 1)));

						foreach (string filePath in Directory.GetFiles(sourcePath, "*", System.IO.SearchOption.AllDirectories))
							File.Copy(filePath, Path.Combine(destPath, filePath.Substring(sourcePath.Length + 1)));
					}
					else
					{
						if (!Directory.Exists(destPath))
							Directory.CreateDirectory(destPath);

						// Get the file name and destination file path
						string fileName = Path.GetFileName(sourcePath);
						string destFilePath = Path.Combine(destPath, fileName);

						File.Copy(sourcePath, destFilePath);
					}

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}

	private async void CopyRight_Click(object sender, RoutedEventArgs e)
	{
		await Dispatcher.InvokeAsync(async () =>
		{
			try
			{
				if (lvFilesRight.SelectedItem != null)
				{
					FileItem selectedItem = (FileItem)lvFilesRight.SelectedItem;
					var destDirPath = _currentDirectoryLeft.FullName;
					var sourceDirPath = (Path.Combine(_currentDirectoryRight.FullName, (lvFilesRight.SelectedItem as FileItem)!.Name));

					if (selectedItem.IsFolder)
					{
						if (!Directory.Exists(destDirPath))
							Directory.CreateDirectory(destDirPath);

						foreach (string dirPath in Directory.GetDirectories(sourceDirPath, "*", System.IO.SearchOption.AllDirectories))
							Directory.CreateDirectory(Path.Combine(destDirPath, dirPath.Substring(sourceDirPath.Length + 1)));

						foreach (string filePath in Directory.GetFiles(sourceDirPath, "*", System.IO.SearchOption.AllDirectories))
							File.Copy(filePath, Path.Combine(destDirPath, filePath.Substring(sourceDirPath.Length + 1)));
					}
					else
					{
						if (!Directory.Exists(destDirPath))
							Directory.CreateDirectory(destDirPath);

						string fileName = Path.GetFileName(sourceDirPath);
						string destFilePath = Path.Combine(destDirPath, fileName);

						File.Copy(sourceDirPath, destFilePath);
					}

					await LoadFilesLeft();
					await LoadFilesRight();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		});
	}


	#region OpenPropertiesDialog
	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct SHELLEXECUTEINFO
	{
		public int cbSize;
		public uint fMask;
		public IntPtr hwnd;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpVerb;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpFile;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpParameters;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpDirectory;
		public int nShow;
		public IntPtr hInstApp;
		public IntPtr lpIDList;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpClass;
		public IntPtr hkeyClass;
		public uint dwHotKey;
		public IntPtr hIcon;
		public IntPtr hProcess;
	}

	private const int SW_SHOW = 5;
	private const uint SEE_MASK_INVOKEIDLIST = 12;
	public static bool ShowFileProperties(string Filename)
	{
		SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
		info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
		info.lpVerb = "properties";
		info.lpFile = Filename;
		info.nShow = SW_SHOW;
		info.fMask = SEE_MASK_INVOKEIDLIST;
		return ShellExecuteEx(ref info);
	}

	#endregion

	private void PropertiesLeft_Click(object sender, RoutedEventArgs e)
	{
		if (lvFiles.SelectedItem is null)
			return;

		var path = (Path.Combine(_currentDirectoryLeft.FullName, (lvFiles.SelectedItem as FileItem)!.Name));
		ShowFileProperties(path);
	}

	private void PropertiesRight_Click(object sender, RoutedEventArgs e)
	{
		if (lvFilesRight.SelectedItem is null)
			return;

		var path = (Path.Combine(_currentDirectoryRight.FullName, (lvFilesRight.SelectedItem as FileItem)!.Name));
		ShowFileProperties(path);
	}

	private async void lvFiles_KeyDown(object sender, KeyEventArgs e)
	{
		if (lvFiles.SelectedItem is null)
			return;

		switch (e.Key)
		{
			case Key.Enter:
				await OpenLeft();
				break;
			case Key.F1:
				await OpenLeft();
				break;
			case Key.F2:
				CopyLeft_Click(sender, e);
				break;
			case Key.F3:
				MoveLeft_Click(sender, e);
				break;
			case Key.F4:
				DeleteLeft_Click(sender, e);
				break;
			case Key.F5:
				PropertiesLeft_Click(sender, e);
				break;
		}
	}

	private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is ListView l)
		{
			if (l.Name == lvFiles.Name)
				lvFilesRight.SelectedItem = null;
			else
				lvFiles.SelectedItem = null;
		}
	}

	private async void lvFilesRight_KeyDown(object sender, KeyEventArgs e)
	{
		if (lvFilesRight.SelectedItem is null)
			return;

		switch (e.Key)
		{
			case Key.Enter:
				await OpenRight();
				break;
			case Key.F1:
				await OpenRight();
				break;
			case Key.F2:
				CopyRight_Click(sender, e);
				break;
			case Key.F3:
				MoveRight_Click(sender, e);
				break;
			case Key.F4:
				DeleteRight_Click(sender, e);
				break;
			case Key.F5:
				PropertiesRight_Click(sender, e);
				break;
		}
	}

	private async void Window_Loaded(object sender, RoutedEventArgs e)
	{
		await LoadFilesLeft();
		await LoadFilesRight();
	}
}