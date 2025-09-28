# 使用说明

## 简介

Calcpad.WebApi 以 api 的方式提供 Calcpad.Core 能力。可以将该 api 集成到既有项目里，实现 calcpad web 化。


## 脚本

本项目基于 Powershell 提供跨平台脚本,使用它们可以简化部署操作：

- zip.ps1

  打包 WebApi 项目及所有依赖项目, 方便上传到服务器上进行编译部署。

- docker-build.ps1

  编译 docker 镜像，最终镜像名为：`calcpad/webapi:latest`

**温馨提示**

powershell 在非 windows 平台上，需要手动安装, 参考：[Install PowerShell on Windows, Linux, and macOS](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.5)

## 使用

推荐使用 docker 的方式进行部署。本节将介绍如何在 docker 中部署 WebApi。

1. 打包项目

定位到 WebApi 根目录

``` powershell
# 执行打包
./zip.ps1
```

2. 将打包的文件上传到服务器进行解压

若是本机 docker 部署，则可忽略这一步

3. 在服务器上编译镜像

定位到 Calcpad.WebApi 目录

``` powershell
./build.ps1
```

4. 初始配置

将 `docker-compose.yml`、`config` 复制到安装位置。示例：

``` bash
cp ./data ~/app/calcpad
```

然后参考 `Calcpad.WebApi/appsettings.json` 在 `appsettings.Production.json` 中将设置修改为生产配置。若仅是测试，可以不用修改。

4. 启动

``` bash
cd ~/app/calcpad
sudo docker compose up -d
```

## 接口

**swagger**

启动项目调试后，项目会生成 swagger 接口界面，地址为：[http://localhost:5098/swagger/index.html](http://localhost:5098/swagger/index.html), 通过该界面可查看接口

当前已实现接口 swagger.json

可以将 `api/swagger.json` 或者 [http://localhost:5098/swagger/v1/swagger.json](http://localhost:5098/swagger/v1/swagger.json) 导入到 Api 管理软件中查看，如 Apifox 等


## 接入既有服务

1. 服务器通过 `/api/v1/user/sign-in` 获取 token，并定期更新 token
2. 通过 `CalcpadFileController.cs` 中的接口上传 calcpad 文件，例如 .txt, .cpd, .cpdz
3. 对于非 .cpdz 文件, 需要注意
   - 图片资源需要先通过 `/api/v1/calcpad-file` 上传并获取资源 url

   - 对于 include 文件，需要先上传，然后获取该文件的 `uniqueId`, 在主文件中像这样使用
   
     ``` tex
      #include ./included_cpd.txt'?uid=uniqueId'
     ```
   
     在正常的路径后，使用 `'?uid=xxx'` 格式将包含的文件 id 写入到文件中，然后再上传。
      这样是为了让服务器识别引用的文件

## 开发

使用 VisualStudio 打开项目后，即可进行调试开发。
