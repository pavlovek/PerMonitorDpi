## PerMonitorDpi

Enable Windows 8.1+ Per-Monitor DPI support for Desktop WPF Apps. Instead of attempting to understand [this long MSDN article](http://msdn.microsoft.com/en-us/library/windows/desktop/ee308410(v=vs.85).aspx), take advantage of my personal suffering and use this instead:

```sh
Install-Package PerMonitorDpiBehavior
```

### How to Use

```xaml
<i:Interaction.Behaviors>
    <perMonitorDpi:PerMonitorDpiBehavior/>
</i:Interaction.Behaviors>
```

To observe the difference, attach a normal monitor to a Surface Pro 2 or other Retina-DPI monitor, then move your window between the two monitors. Per-Monitor DPI apps will stay sharp, normal apps will have blurred text on the Retina monitor.
