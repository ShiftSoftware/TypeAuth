### Incompatible Action Checks

The TypeAuthContext provides a few methods for <strong>checking</strong> an Action. They're listed below:
<ul>
    <li><strong>CanRead</strong></li>
    <li><strong>CanWrite</strong></li>
    <li><strong>CanDelete</strong></li>
    <li><strong>CanAccess</strong></li>
</li>

<br/>

<hr/>
There are times where a check is <strong>logically</strong> Incompatible to be used against an Action.<br/>
Here's an example:<br/><br/>

##### Action
``` C#
public static readonly Action Users = new Action("User Access", ActionType.ReadWriteDelete);
```

<br/>

##### Incompatible Check Example
``` C#
var incompatibleCheck1 = tAuth.CanAccess(Users);
var incompatibleCheck2 = tAuth.AccessValue(Users);
```
The Action Type of <strong>Users</strong> is <strong>ReadWriteDelete</strong>. 
It's not logical to <strong>Check</strong> it using <strong>CanAccess()</strong> and <strong>AccessValue()</strong>
<br/><br/>
There's no technical problem here though. The CanAccess() works exactly like CanRead() and AccessValue() will return null.<br/>
The problem is that it just doesn't make much sense when you think of it deeply.
<hr/>
<br/>

At some point there was a temptation that the Context should throw an exception for these Checks during runtime.<br/>
But having runtime exceptions is against the entire concept of having a Strongly Typed system. Thus the idea was abandoned.<br/><br/>

So we're not doing anything about these harmless incompatibilities.
Unless there're some magical ways to have the complier raise errors during compilation time.