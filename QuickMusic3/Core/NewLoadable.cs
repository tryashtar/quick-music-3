using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickMusic3;

public sealed class NewLoadable<TResult> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task<TResult>? WrappedTask { get; private set; }
    private readonly Func<Task<TResult>> CreateTask;
    private readonly TResult? Placeholder;
    private readonly Predicate<TResult> IsInvalid;
    private Action Callback = () => { };
    public NewLoadable(Func<TResult> create, TResult? placeholder = default, Predicate<TResult>? invalid_check = null)
    {
        CreateTask = () => Task.Run(create);
        Placeholder = placeholder;
        IsInvalid = invalid_check ?? ((x) => false);
    }
    public NewLoadable(Func<Task<TResult>> create, TResult? placeholder = default, Predicate<TResult>? invalid_check = null)
    {
        CreateTask = create;
        Placeholder = placeholder;
        IsInvalid = invalid_check ?? ((x) => false);
    }

    public TResult? Item
    {
        get
        {
            if (WrappedTask == null)
                _ = StartTaskAsync();
            if (WrappedTask.IsCompletedSuccessfully)
            {
                var result = WrappedTask.Result;
                if (IsInvalid(result))
                    _ = StartTaskAsync();
                else
                    return result;
            }
            return Placeholder;
        }
    }

    public void AddCallback(Action action)
    {
        Callback += action;
        if (WrappedTask != null && WrappedTask.IsCompleted)
            action();
    }

    public TaskAwaiter<TResult> GetAwaiter()
    {
        if (WrappedTask == null)
            _ = StartTaskAsync();
        if (WrappedTask.IsCompletedSuccessfully && IsInvalid(WrappedTask.Result))
            _ = StartTaskAsync();
        return WrappedTask.GetAwaiter();
    }

    public TaskStatus Status => WrappedTask?.Status ?? TaskStatus.Created;
    public bool IsSuccessfullyCompleted => WrappedTask?.IsCompletedSuccessfully ?? false;
    public bool IsFaulted => WrappedTask?.IsFaulted ?? false;
    public AggregateException? Exception => WrappedTask?.Exception;

    private async Task StartTaskAsync()
    {
        WrappedTask = CreateTask();
        try
        {
            await WrappedTask;
        }
        catch
        {
        }
        Callback();
        if (PropertyChanged == null)
            return;
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
        if (WrappedTask.IsFaulted)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
        }
        else
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Item)));
        }
    }
}
