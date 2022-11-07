# Getting Started

This guid you to how to use **TypeAuth.AspNetCore** library with asp.net core easily.

## How it works

This library helps you to bring **TypeAuth.Core** into asp.net core easily, and this is works on top of [asp.net core authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) for JWT.

## Installation

`Install-Package ShiftSoftware.TypeAuth.AspNetCore`

## Register TypeAuth.AspNetCore

### Register asp.net core authentication services

As we said this is works on top of asp.net core authentication for this purpose register some asp.net core authentication services is required.

First you must register **Authentication** and \***\*JwtBearer**

```C#
builder.Services.AddAuthentication(a =>
{
    a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "TypeAuth",
        ValidIssuer = "TypeAuth",
        RequireExpirationTime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("Your Secrete Key")),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
    };
});
```

`AddAuthentication` is needed to register asp.net core authentication services, and `AddJwtBearer` is register jwt bearer services, this middleware is used to verify jwt token and parse claims.

> `ValidAudience`, `ValidIssuer` and `IssuerSigningKey` values must be the same as the values that used for [generating jwt token](#generate-jwt-token).

You must add authentication middleware to the app

```C#
app.UseAuthentication();
app.UseAuthorization();
```

> The order of `UseAuthentication()` and `UseAuthorization()` is important, `UseAuthentication()` must come before `UseAuthorization()`

### Register TypeAuth.AspNetCore services

```C#
builder.Services.AddTypeAuth(o => o.AddActionTree<CRMActions>());
```

you can add more than one action trees, `AddActionTree` can be chained.

## Generate jwt token

There is one more step remains, When a user login or asks for token, the token must contain the access trees that in json format, it is not important where you stored the access trees, for example in database or file or any where else, but when you generate the jwt token then the token must contain the access trees in a claim type of `TypeAuth.AspNetCore.TypeAuthClaimTypes.AccessTree`.

```C#
var claims = new List<Claim>();

foreach (var accessTree in accessTrees)
{
    claims.Add(new Claim(TypeAuthClaimTypes.AccessTree, accessTree));
}

var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes("Your Secrete Key")
);

var token = new JwtSecurityToken(
    issuer: "TypeAuth",
    audience: "TypeAuth",
    claims: claims,
    expires: DateTime.Now.AddDays(30),
    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
);

string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

```

> `audience`, `issuer` and `SymmetricSecurityKey` values must be the same as the values that used for [register jwt bearer](#register-aspnet-core-authentication-services)
