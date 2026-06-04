# Quick Start Guide

[Versão em Português](QUICK_START.pt.md)

### 5-Minute Setup Checklist

#### Prerequisites
- [ ] All 7 scripts imported (`GeneratorAction.cs`, `GeneratorState.cs`, `RLGeneratorAgent.cs`, `RLLevelTrainer.cs`, `PlayerPerformanceTracker.cs`, `EnemyDifficultyAdjuster.cs`, `TileType.cs`)
- [ ] Player GameObject created in scene
- [ ] Prefabs prepared (blocks, enemies, collectibles)

#### Step-by-Step Setup (In Order)

1. **Create Trainer Object**
   ```
   Right-click in Hierarchy → 3D Object → Cube
   Delete the cube renderer
   Add Component → RLLevelTrainer
   ```

2. **Create Block Parent**
   ```
   Right-click in Hierarchy → Create Empty
   Add Component → Physics 2D → Composite Collider 2D
   Assign to RLLevelTrainer → Object Parents → Block Parent
   ```

3. **Create Parent Objects**
   ```
   Create 4 more empty GameObjects
   Assign them to:
   - Edge Parent
   - Enemy Parent
   - Coin Parent
   - Hazard Parent
   ```

4. **Assign Prefabs**
   ```
   In RLLevelTrainer Inspector → Prefabs section
   Assign your blocks, enemies, and collectibles
   **IMPORTANT: Block prefabs must have Box Collider 2D with "Used By Composite" ✓**
   ```

5. **Assign Player**
   ```
   Drag Player GameObject → RLLevelTrainer → Player Object
   ```

6. **Set Basic Settings**
   ```
   Block Size: 1
   Level Width: 150 (start small for testing)
   Level Height: 50
   Training Episodes: 1000
   ```

7. **Choose Generation Mode**
   ```
   RLLevelTrainer → Generation Settings → Generation Mode
   Select: Side Scroller OR Top Down
   ```

8. **Add Performance Tracker**
   ```
   Create empty GameObject
   Add Component → PlayerPerformanceTracker
   Assign to RLLevelTrainer → Player Performance Tracker
   ```

9. **Test**
   ```
   Play scene
   Observe level generation in console
   Verify level generates without errors
   ```

#### Enable Features (Optional)
```
Generation Settings:
☑ Have Enemies (if you want enemies)
☑ Have Platforms (if you want platforms)
☑ Have Coins (if you want collectibles)
☑ Have Pickups (if you want health items)
☑ Have Hazards (if you want obstacles)
```

### Common Mistakes to Avoid

❌ **Don't:** Skip assigning prefabs
✅ **Do:** Verify all prefabs are set before playing

❌ **Don't:** Use Box Colliders without "Used By Composite"
✅ **Do:** Check collider settings match requirements

❌ **Don't:** Set training episodes below 1000
✅ **Do:** Use at least 1000 episodes for quality levels

❌ **Don't:** Create huge levels initially
✅ **Do:** Start small (100-150 width) for faster testing

### Integration with Your Game

```csharp
// In your level completion script:
public class LevelManager : MonoBehaviour
{
    private PlayerPerformanceTracker tracker;
    
    void Start()
    {
        tracker = GetComponent<PlayerPerformanceTracker>();
    }
    
    public void OnLevelComplete(bool won)
    {
        // Update difficulty based on performance
        tracker.UpdatePerformance(won);
        
        // Generate next level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
```
