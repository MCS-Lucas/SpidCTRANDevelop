# Resumo Geral das Alterações no Projeto SPID

Este documento detalha todas as modificações recentes feitas no projeto, agrupadas por temas lógicos. O objetivo é fornecer uma visão clara de **o que** mudou e **por que** mudou, facilitando o entendimento da evolução da aplicação e o registro para futuras manutenções.

---

### 🏗️ Tema 1: Infraestrutura, Segurança e Servidor (IIS/Docker)
Este bloco contém configurações cruciais para que a aplicação rode com segurança e estabilidade, especialmente em ambientes de produção como o servidor Windows IIS (CTRAN01).

**Arquivos envolvidos:**
- `../.gitignore`
- `Spid/Program.cs`
- `Spid/appsettings.json`, `appsettings.Docker.json`, `appsettings.Production.json`
- `Spid/web.config`

**Detalhamento das Mudanças:**
- **Data Protection API:** Foi implementada a persistência em disco das chaves criptográficas (salvas fisicamente na pasta `keys/`). Isso impede que todos os usuários percam a sessão ou recebam erros de segurança de formulário (*anti-forgery token*) toda vez que o servidor (IIS ou Docker) for reiniciado ou reciclado.
- **Segurança de Repositório (`.gitignore`):** A pasta `keys/` foi ignorada no Git para evitar o vazamento das credenciais de criptografia da aplicação.
- **IIS e Proxy Reverso:** Adicionamos o arquivo `web.config` configurando corretamente o `AspNetCoreModuleV2` e ativando expressamente os WebSockets, vitais para o SignalR do Blazor Server funcionar de forma fluida no Windows Server. No `Program.cs`, adicionamos suporte a `ForwardedHeaders`, permitindo que a aplicação entenda os IPs reais e o protocolo (HTTP/HTTPS) caso esteja rodando atrás de um balanceador de carga ou proxy transparente.
- **Saneamento de Credenciais:** As senhas do banco de dados de produção foram removidas dos arquivos `appsettings.Production.json` (trocadas por `SENHACTRAN01!`). Isso blinda a infraestrutura caso o código-fonte sofra alguma exposição acidental. Também foram estruturadas as configurações de *Timeout* e tarefas automáticas no boot da aplicação (`RunDatabaseSetupOnBoot`).

---

### ⏲️ Tema 2: Gerenciamento de Sessão e Timeout
Focado exclusivamente no ciclo de vida da autenticação, renovação e na experiência de segurança do usuário logado.

**Arquivos envolvidos:**
- `Spid/Components/Layout/SessionTimeout.razor` (NOVO)
- `Spid/Components/Layout/MainLayout.razor`
- `Spid/Components/Layout/RedirectToLogin.razor`
- `Spid/Components/Routes.razor`
- `Spid/Components/Pages/Login.razor` e `PrimeiroAcesso.razor`
- `Spid/Services/UserSession.cs`

**Detalhamento das Mudanças:**
- **Controle de Inatividade:** A introdução do componente `SessionTimeout.razor` permite monitorar o tempo de ociosidade do usuário na tela. Caso o sistema não registre interações (cliques/teclado) por um período prolongado, ele notifica e encerra a sessão proativamente, mitigando os riscos de acesso não autorizado em computadores que ficaram destravados no setor.
- **Fluxo de Login Aprimorado:** As validações do processo de login foram blindadas para lidar com o ciclo de primeiro acesso do colaborador. Ajustamos o roteamento central no `Routes.razor` e a reatividade do `RedirectToLogin.razor`, garantindo que todas as páginas fechadas rejeitem visitantes sem um *Cookie* válido, redirecionando-os imediatamente para a tela de login.

---

### 📊 Tema 3: Nova Feature de Log de Importações
Uma nova funcionalidade implementada no backend para oferecer controle e auditoria técnica sobre o recebimento de planilhas de viagens.

**Arquivos envolvidos:**
- `Spid/Data/ImportacaoLog.cs` (NOVO)
- `Spid/Data/AppDbContext.cs`
- `Spid/Migrations/20260409140007_AddImportacaoLog.*` (NOVOS)
- `Spid/Migrations/AppDbContextModelSnapshot.cs`
- `Spid/Services/ImportacaoService.cs`
- `Spid/Components/Pages/ImportarViagens.razor`

**Detalhamento das Mudanças:**
- **Auditoria de Dados:** A entidade `ImportacaoLog` foi criada e injetada no banco de dados via Migrations do Entity Framework. A sua função é armazenar metadados exatos sobre os processos de carga em lote.
- **Transparência e Rastreio:** O serviço de leitura de Excel (`ImportacaoService.cs`) e a tela principal de Importação (`ImportarViagens.razor`) foram conectados a essa nova entidade. Sempre que uma planilha é salva com sucesso, a aplicação registra o momento exato e a identidade do usuário (Gestor/Admin) responsável por aquele lote, promovendo histórico e responsabilidade sobre os dados que entram no sistema.

---

### 🎨 Tema 4: Melhorias de UX/UI (Quality of Life) e Atualizações Técnicas
Mudanças estéticas e interativas pensadas para acelerar a rotina de quem usa os painéis gerenciais diariamente.

**Arquivos envolvidos:**
- `Spid/Components/Pages/Home.razor`
- `Spid/Components/Pages/AnalisarViagens.razor`
- `Spid/Components/Pages/StatusConferencias.razor`
- `Spid/wwwroot/app.css`
- `Spid/Spid.csproj`

**Detalhamento das Mudanças:**
- **Navegação Temporal Dinâmica:** Nas três principais telas analíticas (Dashboard Geral, Status das Conferências e Ateste de Viagens), foram acoplados botões de navegação lateral (◀ e ▶) aos filtros de "Mês" e "Ano". Em vez de clicar e abrir o dropdown de seleção repetidas vezes, o gestor pode alternar o período analisado com cliques rápidos, gerando recarregamentos automáticos do gráfico e tabelas instantaneamente.
- **Polimento CSS e Fallbacks:** Foram escritas novas regras de estilo no arquivo global `app.css` (`.filter-btn`) para uniformizar o design desses botões de navegação.
- **Evolução de Estrutura:** No arquivo principal de compilação (`Spid.csproj`), referências e configurações de hospedagem do ASP.NET Core foram alinhadas, e as versões de dependências estratégicas como `ClosedXML` (para Excel) e o *Entity Framework Core Tools* foram consolidadas.

---
---

# 📋 Análise das Alterações Pendentes e Plano de Commits (02/06/2026)

> **Contexto:** O git reconhece **25 alterações** (21 arquivos rastreados + 4 untracked) desde o último commit na branch `main`. Este plano organiza todas as mudanças em **7 commits temáticos**, ordenados por dependência lógica — os commits de infraestrutura/dados vêm primeiro, pois as camadas superiores (páginas, UI) dependem deles.

---

## 📊 Inventário Completo das Alterações

### Arquivos Modificados (21)
| # | Arquivo | Tipo de Mudança |
|---|---------|----------------|
| 1 | `Components/Layout/NavMenu.razor` | Link "Minhas Informações" + ampliação do `isAdmin` |
| 2 | `Components/Pages/AnalisarViagens.razor` | Rename Setor→CentroCusto + coluna "Motivo da Viagem" |
| 3 | `Components/Pages/Home.razor` | Rename Setor→CentroCusto em todo o Dashboard |
| 4 | `Components/Pages/PrimeiroAcesso.razor` | Rename Setor→CentroCusto + validação Gmail + null-safety |
| 5 | `Components/Pages/StatusConferencias.razor` | Rename Setor→CentroCusto + hierarquia de gestores |
| 6 | `Components/Pages/UsuarioForm.razor` | Novo sistema de perfil (grupo+subtipo) + controle de permissões |
| 7 | `Components/Pages/Usuarios.razor` | Filtros por perfil com subtipos + controle de edição |
| 8 | `Data/AppDbContext.cs` | Rename Setor→CentroCusto em todo o DbContext |
| 9 | `Data/Colaborador.cs` | Rename SetorId→CentroCustoId |
| 10 | `Data/ConferenciaMensal.cs` | Rename SetorId→CentroCustoId |
| 11 | `Data/PerfilRecurso.cs` | Atualização do comentário de perfis disponíveis |
| 12 | `Data/Usuario.cs` | Rename SetorId→CentroCustoId + novos perfis no comentário |
| 13 | `Data/Viagem.cs` | Rename SetorId→CentroCustoId |
| 14 | `Migrations/AppDbContextModelSnapshot.cs` | Snapshot reflete rename completo Setor→CentroCusto |
| 15 | `Program.cs` | "Gestor Principal"→"Gestor Primário" no seed + ajustes de login |
| 16 | `Services/ImportacaoService.cs` | Rename Setor→CentroCusto no fluxo de importação Excel |
| 17 | `seguranca_spid.md` | Atualização do doc de segurança com novos nomes de perfis |

### Arquivos Deletados (4)
| # | Arquivo | Motivo |
|---|---------|--------|
| 18 | `Components/Pages/Login.razor` | Página Blazor de login substituída pela rota MVC em Program.cs |
| 19 | `Components/Pages/Login.razor.css` | CSS isolado do Login.razor removido junto com o componente |
| 20 | `CorrecaoBanco.sql` | Script SQL avulso movido para a pasta `queries/` |
| 21 | `Data/Setor.cs` | Entidade substituída por `CentroCusto.cs` |

### Arquivos Novos (Untracked) (5+)
| # | Arquivo | Descrição |
|---|---------|-----------|
| 22 | `Components/Pages/MinhasInformacoes.razor` | Nova página de autoedição de dados do usuário logado |
| 23 | `Data/CentroCusto.cs` | Nova entidade que substitui `Setor.cs` |
| 24 | `Migrations/20260521163023_RenameSetorToCentroCusto.cs` | Migration EF Core para renomear tabelas/colunas |
| 25 | `Migrations/20260521163023_RenameSetorToCentroCusto.Designer.cs` | Designer da migration |
| 26 | `queries/CorrecaoBanco.sql` | Script SQL de correção de horários (movido da raiz) |
| 27 | `queries/insert_permissoes_gestores_centrais.sql` | Script para conceder permissões aos novos perfis |
| 28 | `queries/update_banco_gestor_primario.sql` | Script para renomear "Gestor Principal"→"Gestor Primário" |
| 29 | `queries/update_banco_producao.sql` | Script de migration manual para produção (rename Setor→CentroCusto) |

---

## 🎯 Plano de Commits (7 commits sequenciais)

> **⚠️ IMPORTANTE:** Execute os commits exatamente nesta ordem. Cada commit é auto-contido por tema, mas a ordem respeita as dependências entre camadas (dados → lógica → UI → documentação).

---

### 🔵 Commit 1: Rename da entidade Setor → CentroCusto (Data Layer)
**Mensagem:** `refactor(data): renomear entidade Setor para CentroCusto em todo o modelo de dados`

**Descrição:** Substitui a entidade `Setor` pela nova `CentroCusto`, atualizando todas as propriedades de navegação (FK), o `AppDbContext`, e inclui a migration EF Core correspondente. Remove o arquivo `Setor.cs` obsoleto.

**Arquivos:**
```
git add Data/CentroCusto.cs
git add Data/Colaborador.cs
git add Data/ConferenciaMensal.cs
git add Data/Usuario.cs
git add Data/Viagem.cs
git add Data/PerfilRecurso.cs
git add Data/AppDbContext.cs
git rm Data/Setor.cs
git add Migrations/20260521163023_RenameSetorToCentroCusto.cs
git add Migrations/20260521163023_RenameSetorToCentroCusto.Designer.cs
git add Migrations/AppDbContextModelSnapshot.cs
git commit -m "refactor(data): renomear entidade Setor para CentroCusto em todo o modelo de dados

- Cria nova entidade CentroCusto substituindo Setor
- Atualiza FK SetorId -> CentroCustoId em Colaborador, ConferenciaMensal, Usuario e Viagem
- Atualiza DbContext: DbSet<CentroCusto> CentrosCusto, relacionamentos e índices
- Atualiza comentários de perfis em PerfilRecurso.cs e Usuario.cs
- Inclui migration EF Core RenameSetorToCentroCusto
- Remove entidade Setor.cs obsoleta"
```

---

### 🟢 Commit 2: Renomear perfil "Gestor Principal" → "Gestor Primário" + novos perfis Gestor Central
**Mensagem:** `feat(auth): renomear Gestor Principal para Gestor Primário e adicionar perfis Gestor Central`

**Descrição:** Atualiza o `Program.cs` para que o seed de dados e a rotina de migração automática usem "Gestor Primário" em vez de "Gestor Principal". Prepara a base para os novos perfis "Gestor Central Padrão" e "Gestor Central Ateste". Também ajusta o placeholder de login e o link "Esqueci minha senha".

**Arquivos:**
```
git add Program.cs
git commit -m "feat(auth): renomear Gestor Principal para Gestor Primário e adicionar perfis Gestor Central

- Seed de PerfisRecurso usa 'Gestor Primário' em vez de 'Gestor Principal'
- Rotina RunDatabaseSetup migra perfis antigos automaticamente
- Placeholder do campo Ponto atualizado para 'P_******'
- Link 'Alterar Senha' renomeado para 'Esqueci minha senha'"
```

---

### 🟡 Commit 3: Atualizar ImportacaoService para usar CentroCusto
**Mensagem:** `refactor(service): adaptar ImportacaoService ao rename Setor → CentroCusto`

**Descrição:** Atualiza todo o fluxo de importação de planilhas Excel para usar a terminologia CentroCusto no cache de entidades, criação de colaboradores e vinculação de viagens.

**Arquivos:**
```
git add Services/ImportacaoService.cs
git commit -m "refactor(service): adaptar ImportacaoService ao rename Setor → CentroCusto

- Renomeia setoresCache -> centrosCustoCache
- Renomeia novosSetores -> novosCentrosCusto
- Atualiza criação de Colaborador para usar CentroCustoId
- Atualiza criação de Viagem para usar CentroCustoId"
```

---

### 🟠 Commit 4: Atualizar páginas analíticas (Dashboard, Conferências, Viagens)
**Mensagem:** `refactor(pages): adaptar páginas analíticas ao rename Setor → CentroCusto e novos perfis`

**Descrição:** Atualiza as três principais telas analíticas para usar CentroCusto, amplia o `isAdmin` para incluir Gestores Centrais, adiciona coluna "Motivo da Viagem" na tabela de análise, e implementa hierarquia de gestores responsáveis no StatusConferencias.

**Arquivos:**
```
git add Components/Pages/Home.razor
git add Components/Pages/AnalisarViagens.razor
git add Components/Pages/StatusConferencias.razor
git commit -m "refactor(pages): adaptar páginas analíticas ao rename Setor → CentroCusto e novos perfis

Home.razor:
- Rename setores → centrosCusto em filtros, navegação e queries
- isAdmin inclui perfis 'Gestor Central*'

AnalisarViagens.razor:
- Rename SetorId → CentroCustoId em queries e filtros
- Adicionada coluna 'Motivo da Viagem' na tabela

StatusConferencias.razor:
- Rename completo SetorStatusDto → CentroCustoStatusDto
- Hierarquia de gestores: Gestor Central Ateste > Primário > Secundário
- Coluna 'Gestor Principal' renomeada para 'Gestor(a) Responsável'"
```

---

### 🔴 Commit 5: Novo sistema de gerenciamento de usuários com perfis hierárquicos
**Mensagem:** `feat(usuarios): implementar sistema de perfis hierárquicos com Gestores Centrais`

**Descrição:** Reescreve a lógica de gerenciamento de usuários com seleção de perfil em dois níveis (grupo + subtipo), controle de permissões granular por nível hierárquico, e filtros avançados na listagem. Adiciona a página "Minhas Informações" para autoedição.

**Arquivos:**
```
git add Components/Pages/UsuarioForm.razor
git add Components/Pages/Usuarios.razor
git add Components/Pages/MinhasInformacoes.razor
git add Components/Layout/NavMenu.razor
git commit -m "feat(usuarios): implementar sistema de perfis hierárquicos com Gestores Centrais

UsuarioForm.razor:
- Seleção de perfil em dois níveis: grupo (Admin/Gestor/Gestor Central) + subtipo
- Controle de permissões: quem pode editar quem, baseado na hierarquia
- Validação de segurança: Admin > Gestor Central Ateste > Gestor Central Padrão > Gestor
- Botão de exclusão restrito apenas a Admins

Usuarios.razor:
- Filtro de perfil com subtipos dinâmicos
- Badges coloridos por tipo de perfil (danger, warning, info, primary)
- Botão 'Editar' condicional via PodeEditar()

MinhasInformacoes.razor (NOVO):
- Página /minhas-informacoes para o usuário editar seus próprios dados
- Campos bloqueados conforme nível de perfil (gestores comuns têm restrições)

NavMenu.razor:
- Adicionado link 'Minhas Informações' acessível a todos
- isAdmin ampliado para incluir perfis 'Gestor Central*'"
```

---

### 🟣 Commit 6: Melhorias no PrimeiroAcesso (validação Gmail + null-safety)
**Mensagem:** `feat(primeiro-acesso): exigir e-mail Gmail e melhorar tratamento de null`

**Descrição:** Reforça a validação do primeiro acesso exigindo conta Google/Gmail, adiciona null-safety no fluxo de inicialização e atualiza a referência de Setor para CentroCusto.

**Arquivos:**
```
git add Components/Pages/PrimeiroAcesso.razor
git commit -m "feat(primeiro-acesso): exigir e-mail Gmail e melhorar tratamento de null

- Label e placeholder atualizados para indicar 'Conta Google/Gmail'
- Validação server-side: rejeita e-mails que não terminam com @gmail.com
- Null-check em 'usuario' antes de acessar propriedades (evita NullReferenceException)
- Rename nomeSetor → nomeCentroCusto
- Rename SetorId → CentroCustoId na consulta de centro de custo"
```

---

### ⚫ Commit 7: Limpeza de arquivos obsoletos + scripts SQL + documentação
**Mensagem:** `chore: remover arquivos obsoletos, organizar queries SQL e atualizar documentação`

**Descrição:** Remove o componente Blazor Login.razor (substituído pela rota MVC), move o script CorrecaoBanco.sql para a pasta `queries/`, adiciona scripts de migração de produção, e atualiza a documentação de segurança.

**Arquivos:**
```
git rm Components/Pages/Login.razor
git rm Components/Pages/Login.razor.css
git rm CorrecaoBanco.sql
git add queries/
git add seguranca_spid.md
git commit -m "chore: remover arquivos obsoletos, organizar queries SQL e atualizar documentação

Removidos:
- Login.razor e Login.razor.css (login agora é via rota MVC em Program.cs)
- CorrecaoBanco.sql da raiz (movido para queries/)

Adicionados (queries/):
- CorrecaoBanco.sql — correção de precisão de horários
- insert_permissoes_gestores_centrais.sql — permissões para novos perfis
- update_banco_gestor_primario.sql — rename 'Gestor Principal' → 'Gestor Primário'
- update_banco_producao.sql — migration manual de produção (Setor → CentroCusto)

Atualizado:
- seguranca_spid.md — referências a 'Gestor Principal' → 'Gestor Primário'"
```

---

## ✅ Resumo Visual do Plano

```
Commit 1  ─── 🏗️ Data Layer (CentroCusto + Migration + Snapshot)     [11 arquivos]
    │
Commit 2  ─── 🔑 Program.cs (Seed + Perfis + Login)                   [1 arquivo]
    │
Commit 3  ─── ⚙️ ImportacaoService (Rename no serviço)                [1 arquivo]
    │
Commit 4  ─── 📊 Páginas Analíticas (Home, Viagens, Conferências)     [3 arquivos]
    │
Commit 5  ─── 👥 Gerenciamento de Usuários + MinhasInformações         [4 arquivos]
    │
Commit 6  ─── 🆕 PrimeiroAcesso (Gmail + Null-safety)                 [1 arquivo]
    │
Commit 7  ─── 🧹 Limpeza + Queries SQL + Documentação                 [5+ arquivos]
```

> **Total: 7 commits cobrindo todas as 29 alterações detectadas.**
