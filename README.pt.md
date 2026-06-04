[English version](README.md)
# Gerador de Níveis com Aprendizagem por Reforço

Um gerador de níveis procedurais potente baseado em Unity, que utiliza aprendizagem por reforço para criar níveis de jogo com dificuldade dinâmica. Este sistema usa algoritmos de aprendizagem por reforço (RL) para treinar um agente capaz de gerar níveis envolventes, adaptados ao desempenho do jogador.

**Autor:** Afonso Costa

---

## Índice

1. [Descrição Geral](#descrição-geral)
2. [Funcionalidades](#funcionalidades)
3. [Requisitos](#requisitos)
4. [Instalação](#instalação)
5. [Guia de Configuração](#guia-de-configuração)
6. [Guia de Utilização](#guia-de-utilização)
7. [Configurações](#configurações)
8. [Referência da API](#referência-da-api)
9. [Boas Práticas](#boas-práticas)
10. [Resolução de Problemas](#resolução-de-problemas)

---

## Descrição Geral

O Gerador de Níveis com Aprendizagem por Reforço é um sistema sofisticado concebido para gerar automaticamente níveis de jogo que se adaptam ao nível de habilidade do jogador. Empregando princípios de aprendizagem por reforço, o gerador treina um agente para criar níveis que mantêm um desafio e envolvimento óptimos ao longo da jogabilidade.

### Componentes Principais

- **RLGeneratorAgent.cs** - Agente RL responsável pelas decisões de geração de níveis
- **RLLevelTrainer.cs** - Controlador principal do pipeline de treino e geração
- **PlayerPerformanceTracker.cs** - Monitora o desempenho do jogador e ajusta a dificuldade
- **EnemyDifficultyAdjuster.cs** - Escala os parâmetros dos inimigos com base na dificuldade
- **GeneratorState.cs** - Define o espaço de estados para o agente RL
- **GeneratorAction.cs** - Define o espaço de acções para o agente RL
- **TileType.cs** - Enumera os tipos de blocos/tiles disponíveis

---

## Funcionalidades

✨ **Geração Procedural de Níveis**
- Gera níveis únicos com base no treino
- Suporta estilos de jogo Side-Scroller e Top-Down

🎮 **Ajuste Dinâmico de Dificuldade**
- Adapta a complexidade do nível com base no desempenho do jogador
- Rastreia vitórias/derrotas consecutivas e ajusta-se em conformidade
- Escala de dificuldade configurável

🎯 **Opções de Conteúdo Flexíveis**
- Inimigos opcionais com escala de dificuldade
- Geração de plataformas com alturas personalizáveis
- Itens colecionáveis (moedas, anéis, etc.)
- Recargas de saúde
- Obstáculos e perigos

🤖 **Integração de Aprendizagem por Reforço**
- Agente RL aprende a gerar níveis envolventes
- Episódios de treino configuráveis (1000+ recomendado)
- Progressão de dificuldade baseada em desempenho

🏗️ **Organização Hierárquica de Cenas**
- Atribuição automática de pais a objetos gerados
- Gestão limpa da hierarquia Unity
- Suporte para colisor composto (Composite Collider)

---

## Requisitos

### Software
- **Unity 2020.3 LTS** ou mais recente
- C# 7.3 ou superior
- Configuração de jogabilidade 2D

### Hardware (Recomendado)
- CPU: Processador multi-núcleo para treino eficiente
- RAM: 4GB mínimo (8GB+ recomendado para níveis maiores)
- Armazenamento: ~200MB para ficheiros do projeto

---

## Instalação

### Passo 1: Importar Ficheiros

Certifique-se de que todos os scripts necessários estão presentes no seu projeto:

```
Assets/Scripts/
├── GeneratorAction.cs
├── GeneratorState.cs
├── RLGeneratorAgent.cs
├── RLLevelTrainer.cs
├── PlayerPerformanceTracker.cs
├── EnemyDifficultyAdjuster.cs
└── TileType.cs
```

### Passo 2: Criar Objecto Principal de Treino

1. Crie um GameObject vazio na sua cena (o nome é arbitrário)
2. Anexe o script `RLLevelTrainer.cs` como componente
3. Este será o controlador principal da geração de níveis

---

## Guia de Configuração

### Fase 1: Configuração Básica

#### Passo 1: Criar Pai de Blocos com Colisor Composto

1. Crie um GameObject vazio (o nome é arbitrário)
2. Adicione o componente **Composite Collider 2D**
3. Configure conforme mostrado abaixo:
   - Material: Use o seu material de física (ex.: WallMaterial)
   - Tipo de Geometria: Outlines
   - Tipo de Geração: Synchronous
   - Distância de Vértice: 0.0005
   - Distância de Desvio: 5e-05

#### Passo 2: Configurar Hierarquia de Pais

1. Crie 5 GameObjects vazios no total:
   - 1 com Composite Collider 2D (para blocos)
   - 4 objetos adicionais (para bordas, inimigos, moedas, perigos)

2. Atribua-os no inspetor do RLLevelTrainer em **Object Parents**:
   - **Block Parent** → Objecto com Composite Collider
   - **Edge Parent** → GameObject vazio
   - **Enemy Parent** → GameObject vazio
   - **Coin Parent** → GameObject vazio
   - **Hazard Parent** → GameObject vazio

#### Passo 3: Configurar Prefabs

1. No RLLevelTrainer, localize a secção **Prefabs**
2. Atribua os seus prefabs de jogo:

| Campo | Requisito | Notas |
|-------|-----------|-------|
| Ground Block | Box Collider 2D + "Used By Composite" ✓ | Tamanho recomendado: 1×1 |
| Platform Block | Box Collider 2D + "Used By Composite" ✓ | Mesmo tamanho do chão |
| Left Edge Trigger | Trigger Collider | Impede inimigos caindo à esquerda |
| Right Edge Trigger | Trigger Collider | Impede inimigos caindo à direita |
| Side Scrolling Enemy | Opcional | Será escalado com dificuldade |
| Top Down Enemy | Opcional | Será escalado com dificuldade |
| Coin Prefab | Opcional | Itens colecionáveis |
| Pickup Prefab | Opcional | Saúde/power-ups |
| Hazard Prefab | Opcional | Obstáculos/zonas de dano |
| Death Plane | Trigger Collider | Detecta jogador caindo |

**Importante:** Os blocos de Chão e Plataforma DEVEM ter Box Collider 2D com "Used By Composite" ativado.

#### Passo 4: Configurar Definições Básicas

No inspetor do RLLevelTrainer, defina **Basic Settings**:

```
Block Size: 1 (correspondendo ao tamanho do seu prefab)
Level Width: Veja valores recomendados abaixo
Level Height: Veja valores recomendados abaixo
Training Episodes: Mínimo 1000
Difficulty: 1 (dificuldade inicial)
```

#### Passo 5: Definir Objecto do Jogador

1. Coloque o seu GameObject do Jogador na cena
2. Atribua-o no RLLevelTrainer em **Player Object**
3. A posição do jogador será reposta quando a geração terminar

#### Passo 6: Configurar Escala de Dificuldade de Inimigos (Opcional)

Se o seu jogo inclui inimigos:

1. Adicione `EnemyDifficultyAdjuster.cs` aos prefabs de inimigos
2. Variáveis públicas disponíveis:
   - `MaxHealth` - Saúde do inimigo escalada pela dificuldade
   - `MovementSpeed` - Velocidade do inimigo escalada pela dificuldade

#### Passo 7: Configurar Rastreio de Desempenho

1. Crie um novo GameObject vazio
2. Anexe `PlayerPerformanceTracker.cs`
3. Atribua-o no RLLevelTrainer em **Player Performance Tracker**
4. Configure o valor de Difficulty Increment (padrão: 0.1)

---

### Fase 2: Configuração de Definições de Geração

#### Passo 1: Escolher Modo de Geração

Nas **Generation Settings** do RLLevelTrainer, selecione um:
- **Side Scroller** - Níveis tradicionais com deslocamento horizontal
- **Top Down** - Níveis de vista de cima/bird's-eye view

#### Passo 2: Aplicar Parâmetros Recomendados

**Para Níveis Side Scroller:**
```
Level Width: 100-200
Level Height: 50-75
Training Episodes: ≥1000
```

**Para Níveis Top Down:**
```
Level Width: 20-40
Level Height: 20-40
Training Episodes: 1000-5000 (evite exceder 5000)
```

#### Passo 3: Ajustar Finamente as Opções de Geração

Configure as seguintes opções em **Generation Settings**:

| Opção | Propósito | Impacto |
|-------|----------|--------|
| Have Enemies | Ativar/desativar geração de inimigos | Afecta progressão de dificuldade |
| Have Platforms | Ativar/desativar plataformas | Muda complexidade do nível |
| Have Pickups | Ativar/desativar itens de saúde | Afecta dificuldade de sobrevivência |
| Have Hazards | Ativar/desativar colocação de obstáculos | Aumenta desafio |
| Have Coins | Ativar/desativar colecionáveis | Afecta incentivo de exploração |
| Have Enemy Scaling | Escalar inimigos com dificuldade | Mantém desafio equilibrado |
| Platform Difficulty Relation | Densidade de plataformas vs dificuldade | "Mais em Dificuldade Elevada" ou inverso |
| Min/Max Platform Height | Intervalo de plataformas do chão | Alinhar com mecânica de salto |

---

## Guia de Utilização

### Geração Básica de Níveis

#### Passo 1: Configurar Modo de Geração

```
Selecione o Modo de Geração: Side Scroller ou Top Down
```

#### Passo 2: Ajustar Parâmetros

Modifique as **Basic Settings** baseado no seu modo escolhido (veja Secção 2.2 acima).

#### Passo 3: Personalizar Funcionalidades de Geração

Ative/desative as opções de geração desejadas em **Generation Settings**:

```csharp
// Exemplo de configuração para platformer desafiante
Have Enemies: true
Have Platforms: true
Platform Difficulty Relation: "More On Higher Difficulty"
Min Platform Height: 4
Max Platform Height: 6
Have Coins: true
Have Pickups: false
Have Hazards: true
Have Enemy Scaling: true
```

### Sistema de Dificuldade Adaptativa

#### Integrar Rastreio de Desempenho

1. No seu código de detecção de conclusão/falha de nível, chame:

```csharp
PlayerPerformanceTracker tracker = GetComponent<PlayerPerformanceTracker>();
tracker.UpdatePerformance(playerWon);
```

2. O sistema irá:
   - Rastrear vitórias/derrotas consecutivas
   - Ajustar automaticamente a dificuldade
   - Aplicar `AdjustDifficulty()` internamente

#### Personalizar Escala de Dificuldade

Modifique o campo `Difficulty Increment` no PlayerPerformanceTracker:
- **Valores maiores** (0.2+) = Escala mais agressiva
- **Valores menores** (0.05) = Progressão gradual de dificuldade

### Consultar Informações do Nível

#### Obter Contagem de Inimigos

```csharp
RLLevelTrainer trainer = GetComponent<RLLevelTrainer>();
int enemyCount = trainer.GetEnemyCount();
```

#### Obter Contagem de Colecionáveis

```csharp
int coinCount = trainer.GetCoinCount();
```

### Regenerar Níveis

#### Método 1: Recarregar Cena

```csharp
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
```

#### Método 2: Geração Flexível

```csharp
RLLevelTrainer trainer = GetComponent<RLLevelTrainer>();
trainer.FlexibleGeneration();
```

---

## Configurações

### Referência de Parâmetros

#### Definições Básicas

| Parâmetro | Intervalo | Padrão | Notas |
|-----------|-----------|--------|-------|
| Level Width | 20-500 | 150 | Maior = tempo de geração mais longo |
| Level Height | 20-200 | 50 | Deve acomodar plataformas |
| Training Episodes | 1000+ | 1000 | Mais episódios = melhor treino |
| Block Size | 0.5-2 | 1 | Deve corresponder ao tamanho do prefab |
| Difficulty | 1-10 | 1 | Multiplicador de dificuldade inicial |

#### Definições de Geração

| Parâmetro | Opções | Impacto |
|-----------|--------|--------|
| Generation Mode | Side Scroller, Top Down | Estilo de disposição do nível |
| Have Enemies | true/false | Adiciona desafio de combate |
| Have Platforms | true/false | Adiciona complexidade de navegação |
| Have Pickups | true/false | Fornece recursos de sobrevivência |
| Have Hazards | true/false | Adiciona ameaças ambientais |
| Have Coins | true/false | Adiciona incentivo colecionável |
| Have Enemy Scaling | true/false | Escala dificuldade com inimigos |
| Platform Difficulty Relation | Mais/Menos em Dificuldade Elevada | Mapeamento de dificuldade inversa |
| Min Platform Height | 1-10 | Colocação mínima de plataforma |
| Max Platform Height | Min+1 to 20 | Colocação máxima de plataforma |

#### Escala de Dificuldade de Inimigos

```csharp
// Em EnemyDifficultyAdjuster.cs
public float MaxHealth = 100f;      // Escala com dificuldade
public float MovementSpeed = 5f;    // Escala com dificuldade
```

---

## Referência da API

### RLLevelTrainer

#### Métodos Públicos

```csharp
// Gera um novo nível usando as definições actuais
public void FlexibleGeneration()

// Obtém o número de inimigos desovados no nível actual
public int GetEnemyCount()

// Obtém o número de moedas/colecionáveis no nível actual
public int GetCoinCount()
```

#### Propriedades Principais

```csharp
// Valor de dificuldade actual
public float Difficulty { get; set; }

// Modo de geração (Side Scroller / Top Down)
public GenerationMode GenerationMode { get; set; }

// Estado do treinador
public bool IsTraining { get; private set; }
```

### PlayerPerformanceTracker

#### Métodos Públicos

```csharp
// Actualizar desempenho com base no resultado do nível (true = vitória, false = derrota)
public void UpdatePerformance(bool playerWon)

// Ajustar manualmente a dificuldade
public void AdjustDifficulty()

// Obter vitórias consecutivas actuais
public int GetConsecutiveWins()

// Obter derrotas consecutivas actuais
public int GetConsecutiveLosses()
```

#### Propriedades Públicas

```csharp
// Mudança de dificuldade por vitória/derrota
public float DifficultyIncrement = 0.1f;

// Multiplicador de dificuldade actual
public float CurrentDifficulty { get; private set; }
```

### EnemyDifficultyAdjuster

#### Propriedades Públicas

```csharp
// Saúde máxima do inimigo (escala com dificuldade)
public float MaxHealth = 100f;

// Velocidade de movimento do inimigo (escala com dificuldade)
public float MovementSpeed = 5f;
```

#### Comportamento de Escala Automática

```
SaudeReal = MaxHealth * MultiplicadorDificuldade
VelocidadeReal = MovementSpeed * MultiplicadorDificuldade
```

---

## Boas Práticas

### Design de Níveis

✅ **FAÇA:**
- Comece com dificuldade conservadora (1.0-2.0)
- Teste com diversos níveis de habilidade do jogador
- Certifique-se de que as plataformas se alinham com mecânicas
- Forneça espaço suficiente entre perigos
- Equilibre a escala de dificuldade gradualmente

❌ **NÃO FAÇA:**
- Use dimensões extremamente grandes inicialmente (causa longos tempos de treino)
- Defina episódios de treino muito baixos (<1000)
- Misture modos de geração incompatíveis
- Coloque cadeias de plataformas impossíveis
- Ignore feedback do jogador sobre dificuldade

### Optimização de Desempenho

1. **Para geração mais rápida:**
   - Reduza Level Width/Height
   - Baixe Training Episodes (mínimo 1000)
   - Desactive funcionalidades não utilizadas (inimigos, perigos, etc.)

2. **Para melhor qualidade:**
   - Aumente Training Episodes (2000-5000)
   - Expanda Dimensões do Nível
   - Active todas as funcionalidades relevantes

3. **Gestão de Memória:**
   - Monitore a instanciação de cenas
   - Use object pooling para colecionáveis
   - Destrua níveis anteriores antes de gerar novos

### Testes

1. **Verificações de Sanidade:**
   - Verifique que todos os prefabs estão atribuídos
   - Confirme que os objectos pais existem
   - Teste com uma única funcionalidade ativada primeiro

2. **Progressão de Dificuldade:**
   - Jogue através de vários níveis
   - Verifique se a escala de dificuldade é sentida
   - Ajuste DifficultyIncrement se necessário

3. **Verificação de Conteúdo:**
   - Use `GetEnemyCount()` e `GetCoinCount()`
   - Verifique que o conteúdo esperado aparece
   - Procure erros de geração

---

## Resolução de Problemas

### Problemas Comuns

#### ❌ Os níveis demoram muito tempo a gerar

**Solução:**
- Reduza Level Width e Level Height
- Baixe Training Episodes para 1000
- Mude de Top Down para Side Scroller
- Desactive funcionalidades desnecessárias

#### ❌ Os níveis gerados são injogáveis

**Solução:**
- Ajuste Min/Max Platform Heights
- Verifique que mecânicas do jogador se alinham com design
- Reduza a definição de dificuldade
- Aumente Training Episodes para melhor aprendizagem

#### ❌ Colisores não funcionam corretamente

**Solução:**
- Verifique que Prefabs de Blocos têm Box Collider 2D
- Confirme que "Used By Composite" está ativado
- Verifique que Block Parent tem Composite Collider 2D
- Certifique-se que colisores não estão definidos como triggers

#### ❌ Jogador desova na posição errada

**Solução:**
- Certifique-se que Player Object está atribuído no inspetor
- Verifique que o prefab do jogador é válido
- Verifique que o colisor do jogador não sobrepõe a geometria do nível

#### ❌ Inimigos não desovam

**Solução:**
- Confirme que "Have Enemies" está ativado
- Verifique que os prefabs de inimigos estão atribuídos
- Verifique que Enemy Parent existe
- Adicione EnemyDifficultyAdjuster aos prefabs de inimigos

#### ❌ Dificuldade não se ajusta

**Solução:**
- Verifique que PlayerPerformanceTracker está atribuído
- Confirme que UpdatePerformance() está sendo chamado
- Verifique que DifficultyIncrement > 0
- Certifique-se que os níveis são longos o suficiente para rastrear desempenho

### Dicas de Depuração

```csharp
// Registar informações de geração
Debug.Log($"Inimigos: {trainer.GetEnemyCount()}, Moedas: {trainer.GetCoinCount()}");

// Monitorar progressão de dificuldade
Debug.Log($"Dificuldade Actual: {tracker.CurrentDifficulty}");
Debug.Log($"Vitórias: {tracker.GetConsecutiveWins()}, Derrotas: {tracker.GetConsecutiveLosses()}");

// Rastrear progresso de treino
Debug.Log($"A Treinar: {trainer.IsTraining}");
```

### Obter Apoio

Se os problemas persistirem:
1. Reveja a secção Guia de Configuração cuidadosamente
2. Verifique que todos os ficheiros necessários estão presentes
3. Procure mensagens de erro específicas na consola Unity
4. Certifique-se de compatibilidade de versão Unity (2020.3 LTS+)

---

## Histórico de Versões

| Versão | Data | Alterações |
|--------|------|-----------|
| 1.0 | 05/2026 | Lançamento inicial |

---

## Licença

Copyright [2026] [Afonso Costa]

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

---

**Criado por:** Afonso Costa


Para perguntas ou sugestões, por favor contacte o responsável pelo projecto.
