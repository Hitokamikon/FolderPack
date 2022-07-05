using System.IO;

namespace FolderPack
{
    /// <summary>
    /// 扩展文件信息
    /// </summary>
    internal class ExtendedFileSystemInfo
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        internal FileSystemInfo fileSystemInfo { get; private set; }

        /// <summary>
        /// 文件存储首地址
        /// </summary>
        internal long position { get; private set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        internal long length { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileSystemInfo">文件信息</param>
        /// <param name="position">文件存储首地址</param>
        /// <param name="length">文件长度</param>
        internal ExtendedFileSystemInfo(FileSystemInfo fileSystemInfo, long position, long length)
        {
            this.fileSystemInfo = fileSystemInfo;
            this.position = position;
            this.length = length;
        }
    }
}
