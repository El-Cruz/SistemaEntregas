using CommunityToolkit.Mvvm.Messaging.Messages;
using Entregas.Shared;

namespace Entregas.MAUI.Utilities
{
    // Simple typed messages so VM's can subscribe strongly-typed
    public sealed class EntregaCreadaMessage : ValueChangedMessage<EntregaModel>
    {
        public EntregaCreadaMessage(EntregaModel value) : base(value) { }
    }

    public sealed class EntregaConcretadaMessage : ValueChangedMessage<EntregaModel>
    {
        public EntregaConcretadaMessage(EntregaModel value) : base(value) { }
    }
}