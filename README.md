## 概要
将一个文件夹内的所有文件打包成一个文件（类似一个不压缩大小的压缩包）

## 使用
引用命名空间 FolderPack

### 1 把文件夹转换成一个打包文件

#### 1.1 创建一个<font color = red>Folder</font>对象：
Folder folder = new Folder(path);  

#### 1.2 把Folder对象保存到某个打包文件当中：
folder.SaveToPack(packPath);

### 2 从一个打包文件中读取一个文件的byte数组

#### 2.1 将一个打包文件的路径和其原文件夹中的子目录相对路径一起传入GetBytesFromPack函数：
byte[] bytes = Folder.GetBytesFromPack(packPath , relativePath);  
即可得到某个文件的byte数组

### 3 将一个打包文件中的原文件完全解包到指定路径下

#### 3.1 使用Depack函数：
Folder.Depack(packPath , targetPath);
即可把打包文件中的所有文件解包到targetPath下

