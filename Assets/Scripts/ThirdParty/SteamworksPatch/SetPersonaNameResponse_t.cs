// 临时补丁:为Heathen Steamworks提供缺失的Steamworks.NET类型定义
// 这个文件修复了Steamworks.NET中缺失的SetPersonaNameResponse_t类型

namespace Steamworks
{
    /// <summary>
    /// SetPersonaName响应结果
    /// 这是一个占位符定义,因为当前版本的Steamworks.NET不包含此类型
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct SetPersonaNameResponse_t
    {
        public const int k_iCallback = 300 + 47; // 基于Steam API文档的估计值

        public bool m_bSuccess;
        public bool m_bLocalSuccess;
        public EResult m_result;
    }
}
