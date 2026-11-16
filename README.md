# SBR

## Third-party dependencies

### Unity SerializeReference Extensions

SBR vendors [Unity SerializeReference Extensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions), version 2.1.0, under `Assets/ThirdParty/MackySoft.SerializeReferenceExtensions`.

It adds the `SubclassSelector` Inspector dropdown for fields marked with `[SerializeReference]`. Example:

```csharp
[SerializeReference, SubclassSelector]
private IAttributeValueCalculator currentValueCalculator;
```

The package is licensed under MIT. See `Assets/ThirdParty/MackySoft.SerializeReferenceExtensions/LICENSE` and `THIRD_PARTY_NOTICES.md`.
