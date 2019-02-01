:: Re-generates the dbml file from the sdf file
:: The output is sent to a file name temp.dbml.  From there, you should manually merge your changes into the ConfigurationDataContext.dbml file.  Do not overwrite ConfigurationDataContext.dbml entirely.

:: 从sdf文件重新生成dbml文件
:: 输出将发送到文件名temp.dbml。从那里，您应该手动将更改合并到ConfigurationDataContext.dbml文件中。不要完全覆盖ConfigurationDataContext.dbml。

sqlmetal /pluralize /namespace:ClearCanvas.ImageViewer.StudyManagement.Core.Configuration /dbml:temp.dbml /context:ConfigurationDataContext configuration.sdf