﻿// BMW.Rheingold.CoreFramework.Contracts.INativeError

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface INativeError
    {
        string Identifier { get; }

        string Message { get; }
    }
}
