# 🚀 快速测试指南

## ⏱️ 5分钟快速上手

### 第一步：运行测试脚本

**方法一：双击批处理文件（推荐）**
```
双击 test/run.bat
```

**方法二：命令行运行**
```bash
cd test
dotnet run
```

---

### 第二步：输入测试参数

#### 1. 输入用户 UID
```
请输入微博用户 UID (例如: 7523917567): 
```
💡 **如何获取 UID？**
- 访问微博用户主页：`https://weibo.com/u/7523917567`
- URL 最后的数字就是 UID

#### 2. 输入 Cookie
```
请输入微博 Cookie (从 weibo.cn 或 m.weibo.cn 获取): 
```
💡 **如何获取 Cookie？**
1. 浏览器访问 https://m.weibo.cn/
2. 登录账号
3. 按 F12 → Network → 刷新页面
4. 点击任意请求 → Request Headers → 复制 Cookie

#### 3. 输入测试页数
```
请输入要测试的页数 (默认 3): 
```
建议：首次测试使用 1-3 页

---

### 第三步：查看结果

测试脚本会输出：
- ✅ 用户基本信息（昵称、粉丝数等）
- ✅ 每条微博的详细内容
- ✅ 图片数量和示例 URL
- ✅ 视频是否存在
- ✅ 互动数据（点赞、评论、转发）
- ✅ 统计摘要

---

## 🎯 常见测试场景

### 场景 1：验证 API 是否正常
```bash
UID: 任意公开用户
页数: 1
目的: 确认 Cookie 有效，API 可访问
```

### 场景 2：测试图片下载逻辑
```bash
UID: 图片较多的用户
页数: 3-5
目的: 验证图片 URL 解析是否正确
```

### 场景 3：测试视频下载逻辑
```bash
UID: 视频博主
页数: 2-3
目的: 验证视频 URL 提取是否完整
```

### 场景 4：压力测试
```bash
UID: 高活跃用户
页数: 10+
延时: 修改为 5000-10000ms
目的: 测试长时间运行的稳定性
```

---

## 🔍 调试技巧

### 查看详细响应

在 `Helpers/HttpHelper.cs` 第 58 行后添加：
```csharp
Console.WriteLine($"[DEBUG] Response: {responseBody}");
```

### 跳过延时加速测试

在 `Program.cs` 第 178 行修改：
```csharp
int delay = rd.Next(100, 500);  // 改为更短的延时
```
⚠️ **警告**: 可能导致被封禁，仅用于本地调试

### 保存测试结果到文件

修改启动命令：
```bash
dotnet run > test_result.txt
```

---

## ❓ 故障排查

### 问题：编译失败

**错误**: `No .NET SDKs were found`

**解决**:
```bash
# 检查是否安装 .NET SDK
dotnet --version

# 如果未安装，下载 .NET 6.0 SDK
# https://dotnet.microsoft.com/download/dotnet/6.0
```

### 问题：运行时错误

**错误**: `Could not execute because the application was not found`

**解决**:
```bash
# 先恢复依赖
dotnet restore

# 再编译
dotnet build

# 最后运行
dotnet run
```

### 问题：Cookie 无效

**症状**: API 返回 ok=-100 或空数据

**解决**:
1. 清除浏览器缓存
2. 重新登录 m.weibo.cn
3. 获取全新 Cookie
4. 5 分钟内运行测试

---

## 📊 解读测试结果

### 成功示例
```
📊 该用户共有 1234 条微博
📋 当前页卡片数: 10

👤 用户信息:
   昵称: 测试用户
   UID: 7523917567
   
📝 微博 #1
   🖼️  图片数: 3
   👍 点赞: 100

✅ 测试完成！
```
✅ **表示**: API 正常，Cookie 有效

### 失败示例 1：验证码
```
❌ API 返回错误，ok=-100
⚠️  检测到验证码验证！
```
❌ **表示**: Cookie 被标记或频率过高
💡 **解决**: 等待 30-60 分钟或更换 Cookie

### 失败示例 2：无数据
```
⚠️  没有更多数据了
📊 测试统计摘要:
   总微博数: 0
```
❌ **表示**: UID 错误或用户无公开内容
💡 **解决**: 检查 UID 是否正确

---

## 🎓 学习路径

### 初级：理解 API 结构
1. 运行测试脚本
2. 观察 JSON 响应格式
3. 查看 `Models/WeiboCnMobileModel.cs`

### 中级：修改测试逻辑
1. 调整分页参数
2. 修改延时时间
3. 添加自定义过滤条件

### 高级：集成到主项目
1. 对比测试脚本和 `MainWindow.xaml.cs`
2. 理解完整的下载流程
3. 优化主项目代码

---

## 📞 需要帮助？

- 📖 详细文档: [README.md](README.md)
- 🔧 故障排除: [../TestScript/故障排除指南.md](../TestScript/故障排除指南.md)
- 💬 提交 Issue: GitHub Issues

---

**开始测试吧！** 🚀
