# Analise da estrutura da aplicacao

Data da analise: 2026-10-05

## Objetivo

Este documento registra o estado atual do projeto `WSM.SISTEMAS.STORE`, com foco nas APIs, na aplicacao web MVC e no que ainda falta para a WebApp se comunicar com a API de Identidade para cadastrar usuario e fazer login.

## Visao geral da arquitetura

O repositorio esta organizado como uma solucao .NET separada por servicos:

- `src/services/WSM.SISTEMAS.Identidade.API`: autenticacao, usuarios, Identity, JWT e refresh token.
- `src/services/WSM.SISTEMAS.Catalogo.API`: API de catalogo, ainda em scaffold/template.
- `src/services/WSM.SISTEMAS.Carrinho.API`: API de carrinho, ainda em scaffold/template.
- `src/services/WSM.SISTEMAS.Cliente.API`: API de cliente, ainda em scaffold/template.
- `src/services/WSM.SISTEMAS.Pagamento.API`: API de pagamento, ainda em scaffold/template.
- `src/services/WSM.SISTEMAS.Pedido.API`: API de pedido, ainda em scaffold/template.
- `src/web/WSM.SISTEMAS.WebApp.MVC`: aplicacao ASP.NET Core MVC.
- `src/building blocks/WSM.SISTEMAS.Core`: tipos compartilhados, incluindo usuario autenticado e servico HTTP base.
- `src/building blocks/WSM.SISTEMAS.WebAPI.Core`: base para controllers, tratamento de erros e logging.

A intencao arquitetural esta clara: uma Web MVC consumindo APIs independentes por contexto de negocio. No momento, a API de Identidade e a WebApp sao os pontos mais avancados, mas a integracao entre elas ainda esta incompleta.

## Estado das APIs

### APIs ainda em template

As APIs `Catalogo`, `Carrinho`, `Cliente`, `Pagamento` e `Pedido` ainda estao proximas do template padrao do ASP.NET Core. Elas possuem `WeatherForecastController`, `Program.cs`, `appsettings`, `Dockerfile` e configuracao basica, mas ainda nao possuem endpoints reais de negocio.

Essas APIs nao bloqueiam diretamente cadastro/login, mas ainda nao participam de fluxos reais da loja.

### API de Identidade

A API de Identidade ja possui uma implementacao relevante:

- `AuthController` com endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh-token`
  - `POST /api/auth/revoke-token`
  - `GET /api/auth/me`
- ASP.NET Identity com `ApplicationUser` e `ApplicationRole`.
- `AutenticarDbContext` com migrations.
- JWT configurado em `IdentityConfig`.
- `TokenService` para gerar access token.
- `RefreshTokenService` para gerar, validar, rotacionar e revogar refresh token.
- Swagger configurado com suporte a bearer token.
- Connection string local em `appsettings.Development.json`.

Pontos importantes:

- A API compila.
- O endpoint `register` esta quebrado em runtime porque `AuthController.Register` inicia com `throw new Exception("houve um erro aqui");`.
- O build acusa `warning CS0162` porque o codigo abaixo desse `throw` esta inacessivel.
- Em `appsettings.json`, a connection string e a signing key estao vazias. Em desenvolvimento, `appsettings.Development.json` possui valores locais. Em producao/staging, esses valores precisam vir de secrets, variaveis de ambiente ou outro provider.

## Estado da WebApp MVC

A WebApp ja possui partes preparadas para autenticacao:

- Views:
  - `Views/Auth/Login.cshtml`
  - `Views/Auth/Register.cshtml`
  - `Views/Auth/AccessDenied.cshtml`
- ViewModels:
  - `LoginViewModel`
  - `RegisterViewModel`
  - `AuthResult`
- Cliente HTTP:
  - `IAutenticarApiClient`
  - `AutenticarApiClient`
- Servico auxiliar de cookie:
  - `CookieAuthService`
- Partial de login:
  - `Views/Shared/_LoginPartial.cshtml`

Porem, a WebApp ainda nao esta pronta para executar o fluxo:

- Nao existe `AuthController` MVC para receber `GET/POST Login`, `GET/POST Register` e `POST Logout`.
- Nao existe registro de `IAutenticarApiClient` no DI.
- Nao existe configuracao de `HttpClient` com a URL base da API de Identidade.
- Nao existe configuracao de autenticacao por cookie no `Program.cs`.
- `UseAuthentication()` nao esta no pipeline antes de `UseAuthorization()`.
- `IAspNetUser` e `IHttpContextAccessor` nao estao registrados no DI.
- O projeto Web MVC nao compila no estado atual.
- A pasta `Controllers` existe apenas como `Folder Include` no `.csproj`, mas nao ha controllers MVC reais.

## Resultado do build

Comando executado:

```bash
dotnet build WSM.SISTEMAS.STORE.slnx --no-restore
```

Resultado: falha.

Erros principais:

- `src/web/WSM.SISTEMAS.WebApp.MVC/Extensions/CpfAnnotation.cs`: usa `wsm.sistemas.Core.DomainsObjects`, mas esse namespace/classe nao foi encontrado.
- `src/web/WSM.SISTEMAS.WebApp.MVC/Views/Shared/_LoginPartial.cshtml`: usa `wsm.sistemas.Core.Usuarios`, mas o namespace correto existente e `WSM.SISTEMAS.Core.Usuarios`.
- `_LoginPartial.cshtml` injeta `IAspNetUser`, mas esse servico tambem precisa estar registrado no container.

Aviso importante:

- `src/services/WSM.SISTEMAS.Identidade.API/Controllers/AuthController.cs`: codigo inacessivel no `Register` por causa do `throw` manual.

## Fluxo esperado para cadastro/login

O fluxo desejado deve ser:

1. Usuario acessa `/Auth/Register` ou `/Auth/Login` na WebApp.
2. `AuthController` MVC valida o `ModelState`.
3. `AuthController` MVC chama `IAutenticarApiClient`.
4. `AutenticarApiClient` chama a API de Identidade via `HttpClient`.
5. API de Identidade valida ou cria o usuario com ASP.NET Identity.
6. API retorna `AuthResponse` com `accessToken`, expiracao, `refreshToken`, usuario e roles.
7. MVC cria o cookie local com `CookieAuthService.SignInAsync`.
8. O usuario fica autenticado na WebApp.
9. Chamadas futuras para APIs podem encaminhar o JWT armazenado no cookie.

## O que esta faltando para a WebApp conversar com a API de Identidade

### 1. Corrigir a compilacao da WebApp

Antes de integrar, a WebApp precisa compilar.

Pendencias:

- Corrigir o namespace em `_LoginPartial.cshtml` de `wsm.sistemas.Core.Usuarios` para `WSM.SISTEMAS.Core.Usuarios`.
- Resolver `CpfAnnotation.cs`, que referencia `wsm.sistemas.Core.DomainsObjects.Cpf`, mas esse tipo nao existe no projeto atual. Opcoes:
  - criar o objeto `Cpf` no `WSM.SISTEMAS.Core`;
  - mover a validacao de CPF para dentro da WebApp;
  - remover temporariamente essa annotation se ainda nao for usada.

### 2. Registrar autenticacao por cookie no MVC

O `Program.cs` da WebApp possui apenas:

- `AddControllersWithViews()`
- `UseAuthorization()`

Falta adicionar:

- `AddAuthentication(...).AddCookie(...)`
- `UseAuthentication()` antes de `UseAuthorization()`
- caminhos de login, logout e acesso negado.

Sem isso, `CookieAuthService.SignInAsync` nao tem um esquema de cookie configurado corretamente e `User.Identity.IsAuthenticated` nao sera confiavel na aplicacao.

### 3. Registrar servicos compartilhados no DI da WebApp

O partial de login depende de `IAspNetUser`, implementado por `AspNetUsers`.

Falta registrar:

- `builder.Services.AddHttpContextAccessor();`
- `builder.Services.AddScoped<IAspNetUser, AspNetUsers>();`

Tambem falta registrar o provider de validacao de CPF, se a annotation for mantida:

- `IValidationAttributeAdapterProvider`

### 4. Registrar o cliente HTTP da API de Identidade

`AutenticarApiClient` existe, mas nao esta registrado.

Falta adicionar algo como:

```csharp
builder.Services.AddHttpClient<IAutenticarApiClient, AutenticarApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:IdentidadeUrl"]!);
});
```

Tambem falta configurar a URL no `appsettings.Development.json`, por exemplo:

```json
{
  "ApiSettings": {
    "IdentidadeUrl": "https://localhost:7227"
  }
}
```

Como a chamada login/cadastro acontece no servidor MVC via `HttpClient`, CORS nao e necessario para esse fluxo. CORS so sera necessario se o navegador chamar a API diretamente via JavaScript.

### 5. Criar o `AuthController` da WebApp MVC

As views `Login.cshtml` e `Register.cshtml` usam `asp-action`, mas nao ha controller para essas actions.

Falta criar um controller MVC com pelo menos:

- `GET /Auth/Login`
- `POST /Auth/Login`
- `GET /Auth/Register`
- `POST /Auth/Register`
- `POST /Auth/Logout`
- `GET /Auth/AccessDenied`

Esse controller deve:

- receber `LoginViewModel` e `RegisterViewModel`;
- chamar `IAutenticarApiClient.LoginAsync` e `RegisterAsync`;
- quando `Succeeded == true`, chamar `CookieAuthService.SignInAsync`;
- quando falhar, adicionar erros no `ModelState`;
- respeitar `ReturnUrl` com validacao de URL local.

### 6. Remover o `throw` do endpoint de cadastro da API

O cadastro nunca funcionara enquanto existir:

```csharp
throw new Exception("houve um erro aqui");
```

Esse trecho esta em `AuthController.Register` da API de Identidade. Ele precisa ser removido para que o Identity consiga criar o usuario.

### 7. Alinhar contrato de cadastro entre MVC e API

A API espera `RegisterUserRequest` com:

- `nome`
- `email`
- `password`
- `confirmPassword`

O client atual envia apenas:

- `email`
- `password`
- `nome`

Como a API possui `[Compare(nameof(Password))]` em `ConfirmPassword`, o cadastro pode falhar por validacao quando o model binding receber `confirmPassword` vazio.

O `RegisterAsync` da WebApp deve receber e enviar `confirmPassword`, ou a API deve remover esse campo se a confirmacao for responsabilidade exclusiva do MVC.

Recomendacao: manter a validacao nos dois lados e enviar `confirmPassword` para a API.

### 8. Alinhar o contrato de resposta de login/cadastro

A API retorna `AuthResponse` com propriedades que, em JSON camelCase, ficam assim:

- `userId`
- `nome`
- `email`
- `accessToken`
- `accessTokenExpiresAtUtc`
- `refreshToken`
- `refreshTokenExpiresAtUtc`
- `roles`

O client atual tenta ler:

- `accessToken`
- `expiresAtUtc`
- `userId`
- `email`
- `nome`
- `fotoPerfilUrl`
- `roles`

Problema: `expiresAtUtc` nao existe na resposta da API. O nome correto atual e `accessTokenExpiresAtUtc`.

Consequencia: mesmo que o login na API retorne `200`, `MapToAuthResult` pode lancar excecao ao acessar uma propriedade inexistente, e o MVC transformara isso em erro generico.

Falta escolher uma das opcoes:

- alterar o client para ler `accessTokenExpiresAtUtc`; ou
- alterar a API para retornar `expiresAtUtc`.

Tambem falta mapear `refreshTokenExpiresAtUtc`.

### 9. Alinhar refresh token: JSON versus cookie

A API retorna o refresh token no corpo JSON (`refreshToken`).

O client atual tenta extrair refresh token do header `Set-Cookie`:

```csharp
ExtractRefreshTokenFromCookie(response)
```

Problema: a API nao esta setando cookie. Logo, o refresh token fica `null` no `AuthResult`, mesmo estando no JSON.

Falta escolher um padrao:

- Simples para MVC server-side: ler `refreshToken` do JSON e salvar no cookie de autenticacao MVC como claim.
- Mais seguro para browser/API direta: API setar refresh token em cookie `HttpOnly`, `Secure`, `SameSite`, e o client seguir lendo `Set-Cookie`.

Para este projeto MVC server-side, a opcao mais direta e ler do JSON.

### 10. Alinhar endpoints de refresh e logout/revogacao

A API possui:

- `POST /api/auth/refresh-token`
- `POST /api/auth/revoke-token`

O client chama:

- `POST /api/auth/refresh`
- `POST /api/auth/logout`

Essas rotas nao batem.

Tambem ha diferenca no formato:

- A API espera `RefreshTokenRequest` e `RevokeTokenRequest` no corpo JSON.
- O client envia o refresh token como cookie/header e nao envia body.

Falta alinhar:

- rotas;
- nome dos metodos;
- formato do request;
- forma de enviar o refresh token.

### 11. Aplicar migrations e garantir banco local

A API de Identidade usa SQL Server LocalDB em desenvolvimento:

```json
"Server=(localdb)\\mssqllocaldb;Database=WsmBDAuth;..."
```

Falta garantir que:

- LocalDB esta instalado/ativo;
- a migration `InitialIdentity` foi aplicada;
- a API de Identidade sobe em `https://localhost:7227` ou na URL configurada na WebApp.

### 12. Criar ou restaurar controllers MVC principais

O projeto possui views para `Home`, `Auth`, `Account`, `Produtos`, `AdminProdutos` e `AdminUsuarios`, mas nao foram encontrados controllers MVC correspondentes.

Mesmo fora do login, isso afeta a navegacao geral da WebApp. Para o login/cadastro especificamente, o `AuthController` MVC e obrigatorio.

## Diagnostico resumido

Hoje a API de Identidade esta quase pronta para login, mas o cadastro esta bloqueado por um `throw` manual. A WebApp tem telas e um client HTTP parcialmente implementados, mas ainda falta o controller MVC, a configuracao de cookie authentication, o registro de DI, a URL da API, e o alinhamento dos contratos JSON/rotas.

O primeiro objetivo deve ser fazer a WebApp compilar. O segundo deve ser fechar o fluxo minimo:

1. API de Identidade rodando com banco e JWT configurados.
2. WebApp com cookie auth e `IAutenticarApiClient` registrados.
3. `AuthController` MVC criado.
4. Contrato `Register/Login/AuthResponse` alinhado entre os dois projetos.

Depois disso, cadastro e login devem conseguir funcionar de ponta a ponta.

## Checklist pratico de implementacao

- [ ] Corrigir namespaces `wsm.sistemas.*` para `WSM.SISTEMAS.*` onde aplicavel.
- [ ] Resolver ou remover dependencia inexistente `Core.DomainsObjects.Cpf`.
- [ ] Remover o `throw` de `AuthController.Register` na API de Identidade.
- [ ] Registrar cookie authentication na WebApp.
- [ ] Adicionar `app.UseAuthentication()` antes de `app.UseAuthorization()`.
- [ ] Registrar `IHttpContextAccessor`.
- [ ] Registrar `IAspNetUser -> AspNetUsers`.
- [ ] Registrar `IAutenticarApiClient -> AutenticarApiClient` com `HttpClient`.
- [ ] Adicionar `ApiSettings:IdentidadeUrl` na configuracao da WebApp.
- [ ] Criar `AuthController` MVC.
- [ ] Enviar `confirmPassword` no cadastro para a API.
- [ ] Ajustar `MapToAuthResult` para ler `accessTokenExpiresAtUtc`.
- [ ] Ler `refreshToken` do JSON ou fazer a API setar cookie de refresh token.
- [ ] Ajustar rotas `refresh-token` e `revoke-token` no client, ou renomear endpoints na API.
- [ ] Aplicar migrations do banco de Identidade.
- [ ] Rodar build completo.
- [ ] Testar manualmente `POST /api/auth/register` e `POST /api/auth/login` no Swagger.
- [ ] Testar fluxo pela WebApp: abrir `/Auth/Register`, criar usuario, autenticar cookie e navegar logado.
