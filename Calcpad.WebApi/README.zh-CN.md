# 使用说明

[English](/README.md) | [简体中文](/README.zh-CN.md)

## 简介

Calcpad.WebApi 以 API 的方式提供 Calcpad.Core 能力。可以将该 API 集成到既有项目里，实现 calcpad web 化。


## 脚本

本项目基于 Powershell 提供跨平台脚本，使用它们可以简化部署操作：

- zip.ps1

  打包 WebApi 项目及所有依赖项目, 方便上传到服务器上进行编译部署。

- docker-build.ps1

  编译 docker 镜像，最终镜像名为：`calcpad/webapi:latest`

**温馨提示**

powershell 在非 windows 平台上，需要手动安装, 参考：[Install PowerShell on Windows, Linux, and macOS](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.5)

## 安装使用

推荐使用 docker 的方式进行部署，在部署前，请提前安装 MongoDB 数据库。

本节将介绍如何在 docker 中部署 WebApi。

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

## 功能介绍

### 授权认证

**配置初始用户**

在 `appsettings.Production.json` 中配置 `Users` 字段，作为系统的初始用户，格式如下：

``` json
{
  "Users": [
    {
      "Username": "admin",
      "Password": "calcpad",
      "Roles": [ "admin", "api" ]
    },
    {
      "Username": "calcpad",
      "Password": "calcpad1234"
    }
  ],
}
```

**授权认证**

系统使用 JWT 进行权限验证, 在调用对应接口时，需要提前进行登录。

登录的接口为 `Post /api/v1/user/sign-in`, body 参数为：

``` json
{
  "username":"",
  "password":""
}
```

### 基础功能

在 ClalcpadFileController 中提供了对文件的上传、复制、执行、下载等功能，请查看对应的接口进行了解。

**特别说明：**

Web 版本后台存储的文件路径为系统自动生成，在对接 API 时，不应依赖于路径进行管理。在进行文件操作时，主要依赖 `UniqueId` 进行文件的查找访问。

### AI 翻译

在 I18nController 提供了对结果进行 AI 翻译的功能，此处进行简要介绍。

**AI 配置**

在 `appsettings.Production.json` 中通过字段 `AI` 和 `Translation` 进行配置。具体请参考 `appsettings.json` 中的注释。

翻译功能依赖于 AI 自动生成初始翻译，因此必须配置并启用 AI，当前仅兼容了 OpenAI 方式调用，若接口适配 OpenAI 接口规范，也可以使用。

**使用步骤**

1. 配置 `AI`
2. 配置 `Translation` 提示词 [可选]
3. 调用接口生成多语言
4. 在执行计算时，同时传递需要翻译的语言参数

**修改翻译**

若 AI 翻译的结果不准确，可以通过调用 API 对每条翻译的记录进行修改，修改后的字段，AI 将不再进行翻译。

### 版本升级

版本升级可以当成复制 Calcpad 文件操作来处理。

版本升级时，涉及到数据的迁移处理，目前已知的有两种类型：

1. 通过 `?` 生成的用户输入
2. `#read` 宏读取的文件路径(该操作非 CalcpadCore 所拥有，为本系统新增功能)

为了解决上述两种数据迁移，真实的迁移步骤为：

1. 从新模板复制一个 Calcpad 文件
2. 提取老文件中的 #read 列表
3. 将老文件中的 #read 列表赋予到新的文件中(不单独复制，保证新老数据为同一数据引用)
4. 将老文件中的变量值赋予到新的文件中