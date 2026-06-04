# Guia de Início Rápido

[English Version](QUICK_START.md)

### Checklist de Configuração de 5 Minutos

#### Pré-requisitos
- [ ] Todos os 7 scripts importados (`GeneratorAction.cs`, `GeneratorState.cs`, `RLGeneratorAgent.cs`, `RLLevelTrainer.cs`, `PlayerPerformanceTracker.cs`, `EnemyDifficultyAdjuster.cs`, `TileType.cs`)
- [ ] GameObject do Jogador criado na cena
- [ ] Prefabs preparados (blocos, inimigos, colecionáveis)

#### Configuração Passo a Passo (Por Ordem)

1. **Criar Objecto de Treinador**
   ```
   Clique direito na Hierarquia → 3D Object → Cube
   Apague o cubo renderer
   Add Component → RLLevelTrainer
   ```

2. **Criar Pai de Blocos**
   ```
   Clique direito na Hierarquia → Create Empty
   Add Component → Physics 2D → Composite Collider 2D
   Atribua a RLLevelTrainer → Object Parents → Block Parent
   ```

3. **Criar Objectos Pais**
   ```
   Crie 4 GameObjects vazios adicionais
   Atribua-os a:
   - Edge Parent
   - Enemy Parent
   - Coin Parent
   - Hazard Parent
   ```

4. **Atribuir Prefabs**
   ```
   No Inspetor RLLevelTrainer → Secção Prefabs
   Atribua os seus blocos, inimigos e colecionáveis
   **IMPORTANTE: Prefabs de blocos devem ter Box Collider 2D com "Used By Composite" ✓**
   ```

5. **Atribuir Jogador**
   ```
   Arraste GameObject do Jogador → RLLevelTrainer → Player Object
   ```

6. **Definir Definições Básicas**
   ```
   Block Size: 1
   Level Width: 150 (comece pequeno para testes)
   Level Height: 50
   Training Episodes: 1000
   ```

7. **Escolher Modo de Geração**
   ```
   RLLevelTrainer → Generation Settings → Generation Mode
   Seleccione: Side Scroller OU Top Down
   ```

8. **Adicionar Rastreador de Desempenho**
   ```
   Crie GameObject vazio
   Add Component → PlayerPerformanceTracker
   Atribua a RLLevelTrainer → Player Performance Tracker
   ```

9. **Testar**
   ```
   Jogue a cena
   Observe geração de nível na consola
   Verifique que o nível é gerado sem erros
   ```

#### Activar Funcionalidades (Opcional)
```
Generation Settings:
☑ Have Enemies (se quiser inimigos)
☑ Have Platforms (se quiser plataformas)
☑ Have Coins (se quiser colecionáveis)
☑ Have Pickups (se quiser itens de saúde)
☑ Have Hazards (se quiser obstáculos)
```

### Erros Comuns a Evitar

❌ **Não faça:** Ignore atribuir prefabs
✅ **Faça:** Verifique que todos os prefabs estão definidos antes de jogar

❌ **Não faça:** Use Box Colliders sem "Used By Composite"
✅ **Faça:** Verifique que as definições do colisor correspondem aos requisitos

❌ **Não faça:** Defina episódios de treino abaixo de 1000
✅ **Faça:** Use pelo menos 1000 episódios para níveis de qualidade

❌ **Não faça:** Crie níveis enormes inicialmente
✅ **Faça:** Comece pequeno (100-150 de largura) para testes mais rápidos

### Integração com o Seu Jogo

```csharp
// No seu script de conclusão de nível:
public class GerenciadorNivel : MonoBehaviour
{
    private PlayerPerformanceTracker tracker;
    
    void Start()
    {
        tracker = GetComponent<PlayerPerformanceTracker>();
    }
    
    public void AoCompletarNivel(bool ganhou)
    {
        // Actualizar dificuldade baseado em desempenho
        tracker.UpdatePerformance(ganhou);
        
        // Gerar próximo nível
        GetComponent<RLLevelTrainer>().FlexibleGeneration();
    }
}
```

---

## Parameter Quick Reference / Referência Rápida de Parâmetros

### Recommended Configurations / Configurações Recomendadas

#### Side Scroller (Performance-Oriented)
```
Level Width: 100
Level Height: 50
Training Episodes: 1000
Generation Time: ~30-60 seconds
```

#### Side Scroller (Quality-Oriented)
```
Level Width: 150
Level Height: 75
Training Episodes: 2000
Generation Time: ~60-120 seconds
```

#### Top Down (Performance-Oriented)
```
Level Width: 25
Level Height: 25
Training Episodes: 1000
Generation Time: ~60-120 seconds
```

#### Top Down (Quality-Oriented)
```
Level Width: 35
Level Height: 35
Training Episodes: 3000
Generation Time: ~180-300 seconds
```

### Platform Height Reference / Referência de Altura de Plataformas

If player jump height is approximately:
- **3-4 units** → Min: 2, Max: 4
- **4-6 units** → Min: 3, Max: 6
- **6-8 units** → Min: 4, Max: 8

Se a altura de salto do jogador é aproximadamente:
- **3-4 unidades** → Mín: 2, Máx: 4
- **4-6 unidades** → Mín: 3, Máx: 6
- **6-8 unidades** → Mín: 4, Máx: 8

### Debug Commands / Comandos de Depuração

```csharp
// Check generation progress
RLLevelTrainer trainer = FindObjectOfType<RLLevelTrainer>();
Debug.Log($"Training: {trainer.IsTraining}");
Debug.Log($"Enemies: {trainer.GetEnemyCount()}");
Debug.Log($"Coins: {trainer.GetCoinCount()}");

// Check difficulty progression
PlayerPerformanceTracker tracker = FindObjectOfType<PlayerPerformanceTracker>();
Debug.Log($"Current Difficulty: {tracker.CurrentDifficulty}");
Debug.Log($"Consecutive Wins: {tracker.GetConsecutiveWins()}");
Debug.Log($"Consecutive Losses: {tracker.GetConsecutiveLosses()}");
```

---

## Troubleshooting Quick Links / Links Rápidos de Resolução de Problemas

| Problem | Solution |
|---------|----------|
| Prefabs not assigned | See Setup Guide Step 4 |
| Colliders not working | Check "Used By Composite" is enabled |
| Generation too slow | Reduce dimensions, lower episodes |
| Levels unplayable | Increase training episodes, adjust platform heights |
| Difficulty not changing | Verify PlayerPerformanceTracker is assigned |
| Player in wrong position | Ensure Player Object is assigned |

| Problema | Solução |
|----------|---------|
| Prefabs não atribuídos | Veja Guia de Configuração Passo 4 |
| Colisores não funcionam | Verifique "Used By Composite" ativado |
| Geração muito lenta | Reduza dimensões, reduza episódios |
| Níveis injogáveis | Aumente episódios de treino, ajuste alturas |
| Dificuldade não muda | Verifique PlayerPerformanceTracker atribuído |
| Jogador em posição errada | Certifique-se Player Object está atribuído |
