# 🔍 Comparação de Segurança — FrotiX vs Spid

Análise comparativa das camadas de segurança documentadas no [walkthrough.md](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/walkthrough.md) (FrotiX) e o que está implementado atualmente no Spid.

---

## Quadro Comparativo Geral

| Camada de Segurança | FrotiX | Spid | Status |
|---|---|---|---|
| HTTPS Redirection | ✅ | ✅ | ✔️ OK |
| HSTS (produção) | ✅ | ✅ | ✔️ OK |
| ASP.NET Identity (framework completo) | ✅ | ❌ Autenticação manual | ⚠️ Diferente |
| Cookie Auth configurado | ✅ HttpOnly, sliding expiration | ❌ Sem cookie real | 🔴 Ausente |
| Filtro global RequireAuthenticatedUser | ✅ | ❌ Redirect manual no layout | 🔴 Ausente |
| Sistema de Roles (Identity) | ✅ | ⚠️ Campo [Perfil](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Data/PerfilRecurso.cs#3-13) em string | ⚠️ Simplificado |
| Controle de acesso granular (Recurso/ControleAcesso) | ✅ ACL hierárquica | ✅ Recurso + PerfilRecurso | ✔️ OK |
| Menu dinâmico por perfil | ✅ | ✅ | ✔️ OK |
| Proteção CSRF (Antiforgery) | ✅ Header customizado | ✅ Middleware Blazor | ✔️ OK |
| Sessão segura (HttpOnly, timeout) | ✅ | ❌ Scoped em memória | 🔴 Ausente |
| ErrorLoggingMiddleware | ✅ | ❌ | 🔴 Ausente |
| UiExceptionMiddleware | ✅ | ❌ | 🔴 Ausente |
| GlobalExceptionFilter | ✅ | ❌ | 🔴 Ausente |
| Try-catch em todas as funções | ✅ | ❌ | 🔴 Ausente |
| Global Exception Handlers (AppDomain) | ✅ | ❌ | 🔴 Ausente |
| Logger provider customizado | ✅ | ❌ | 🔴 Ausente |
| Log de emergência em arquivo | ✅ | ❌ | 🔴 Ausente |
| Páginas de erro customizadas | ✅ | ⚠️ [Error.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Pages/Error.razor) básico + [NotFound.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Pages/NotFound.razor) | ⚠️ Parcial |
| Limites de upload e validação | ✅ | ❌ | 🔴 Ausente |
| Política de senhas | ⚠️ Relaxada | ❌ Sem validação nenhuma | 🔴 Ausente |

---

## 🟢 O que o Spid já faz bem

### 1. Segurança de Transporte
O [Program.cs](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Program.cs#L112-L119) já implementa `UseHsts()` em produção e `UseHttpsRedirection()`, equivalente ao FrotiX.

### 2. Controle de Acesso por Recurso
O sistema de **Recurso + PerfilRecurso** é funcional e serve ao propósito. O [NavMenu.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Layout/NavMenu.razor) filtra dynamicamente os itens do menu conforme as permissões do perfil do usuário logado — exatamente como o FrotiX faz.

### 3. Hash de Senhas
O [UserSession.cs](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Services/UserSession.cs) usa `PasswordHasher<Usuario>` do ASP.NET Identity para hashear e verificar senhas — isso é seguro e equivalente ao que o Identity faz internamente.

### 4. Antiforgery (CSRF)
O Blazor Server com `app.UseAntiforgery()` no [Program.cs](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Program.cs#L121) protege automaticamente contra CSRF via circuit do SignalR.

---

## 🔴 Lacunas Críticas — O que está FALTANDO no Spid

### 1. Autenticação sem Cookie Real (Maior risco)

> [!CAUTION]
> O Spid **não usa cookie de autenticação**. A sessão do usuário vive apenas em memória, no [UserSession](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Services/UserSession.cs#6-42) registrado como **Scoped**. Isso significa que:
> - **Qualquer refresh no navegador** pode perder o login (depende do circuit do SignalR)
> - **Não há expiração automática** — sem timeout de sessão
> - **Sem SlidingExpiration** — sem renovação de sessão
> - **O cookie não é HttpOnly** — porque não existe cookie

No FrotiX, o cookie é configurado com `HttpOnly`, `SlidingExpiration`, timeout de 10 horas e redirecionamento automático para login em caso de expiração.

### 2. Sem Filtro Global de Autenticação

> [!WARNING]
> No FrotiX, **toda rota exige autenticação por padrão** via `AuthorizationPolicyBuilder().RequireAuthenticatedUser()`. No Spid, a proteção é feita apenas pelo componente `RedirectToLogin` no [MainLayout.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Layout/MainLayout.razor). Isso é frágil porque:
> - Depende de cada página usar o `MainLayout`
> - A página `/login` usa `LoginLayout` para escapar do redirect — mas outras páginas poderiam fazer o mesmo acidentalmente
> - Não protege rotas de API ou endpoints que possam ser adicionados futuramente

### 3. Zero Tratamento Centralizado de Erros

O FrotiX tem **6 camadas** de tratamento de erros. O Spid tem apenas:
- O handler padrão do ASP.NET (`UseExceptionHandler("/Error")`) em produção
- Um [Error.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Pages/Error.razor) básico 
- **Nenhum middleware custom**, **nenhum filter**, **nenhum try-catch padronizado**

Isso significa que erros podem:
- Expor stack traces em cenários inesperados
- Não ser registrados em log
- Causar comportamento indefinido para o usuário

### 4. Zero Sistema de Logging

> [!IMPORTANT]
> O Spid não possui **nenhum sistema de logging** — nem o padrão do ASP.NET está sendo aproveitado para registrar erros de aplicação. Se algo quebrar em produção, não há rastro para diagnóstico.

### 5. Sem Política de Senhas

O seed do [Program.cs](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Program.cs#L40-L50) cria usuários com senhas como `"admin123"` e `"gestor123"`. Não há **nenhuma validação** de comprimento, complexidade ou força de senha na hora de cadastrar/alterar usuários.

O FrotiX já tinha a política relaxada (o walkthrough marca isso como ponto de atenção), mas ao menos tinha a **infraestrutura** do Identity para configurá-la. O Spid não tem nem isso.

### 6. Sem Limites de Upload / Request

O FrotiX configura `MaxRequestBodySize = 100MB` e `MaxModelValidationErrors = 50`. O Spid não configura nenhum limite, ficando vulnerável a uploads excessivos ou ataques de negação de serviço.

---

## ⚠️ Pontos Intermediários

### Perfis como Strings
O Spid usa strings hardcoded (`"Admin"`, `"Gestor Primário"`, `"Gestor Secundário"`) em vez do sistema de Roles do Identity. Isso funciona, mas:
- É propenso a typos
- Dificulta renames futuros
- Não tem gestão administrativa (criar/editar perfis pela UI)

### Páginas de Erro
O Spid tem [Error.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Pages/Error.razor) e [NotFound.razor](file:///Users/pablotrajano/Desktop/CTRAN/Spid%20BB/DevSpid/Spid/Spid/Components/Pages/NotFound.razor), mas são básicos. O FrotiX tem páginas customizadas para 404, 401, 500 etc., com `UseStatusCodePagesWithReExecute`.

O Spid usa `UseStatusCodePagesWithReExecute("/not-found")`, o que cobre parcialmente o caso de 404.

---

## 📋 Prioridades de Implementação Sugeridas

Ordenadas por **impacto de segurança** (maior risco primeiro):

| # | Item | Esforço | Impacto |
|---|------|---------|---------|
| 1 | **Implementar autenticação via Cookie** (ou `AuthenticationStateProvider` do Blazor) com timeout e HttpOnly | Médio | 🔴 Crítico |
| 2 | **Adicionar tratamento de erros global** — ao menos um middleware que logue exceções e retorne página genérica | Baixo | 🔴 Crítico |
| 3 | **Adicionar logging básico** — registrar erros em arquivo ou console estruturado | Baixo | 🔴 Crítico |
| 4 | **Validação de senhas** — mínimo 8 caracteres na criação/alteração | Baixo | 🟡 Alto |
| 5 | **Limitar tamanho de uploads** no Kestrel/IIS | Baixo | 🟡 Médio |
| 6 | **Try-catch padronizado** nos handlers de Blazor mais críticos | Médio | 🟡 Médio |
| 7 | **Fortalecer proteção de rotas** — `AuthenticationStateProvider` + `[Authorize]` | Médio | 🟡 Alto |

---

## 🏁 Conclusão

O Spid é um projeto **funcional e em crescimento**, com boas bases em controle de acesso granular e hash de senhas. No entanto, comparado ao FrotiX, faltam camadas fundamentais de segurança — especialmente em **autenticação persistente**, **tratamento de erros** e **logging**. 

O FrotiX, apesar de suas próprias vulnerabilidades (CORS aberto, política de senhas laxa, connection strings expostas), tem uma arquitetura de segurança **em profundidade** com múltiplas camadas sobrepostas. O Spid ainda não tem essa profundidade.

A boa notícia é que os itens mais críticos (cookie auth, error handling, logging) são de esforço **baixo a médio** e podem ser implementados incrementalmente sem refatorar o que já funciona.
