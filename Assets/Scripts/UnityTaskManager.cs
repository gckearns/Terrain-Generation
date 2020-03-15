using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class UnityTaskManager : MonoBehaviour
{

    public static UnityTaskManager instance;

    public int maxTasksPerUpdate;

    public Queue<UnityTask> scheduledTasks = new Queue<UnityTask>();
    public Queue<UnityTask> runningTasks = new Queue<UnityTask>();

    public static void ScheduleTask(Func<object> request, Action<object> callback)
    {
        instance.scheduledTasks.Enqueue(new UnityTask(new Task<object>(request), callback));
    }

    void Awake()
    {
        instance = FindObjectOfType<UnityTaskManager>();
        Debug.Log("UnityTaskManager Initialized");
    }

    void Update()
    {
        // Starting more than one (1) Task per update seems to slow down the UI
        // This ensures that only one(1) Task is started per frame (as long as maxTasksPerUpdate=1)
        int tasksEnqueuedThisUpdate = 0;
        for (int i = 0; i < scheduledTasks.Count; i++)
        {
            if (tasksEnqueuedThisUpdate < maxTasksPerUpdate)
            {
                UnityTask task = scheduledTasks.Dequeue();
                runningTasks.Enqueue(task);
                task.task.Start();
                tasksEnqueuedThisUpdate++;
            }
            else
            {
                break;
            }
        }

        // Executing callbacks on more than one (1) Task per update slows down the UI
        // This ensures that only one(1) Task callback is executed per frame
        for (int i = 0; i < runningTasks.Count; i++)
        {
            if (runningTasks.Peek().task.IsCompleted)
            {
                runningTasks.Dequeue().OnTaskCompleted();
                break;
            }
        }
    }

    public struct UnityTask
    {
        public Task<object> task;
        Action<object> callback;

        public UnityTask(Task<object> task, Action<object> callback)
        {
            this.task = task;
            this.callback = callback;
        }

        public void OnTaskCompleted()
        {
            callback(task.Result);
            // Disposing the Task helps recycle the resources in the ThreadPool
            task.Dispose();
        }
    }
}
