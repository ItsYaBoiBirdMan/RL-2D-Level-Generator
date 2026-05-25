using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static UnityEvent EnemyDeathEvent = new UnityEvent();
    public static UnityEvent PlayerFallingOffMapEvent = new UnityEvent();
    public static UnityEvent PlayerDeathEvent = new UnityEvent();
    public static UnityEvent CoinCollectedEvent = new UnityEvent();
}
