using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FolderPack
{
    /// <summary>
    /// 文件夹
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string path { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">路径</param>
        public Folder(string path)
        {
            this.path = Path.GetFullPath(path);
        }

        /// <summary>
        /// 将文件夹保存为一个文件
        /// </summary>
        /// <param name="packPath">文件地址</param>
        /// <exception cref="Exception">保存失败</exception>
        public void SaveToPack(string packPath)
        {
            if(!Directory.Exists(path))
                throw new Exception(string.Format("找不到路径【{0}】" , path));

            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            try
            {
                fileStream = File.Create(packPath);
                streamWriter = new StreamWriter(fileStream , Encoding.UTF8);
                List<ExtendedFileSystemInfo> fileSystemInfos = new List<ExtendedFileSystemInfo>();
                WriteFiles(fileSystemInfos , streamWriter);
                WriteHead(fileSystemInfos , streamWriter);
            }
            finally
            {
                if(streamWriter != null)
                    streamWriter.Close();
                if(fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// 从一个文件解包出文件到指定的目标目录下
        /// </summary>
        /// <param name="packPath">文件地址</param>
        /// <param name="targetPath">目标目录</param>
        static public void Depack(string packPath , string targetPath)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            try
            {
                fileStream = new FileStream(packPath, FileMode.Open);
                streamReader = new StreamReader(fileStream);
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                int count = 0;
                int readPosition = 0;
                string headLine = streamReader.ReadLine();
                readPosition += Encoding.UTF8.GetByteCount(headLine) + 2;
                List<ExtendedFileSystemInfo> extendedFileSystemInfos = new List<ExtendedFileSystemInfo>();

                if (!int.TryParse(headLine, out count))
                {
                    return ;
                }
                for (int i = 0; i < count; i++)
                {
                    string line = streamReader.ReadLine();
                    readPosition += Encoding.UTF8.GetByteCount(line) + 2;
                    int pathId = line.IndexOf(':');
                    string path = line.Substring(0, pathId);
                    path = string.Format("{0}/{1}", targetPath, path);

                    string[] paras = line.Substring(pathId + 1, line.Length - pathId - 1).Split(',');
                    int position = int.Parse(paras[0]);
                    int length = int.Parse(paras[1]);
                    FileAttributes fileAttributes = (FileAttributes)int.Parse(paras[2]);
                    long CreationTime = long.Parse(paras[3]);
                    long CreationTimeUtc = long.Parse(paras[4]);
                    long LastAccessTime = long.Parse(paras[5]);
                    long LastAccessTimeUtc = long.Parse(paras[6]);
                    long LastWriteTime = long.Parse(paras[7]);
                    long LastWriteTimeUtc = long.Parse(paras[8]);

                    if (position < 0)
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        directoryInfo.Attributes = fileAttributes;

                        directoryInfo.CreationTime = DateTime.FromBinary(CreationTime);
                        directoryInfo.CreationTimeUtc = DateTime.FromBinary(CreationTimeUtc);
                        directoryInfo.LastAccessTime = DateTime.FromBinary(LastAccessTime);
                        directoryInfo.LastAccessTimeUtc = DateTime.FromBinary(LastAccessTimeUtc);
                        directoryInfo.LastWriteTime = DateTime.FromBinary(LastWriteTime);
                        directoryInfo.LastWriteTimeUtc = DateTime.FromBinary(LastWriteTimeUtc);
                    }
                    else
                    {
                        if (!File.Exists(path))
                        {
                            FileStream fs = File.Create(path);
                            fs.Close();
                        }
                        FileInfo fileInfo = new FileInfo(path);
                        fileInfo.Attributes = fileAttributes;
                        fileInfo.CreationTime = DateTime.FromBinary(CreationTime);
                        fileInfo.CreationTimeUtc = DateTime.FromBinary(CreationTimeUtc);
                        fileInfo.LastAccessTime = DateTime.FromBinary(LastAccessTime);
                        fileInfo.LastAccessTimeUtc = DateTime.FromBinary(LastAccessTimeUtc);
                        fileInfo.LastWriteTime = DateTime.FromBinary(LastWriteTime);
                        fileInfo.LastWriteTimeUtc = DateTime.FromBinary(LastWriteTimeUtc);
                        extendedFileSystemInfos.Add(new ExtendedFileSystemInfo(fileInfo, position, length));
                    }
                }

                for (int i = 0; i < extendedFileSystemInfos.Count; i++)
                {
                    ExtendedFileSystemInfo extendedFileSystemInfo = extendedFileSystemInfos[i];
                    fileStream.Position = readPosition + extendedFileSystemInfo.position;
                    byte[] bytes = new byte[extendedFileSystemInfo.length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    FileStream fs = null;

                    try
                    {
                        fs = new FileStream(extendedFileSystemInfo.fileSystemInfo.FullName, FileMode.Open);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// 根据相对路径获取目标文件下子文件的byte数组
        /// </summary>
        /// <param name="packPath">目标文件地址</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>指定的子文件的byte数组</returns>
        static public byte[] GetBytesFromPack(string packPath , string relativePath)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            relativePath = Path.GetFullPath(relativePath);
            try
            {
                fileStream = new FileStream(packPath, FileMode.Open);
                streamReader = new StreamReader(fileStream);
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                int count = 0;
                int readPosition = 0;
                string headLine = streamReader.ReadLine();
                readPosition += Encoding.UTF8.GetByteCount(headLine) + 2;
                if (!int.TryParse(headLine, out count))
                {
                    return null;
                }
                for(int i = 0; i < count; i++)
                {
                    string line = streamReader.ReadLine();
                    readPosition += Encoding.UTF8.GetByteCount(line) + 2;
                    int pathId = line.IndexOf(':');
                    string path = line.Substring(0, pathId);
                    path = Path.GetFullPath(path);
                    if(path == relativePath)
                    {
                        string[] paras = line.Substring(pathId + 1, line.Length - pathId - 1).Split(',');
                        i++;
                        for(;i < count;i++)
                        {
                            line = streamReader.ReadLine();
                            readPosition += Encoding.UTF8.GetByteCount(line) + 2;
                        }
                        int position = int.Parse(paras[0]);
                        int length = int.Parse(paras[1]);

                        streamReader.BaseStream.Seek(position + readPosition, SeekOrigin.Begin);
                        byte[] data = new byte[length];
                        streamReader.BaseStream.Read(data, 0, length);

                        return data;
                    }
                }
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
                if (fileStream != null)
                    fileStream.Close();
            }

            return null;
        }

        void WriteFiles(List<ExtendedFileSystemInfo> fileSystemInfos , StreamWriter streamWriter)
        {
            ReadFilesToList(new DirectoryInfo(path), fileSystemInfos, streamWriter);
        }

        void WriteHead(List<ExtendedFileSystemInfo> fileSystemInfos, StreamWriter streamWriter)
        {
            byte[] data = new byte[streamWriter.BaseStream.Length];
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            streamWriter.BaseStream.Read(data, 0, data.Length);

            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            streamWriter.WriteLine(fileSystemInfos.Count.ToString());

            for (int i = 0; i < fileSystemInfos.Count;i++)
            {
                ExtendedFileSystemInfo extendedFileSystemInfo = fileSystemInfos[i];
                string relativePath = GetRelativePath(path, extendedFileSystemInfo.fileSystemInfo.FullName);
                streamWriter.WriteLine(string.Format("{0}:{1},{2},{3},{4},{5},{6},{7},{8},{9}", relativePath, 
                    extendedFileSystemInfo.position,
                    extendedFileSystemInfo.length, 
                    (int)extendedFileSystemInfo.fileSystemInfo.Attributes,
                    extendedFileSystemInfo.fileSystemInfo.CreationTime.ToBinary(),
                    extendedFileSystemInfo.fileSystemInfo.CreationTimeUtc.ToBinary(),
                    extendedFileSystemInfo.fileSystemInfo.LastAccessTime.ToBinary(),
                    extendedFileSystemInfo.fileSystemInfo.LastAccessTimeUtc.ToBinary(),
                    extendedFileSystemInfo.fileSystemInfo.LastWriteTime.ToBinary(),
                    extendedFileSystemInfo.fileSystemInfo.LastWriteTimeUtc.ToBinary()));
            }
            streamWriter.Flush();
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            streamWriter.BaseStream.Write(data , 0 , data.Length);
            streamWriter.Flush();
        }

        void ReadFilesToList(DirectoryInfo directoryInfo , List<ExtendedFileSystemInfo> fileSystemInfos , StreamWriter streamWriter)
        {
            if (directoryInfo.FullName != path)
                fileSystemInfos.Add(new ExtendedFileSystemInfo(directoryInfo , -1 , 0));
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            for(int i = 0; i < directoryInfos.Length; i++)
            {
                DirectoryInfo child = directoryInfos[i];
                ReadFilesToList(child, fileSystemInfos, streamWriter);
            }

            FileInfo[] fileInfos = directoryInfo.GetFiles();
            for(int i = 0; i< fileInfos.Length;i++)
            {
                FileInfo child = fileInfos[i];

                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(child.FullName, FileMode.Open, FileAccess.Read);
                    fileSystemInfos.Add(new ExtendedFileSystemInfo(child, streamWriter.BaseStream.Position, fileStream.Length));
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, (int)fileStream.Length);
                    streamWriter.BaseStream.Write(bytes, 0, bytes.Length);
                    streamWriter.Flush();
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
        }

        /// <summary>
        /// 关于获取相对路径的计算,获得pathB相对于pathA
        /// </summary>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <returns>pathB相对于pathA的相对路径</returns>
        string GetRelativePath(string pathA, string pathB)
        {
            string[] pathAArray = pathA.Split(new char[] { '\\', '/' } , StringSplitOptions.RemoveEmptyEntries);
            string[] pathBArray = pathB.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            //返回2者之间的最小长度
            int s = pathAArray.Length >= pathBArray.Length ? pathBArray.Length : pathAArray.Length;
            //两个目录最底层的共用目录的索引
            int closestRootIndex = -1;
            for (int i = 0; i < s; i++)
            {
                if (pathAArray[i] == pathBArray[i])
                {
                    closestRootIndex = i;
                }
                else
                {
                    break;
                }
            }
            //由pathA计算 ‘../’部分
            string pathADepth = "";
            for (int i = 0; i < pathAArray.Length; i++)
            {
                if (i > closestRootIndex + 1)
                {
                    pathADepth += "../";
                }
            }
            //由pathB计算‘../’后面的目录
            string pathBdepth = "";
            for (int i = closestRootIndex + 1; i < pathBArray.Length; i++)
            {
                pathBdepth += "/" + pathBArray[i];
            }
            pathBdepth = pathBdepth.Substring(1);//去掉重复的斜杠 “ / ”
            return pathADepth + pathBdepth;//pathB相对于pathA的相对路径
        }


    }
}
