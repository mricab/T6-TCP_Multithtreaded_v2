using System;
namespace client
{
    public interface IServerHandler
    {
        event EventHandler UserQuitted;
    }
}
