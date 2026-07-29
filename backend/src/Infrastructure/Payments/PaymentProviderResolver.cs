using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Infrastructure.Payments;

public class PaymentProviderResolver(IEnumerable<IPaymentProvider> providers) : IPaymentProviderResolver
{
    public IPaymentProvider Resolve(string providerKey)
    {
        var provider = providers.FirstOrDefault(p => p.ProviderKey.Equals(providerKey, StringComparison.OrdinalIgnoreCase));
        return provider ?? throw new BadRequestException($"Payment provider '{providerKey}' is not enabled.");
    }
}
