# Contratos da Saga

Cópia local, byte a byte, do pacote [`Wrench.Contracts`](https://github.com/euthribeiro/wrench-contracts)
v1.0.0, no mesmo namespace `Wrench.Contracts`.

A cópia existe para que este repositório compile sem depender do feed autenticado do GitHub
Packages. A troca pelo pacote é uma operação de duas linhas e não toca em nenhum outro arquivo:

1. apagar esta pasta;
2. adicionar ao `wrench.auto.repair.ordem.servico.application.csproj`:

```xml
<PackageReference Include="Wrench.Contracts" Version="1.0.0" />
```

e um `nuget.config` na raiz da solution apontando para
`https://nuget.pkg.github.com/euthribeiro/index.json`.

Qualquer mudança aqui precisa ser feita antes no repositório do pacote: ele é a fonte da verdade
e o `execucao-service` e o `billing-service` leem os mesmos contratos.
