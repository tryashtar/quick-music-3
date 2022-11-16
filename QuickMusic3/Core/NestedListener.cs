using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QuickMusic3;

public sealed class NestedListener<T>
{
    public event EventHandler<T>? Changed;
    public event EventHandler<(T item, string propertyName)>? ItemChanged;
    private readonly object[] Chain;
    private readonly PropertyChangedEventHandler[] Events;
    private readonly string[] Properties;
    public NestedListener(object start, params string[] properties)
    {
        Properties = properties;
        Chain = new object[properties.Length + 1];
        Events = new PropertyChangedEventHandler[properties.Length + 1];
        Chain[0] = start;
        Resubscribe(0);
        for (int i = 0; i < Chain.Length; i++)
        {
            if (Chain[i] is INotifyPropertyChanged)
                break;
            if (Chain[i] == null)
                throw new ArgumentException("No path to a changing object");
        }
        if (Chain[^1] != null && Chain[^1] is not T)
            throw new ArgumentException($"{Chain[^1].GetType()} is not {typeof(T)}");
    }

    private void Resubscribe(int start)
    {
        for (int i = start; i < Chain.Length; i++)
        {
            if (Chain[i] == null)
                break;
            if (Chain[i] is INotifyPropertyChanged p1)
                p1.PropertyChanged -= Events[i];
            if (i < Chain.Length - 1)
                Chain[i + 1] = Chain[i].GetType().GetProperty(Properties[i], BindingFlags.Instance | BindingFlags.Public).GetValue(Chain[i]);
            if (Chain[i] is INotifyPropertyChanged p2)
            {
                int cap = i;
                Events[i] = (s, e) =>
                {
                    if (cap < Properties.Length && e.PropertyName == Properties[cap])
                    {
                        Resubscribe(cap + 1);
                        Changed?.Invoke(this, (T)Chain[^1]);
                    }
                    if (cap == Chain.Length - 1)
                        ItemChanged?.Invoke(s, ((T)Chain[^1], e.PropertyName));
                };
                p2.PropertyChanged += Events[i];
            }
        }
    }
}
