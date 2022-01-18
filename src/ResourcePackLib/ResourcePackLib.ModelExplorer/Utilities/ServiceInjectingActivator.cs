using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ResourcePackLib.ModelExplorer.Attributes;

namespace ResourcePackLib.ModelExplorer.Utilities;

public static class ServiceInjectingActivator
{
    private static ILogger Log = LogManager.GetCurrentClassLogger();

    public static T GetOrCreateInstance<T>([NotNull] this IServiceProvider serviceProvider) where T : class
    {
        try
        {
            var svc = serviceProvider.GetService<T>();
            if (svc != default)
                return svc;
        }
        catch
        {
        }

        return CreateInstance<T>(serviceProvider);
    }

    private static T InjectAttributedMembers<T>(IServiceProvider serviceProvider, T obj) where T : class
    {
        var type = typeof(T);
        var membersWithServiceAttribute = type
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<ServiceAttribute>() != null).ToArray();

        if (membersWithServiceAttribute.Length == 0) return obj;

        foreach (var memberInfo in membersWithServiceAttribute)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                try
                {
                    var svc = serviceProvider.GetService(propertyInfo.PropertyType);
                    propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(obj, svc, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, null, null);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Unable to inject service of type ({propertyInfo.PropertyType}) to property '{propertyInfo.Name}' on type '{propertyInfo.DeclaringType?.FullName}'");
                    throw;
                }
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                try
                {
                    var svc = serviceProvider.GetService(fieldInfo.FieldType);
                    fieldInfo.DeclaringType.GetField(fieldInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(obj, svc, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, null);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Unable to inject service of type ({fieldInfo.FieldType}) to field '{fieldInfo.Name}' on type '{fieldInfo.DeclaringType?.FullName}'");
                    throw;
                }
            }
            else
            {
                Log.Error($"Unable to inject service to member '{memberInfo.Name}' on type '{memberInfo.DeclaringType?.FullName}' because it is not a Property or a Field!");
            }
        }

        return obj;
    }

    public static T CreateInstance<T>([NotNull] this IServiceProvider serviceProvider) where T : class
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        var type = typeof(T);
        var constructors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).ToArray();

        if (constructors.Length == 0)
            return Activator.CreateInstance<T>();

        var exceptions = new List<Exception>();

        // Find a constructor we can populate (start with most args)
        foreach (var constructor in constructors)
        {
            T constructedObj = default;
            try
            {
                var parameters = constructor.GetParameters();
                var parameterValues = new object[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                {
                    parameterValues[i] = serviceProvider.GetService(parameters[i].ParameterType);
                }

                constructedObj = Activator.CreateInstance(type, parameterValues) as T;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                constructedObj = default;
            }

            if (constructedObj != default)
            {
                InjectAttributedMembers(serviceProvider, constructedObj);
                return constructedObj;
            }
        }

        throw new InvalidOperationException($"No suitable constructor for type {type} found.", new AggregateException(exceptions));
    }
}