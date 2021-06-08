using System;
namespace client
{
    public interface IKeepAlive
    {
        event EventHandler ServerDown;
        event EventHandler KeepAliveDown;
    }
}
