@echo off
chcp 65001 >nul
echo ========================================
echo   m.weibo.cn API 测试脚本 - 快速启动
echo ========================================
echo.

cd /d "%~dp0"

echo [1/2] 恢复依赖包...
dotnet restore
if errorlevel 1 (
    echo ❌ 依赖包恢复失败
    pause
    exit /b 1
)

echo.
echo [2/2] 运行测试脚本...
echo.
dotnet run

echo.
pause
