# ActiveCampaign .Net Client (C#)
The purpose of ActiveCampaign .Net Client is to interact with ActiveCampaign API V3 without having to know the in and outs of it.

## How to use

### Initialization

```csharp
using var ac = new ActiveCampaign.Client( "<API access url here>", "<API access key here>" );
```
You can find your API access url and key in your ActiveCampaign account: Settings (cog) -> Developer

### Adding a contact

```csharp
var contactData = await ac.AddContact( "example@example.com", <optional cancellation token> );
```
