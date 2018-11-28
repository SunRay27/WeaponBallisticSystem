using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdatable
{
    //Все объекты должны идентифицироваться как объекты обновления, для этого создан интерфейс, который наследуют все подобные компоненты
    //У них у всех должна быть функция OnUpdate
    void OnUpdate(float dt);
    void OnLateUpdate(float dt);
    void OnFixedUpdate(float dt);
}
public class CustomMonoBehaviour : MonoBehaviour, IUpdatable
{
    //Когда объект появляется
    private void Start()
    {
        //он регистрирует себя в менеджере
        GameUpdateSystem.Instance.Register(this);
        //вызывает функцию OnStart
        OnStart();
    }
    //Когда объект уничтожается
    private void OnDestroy()
    {
        //Он убирается из списка
        GameUpdateSystem.Instance.Delete(this);
    }
    //Виртуальная функция OnStart
    protected virtual void OnStart()
    {
        //Вызывается самим объектом
    }

    //Виртуальная функция OnUpdate
    public virtual void OnUpdate(float dt)
    {
        //Вызывается менеджером
    }
    //Виртуальная функция OnFixedUpdate
    public virtual void OnFixedUpdate(float dt)
    {
        //Вызывается менеджером
    }
    public virtual void OnLateUpdate(float dt)
    {
        //Вызывается менеджером
    }
}
public class GameUpdateSystem : MonoBehaviour
{
    //Менеджер - синглтон
    public static GameUpdateSystem Instance
    {
        get { return instance; }
    }
    private static GameUpdateSystem instance;
    //Список объектов для обновления
    List<CustomMonoBehaviour> itemsToUpdate = new List<CustomMonoBehaviour>();

    void Awake()
    {
        //Определить синглтон
        instance = this;
    }
    /// <summary>
    /// Зарегистрировать объект обновления
    /// </summary>
    /// <param name="mono">компонент</param>
    public void Register(CustomMonoBehaviour mono)
    {
        if (!itemsToUpdate.Contains(mono))
            itemsToUpdate.Add(mono);
    }
    /// <summary>
    /// Удалить компонент обновления
    /// </summary>
    /// <param name="mono">компонент</param>
    public void Delete(CustomMonoBehaviour mono)
    {
        if (itemsToUpdate.Contains(mono))
            itemsToUpdate.Remove(mono);
    }
    float dt;
    void Update()
    {
        //Сохранить deltaTime
        dt = Time.deltaTime;
        //Вызов функций обновления на элементах списка
        for (int i = 0; i < itemsToUpdate.Count; i++)
            itemsToUpdate[i].OnUpdate(dt);
    }
    private void FixedUpdate()
    {
        //Вызов функций фикс. обновления на элементах списка
        for (int i = 0; i < itemsToUpdate.Count; i++)
            itemsToUpdate[i].OnFixedUpdate(dt);
    }
    private void LateUpdate()
    {
        //Вызов функций фикс. обновления на элементах списка
        for (int i = 0; i < itemsToUpdate.Count; i++)
            itemsToUpdate[i].OnLateUpdate(dt);
    }

}
