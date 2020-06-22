using System.Collections;

namespace SuperGMS.FileEx
{
    public interface ISftpClient
    {
        bool Connected { get; }
        bool Connect();
        void Disconnect();
        bool Put(string localPath, string remotePath);
        bool Put(byte[] fileContent, string remotePath);
        bool PutStream(System.IO.Stream localFile, string remotePath);
        bool Move(string oldPath, string newPath);
        void Get(string fromFilePath);
        void Get(string[] fromFilePaths);
        void Get(string[] fromFilePaths, string toDirPath);
        void Get(string fromFilePath, string toFilePath);
        bool Delete(string remoteFile);
        ArrayList GetFileList(string remotePath, string fileType);
        ArrayList GetFileList(string path);

        /// <summary>
        /// 目录是否存在
        /// </summary>
        /// <param name="dirName">目录名称必须从根开始</param>
        /// <returns></returns>
        bool DirExist(string dirName);

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dirName">目录名称必须从根开始</param>
        /// <returns></returns>
        void Mkdir(string dirName);
    }
}