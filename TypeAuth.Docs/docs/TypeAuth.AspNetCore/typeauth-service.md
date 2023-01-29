# TypeAuth Service

After you register **TypeAuth AspNetCore** you can user `TypeAuthService` and inject it to any controllers or any other services, the service has a property `TypeAuthContext` that you can use it for check for permissions or actions.

## TypeAuth Service Injection

Can be injected into controller like this

```C#
public class CustomersController : ControllerBase
{
    private readonly TypeAuthService typeAuthService;

    public CustomersController(TypeAuthService typeAuthService)
    {
        this.typeAuthService = typeAuthService;
    }
}
```

Or inject it into service like this

```C#
public class CustomersService
{
    private readonly TypeAuthService typeAuthService;

    public CustomersService(TypeAuthService typeAuthService)
    {
        this.typeAuthService = typeAuthService;
    }
}
```

## Use TypeAuth Service

After injecting the service you can use the functions of `TypeAuthContext`

```C#
typeAuthService.TypeAuthContext.CanAccess(action);
```

Or

```C#
typeAuthService.TypeAuthContext.CanRead(action);
```

Or

```C#
typeAuthService.TypeAuthContext.CanWrite(action);
```

Or

```C#
typeAuthService.TypeAuthContext.CanDelete(action);
```

Or

```C#
typeAuthService.TypeAuthContext.AccessValue(action);
```

Or

```C#
typeAuthService.TypeAuthContext.Can(actionTreeType, actionName, access);
```
