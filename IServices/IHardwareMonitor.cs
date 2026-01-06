namespace HardwareMonitor.IServices;

public interface IHardwareMonitor : IDisposable
{
    event Action FileChanged;
    void StartMonitoring();
    void StopMonitoring();
}
