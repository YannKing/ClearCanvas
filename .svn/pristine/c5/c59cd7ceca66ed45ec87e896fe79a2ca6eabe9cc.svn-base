# This file is useful when doing development on Linux using X-Develop
# X-Develop will not execute the post-build steps that copy the files
# to the appropriate directories, so this manual script is helpful
# Copy it into the output folder (e.g. "bin/Debug Mono") for use.

# 使用X-Develop在Linux上进行开发时，此文件很有用 
# X-Develop不会执行复制文件的构建后步骤到适当的目录，所以这个手动脚本是有帮助的
# 将其复制到输出文件夹（例如“bin / Debug Mono”）以供使用。


# Delete contents of plugin directory 
# 删除插件目录的内容
rm -r -f ./plugins
rm -r -f ./common

# Make required directories
# 制作必需的目录
mkdir ./common
mkdir ./plugins

# Copy shared assemblies
# 复制共享程序集
cp "../../../../Common/bin/Debug Mono/ClearCanvas.Common.dll" ./common
cp "../../../../Dicom/bin/Debug Mono/ClearCanvas.Dicom.dll" ./common
cp "../../../../Dicom/OffisWrapper/csharp/Debug/ClearCanvas.Dicom.OffisWrapper.dll" ./common
cp "../../../../Dicom/OffisWrapper/cppwrapper/Debug/libOffisDcm.so" ./common
cp "../../../SplashScreen/bin/Debug Mono/ClearCanvas.Workstation.SplashScreen.dll" ./common

# Copy plugin assemblies
# 复制插件程序集
cp "../../../View/GTK/bin/Debug Mono/ClearCanvas.Workstation.View.GTK.dll" ./plugins
cp "../../../Model/bin/Debug Mono/ClearCanvas.Workstation.Model.dll" ./plugins
cp "../../../Tools/Measurement/bin/Debug Mono/ClearCanvas.Workstation.Tools.Measurement.dll" ./plugins
cp "../../../Tools/Standard/bin/Debug Mono/ClearCanvas.Workstation.Tools.Standard.dll" ./plugins
cp "../../../Edit/bin/Debug Mono/ClearCanvas.Workstation.Edit.dll" ./plugins
cp "../../../Layout/Basic/bin/Debug Mono/ClearCanvas.Workstation.Layout.Basic.dll" ./plugins
cp "../../../Renderer/GDI/bin/Debug Mono/ClearCanvas.Workstation.Renderer.GDI.dll" ./plugins
cp "../../../StudyFinders/Local/bin/Debug Mono/ClearCanvas.Workstation.StudyFinders.Local.dll" ./plugins
cp "../../../StudyLoaders/Local/bin/Debug Mono/ClearCanvas.Workstation.StudyLoaders.Local.dll" ./plugins

# Temp HACK - Copy shared assemblies into root folder, because "common" isn't known by mono
# Temp HACK  - 将共享程序集复制到根文件夹中，因为mono不知道“common”
cp "../../../../Common/bin/Debug Mono/ClearCanvas.Common.dll" .
cp "../../../../Dicom/bin/Debug Mono/ClearCanvas.Dicom.dll" .
cp "../../../../Dicom/OffisWrapper/csharp/Debug/ClearCanvas.Dicom.OffisWrapper.dll" .
cp "../../../../Dicom/OffisWrapper/cppwrapper/Debug/libOffisDcm.so" .
