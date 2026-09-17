# Microservices
微服务，领域驱动，参考微软官方推荐的购物架构改造，邮箱系统

结构说明：
Application:业务管道；常用于创建MQ业务管道；应用程序入口；
一般配合MediatR使用，若是没使用MediatR可以不创建这个目录；
Domain: 业务模型配置；
infrastructure : 基础设施；写库配置，库操作（表配置，数据迁移，Repositories），链接库；
IntegrationEvents：事件队列模型及处理器
Repositories：仓储/持久处理
Services：业务实现，一般用于交互业务，domain一般处理领域业务。根据自己的情况灵活选择，不做强制要求。
Grpc：一般用于服务与服务间的通信，http2.X，可长连接。网络路由定义。采用谷歌推荐的数据存储结构，比json更小、更快。可更具api灵活选用。
Controllers：Api首选，浏览器支持。短连接无状态。http1.X。Grpc和Controllers走的是不同类型的通信管道，大神可以自己定义管道类型，实现不同的通信方式。
入口文件：src\Email\Email.API
这个架构是参考微软官方推荐的购物架构做了轻微改造，曲向低代码。



