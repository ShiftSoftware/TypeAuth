# TypeAuth Attribute

TypeAuth attribute can be used on controller methods or on the controller it self like aps.net core **Authorize** attribute to make sure the right user (authorized user) can have access to this method or controller.

TypeAuth attribute can be user mixed with asp.net core **Authorize** attribute, TypeAuth attribute take three parameter action tree, action and access, and it has only one purpose that provide to authorize with **TypeAuth Core**, thus asp.net core **Authorize** attribute can be used for other purposes, like to authorize a method just by valid jwt token (or user just logged in) without any role or claim or any action he has.

## Usage

Can be used on controller

```C#
[Route("api/[controller]")]
[ApiController]
[TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Read)]
public class SalesController : ControllerBase
{
    //Methods
}
```

Can be used with controller methods

```C#
[HttpPost]
[TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Write)]
public IActionResult Post([FromBody] int discount)
{
    //Code
}
```

Or the action-tree can be used as generic

```C#
[HttpPost]
[TypeAuth<CRMActions>(nameof(CRMActions.Sales), Access.Write)]
public IActionResult Post([FromBody] int discount)
{
    //Code
}
```

Can be used mixed with asp.net core **Authorize** attribute

```C#
[HttpPost]
[TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Write)]
public IActionResult Post([FromBody] int discount)
{
    //Code
}

[HttpGet]
[Authorize]
public IActionResult Get()
{
//Code
}
```

This indicated that the user must has sales write permission for post a sale, but for getting list of sales the user just needed to be logged in (has a valid jwt token).
